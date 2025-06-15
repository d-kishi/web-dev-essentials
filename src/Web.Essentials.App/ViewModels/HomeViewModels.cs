using Web.Essentials.Domain.Entities;

namespace Web.Essentials.App.ViewModels;

/// <summary>
/// ホーム画面用ビューモデル
/// </summary>
/// <remarks>
/// トップページでの新着商品、統計情報、カテゴリツリーの表示に使用
/// </remarks>
public class HomeIndexViewModel
{
    /// <summary>
    /// 新着商品一覧
    /// </summary>
    public IEnumerable<ProductListItemViewModel> LatestProducts { get; set; } = new List<ProductListItemViewModel>();

    /// <summary>
    /// 商品統計情報
    /// </summary>
    public ProductStatisticsDisplayModel ProductStatistics { get; set; } = new();

    /// <summary>
    /// カテゴリツリー
    /// </summary>
    public IEnumerable<CategoryTreeViewModel> CategoryTree { get; set; } = new List<CategoryTreeViewModel>();

    /// <summary>
    /// お知らせメッセージ
    /// </summary>
    public string? AnnouncementMessage { get; set; }

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    /// <summary>
    /// 最終更新日時表示（フォーマット済み）
    /// </summary>
    public string FormattedLastUpdated => LastUpdated.ToString("yyyy/MM/dd HH:mm");
}

/// <summary>
/// 商品統計情報表示用モデル
/// </summary>
/// <remarks>
/// ホーム画面でのダッシュボード表示用
/// Domain層のProductStatisticsから変換
/// </remarks>
public class ProductStatisticsDisplayModel
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
    public decimal AveragePrice { get; set; }

    /// <summary>
    /// 最高価格
    /// </summary>
    public decimal MaxPrice { get; set; }

    /// <summary>
    /// 最低価格
    /// </summary>
    public decimal MinPrice { get; set; }

    /// <summary>
    /// 平均価格表示（フォーマット済み）
    /// </summary>
    public string FormattedAveragePrice => $"¥{AveragePrice:N0}";

    /// <summary>
    /// 最高価格表示（フォーマット済み）
    /// </summary>
    public string FormattedMaxPrice => $"¥{MaxPrice:N0}";

    /// <summary>
    /// 最低価格表示（フォーマット済み）
    /// </summary>
    public string FormattedMinPrice => $"¥{MinPrice:N0}";

    /// <summary>
    /// 販売中商品の割合
    /// </summary>
    public double OnSalePercentage => TotalCount > 0 ? (double)OnSaleCount / TotalCount * 100 : 0;

    /// <summary>
    /// 販売中商品の割合表示（フォーマット済み）
    /// </summary>
    public string FormattedOnSalePercentage => $"{OnSalePercentage:F1}%";

    /// <summary>
    /// 統計情報が存在するかどうか
    /// </summary>
    public bool HasData => TotalCount > 0;
}

/// <summary>
/// サイト情報表示用モデル
/// </summary>
/// <remarks>
/// サイト全体の情報やメタデータの表示用
/// </remarks>
public class SiteInfoDisplayModel
{
    /// <summary>
    /// サイト名
    /// </summary>
    public string SiteName { get; set; } = "Web.Essentials - ECサイト商品管理システム";

    /// <summary>
    /// サイト説明
    /// </summary>
    public string SiteDescription { get; set; } = ".NET 8 MVC + Pure JavaScript で構築した商品管理システム";

    /// <summary>
    /// バージョン情報
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// 開発環境情報
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// 使用技術情報
    /// </summary>
    public List<string> TechStack { get; set; } = new()
    {
        ".NET 8",
        "ASP.NET Core MVC",
        "Entity Framework Core (InMemory)",
        "Pure JavaScript",
        "RxJS",
        "Fetch API",
        "Clean Architecture"
    };
}