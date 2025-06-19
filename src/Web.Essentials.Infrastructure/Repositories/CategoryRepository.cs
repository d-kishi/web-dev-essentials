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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.Level == 0)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetChildCategoriesAsync(int parentId)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetDescendantCategoriesAsync(int parentId)
    {
        var descendants = new List<Category>();
        await CollectDescendantsAsync(parentId, descendants);
        return descendants;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<Dictionary<int, int>> GetCategoryCountByLevelAsync()
    {
        return await _context.Categories
            .GroupBy(c => c.Level)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    /// <inheritdoc />
    public async Task<Category> AddAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    /// <inheritdoc />
    public async Task<Category> UpdateAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }

    /// <inheritdoc />
    public async Task<bool> IsNameDuplicateAsync(string name, int? excludeCategoryId = null)
    {
        var query = _context.Categories.Where(c => c.Name == name);

        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCategoryId.Value);
        }

        return await query.AnyAsync();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<bool> WouldCreateCircularReferenceAsync(int categoryId, int newParentId)
    {
        // 自分自身を親に設定しようとしている場合
        if (categoryId == newParentId)
            return true;

        // 子孫カテゴリを親に設定しようとしている場合
        var descendants = await GetDescendantCategoriesAsync(categoryId);
        return descendants.Any(d => d.Id == newParentId);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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