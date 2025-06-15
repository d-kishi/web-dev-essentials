namespace Web.Essentials.Domain.Entities;

/// <summary>
/// 商品カテゴリ関連エンティティ - 商品とカテゴリの多対多関係を管理する中間テーブル
/// </summary>
/// <remarks>
/// このクラスは商品とカテゴリの関連を管理し、以下の特徴を持つ：
/// - 複合主キー（ProductId, CategoryId）による一意性保証
/// - 商品は複数のカテゴリに属することが可能
/// - カテゴリには複数の商品が属することが可能
/// - 関連付けの作成日時を記録
/// 
/// 設計方針：
/// - 商品は最下層カテゴリのみに関連付けることを推奨
/// - 階層の上位カテゴリへの関連は自動的に推論される
/// </remarks>
public class ProductCategory
{
    /// <summary>
    /// 商品ID（複合主キーの一部、外部キー）
    /// </summary>
    /// <remarks>
    /// Product エンティティへの外部キー
    /// 複合主キーの構成要素
    /// </remarks>
    public int ProductId { get; set; }

    /// <summary>
    /// カテゴリID（複合主キーの一部、外部キー）
    /// </summary>
    /// <remarks>
    /// Category エンティティへの外部キー
    /// 複合主キーの構成要素
    /// </remarks>
    public int CategoryId { get; set; }

    /// <summary>
    /// 関連付け作成日時
    /// </summary>
    /// <remarks>
    /// 商品とカテゴリの関連が作成された日時
    /// UTC時刻で保存
    /// 監査目的で使用
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 関連する商品（ナビゲーションプロパティ）
    /// </summary>
    /// <remarks>
    /// ProductId で参照される Product エンティティ
    /// Entity Framework による遅延読み込み対応
    /// </remarks>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// 関連するカテゴリ（ナビゲーションプロパティ）
    /// </summary>
    /// <remarks>
    /// CategoryId で参照される Category エンティティ
    /// Entity Framework による遅延読み込み対応
    /// </remarks>
    public virtual Category Category { get; set; } = null!;
}