using Web.Essentials.App.DTOs;
using Web.Essentials.App.ViewModels;
using Web.Essentials.Domain.Entities;
using Web.Essentials.Domain.Repositories;

namespace Web.Essentials.App.Services;

/// <summary>
/// 商品管理アプリケーションサービス
/// 商品に関するビジネスロジックと各レイヤーの調整を担当
/// </summary>
public class ProductService
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

    /// <summary>
    /// 商品一覧取得（検索・ページング対応）
    /// コントローラーから呼び出される商品一覧取得の共通ロジック
    /// </summary>
    /// <param name="searchKeyword">検索キーワード</param>
    /// <param name="categoryId">カテゴリID（オプション）</param>
    /// <param name="page">ページ番号</param>
    /// <param name="pageSize">1ページあたりの件数</param>
    /// <returns>商品一覧とページング情報</returns>
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
                SearchKeyword = searchKeyword,
                CategoryId = categoryId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品一覧取得サービス実行中にエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// 商品詳細取得
    /// 指定されたIDの商品詳細を取得
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>商品詳細情報、存在しない場合はnull</returns>
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

            // 商品画像とカテゴリ情報を取得
            var productImages = await _productImageRepository.GetByProductIdAsync(id);
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);

            // ViewModelに変換
            var viewModel = new ProductDetailsViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = (uint)product.Price,
                CategoryId = product.CategoryId,
                CategoryName = category?.Name ?? "未分類",
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

    /// <summary>
    /// 商品登録用のViewModel準備
    /// カテゴリ一覧などの必要なデータを含む登録用ViewModelを作成
    /// </summary>
    /// <returns>商品登録用ViewModel</returns>
    public async Task<ProductCreateViewModel> PrepareCreateViewModelAsync()
    {
        try
        {
            _logger.LogInformation("商品登録用ViewModel準備サービス実行");

            var categories = await _categoryRepository.GetAllAsync();

            return new ProductCreateViewModel
            {
                Categories = categories.Select(c => new CategorySelectItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    FullPath = c.Name, // Simple path for now
                    Level = 0 // Default level for now
                }).OrderBy(c => c.Name).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品登録用ViewModel準備サービス実行中にエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// 商品編集用のViewModel準備
    /// 既存の商品データとカテゴリ一覧を含む編集用ViewModelを作成
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>商品編集用ViewModel、存在しない場合はnull</returns>
    public async Task<ProductEditViewModel?> PrepareEditViewModelAsync(int id)
    {
        try
        {
            _logger.LogInformation("商品編集用ViewModel準備サービス実行。商品ID: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("商品が見つかりませんでした。商品ID: {ProductId}", id);
                return null;
            }

            // カテゴリ一覧と商品画像を取得
            var categories = await _categoryRepository.GetAllAsync();
            var productImages = await _productImageRepository.GetByProductIdAsync(id);

            return new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = (uint)product.Price,
                CategoryId = product.CategoryId,
                JanCode = product.JanCode,
                Categories = categories.Select(c => new CategorySelectItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    FullPath = c.Name, // Simple path for now
                    Level = 0 // Default level for now
                }).OrderBy(c => c.Name).ToList(),
                ExistingImages = productImages.Select(img => new ProductImageViewModel
                {
                    Id = img.Id,
                    ProductId = img.ProductId,
                    ImagePath = img.ImagePath,
                    DisplayOrder = img.DisplayOrder,
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

    /// <summary>
    /// JANコード重複チェック
    /// 指定されたJANコードが既に使用されているかをチェック
    /// </summary>
    /// <param name="janCode">チェック対象のJANコード</param>
    /// <param name="excludeProductId">除外する商品ID（編集時に自分自身を除外）</param>
    /// <returns>重複している場合はtrue</returns>
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

        // カテゴリIDによるフィルタリング
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
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
        var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetAllAsync();
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryId = p.CategoryId,
            CategoryName = categoryDict.TryGetValue(p.CategoryId, out var categoryName) ? categoryName : "未分類",
            JanCode = p.JanCode,
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
}