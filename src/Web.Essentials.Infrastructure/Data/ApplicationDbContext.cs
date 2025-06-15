using Microsoft.EntityFrameworkCore;
using Web.Essentials.Domain.Entities;

namespace Web.Essentials.Infrastructure.Data;

/// <summary>
/// アプリケーションデータベースコンテキスト - Entity Framework Core DbContext の実装
/// </summary>
/// <remarks>
/// このクラスはデータベースとのやり取りを管理し、以下の特徴を持つ：
/// - InMemoryデータベースプロバイダーの使用（教育目的）
/// - エンティティの設定とマッピング
/// - 初期データ（Seed Data）の設定
/// - 監査フィールドの自動更新
/// 
/// 本プロジェクトでは永続化は行わず、アプリケーション再起動でデータがリセットされる
/// </remarks>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// ApplicationDbContext のコンストラクタ
    /// </summary>
    /// <param name="options">DbContext のオプション設定</param>
    /// <remarks>
    /// 依存性注入によってオプションが提供される
    /// InMemoryデータベースの設定は Program.cs で行う
    /// </remarks>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 商品エンティティのDbSet
    /// </summary>
    /// <remarks>
    /// 商品データへのアクセスポイント
    /// CRUD操作やクエリの起点となる
    /// </remarks>
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// カテゴリエンティティのDbSet
    /// </summary>
    /// <remarks>
    /// カテゴリデータへのアクセスポイント
    /// 階層構造の管理を含む
    /// </remarks>
    public DbSet<Category> Categories { get; set; } = null!;

    /// <summary>
    /// 商品カテゴリ関連エンティティのDbSet
    /// </summary>
    /// <remarks>
    /// 商品とカテゴリの多対多関係を管理
    /// 現在は単純な1対多関係だが、将来的な拡張に備える
    /// </remarks>
    public DbSet<ProductCategory> ProductCategories { get; set; } = null!;

    /// <summary>
    /// 商品画像エンティティのDbSet
    /// </summary>
    /// <remarks>
    /// 商品に関連する画像データを管理
    /// 表示順序や画像メタデータを含む
    /// </remarks>
    public DbSet<ProductImage> ProductImages { get; set; } = null!;

    /// <summary>
    /// モデル作成時の設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    /// <remarks>
    /// エンティティの設定、関係性の定義、初期データの設定を行う
    /// InMemoryデータベース用にシンプルな設定とする
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 商品エンティティの設定
        ConfigureProduct(modelBuilder);

        // カテゴリエンティティの設定
        ConfigureCategory(modelBuilder);

        // 商品カテゴリエンティティの設定
        ConfigureProductCategory(modelBuilder);

        // 商品画像エンティティの設定
        ConfigureProductImage(modelBuilder);

        // 初期データの設定
        SeedData(modelBuilder);
    }

    /// <summary>
    /// 商品エンティティの設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    private static void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            // 主キー設定
            entity.HasKey(e => e.Id);

            // プロパティ設定
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.JanCode)
                .HasMaxLength(13);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // 関係性設定
            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// カテゴリエンティティの設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            // 主キー設定
            entity.HasKey(e => e.Id);

            // プロパティ設定
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();
        });
    }

    /// <summary>
    /// 商品カテゴリエンティティの設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    private static void ConfigureProductCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            // 複合主キー設定
            entity.HasKey(e => new { e.ProductId, e.CategoryId });

            // プロパティ設定
            entity.Property(e => e.CreatedAt)
                .IsRequired();

            // 関係性設定
            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// 商品画像エンティティの設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    private static void ConfigureProductImage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductImage>(entity =>
        {
            // 主キー設定
            entity.HasKey(e => e.Id);

            // プロパティ設定
            entity.Property(e => e.ImagePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.DisplayOrder)
                .IsRequired();

            entity.Property(e => e.AltText)
                .HasMaxLength(200);

            entity.Property(e => e.IsMain)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // 関係性設定
            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// 初期データの設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    /// <remarks>
    /// アプリケーション起動時に自動的に投入される初期データ
    /// 開発・テスト用のサンプルデータを提供
    /// </remarks>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // カテゴリ初期データ
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = 1,
                Name = "電子機器",
                Description = "コンピューター、スマートフォン、タブレットなどの電子機器",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Category
            {
                Id = 2,
                Name = "書籍",
                Description = "小説、技術書、雑誌などの書籍類",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Category
            {
                Id = 3,
                Name = "衣類",
                Description = "シャツ、パンツ、靴などの衣類・アクセサリー",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Category
            {
                Id = 4,
                Name = "食品",
                Description = "食品、飲料、お菓子などの食べ物",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Category
            {
                Id = 5,
                Name = "生活用品",
                Description = "日用品、清掃用品、文房具などの生活必需品",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        );

        // 商品初期データ
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "ノートパソコン ThinkPad X1",
                Description = "高性能なビジネス向けノートパソコン。軽量で持ち運びに便利。",
                Price = 150000m,
                CategoryId = 1,
                JanCode = "4901234567890",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = 2,
                Name = "プログラミング入門書",
                Description = "初心者向けのプログラミング学習書。C#の基礎から応用まで。",
                Price = 3500m,
                CategoryId = 2,
                JanCode = "4901234567891",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = 3,
                Name = "コットンTシャツ",
                Description = "100%コットンの快適なTシャツ。カジュアルからビジネスカジュアルまで。",
                Price = 2800m,
                CategoryId = 3,
                JanCode = "4901234567892",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = 4,
                Name = "有機コーヒー豆",
                Description = "南米産の有機栽培コーヒー豆。深いコクと豊かな香り。",
                Price = 1200m,
                CategoryId = 4,
                JanCode = "4901234567893",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = 5,
                Name = "多機能ボールペン",
                Description = "黒・赤・青のインクとシャープペンシル機能を備えた便利なペン。",
                Price = 800m,
                CategoryId = 5,
                JanCode = "4901234567894",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        );
    }

    /// <summary>
    /// 変更追跡の自動更新
    /// </summary>
    /// <returns>保存された変更の数</returns>
    /// <remarks>
    /// SaveChangesが呼ばれた際に、CreatedAt と UpdatedAt を自動更新
    /// 新規作成時は両方、更新時は UpdatedAt のみを現在時刻に設定
    /// </remarks>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// 変更追跡の自動更新（非同期版）
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>保存された変更の数</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// タイムスタンプの自動更新処理
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Product product)
            {
                if (entry.State == EntityState.Added)
                {
                    product.CreatedAt = DateTime.Now;
                }
                product.UpdatedAt = DateTime.Now;
            }
            else if (entry.Entity is Category category)
            {
                if (entry.State == EntityState.Added)
                {
                    category.CreatedAt = DateTime.Now;
                }
                category.UpdatedAt = DateTime.Now;
            }
            else if (entry.Entity is ProductCategory productCategory)
            {
                if (entry.State == EntityState.Added)
                {
                    productCategory.CreatedAt = DateTime.Now;
                }
            }
            else if (entry.Entity is ProductImage productImage)
            {
                if (entry.State == EntityState.Added)
                {
                    productImage.CreatedAt = DateTime.Now;
                }
                productImage.UpdatedAt = DateTime.Now;
            }
        }
    }
}