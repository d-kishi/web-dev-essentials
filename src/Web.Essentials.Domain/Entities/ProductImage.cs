namespace Web.Essentials.Domain.Entities;

/// <summary>
/// 商品画像エンティティ - 商品に関連付けられる画像情報を管理するドメインエンティティ
/// </summary>
/// <remarks>
/// このクラスは商品画像の情報を管理し、以下の特徴を持つ：
/// - 1つの商品に対して最大5枚の画像を管理
/// - メイン画像とサブ画像の区別
/// - 表示順序による画像の並び順制御
/// - アクセシビリティ対応の代替テキスト
/// - ファイルパスによる画像ファイルの管理
/// </remarks>
public class ProductImage
{
    /// <summary>
    /// 画像ID（主キー）
    /// </summary>
    /// <remarks>
    /// データベースで自動採番される画像の一意識別子
    /// </remarks>
    public int Id { get; set; }

    /// <summary>
    /// 商品ID（外部キー）
    /// </summary>
    /// <remarks>
    /// Product エンティティへの外部キー
    /// この画像が属する商品を特定
    /// </remarks>
    public int ProductId { get; set; }

    /// <summary>
    /// 画像ファイルパス
    /// </summary>
    /// <remarks>
    /// 必須項目、最大500文字
    /// Webアプリケーションの wwwroot からの相対パス
    /// 例: "/uploads/products/product_1_main.jpg"
    /// </remarks>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 表示順序
    /// </summary>
    /// <remarks>
    /// 必須項目、1-5の整数値
    /// 同一商品内での画像の表示順序を制御
    /// 1が最初、5が最後
    /// 同一商品で重複不可
    /// </remarks>
    public int DisplayOrder { get; set; } = 1;

    /// <summary>
    /// 代替テキスト
    /// </summary>
    /// <remarks>
    /// 任意項目、最大200文字
    /// HTMLのalt属性として使用
    /// アクセシビリティ対応のため画像の内容を説明
    /// </remarks>
    public string? AltText { get; set; }

    /// <summary>
    /// メイン画像フラグ
    /// </summary>
    /// <remarks>
    /// 必須項目、ブール値
    /// 商品一覧などで代表として表示される画像を特定
    /// 1つの商品につき最大1つのメイン画像のみ許可
    /// </remarks>
    public bool IsMain { get; set; } = false;

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
    /// 関連する商品（ナビゲーションプロパティ）
    /// </summary>
    /// <remarks>
    /// ProductId で参照される Product エンティティ
    /// Entity Framework による遅延読み込み対応
    /// </remarks>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// ファイル拡張子を取得
    /// </summary>
    /// <returns>ファイル拡張子（ピリオド付き、例: ".jpg"）</returns>
    /// <remarks>
    /// 画像形式の判定や処理に使用
    /// ImagePath からファイル拡張子を抽出
    /// </remarks>
    public string GetFileExtension()
    {
        return Path.GetExtension(ImagePath);
    }

    /// <summary>
    /// ファイル名（拡張子なし）を取得
    /// </summary>
    /// <returns>拡張子を除いたファイル名</returns>
    /// <remarks>
    /// ファイル名の処理や表示に使用
    /// </remarks>
    public string GetFileNameWithoutExtension()
    {
        return Path.GetFileNameWithoutExtension(ImagePath);
    }

    /// <summary>
    /// 画像の種類を判定
    /// </summary>
    /// <returns>サポートされている画像形式の場合true</returns>
    /// <remarks>
    /// サポート対象: .jpg, .jpeg, .png, .gif
    /// アップロード時のバリデーションに使用
    /// </remarks>
    public bool IsSupportedImageFormat()
    {
        var extension = GetFileExtension().ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif";
    }
}