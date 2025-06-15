using Microsoft.EntityFrameworkCore;
using Web.Essentials.Domain.Entities;
using Web.Essentials.Domain.Repositories;
using Web.Essentials.Infrastructure.Data;

namespace Web.Essentials.Infrastructure.Repositories;

/// <summary>
/// カテゴリリポジトリ実装クラス - ICategoryRepository の Entity Framework Core 実装
/// </summary>
/// <remarks>
/// このクラスはカテゴリデータの永続化処理を実装し、以下の特徴を持つ：
/// - 階層構造に対応したデータ操作
/// - 自己参照による親子関係の管理
/// - 循環参照チェック機能
/// - 効率的な階層クエリ処理
/// - 商品数の集計機能
/// </remarks>
public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// CategoryRepository のコンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <remarks>
    /// 依存性注入により ApplicationDbContext が提供される
    /// </remarks>
    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// カテゴリIDによる単一カテゴリ取得
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <param name="includeRelations">関連エンティティを含むかどうか</param>
    /// <returns>カテゴリエンティティ、存在しない場合はnull</returns>
    public async Task<Category?> GetByIdAsync(int id, bool includeRelations = false)
    {
        var query = _context.Categories.AsQueryable();

        if (includeRelations)
        {
            // 関連エンティティを含める場合
            query = query
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories.OrderBy(cc => cc.SortOrder))
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// カテゴリ名による検索
    /// </summary>
    /// <param name="name">カテゴリ名</param>
    /// <returns>カテゴリエンティティ、存在しない場合はnull</returns>
    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    /// <summary>
    /// 全カテゴリ取得（階層構造・検索・フィルタリング対応）
    /// </summary>
    public async Task<IEnumerable<Category>> GetAllAsync(
        int? level = null,
        int? parentId = null,
        string? nameTerm = null,
        bool includeProductCount = false)
    {
        var query = _context.Categories.AsQueryable();

        // 階層レベルでの絞り込み
        if (level.HasValue)
        {
            query = query.Where(c => c.Level == level.Value);
        }

        // 親カテゴリIDでの絞り込み
        if (parentId.HasValue)
        {
            query = query.Where(c => c.ParentCategoryId == parentId.Value);
        }

        // カテゴリ名での部分一致検索
        if (!string.IsNullOrWhiteSpace(nameTerm))
        {
            query = query.Where(c => c.Name.Contains(nameTerm));
        }

        // 関連データを含める
        query = query
            .Include(c => c.ParentCategory)
            .Include(c => c.ChildCategories.OrderBy(cc => cc.SortOrder));

        if (includeProductCount)
        {
            query = query.Include(c => c.ProductCategories);
        }

        // Level と SortOrder でソート
        return await query
            .OrderBy(c => c.Level)
            .ThenBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <summary>
    /// ルートカテゴリ一覧取得
    /// </summary>
    /// <returns>階層レベル0のカテゴリ一覧</returns>
    public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.Level == 0)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <summary>
    /// 指定カテゴリの子カテゴリ一覧取得
    /// </summary>
    /// <param name="parentId">親カテゴリID</param>
    /// <returns>子カテゴリ一覧</returns>
    public async Task<IEnumerable<Category>> GetChildCategoriesAsync(int parentId)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <summary>
    /// 指定カテゴリの全子孫カテゴリ取得
    /// </summary>
    /// <param name="parentId">親カテゴリID</param>
    /// <returns>子孫カテゴリ一覧</returns>
    public async Task<IEnumerable<Category>> GetDescendantCategoriesAsync(int parentId)
    {
        var descendants = new List<Category>();
        await CollectDescendantsAsync(parentId, descendants);
        return descendants;
    }

    /// <summary>
    /// 指定カテゴリの先祖カテゴリ取得（パンくずリスト用）
    /// </summary>
    /// <param name="categoryId">カテゴリID</param>
    /// <returns>ルートから指定カテゴリまでの階層パス</returns>
    public async Task<IEnumerable<Category>> GetAncestorCategoriesAsync(int categoryId)
    {
        var ancestors = new List<Category>();
        var current = await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        while (current != null)
        {
            ancestors.Insert(0, current); // 先頭に挿入してルートから順に並べる
            current = current.ParentCategory;
        }

        return ancestors;
    }

    /// <summary>
    /// 階層レベル別カテゴリ数取得
    /// </summary>
    /// <returns>レベルごとのカテゴリ数</returns>
    public async Task<Dictionary<int, int>> GetCategoryCountByLevelAsync()
    {
        return await _context.Categories
            .GroupBy(c => c.Level)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// カテゴリ追加
    /// </summary>
    /// <param name="category">追加するカテゴリエンティティ</param>
    /// <returns>追加されたカテゴリエンティティ</returns>
    public async Task<Category> AddAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    /// <summary>
    /// カテゴリ更新
    /// </summary>
    /// <param name="category">更新するカテゴリエンティティ</param>
    /// <returns>更新されたカテゴリエンティティ</returns>
    public async Task<Category> UpdateAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    /// <summary>
    /// カテゴリ削除
    /// </summary>
    /// <param name="id">削除するカテゴリID</param>
    /// <returns>削除の成功可否</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        // 削除可能性を事前にチェック
        if (!await CanDeleteAsync(id))
            return false;

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// カテゴリ存在確認
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>存在する場合true</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }

    /// <summary>
    /// カテゴリ名の重複確認
    /// </summary>
    /// <param name="name">カテゴリ名</param>
    /// <param name="excludeCategoryId">確認対象から除外するカテゴリID</param>
    /// <returns>重複する場合true</returns>
    public async Task<bool> IsNameDuplicateAsync(string name, int? excludeCategoryId = null)
    {
        var query = _context.Categories.Where(c => c.Name == name);

        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCategoryId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// カテゴリ削除可能性確認
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>削除可能な場合true</returns>
    public async Task<bool> CanDeleteAsync(int id)
    {
        // 子カテゴリが存在するかチェック
        var hasChildren = await _context.Categories
            .AnyAsync(c => c.ParentCategoryId == id);

        if (hasChildren)
            return false;

        // 関連商品が存在するかチェック
        var hasProducts = await _context.ProductCategories
            .AnyAsync(pc => pc.CategoryId == id);

        return !hasProducts;
    }

    /// <summary>
    /// 親カテゴリ変更時の循環参照チェック
    /// </summary>
    /// <param name="categoryId">変更対象のカテゴリID</param>
    /// <param name="newParentId">新しい親カテゴリID</param>
    /// <returns>循環参照が発生する場合true</returns>
    public async Task<bool> WouldCreateCircularReferenceAsync(int categoryId, int newParentId)
    {
        // 自分自身を親に設定しようとしている場合
        if (categoryId == newParentId)
            return true;

        // 子孫カテゴリを親に設定しようとしている場合
        var descendants = await GetDescendantCategoriesAsync(categoryId);
        return descendants.Any(d => d.Id == newParentId);
    }

    /// <summary>
    /// 指定カテゴリの商品数取得（子孫カテゴリ含む）
    /// </summary>
    /// <param name="categoryId">カテゴリID</param>
    /// <param name="includeDescendants">子孫カテゴリの商品も含むかどうか</param>
    /// <returns>関連商品数</returns>
    public async Task<int> GetProductCountAsync(int categoryId, bool includeDescendants = true)
    {
        var categoryIds = new List<int> { categoryId };

        if (includeDescendants)
        {
            var descendants = await GetDescendantCategoriesAsync(categoryId);
            categoryIds.AddRange(descendants.Select(d => d.Id));
        }

        return await _context.ProductCategories
            .Where(pc => categoryIds.Contains(pc.CategoryId))
            .CountAsync();
    }

    /// <summary>
    /// カテゴリ階層ツリー取得（全階層）
    /// </summary>
    /// <returns>階層構造を保持したカテゴリツリー</returns>
    public async Task<IEnumerable<Category>> GetCategoryTreeAsync()
    {
        // 全カテゴリを一度に取得
        var allCategories = await _context.Categories
            .Include(c => c.ChildCategories)
            .OrderBy(c => c.Level)
            .ThenBy(c => c.SortOrder)
            .ToListAsync();

        // ルートカテゴリのみを返す（子カテゴリは Include で自動的に含まれる）
        return allCategories.Where(c => c.Level == 0);
    }

    /// <summary>
    /// 子孫カテゴリを再帰的に収集するヘルパーメソッド
    /// </summary>
    /// <param name="parentId">親カテゴリID</param>
    /// <param name="descendants">収集先のリスト</param>
    /// <remarks>
    /// GetDescendantCategoriesAsync から呼び出される内部メソッド
    /// </remarks>
    private async Task CollectDescendantsAsync(int parentId, List<Category> descendants)
    {
        var children = await _context.Categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        foreach (var child in children)
        {
            descendants.Add(child);
            // 再帰的に子孫を収集
            await CollectDescendantsAsync(child.Id, descendants);
        }
    }
}