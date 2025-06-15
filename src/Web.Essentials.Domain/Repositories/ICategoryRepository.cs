using Web.Essentials.Domain.Entities;

namespace Web.Essentials.Domain.Repositories;

/// <summary>
/// カテゴリリポジトリインターフェース - カテゴリエンティティのデータアクセス抽象化
/// </summary>
/// <remarks>
/// このインターフェースはカテゴリデータの永続化に関する操作を定義し、以下の特徴を持つ：
/// - 階層構造に対応したCRUD操作
/// - 階層レベル別の検索機能
/// - 親子関係の整合性チェック
/// - 非同期処理対応
/// - 商品数の集計機能
/// 
/// 3階層までの階層構造を前提とした設計
/// </remarks>
public interface ICategoryRepository
{
    /// <summary>
    /// カテゴリIDによる単一カテゴリ取得
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <param name="includeRelations">関連エンティティ（親、子、商品）を含むかどうか</param>
    /// <returns>カテゴリエンティティ、存在しない場合はnull</returns>
    /// <remarks>
    /// includeRelations が true の場合、ParentCategory、ChildCategories、ProductCategories も取得
    /// 階層表示や詳細画面での関連情報表示に使用
    /// </remarks>
    Task<Category?> GetByIdAsync(int id, bool includeRelations = false);

    /// <summary>
    /// カテゴリ名による検索
    /// </summary>
    /// <param name="name">カテゴリ名（完全一致）</param>
    /// <returns>カテゴリエンティティ、存在しない場合はnull</returns>
    /// <remarks>
    /// カテゴリ名の重複チェックに使用
    /// カテゴリ名はシステム全体でユニーク制約
    /// </remarks>
    Task<Category?> GetByNameAsync(string name);

    /// <summary>
    /// 全カテゴリ取得（階層構造・検索・フィルタリング対応）
    /// </summary>
    /// <param name="level">階層レベルでの絞り込み（null可）</param>
    /// <param name="parentId">親カテゴリIDでの絞り込み（null可）</param>
    /// <param name="nameTerm">カテゴリ名での部分一致検索（null可）</param>
    /// <param name="includeProductCount">関連商品数を含むかどうか</param>
    /// <returns>検索条件に一致するカテゴリ一覧</returns>
    /// <remarks>
    /// 階層構造を意識した検索機能
    /// SortOrder による並び順で返却される
    /// includeProductCount が true の場合、子孫カテゴリの商品数も含む
    /// </remarks>
    Task<IEnumerable<Category>> GetAllAsync(
        int? level = null,
        int? parentId = null,
        string? nameTerm = null,
        bool includeProductCount = false);

    /// <summary>
    /// ルートカテゴリ一覧取得
    /// </summary>
    /// <returns>階層レベル0のカテゴリ一覧</returns>
    /// <remarks>
    /// トップレベルカテゴリの表示に使用
    /// SortOrder による並び順で返却
    /// </remarks>
    Task<IEnumerable<Category>> GetRootCategoriesAsync();

    /// <summary>
    /// 指定カテゴリの子カテゴリ一覧取得
    /// </summary>
    /// <param name="parentId">親カテゴリID</param>
    /// <returns>子カテゴリ一覧</returns>
    /// <remarks>
    /// 階層ナビゲーションや子カテゴリ選択に使用
    /// SortOrder による並び順で返却
    /// </remarks>
    Task<IEnumerable<Category>> GetChildCategoriesAsync(int parentId);

    /// <summary>
    /// 指定カテゴリの全子孫カテゴリ取得
    /// </summary>
    /// <param name="parentId">親カテゴリID</param>
    /// <returns>子孫カテゴリ一覧（階層の深さに関係なく全て）</returns>
    /// <remarks>
    /// 階層削除時の影響範囲確認や商品検索での階層展開に使用
    /// 再帰的に全ての子孫を取得
    /// </remarks>
    Task<IEnumerable<Category>> GetDescendantCategoriesAsync(int parentId);

    /// <summary>
    /// 指定カテゴリの先祖カテゴリ取得（パンくずリスト用）
    /// </summary>
    /// <param name="categoryId">カテゴリID</param>
    /// <returns>ルートから指定カテゴリまでの階層パス</returns>
    /// <remarks>
    /// パンくずリストやカテゴリパス表示に使用
    /// ルートから順番に並んだリストを返却
    /// </remarks>
    Task<IEnumerable<Category>> GetAncestorCategoriesAsync(int categoryId);

    /// <summary>
    /// 階層レベル別カテゴリ数取得
    /// </summary>
    /// <returns>レベルごとのカテゴリ数</returns>
    /// <remarks>
    /// 統計情報や管理画面での概要表示に使用
    /// </remarks>
    Task<Dictionary<int, int>> GetCategoryCountByLevelAsync();

    /// <summary>
    /// カテゴリ追加
    /// </summary>
    /// <param name="category">追加するカテゴリエンティティ</param>
    /// <returns>追加されたカテゴリエンティティ（IDが設定済み）</returns>
    /// <remarks>
    /// 階層レベルと整合性チェックは事前に実施すること
    /// 作成日時・更新日時は自動設定される
    /// </remarks>
    Task<Category> AddAsync(Category category);

    /// <summary>
    /// カテゴリ更新
    /// </summary>
    /// <param name="category">更新するカテゴリエンティティ</param>
    /// <returns>更新されたカテゴリエンティティ</returns>
    /// <remarks>
    /// 親カテゴリ変更時の循環参照チェックは事前に実施すること
    /// 更新日時は自動更新される
    /// </remarks>
    Task<Category> UpdateAsync(Category category);

    /// <summary>
    /// カテゴリ削除
    /// </summary>
    /// <param name="id">削除するカテゴリID</param>
    /// <returns>削除の成功可否</returns>
    /// <remarks>
    /// 子カテゴリまたは関連商品が存在する場合は削除失敗
    /// 削除前に CanDeleteAsync での確認を推奨
    /// </remarks>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// カテゴリ存在確認
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>存在する場合true、しない場合false</returns>
    /// <remarks>
    /// 削除前の存在確認や参照整合性チェックに使用
    /// </remarks>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// カテゴリ名の重複確認
    /// </summary>
    /// <param name="name">カテゴリ名</param>
    /// <param name="excludeCategoryId">確認対象から除外するカテゴリID（更新時に使用）</param>
    /// <returns>重複する場合true、しない場合false</returns>
    /// <remarks>
    /// カテゴリ登録・更新時のバリデーションに使用
    /// カテゴリ名はシステム全体でユニーク
    /// </remarks>
    Task<bool> IsNameDuplicateAsync(string name, int? excludeCategoryId = null);

    /// <summary>
    /// カテゴリ削除可能性確認
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>削除可能な場合true、不可能な場合false</returns>
    /// <remarks>
    /// 子カテゴリまたは関連商品が存在する場合は削除不可
    /// 削除操作前の事前確認に使用
    /// </remarks>
    Task<bool> CanDeleteAsync(int id);

    /// <summary>
    /// 親カテゴリ変更時の循環参照チェック
    /// </summary>
    /// <param name="categoryId">変更対象のカテゴリID</param>
    /// <param name="newParentId">新しい親カテゴリID</param>
    /// <returns>循環参照が発生する場合true、しない場合false</returns>
    /// <remarks>
    /// カテゴリの親子関係変更時の整合性チェックに使用
    /// 自分自身や子孫を親に設定することを防ぐ
    /// </remarks>
    Task<bool> WouldCreateCircularReferenceAsync(int categoryId, int newParentId);

    /// <summary>
    /// 指定カテゴリの商品数取得（子孫カテゴリ含む）
    /// </summary>
    /// <param name="categoryId">カテゴリID</param>
    /// <param name="includeDescendants">子孫カテゴリの商品も含むかどうか</param>
    /// <returns>関連商品数</returns>
    /// <remarks>
    /// カテゴリ一覧での商品数表示や統計情報に使用
    /// </remarks>
    Task<int> GetProductCountAsync(int categoryId, bool includeDescendants = true);

    /// <summary>
    /// カテゴリ階層ツリー取得（全階層）
    /// </summary>
    /// <returns>階層構造を保持したカテゴリツリー</returns>
    /// <remarks>
    /// カテゴリ選択UIやサイトマップ表示に使用
    /// 親子関係が設定された完全なツリー構造を返却
    /// </remarks>
    Task<IEnumerable<Category>> GetCategoryTreeAsync();
}