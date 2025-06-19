using Microsoft.AspNetCore.Mvc;
using Web.Essentials.App.DTOs;
using Web.Essentials.Domain.Repositories;

namespace Web.Essentials.App.Controllers.Api;

/// <summary>
/// 商品管理APIコントローラー
/// Ajax通信による商品データの取得・検索・ページングを提供
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductApiController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<ProductApiController> _logger;

    /// <summary>
    /// コンストラクタ
    /// 依存性注入によりリポジトリとロガーを受け取る
    /// </summary>
    /// <param name="productRepository">商品リポジトリ</param>
    /// <param name="categoryRepository">カテゴリリポジトリ</param>
    /// <param name="logger">ロガー</param>
    public ProductApiController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ILogger<ProductApiController> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <summary>
    /// 商品一覧取得（検索・ページング対応）
    /// Ajax通信で呼び出され、商品一覧をJSON形式で返却
    /// </summary>
    /// <param name="searchKeyword">検索キーワード（商品名での部分一致検索）</param>
    /// <param name="categoryId">カテゴリIDでの絞り込み（オプション）</param>
    /// <param name="page">ページ番号（デフォルト1）</param>
    /// <param name="pageSize">1ページあたりの件数（デフォルト10）</param>
    /// <param name="sortBy">ソート項目（name_asc, price_desc, updatedat_asc等）</param>
    /// <returns>商品一覧とページング情報を含むAPI応答</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProductListDto>>> GetProducts(
        [FromQuery] string? searchKeyword = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "updatedat_desc")
    {
        try
        {
            _logger.LogInformation("商品一覧取得API呼び出し。検索キーワード: {SearchKeyword}, カテゴリID: {CategoryId}, ページ: {Page}, ページサイズ: {PageSize}, ソート: {SortBy}",
                searchKeyword, categoryId, page, pageSize, sortBy);

            // 入力値バリデーション
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            // ソートパラメーターの解析
            var (repositorySortBy, repositorySortOrder) = ParseSortParameter(sortBy);

            // 商品一覧を取得（検索・フィルタリング・ページング対応）
            var (allProducts, totalCount) = await _productRepository.GetAllAsync(
                nameTerm: searchKeyword,
                janCodeTerm: null,
                categoryId: categoryId,
                status: null,
                minPrice: null,
                maxPrice: null,
                sortBy: repositorySortBy,
                sortOrder: repositorySortOrder,
                page: page,
                pageSize: pageSize);

            // リポジトリで既にフィルタリング・ページング済みの商品を使用
            var pagedProducts = allProducts.ToList();

            // DTOに変換
            var productDTOs = pagedProducts.Select(p => new ProductDto
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

            // ページング情報を作成
            var paging = new PagingDto
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasPreviousPage = page > 1,
                HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize)
            };

            // レスポンスDTO作成
            var response = new ApiResponse<ProductListDto>
            {
                Success = true,
                Data = new ProductListDto
                {
                    Products = productDTOs,
                    Paging = paging,
                    SearchKeyword = searchKeyword,
                    CategoryId = categoryId
                },
                Message = "商品一覧を正常に取得しました"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品一覧取得API実行中にエラーが発生しました");
            
            var errorResponse = new ApiResponse<ProductListDto>
            {
                Success = false,
                Data = null,
                Message = "商品一覧の取得中にエラーが発生しました",
                Errors = new List<ApiErrorDto> { new ApiErrorDto("サーバー内部エラーが発生しました") }
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// 商品詳細取得
    /// 指定されたIDの商品詳細をJSON形式で返却
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>商品詳細のAPI応答</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
    {
        try
        {
            _logger.LogInformation("商品詳細取得API呼び出し。商品ID: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id);
            
            if (product == null)
            {
                var notFoundResponse = new ApiResponse<ProductDto>
                {
                    Success = false,
                    Data = null,
                    Message = $"商品ID {id} が見つかりません",
                    Errors = new List<ApiErrorDto> { new ApiErrorDto("指定された商品が存在しません") }
                };

                return NotFound(notFoundResponse);
            }

            // DTOに変換
            var productDTO = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                JanCode = product.JanCode,
                Status = (int)product.Status,
                StatusName = product.Status.ToString(),
                Categories = product.ProductCategories.Select(pc => new ProductCategoryDto
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name,
                    FullPath = pc.Category.Name,
                    Level = 1
                }).ToList(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            var response = new ApiResponse<ProductDto>
            {
                Success = true,
                Data = productDTO,
                Message = "商品詳細を正常に取得しました"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品詳細取得API実行中にエラーが発生しました。商品ID: {ProductId}", id);
            
            var errorResponse = new ApiResponse<ProductDto>
            {
                Success = false,
                Data = null,
                Message = "商品詳細の取得中にエラーが発生しました",
                Errors = new List<ApiErrorDto> { new ApiErrorDto("サーバー内部エラーが発生しました") }
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// 商品検索候補取得
    /// オートコンプリート用の商品名候補をJSON形式で返却
    /// </summary>
    /// <param name="term">検索語句（部分一致）</param>
    /// <param name="limit">取得件数上限（デフォルト10）</param>
    /// <returns>商品名候補のAPI応答</returns>
    [HttpGet("suggestions")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetProductSuggestions(
        [FromQuery] string term,
        [FromQuery] int limit = 10)
    {
        try
        {
            _logger.LogInformation("商品検索候補取得API呼び出し。検索語句: {Term}, 上限: {Limit}", term, limit);

            if (string.IsNullOrWhiteSpace(term))
            {
                var emptyResponse = new ApiResponse<List<string>>
                {
                    Success = true,
                    Data = new List<string>(),
                    Message = "検索語句が空のため、候補はありません"
                };

                return Ok(emptyResponse);
            }

            // 入力値バリデーション
            if (limit < 1 || limit > 50) limit = 10;

            var (allProducts, _) = await _productRepository.GetAllAsync(
                nameTerm: term,
                pageSize: limit);
            
            // 商品名での部分一致検索（重複排除）
            var suggestions = allProducts
                .Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Name)
                .Distinct()
                .OrderBy(name => name)
                .Take(limit)
                .ToList();

            var response = new ApiResponse<List<string>>
            {
                Success = true,
                Data = suggestions,
                Message = $"{suggestions.Count}件の候補を取得しました"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品検索候補取得API実行中にエラーが発生しました");
            
            var errorResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Data = new List<string>(),
                Message = "検索候補の取得中にエラーが発生しました",
                Errors = new List<ApiErrorDto> { new ApiErrorDto("サーバー内部エラーが発生しました") }
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// ソートパラメーターの解析
    /// フロントエンドから送信される "column_direction" 形式を
    /// リポジトリが期待する形式に変換
    /// </summary>
    /// <param name="sortBy">ソートパラメーター（例: "name_asc", "price_desc", "updatedat_desc"）</param>
    /// <returns>リポジトリ用のソート項目と順序のタプル</returns>
    private static (string sortBy, string sortOrder) ParseSortParameter(string sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return ("updatedat", "desc");
        }

        var parts = sortBy.Split('_');
        if (parts.Length != 2)
        {
            return ("updatedat", "desc");
        }

        var column = parts[0].ToLowerInvariant();
        var direction = parts[1].ToLowerInvariant();

        // カラム名のマッピング（フロントエンド → リポジトリ）
        var repositoryColumn = column switch
        {
            "name" => "name",
            "price" => "price", 
            "updatedat" => "updatedat", // リポジトリは updatedat をサポート
            "updated" => "updated",     // リポジトリは updated もサポート
            "created" => "created",
            "createdat" => "createdat",
            _ => "updatedat" // デフォルトは更新日時
        };

        // 方向のバリデーション
        var repositoryOrder = direction == "asc" ? "asc" : "desc";

        return (repositoryColumn, repositoryOrder);
    }
}