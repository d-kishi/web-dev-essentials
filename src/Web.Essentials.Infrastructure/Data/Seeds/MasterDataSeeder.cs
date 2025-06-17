using Microsoft.EntityFrameworkCore;
using Web.Essentials.Domain.Entities;

namespace Web.Essentials.Infrastructure.Data.Seeds;

/// <summary>
/// マスターデータシーダー統合クラス
/// </summary>
/// <remarks>
/// 各エンティティのシーダークラスを統合し、データベースの初期化を一元管理
/// - 依存関係に基づく正しい順序でのデータ投入
/// - Entity Framework Core の HasData メソッドによる初期データ設定
/// - 教育目的のスポーツ用品データを提供
/// </remarks>
public static class MasterDataSeeder
{
    /// <summary>
    /// ModelBuilder に対してすべての初期データを設定
    /// </summary>
    /// <param name="modelBuilder">Entity Framework の ModelBuilder インスタンス</param>
    /// <remarks>
    /// 以下の順序で初期データを設定：
    /// 1. Categories（カテゴリ：親子関係があるため最初）
    /// 2. Products（商品：カテゴリに依存）
    /// 3. ProductCategories（商品カテゴリ関連：商品とカテゴリに依存）
    /// 4. ProductImages（商品画像：商品に依存）
    /// </remarks>
    public static void SeedAll(ModelBuilder modelBuilder)
    {
        SeedCategories(modelBuilder);
        SeedProducts(modelBuilder);
        SeedProductCategories(modelBuilder);
        SeedProductImages(modelBuilder);
    }

    /// <summary>
    /// カテゴリの初期データを設定
    /// </summary>
    /// <param name="modelBuilder">Entity Framework の ModelBuilder インスタンス</param>
    /// <remarks>
    /// 3階層のカテゴリ構造（競技別 → 用品種別 → 詳細分類）を設定
    /// 自己参照による階層構造のため、外部キー制約に注意
    /// </remarks>
    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(CategorySeeder.GetCategories());
    }

    /// <summary>
    /// 商品の初期データを設定
    /// </summary>
    /// <param name="modelBuilder">Entity Framework の ModelBuilder インスタンス</param>
    /// <remarks>
    /// スポーツ用品の商品データを設定
    /// CategoryId による単純な1対多関係を使用
    /// </remarks>
    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(ProductSeeder.GetProducts());
    }

    /// <summary>
    /// 商品カテゴリ関連の初期データを設定
    /// </summary>
    /// <param name="modelBuilder">Entity Framework の ModelBuilder インスタンス</param>
    /// <remarks>
    /// 商品とカテゴリの多対多関係を設定
    /// 複合主キー（ProductId, CategoryId）による関連管理
    /// 最下層カテゴリのみに関連付けることを推奨
    /// </remarks>
    private static void SeedProductCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductCategory>().HasData(ProductCategorySeeder.GetProductCategories());
    }

    /// <summary>
    /// 商品画像の初期データを設定
    /// </summary>
    /// <param name="modelBuilder">Entity Framework の ModelBuilder インスタンス</param>
    /// <remarks>
    /// 商品画像データを設定（商品あたり最大5枚）
    /// メイン画像フラグとサブ画像の区別を含む
    /// 表示順序による画像の並び順を管理
    /// </remarks>
    private static void SeedProductImages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductImage>().HasData(ProductImageSeeder.GetProductImages());
    }

    /// <summary>
    /// 初期データの整合性チェック
    /// </summary>
    /// <returns>整合性チェック結果</returns>
    /// <remarks>
    /// 開発時のデバッグ用メソッド
    /// 以下の整合性をチェック：
    /// - 商品に指定されたCategoryIdが存在するか
    /// - ProductCategoriesの外部キーが有効か
    /// - ProductImagesの外部キーが有効か
    /// - メイン画像が商品あたり最大1つか
    /// </remarks>
    public static (bool IsValid, string[] Errors) ValidateDataIntegrity()
    {
        var errors = new List<string>();
        
        var categories = CategorySeeder.GetCategories();
        var products = ProductSeeder.GetProducts();
        var productCategories = ProductCategorySeeder.GetProductCategories();
        var productImages = ProductImageSeeder.GetProductImages();
        
        var categoryIds = categories.Select(c => c.Id).ToHashSet();
        var productIds = products.Select(p => p.Id).ToHashSet();

        // 商品のCategoryIdチェック
        foreach (var product in products)
        {
            if (!categoryIds.Contains(product.CategoryId))
            {
                errors.Add($"Product ID {product.Id} references non-existent Category ID {product.CategoryId}");
            }
        }

        // ProductCategoriesの外部キーチェック
        foreach (var pc in productCategories)
        {
            if (!productIds.Contains(pc.ProductId))
            {
                errors.Add($"ProductCategory references non-existent Product ID {pc.ProductId}");
            }
            if (!categoryIds.Contains(pc.CategoryId))
            {
                errors.Add($"ProductCategory references non-existent Category ID {pc.CategoryId}");
            }
        }

        // ProductImagesの外部キーチェック
        foreach (var pi in productImages)
        {
            if (!productIds.Contains(pi.ProductId))
            {
                errors.Add($"ProductImage ID {pi.Id} references non-existent Product ID {pi.ProductId}");
            }
        }

        // メイン画像の重複チェック
        var mainImageCounts = productImages
            .Where(pi => pi.IsMain)
            .GroupBy(pi => pi.ProductId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var productId in mainImageCounts)
        {
            errors.Add($"Product ID {productId} has multiple main images");
        }

        // 表示順序の重複チェック
        var duplicateDisplayOrders = productImages
            .GroupBy(pi => new { pi.ProductId, pi.DisplayOrder })
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicate in duplicateDisplayOrders)
        {
            errors.Add($"Product ID {duplicate.ProductId} has duplicate DisplayOrder {duplicate.DisplayOrder}");
        }

        return (errors.Count == 0, errors.ToArray());
    }

    /// <summary>
    /// 初期データの統計情報を取得
    /// </summary>
    /// <returns>初期データ統計</returns>
    /// <remarks>
    /// 開発時の確認用メソッド
    /// 各エンティティのデータ件数と基本統計を提供
    /// </remarks>
    public static object GetDataStatistics()
    {
        var categories = CategorySeeder.GetCategories();
        var products = ProductSeeder.GetProducts();
        var productCategories = ProductCategorySeeder.GetProductCategories();
        var productImages = ProductImageSeeder.GetProductImages();

        return new
        {
            Categories = new
            {
                Total = categories.Length,
                ByLevel = categories.GroupBy(c => c.Level).ToDictionary(g => $"Level{g.Key}", g => g.Count())
            },
            Products = new
            {
                Total = products.Length,
                ByStatus = products.GroupBy(p => p.Status).ToDictionary(g => g.Key.ToString(), g => g.Count()),
                AveragePrice = products.Average(p => p.Price),
                PriceRange = new { Min = products.Min(p => p.Price), Max = products.Max(p => p.Price) }
            },
            ProductCategories = new
            {
                Total = productCategories.Length,
                MultiCategoryProducts = productCategories.GroupBy(pc => pc.ProductId).Where(g => g.Count() > 1).Count()
            },
            ProductImages = new
            {
                Total = productImages.Length,
                ProductsWithImages = productImages.Select(pi => pi.ProductId).Distinct().Count(),
                AverageImagesPerProduct = (double)productImages.Length / productImages.Select(pi => pi.ProductId).Distinct().Count(),
                MainImages = productImages.Count(pi => pi.IsMain)
            }
        };
    }
}