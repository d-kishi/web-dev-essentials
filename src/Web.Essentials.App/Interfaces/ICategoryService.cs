using Web.Essentials.App.DTOs;
using Web.Essentials.App.ViewModels;
using Web.Essentials.Domain.Entities;

namespace Web.Essentials.App.Interfaces;

/// <summary>
/// カテゴリサービスインターフェース
/// </summary>
/// <remarks>
/// カテゴリに関するビジネスロジックを定義
/// 階層構造の管理とナビゲーション機能を含む
/// </remarks>
public interface ICategoryService
{
    #region 一覧・検索・階層取得

    /// <summary>
    /// カテゴリ一覧取得（検索・フィルタ対応）
    /// </summary>
    /// <param name="searchRequest">検索条件</param>
    /// <returns>カテゴリ一覧DTO</returns>
    Task<List<CategoryDto>> GetCategoriesAsync(CategorySearchRequestDto searchRequest);

    /// <summary>
    /// カテゴリ一覧取得（ViewModel用）
    /// </summary>
    /// <param name="viewModel">検索条件ViewModel</param>
    /// <returns>カテゴリ一覧ViewModel</returns>
    Task<CategoryIndexViewModel> GetCategoryIndexAsync(CategoryIndexViewModel viewModel);

    /// <summary>
    /// カテゴリツリー取得
    /// </summary>
    /// <param name="rootCategoryId">ルートカテゴリID（nullの場合は全ツリー）</param>
    /// <returns>カテゴリツリーDTO</returns>
    Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync(int? rootCategoryId = null);

    /// <summary>
    /// 選択用カテゴリ一覧取得
    /// </summary>
    /// <returns>選択用カテゴリDTO一覧</returns>
    Task<IEnumerable<CategorySelectDto>> GetCategoriesForSelectAsync();

    /// <summary>
    /// 階層構造を持つ選択用カテゴリ一覧取得
    /// </summary>
    /// <returns>階層構造を持つCategorySelectItem一覧</returns>
    Task<IEnumerable<CategorySelectItem>> GetCategorySelectItemsAsync();

    #endregion

    #region 詳細・存在確認

    /// <summary>
    /// カテゴリ詳細取得
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>カテゴリ詳細ViewModel</returns>
    Task<CategoryDetailsViewModel?> GetCategoryDetailsAsync(int id);

    /// <summary>
    /// カテゴリ存在確認
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>存在する場合true</returns>
    Task<bool> ExistsAsync(int id);

    #endregion

    #region 作成・更新・削除

    /// <summary>
    /// カテゴリ作成
    /// </summary>
    /// <param name="createModel">作成用ViewModel</param>
    /// <returns>作成されたカテゴリID</returns>
    Task<int> CreateCategoryAsync(CategoryCreateViewModel createModel);

    /// <summary>
    /// カテゴリ更新
    /// </summary>
    /// <param name="editModel">更新用ViewModel</param>
    /// <returns>更新成功の場合true</returns>
    Task<bool> UpdateCategoryAsync(CategoryEditViewModel editModel);

    /// <summary>
    /// カテゴリ削除
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>削除成功の場合true</returns>
    Task<bool> DeleteCategoryAsync(int id);

    #endregion

    #region 編集用データ取得

    /// <summary>
    /// カテゴリ編集用データ取得
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>編集用ViewModel</returns>
    Task<CategoryEditViewModel?> GetCategoryForEditAsync(int id);

    /// <summary>
    /// カテゴリ作成用データ取得
    /// </summary>
    /// <returns>作成用ViewModel</returns>
    Task<CategoryCreateViewModel> GetCategoryForCreateAsync();

    #endregion

    #region 階層構造管理

    /// <summary>
    /// 子カテゴリ一覧取得
    /// </summary>
    /// <param name="parentId">親カテゴリID</param>
    /// <returns>子カテゴリ一覧</returns>
    Task<IEnumerable<CategoryListItemViewModel>> GetChildCategoriesAsync(int parentId);

    /// <summary>
    /// 親カテゴリ一覧取得（階層パス）
    /// </summary>
    /// <param name="categoryId">カテゴリID</param>
    /// <returns>親カテゴリ一覧（パンくずリスト用）</returns>
    Task<IEnumerable<CategoryBreadcrumbDto>> GetAncestorsAsync(int categoryId);

    /// <summary>
    /// 指定カテゴリの削除可能性チェック
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>削除可能の場合true</returns>
    Task<bool> CanDeleteAsync(int id);

    /// <summary>
    /// 階層移動の妥当性チェック
    /// </summary>
    /// <param name="categoryId">移動対象カテゴリID</param>
    /// <param name="newParentId">新しい親カテゴリID</param>
    /// <returns>移動可能の場合true</returns>
    Task<bool> CanMoveToParentAsync(int categoryId, int? newParentId);

    #endregion

    #region バリデーション

    /// <summary>
    /// カテゴリ名重複チェック
    /// </summary>
    /// <param name="name">カテゴリ名</param>
    /// <param name="excludeId">除外するカテゴリID（編集時）</param>
    /// <returns>重複している場合true</returns>
    Task<bool> IsCategoryNameDuplicateAsync(string name, int? excludeId = null);

    #endregion

    #region レベル・順序管理

    /// <summary>
    /// 指定レベルのカテゴリ一覧取得
    /// </summary>
    /// <param name="level">階層レベル</param>
    /// <returns>カテゴリ一覧</returns>
    Task<IEnumerable<CategoryListItemViewModel>> GetCategoriesByLevelAsync(int level);

    /// <summary>
    /// 表示順序の再配置
    /// </summary>
    /// <param name="parentId">親カテゴリID</param>
    /// <param name="categoryIds">新しい順序でのカテゴリID一覧</param>
    /// <returns>成功の場合true</returns>
    Task<bool> ReorderCategoriesAsync(int? parentId, IEnumerable<int> categoryIds);

    #endregion
}