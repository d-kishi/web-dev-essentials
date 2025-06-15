using Web.Essentials.Domain.Entities;

namespace Web.Essentials.Domain.Repositories;

/// <summary>
/// 商品画像リポジトリインターフェース - 商品画像エンティティのデータアクセス抽象化
/// </summary>
/// <remarks>
/// このインターフェースは商品画像データの永続化に関する操作を定義し、以下の特徴を持つ：
/// - 商品に関連付けられた画像管理
/// - 表示順序の制御
/// - メイン画像の一意性保証
/// - ファイル操作との連携
/// - 非同期処理対応
/// 
/// 1つの商品に最大5枚の画像を管理することを前提とした設計
/// </remarks>
public interface IProductImageRepository
{
    /// <summary>
    /// 画像IDによる単一画像取得
    /// </summary>
    /// <param name="id">画像ID</param>
    /// <param name="includeProduct">関連商品情報を含むかどうか</param>
    /// <returns>商品画像エンティティ、存在しない場合はnull</returns>
    /// <remarks>
    /// 画像詳細表示や編集画面での使用を想定
    /// </remarks>
    Task<ProductImage?> GetByIdAsync(int id, bool includeProduct = false);

    /// <summary>
    /// 指定商品の全画像取得
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="orderByDisplayOrder">表示順序でソートするかどうか</param>
    /// <returns>商品に関連付けられた画像一覧</returns>
    /// <remarks>
    /// 商品詳細画面での画像表示に使用
    /// orderByDisplayOrder が true の場合、DisplayOrder 昇順で返却
    /// </remarks>
    Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId, bool orderByDisplayOrder = true);

    /// <summary>
    /// 指定商品のメイン画像取得
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>メイン画像、存在しない場合はnull</returns>
    /// <remarks>
    /// 商品一覧での代表画像表示に使用
    /// IsMain が true の画像を取得
    /// </remarks>
    Task<ProductImage?> GetMainImageByProductIdAsync(int productId);

    /// <summary>
    /// 複数商品のメイン画像を一括取得
    /// </summary>
    /// <param name="productIds">商品ID一覧</param>
    /// <returns>商品IDをキーとした画像の辞書</returns>
    /// <remarks>
    /// 商品一覧画面での効率的な画像表示に使用
    /// N+1問題を避けるための一括取得機能
    /// </remarks>
    Task<Dictionary<int, ProductImage?>> GetMainImagesByProductIdsAsync(IEnumerable<int> productIds);

    /// <summary>
    /// 画像ファイルパスによる検索
    /// </summary>
    /// <param name="imagePath">画像ファイルパス</param>
    /// <returns>商品画像エンティティ、存在しない場合はnull</returns>
    /// <remarks>
    /// ファイル整合性チェックや重複確認に使用
    /// </remarks>
    Task<ProductImage?> GetByImagePathAsync(string imagePath);

    /// <summary>
    /// 商品画像追加
    /// </summary>
    /// <param name="productImage">追加する商品画像エンティティ</param>
    /// <returns>追加された商品画像エンティティ（IDが設定済み）</returns>
    /// <remarks>
    /// 表示順序の重複チェックとメイン画像の一意性チェックは事前に実施すること
    /// 作成日時・更新日時は自動設定される
    /// </remarks>
    Task<ProductImage> AddAsync(ProductImage productImage);

    /// <summary>
    /// 商品画像更新
    /// </summary>
    /// <param name="productImage">更新する商品画像エンティティ</param>
    /// <returns>更新された商品画像エンティティ</returns>
    /// <remarks>
    /// 更新日時は自動更新される
    /// メイン画像変更時の整合性チェックは事前に実施すること
    /// </remarks>
    Task<ProductImage> UpdateAsync(ProductImage productImage);

    /// <summary>
    /// 商品画像削除
    /// </summary>
    /// <param name="id">削除する画像ID</param>
    /// <returns>削除の成功可否</returns>
    /// <remarks>
    /// 実際のファイル削除は上位層で実施すること
    /// 表示順序の再調整は DeleteByProductIdAsync を使用することを推奨
    /// </remarks>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 指定商品の全画像削除
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>削除の成功可否</returns>
    /// <remarks>
    /// 商品削除時の関連画像一括削除に使用
    /// 実際のファイル削除は上位層で実施すること
    /// </remarks>
    Task<bool> DeleteByProductIdAsync(int productId);

    /// <summary>
    /// 画像存在確認
    /// </summary>
    /// <param name="id">画像ID</param>
    /// <returns>存在する場合true、しない場合false</returns>
    /// <remarks>
    /// 削除前の存在確認や参照整合性チェックに使用
    /// </remarks>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// 指定商品での表示順序重複確認
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="displayOrder">表示順序</param>
    /// <param name="excludeImageId">確認対象から除外する画像ID（更新時に使用）</param>
    /// <returns>重複する場合true、しない場合false</returns>
    /// <remarks>
    /// 画像登録・更新時のバリデーションに使用
    /// 同一商品内で表示順序は重複不可
    /// </remarks>
    Task<bool> IsDisplayOrderDuplicateAsync(int productId, int displayOrder, int? excludeImageId = null);

    /// <summary>
    /// 指定商品のメイン画像設定確認
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="excludeImageId">確認対象から除外する画像ID（更新時に使用）</param>
    /// <returns>メイン画像が設定済みの場合true、未設定の場合false</returns>
    /// <remarks>
    /// メイン画像設定時のバリデーションに使用
    /// 1つの商品に対してメイン画像は1つまで
    /// </remarks>
    Task<bool> HasMainImageAsync(int productId, int? excludeImageId = null);

    /// <summary>
    /// 指定商品の画像数取得
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>画像数</returns>
    /// <remarks>
    /// 画像追加時の上限チェック（最大5枚）に使用
    /// </remarks>
    Task<int> GetImageCountByProductIdAsync(int productId);

    /// <summary>
    /// 指定商品での使用可能な表示順序取得
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>使用可能な表示順序一覧（1-5の範囲）</returns>
    /// <remarks>
    /// 新規画像追加時の表示順序選択肢提供に使用
    /// </remarks>
    Task<IEnumerable<int>> GetAvailableDisplayOrdersAsync(int productId);

    /// <summary>
    /// 指定商品の表示順序を再調整
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>再調整の成功可否</returns>
    /// <remarks>
    /// 画像削除後の表示順序の歯抜けを解消
    /// 1から連番になるよう自動調整
    /// </remarks>
    Task<bool> ReorderDisplayOrderAsync(int productId);

    /// <summary>
    /// メイン画像の変更
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="newMainImageId">新しいメイン画像ID</param>
    /// <returns>変更の成功可否</returns>
    /// <remarks>
    /// 既存のメイン画像を非メイン画像に変更し、指定画像をメイン画像に設定
    /// トランザクション内での実行を推奨
    /// </remarks>
    Task<bool> ChangeMainImageAsync(int productId, int newMainImageId);

    /// <summary>
    /// 孤立画像ファイルの検出
    /// </summary>
    /// <param name="existingFilePaths">実際に存在するファイルパス一覧</param>
    /// <returns>データベースに記録されているが実ファイルが存在しない画像一覧</returns>
    /// <remarks>
    /// ファイル整合性チェックやクリーンアップ処理に使用
    /// </remarks>
    Task<IEnumerable<ProductImage>> GetOrphanedImagesAsync(IEnumerable<string> existingFilePaths);

    /// <summary>
    /// 未使用ファイルの検出
    /// </summary>
    /// <param name="allFilePaths">実際に存在するファイルパス一覧</param>
    /// <returns>実ファイルは存在するがデータベースに記録されていないファイルパス一覧</returns>
    /// <remarks>
    /// ファイル整合性チェックやクリーンアップ処理に使用
    /// </remarks>
    Task<IEnumerable<string>> GetUnusedFilePathsAsync(IEnumerable<string> allFilePaths);
}