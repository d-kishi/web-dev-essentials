using Web.Essentials.App.DTOs;
using Web.Essentials.App.ViewModels;
using Web.Essentials.App.Interfaces;
using Web.Essentials.Domain.Entities;
using Web.Essentials.Domain.Repositories;

namespace Web.Essentials.App.Services;

/// <summary>
/// カテゴリ管理アプリケーションサービス
/// カテゴリに関するビジネスロジックと各レイヤーの調整を担当
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CategoryService> _logger;

    /// <summary>
    /// コンストラクタ
    /// 依存性注入によりリポジトリとロガーを受け取る
    /// </summary>
    /// <param name="categoryRepository">カテゴリリポジトリ</param>
    /// <param name="productRepository">商品リポジトリ（関連チェック用）</param>
    /// <param name="logger">ロガー</param>
    public CategoryService(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// カテゴリ一覧取得（検索・ページング対応）
    /// コントローラーから呼び出されるカテゴリ一覧取得の共通ロジック
    /// </summary>
    /// <param name="searchKeyword">検索キーワード</param>
    /// <param name="page">ページ番号</param>
    /// <param name="pageSize">1ページあたりの件数</param>
    /// <returns>カテゴリ一覧とページング情報</returns>
    public async Task<CategoryListDto> GetCategoryListAsync(
        string? searchKeyword = null,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("カテゴリ一覧取得サービス実行。検索キーワード: {SearchKeyword}", searchKeyword);

            // 入力値バリデーション
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            // 全カテゴリを取得
            var allCategories = await _categoryRepository.GetAllAsync();

            // 検索・フィルタリング処理
            var filteredCategories = FilterCategories(allCategories, searchKeyword);

            // 総件数を取得
            var totalCount = filteredCategories.Count();

            // ページング処理
            var pagedCategories = filteredCategories
                .OrderByDescending(c => c.UpdatedAt)
                .ThenBy(c => c.Name)  // 同じ更新日時の場合は名前順
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // DTOに変換（商品数も含む）
            var categoryDTOs = await ConvertToCategoryDtosAsync(pagedCategories);

            // ページング情報を作成
            var paging = CreatePagingInfo(page, pageSize, totalCount);

            return new CategoryListDto
            {
                Categories = categoryDTOs,
                Paging = paging,
                SearchKeyword = searchKeyword
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ一覧取得サービス実行中にエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// カテゴリ詳細取得
    /// 指定されたIDのカテゴリ詳細を取得
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>カテゴリ詳細情報、存在しない場合はnull</returns>
    public async Task<CategoryDetailsViewModel?> GetCategoryDetailsAsync(int id)
    {
        try
        {
            _logger.LogInformation("カテゴリ詳細取得サービス実行。カテゴリID: {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("カテゴリが見つかりませんでした。カテゴリID: {CategoryId}", id);
                return null;
            }

            // 関連商品数を取得（多対多関係経由）
            var (relatedProducts, _) = await _productRepository.GetAllAsync();
            var productCount = relatedProducts.Count(p => 
                p.ProductCategories.Any(pc => pc.CategoryId == id));

            // ViewModelに変換
            var viewModel = new CategoryDetailsViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ProductCount = productCount
            };

            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ詳細取得サービス実行中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            throw;
        }
    }

    /// <summary>
    /// カテゴリ編集用のViewModel準備
    /// 既存のカテゴリデータを含む編集用ViewModelを作成
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>カテゴリ編集用ViewModel、存在しない場合はnull</returns>
    public async Task<CategoryEditViewModel?> PrepareEditViewModelAsync(int id)
    {
        try
        {
            _logger.LogInformation("カテゴリ編集用ViewModel準備サービス実行。カテゴリID: {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("カテゴリが見つかりませんでした。カテゴリID: {CategoryId}", id);
                return null;
            }

            return new CategoryEditViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ編集用ViewModel準備サービス実行中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            throw;
        }
    }

    /// <summary>
    /// カテゴリ名重複チェック
    /// 指定されたカテゴリ名が既に使用されているかをチェック
    /// </summary>
    /// <param name="name">チェック対象のカテゴリ名</param>
    /// <param name="excludeCategoryId">除外するカテゴリID（編集時に自分自身を除外）</param>
    /// <returns>重複している場合はtrue</returns>
    public async Task<bool> IsCategoryNameDuplicateAsync(string name, int? excludeCategoryId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false; // 空の名前は重複チェック対象外
            }

            _logger.LogInformation("カテゴリ名重複チェックサービス実行。カテゴリ名: {CategoryName}, 除外カテゴリID: {ExcludeCategoryId}",
                name, excludeCategoryId);

            var allCategories = await _categoryRepository.GetAllAsync();
            
            var duplicateExists = allCategories.Any(c => 
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && 
                (!excludeCategoryId.HasValue || c.Id != excludeCategoryId.Value));

            if (duplicateExists)
            {
                _logger.LogWarning("カテゴリ名の重複が検出されました。カテゴリ名: {CategoryName}", name);
            }

            return duplicateExists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ名重複チェックサービス実行中にエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// カテゴリ削除可能性チェック
    /// 指定されたカテゴリが削除可能かをチェック（関連商品の有無を確認）
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>削除可能な場合はtrue、関連商品がある場合はfalse</returns>
    public async Task<bool> CanDeleteCategoryAsync(int id)
    {
        try
        {
            _logger.LogInformation("カテゴリ削除可能性チェックサービス実行。カテゴリID: {CategoryId}", id);

            var (relatedProducts, _) = await _productRepository.GetAllAsync();
            var hasRelatedProducts = relatedProducts.Any(p => 
                p.ProductCategories.Any(pc => pc.CategoryId == id));

            if (hasRelatedProducts)
            {
                _logger.LogWarning("カテゴリに関連商品が存在するため削除できません。カテゴリID: {CategoryId}", id);
            }

            return !hasRelatedProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ削除可能性チェックサービス実行中にエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// 全カテゴリ一覧取得（選択肢用）
    /// セレクトボックスなどで使用するための全カテゴリを取得
    /// </summary>
    /// <returns>全カテゴリのDTO一覧</returns>
    public async Task<List<CategoryDto>> GetAllCategoriesForSelectionAsync()
    {
        try
        {
            _logger.LogInformation("選択肢用全カテゴリ一覧取得サービス実行");

            var allCategories = await _categoryRepository.GetAllAsync();

            return allCategories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "選択肢用全カテゴリ一覧取得サービス実行中にエラーが発生しました");
            throw;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// カテゴリのフィルタリング処理
    /// 検索キーワードによるカテゴリフィルタリング
    /// </summary>
    /// <param name="categories">フィルタリング対象のカテゴリ一覧</param>
    /// <param name="searchKeyword">検索キーワード</param>
    /// <returns>フィルタリング後のカテゴリ一覧</returns>
    private static IQueryable<Category> FilterCategories(
        IEnumerable<Category> categories, 
        string? searchKeyword)
    {
        var query = categories.AsQueryable();

        // 検索キーワードによるフィルタリング
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(c => 
                c.Name.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(c.Description) && c.Description.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase)));
        }

        return query;
    }

    /// <summary>
    /// カテゴリエンティティをDTOに変換
    /// 関連商品数も含めた詳細なDTO変換
    /// </summary>
    /// <param name="categories">変換対象のカテゴリエンティティ一覧</param>
    /// <returns>変換後のDTO一覧</returns>
    private async Task<List<CategoryDto>> ConvertToCategoryDtosAsync(IList<Category> categories)
    {
        // 関連商品数を一括取得（N+1問題を避けるため）
        var (allProducts, _) = await _productRepository.GetAllAsync();
        var productCountDict = allProducts
            .SelectMany(p => p.ProductCategories.Select(pc => pc.CategoryId))
            .GroupBy(categoryId => categoryId)
            .ToDictionary(g => g.Key, g => g.Count());

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            ProductCount = productCountDict.TryGetValue(c.Id, out var count) ? count : 0
        }).ToList();
    }

    /// <summary>
    /// ページング情報の作成
    /// </summary>
    /// <param name="currentPage">現在のページ番号</param>
    /// <param name="pageSize">1ページあたりの件数</param>
    /// <param name="totalCount">総件数</param>
    /// <returns>ページング情報</returns>
    private static PagingDto CreatePagingInfo(int currentPage, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagingDto
        {
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = currentPage > 1,
            HasNextPage = currentPage < totalPages
        };
    }

    #endregion

    #region Additional Interface Methods

    /// <summary>
    /// カテゴリ存在確認
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>存在する場合true</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            _logger.LogInformation("カテゴリ存在確認サービス実行。カテゴリID: {CategoryId}", id);
            
            var category = await _categoryRepository.GetByIdAsync(id);
            return category != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ存在確認サービス実行中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            throw;
        }
    }

    /// <summary>
    /// カテゴリ削除
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>削除成功の場合true</returns>
    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            _logger.LogInformation("カテゴリ削除サービス実行。カテゴリID: {CategoryId}", id);

            // 削除可能性チェック
            var canDelete = await CanDeleteCategoryAsync(id);
            if (!canDelete)
            {
                _logger.LogWarning("カテゴリの削除ができません。関連商品が存在します。カテゴリID: {CategoryId}", id);
                return false;
            }

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("削除対象のカテゴリが見つかりませんでした。カテゴリID: {CategoryId}", id);
                return false;
            }

            await _categoryRepository.DeleteAsync(category.Id);
            _logger.LogInformation("カテゴリを正常に削除しました。カテゴリID: {CategoryId}", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ削除サービス実行中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            throw;
        }
    }

    /// <summary>
    /// インターフェース用のメソッド名エイリアス
    /// </summary>
    public async Task<bool> CanDeleteAsync(int id) => await CanDeleteCategoryAsync(id);

    /// <summary>
    /// カテゴリ一覧取得（インターフェース用）
    /// </summary>
    public async Task<CategoryListDto> GetCategoriesAsync(CategorySearchRequestDto searchRequest)
    {
        return await GetCategoryListAsync(
            searchRequest.NameTerm, 
            1, // デフォルトページ
            10); // デフォルトページサイズ
    }

    /// <summary>
    /// 選択用カテゴリ一覧取得（インターフェース用）
    /// </summary>
    public async Task<IEnumerable<CategorySelectDto>> GetCategoriesForSelectAsync()
    {
        var categories = await GetAllCategoriesForSelectionAsync();
        return categories.Select(c => new CategorySelectDto
        {
            Id = c.Id,
            Name = c.Name,
            FullPath = c.Name,
            Level = 0
        });
    }

    /// <summary>
    /// カテゴリ編集用データ取得（インターフェース用）
    /// </summary>
    public async Task<CategoryEditViewModel?> GetCategoryForEditAsync(int id)
    {
        return await PrepareEditViewModelAsync(id);
    }

    /// <summary>
    /// カテゴリ作成用データ取得
    /// </summary>
    public async Task<CategoryCreateViewModel> GetCategoryForCreateAsync()
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        var parentCategories = allCategories.Select(c => new CategorySelectItem
        {
            Id = c.Id,
            Name = c.Name,
            FullPath = c.Name,
            Level = 0
        });

        return new CategoryCreateViewModel
        {
            ParentCategories = parentCategories
        };
    }

    /// <summary>
    /// カテゴリ作成
    /// </summary>
    public async Task<int> CreateCategoryAsync(CategoryCreateViewModel createModel)
    {
        try
        {
            var category = new Category
            {
                Name = createModel.Name,
                Description = createModel.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            _logger.LogInformation("カテゴリを正常に作成しました。カテゴリID: {CategoryId}", category.Id);
            
            return category.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ作成サービス実行中にエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// カテゴリ更新
    /// </summary>
    public async Task<bool> UpdateCategoryAsync(CategoryEditViewModel editModel)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(editModel.Id);
            if (category == null)
            {
                _logger.LogWarning("更新対象のカテゴリが見つかりませんでした。カテゴリID: {CategoryId}", editModel.Id);
                return false;
            }

            category.Name = editModel.Name;
            category.Description = editModel.Description;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);
            _logger.LogInformation("カテゴリを正常に更新しました。カテゴリID: {CategoryId}", category.Id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ更新サービス実行中にエラーが発生しました。カテゴリID: {CategoryId}", editModel.Id);
            throw;
        }
    }

    // 以下は基本的なスタブ実装（将来的に完全実装予定）

    public async Task<CategoryIndexViewModel> GetCategoryIndexAsync(CategoryIndexViewModel viewModel)
    {
        // 基本実装
        return await Task.FromResult(viewModel);
    }

    public async Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync(int? rootCategoryId = null)
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => new CategoryTreeDto
        {
            Id = c.Id,
            Name = c.Name,
            Level = 0,
            SortOrder = 1,
            ProductCount = 0
        });
    }

    public async Task<IEnumerable<CategoryListItemViewModel>> GetChildCategoriesAsync(int parentId)
    {
        // 階層構造が実装されていないため、空のリストを返す
        return await Task.FromResult(new List<CategoryListItemViewModel>());
    }

    public async Task<IEnumerable<CategoryBreadcrumbDto>> GetAncestorsAsync(int categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null) return new List<CategoryBreadcrumbDto>();

        return new List<CategoryBreadcrumbDto>
        {
            new CategoryBreadcrumbDto
            {
                Id = category.Id,
                Name = category.Name,
                Level = 0,
                IsLast = true
            }
        };
    }

    public async Task<bool> CanMoveToParentAsync(int categoryId, int? newParentId)
    {
        // 基本実装：階層構造が実装されていないため、常にtrue
        return await Task.FromResult(true);
    }

    public async Task<IEnumerable<CategoryListItemViewModel>> GetCategoriesByLevelAsync(int level)
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => new CategoryListItemViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Level = 0,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });
    }

    public async Task<bool> ReorderCategoriesAsync(int? parentId, IEnumerable<int> categoryIds)
    {
        // 基本実装：順序管理が実装されていないため、常にtrue
        return await Task.FromResult(true);
    }

    #endregion
}