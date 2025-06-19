using Microsoft.EntityFrameworkCore;
using Web.Essentials.Domain.Entities;
using Web.Essentials.Domain.Repositories;
using Web.Essentials.Infrastructure.Data;

namespace Web.Essentials.Infrastructure.Repositories;

/// <summary>
/// 商品画像リポジトリ実装クラス - IProductImageRepository の Entity Framework Core 実装
/// </summary>
/// <remarks>
/// このクラスは商品画像データの永続化処理を実装し、以下の特徴を持つ：
/// - 商品との関連付け管理
/// - 表示順序とメイン画像の制御
/// - ファイル整合性チェック機能
/// - 効率的な一括操作
/// - 制約チェック機能
/// </remarks>
public class ProductImageRepository : IProductImageRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// ProductImageRepository のコンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <remarks>
    /// 依存性注入により ApplicationDbContext が提供される
    /// </remarks>
    public ProductImageRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<ProductImage?> GetByIdAsync(int id, bool includeProduct = false)
    {
        var query = _context.ProductImages.AsQueryable();

        if (includeProduct)
        {
            query = query.Include(pi => pi.Product);
        }

        return await query.FirstOrDefaultAsync(pi => pi.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId, bool orderByDisplayOrder = true)
    {
        var query = _context.ProductImages
            .Where(pi => pi.ProductId == productId);

        if (orderByDisplayOrder)
        {
            query = query.OrderBy(pi => pi.DisplayOrder);
        }

        return await query.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ProductImage?> GetMainImageByProductIdAsync(int productId)
    {
        return await _context.ProductImages
            .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.IsMain);
    }

    /// <inheritdoc />
    public async Task<Dictionary<int, ProductImage?>> GetMainImagesByProductIdsAsync(IEnumerable<int> productIds)
    {
        var productIdList = productIds.ToList();
        
        var mainImages = await _context.ProductImages
            .Where(pi => productIdList.Contains(pi.ProductId) && pi.IsMain)
            .ToListAsync();

        // 商品IDをキーとした辞書を作成
        var result = new Dictionary<int, ProductImage?>();
        
        foreach (var productId in productIdList)
        {
            result[productId] = mainImages.FirstOrDefault(pi => pi.ProductId == productId);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<ProductImage?> GetByImagePathAsync(string imagePath)
    {
        return await _context.ProductImages
            .FirstOrDefaultAsync(pi => pi.ImagePath == imagePath);
    }

    /// <inheritdoc />
    public async Task<ProductImage> AddAsync(ProductImage productImage)
    {
        if (productImage == null)
            throw new ArgumentNullException(nameof(productImage));

        _context.ProductImages.Add(productImage);
        await _context.SaveChangesAsync();
        return productImage;
    }

    /// <inheritdoc />
    public async Task<ProductImage> UpdateAsync(ProductImage productImage)
    {
        if (productImage == null)
            throw new ArgumentNullException(nameof(productImage));

        _context.ProductImages.Update(productImage);
        await _context.SaveChangesAsync();
        return productImage;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var productImage = await _context.ProductImages.FindAsync(id);
        if (productImage == null)
            return false;

        _context.ProductImages.Remove(productImage);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteByProductIdAsync(int productId)
    {
        var productImages = await _context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .ToListAsync();

        if (!productImages.Any())
            return true; // 削除対象がない場合は成功とみなす

        _context.ProductImages.RemoveRange(productImages);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ProductImages.AnyAsync(pi => pi.Id == id);
    }

    /// <inheritdoc />
    public async Task<bool> IsDisplayOrderDuplicateAsync(int productId, int displayOrder, int? excludeImageId = null)
    {
        var query = _context.ProductImages
            .Where(pi => pi.ProductId == productId && pi.DisplayOrder == displayOrder);

        if (excludeImageId.HasValue)
        {
            query = query.Where(pi => pi.Id != excludeImageId.Value);
        }

        return await query.AnyAsync();
    }

    /// <inheritdoc />
    public async Task<bool> HasMainImageAsync(int productId, int? excludeImageId = null)
    {
        var query = _context.ProductImages
            .Where(pi => pi.ProductId == productId && pi.IsMain);

        if (excludeImageId.HasValue)
        {
            query = query.Where(pi => pi.Id != excludeImageId.Value);
        }

        return await query.AnyAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetImageCountByProductIdAsync(int productId)
    {
        return await _context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .CountAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<int>> GetAvailableDisplayOrdersAsync(int productId)
    {
        const int maxDisplayOrder = 5;
        var allOrders = Enumerable.Range(1, maxDisplayOrder);

        var usedOrders = await _context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .Select(pi => pi.DisplayOrder)
            .ToListAsync();

        return allOrders.Except(usedOrders);
    }

    /// <inheritdoc />
    public async Task<bool> ReorderDisplayOrderAsync(int productId)
    {
        var images = await _context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .OrderBy(pi => pi.DisplayOrder)
            .ToListAsync();

        if (!images.Any())
            return true;

        // 1から連番になるよう再調整
        for (int i = 0; i < images.Count; i++)
        {
            images[i].DisplayOrder = i + 1;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ChangeMainImageAsync(int productId, int newMainImageId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 既存のメイン画像をすべて非メイン画像に変更
            var existingMainImages = await _context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.IsMain)
                .ToListAsync();

            foreach (var image in existingMainImages)
            {
                image.IsMain = false;
            }

            // 新しいメイン画像を設定
            var newMainImage = await _context.ProductImages
                .FirstOrDefaultAsync(pi => pi.Id == newMainImageId && pi.ProductId == productId);

            if (newMainImage == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            newMainImage.IsMain = true;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProductImage>> GetOrphanedImagesAsync(IEnumerable<string> existingFilePaths)
    {
        var existingPathSet = existingFilePaths.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return await _context.ProductImages
            .Where(pi => !existingPathSet.Contains(pi.ImagePath))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetUnusedFilePathsAsync(IEnumerable<string> allFilePaths)
    {
        var usedPaths = await _context.ProductImages
            .Select(pi => pi.ImagePath)
            .ToListAsync();

        var usedPathSet = usedPaths.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return allFilePaths.Where(path => !usedPathSet.Contains(path));
    }
}