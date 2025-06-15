namespace Web.Essentials.Domain.Entities;

/// <summary>
/// カテゴリエンティティ - 商品分類のための階層構造を持つドメインエンティティ
/// </summary>
/// <remarks>
/// このクラスは商品カテゴリの階層構造を管理し、以下の特徴を持つ：
/// - 3階層までの階層構造（ルート→サブ→詳細）
/// - 自己参照による親子関係
/// - 表示順序の管理
/// - 商品との多対多関係は ProductCategory エンティティで管理
/// 
/// 階層例：
/// Level 0: スポーツ（ルートカテゴリ）
/// Level 1: └ ランニング（サブカテゴリ）
/// Level 2:   └ シューズ（詳細カテゴリ）
/// </remarks>
public class Category
{
    /// <summary>
    /// カテゴリID（主キー）
    /// </summary>
    /// <remarks>
    /// データベースで自動採番されるカテゴリの一意識別子
    /// </remarks>
    public int Id { get; set; }

    /// <summary>
    /// カテゴリ名
    /// </summary>
    /// <remarks>
    /// 必須項目、最大50文字、システム全体で重複不可
    /// カテゴリの表示名として使用される
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// カテゴリ説明
    /// </summary>
    /// <remarks>
    /// 任意項目、最大500文字
    /// カテゴリの詳細説明や対象商品の特徴を記述
    /// </remarks>
    public string? Description { get; set; }

    /// <summary>
    /// 親カテゴリID（外部キー）
    /// </summary>
    /// <remarks>
    /// 任意項目、ルートカテゴリの場合はnull
    /// 階層構造を実現するための自己参照外部キー
    /// </remarks>
    public int? ParentCategoryId { get; set; }

    /// <summary>
    /// 階層レベル
    /// </summary>
    /// <remarks>
    /// 必須項目、0以上の整数
    /// 0: ルートカテゴリ
    /// 1: サブカテゴリ
    /// 2: 詳細カテゴリ
    /// 最大3階層まで対応
    /// </remarks>
    public int Level { get; set; } = 0;

    /// <summary>
    /// 表示順序
    /// </summary>
    /// <remarks>
    /// 必須項目、同一階層内での表示順序を制御
    /// 小さい値ほど前に表示される
    /// </remarks>
    public int SortOrder { get; set; } = 0;

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
    /// 親カテゴリ（ナビゲーションプロパティ）
    /// </summary>
    /// <remarks>
    /// 自己参照による親カテゴリへの参照
    /// ルートカテゴリの場合はnull
    /// </remarks>
    public virtual Category? ParentCategory { get; set; }

    /// <summary>
    /// 子カテゴリ一覧（ナビゲーションプロパティ）
    /// </summary>
    /// <remarks>
    /// 自己参照による子カテゴリの集合
    /// 階層構造の下位レベルを表現
    /// </remarks>
    public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();

    /// <summary>
    /// 商品カテゴリ関連（多対多関係の管理用）
    /// </summary>
    /// <remarks>
    /// ProductCategory エンティティを介して商品との関係を管理
    /// 一つのカテゴリには複数の商品が属することが可能
    /// </remarks>
    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    /// <summary>
    /// カテゴリの完全パス文字列を取得
    /// </summary>
    /// <returns>ルートから現在のカテゴリまでの階層パス（例: "スポーツ > ランニング > シューズ"）</returns>
    /// <remarks>
    /// 表示用のヘルパーメソッド
    /// 階層構造を分かりやすく表現するために使用
    /// </remarks>
    public string GetFullPath()
    {
        var path = new List<string>();
        var current = this;

        // 親を辿ってパスを構築（逆順で追加）
        while (current != null)
        {
            path.Insert(0, current.Name);
            current = current.ParentCategory;
        }

        return string.Join(" > ", path);
    }

    /// <summary>
    /// 指定されたカテゴリが子孫カテゴリかどうかを判定
    /// </summary>
    /// <param name="category">判定対象のカテゴリ</param>
    /// <returns>子孫カテゴリの場合true、そうでなければfalse</returns>
    /// <remarks>
    /// カテゴリの移動や削除時の循環参照チェックに使用
    /// </remarks>
    public bool IsDescendantOf(Category category)
    {
        var current = ParentCategory;
        
        while (current != null)
        {
            if (current.Id == category.Id)
                return true;
            current = current.ParentCategory;
        }

        return false;
    }
}