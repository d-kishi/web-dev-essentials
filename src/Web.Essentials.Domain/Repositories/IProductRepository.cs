using Web.Essentials.Domain.Entities;

namespace Web.Essentials.Domain.Repositories;

/// <summary>
/// 商品リポジトリインターフェース - 商品エンティティのデータアクセス抽象化
/// </summary>
/// <remarks>
/// このインターフェースは商品データの永続化に関する操作を定義し、以下の特徴を持つ：
/// - 基本的なCRUD操作の提供
/// - 検索・フィルタリング機能
/// - ページング対応
/// - 非同期処理対応
/// - カテゴリとの関連を含む検索機能
/// 
/// 依存関係逆転の原則により、Domain層でインターフェースを定義し、
/// Infrastructure層で具象実装を提供する
/// </remarks>
public interface IProductRepository
{
    /// <summary>
    /// 商品IDによる単一商品取得
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="includeRelations">関連エンティティ（カテゴリ、画像）を含むかどうか</param>
    /// <returns>商品エンティティ、存在しない場合はnull</returns>
    /// <remarks>
    /// includeRelations が true の場合、ProductCategories および ProductImages も一緒に取得される
    /// パフォーマンスを考慮し、必要に応じて関連データの取得を制御
    /// </remarks>
    Task<Product?> GetByIdAsync(int id, bool includeRelations = false);

    /// <summary>
    /// JANコードによる商品取得
    /// </summary>
    /// <param name="janCode">JANコード（13桁）</param>
    /// <returns>商品エンティティ、存在しない場合はnull</returns>
    /// <remarks>
    /// JANコードの重複チェックや商品検索に使用
    /// JANコードはユニーク制約があるため、結果は1件または0件
    /// </remarks>
    Task<Product?> GetByJanCodeAsync(string janCode);

    /// <summary>
    /// 商品一覧取得（検索・フィルタリング・ページング対応）
    /// </summary>
    /// <param name="nameTerm">商品名での部分一致検索（null可）</param>
    /// <param name="janCodeTerm">JANコードでの部分一致検索（null可）</param>
    /// <param name="categoryId">カテゴリIDでの絞り込み（null可、階層検索対応）</param>
    /// <param name="status">商品ステータスでの絞り込み（null可）</param>
    /// <param name="minPrice">最低価格（null可）</param>
    /// <param name="maxPrice">最高価格（null可）</param>
    /// <param name="sortBy">ソート項目（name, price, createdAt）</param>
    /// <param name="sortOrder">ソート順（asc, desc）</param>
    /// <param name="page">ページ番号（1から開始）</param>
    /// <param name="pageSize">1ページあたりの件数</param>
    /// <returns>検索条件に一致する商品一覧とページング情報</returns>
    /// <remarks>
    /// 複合検索条件に対応した柔軟な検索機能
    /// categoryId指定時は、指定されたカテゴリの子孫カテゴリに属する商品も含む
    /// ソート機能により、様々な順序での商品表示が可能
    /// </remarks>
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
        string? nameTerm = null,
        string? janCodeTerm = null,
        int? categoryId = null,
        ProductStatus? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string sortBy = "createdAt",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = 10);

    /// <summary>
    /// 指定カテゴリに属する商品一覧取得
    /// </summary>
    /// <param name="categoryId">カテゴリID</param>
    /// <param name="includeDescendants">子孫カテゴリの商品も含むかどうか</param>
    /// <returns>カテゴリに属する商品一覧</returns>
    /// <remarks>
    /// カテゴリ詳細画面での商品一覧表示に使用
    /// includeDescendants が true の場合、階層下位のカテゴリに属する商品も含む
    /// </remarks>
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, bool includeDescendants = false);

    /// <summary>
    /// 最新商品取得
    /// </summary>
    /// <param name="count">取得件数</param>
    /// <returns>作成日時順での最新商品一覧</returns>
    /// <remarks>
    /// ホーム画面での新着商品表示に使用
    /// 販売中ステータスの商品のみを対象とする
    /// </remarks>
    Task<IEnumerable<Product>> GetLatestAsync(int count = 10);

    /// <summary>
    /// 商品追加
    /// </summary>
    /// <param name="product">追加する商品エンティティ</param>
    /// <returns>追加された商品エンティティ（IDが設定済み）</returns>
    /// <remarks>
    /// 作成日時・更新日時は自動設定される
    /// JANコードの重複チェックは事前に実施すること
    /// </remarks>
    Task<Product> AddAsync(Product product);

    /// <summary>
    /// 商品更新
    /// </summary>
    /// <param name="product">更新する商品エンティティ</param>
    /// <returns>更新された商品エンティティ</returns>
    /// <remarks>
    /// 更新日時は自動更新される
    /// 楽観的排他制御やバージョン管理は上位層で実装
    /// </remarks>
    Task<Product> UpdateAsync(Product product);

    /// <summary>
    /// 商品削除
    /// </summary>
    /// <param name="id">削除する商品ID</param>
    /// <returns>削除の成功可否</returns>
    /// <remarks>
    /// 関連する ProductCategory および ProductImage も自動削除される（CASCADE DELETE）
    /// 物理削除を実行するため、論理削除が必要な場合は Status を変更すること
    /// </remarks>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 商品存在確認
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>存在する場合true、しない場合false</returns>
    /// <remarks>
    /// 削除前の存在確認や参照整合性チェックに使用
    /// </remarks>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// JANコードの重複確認
    /// </summary>
    /// <param name="janCode">JANコード</param>
    /// <param name="excludeProductId">確認対象から除外する商品ID（更新時に使用）</param>
    /// <returns>重複する場合true、しない場合false</returns>
    /// <remarks>
    /// 商品登録・更新時のバリデーションに使用
    /// excludeProductId により、更新時の自己重複を除外可能
    /// </remarks>
    Task<bool> IsJanCodeDuplicateAsync(string janCode, int? excludeProductId = null);

    /// <summary>
    /// 商品統計情報取得
    /// </summary>
    /// <returns>商品の統計情報（総数、ステータス別件数など）</returns>
    /// <remarks>
    /// ダッシュボードやレポート機能で使用
    /// パフォーマンスを考慮した集計クエリを実装
    /// </remarks>
    Task<ProductStatistics> GetStatisticsAsync();
}

/// <summary>
/// 商品統計情報
/// </summary>
/// <remarks>
/// 商品の統計データを格納するための値オブジェクト
/// </remarks>
public class ProductStatistics
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
}