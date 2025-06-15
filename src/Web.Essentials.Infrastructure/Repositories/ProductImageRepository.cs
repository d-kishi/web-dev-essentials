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

    /// <summary>
    /// 画像IDによる単一画像取得
    /// </summary>
    /// <param name="id">画像ID</param>
    /// <param name="includeProduct">関連商品情報を含むかどうか</param>
    /// <returns>商品画像エンティティ、存在しない場合はnull</returns>
    public async Task<ProductImage?> GetByIdAsync(int id, bool includeProduct = false)
    {
        var query = _context.ProductImages.AsQueryable();

        if (includeProduct)
        {
            query = query.Include(pi => pi.Product);
        }

        return await query.FirstOrDefaultAsync(pi => pi.Id == id);
    }

    /// <summary>
    /// 指定商品の全画像取得
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="orderByDisplayOrder">表示順序でソートするかどうか</param>
    /// <returns>商品に関連付けられた画像一覧</returns>
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

    /// <summary>
    /// 指定商品のメイン画像取得
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>メイン画像、存在しない場合はnull</returns>
    public async Task<ProductImage?> GetMainImageByProductIdAsync(int productId)
    {
        return await _context.ProductImages
            .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.IsMain);
    }

    /// <summary>
    /// 複数商品のメイン画像を一括取得
    /// </summary>
    /// <param name="productIds">商品ID一覧</param>
    /// <returns>商品IDをキーとした画像の辞書</returns>
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

    /// <summary>
    /// 画像ファイルパスによる検索
    /// </summary>
    /// <param name="imagePath">画像ファイルパス</param>
    /// <returns>商品画像エンティティ、存在しない場合はnull</returns>
    public async Task<ProductImage?> GetByImagePathAsync(string imagePath)
    {
        return await _context.ProductImages
            .FirstOrDefaultAsync(pi => pi.ImagePath == imagePath);
    }

    /// <summary>
    /// 商品画像追加
    /// </summary>
    /// <param name="productImage">追加する商品画像エンティティ</param>
    /// <returns>追加された商品画像エンティティ</returns>
    public async Task<ProductImage> AddAsync(ProductImage productImage)
    {
        if (productImage == null)
            throw new ArgumentNullException(nameof(productImage));

        _context.ProductImages.Add(productImage);
        await _context.SaveChangesAsync();
        return productImage;
    }

    /// <summary>
    /// 商品画像更新
    /// </summary>
    /// <param name="productImage">更新する商品画像エンティティ</param>
    /// <returns>更新された商品画像エンティティ</returns>
    public async Task<ProductImage> UpdateAsync(ProductImage productImage)
    {
        if (productImage == null)
            throw new ArgumentNullException(nameof(productImage));

        _context.ProductImages.Update(productImage);
        await _context.SaveChangesAsync();
        return productImage;
    }

    /// <summary>
    /// 商品画像削除
    /// </summary>
    /// <param name="id">削除する画像ID</param>
    /// <returns>削除の成功可否</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var productImage = await _context.ProductImages.FindAsync(id);
        if (productImage == null)
            return false;

        _context.ProductImages.Remove(productImage);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 指定商品の全画像削除
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>削除の成功可否</returns>
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

    /// <summary>
    /// 画像存在確認
    /// </summary>
    /// <param name="id">画像ID</param>
    /// <returns>存在する場合true</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ProductImages.AnyAsync(pi => pi.Id == id);
    }

    /// <summary>
    /// 指定商品での表示順序重複確認
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="displayOrder">表示順序</param>
    /// <param name="excludeImageId">確認対象から除外する画像ID</param>
    /// <returns>重複する場合true</returns>
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

    /// <summary>
    /// 指定商品のメイン画像設定確認
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="excludeImageId">確認対象から除外する画像ID</param>
    /// <returns>メイン画像が設定済みの場合true</returns>
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

    /// <summary>
    /// 指定商品の画像数取得
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>画像数</returns>
    public async Task<int> GetImageCountByProductIdAsync(int productId)
    {
        return await _context.ProductImages
            .Where(pi => pi.ProductId == productId)
            .CountAsync();
    }

    /// <summary>
    /// 指定商品での使用可能な表示順序取得
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>使用可能な表示順序一覧（1-5の範囲）</returns>
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

    /// <summary>
    /// 指定商品の表示順序を再調整
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>再調整の成功可否</returns>
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

    /// <summary>
    /// メイン画像の変更
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="newMainImageId">新しいメイン画像ID</param>
    /// <returns>変更の成功可否</returns>
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

    /// <summary>
    /// 孤立画像ファイルの検出
    /// </summary>
    /// <param name="existingFilePaths">実際に存在するファイルパス一覧</param>
    /// <returns>データベースに記録されているが実ファイルが存在しない画像一覧</returns>
    public async Task<IEnumerable<ProductImage>> GetOrphanedImagesAsync(IEnumerable<string> existingFilePaths)
    {
        var existingPathSet = existingFilePaths.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return await _context.ProductImages
            .Where(pi => !existingPathSet.Contains(pi.ImagePath))
            .ToListAsync();
    }

    /// <summary>
    /// 未使用ファイルの検出
    /// </summary>
    /// <param name="allFilePaths">実際に存在するファイルパス一覧</param>
    /// <returns>実ファイルは存在するがデータベースに記録されていないファイルパス一覧</returns>
    public async Task<IEnumerable<string>> GetUnusedFilePathsAsync(IEnumerable<string> allFilePaths)
    {
        var usedPaths = await _context.ProductImages
            .Select(pi => pi.ImagePath)
            .ToListAsync();

        var usedPathSet = usedPaths.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return allFilePaths.Where(path => !usedPathSet.Contains(path));
    }
}