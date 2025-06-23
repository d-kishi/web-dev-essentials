using Web.Essentials.App.DTOs;
using Web.Essentials.App.ViewModels;
using Web.Essentials.App.Interfaces;
using Web.Essentials.Domain.Entities;
using Web.Essentials.Domain.Repositories;

namespace Web.Essentials.App.Services;

/// <summary>
/// 商品管理アプリケーションサービス
/// 商品に関するビジネスロジックと各レイヤーの調整を担当
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductImageRepository _productImageRepository;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// コンストラクタ
    /// 依存性注入によりリポジトリとロガーを受け取る
    /// </summary>
    /// <param name="productRepository">商品リポジトリ</param>
    /// <param name="categoryRepository">カテゴリリポジトリ</param>
    /// <param name="productImageRepository">商品画像リポジトリ</param>
    /// <param name="logger">ロガー</param>
    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IProductImageRepository productImageRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _productImageRepository = productImageRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ProductListDto> GetProductListAsync(
        string? searchKeyword = null,
        int? categoryId = null,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("商品一覧取得サービス実行。検索キーワード: {SearchKeyword}, カテゴリID: {CategoryId}",
                searchKeyword, categoryId);

            // 入力値バリデーション
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            // 全商品を取得
            var (allProducts, _) = await _productRepository.GetAllAsync();

            // 検索・フィルタリング処理
            var filteredProducts = FilterProducts(allProducts, searchKeyword, categoryId);

            // 総件数を取得
            var totalCount = filteredProducts.Count();

            // ページング処理
            var pagedProducts = filteredProducts
                .OrderByDescending(p => p.UpdatedAt)
                .ThenBy(p => p.Name)  // 同じ更新日時の場合は名前順
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // DTOに変換
            var productDTOs = await ConvertToProductDtosAsync(pagedProducts);

            // ページング情報を作成
            var paging = CreatePagingInfo(page, pageSize, totalCount);

            return new ProductListDto
            {
                Products = productDTOs,
                Paging = paging,
                SearchKeyword = searchKeyword
                // CategoryIdは多対多関係のため、Productエンティティには直接的なCategoryIdは存在しない
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品一覧取得サービス実行中にエラーが発生しました");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductDetailsViewModel?> GetProductDetailsAsync(int id)
    {
        try
        {
            _logger.LogInformation("商品詳細取得サービス実行。商品ID: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("商品が見つかりませんでした。商品ID: {ProductId}", id);
                return null;
            }

            // 商品画像を取得
            var productImages = await _productImageRepository.GetByProductIdAsync(id);
            
            // カテゴリ名を取得（多対多関係のため最初のカテゴリ名を使用）
            var firstCategory = product.ProductCategories.FirstOrDefault()?.Category;
            var categoryName = firstCategory?.Name ?? "未分類";

            // ViewModelに変換
            var viewModel = new ProductDetailsViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = (uint)product.Price,
                Status = product.Status,
                CategoryName = categoryName,
                Categories = product.ProductCategories.Select(pc => new CategoryDisplayItem
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name,
                    FullPath = pc.Category.Name
                }).ToList(),
                JanCode = product.JanCode,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Images = productImages.Select(img => new ProductImageDisplayItem
                {
                    Id = img.Id,
                    ImagePath = img.ImagePath,
                    DisplayOrder = img.DisplayOrder,
                    AltText = img.AltText,
                    IsMain = img.IsMain
                }).OrderBy(img => img.DisplayOrder).ToList()
            };

            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品詳細取得サービス実行中にエラーが発生しました。商品ID: {ProductId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductCreateViewModel> PrepareCreateViewModelAsync()
    {
        try
        {
            _logger.LogInformation("商品登録用ViewModel準備サービス実行");

            var categories = await _categoryRepository.GetAllAsync();
            var categorySelectItems = ConvertToCategorySelectItems(categories);

            return new ProductCreateViewModel
            {
                Categories = categorySelectItems.OrderBy(c => c.FullPath).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品登録用ViewModel準備サービス実行中にエラーが発生しました");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductEditViewModel?> PrepareEditViewModelAsync(int id)
    {
        try
        {
            _logger.LogInformation("商品編集用ViewModel準備サービス実行。商品ID: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id, includeRelations: true);
            if (product == null)
            {
                _logger.LogWarning("商品が見つかりませんでした。商品ID: {ProductId}", id);
                return null;
            }

            // カテゴリ一覧と商品画像を取得
            var categories = await _categoryRepository.GetAllAsync();
            var productImages = await _productImageRepository.GetByProductIdAsync(id);
            var categorySelectItems = ConvertToCategorySelectItems(categories);

            // ProductCategoriesが正しくロードされているかログ出力
            var selectedCategoryIds = product.ProductCategories?.Select(pc => pc.CategoryId).ToList() ?? new List<int>();
            _logger.LogInformation("商品編集ViewModel準備: 商品ID {ProductId}, 関連カテゴリ数: {CategoryCount}, カテゴリIDs: {CategoryIds}", 
                id, selectedCategoryIds.Count, string.Join(",", selectedCategoryIds));

            return new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = (uint)product.Price,
                Status = product.Status,
                JanCode = product.JanCode,
                SelectedCategoryIds = selectedCategoryIds,
                Categories = categorySelectItems.OrderBy(c => c.FullPath).ToList(),
                ExistingImages = productImages.Select(img => new ProductImageViewModel
                {
                    Id = img.Id,
                    ProductId = img.ProductId,
                    ImagePath = img.ImagePath,
                    DisplayOrder = img.DisplayOrder,
                    AltText = img.AltText,
                    IsMain = img.IsMain,
                    CreatedAt = img.CreatedAt
                }).OrderBy(img => img.DisplayOrder).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品編集用ViewModel準備サービス実行中にエラーが発生しました。商品ID: {ProductId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsJanCodeDuplicateAsync(string janCode, int? excludeProductId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(janCode))
            {
                return false; // 空のJANコードは重複チェック対象外
            }

            _logger.LogInformation("JANコード重複チェックサービス実行。JANコード: {JanCode}, 除外商品ID: {ExcludeProductId}",
                janCode, excludeProductId);

            var (allProducts, _) = await _productRepository.GetAllAsync();
            
            var duplicateExists = allProducts.Any(p => 
                !string.IsNullOrEmpty(p.JanCode) && 
                p.JanCode.Equals(janCode, StringComparison.OrdinalIgnoreCase) && 
                (!excludeProductId.HasValue || p.Id != excludeProductId.Value));

            if (duplicateExists)
            {
                _logger.LogWarning("JANコードの重複が検出されました。JANコード: {JanCode}", janCode);
            }

            return duplicateExists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JANコード重複チェックサービス実行中にエラーが発生しました");
            throw;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// 商品のフィルタリング処理
    /// 検索キーワードとカテゴリIDによる商品フィルタリング
    /// </summary>
    /// <param name="products">フィルタリング対象の商品一覧</param>
    /// <param name="searchKeyword">検索キーワード</param>
    /// <param name="categoryId">カテゴリID</param>
    /// <returns>フィルタリング後の商品一覧</returns>
    private static IQueryable<Product> FilterProducts(
        IEnumerable<Product> products, 
        string? searchKeyword, 
        int? categoryId)
    {
        var query = products.AsQueryable();

        // 検索キーワードによるフィルタリング
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(p => 
                p.Name.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(p.JanCode) && p.JanCode.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase)));
        }

        // カテゴリIDによるフィルタリング（多対多関係対応）
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId.Value));
        }

        return query;
    }

    /// <summary>
    /// 商品エンティティをDTOに変換
    /// カテゴリ名も含めた詳細なDTO変換
    /// </summary>
    /// <param name="products">変換対象の商品エンティティ一覧</param>
    /// <returns>変換後のDTO一覧</returns>
    private async Task<List<ProductDto>> ConvertToProductDtosAsync(IList<Product> products)
    {
        // カテゴリ情報を一括取得（N+1問題を避けるため）
        var categories = await _categoryRepository.GetAllAsync();
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            JanCode = p.JanCode,
            Status = (int)p.Status,
            StatusName = p.Status.ToString(),
            Categories = p.ProductCategories.Select(pc => new ProductCategoryDto
            {
                Id = pc.Category.Id,
                Name = pc.Category.Name,
                FullPath = pc.Category.Name,
                Level = 1
            }).ToList(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
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

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            _logger.LogInformation("商品存在確認サービス実行。商品ID: {ProductId}", id);
            
            var product = await _productRepository.GetByIdAsync(id);
            return product != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品存在確認サービス実行中にエラーが発生しました。商品ID: {ProductId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> CreateProductAsync(ProductCreateViewModel createModel)
    {
        try
        {
            _logger.LogInformation("商品作成サービス実行。商品名: {ProductName}", createModel.Name);

            var product = new Product
            {
                Name = createModel.Name,
                Description = createModel.Description,
                Price = createModel.Price,
                JanCode = createModel.JanCode,
                Status = createModel.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);

            // カテゴリ関係の設定
            if (createModel.SelectedCategoryIds?.Any() == true)
            {
                foreach (var categoryId in createModel.SelectedCategoryIds)
                {
                    var productCategory = new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId
                    };
                    product.ProductCategories.Add(productCategory);
                }
                await _productRepository.UpdateAsync(product);
            }

            _logger.LogInformation("商品を正常に作成しました。商品ID: {ProductId}", product.Id);
            return product.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品作成サービス実行中にエラーが発生しました");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateProductAsync(ProductEditViewModel editModel)
    {
        try
        {
            _logger.LogInformation("商品更新サービス実行。商品ID: {ProductId}", editModel.Id);

            // 先にカテゴリ関係を更新（InMemoryプロバイダーの複合キー問題を回避）
            await _productRepository.UpdateProductCategoriesAsync(editModel.Id, editModel.SelectedCategoryIds?.ToList() ?? new List<int>());

            // 基本情報のみを更新（カテゴリ関係は含めない）
            var product = await _productRepository.GetByIdAsync(editModel.Id, includeRelations: false);
            if (product == null)
            {
                _logger.LogWarning("更新対象の商品が見つかりませんでした。商品ID: {ProductId}", editModel.Id);
                return false;
            }

            // 基本情報の更新
            product.Name = editModel.Name;
            product.Description = editModel.Description;
            product.Price = editModel.Price;
            product.JanCode = editModel.JanCode;
            product.Status = editModel.Status;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateBasicPropertiesAsync(product);
            _logger.LogInformation("商品を正常に更新しました。商品ID: {ProductId}", product.Id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品更新サービス実行中にエラーが発生しました。商品ID: {ProductId}", editModel.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            _logger.LogInformation("商品削除サービス実行。商品ID: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("削除対象の商品が見つかりませんでした。商品ID: {ProductId}", id);
                return false;
            }

            await _productRepository.DeleteAsync(id);
            _logger.LogInformation("商品を正常に削除しました。商品ID: {ProductId}", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品削除サービス実行中にエラーが発生しました。商品ID: {ProductId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductCreateViewModel> GetProductForCreateAsync()
    {
        return await PrepareCreateViewModelAsync();
    }

    /// <inheritdoc />
    public async Task<ProductEditViewModel?> GetProductForEditAsync(int id)
    {
        return await PrepareEditViewModelAsync(id);
    }

    /// <inheritdoc />
    public async Task<ProductListDto> GetProductsAsync(ProductSearchRequestDto searchRequest)
    {
        // 基本実装: 既存のGetProductListAsyncを活用
        return await GetProductListAsync(
            searchRequest.NameTerm,
            searchRequest.CategoryId,
            searchRequest.Page,
            searchRequest.PageSize);
    }


    /// <inheritdoc />
    public async Task<ProductStatisticsDto> GetStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("商品統計情報取得サービス実行");

            var (allProducts, totalCount) = await _productRepository.GetAllAsync();
            
            var statistics = new ProductStatisticsDto
            {
                TotalCount = totalCount,
                PreSaleCount = allProducts.Count(p => p.Status == ProductStatus.PreSale),
                OnSaleCount = allProducts.Count(p => p.Status == ProductStatus.OnSale),
                DiscontinuedCount = allProducts.Count(p => p.Status == ProductStatus.Discontinued),
                AveragePrice = allProducts.Any() ? (uint)allProducts.Average(p => (double)p.Price) : 0,
                MaxPrice = allProducts.Any() ? allProducts.Max(p => p.Price) : 0,
                MinPrice = allProducts.Any() ? allProducts.Min(p => p.Price) : 0
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品統計情報取得サービス実行中にエラーが発生しました");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetProductCountByCategoryAsync(int categoryId)
    {
        try
        {
            _logger.LogInformation("カテゴリ別商品数取得サービス実行。カテゴリID: {CategoryId}", categoryId);

            var (allProducts, _) = await _productRepository.GetAllAsync();
            return allProducts.Count(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ別商品数取得サービス実行中にエラーが発生しました。カテゴリID: {CategoryId}", categoryId);
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// 商品用カテゴリ選択項目を取得（最下位カテゴリのみ表示、ラベルで階層構造を表現）
    /// </summary>
    /// <param name="categories">カテゴリエンティティのコレクション</param>
    /// <returns>最下位カテゴリのみのCategorySelectItemsのコレクション（フラットリスト）</returns>
    private IEnumerable<CategorySelectItem> ConvertToCategorySelectItems(IEnumerable<Category> categories)
    {
        // 親子関係をディクショナリで構築
        var categoryDict = categories.ToDictionary(c => c.Id);
        
        // 最下位カテゴリ（子を持たないカテゴリ）のみを取得
        var leafCategories = categories.Where(c => !categories.Any(child => child.ParentCategoryId == c.Id)).ToList();

        return leafCategories.Select(leaf => new CategorySelectItem
        {
            Id = leaf.Id,
            Name = leaf.Name,
            Description = leaf.Description,
            FullPath = BuildCategoryFullPath(leaf, categoryDict),
            Level = leaf.Level,
            ProductCount = null, // 商品数は表示しないため null
            ChildCategories = new List<CategorySelectItem>() // 最下位なので子カテゴリはなし
        }).OrderBy(c => c.FullPath);
    }



    /// <summary>
    /// カテゴリの完全パスを構築
    /// </summary>
    /// <param name="category">対象カテゴリ</param>
    /// <param name="categoryDict">カテゴリ辞書</param>
    /// <returns>完全パス</returns>
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

    #endregion
}