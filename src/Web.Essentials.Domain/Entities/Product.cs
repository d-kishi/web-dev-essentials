namespace Web.Essentials.Domain.Entities;

/// <summary>
/// 商品エンティティ - ECサイトの商品情報を表現するドメインエンティティ
/// </summary>
/// <remarks>
/// このクラスは商品の基本情報を管理し、以下の特徴を持つ：
/// - 商品の基本属性（名前、説明、価格、JANコード）
/// - 商品ステータス管理（販売開始前、販売中、取扱終了）
/// - 監査情報（作成日時、更新日時）
/// - カテゴリとの多対多関係は ProductCategory エンティティで管理
/// - 商品画像との一対多関係は ProductImage エンティティで管理
/// </remarks>
public class Product
{
    /// <summary>
    /// 商品ID（主キー）
    /// </summary>
    /// <remarks>
    /// データベースで自動採番される商品の一意識別子
    /// </remarks>
    public int Id { get; set; }

    /// <summary>
    /// 商品名
    /// </summary>
    /// <remarks>
    /// 必須項目、最大100文字
    /// 商品の表示名として使用される
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品説明
    /// </summary>
    /// <remarks>
    /// 任意項目、最大1000文字
    /// 商品の詳細情報や特徴を記述
    /// </remarks>
    public string? Description { get; set; }

    /// <summary>
    /// 価格（日本円）
    /// </summary>
    /// <remarks>
    /// 必須項目、0以上の値
    /// 税込み価格を想定
    /// </remarks>
    public decimal Price { get; set; }

    /// <summary>
    /// カテゴリID（外部キー）
    /// </summary>
    /// <remarks>
    /// 必須項目、商品が属するカテゴリのID
    /// 単純な1対多関係を使用
    /// </remarks>
    public int CategoryId { get; set; }

    /// <summary>
    /// JANコード（13桁）
    /// </summary>
    /// <remarks>
    /// 任意項目、設定時は13桁の数字
    /// 商品の国際的な標準識別コード
    /// 設定時は重複不可
    /// </remarks>
    public string? JanCode { get; set; }

    /// <summary>
    /// 商品ステータス
    /// </summary>
    /// <remarks>
    /// 必須項目、以下の値を持つ：
    /// 0: 販売開始前（Pre-Sale）
    /// 1: 販売中（On Sale）
    /// 2: 取扱終了（Discontinued）
    /// </remarks>
    public ProductStatus Status { get; set; } = ProductStatus.PreSale;

    /// <summary>
    /// 作成日時
    /// </summary>
    /// <remarks>
    /// エンティティ作成時に自動設定される
    /// UTC時刻で保存
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新日時
    /// </summary>
    /// <remarks>
    /// エンティティ更新時に自動更新される
    /// UTC時刻で保存
    /// </remarks>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 商品カテゴリ関連（多対多関係の管理用）
    /// </summary>
    /// <remarks>
    /// ProductCategory エンティティを介してカテゴリとの関係を管理
    /// 一つの商品は複数のカテゴリに属することが可能
    /// </remarks>
    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    /// <summary>
    /// 商品画像関連（一対多関係）
    /// </summary>
    /// <remarks>
    /// 一つの商品に対して最大5枚の画像を関連付け可能
    /// メイン画像とサブ画像の概念を含む
    /// </remarks>
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}

/// <summary>
/// 商品ステータス列挙型
/// </summary>
/// <remarks>
/// 商品の販売状態を表現する列挙型
/// データベースでは整数値として保存される
/// </remarks>
public enum ProductStatus
{
    /// <summary>
    /// 販売開始前 - まだ販売が開始されていない商品
    /// </summary>
    PreSale = 0,

    /// <summary>
    /// 販売中 - 現在販売中の商品
    /// </summary>
    OnSale = 1,

    /// <summary>
    /// 取扱終了 - 販売を終了した商品
    /// </summary>
    Discontinued = 2
}