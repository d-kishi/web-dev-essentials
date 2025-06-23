using Microsoft.EntityFrameworkCore;
using Web.Essentials.Domain.Entities;
using Web.Essentials.Domain.Repositories;
using Web.Essentials.Infrastructure.Data;

namespace Web.Essentials.Infrastructure.Repositories;

/// <summary>
/// 商品リポジトリ実装クラス - IProductRepository の Entity Framework Core 実装
/// </summary>
/// <remarks>
/// このクラスは商品データの永続化処理を実装し、以下の特徴を持つ：
/// - Entity Framework Core による InMemory データベースアクセス
/// - LINQ を使用した効率的なクエリ処理
/// - 非同期処理による パフォーマンス最適化
/// - 複雑な検索条件とページング処理
/// - 関連エンティティの最適な読み込み制御
/// </remarks>
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// ProductRepository のコンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <remarks>
    /// 依存性注入により ApplicationDbContext が提供される
    /// </remarks>
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(int id, bool includeRelations = false)
    {
        var query = _context.Products.AsQueryable();

        if (includeRelations)
        {
            // 関連エンティティを含める場合は Include を使用
            query = query
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                        .ThenInclude(c => c.ParentCategory)
                .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder));
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<Product?> GetByJanCodeAsync(string janCode)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.JanCode == janCode);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
        string? nameTerm = null,
        string? janCodeTerm = null,
        int? categoryId = null,
        ProductStatus? status = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string sortBy = "createdAt",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductImages.Where(pi => pi.IsMain))
            .AsQueryable();

        // 商品名での部分一致検索
        if (!string.IsNullOrWhiteSpace(nameTerm))
        {
            query = query.Where(p => p.Name.Contains(nameTerm));
        }

        // JANコードでの部分一致検索
        if (!string.IsNullOrWhiteSpace(janCodeTerm))
        {
            query = query.Where(p => p.JanCode != null && p.JanCode.Contains(janCodeTerm));
        }

        // カテゴリでの絞り込み（階層検索対応）
        if (categoryId.HasValue)
        {
            // 指定カテゴリとその子孫カテゴリのIDを取得
            var categoryIds = await GetCategoryAndDescendantIdsAsync(categoryId.Value);
            query = query.Where(p => p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
        }

        // ステータスでの絞り込み
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        // 価格範囲での絞り込み
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // 総件数を取得（ソートやページングの前に）
        var totalCount = await query.CountAsync();

        // ソート処理
        query = ApplySorting(query, sortBy, sortOrder);

        // ページング処理
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, bool includeDescendants = false)
    {
        var categoryIds = includeDescendants
            ? await GetCategoryAndDescendantIdsAsync(categoryId)
            : new List<int> { categoryId };

        return await _context.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductImages.Where(pi => pi.IsMain))
            .Where(p => p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetLatestAsync(int count = 10)
    {
        return await _context.Products
            .Include(p => p.ProductImages.Where(pi => pi.IsMain))
            .Where(p => p.Status == ProductStatus.OnSale)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <inheritdoc />
    public async Task<Product> UpdateAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }
    
    /// <summary>
    /// 商品の基本プロパティのみを安全に更新する
    /// ProductCategoriesの関係は UpdateProductCategoriesAsync で別途管理
    /// </summary>
    /// <param name="product">更新する商品情報</param>
    /// <returns>更新された商品</returns>
    public async Task<Product> UpdateBasicPropertiesAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        // エンティティの状態をクリアして競合を回避
        var entry = _context.Entry(product);
        entry.State = EntityState.Detached;
        
        // 基本プロパティのみを更新（ProductCategoriesは除外）
        var trackedProduct = await _context.Products
            .Where(p => p.Id == product.Id)
            .FirstOrDefaultAsync();
            
        if (trackedProduct == null)
            throw new InvalidOperationException($"Product with ID {product.Id} not found");
            
        // 基本プロパティのみをコピー
        trackedProduct.Name = product.Name;
        trackedProduct.Description = product.Description;
        trackedProduct.Price = product.Price;
        trackedProduct.JanCode = product.JanCode;
        trackedProduct.Status = product.Status;
        trackedProduct.UpdatedAt = product.UpdatedAt;
        
        await _context.SaveChangesAsync();
        return trackedProduct;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }
    
    /// <summary>
    /// 商品のカテゴリ関係を安全に更新する
    /// InMemoryプロバイダーの複合キー問題を回避するため、専用メソッドで処理
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="categoryIds">新しいカテゴリIDのリスト</param>
    public async Task UpdateProductCategoriesAsync(int productId, List<int> categoryIds)
    {
        // 既存のProductCategoryを全て削除
        var existingProductCategories = await _context.ProductCategories
            .Where(pc => pc.ProductId == productId)
            .ToListAsync();
            
        if (existingProductCategories.Any())
        {
            _context.ProductCategories.RemoveRange(existingProductCategories);
            await _context.SaveChangesAsync(); // 先に削除を確定
        }
        
        // 新しいProductCategoryを追加
        if (categoryIds.Any())
        {
            var newProductCategories = categoryIds.Select(categoryId => new ProductCategory
            {
                ProductId = productId,
                CategoryId = categoryId,
                CreatedAt = DateTime.UtcNow
            }).ToList();
            
            await _context.ProductCategories.AddRangeAsync(newProductCategories);
            await _context.SaveChangesAsync(); // 追加を確定
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsJanCodeDuplicateAsync(string janCode, int? excludeProductId = null)
    {
        var query = _context.Products.Where(p => p.JanCode == janCode);

        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return await query.AnyAsync();
    }

    /// <inheritdoc />
    public async Task<ProductStatistics> GetStatisticsAsync()
    {
        var products = await _context.Products.ToListAsync();

        if (!products.Any())
        {
            return new ProductStatistics
            {
                TotalCount = 0,
                PreSaleCount = 0,
                OnSaleCount = 0,
                DiscontinuedCount = 0,
                AveragePrice = 0,
                MaxPrice = 0,
                MinPrice = 0
            };
        }

        return new ProductStatistics
        {
            TotalCount = products.Count,
            PreSaleCount = products.Count(p => p.Status == ProductStatus.PreSale),
            OnSaleCount = products.Count(p => p.Status == ProductStatus.OnSale),
            DiscontinuedCount = products.Count(p => p.Status == ProductStatus.Discontinued),
            AveragePrice = (uint)products.Average(p => (double)p.Price),
            MaxPrice = products.Max(p => p.Price),
            MinPrice = products.Min(p => p.Price)
        };
    }

    /// <summary>
    /// 指定カテゴリとその子孫カテゴリのIDを取得
    /// </summary>
    /// <param name="categoryId">カテゴリID</param>
    /// <returns>カテゴリIDのリスト</returns>
    /// <remarks>
    /// 階層検索で使用するヘルパーメソッド
    /// 再帰的に子孫カテゴリを取得
    /// </remarks>
    private async Task<List<int>> GetCategoryAndDescendantIdsAsync(int categoryId)
    {
        var categoryIds = new List<int> { categoryId };

        // 子カテゴリを再帰的に取得
        var childCategories = await _context.Categories
            .Where(c => c.ParentCategoryId == categoryId)
            .ToListAsync();

        foreach (var child in childCategories)
        {
            var descendantIds = await GetCategoryAndDescendantIdsAsync(child.Id);
            categoryIds.AddRange(descendantIds);
        }

        return categoryIds;
    }

    /// <summary>
    /// ソート処理を適用
    /// </summary>
    /// <param name="query">対象クエリ</param>
    /// <param name="sortBy">ソート項目</param>
    /// <param name="sortOrder">ソート順</param>
    /// <returns>ソートが適用されたクエリ</returns>
    /// <remarks>
    /// 動的ソート処理のヘルパーメソッド
    /// </remarks>
    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy, string sortOrder)
    {
        var isDescending = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "createdat" or "created" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "updatedat" or "updated" => isDescending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt) // デフォルトは作成日時降順
        };
    }
}