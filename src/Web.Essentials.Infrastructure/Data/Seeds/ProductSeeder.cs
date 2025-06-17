using Web.Essentials.Domain.Entities;

namespace Web.Essentials.Infrastructure.Data.Seeds;

/// <summary>
/// 商品エンティティの初期データ生成クラス
/// </summary>
/// <remarks>
/// スポーツ用品に特化した商品データを提供
/// - 野球、サッカー、バスケットボール、バレーボール、ランニング関連商品
/// - 価格はuint型に対応した範囲内で設定
/// - 商品ステータス（販売開始前、販売中、取扱終了）を含む
/// - JANコードはダミーデータとして設定
/// </remarks>
public static class ProductSeeder
{
    /// <summary>
    /// スポーツ関連商品の初期データを取得
    /// </summary>
    /// <returns>商品エンティティの配列</returns>
    public static Product[] GetProducts()
    {
        return new Product[]
        {
            // ============================================
            // 野球関連商品
            // ============================================
            new() 
            { 
                Id = 1, 
                Name = "プロモデル硬式バット", 
                Description = "プロ仕様の硬式野球バット　84cm　33inch", 
                Price = 15800, 
                // CategoryId = 23, // 硬式バット（Level 2）
                JanCode = "4901001001001", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 2, 
                Name = "内野手用グローブ", 
                Description = "高品質牛革製内野手用グローブ、サイズM", 
                Price = 8900, 
                // CategoryId = 25, // 内野手用（Level 2）
                JanCode = "4901001001002", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 3, 
                Name = "硬式野球ボール", 
                Description = "公認硬式野球ボール、1ダース", 
                Price = 580, 
                // CategoryId = 9, // ボール（野球）（Level 1）
                JanCode = "4901001001003", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 4, 
                Name = "野球ユニフォーム上下セット", 
                Description = "チーム用野球ユニフォーム上下セット、サイズL", 
                Price = 12000, 
                // CategoryId = 8, // ウェア（野球）（Level 1）
                JanCode = "4901001001004", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // サッカー関連商品
            // ============================================
            new() 
            { 
                Id = 5, 
                Name = "HGサッカースパイク", 
                Description = "ハードグラウンド用サッカースパイク　25.5cm", 
                Price = 8900, 
                // CategoryId = 28, // HGシューズ（Level 2）
                JanCode = "4901002002001", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 6, 
                Name = "サッカーボール　5号球", 
                Description = "公認サッカーボール　5号球、FIFA公認", 
                Price = 3200, 
                // CategoryId = 12, // ボール（サッカー）（Level 1）
                JanCode = "4901002002002", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 7, 
                Name = "サッカーユニフォームセット", 
                Description = "チーム用サッカーユニフォーム上下セット、サイズM", 
                Price = 9800, 
                // CategoryId = 11, // ウェア（サッカー）（Level 1）
                JanCode = "4901002002003", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 8, 
                Name = "ゴールキーパーグローブ", 
                Description = "プロ仕様ゴールキーパーグローブ、サイズ9", 
                Price = 4500, 
                // CategoryId = 13, // ゴールキーパー用品（Level 1）
                JanCode = "4901002002004", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 21, 
                Name = "HG/AG兼用サッカースパイク", 
                Description = "ハードグラウンド・人工芝対応シューズ　25.5cm", 
                Price = 11800, 
                // CategoryId = 28, // HGシューズ（Level 2）※複数カテゴリはProductCategoryで管理
                JanCode = "4901002002005", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // バスケットボール関連商品
            // ============================================
            new() 
            { 
                Id = 9, 
                Name = "バスケットボールシューズハイカット", 
                Description = "プロモデルバスケットボールシューズ　27.0cm", 
                Price = 12800, 
                // CategoryId = 31, // ハイカット（Level 2）
                JanCode = "4901003003001", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 10, 
                Name = "バスケットボール　7号球", 
                Description = "公認バスケットボール　7号球、室内用", 
                Price = 4800, 
                // CategoryId = 16, // ボール（バスケットボール）（Level 1）
                JanCode = "4901003003002", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 11, 
                Name = "バスケットボールユニフォーム", 
                Description = "チーム用バスケットボールユニフォーム上下セット", 
                Price = 11000, 
                // CategoryId = 15, // ウェア（バスケットボール）（Level 1）
                JanCode = "4901003003003", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 22, 
                Name = "レガシーバスケットボールシューズ", 
                Description = "旧モデルバスケットボールシューズ、在庫限り", 
                Price = 6800, 
                // CategoryId = 32, // ローカット（Level 2）
                JanCode = "4901003003099", 
                Status = ProductStatus.Discontinued, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // バレーボール関連商品
            // ============================================
            new() 
            { 
                Id = 12, 
                Name = "バレーボールシューズ", 
                Description = "屋内用バレーボールシューズ　25.0cm", 
                Price = 7800, 
                // CategoryId = 17, // シューズ（バレーボール）（Level 1）
                JanCode = "4901004004001", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 13, 
                Name = "バレーボール　5号球", 
                Description = "公認バレーボール　5号球、室内用", 
                Price = 3800, 
                // CategoryId = 19, // ボール（バレーボール）（Level 1）
                JanCode = "4901004004002", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 14, 
                Name = "バレーボールユニフォーム", 
                Description = "チーム用バレーボールユニフォーム上下セット", 
                Price = 9500, 
                // CategoryId = 18, // ウェア（バレーボール）（Level 1）
                JanCode = "4901004004003", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // ランニング関連商品
            // ============================================
            new() 
            { 
                Id = 15, 
                Name = "中長距離用ランニングシューズ", 
                Description = "中距離・長距離対応ランニングシューズ　26.5cm", 
                Price = 12800, 
                // CategoryId = 34, // 中距離用（Level 2）※複数カテゴリはProductCategoryで管理
                JanCode = "4901005005001", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 16, 
                Name = "短距離スプリントシューズ", 
                Description = "短距離スプリント用シューズ　27.0cm", 
                Price = 15600, 
                // CategoryId = 33, // 短距離用（Level 2）
                JanCode = "4901005005002", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 17, 
                Name = "トレイルランニングシューズ", 
                Description = "トレイルランニング専用シューズ　26.0cm", 
                Price = 11200, 
                // CategoryId = 39, // トレイルラン用（Level 2）
                JanCode = "4901005005003", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 18, 
                Name = "ランニングシャツ半袖", 
                Description = "速乾機能付きランニングシャツ、サイズM", 
                Price = 3200, 
                // CategoryId = 35, // 半袖（Level 2）
                JanCode = "4901005005004", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 19, 
                Name = "ランニングタイツ", 
                Description = "コンプレッション機能付きランニングタイツ", 
                Price = 4800, 
                // CategoryId = 37, // タイツ（Level 2）
                JanCode = "4901005005005", 
                Status = ProductStatus.OnSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 20, 
                Name = "GPSランニングウォッチ", 
                Description = "高機能GPS付きランニングウォッチ", 
                Price = 28000, 
                // CategoryId = 40, // GPSウォッチ（Level 2）
                JanCode = "4901005005006", 
                Status = ProductStatus.PreSale, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // 販売終了商品の例
            // ============================================
            new() 
            { 
                Id = 23, 
                Name = "ヴィンテージサッカーボール", 
                Description = "コレクターアイテム、限定版サッカーボール", 
                Price = 15000, 
                // CategoryId = 12, // ボール（サッカー）（Level 1）
                JanCode = "4901002002099", 
                Status = ProductStatus.Discontinued, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            }
        };
    }
}