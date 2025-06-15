using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Web.Essentials.Domain.Entities;

namespace Web.Essentials.App.ViewModels;

/// <summary>
/// カテゴリ表示用ビューモデル
/// </summary>
/// <remarks>
/// セレクトボックスなどでカテゴリ表示に使用
/// </remarks>
public class CategoryViewModel
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
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 商品画像用ビューモデル
/// </summary>
/// <remarks>
/// 商品画像の表示・管理に使用
/// </remarks>
public class ProductImageViewModel
{
    /// <summary>
    /// 画像ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品ID
    /// </summary>
    public int ProductId { get; set; }

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
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成日時表示（フォーマット済み）
    /// </summary>
    public string FormattedCreatedAt => CreatedAt.ToString("yyyy/MM/dd HH:mm");
}

/// <summary>
/// 商品削除用ビューモデル
/// </summary>
/// <remarks>
/// 商品削除確認画面で使用
/// </remarks>
public class ProductDeleteViewModel
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
    /// 価格表示（フォーマット済み）
    /// </summary>
    public string FormattedPrice => $"¥{Price:N0}";

    /// <summary>
    /// JANコード
    /// </summary>
    public string? JanCode { get; set; }

    /// <summary>
    /// ステータス
    /// </summary>
    public ProductStatus Status { get; set; }

    /// <summary>
    /// カテゴリID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// ステータス表示名
    /// </summary>
    public string StatusName => Status switch
    {
        ProductStatus.PreSale => "販売開始前",
        ProductStatus.OnSale => "販売中",
        ProductStatus.Discontinued => "取扱終了",
        _ => "不明"
    };

    /// <summary>
    /// カテゴリ一覧
    /// </summary>
    public IEnumerable<CategoryDisplayItem> Categories { get; set; } = new List<CategoryDisplayItem>();

    /// <summary>
    /// 画像一覧
    /// </summary>
    public IEnumerable<ProductImageDisplayItem> Images { get; set; } = new List<ProductImageDisplayItem>();

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
}

/// <summary>
/// 商品一覧画面用ビューモデル
/// </summary>
/// <remarks>
/// Ajax検索機能付きの商品一覧表示に使用
/// 検索条件とページング情報を含む
/// </remarks>
public class ProductIndexViewModel
{
    /// <summary>
    /// 検索条件：商品名
    /// </summary>
    [Display(Name = "商品名")]
    [StringLength(100, ErrorMessage = "商品名は100文字以内で入力してください")]
    public string? NameTerm { get; set; }

    /// <summary>
    /// 検索条件：JANコード
    /// </summary>
    [Display(Name = "JANコード")]
    [StringLength(13, ErrorMessage = "JANコードは13文字以内で入力してください")]
    public string? JanCodeTerm { get; set; }

    /// <summary>
    /// 検索条件：カテゴリID
    /// </summary>
    [Display(Name = "カテゴリ")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// 検索条件：商品ステータス
    /// </summary>
    [Display(Name = "ステータス")]
    public ProductStatus? Status { get; set; }

    /// <summary>
    /// 検索条件：最低価格
    /// </summary>
    [Display(Name = "価格（最低）")]
    [Range(0, uint.MaxValue, ErrorMessage = "価格は0以上の値を入力してください")]
    public uint? MinPrice { get; set; }

    /// <summary>
    /// 検索条件：最高価格
    /// </summary>
    [Display(Name = "価格（最高）")]
    [Range(0, uint.MaxValue, ErrorMessage = "価格は0以上の値を入力してください")]
    public uint? MaxPrice { get; set; }

    /// <summary>
    /// ソート項目
    /// </summary>
    [Display(Name = "並び順")]
    public string SortBy { get; set; } = "createdAt";

    /// <summary>
    /// ソート順序
    /// </summary>
    public string SortOrder { get; set; } = "desc";

    /// <summary>
    /// 現在のページ番号
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 1ページあたりの表示件数
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 商品一覧（Ajax更新用・初期表示では空）
    /// </summary>
    public IEnumerable<ProductListItemViewModel> Products { get; set; } = new List<ProductListItemViewModel>();

    /// <summary>
    /// カテゴリ選択用リスト
    /// </summary>
    public IEnumerable<CategorySelectItem> Categories { get; set; } = new List<CategorySelectItem>();

    /// <summary>
    /// ページング情報
    /// </summary>
    public PagingInfo PagingInfo { get; set; } = new();

    /// <summary>
    /// ページング情報（別名）
    /// </summary>
    public PagingInfo Paging => PagingInfo;

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
}

/// <summary>
/// 商品一覧項目用ビューモデル
/// </summary>
/// <remarks>
/// 商品一覧での表示項目を定義
/// Ajax応答でも使用
/// </remarks>
public class ProductListItemViewModel
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
    /// 商品説明（フル版）
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 価格
    /// </summary>
    public uint Price { get; set; }

    /// <summary>
    /// 価格表示（フォーマット済み）
    /// </summary>
    public string FormattedPrice => $"¥{Price:N0}";

    /// <summary>
    /// JANコード
    /// </summary>
    public string? JanCode { get; set; }

    /// <summary>
    /// ステータス
    /// </summary>
    public ProductStatus Status { get; set; }

    /// <summary>
    /// ステータス表示名
    /// </summary>
    public string StatusName => Status switch
    {
        ProductStatus.PreSale => "販売開始前",
        ProductStatus.OnSale => "販売中",
        ProductStatus.Discontinued => "取扱終了",
        _ => "不明"
    };

    /// <summary>
    /// メイン画像パス
    /// </summary>
    public string? MainImagePath { get; set; }

    /// <summary>
    /// カテゴリ一覧
    /// </summary>
    public IEnumerable<string> CategoryNames { get; set; } = new List<string>();

    /// <summary>
    /// カテゴリ名（メイン）
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成日時表示（フォーマット済み）
    /// </summary>
    public string FormattedCreatedAt => CreatedAt.ToString("yyyy/MM/dd HH:mm");
}

/// <summary>
/// 商品詳細画面用ビューモデル
/// </summary>
/// <remarks>
/// 商品詳細表示と削除確認画面で使用
/// </remarks>
public class ProductDetailsViewModel
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
    /// 価格表示（フォーマット済み）
    /// </summary>
    public string FormattedPrice => $"¥{Price:N0}";

    /// <summary>
    /// JANコード
    /// </summary>
    public string? JanCode { get; set; }

    /// <summary>
    /// ステータス
    /// </summary>
    public ProductStatus Status { get; set; }

    /// <summary>
    /// カテゴリID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// カテゴリ名
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// ステータス表示名
    /// </summary>
    public string StatusName => Status switch
    {
        ProductStatus.PreSale => "販売開始前",
        ProductStatus.OnSale => "販売中",
        ProductStatus.Discontinued => "取扱終了",
        _ => "不明"
    };

    /// <summary>
    /// カテゴリ一覧
    /// </summary>
    public IEnumerable<CategoryDisplayItem> Categories { get; set; } = new List<CategoryDisplayItem>();

    /// <summary>
    /// 画像一覧
    /// </summary>
    public IEnumerable<ProductImageDisplayItem> Images { get; set; } = new List<ProductImageDisplayItem>();

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
}

/// <summary>
/// 商品登録画面用ビューモデル
/// </summary>
/// <remarks>
/// 新規商品登録フォームで使用
/// バリデーション属性とファイルアップロード対応
/// </remarks>
public class ProductCreateViewModel
{
    /// <summary>
    /// 商品名
    /// </summary>
    [Required(ErrorMessage = "商品名は必須です")]
    [Display(Name = "商品名")]
    [StringLength(100, ErrorMessage = "商品名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品説明
    /// </summary>
    [Display(Name = "商品説明")]
    [StringLength(1000, ErrorMessage = "商品説明は1000文字以内で入力してください")]
    public string? Description { get; set; }

    /// <summary>
    /// 価格
    /// </summary>
    [Required(ErrorMessage = "価格は必須です")]
    [Display(Name = "価格")]
    [Range(0, uint.MaxValue, ErrorMessage = "価格は0以上の値を入力してください")]
    public uint Price { get; set; }

    /// <summary>
    /// JANコード
    /// </summary>
    [Display(Name = "JANコード")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "JANコードは13桁で入力してください")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "JANコードは13桁の数字で入力してください")]
    public string? JanCode { get; set; }

    /// <summary>
    /// ステータス
    /// </summary>
    [Required(ErrorMessage = "ステータスは必須です")]
    [Display(Name = "ステータス")]
    public ProductStatus Status { get; set; } = ProductStatus.PreSale;

    /// <summary>
    /// カテゴリID
    /// </summary>
    [Required(ErrorMessage = "カテゴリは必須です")]
    [Display(Name = "カテゴリ")]
    public int CategoryId { get; set; }

    /// <summary>
    /// 選択されたカテゴリIDリスト
    /// </summary>
    [Required(ErrorMessage = "カテゴリは1つ以上選択してください")]
    [Display(Name = "カテゴリ")]
    public List<int> SelectedCategoryIds { get; set; } = new();

    /// <summary>
    /// アップロード画像ファイル（最大5枚）
    /// </summary>
    [Display(Name = "商品画像")]
    public List<IFormFile>? ImageFiles { get; set; }

    /// <summary>
    /// 画像の代替テキスト
    /// </summary>
    [Display(Name = "画像の説明")]
    public List<string>? ImageAltTexts { get; set; }

    /// <summary>
    /// メイン画像のインデックス（0から開始）
    /// </summary>
    [Display(Name = "メイン画像")]
    public int MainImageIndex { get; set; } = 0;

    /// <summary>
    /// カテゴリ選択用リスト
    /// </summary>
    public IEnumerable<CategorySelectItem> Categories { get; set; } = new List<CategorySelectItem>();
}

/// <summary>
/// 商品編集画面用ビューモデル
/// </summary>
/// <remarks>
/// 既存商品の編集フォームで使用
/// 現在の値をプリセット
/// </remarks>
public class ProductEditViewModel
{
    /// <summary>
    /// 商品ID
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// 商品名
    /// </summary>
    [Required(ErrorMessage = "商品名は必須です")]
    [Display(Name = "商品名")]
    [StringLength(100, ErrorMessage = "商品名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 商品説明
    /// </summary>
    [Display(Name = "商品説明")]
    [StringLength(1000, ErrorMessage = "商品説明は1000文字以内で入力してください")]
    public string? Description { get; set; }

    /// <summary>
    /// 価格
    /// </summary>
    [Required(ErrorMessage = "価格は必須です")]
    [Display(Name = "価格")]
    [Range(0, uint.MaxValue, ErrorMessage = "価格は0以上の値を入力してください")]
    public uint Price { get; set; }

    /// <summary>
    /// JANコード
    /// </summary>
    [Display(Name = "JANコード")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "JANコードは13桁で入力してください")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "JANコードは13桁の数字で入力してください")]
    public string? JanCode { get; set; }

    /// <summary>
    /// ステータス
    /// </summary>
    [Required(ErrorMessage = "ステータスは必須です")]
    [Display(Name = "ステータス")]
    public ProductStatus Status { get; set; }

    /// <summary>
    /// カテゴリID
    /// </summary>
    [Required(ErrorMessage = "カテゴリは必須です")]
    [Display(Name = "カテゴリ")]
    public int CategoryId { get; set; }

    /// <summary>
    /// 選択されたカテゴリIDリスト
    /// </summary>
    [Required(ErrorMessage = "カテゴリは1つ以上選択してください")]
    [Display(Name = "カテゴリ")]
    public List<int> SelectedCategoryIds { get; set; } = new();

    /// <summary>
    /// 新規アップロード画像ファイル
    /// </summary>
    [Display(Name = "追加画像")]
    public List<IFormFile>? NewImageFiles { get; set; }

    /// <summary>
    /// 新規画像の代替テキスト
    /// </summary>
    [Display(Name = "画像の説明")]
    public List<string>? NewImageAltTexts { get; set; }

    /// <summary>
    /// 削除対象の画像IDリスト
    /// </summary>
    public List<int> DeleteImageIds { get; set; } = new();

    /// <summary>
    /// メイン画像ID
    /// </summary>
    [Display(Name = "メイン画像")]
    public int? MainImageId { get; set; }

    /// <summary>
    /// 既存画像一覧
    /// </summary>
    public IEnumerable<ProductImageViewModel> ExistingImages { get; set; } = new List<ProductImageViewModel>();

    /// <summary>
    /// 現在の画像一覧
    /// </summary>
    public IEnumerable<ProductImageDisplayItem> CurrentImages { get; set; } = new List<ProductImageDisplayItem>();

    /// <summary>
    /// カテゴリ選択用リスト
    /// </summary>
    public IEnumerable<CategorySelectItem> Categories { get; set; } = new List<CategorySelectItem>();

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// カテゴリ選択項目
/// </summary>
/// <remarks>
/// カテゴリ選択ドロップダウンで使用
/// </remarks>
public class CategorySelectItem
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
    /// 完全パス（階層構造を含む）
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
/// カテゴリ表示項目
/// </summary>
/// <remarks>
/// 商品詳細画面でのカテゴリ表示用
/// </remarks>
public class CategoryDisplayItem
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
}

/// <summary>
/// 商品画像表示項目
/// </summary>
/// <remarks>
/// 商品詳細画面での画像表示用
/// </remarks>
public class ProductImageDisplayItem
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
/// ページング情報
/// </summary>
/// <remarks>
/// 一覧画面でのページング制御用
/// </remarks>
public class PagingInfo
{
    /// <summary>
    /// 現在のページ番号
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// 1ページあたりの件数
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 総件数
    /// </summary>
    public int TotalCount { get; set; }

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
    /// 表示開始件数
    /// </summary>
    public int StartIndex => (CurrentPage - 1) * PageSize + 1;

    /// <summary>
    /// 表示終了件数
    /// </summary>
    public int EndIndex => Math.Min(CurrentPage * PageSize, TotalCount);
}