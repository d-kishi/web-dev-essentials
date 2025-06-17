using Web.Essentials.Domain.Entities;

namespace Web.Essentials.Infrastructure.Data.Seeds;

/// <summary>
/// 商品画像エンティティの初期データ生成クラス
/// </summary>
/// <remarks>
/// スポーツ関連商品の画像データを提供
/// - 商品あたり最大5枚の画像を管理
/// - メイン画像（IsMain = true）とサブ画像の区別
/// - 表示順序による画像の並び順制御
/// - アクセシビリティ対応の代替テキスト設定
/// </remarks>
public static class ProductImageSeeder
{
    /// <summary>
    /// スポーツ関連商品画像の初期データを取得
    /// </summary>
    /// <returns>商品画像エンティティの配列</returns>
    public static ProductImage[] GetProductImages()
    {
        return new ProductImage[]
        {
            // ============================================
            // プロモデル硬式バット (ProductId: 1) の画像
            // ============================================
            new() 
            { 
                Id = 1, 
                ProductId = 1, 
                ImagePath = "/uploads/products/baseball_bat_main.jpg", 
                DisplayOrder = 1, 
                AltText = "硬式バットメイン画像", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 2, 
                ProductId = 1, 
                ImagePath = "/uploads/products/baseball_bat_grip.jpg", 
                DisplayOrder = 2, 
                AltText = "硬式バットグリップ部分", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 3, 
                ProductId = 1, 
                ImagePath = "/uploads/products/baseball_bat_logo.jpg", 
                DisplayOrder = 3, 
                AltText = "硬式バットロゴ部分", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // 内野手用グローブ (ProductId: 2) の画像
            // ============================================
            new() 
            { 
                Id = 4, 
                ProductId = 2, 
                ImagePath = "/uploads/products/baseball_glove_main.jpg", 
                DisplayOrder = 1, 
                AltText = "内野手グローブメイン", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 5, 
                ProductId = 2, 
                ImagePath = "/uploads/products/baseball_glove_back.jpg", 
                DisplayOrder = 2, 
                AltText = "内野手グローブ背面", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 6, 
                ProductId = 2, 
                ImagePath = "/uploads/products/baseball_glove_palm.jpg", 
                DisplayOrder = 3, 
                AltText = "内野手グローブ手のひら", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // 硬式野球ボール (ProductId: 3) の画像
            // ============================================
            new() 
            { 
                Id = 7, 
                ProductId = 3, 
                ImagePath = "/uploads/products/baseball_ball.jpg", 
                DisplayOrder = 1, 
                AltText = "硬式野球ボール", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // 野球ユニフォーム上下セット (ProductId: 4) の画像
            // ============================================
            new() 
            { 
                Id = 8, 
                ProductId = 4, 
                ImagePath = "/uploads/products/baseball_uniform_set.jpg", 
                DisplayOrder = 1, 
                AltText = "野球ユニフォームセット", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 9, 
                ProductId = 4, 
                ImagePath = "/uploads/products/baseball_uniform_top.jpg", 
                DisplayOrder = 2, 
                AltText = "野球ユニフォームシャツ", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // HGサッカースパイク (ProductId: 5) の画像
            // ============================================
            new() 
            { 
                Id = 10, 
                ProductId = 5, 
                ImagePath = "/uploads/products/soccer_hg_spikes_main.jpg", 
                DisplayOrder = 1, 
                AltText = "HGサッカースパイクメイン", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 11, 
                ProductId = 5, 
                ImagePath = "/uploads/products/soccer_hg_spikes_sole.jpg", 
                DisplayOrder = 2, 
                AltText = "HGサッカースパイク靴底", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 12, 
                ProductId = 5, 
                ImagePath = "/uploads/products/soccer_hg_spikes_side.jpg", 
                DisplayOrder = 3, 
                AltText = "HGサッカースパイクサイド", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // サッカーボール5号球 (ProductId: 6) の画像
            // ============================================
            new() 
            { 
                Id = 13, 
                ProductId = 6, 
                ImagePath = "/uploads/products/soccer_ball_5.jpg", 
                DisplayOrder = 1, 
                AltText = "サッカーボール5号球", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 14, 
                ProductId = 6, 
                ImagePath = "/uploads/products/soccer_ball_logo.jpg", 
                DisplayOrder = 2, 
                AltText = "サッカーボールFIFAロゴ", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // サッカーユニフォームセット (ProductId: 7) の画像
            // ============================================
            new() 
            { 
                Id = 15, 
                ProductId = 7, 
                ImagePath = "/uploads/products/soccer_uniform_set.jpg", 
                DisplayOrder = 1, 
                AltText = "サッカーユニフォームセット", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // ゴールキーパーグローブ (ProductId: 8) の画像
            // ============================================
            new() 
            { 
                Id = 16, 
                ProductId = 8, 
                ImagePath = "/uploads/products/goalkeeper_gloves.jpg", 
                DisplayOrder = 1, 
                AltText = "ゴールキーパーグローブ", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 17, 
                ProductId = 8, 
                ImagePath = "/uploads/products/goalkeeper_gloves_grip.jpg", 
                DisplayOrder = 2, 
                AltText = "ゴールキーパーグローブグリップ", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // バスケットボールシューズハイカット (ProductId: 9) の画像
            // ============================================
            new() 
            { 
                Id = 18, 
                ProductId = 9, 
                ImagePath = "/uploads/products/basketball_highcut_main.jpg", 
                DisplayOrder = 1, 
                AltText = "バスケットボールシューズメイン", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 19, 
                ProductId = 9, 
                ImagePath = "/uploads/products/basketball_highcut_sole.jpg", 
                DisplayOrder = 2, 
                AltText = "バスケットボールシューズ靴底", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // バスケットボール7号球 (ProductId: 10) の画像
            // ============================================
            new() 
            { 
                Id = 20, 
                ProductId = 10, 
                ImagePath = "/uploads/products/basketball_7.jpg", 
                DisplayOrder = 1, 
                AltText = "バスケットボール7号球", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // 中長距離用ランニングシューズ (ProductId: 15) の画像
            // ============================================
            new() 
            { 
                Id = 21, 
                ProductId = 15, 
                ImagePath = "/uploads/products/running_road_main.jpg", 
                DisplayOrder = 1, 
                AltText = "ロードランニングシューズメイン", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 22, 
                ProductId = 15, 
                ImagePath = "/uploads/products/running_road_sole.jpg", 
                DisplayOrder = 2, 
                AltText = "ロードランニングシューズ靴底", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 23, 
                ProductId = 15, 
                ImagePath = "/uploads/products/running_road_side.jpg", 
                DisplayOrder = 3, 
                AltText = "ロードランニングシューズサイド", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // 短距離スプリントシューズ (ProductId: 16) の画像
            // ============================================
            new() 
            { 
                Id = 24, 
                ProductId = 16, 
                ImagePath = "/uploads/products/sprint_shoes_main.jpg", 
                DisplayOrder = 1, 
                AltText = "短距離スプリントシューズメイン", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 25, 
                ProductId = 16, 
                ImagePath = "/uploads/products/sprint_shoes_spikes.jpg", 
                DisplayOrder = 2, 
                AltText = "短距離スプリントシューズスパイク", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // ランニングシャツ半袖 (ProductId: 18) の画像
            // ============================================
            new() 
            { 
                Id = 26, 
                ProductId = 18, 
                ImagePath = "/uploads/products/running_shirt_main.jpg", 
                DisplayOrder = 1, 
                AltText = "ランニングシャツ半袖メイン", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 27, 
                ProductId = 18, 
                ImagePath = "/uploads/products/running_shirt_back.jpg", 
                DisplayOrder = 2, 
                AltText = "ランニングシャツ半袖背面", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // GPSランニングウォッチ (ProductId: 20) の画像
            // ============================================
            new() 
            { 
                Id = 28, 
                ProductId = 20, 
                ImagePath = "/uploads/products/gps_watch_main.jpg", 
                DisplayOrder = 1, 
                AltText = "GPSランニングウォッチメイン", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 29, 
                ProductId = 20, 
                ImagePath = "/uploads/products/gps_watch_display.jpg", 
                DisplayOrder = 2, 
                AltText = "GPSランニングウォッチディスプレイ", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 30, 
                ProductId = 20, 
                ImagePath = "/uploads/products/gps_watch_side.jpg", 
                DisplayOrder = 3, 
                AltText = "GPSランニングウォッチサイド", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            
            // ============================================
            // HG/AG兼用サッカースパイク (ProductId: 21) の画像
            // ============================================
            new() 
            { 
                Id = 31, 
                ProductId = 21, 
                ImagePath = "/uploads/products/soccer_hg_ag_main.jpg", 
                DisplayOrder = 1, 
                AltText = "HG/AG兼用サッカースパイクメイン", 
                IsMain = true, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            },
            new() 
            { 
                Id = 32, 
                ProductId = 21, 
                ImagePath = "/uploads/products/soccer_hg_ag_sole.jpg", 
                DisplayOrder = 2, 
                AltText = "HG/AG兼用サッカースパイク靴底", 
                IsMain = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            }
        };
    }
}