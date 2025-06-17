using Microsoft.EntityFrameworkCore;
using Web.Essentials.Domain.Entities;
using Web.Essentials.Infrastructure.Data.Seeds;

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
                .IsRequired();

            entity.Property(e => e.JanCode)
                .HasMaxLength(13);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // インデックスとユニーク制約
            entity.HasIndex(e => e.JanCode)
                .IsUnique(); // JANコード重複不可

            // 関係性設定（多対多関係はProductCategoryで管理）
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

            // 自己参照関係設定（階層構造）
            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ユニーク制約（カテゴリ名重複不可）
            entity.HasIndex(e => e.Name)
                .IsUnique();
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
            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ユニーク制約とインデックス
            entity.HasIndex(e => new { e.ProductId, e.DisplayOrder })
                .IsUnique(); // 同一商品でDisplayOrderの重複不可

            entity.HasIndex(e => new { e.ProductId, e.IsMain }); // メイン画像のインデックス
        });
    }

    /// <summary>
    /// 初期データの設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    /// <remarks>
    /// スポーツ用品特化の初期データを外部ファイルから読み込み
    /// MasterDataSeeder クラスを使用して一元管理された初期データを設定
    /// 保守性と可読性を向上させるため、データ定義をシーダークラスに分離
    /// </remarks>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // 外部シーダークラスを使用してすべての初期データを設定
        MasterDataSeeder.SeedAll(modelBuilder);
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