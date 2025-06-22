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

    /// <inheritdoc />
    public async Task<List<CategoryDto>> GetCategoriesAsync(CategorySearchRequestDto searchRequest)
    {
        try
        {
            _logger.LogInformation("カテゴリ一覧取得サービス実行。検索キーワード: {SearchKeyword}", searchRequest.NameTerm);

            // 全カテゴリを取得
            var allCategories = await _categoryRepository.GetAllAsync();

            // 検索・フィルタリング処理
            var filteredCategories = await FilterCategoriesAsync(allCategories, searchRequest);

            // DTOに変換（商品数も含む）
            var categoryDTOs = await ConvertToCategoryDtosAsync(filteredCategories);

            return categoryDTOs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ一覧取得サービス実行中にエラーが発生しました");
            throw;
        }
    }


    /// <inheritdoc />
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

            // 親カテゴリ選択用リストを取得（自分自身は除外）
            var allCategories = await _categoryRepository.GetAllAsync();
            var parentCategories = await ConvertToCategorySelectItemsAsync(
                allCategories.Where(c => c.Id != id));  // 自分自身を除外

            return new CategoryEditViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                CurrentLevel = category.Level,
                SortOrder = category.SortOrder,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ParentCategories = parentCategories
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ編集用ViewModel準備サービス実行中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            throw;
        }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
    /// 検索条件に基づいてカテゴリをフィルタリングし、名前や説明による部分一致検索、
    /// レベル指定フィルタリング、親カテゴリID指定フィルタリング、削除可能フィルタリングを実行
    /// </summary>
    /// <param name="categories">フィルタリング対象のカテゴリ一覧</param>
    /// <param name="searchRequest">検索条件。NameTerm（名前検索）、Level（レベル指定）、ParentId（親カテゴリID）、DeletableOnly（削除可能）を含む</param>
    /// <returns>検索条件に一致するフィルタリング後のカテゴリ一覧</returns>
    private async Task<IList<Category>> FilterCategoriesAsync(
        IEnumerable<Category> categories, 
        CategorySearchRequestDto searchRequest)
    {
        var query = categories.AsQueryable();

        // 検索キーワードによるフィルタリング
        if (!string.IsNullOrWhiteSpace(searchRequest.NameTerm))
        {
            query = query.Where(c => 
                c.Name.Contains(searchRequest.NameTerm, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(c.Description) && c.Description.Contains(searchRequest.NameTerm, StringComparison.OrdinalIgnoreCase)));
        }

        // レベル指定フィルタリング
        if (searchRequest.Level.HasValue)
        {
            query = query.Where(c => c.Level == searchRequest.Level.Value);
        }

        // 親カテゴリID指定フィルタリング
        if (searchRequest.ParentId.HasValue)
        {
            query = query.Where(c => c.ParentCategoryId == searchRequest.ParentId.Value);
        }

        var filteredCategories = query.ToList();

        // 削除可能フィルタリング（商品数=0かつ子カテゴリなし）
        if (searchRequest.DeletableOnly == true)
        {
            var deletableCategories = new List<Category>();
            
            // 商品数を一括取得
            var (allProducts, _) = await _productRepository.GetAllAsync();
            var productCountDict = allProducts
                .SelectMany(p => p.ProductCategories.Select(pc => pc.CategoryId))
                .GroupBy(categoryId => categoryId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var category in filteredCategories)
            {
                // 商品数チェック
                var productCount = productCountDict.TryGetValue(category.Id, out var count) ? count : 0;
                
                // 子カテゴリ存在チェック
                var hasChildren = categories.Any(c => c.ParentCategoryId == category.Id);
                
                // 削除可能な場合のみ追加
                if (productCount == 0 && !hasChildren)
                {
                    deletableCategories.Add(category);
                }
            }
            
            return deletableCategories;
        }

        return filteredCategories;
    }

    /// <summary>
    /// カテゴリエンティティをDTOに変換
    /// 関連商品数も含めた詳細なDTO変換処理を実行し、N+1問題を避けるため
    /// 全商品データを一括取得してカテゴリごとの商品数を効率的に計算
    /// </summary>
    /// <param name="categories">変換対象のカテゴリエンティティ一覧</param>
    /// <returns>カテゴリID、名前、説明、作成日時、更新日時、関連商品数、削除可能判定を含むDTO一覧</returns>
    private async Task<List<CategoryDto>> ConvertToCategoryDtosAsync(IList<Category> categories)
    {
        // 関連商品数を一括取得（N+1問題を避けるため）
        var (allProducts, _) = await _productRepository.GetAllAsync();
        var productCountDict = allProducts
            .SelectMany(p => p.ProductCategories.Select(pc => pc.CategoryId))
            .GroupBy(categoryId => categoryId)
            .ToDictionary(g => g.Key, g => g.Count());

        // 全カテゴリを取得して子カテゴリ存在チェック用に準備
        var allCategories = await _categoryRepository.GetAllAsync();

        return categories.Select(c => {
            var productCount = productCountDict.TryGetValue(c.Id, out var count) ? count : 0;
            var hasChildren = allCategories.Any(child => child.ParentCategoryId == c.Id);
            
            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                Level = c.Level,
                SortOrder = c.SortOrder,
                FullPath = c.Name, // 簡易実装
                ProductCount = productCount,
                HasChildren = hasChildren,
                CanDelete = productCount == 0 && !hasChildren,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }).ToList();
    }

    /// <summary>
    /// ページング情報の作成
    /// 総件数とページサイズから総ページ数を計算し、前後ページの存在状況を判定
    /// </summary>
    /// <param name="currentPage">現在のページ番号（1から開始）</param>
    /// <param name="pageSize">1ページあたりの表示件数</param>
    /// <param name="totalCount">検索結果の総件数</param>
    /// <returns>現在ページ、ページサイズ、総件数、総ページ数、前後ページ存在フラグを含むページング情報</returns>
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<bool> CanDeleteAsync(int id) => await CanDeleteCategoryAsync(id);


    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<CategoryEditViewModel?> GetCategoryForEditAsync(int id)
    {
        return await PrepareEditViewModelAsync(id);
    }

    /// <inheritdoc />
    public async Task<CategoryCreateViewModel> GetCategoryForCreateAsync()
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        var parentCategories = await ConvertToCategorySelectItemsAsync(allCategories);

        return new CategoryCreateViewModel
        {
            ParentCategories = parentCategories
        };
    }

    /// <summary>
    /// カテゴリエンティティのコレクションを階層構造を持つCategorySelectItemsに変換
    /// 親子関係を解析してツリー構造を構築し、各カテゴリの完全パス（親 > 子）形式で表示名を生成
    /// ルートカテゴリから開始して再帰的に子カテゴリを処理
    /// </summary>
    /// <param name="categories">変換対象のカテゴリエンティティのコレクション</param>
    /// <returns>階層構造と完全パスを持つCategorySelectItemsのコレクション</returns>
    private async Task<IEnumerable<CategorySelectItem>> ConvertToCategorySelectItemsAsync(IEnumerable<Category> categories)
    {
        var result = new List<CategorySelectItem>();
        var categoryDict = categories.ToDictionary(c => c.Id);

        foreach (var category in categories)
        {
            var fullPath = BuildCategoryFullPath(category, categoryDict);
            
            result.Add(new CategorySelectItem
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                FullPath = fullPath,
                Level = category.Level,
                ProductCount = 0, // 簡略化
                ChildCategories = new List<CategorySelectItem>() // フラットリストなので空
            });
        }

        // FullPathの昇順でソート（「ルートカテゴリとして作成」が先頭に来るように）
        return result.OrderBy(c => c.FullPath);
    }

    /// <summary>
    /// カテゴリエンティティをCategorySelectItemに変換（再帰的に子カテゴリも含む）
    /// 完全パスの構築、子カテゴリの再帰的処理、商品数の設定を実行
    /// </summary>
    /// <param name="category">変換対象のカテゴリエンティティ</param>
    /// <param name="categoryDict">高速検索用のカテゴリIDでインデックス化されたカテゴリ辞書</param>
    /// <param name="productCounts">パフォーマンス最適化済みのカテゴリごとの商品数マップ</param>
    /// <returns>階層構造情報、完全パス、子カテゴリ一覧を含む変換されたCategorySelectItem</returns>
    private CategorySelectItem ConvertToCategorySelectItem(
        Category category, 
        Dictionary<int, Category> categoryDict,
        Dictionary<int, int> productCounts)
    {
        var fullPath = BuildCategoryFullPath(category, categoryDict);
        var childCategories = categoryDict.Values
            .Where(c => c.ParentCategoryId == category.Id)
            .Select(child => ConvertToCategorySelectItem(child, categoryDict, productCounts))
            .ToList();

        return new CategorySelectItem
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            FullPath = fullPath,
            Level = category.Level,
            ProductCount = productCounts.GetValueOrDefault(category.Id, 0),
            ChildCategories = childCategories
        };
    }

    /// <summary>
    /// カテゴリの完全パスを構築
    /// 指定カテゴリから親方向に辿って階層パスを"親 > 子 > 孫"形式で生成
    /// </summary>
    /// <param name="category">完全パス生成対象のカテゴリエンティティ</param>
    /// <param name="categoryDict">親カテゴリ検索用のカテゴリID辞書</param>
    /// <returns>"親カテゴリ > 子カテゴリ > 対象カテゴリ"形式の完全パス文字列</returns>
    private string BuildCategoryFullPath(Category category, Dictionary<int, Category> categoryDict)
    {
        var pathParts = new List<string>();
        var current = category;

        while (current != null)
        {
            pathParts.Insert(0, current.Name);
            current = current.ParentCategoryId.HasValue && categoryDict.ContainsKey(current.ParentCategoryId.Value)
                ? categoryDict[current.ParentCategoryId.Value]
                : null;
        }

        return string.Join(" > ", pathParts);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategorySelectItem>> GetCategorySelectItemsAsync()
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        return await ConvertToCategorySelectItemsAsync(allCategories);
    }

    /// <inheritdoc />
    public async Task<int> CreateCategoryAsync(CategoryCreateViewModel createModel)
    {
        try
        {
            // 親カテゴリが設定されている場合、レベルを計算
            var level = 0;
            if (createModel.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(createModel.ParentCategoryId.Value);
                if (parentCategory != null)
                {
                    level = parentCategory.Level + 1;
                }
            }

            var category = new Category
            {
                Name = createModel.Name,
                Description = createModel.Description,
                ParentCategoryId = createModel.ParentCategoryId,
                Level = level,
                SortOrder = createModel.SortOrder,
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

    /// <inheritdoc />
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

            // 親カテゴリが変更された場合、レベルを再計算
            var level = 0;
            if (editModel.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(editModel.ParentCategoryId.Value);
                if (parentCategory != null)
                {
                    level = parentCategory.Level + 1;
                }
            }

            category.Name = editModel.Name;
            category.Description = editModel.Description;
            category.ParentCategoryId = editModel.ParentCategoryId;
            category.Level = level;
            category.SortOrder = editModel.SortOrder;
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

    /// <inheritdoc />
    public async Task<CategoryIndexViewModel> GetCategoryIndexAsync(CategoryIndexViewModel viewModel)
    {
        // 基本実装
        return await Task.FromResult(viewModel);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryListItemViewModel>> GetChildCategoriesAsync(int parentId)
    {
        // 階層構造が実装されていないため、空のリストを返す
        return await Task.FromResult(new List<CategoryListItemViewModel>());
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<bool> CanMoveToParentAsync(int categoryId, int? newParentId)
    {
        // 基本実装：階層構造が実装されていないため、常にtrue
        return await Task.FromResult(true);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<bool> ReorderCategoriesAsync(int? parentId, IEnumerable<int> categoryIds)
    {
        // 基本実装：順序管理が実装されていないため、常にtrue
        return await Task.FromResult(true);
    }

    #endregion
}