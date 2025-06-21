using Web.Essentials.Domain.Entities;

namespace Web.Essentials.App.DTOs;


/// <summary>
/// 商品API応答用DTO
/// </summary>
/// <remarks>
/// Ajax通信での商品データ転送に使用
/// ViewModelより軽量で、API専用の構造
/// </remarks>
public class ProductDto
{
    /// <summary>
    /// 商品ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品説明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 価格
    /// </summary>
    public uint Price { get; set; }

    /// <summary>
    /// JANコード
    /// </summary>
    public string? JanCode { get; set; }

    /// <summary>
    /// ステータス（数値）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// ステータス名
    /// </summary>
    public string StatusName { get; set; } = string.Empty;


    /// <summary>
    /// カテゴリ情報一覧
    /// </summary>
    public List<ProductCategoryDto> Categories { get; set; } = new();

    /// <summary>
    /// 画像情報一覧
    /// </summary>
    public List<ProductImageDto> Images { get; set; } = new();

    /// <summary>
    /// 作成日時（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時（UTC）
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 商品一覧項目用DTO
/// </summary>
/// <remarks>
/// 商品一覧API応答での軽量データ転送用
/// 必要最小限の情報のみ含む
/// </remarks>
public class ProductListItemDto
{
    /// <summary>
    /// 商品ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品説明（短縮版）
    /// </summary>
    public string? ShortDescription { get; set; }

    /// <summary>
    /// 価格
    /// </summary>
    public uint Price { get; set; }

    /// <summary>
    /// JANコード
    /// </summary>
    public string? JanCode { get; set; }

    /// <summary>
    /// ステータス（数値）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// ステータス名
    /// </summary>
    public string StatusName { get; set; } = string.Empty;

    /// <summary>
    /// メイン画像パス
    /// </summary>
    public string? MainImagePath { get; set; }

    /// <summary>
    /// カテゴリ名一覧（カンマ区切り用）
    /// </summary>
    public List<string> CategoryNames { get; set; } = new();

    /// <summary>
    /// 作成日時（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 商品カテゴリ情報DTO
/// </summary>
/// <remarks>
/// 商品に関連付けられたカテゴリ情報
/// </remarks>
public class ProductCategoryDto
{
    /// <summary>
    /// カテゴリID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// カテゴリ名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 完全パス（階層構造）
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// 階層レベル
    /// </summary>
    public int Level { get; set; }
}

/// <summary>
/// 商品画像情報DTO
/// </summary>
/// <remarks>
/// 商品に関連付けられた画像情報
/// </remarks>
public class ProductImageDto
{
    /// <summary>
    /// 画像ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 画像パス
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 表示順序
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 代替テキスト
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// メイン画像フラグ
    /// </summary>
    public bool IsMain { get; set; }
}

/// <summary>
/// 商品検索リクエストDTO
/// </summary>
/// <remarks>
/// Ajax商品検索APIのリクエストパラメータ
/// </remarks>
public class ProductSearchRequestDto
{
    /// <summary>
    /// ページ番号（1から開始）
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 1ページあたりの件数
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 商品名での部分一致検索
    /// </summary>
    public string? NameTerm { get; set; }

    /// <summary>
    /// JANコードでの部分一致検索
    /// </summary>
    public string? JanCode { get; set; }

    /// <summary>
    /// カテゴリIDでの絞り込み
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// ステータスでの絞り込み
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// 最低価格
    /// </summary>
    public uint? MinPrice { get; set; }

    /// <summary>
    /// 最高価格
    /// </summary>
    public uint? MaxPrice { get; set; }

    /// <summary>
    /// ソート項目
    /// </summary>
    public string SortBy { get; set; } = "createdAt";

    /// <summary>
    /// ソート順序
    /// </summary>
    public string SortOrder { get; set; } = "desc";

    /// <summary>
    /// ProductStatusへの変換
    /// </summary>
    public ProductStatus? GetProductStatus()
    {
        return Status.HasValue ? (ProductStatus)Status.Value : null;
    }
}

/// <summary>
/// 商品統計情報DTO
/// </summary>
/// <remarks>
/// ダッシュボードAPI応答用
/// </remarks>
public class ProductStatisticsDto
{
    /// <summary>
    /// 総商品数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 販売開始前商品数
    /// </summary>
    public int PreSaleCount { get; set; }

    /// <summary>
    /// 販売中商品数
    /// </summary>
    public int OnSaleCount { get; set; }

    /// <summary>
    /// 取扱終了商品数
    /// </summary>
    public int DiscontinuedCount { get; set; }

    /// <summary>
    /// 平均価格
    /// </summary>
    public uint AveragePrice { get; set; }

    /// <summary>
    /// 最高価格
    /// </summary>
    public uint MaxPrice { get; set; }

    /// <summary>
    /// 最低価格
    /// </summary>
    public uint MinPrice { get; set; }
}

/// <summary>
/// ページング情報DTO
/// </summary>
/// <remarks>
/// API応答でのページング情報転送用
/// </remarks>
public class PagingDto
{
    /// <summary>
    /// 現在のページ番号
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// 総ページ数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 1ページあたりの件数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 総件数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 次のページが存在するか
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// 前のページが存在するか
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// 開始レコード番号
    /// </summary>
    public int StartRecord => (CurrentPage - 1) * PageSize + 1;

    /// <summary>
    /// 終了レコード番号
    /// </summary>
    public int EndRecord => Math.Min(CurrentPage * PageSize, TotalCount);
}

/// <summary>
/// 商品一覧応答DTO
/// </summary>
/// <remarks>
/// 商品一覧API応答用のラッパークラス
/// 商品リストとページング情報を含む
/// </remarks>
public class ProductListDto
{
    /// <summary>
    /// 商品一覧
    /// </summary>
    public List<ProductDto> Products { get; set; } = new();

    /// <summary>
    /// ページング情報
    /// </summary>
    public PagingDto Paging { get; set; } = new();

    /// <summary>
    /// 検索キーワード
    /// </summary>
    public string? SearchKeyword { get; set; }

    /// <summary>
    /// フィルタ用カテゴリID
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// フィルタ用ステータス
    /// </summary>
    public int? Status { get; set; }

}