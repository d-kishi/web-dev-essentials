using Microsoft.AspNetCore.Mvc;
using Web.Essentials.App.DTOs;
using Web.Essentials.Domain.Repositories;

namespace Web.Essentials.App.Controllers.Api;

/// <summary>
/// カテゴリ管理APIコントローラー
/// Ajax通信によるカテゴリデータの取得・検索・ページングを提供
/// </summary>
[ApiController]
[Route("api/categories")]
public class CategoryApiController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryApiController> _logger;

    /// <summary>
    /// コンストラクタ
    /// 依存性注入によりリポジトリとロガーを受け取る
    /// </summary>
    /// <param name="categoryRepository">カテゴリリポジトリ</param>
    /// <param name="logger">ロガー</param>
    public CategoryApiController(
        ICategoryRepository categoryRepository,
        ILogger<CategoryApiController> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <summary>
    /// カテゴリ一覧取得（検索対応）
    /// Ajax通信で呼び出され、カテゴリ一覧をJSON形式で返却
    /// 検索キーワードが指定された場合は、ヒットしたカテゴリとその祖先のみを返却
    /// </summary>
    /// <param name="searchKeyword">検索キーワード（カテゴリ名での部分一致検索）</param>
    /// <returns>カテゴリ一覧を含むAPI応答</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories(
        [FromQuery] string? searchKeyword = null)
    {
        try
        {
            _logger.LogInformation("カテゴリ一覧取得API呼び出し。検索キーワード: {SearchKeyword}",
                searchKeyword);

            // 全カテゴリを取得
            var allCategories = await _categoryRepository.GetAllAsync();
            
            List<CategoryDto> resultCategories;

            if (string.IsNullOrWhiteSpace(searchKeyword))
            {
                // 検索キーワードが無い場合は全カテゴリを返却
                resultCategories = allCategories
                    .OrderBy(c => c.Level)
                    .ThenBy(c => c.SortOrder)
                    .Select(c => CreateCategoryDto(c, allCategories))
                    .ToList();
            }
            else
            {
                // 検索キーワードに部分一致するカテゴリを取得
                var hitCategories = allCategories
                    .Where(c => c.Name.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // ヒットしたカテゴリとその祖先カテゴリのIDセットを作成
                var resultCategoryIds = new HashSet<int>();
                
                foreach (var hitCategory in hitCategories)
                {
                    // ヒットしたカテゴリ自体を追加
                    resultCategoryIds.Add(hitCategory.Id);
                    
                    // 祖先カテゴリを辿って追加
                    var currentCategory = hitCategory;
                    while (currentCategory.ParentCategoryId.HasValue)
                    {
                        var parentCategory = allCategories
                            .FirstOrDefault(c => c.Id == currentCategory.ParentCategoryId.Value);
                        
                        if (parentCategory != null)
                        {
                            resultCategoryIds.Add(parentCategory.Id);
                            currentCategory = parentCategory;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                // 結果に含めるカテゴリを抽出してソート
                resultCategories = allCategories
                    .Where(c => resultCategoryIds.Contains(c.Id))
                    .OrderBy(c => c.Level)
                    .ThenBy(c => c.SortOrder)
                    .Select(c => CreateCategoryDto(c, allCategories))
                    .ToList();
            }

            // レスポンスDTO作成
            var response = new ApiResponse<List<CategoryDto>>
            {
                Success = true,
                Data = resultCategories,
                Message = string.IsNullOrWhiteSpace(searchKeyword) 
                    ? "カテゴリ一覧を正常に取得しました"
                    : $"検索キーワード「{searchKeyword}」に該当するカテゴリとその祖先を取得しました（{resultCategories.Count}件）"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ一覧取得API実行中にエラーが発生しました");
            
            var errorResponse = new ApiResponse<List<CategoryDto>>
            {
                Success = false,
                Data = new List<CategoryDto>(),
                Message = "カテゴリ一覧の取得中にエラーが発生しました",
                Errors = new List<ApiErrorDto> { new ApiErrorDto("サーバー内部エラーが発生しました") }
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// カテゴリエンティティからDTOを作成するヘルパーメソッド
    /// </summary>
    /// <param name="category">カテゴリエンティティ</param>
    /// <param name="allCategories">全カテゴリリスト（子カテゴリ存在判定用）</param>
    /// <returns>カテゴリDTO</returns>
    private CategoryDto CreateCategoryDto(Domain.Entities.Category category, IEnumerable<Domain.Entities.Category> allCategories)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            Level = category.Level,
            SortOrder = category.SortOrder,
            FullPath = category.GetFullPath(),
            ProductCount = category.ProductCategories?.Count ?? 0,
            HasChildren = allCategories.Any(child => child.ParentCategoryId == category.Id),
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    /// <summary>
    /// カテゴリ詳細取得
    /// 指定されたIDのカテゴリ詳細をJSON形式で返却
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>カテゴリ詳細のAPI応答</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
    {
        try
        {
            _logger.LogInformation("カテゴリ詳細取得API呼び出し。カテゴリID: {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id);
            
            if (category == null)
            {
                var notFoundResponse = new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Data = null,
                    Message = $"カテゴリID {id} が見つかりません",
                    Errors = new List<ApiErrorDto> { new ApiErrorDto("指定されたカテゴリが存在しません") }
                };

                return NotFound(notFoundResponse);
            }

            // DTOに変換
            var allCategories = await _categoryRepository.GetAllAsync();
            var categoryDTO = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                Level = category.Level,
                SortOrder = category.SortOrder,
                FullPath = category.GetFullPath(),
                ProductCount = category.ProductCategories?.Count ?? 0,
                HasChildren = allCategories.Any(child => child.ParentCategoryId == category.Id),
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            var response = new ApiResponse<CategoryDto>
            {
                Success = true,
                Data = categoryDTO,
                Message = "カテゴリ詳細を正常に取得しました"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ詳細取得API実行中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            
            var errorResponse = new ApiResponse<CategoryDto>
            {
                Success = false,
                Data = null,
                Message = "カテゴリ詳細の取得中にエラーが発生しました",
                Errors = new List<ApiErrorDto> { new ApiErrorDto("サーバー内部エラーが発生しました") }
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// 全カテゴリ一覧取得（選択肢用）
    /// セレクトボックスなどで使用するための全カテゴリをJSON形式で返却
    /// </summary>
    /// <returns>全カテゴリのAPI応答</returns>
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAllCategories()
    {
        try
        {
            _logger.LogInformation("全カテゴリ一覧取得API呼び出し");

            var allCategories = await _categoryRepository.GetAllAsync();

            // DTOに変換
            var categoryDTOs = allCategories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList();

            var response = new ApiResponse<List<CategoryDto>>
            {
                Success = true,
                Data = categoryDTOs,
                Message = $"全カテゴリ一覧を正常に取得しました（{categoryDTOs.Count}件）"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "全カテゴリ一覧取得API実行中にエラーが発生しました");
            
            var errorResponse = new ApiResponse<List<CategoryDto>>
            {
                Success = false,
                Data = new List<CategoryDto>(),
                Message = "全カテゴリ一覧の取得中にエラーが発生しました",
                Errors = new List<ApiErrorDto> { new ApiErrorDto("サーバー内部エラーが発生しました") }
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// カテゴリ検索候補取得
    /// オートコンプリート用のカテゴリ名候補をJSON形式で返却
    /// </summary>
    /// <param name="term">検索語句（部分一致）</param>
    /// <param name="limit">取得件数上限（デフォルト10）</param>
    /// <returns>カテゴリ名候補のAPI応答</returns>
    [HttpGet("suggestions")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetCategorySuggestions(
        [FromQuery] string term,
        [FromQuery] int limit = 10)
    {
        try
        {
            _logger.LogInformation("カテゴリ検索候補取得API呼び出し。検索語句: {Term}, 上限: {Limit}", term, limit);

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

            var allCategories = await _categoryRepository.GetAllAsync();
            
            // カテゴリ名での部分一致検索（重複排除）
            var suggestions = allCategories
                .Where(c => c.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Name)
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
            _logger.LogError(ex, "カテゴリ検索候補取得API実行中にエラーが発生しました");
            
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
}