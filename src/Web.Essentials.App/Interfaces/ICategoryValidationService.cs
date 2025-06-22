namespace Web.Essentials.App.Interfaces;

/// <summary>
/// カテゴリバリデーション専用サービスのインターフェース
/// 単一責任の原則に基づき、カテゴリのバリデーション処理のみを定義
/// </summary>
public interface ICategoryValidationService
{
    /// <summary>
    /// カテゴリ名が重複しているかどうかをチェックする
    /// </summary>
    /// <param name="name">チェック対象のカテゴリ名</param>
    /// <param name="excludeId">除外するカテゴリID（編集時に現在のカテゴリを除外する場合）</param>
    /// <returns>重複している場合はtrue、重複していない場合はfalse</returns>
    /// <exception cref="Exception">データベースアクセスエラーが発生した場合</exception>
    Task<bool> IsCategoryNameDuplicateAsync(string name, int? excludeId = null);

    /// <summary>
    /// カテゴリが削除可能かどうかをチェックする
    /// 子カテゴリまたは関連商品が存在する場合は削除不可
    /// </summary>
    /// <param name="categoryId">チェック対象のカテゴリID</param>
    /// <returns>削除可能な場合はtrue、削除不可の場合はfalse</returns>
    /// <exception cref="Exception">データベースアクセスエラーが発生した場合</exception>
    Task<bool> CanDeleteCategoryAsync(int categoryId);

    /// <summary>
    /// カテゴリの階層構造が有効かどうかをバリデーションする
    /// 3階層制限、循環参照、親カテゴリの存在確認を行う
    /// </summary>
    /// <param name="parentCategoryId">親カテゴリID（nullの場合はルートカテゴリ）</param>
    /// <param name="currentCategoryId">現在のカテゴリID（編集時のみ指定）</param>
    /// <returns>バリデーション結果（成功/失敗とエラーメッセージリスト）</returns>
    /// <exception cref="Exception">データベースアクセスエラーが発生した場合</exception>
    Task<(bool IsValid, List<string> ErrorMessages)> ValidateCategoryHierarchyAsync(int? parentCategoryId, int? currentCategoryId = null);
}