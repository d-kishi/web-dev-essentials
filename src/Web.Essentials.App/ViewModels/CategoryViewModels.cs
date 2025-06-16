using System.ComponentModel.DataAnnotations;
using Web.Essentials.Domain.Entities;

namespace Web.Essentials.App.ViewModels;

/// <summary>
/// カテゴリ一覧画面用ビューモデル
/// </summary>
/// <remarks>
/// Ajax検索機能付きのカテゴリ一覧表示に使用
/// 階層構造の表示に対応
/// </remarks>
public class CategoryIndexViewModel
{
    /// <summary>
    /// 検索条件：カテゴリ名
    /// </summary>
    [Display(Name = "カテゴリ名")]
    [StringLength(50, ErrorMessage = "カテゴリ名は50文字以内で入力してください")]
    public string? NameTerm { get; set; }

    /// <summary>
    /// 検索条件：階層レベル
    /// </summary>
    [Display(Name = "階層レベル")]
    [Range(0, 2, ErrorMessage = "階層レベルは0-2の範囲で選択してください")]
    public int? Level { get; set; }

    /// <summary>
    /// 検索条件：親カテゴリID
    /// </summary>
    [Display(Name = "親カテゴリ")]
    public int? ParentId { get; set; }

    /// <summary>
    /// 商品数を含むかどうか
    /// </summary>
    [Display(Name = "商品数を表示")]
    public bool IncludeProductCount { get; set; } = true;

    /// <summary>
    /// カテゴリ一覧（Ajax更新用・初期表示では階層構造で表示）
    /// </summary>
    public IEnumerable<CategoryListItemViewModel> Categories { get; set; } = new List<CategoryListItemViewModel>();

    /// <summary>
    /// 親カテゴリ選択用リスト
    /// </summary>
    public IEnumerable<CategorySelectItem> ParentCategories { get; set; } = new List<CategorySelectItem>();

    /// <summary>
    /// メッセージ
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 検索キーワード（現在の検索条件保持用）
    /// </summary>
    public string? SearchKeyword { get; set; }

    /// <summary>
    /// 現在のページ番号（ページング用）
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// 1ページあたりの表示件数
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// ページング情報
    /// </summary>
    public PagingInfo? Paging { get; set; }
}

/// <summary>
/// カテゴリ一覧項目用ビューモデル
/// </summary>
/// <remarks>
/// カテゴリ一覧での表示項目を定義
/// 階層構造の表現を含む
/// </remarks>
public class CategoryListItemViewModel
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
    /// 親カテゴリ名
    /// </summary>
    public string? ParentCategoryName { get; set; }

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
    /// 関連商品数（子孫カテゴリ含む）
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// 子カテゴリ数
    /// </summary>
    public int ChildCount { get; set; }

    /// <summary>
    /// 削除可能かどうか
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 子カテゴリが存在するかどうか
    /// </summary>
    public bool HasChildren { get; set; }

    /// <summary>
    /// 作成日時表示（フォーマット済み）
    /// </summary>
    public string FormattedCreatedAt => CreatedAt.ToString("yyyy/MM/dd HH:mm");

    /// <summary>
    /// インデント付きの表示名
    /// </summary>
    public string IndentedName => new string('　', Level) + Name;

    /// <summary>
    /// 階層レベル表示名
    /// </summary>
    public string LevelDisplayName => Level switch
    {
        0 => "ルート",
        1 => "サブ",
        2 => "詳細",
        _ => $"Level {Level}"
    };
}

/// <summary>
/// カテゴリ詳細画面用ビューモデル
/// </summary>
/// <remarks>
/// カテゴリ詳細表示と削除確認画面で使用
/// 関連商品の一覧も含む
/// </remarks>
public class CategoryDetailsViewModel
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
    /// 親カテゴリ名
    /// </summary>
    public string? ParentCategoryName { get; set; }

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
    /// 先祖カテゴリ一覧（パンくずリスト用）
    /// </summary>
    public IEnumerable<CategoryBreadcrumbItem> Breadcrumbs { get; set; } = new List<CategoryBreadcrumbItem>();

    /// <summary>
    /// 子カテゴリ一覧
    /// </summary>
    public IEnumerable<CategoryListItemViewModel> ChildCategories { get; set; } = new List<CategoryListItemViewModel>();

    /// <summary>
    /// 関連商品一覧
    /// </summary>
    public IEnumerable<ProductListItemViewModel> RelatedProducts { get; set; } = new List<ProductListItemViewModel>();

    /// <summary>
    /// 削除可能かどうか
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 関連商品数
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 作成日時表示（フォーマット済み）
    /// </summary>
    public string FormattedCreatedAt => CreatedAt.ToString("yyyy/MM/dd HH:mm:ss");

    /// <summary>
    /// 更新日時表示（フォーマット済み）
    /// </summary>
    public string FormattedUpdatedAt => UpdatedAt.ToString("yyyy/MM/dd HH:mm:ss");

    /// <summary>
    /// 階層レベル表示名
    /// </summary>
    public string LevelDisplayName => Level switch
    {
        0 => "ルートカテゴリ",
        1 => "サブカテゴリ",
        2 => "詳細カテゴリ",
        _ => $"Level {Level} カテゴリ"
    };

    /// <summary>
    /// 親カテゴリ情報
    /// </summary>
    public CategoryListItemViewModel? ParentCategory { get; set; }

    /// <summary>
    /// 兄弟カテゴリ一覧
    /// </summary>
    public IEnumerable<CategoryListItemViewModel> SiblingCategories { get; set; } = new List<CategoryListItemViewModel>();

    /// <summary>
    /// 関連商品一覧（RelatedProductsのエイリアス）
    /// </summary>
    public IEnumerable<ProductListItemViewModel> Products => RelatedProducts;
}

/// <summary>
/// カテゴリ登録画面用ビューモデル
/// </summary>
/// <remarks>
/// 新規カテゴリ登録フォームで使用
/// 親カテゴリ選択と階層レベル制御
/// </remarks>
public class CategoryCreateViewModel
{
    /// <summary>
    /// カテゴリ名
    /// </summary>
    [Required(ErrorMessage = "カテゴリ名は必須です")]
    [Display(Name = "カテゴリ名")]
    [StringLength(50, ErrorMessage = "カテゴリ名は50文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// カテゴリ説明
    /// </summary>
    [Display(Name = "カテゴリ説明")]
    [StringLength(500, ErrorMessage = "カテゴリ説明は500文字以内で入力してください")]
    public string? Description { get; set; }

    /// <summary>
    /// 親カテゴリID
    /// </summary>
    [Display(Name = "親カテゴリ")]
    public int? ParentCategoryId { get; set; }

    /// <summary>
    /// 表示順序
    /// </summary>
    [Required(ErrorMessage = "表示順序は必須です")]
    [Display(Name = "表示順序")]
    [Range(0, int.MaxValue, ErrorMessage = "表示順序は0以上の値を入力してください")]
    public int SortOrder { get; set; } = 1;

    /// <summary>
    /// 親カテゴリ選択用リスト
    /// </summary>
    public IEnumerable<CategorySelectItem> ParentCategories { get; set; } = new List<CategorySelectItem>();

    /// <summary>
    /// 自動計算される階層レベル（表示用）
    /// </summary>
    public int CalculatedLevel { get; set; }
}

/// <summary>
/// カテゴリ編集画面用ビューモデル
/// </summary>
/// <remarks>
/// 既存カテゴリの編集フォームで使用
/// 循環参照チェック機能を含む
/// </remarks>
public class CategoryEditViewModel
{
    /// <summary>
    /// カテゴリID
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// カテゴリ名
    /// </summary>
    [Required(ErrorMessage = "カテゴリ名は必須です")]
    [Display(Name = "カテゴリ名")]
    [StringLength(50, ErrorMessage = "カテゴリ名は50文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// カテゴリ説明
    /// </summary>
    [Display(Name = "カテゴリ説明")]
    [StringLength(500, ErrorMessage = "カテゴリ説明は500文字以内で入力してください")]
    public string? Description { get; set; }

    /// <summary>
    /// 親カテゴリID
    /// </summary>
    [Display(Name = "親カテゴリ")]
    public int? ParentCategoryId { get; set; }

    /// <summary>
    /// 表示順序
    /// </summary>
    [Required(ErrorMessage = "表示順序は必須です")]
    [Display(Name = "表示順序")]
    [Range(0, int.MaxValue, ErrorMessage = "表示順序は0以上の値を入力してください")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 現在の階層レベル
    /// </summary>
    public int CurrentLevel { get; set; }

    /// <summary>
    /// 階層レベル（Level プロパティのエイリアス）
    /// </summary>
    public int Level => CurrentLevel;

    /// <summary>
    /// 親カテゴリ選択用リスト（自分自身と子孫は除外）
    /// </summary>
    public IEnumerable<CategorySelectItem> ParentCategories { get; set; } = new List<CategorySelectItem>();

    /// <summary>
    /// 子カテゴリ数
    /// </summary>
    public int ChildCount { get; set; }

    /// <summary>
    /// 関連商品数
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 子カテゴリ一覧
    /// </summary>
    public IEnumerable<CategoryListItemViewModel> ChildCategories { get; set; } = new List<CategoryListItemViewModel>();

    /// <summary>
    /// 完全パス
    /// </summary>
    public string FullPath { get; set; } = string.Empty;
}

/// <summary>
/// パンくずリスト項目
/// </summary>
/// <remarks>
/// カテゴリの階層ナビゲーション用
/// </remarks>
public class CategoryBreadcrumbItem
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

/// <summary>
/// カテゴリ階層ツリー表示用ビューモデル
/// </summary>
/// <remarks>
/// カテゴリ選択UIやサイトマップで使用
/// </remarks>
public class CategoryTreeViewModel
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
    public IEnumerable<CategoryTreeViewModel> Children { get; set; } = new List<CategoryTreeViewModel>();

    /// <summary>
    /// 展開状態
    /// </summary>
    public bool IsExpanded { get; set; } = false;

    /// <summary>
    /// 選択状態
    /// </summary>
    public bool IsSelected { get; set; } = false;

    /// <summary>
    /// 子カテゴリを持つかどうか
    /// </summary>
    public bool HasChildren => Children.Any();
}

/// <summary>
/// カテゴリ削除用ビューモデル
/// </summary>
/// <remarks>
/// カテゴリ削除確認画面で使用
/// </remarks>
public class CategoryDeleteViewModel
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
    /// 親カテゴリ名
    /// </summary>
    public string? ParentCategoryName { get; set; }

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
    /// 削除可能かどうか
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 作成日時表示（フォーマット済み）
    /// </summary>
    public string FormattedCreatedAt => CreatedAt.ToString("yyyy/MM/dd HH:mm:ss");

    /// <summary>
    /// 更新日時表示（フォーマット済み）
    /// </summary>
    public string FormattedUpdatedAt => UpdatedAt.ToString("yyyy/MM/dd HH:mm:ss");

    /// <summary>
    /// 階層レベル表示名
    /// </summary>
    public string LevelDisplayName => Level switch
    {
        0 => "ルートカテゴリ",
        1 => "サブカテゴリ",
        2 => "詳細カテゴリ",
        _ => $"Level {Level} カテゴリ"
    };
}
/// <summary>
/// ページング情報
/// </summary>
/// <remarks>
/// Ajax通信とMVC画面の両方で使用
/// </remarks>
public class PagingInfo
{
    /// <summary>
    /// 現在のページ番号
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// 1ページあたりの表示件数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 総件数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 総ページ数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 次のページが存在するかどうか
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// 前のページが存在するかどうか
    /// </summary>
    public bool HasPreviousPage { get; set; }
}
