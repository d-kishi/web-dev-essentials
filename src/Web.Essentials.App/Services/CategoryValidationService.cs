using Web.Essentials.App.Interfaces;
using Web.Essentials.Domain.Repositories;

namespace Web.Essentials.App.Services;

/// <summary>
/// カテゴリバリデーション専用サービス
/// カテゴリの重複チェックと削除可能性の検証を担当
/// 単一責任の原則に基づき、バリデーション処理のみに集中
/// </summary>
public class CategoryValidationService : ICategoryValidationService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CategoryValidationService> _logger;

    /// <summary>
    /// コンストラクタ
    /// 依存性注入によりリポジトリとロガーを受け取る
    /// </summary>
    /// <param name="categoryRepository">カテゴリリポジトリ</param>
    /// <param name="productRepository">商品リポジトリ（関連チェック用）</param>
    /// <param name="logger">ロガー</param>
    public CategoryValidationService(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        ILogger<CategoryValidationService> logger)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> IsCategoryNameDuplicateAsync(string name, int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            _logger.LogInformation("カテゴリ名重複チェック実行。名前: {CategoryName}, 除外ID: {ExcludeId}", name, excludeId);

            var existingCategory = await _categoryRepository.GetByNameAsync(name.Trim());
            
            if (existingCategory == null)
            {
                return false;
            }

            // 除外IDが指定されている場合（編集時）は、そのIDと異なる場合のみ重複とみなす
            if (excludeId.HasValue)
            {
                return existingCategory.Id != excludeId.Value;
            }

            // 新規作成時は存在するだけで重複
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ名重複チェック中にエラーが発生しました。名前: {CategoryName}", name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CanDeleteCategoryAsync(int categoryId)
    {
        try
        {
            _logger.LogInformation("カテゴリ削除可能性チェック実行。カテゴリID: {CategoryId}", categoryId);

            // 子カテゴリの存在チェック
            var childCategories = await _categoryRepository.GetChildCategoriesAsync(categoryId);
            if (childCategories.Any())
            {
                _logger.LogInformation("カテゴリID {CategoryId} には子カテゴリが存在するため削除不可", categoryId);
                return false;
            }

            // 関連商品の存在チェック
            var relatedProducts = await _productRepository.GetByCategoryAsync(categoryId);
            if (relatedProducts.Any())
            {
                _logger.LogInformation("カテゴリID {CategoryId} には関連商品が存在するため削除不可", categoryId);
                return false;
            }

            _logger.LogInformation("カテゴリID {CategoryId} は削除可能", categoryId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ削除可能性チェック中にエラーが発生しました。カテゴリID: {CategoryId}", categoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(bool IsValid, List<string> ErrorMessages)> ValidateCategoryHierarchyAsync(int? parentCategoryId, int? currentCategoryId = null)
    {
        var errorMessages = new List<string>();

        try
        {
            // 親カテゴリが指定されていない場合（ルートカテゴリ）は有効
            if (!parentCategoryId.HasValue)
            {
                return (true, errorMessages);
            }

            // 親カテゴリの存在確認
            var parentCategory = await _categoryRepository.GetByIdAsync(parentCategoryId.Value);
            if (parentCategory == null)
            {
                errorMessages.Add("指定された親カテゴリが存在しません。");
                return (false, errorMessages);
            }

            // 自分自身を親に指定していないかチェック（編集時）
            if (currentCategoryId.HasValue && parentCategoryId.Value == currentCategoryId.Value)
            {
                errorMessages.Add("自分自身を親カテゴリに指定することはできません。");
                return (false, errorMessages);
            }

            // 3階層制限のチェック（親カテゴリのレベルが2の場合は子カテゴリは作成不可）
            if (parentCategory.Level >= 2)
            {
                errorMessages.Add("カテゴリの階層は3階層までです。これ以上深い階層は作成できません。");
                return (false, errorMessages);
            }

            // 循環参照のチェック（編集時）
            if (currentCategoryId.HasValue)
            {
                var isCircularReference = await CheckCircularReferenceAsync(currentCategoryId.Value, parentCategoryId.Value);
                if (isCircularReference)
                {
                    errorMessages.Add("指定された親カテゴリは循環参照を引き起こします。");
                    return (false, errorMessages);
                }
            }

            return (true, errorMessages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ階層バリデーション中にエラーが発生しました。親カテゴリID: {ParentCategoryId}, 現在カテゴリID: {CurrentCategoryId}", parentCategoryId, currentCategoryId);
            throw;
        }
    }

    /// <summary>
    /// 循環参照をチェックする
    /// カテゴリAの親にカテゴリBを設定した時、カテゴリBの祖先にカテゴリAが含まれていないかを確認。
    /// 提案された親カテゴリからルート方向に辿って、現在のカテゴリIDが見つかるかをチェック。
    /// 無限ループ防止のためチェック済みIDセットで重複アクセスを検出
    /// </summary>
    /// <param name="currentCategoryId">現在編集中のカテゴリID</param>
    /// <param name="proposedParentId">新しく設定しようとしている親カテゴリID</param>
    /// <returns>循環参照が発生して無限ループの原因となる場合はtrue、適切な階層構造の場合はfalse</returns>
    private async Task<bool> CheckCircularReferenceAsync(int currentCategoryId, int proposedParentId)
    {
        // 提案された親カテゴリから遡って、現在のカテゴリIDが見つかるかチェック
        var checkingCategoryId = proposedParentId;
        var checkedIds = new HashSet<int>();

        while (checkingCategoryId > 0)
        {
            // 無限ループ防止
            if (checkedIds.Contains(checkingCategoryId))
            {
                break;
            }
            checkedIds.Add(checkingCategoryId);

            // 現在のカテゴリIDが見つかった場合は循環参照
            if (checkingCategoryId == currentCategoryId)
            {
                return true;
            }

            // 親カテゴリを取得して継続
            var category = await _categoryRepository.GetByIdAsync(checkingCategoryId);
            if (category?.ParentCategoryId == null)
            {
                break;
            }

            checkingCategoryId = category.ParentCategoryId.Value;
        }

        return false;
    }
}