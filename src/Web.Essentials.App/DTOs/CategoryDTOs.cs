namespace Web.Essentials.App.DTOs;


/// <summary>
/// カテゴリAPI応答用DTO
/// </summary>
/// <remarks>
/// Ajax通信でのカテゴリデータ転送に使用
/// 階層構造情報を含む
/// </remarks>
public class CategoryDto
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
    /// カテゴリ説明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 親カテゴリID
    /// </summary>
    public int? ParentCategoryId { get; set; }

    /// <summary>
    /// 階層レベル
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 表示順序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 完全パス（階層構造）
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// 関連商品数（子孫カテゴリ含む）
    /// </summary>
    public int? ProductCount { get; set; }

    /// <summary>
    /// 子カテゴリを持つかどうか
    /// </summary>
    public bool HasChildren { get; set; }

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
/// カテゴリ一覧項目用DTO
/// </summary>
/// <remarks>
/// カテゴリ一覧API応答での軽量データ転送用
/// </remarks>
public class CategoryListItemDto
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
    /// カテゴリ説明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 親カテゴリID
    /// </summary>
    public int? ParentCategoryId { get; set; }

    /// <summary>
    /// 階層レベル
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 表示順序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 完全パス
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// 関連商品数
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// 子カテゴリ数
    /// </summary>
    public int ChildCount { get; set; }

    /// <summary>
    /// 作成日時（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// カテゴリツリー用DTO
/// </summary>
/// <remarks>
/// 階層構造でのカテゴリ表示API応答用
/// 再帰的な構造を持つ
/// </remarks>
public class CategoryTreeDto
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
    /// 階層レベル
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 表示順序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 関連商品数
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// 子カテゴリ一覧
    /// </summary>
    public List<CategoryTreeDto> Children { get; set; } = new();

    /// <summary>
    /// 子カテゴリを持つかどうか
    /// </summary>
    public bool HasChildren => Children.Any();
}

/// <summary>
/// カテゴリ検索リクエストDTO
/// </summary>
/// <remarks>
/// Ajaxカテゴリ検索APIのリクエストパラメータ
/// </remarks>
public class CategorySearchRequestDto
{
    /// <summary>
    /// 階層レベルでの絞り込み
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// 親カテゴリIDでの絞り込み
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// カテゴリ名での部分一致検索
    /// </summary>
    public string? NameTerm { get; set; }

    /// <summary>
    /// 関連商品数を含むかどうか
    /// </summary>
    public bool IncludeProductCount { get; set; } = false;
}

/// <summary>
/// カテゴリ選択項目DTO
/// </summary>
/// <remarks>
/// カテゴリ選択ドロップダウン用API応答
/// </remarks>
public class CategorySelectDto
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
    /// 完全パス
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// 階層レベル
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// インデント付きの表示名
    /// </summary>
    public string DisplayName => new string('　', Level) + Name;
}

/// <summary>
/// カテゴリパンくずリスト項目DTO
/// </summary>
/// <remarks>
/// 階層ナビゲーション用API応答
/// </remarks>
public class CategoryBreadcrumbDto
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
    /// 階層レベル
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 最後の項目かどうか
    /// </summary>
    public bool IsLast { get; set; }
}

