using Web.Essentials.Domain.Entities;

namespace Web.Essentials.Infrastructure.Data.Seeds;

/// <summary>
/// カテゴリエンティティの初期データ生成クラス
/// </summary>
/// <remarks>
/// スポーツ用品特化の3階層カテゴリ構造を提供
/// - Level 0: 競技別（野球、サッカー、バスケットボール等）
/// - Level 1: 用品種別（シューズ、ウェア、ボール等）
/// - Level 2: 詳細分類（短距離用、中距離用、長距離用等）
/// </remarks>
public static class CategorySeeder
{
    /// <summary>
    /// スポーツカテゴリの初期データを取得
    /// </summary>
    /// <returns>カテゴリエンティティの配列</returns>
    public static Category[] GetCategories()
    {
        return new Category[]
        {
            // ============================================
            // ルートカテゴリ (Level 0) - 競技別
            // ============================================
            new() { Id = 1, Name = "野球", Description = "野球関連用品", ParentCategoryId = null, Level = 0, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "サッカー", Description = "サッカー関連用品", ParentCategoryId = null, Level = 0, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, Name = "バスケットボール", Description = "バスケットボール関連用品", ParentCategoryId = null, Level = 0, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, Name = "バレーボール", Description = "バレーボール関連用品", ParentCategoryId = null, Level = 0, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 5, Name = "ランニング", Description = "ランニング関連用品", ParentCategoryId = null, Level = 0, SortOrder = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // ============================================
            // サブカテゴリ (Level 1) - 用品種別
            // ============================================
            
            // 野球カテゴリ (Parent: 1)
            new() { Id = 6, Name = "バット", Description = "野球バット", ParentCategoryId = 1, Level = 1, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 7, Name = "グローブ", Description = "野球グローブ", ParentCategoryId = 1, Level = 1, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 8, Name = "ウェア", Description = "野球ウェア", ParentCategoryId = 1, Level = 1, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 9, Name = "ボール", Description = "野球ボール", ParentCategoryId = 1, Level = 1, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // サッカーカテゴリ (Parent: 2)
            new() { Id = 10, Name = "シューズ", Description = "サッカーシューズ", ParentCategoryId = 2, Level = 1, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 11, Name = "ウェア", Description = "サッカーウェア", ParentCategoryId = 2, Level = 1, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 12, Name = "ボール", Description = "サッカーボール", ParentCategoryId = 2, Level = 1, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 13, Name = "ゴールキーパー用品", Description = "ゴールキーパー用品", ParentCategoryId = 2, Level = 1, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // バスケットボールカテゴリ (Parent: 3)
            new() { Id = 14, Name = "シューズ", Description = "バスケットボールシューズ", ParentCategoryId = 3, Level = 1, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 15, Name = "ウェア", Description = "バスケットボールウェア", ParentCategoryId = 3, Level = 1, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 16, Name = "ボール", Description = "バスケットボール", ParentCategoryId = 3, Level = 1, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // バレーボールカテゴリ (Parent: 4)
            new() { Id = 17, Name = "シューズ", Description = "バレーボールシューズ", ParentCategoryId = 4, Level = 1, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 18, Name = "ウェア", Description = "バレーボールウェア", ParentCategoryId = 4, Level = 1, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 19, Name = "ボール", Description = "バレーボール", ParentCategoryId = 4, Level = 1, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // ランニングカテゴリ (Parent: 5)
            new() { Id = 20, Name = "シューズ", Description = "ランニングシューズ", ParentCategoryId = 5, Level = 1, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 21, Name = "ウェア", Description = "ランニングウェア", ParentCategoryId = 5, Level = 1, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 22, Name = "アクセサリー", Description = "ランニングアクセサリー", ParentCategoryId = 5, Level = 1, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // ============================================
            // 詳細カテゴリ (Level 2) - 詳細分類
            // ============================================
            
            // 野球バット (Parent: 6)
            new() { Id = 23, Name = "硬式バット", Description = "硬式野球用バット", ParentCategoryId = 6, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 24, Name = "軟式バット", Description = "軟式野球用バット", ParentCategoryId = 6, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // 野球グローブ (Parent: 7)
            new() { Id = 25, Name = "内野手用", Description = "内野手用グローブ", ParentCategoryId = 7, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 26, Name = "外野手用", Description = "外野手用グローブ", ParentCategoryId = 7, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 27, Name = "捕手用", Description = "捕手用グローブ", ParentCategoryId = 7, Level = 2, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // サッカーシューズ (Parent: 10)
            new() { Id = 28, Name = "HGシューズ", Description = "ハードグラウンド用シューズ", ParentCategoryId = 10, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 29, Name = "AGシューズ", Description = "人工芝用シューズ", ParentCategoryId = 10, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 30, Name = "FGシューズ", Description = "天然芝用シューズ", ParentCategoryId = 10, Level = 2, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 42, Name = "TFシューズ", Description = "ターフ用シューズ（小さなコート用）", ParentCategoryId = 10, Level = 2, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // バスケットボールシューズ (Parent: 14)
            new() { Id = 31, Name = "ハイカット", Description = "ハイカットシューズ", ParentCategoryId = 14, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 32, Name = "ローカット", Description = "ローカットシューズ", ParentCategoryId = 14, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // ランニングシューズ (Parent: 20)
            new() { Id = 33, Name = "短距離用", Description = "短距離ランニング用シューズ", ParentCategoryId = 20, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 34, Name = "中距離用", Description = "中距離ランニング用シューズ", ParentCategoryId = 20, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 38, Name = "長距離用", Description = "長距離ランニング用シューズ", ParentCategoryId = 20, Level = 2, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 39, Name = "トレイルラン用", Description = "トレイルランニング用シューズ", ParentCategoryId = 20, Level = 2, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // ランニングウェア (Parent: 21)
            new() { Id = 35, Name = "半袖", Description = "ランニング半袖シャツ", ParentCategoryId = 21, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 36, Name = "長袖", Description = "ランニング長袖シャツ", ParentCategoryId = 21, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 37, Name = "タイツ", Description = "ランニングタイツ", ParentCategoryId = 21, Level = 2, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            
            // ランニングアクセサリー (Parent: 22)
            new() { Id = 40, Name = "GPSウォッチ", Description = "GPS機能付きランニングウォッチ", ParentCategoryId = 22, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 41, Name = "ランニングポーチ", Description = "ランニング用ポーチ・ベルト", ParentCategoryId = 22, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
    }
}