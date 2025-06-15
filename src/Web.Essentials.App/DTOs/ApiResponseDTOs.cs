namespace Web.Essentials.App.DTOs;

/// <summary>
/// API 共通レスポンス基底クラス
/// </summary>
/// <remarks>
/// すべてのAPI応答で使用する共通フォーマット
/// 成功・失敗の判定とエラー情報を統一
/// </remarks>
public class ApiResponseBase
{
    /// <summary>
    /// 成功フラグ
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// メッセージ
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// エラー一覧
    /// </summary>
    public List<ApiErrorDto>? Errors { get; set; }

    /// <summary>
    /// エラーが存在するかどうか
    /// </summary>
    public bool HasErrors => Errors?.Any() == true;

    /// <summary>
    /// 成功応答を作成
    /// </summary>
    /// <param name="message">成功メッセージ</param>
    /// <returns>成功応答</returns>
    public static ApiResponseBase CreateSuccess(string? message = null)
    {
        return new ApiResponseBase
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// エラー応答を作成
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="errors">エラー詳細</param>
    /// <returns>エラー応答</returns>
    public static ApiResponseBase CreateError(string message, List<ApiErrorDto>? errors = null)
    {
        return new ApiResponseBase
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

/// <summary>
/// データ付きAPI応答クラス
/// </summary>
/// <typeparam name="T">データの型</typeparam>
/// <remarks>
/// データを含むAPI応答で使用
/// </remarks>
public class ApiResponse<T> : ApiResponseBase
{
    /// <summary>
    /// レスポンスデータ
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 成功応答を作成
    /// </summary>
    /// <param name="data">レスポンスデータ</param>
    /// <param name="message">成功メッセージ</param>
    /// <returns>成功応答</returns>
    public static ApiResponse<T> CreateSuccess(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    /// <summary>
    /// エラー応答を作成
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="errors">エラー詳細</param>
    /// <returns>エラー応答</returns>
    public static new ApiResponse<T> CreateError(string message, List<ApiErrorDto>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            Data = default
        };
    }
}

/// <summary>
/// API エラー詳細情報
/// </summary>
/// <remarks>
/// バリデーションエラーやフィールド固有のエラー情報
/// </remarks>
public class ApiErrorDto
{
    /// <summary>
    /// フィールド名
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// エラーコード
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="field">フィールド名</param>
    /// <param name="code">エラーコード</param>
    public ApiErrorDto(string message, string? field = null, string? code = null)
    {
        Message = message;
        Field = field;
        Code = code;
    }
}

/// <summary>
/// ページング付きAPI応答クラス
/// </summary>
/// <typeparam name="T">データの型</typeparam>
/// <remarks>
/// 一覧取得APIで使用するページング対応応答
/// </remarks>
public class PagedApiResponse<T> : ApiResponseBase
{
    /// <summary>
    /// データ一覧
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// 総件数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 現在のページ番号
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// 1ページあたりの件数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 総ページ数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// 次のページが存在するか
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>
    /// 前のページが存在するか
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// 成功応答を作成
    /// </summary>
    /// <param name="items">データ一覧</param>
    /// <param name="totalCount">総件数</param>
    /// <param name="currentPage">現在のページ番号</param>
    /// <param name="pageSize">1ページあたりの件数</param>
    /// <param name="message">成功メッセージ</param>
    /// <returns>成功応答</returns>
    public static PagedApiResponse<T> CreateSuccess(
        IEnumerable<T> items,
        int totalCount,
        int currentPage,
        int pageSize,
        string? message = null)
    {
        return new PagedApiResponse<T>
        {
            Success = true,
            Items = items,
            TotalCount = totalCount,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Message = message
        };
    }

    /// <summary>
    /// エラー応答を作成
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="errors">エラー詳細</param>
    /// <returns>エラー応答</returns>
    public static new PagedApiResponse<T> CreateError(string message, List<ApiErrorDto>? errors = null)
    {
        return new PagedApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            Items = new List<T>(),
            TotalCount = 0,
            CurrentPage = 1,
            PageSize = 10
        };
    }
}