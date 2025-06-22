using Web.Essentials.App.ViewModels;
using Web.Essentials.Domain.Entities;

namespace Web.Essentials.App.Interfaces;

/// <summary>
/// カテゴリ階層構造処理専用サービスのインターフェース
/// 単一責任の原則に基づき、カテゴリの階層構造処理のみを定義
/// </summary>
public interface ICategoryHierarchyService
{
    /// <summary>
    /// カテゴリ選択用のアイテム一覧を階層構造で取得する
    /// 階層構造を保ったまま、表示用のインデント情報も含める
    /// </summary>
    /// <returns>階層構造を保ったカテゴリ選択用アイテム一覧</returns>
    /// <exception cref="Exception">データベースアクセスエラーが発生した場合</exception>
    Task<IEnumerable<CategorySelectItem>> GetCategorySelectItemsAsync();

    /// <summary>
    /// カテゴリの完全なパス（階層構造）を文字列で構築する
    /// 例: "スポーツ > シューズ > ランニング"
    /// </summary>
    /// <param name="category">対象カテゴリ</param>
    /// <param name="categoryDict">全カテゴリの辞書（パフォーマンス向上のため）</param>
    /// <returns>階層パス文字列</returns>
    string BuildCategoryFullPath(Category category, Dictionary<int, Category> categoryDict);

    /// <summary>
    /// カテゴリリストを階層構造に構築する
    /// フラットなカテゴリリストから親子関係を構築し、ルートカテゴリのみを返す
    /// </summary>
    /// <param name="categories">フラットなカテゴリリスト</param>
    /// <returns>階層構造を持つルートカテゴリリスト</returns>
    /// <exception cref="Exception">階層構造構築エラーが発生した場合</exception>
    Task<List<Category>> BuildCategoryHierarchyAsync(IEnumerable<Category> categories);

    /// <summary>
    /// 指定されたカテゴリIDから親カテゴリを遡ってパスを取得する
    /// ルートカテゴリから指定カテゴリまでの経路を配列で返す
    /// </summary>
    /// <param name="categoryId">対象カテゴリID</param>
    /// <param name="categoryDict">全カテゴリの辞書（パフォーマンス向上のため）</param>
    /// <returns>ルートから指定カテゴリまでのパス</returns>
    List<Category> GetCategoryPath(int categoryId, Dictionary<int, Category> categoryDict);

    /// <summary>
    /// 指定されたカテゴリの階層レベルを計算する
    /// ルートカテゴリをレベル0として、子の階層を計算
    /// </summary>
    /// <param name="categoryId">対象カテゴリID</param>
    /// <param name="categoryDict">全カテゴリの辞書（パフォーマンス向上のため）</param>
    /// <returns>階層レベル（0:ルート、1:第1階層、2:第2階層）</returns>
    int CalculateCategoryLevel(int categoryId, Dictionary<int, Category> categoryDict);
}