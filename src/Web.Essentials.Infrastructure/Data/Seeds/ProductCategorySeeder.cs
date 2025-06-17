using Web.Essentials.Domain.Entities;

namespace Web.Essentials.Infrastructure.Data.Seeds;

/// <summary>
/// 商品カテゴリ関連エンティティの初期データ生成クラス
/// </summary>
/// <remarks>
/// 商品とカテゴリの多対多関係を定義
/// - 最下層カテゴリのみに関連付けることを原則とする
/// - 1つの商品が複数のカテゴリに属する場合の対応
/// - 階層の上位カテゴリへの関連は検索時に自動推論される
/// </remarks>
public static class ProductCategorySeeder
{
    /// <summary>
    /// 商品カテゴリ関連の初期データを取得
    /// </summary>
    /// <returns>商品カテゴリ関連エンティティの配列</returns>
    public static ProductCategory[] GetProductCategories()
    {
        return new ProductCategory[]
        {
            // ============================================
            // 野球関連商品のカテゴリ関連
            // ============================================
            
            // プロモデル硬式バット (ID: 1) → 硬式バット (ID: 23, Level 2)
            new() { ProductId = 1, CategoryId = 23, CreatedAt = DateTime.UtcNow },
            
            // 内野手用グローブ (ID: 2) → 内野手用 (ID: 25, Level 2)
            new() { ProductId = 2, CategoryId = 25, CreatedAt = DateTime.UtcNow },
            
            // 硬式野球ボール (ID: 3) → ボール（野球） (ID: 9, Level 1)
            // ※ボールカテゴリはLevel 1で最下層
            new() { ProductId = 3, CategoryId = 9, CreatedAt = DateTime.UtcNow },
            
            // 野球ユニフォーム上下セット (ID: 4) → ウェア（野球） (ID: 8, Level 1)
            // ※ウェアカテゴリはLevel 1で最下層
            new() { ProductId = 4, CategoryId = 8, CreatedAt = DateTime.UtcNow },
            
            // ============================================
            // サッカー関連商品のカテゴリ関連
            // ============================================
            
            // HGサッカースパイク (ID: 5) → HGシューズ (ID: 28, Level 2)
            new() { ProductId = 5, CategoryId = 28, CreatedAt = DateTime.UtcNow },
            
            // サッカーボール5号球 (ID: 6) → ボール（サッカー） (ID: 12, Level 1)
            // ※ボールカテゴリはLevel 1で最下層
            new() { ProductId = 6, CategoryId = 12, CreatedAt = DateTime.UtcNow },
            
            // サッカーユニフォームセット (ID: 7) → ウェア（サッカー） (ID: 11, Level 1)
            // ※ウェアカテゴリはLevel 1で最下層
            new() { ProductId = 7, CategoryId = 11, CreatedAt = DateTime.UtcNow },
            
            // ゴールキーパーグローブ (ID: 8) → ゴールキーパー用品 (ID: 13, Level 1)
            // ※ゴールキーパー用品カテゴリはLevel 1で最下層
            new() { ProductId = 8, CategoryId = 13, CreatedAt = DateTime.UtcNow },
            
            // HG/AG兼用サッカースパイク (ID: 21) → 複数カテゴリに関連付け
            new() { ProductId = 21, CategoryId = 28, CreatedAt = DateTime.UtcNow }, // HGシューズ
            new() { ProductId = 21, CategoryId = 29, CreatedAt = DateTime.UtcNow }, // AGシューズ
            
            // ヴィンテージサッカーボール (ID: 23) → ボール（サッカー） (ID: 12, Level 1)
            new() { ProductId = 23, CategoryId = 12, CreatedAt = DateTime.UtcNow },
            
            // ============================================
            // バスケットボール関連商品のカテゴリ関連
            // ============================================
            
            // バスケットボールシューズハイカット (ID: 9) → ハイカット (ID: 31, Level 2)
            new() { ProductId = 9, CategoryId = 31, CreatedAt = DateTime.UtcNow },
            
            // バスケットボール7号球 (ID: 10) → ボール（バスケットボール） (ID: 16, Level 1)
            // ※ボールカテゴリはLevel 1で最下層
            new() { ProductId = 10, CategoryId = 16, CreatedAt = DateTime.UtcNow },
            
            // バスケットボールユニフォーム (ID: 11) → ウェア（バスケットボール） (ID: 15, Level 1)
            // ※ウェアカテゴリはLevel 1で最下層
            new() { ProductId = 11, CategoryId = 15, CreatedAt = DateTime.UtcNow },
            
            // レガシーバスケットボールシューズ (ID: 22) → ローカット (ID: 32, Level 2)
            new() { ProductId = 22, CategoryId = 32, CreatedAt = DateTime.UtcNow },
            
            // ============================================
            // バレーボール関連商品のカテゴリ関連
            // ============================================
            
            // バレーボールシューズ (ID: 12) → シューズ（バレーボール） (ID: 17, Level 1)
            // ※バレーボールのシューズカテゴリはLevel 1で最下層
            new() { ProductId = 12, CategoryId = 17, CreatedAt = DateTime.UtcNow },
            
            // バレーボール5号球 (ID: 13) → ボール（バレーボール） (ID: 19, Level 1)
            // ※ボールカテゴリはLevel 1で最下層
            new() { ProductId = 13, CategoryId = 19, CreatedAt = DateTime.UtcNow },
            
            // バレーボールユニフォーム (ID: 14) → ウェア（バレーボール） (ID: 18, Level 1)
            // ※ウェアカテゴリはLevel 1で最下層
            new() { ProductId = 14, CategoryId = 18, CreatedAt = DateTime.UtcNow },
            
            // ============================================
            // ランニング関連商品のカテゴリ関連
            // ============================================
            
            // 中長距離用ランニングシューズ (ID: 15) → 複数カテゴリに関連付け
            new() { ProductId = 15, CategoryId = 34, CreatedAt = DateTime.UtcNow }, // 中距離用 (Level 2)
            new() { ProductId = 15, CategoryId = 38, CreatedAt = DateTime.UtcNow }, // 長距離用 (Level 2)
            
            // 短距離スプリントシューズ (ID: 16) → 短距離用 (ID: 33, Level 2)
            new() { ProductId = 16, CategoryId = 33, CreatedAt = DateTime.UtcNow },
            
            // トレイルランニングシューズ (ID: 17) → トレイルラン用 (ID: 39, Level 2)
            new() { ProductId = 17, CategoryId = 39, CreatedAt = DateTime.UtcNow },
            
            // ランニングシャツ半袖 (ID: 18) → 半袖 (ID: 35, Level 2)
            new() { ProductId = 18, CategoryId = 35, CreatedAt = DateTime.UtcNow },
            
            // ランニングタイツ (ID: 19) → タイツ (ID: 37, Level 2)
            new() { ProductId = 19, CategoryId = 37, CreatedAt = DateTime.UtcNow },
            
            // GPSランニングウォッチ (ID: 20) → GPSウォッチ (ID: 40, Level 2)
            new() { ProductId = 20, CategoryId = 40, CreatedAt = DateTime.UtcNow }
        };
    }
}