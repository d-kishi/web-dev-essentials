# データベース設計仕様書

## 目次

1. [データベース概要](#1-データベース概要)
   - [1.1 データベース種別](#11-データベース種別)
   - [1.2 文字エンコーディング](#12-文字エンコーディング)
2. [テーブル設計](#2-テーブル設計)
   - [2.1 商品テーブル (Products)](#21-商品テーブル-products)
   - [2.2 カテゴリテーブル (Categories)](#22-カテゴリテーブル-categories)
   - [2.3 商品カテゴリ対照テーブル (ProductCategories)](#23-商品カテゴリ対照テーブル-productcategories)
   - [2.4 商品画像テーブル (ProductImages)](#24-商品画像テーブル-productimages)
3. [リレーションシップ](#3-リレーションシップ)
   - [3.1 外部キー制約](#31-外部キー制約)
   - [3.2 カーディナリティ](#32-カーディナリティ)
4. [バリデーションルール](#4-バリデーションルール)
   - [4.1 商品テーブル (Products)](#41-商品テーブル-products)
   - [4.2 カテゴリテーブル (Categories)](#42-カテゴリテーブル-categories)
   - [4.3 商品カテゴリ対照テーブル (ProductCategories)](#43-商品カテゴリ対照テーブル-productcategories)
   - [4.4 商品画像テーブル (ProductImages)](#44-商品画像テーブル-productimages)
5. [初期データ (Seed Data)](#5-初期データ-seed-data)
   - [5.1 カテゴリ初期データ（スポーツ特化・3階層構造）](#51-カテゴリ初期データスポーツ特化3階層構造)
   - [5.2 商品初期データ（スポーツ関連）](#52-商品初期データスポーツ関連)
   - [5.3 商品カテゴリ関連初期データ（最下層カテゴリのみ）](#53-商品カテゴリ関連初期データ最下層カテゴリのみ)
   - [5.4 商品画像初期データ（スポーツ関連）](#54-商品画像初期データスポーツ関連)
6. [Entity Framework Core 設定](#6-entity-framework-core-設定)
   - [6.1 DbContext 設定例](#61-dbcontext-設定例)
7. [パフォーマンス考慮事項](#7-パフォーマンス考慮事項)
   - [7.1 検索最適化](#71-検索最適化)
   - [7.2 ページング設定](#72-ページング設定)
8. [制約事項](#8-制約事項)
   - [8.1 削除制約](#81-削除制約)
   - [8.2 InMemoryデータベース制約](#82-inmemoryデータベース制約)

---

## 1. データベース概要

### 1.1 データベース種別
- **Entity Framework Core InMemoryデータベースプロバイダー**
- 教育目的のため永続化なし
- アプリケーション再起動時にデータリセット

### 1.2 文字エンコーディング
- **UTF-8**

## 2. テーブル設計

### 2.1 商品テーブル (Products)

| カラム名 | データ型 | NULL許可 | 主キー | 外部キー | 説明 |
|---------|---------|---------|--------|---------|------|
| Id | int | ✗ | ✓ | - | 商品ID（自動採番） |
| Name | nvarchar(100) | ✗ | - | - | 商品名 |
| Description | nvarchar(1000) | ✓ | - | - | 商品説明 |
| Price | uint | ✗ | - | - | 価格（日本円、整数） |
| JanCode | nvarchar(13) | ✓ | - | - | JANコード（13桁） |
| Status | int | ✗ | - | - | 商品ステータス（0:販売開始前、1:販売中、2:取扱終了） |
| CreatedAt | datetime2 | ✗ | - | - | 作成日時 |
| UpdatedAt | datetime2 | ✗ | - | - | 更新日時 |

#### インデックス
```sql
-- パフォーマンス向上のため
CREATE INDEX IX_Products_Name ON Products (Name);
CREATE INDEX IX_Products_JanCode ON Products (JanCode);
CREATE INDEX IX_Products_Status ON Products (Status);
```

### 2.2 カテゴリテーブル (Categories)

| カラム名 | データ型 | NULL許可 | 主キー | 外部キー | 説明 |
|---------|---------|---------|--------|---------|------|
| Id | int | ✗ | ✓ | - | カテゴリID（自動採番） |
| Name | nvarchar(50) | ✗ | - | - | カテゴリ名 |
| Description | nvarchar(500) | ✓ | - | - | カテゴリ説明 |
| ParentCategoryId | int | ✓ | - | ✓ | 親カテゴリID（階層構造用） |
| Level | int | ✗ | - | - | 階層レベル（ルート=0） |
| SortOrder | int | ✗ | - | - | 表示順序 |
| CreatedAt | datetime2 | ✗ | - | - | 作成日時 |
| UpdatedAt | datetime2 | ✗ | - | - | 更新日時 |

#### インデックス
```sql
-- カテゴリ名での検索用
CREATE INDEX IX_Categories_Name ON Categories (Name);
CREATE UNIQUE INDEX IX_Categories_Name_Unique ON Categories (Name);
-- 階層構造用
CREATE INDEX IX_Categories_ParentCategoryId ON Categories (ParentCategoryId);
CREATE INDEX IX_Categories_Level_SortOrder ON Categories (Level, SortOrder);
```

### 2.3 商品カテゴリ対照テーブル (ProductCategories)

| カラム名 | データ型 | NULL許可 | 主キー | 外部キー | 説明 |
|---------|---------|---------|--------|---------|------|
| ProductId | int | ✗ | ✓ | ✓ | 商品ID |
| CategoryId | int | ✗ | ✓ | ✓ | カテゴリID |
| CreatedAt | datetime2 | ✗ | - | - | 関連付け作成日時 |

#### インデックス
```sql
-- 複合主キー
CREATE UNIQUE INDEX IX_ProductCategories_ProductId_CategoryId ON ProductCategories (ProductId, CategoryId);
-- 逆引き検索用
CREATE INDEX IX_ProductCategories_CategoryId ON ProductCategories (CategoryId);
```

### 2.4 商品画像テーブル (ProductImages)

| カラム名 | データ型 | NULL許可 | 主キー | 外部キー | 説明 |
|---------|---------|---------|--------|---------|------|
| Id | int | ✗ | ✓ | - | 画像ID（自動採番） |
| ProductId | int | ✗ | - | ✓ | 商品ID |
| ImagePath | nvarchar(500) | ✗ | - | - | 画像ファイルパス |
| DisplayOrder | int | ✗ | - | - | 表示順序（1-5） |
| AltText | nvarchar(200) | ✓ | - | - | 画像の代替テキスト |
| IsMain | bit | ✗ | - | - | メイン画像フラグ |
| CreatedAt | datetime2 | ✗ | - | - | 作成日時 |
| UpdatedAt | datetime2 | ✗ | - | - | 更新日時 |

#### インデックス
```sql
-- 商品別画像検索用
CREATE INDEX IX_ProductImages_ProductId ON ProductImages (ProductId);
-- 表示順序用
CREATE INDEX IX_ProductImages_ProductId_DisplayOrder ON ProductImages (ProductId, DisplayOrder);
-- メイン画像検索用
CREATE INDEX IX_ProductImages_ProductId_IsMain ON ProductImages (ProductId, IsMain);
```

## 3. リレーションシップ

### 3.1 外部キー制約
```sql
-- カテゴリテーブル → カテゴリテーブル（階層構造）
ALTER TABLE Categories 
ADD CONSTRAINT FK_Categories_ParentCategory 
FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id)
ON DELETE RESTRICT;

-- 商品カテゴリ対照テーブル → 商品テーブル
ALTER TABLE ProductCategories 
ADD CONSTRAINT FK_ProductCategories_Products 
FOREIGN KEY (ProductId) REFERENCES Products(Id)
ON DELETE CASCADE;

-- 商品カテゴリ対照テーブル → カテゴリテーブル
ALTER TABLE ProductCategories 
ADD CONSTRAINT FK_ProductCategories_Categories 
FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
ON DELETE RESTRICT;

-- 商品画像テーブル → 商品テーブル
ALTER TABLE ProductImages 
ADD CONSTRAINT FK_ProductImages_Products 
FOREIGN KEY (ProductId) REFERENCES Products(Id)
ON DELETE CASCADE;
```

### 3.2 カーディナリティ
- **Categories : Categories = 1 : N** (親カテゴリに複数の子カテゴリ)
- **Products : Categories = N : N** (商品は複数カテゴリに属し、カテゴリには複数商品が属する)
- **Products : ProductImages = 1 : N** (1つの商品に最大5枚の画像)
- **ProductCategories** が多対多の関連を管理

## 4. バリデーションルール

### 4.1 商品テーブル (Products)
- **Name**: 必須、1-100文字、重複許可
- **Description**: 任意、最大1000文字
- **Price**: 必須、0以上4294967295以下の整数（日本円）
- **JanCode**: 任意、13桁の数字、重複不可（設定時）
- **Status**: 必須、0-2の整数値（0:販売開始前、1:販売中、2:取扱終了）
- **CreatedAt**: 必須、現在日時で自動設定
- **UpdatedAt**: 必須、更新時に自動設定

### 4.2 カテゴリテーブル (Categories)
- **Name**: 必須、1-50文字、**重複不可**
- **Description**: 任意、最大500文字
- **ParentCategoryId**: 任意、Categories.Idに存在する値（ルートカテゴリの場合はNULL）
- **Level**: 必須、0以上の整数（ルートカテゴリは0、子カテゴリは親のLevel+1）
- **SortOrder**: 必須、表示順序用の整数値
- **CreatedAt**: 必須、現在日時で自動設定
- **UpdatedAt**: 必須、更新時に自動設定

### 4.3 商品カテゴリ対照テーブル (ProductCategories)
- **ProductId**: 必須、Products.Idに存在する値
- **CategoryId**: 必須、Categories.Idに存在する値
- **複合キー制約**: (ProductId, CategoryId)の組み合わせは一意
- **CreatedAt**: 必須、現在日時で自動設定

### 4.4 商品画像テーブル (ProductImages)
- **ProductId**: 必須、Products.Idに存在する値
- **ImagePath**: 必須、最大500文字、有効なファイルパス形式
- **DisplayOrder**: 必須、1-5の整数値
- **AltText**: 任意、最大200文字
- **IsMain**: 必須、ブール値（商品あたり最大1つのTrue）
- **一意制約**: 同一商品でDisplayOrderの重複不可
- **件数制約**: 1商品あたり最大5件まで
- **CreatedAt**: 必須、現在日時で自動設定
- **UpdatedAt**: 必須、更新時に自動設定

## 5. 初期データ (Seed Data)

### 5.1 カテゴリ初期データ（スポーツ特化・3階層構造）
```csharp
// スポーツ関連カテゴリサンプルデータ（3階層構造）
new Category[]
{
    // ルートカテゴリ (Level 0) - 競技別
    new() { Id = 1, Name = "野球", Description = "野球関連用品", ParentCategoryId = null, Level = 0, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 2, Name = "サッカー", Description = "サッカー関連用品", ParentCategoryId = null, Level = 0, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 3, Name = "バスケットボール", Description = "バスケットボール関連用品", ParentCategoryId = null, Level = 0, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 4, Name = "バレーボール", Description = "バレーボール関連用品", ParentCategoryId = null, Level = 0, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 5, Name = "ランニング", Description = "ランニング関連用品", ParentCategoryId = null, Level = 0, SortOrder = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // サブカテゴリ (Level 1) - 用品種別
    // 野球カテゴリ
    new() { Id = 6, Name = "バット", Description = "野球バット", ParentCategoryId = 1, Level = 1, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 7, Name = "グローブ", Description = "野球グローブ", ParentCategoryId = 1, Level = 1, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 8, Name = "ウェア", Description = "野球ウェア", ParentCategoryId = 1, Level = 1, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 9, Name = "ボール", Description = "野球ボール", ParentCategoryId = 1, Level = 1, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // サッカーカテゴリ
    new() { Id = 10, Name = "シューズ", Description = "サッカーシューズ", ParentCategoryId = 2, Level = 1, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 11, Name = "ウェア", Description = "サッカーウェア", ParentCategoryId = 2, Level = 1, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 12, Name = "ボール", Description = "サッカーボール", ParentCategoryId = 2, Level = 1, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 13, Name = "ゴールキーパー用品", Description = "ゴールキーパー用品", ParentCategoryId = 2, Level = 1, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // バスケットボールカテゴリ
    new() { Id = 14, Name = "シューズ", Description = "バスケットボールシューズ", ParentCategoryId = 3, Level = 1, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 15, Name = "ウェア", Description = "バスケットボールウェア", ParentCategoryId = 3, Level = 1, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 16, Name = "ボール", Description = "バスケットボール", ParentCategoryId = 3, Level = 1, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // バレーボールカテゴリ
    new() { Id = 17, Name = "シューズ", Description = "バレーボールシューズ", ParentCategoryId = 4, Level = 1, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 18, Name = "ウェア", Description = "バレーボールウェア", ParentCategoryId = 4, Level = 1, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 19, Name = "ボール", Description = "バレーボール", ParentCategoryId = 4, Level = 1, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // ランニングカテゴリ
    new() { Id = 20, Name = "シューズ", Description = "ランニングシューズ", ParentCategoryId = 5, Level = 1, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 21, Name = "ウェア", Description = "ランニングウェア", ParentCategoryId = 5, Level = 1, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 22, Name = "アクセサリー", Description = "ランニングアクセサリー", ParentCategoryId = 5, Level = 1, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // 詳細カテゴリ (Level 2)
    // 野球バット
    new() { Id = 23, Name = "硬式バット", Description = "硬式野球用バット", ParentCategoryId = 6, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 24, Name = "軟式バット", Description = "軟式野球用バット", ParentCategoryId = 6, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // 野球グローブ
    new() { Id = 25, Name = "内野手用", Description = "内野手用グローブ", ParentCategoryId = 7, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 26, Name = "外野手用", Description = "外野手用グローブ", ParentCategoryId = 7, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 27, Name = "捕手用", Description = "捕手用グローブ", ParentCategoryId = 7, Level = 2, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // サッカーシューズ
    new() { Id = 28, Name = "HGシューズ", Description = "ハードグラウンド用シューズ", ParentCategoryId = 10, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 29, Name = "AGシューズ", Description = "人工芝用シューズ", ParentCategoryId = 10, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 30, Name = "FGシューズ", Description = "天然芝用シューズ", ParentCategoryId = 10, Level = 2, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 42, Name = "TFシューズ", Description = "ターフ用シューズ（小さなコート用）", ParentCategoryId = 10, Level = 2, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // バスケットボールシューズ
    new() { Id = 31, Name = "ハイカット", Description = "ハイカットシューズ", ParentCategoryId = 14, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 32, Name = "ローカット", Description = "ローカットシューズ", ParentCategoryId = 14, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // ランニングシューズ
    new() { Id = 33, Name = "短距離用", Description = "短距離ランニング用シューズ", ParentCategoryId = 20, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 34, Name = "中距離用", Description = "中距離ランニング用シューズ", ParentCategoryId = 20, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 38, Name = "長距離用", Description = "長距離ランニング用シューズ", ParentCategoryId = 20, Level = 2, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 39, Name = "トレイルラン用", Description = "トレイルランニング用シューズ", ParentCategoryId = 20, Level = 2, SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // ランニングウェア
    new() { Id = 35, Name = "半袖", Description = "ランニング半袖シャツ", ParentCategoryId = 21, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 36, Name = "長袖", Description = "ランニング長袖シャツ", ParentCategoryId = 21, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 37, Name = "タイツ", Description = "ランニングタイツ", ParentCategoryId = 21, Level = 2, SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // ランニングアクセサリーの詳細カテゴリ
    new() { Id = 40, Name = "GPSウォッチ", Description = "GPS機能付きランニングウォッチ", ParentCategoryId = 22, Level = 2, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 41, Name = "ランニングポーチ", Description = "ランニング用ポーチ・ベルト", ParentCategoryId = 22, Level = 2, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
};
```

### 5.2 商品初期データ（スポーツ関連）
```csharp
// スポーツ関連商品サンプルデータ（価格はuint、JANコード、ステータスを含む）
new Product[]
{
    // 野球関連商品
    new() { Id = 1, Name = "プロモデル硬式バット", Description = "プロ仕様の硬式野球バット　84cm　33inch", Price = 15800, JanCode = "4901001001001", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 2, Name = "内野手用グローブ", Description = "高品質牛革製内野手用グローブ、サイズM", Price = 8900, JanCode = "4901001001002", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 3, Name = "硬式野球ボール", Description = "公認硬式野球ボール、1ダース", Price = 580, JanCode = "4901001001003", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 4, Name = "野球ユニフォーム上下セット", Description = "チーム用野球ユニフォーム上下セット、サイズL", Price = 12000, JanCode = "4901001001004", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // サッカー関連商品
    new() { Id = 5, Name = "HGサッカースパイク", Description = "ハードグラウンド用サッカースパイク　25.5cm", Price = 8900, JanCode = "4901002002001", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 6, Name = "サッカーボール　5号球", Description = "公認サッカーボール　5号球、FIFA公認", Price = 3200, JanCode = "4901002002002", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 7, Name = "サッカーユニフォームセット", Description = "チーム用サッカーユニフォーム上下セット、サイズM", Price = 9800, JanCode = "4901002002003", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 8, Name = "ゴールキーパーグローブ", Description = "プロ仕様ゴールキーパーグローブ、サイズ9", Price = 4500, JanCode = "4901002002004", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // バスケットボール関連商品
    new() { Id = 9, Name = "バスケットボールシューズハイカット", Description = "プロモデルバスケットボールシューズ　27.0cm", Price = 12800, JanCode = "4901003003001", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 10, Name = "バスケットボール　7号球", Description = "公認バスケットボール　7号球、室内用", Price = 4800, JanCode = "4901003003002", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 11, Name = "バスケットボールユニフォーム", Description = "チーム用バスケットボールユニフォーム上下セット", Price = 11000, JanCode = "4901003003003", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // バレーボール関連商品
    new() { Id = 12, Name = "バレーボールシューズ", Description = "屋内用バレーボールシューズ　25.0cm", Price = 7800, JanCode = "4901004004001", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 13, Name = "バレーボール　5号球", Description = "公認バレーボール　5号球、室内用", Price = 3800, JanCode = "4901004004002", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 14, Name = "バレーボールユニフォーム", Description = "チーム用バレーボールユニフォーム上下セット", Price = 9500, JanCode = "4901004004003", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // ランニング関連商品
    new() { Id = 15, Name = "中長距離用ランニングシューズ", Description = "中距離・長距離対応ランニングシューズ　26.5cm", Price = 12800, JanCode = "4901005005001", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 16, Name = "短距離スプリントシューズ", Description = "短距離スプリント用シューズ　27.0cm", Price = 15600, JanCode = "4901005005002", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 17, Name = "トレイルランニングシューズ", Description = "トレイルランニング専用シューズ　26.0cm", Price = 11200, JanCode = "4901005005003", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 18, Name = "ランニングシャツ半袖", Description = "速乾機能付きランニングシャツ、サイズM", Price = 3200, JanCode = "4901005005004", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 19, Name = "ランニングタイツ", Description = "コンプレッション機能付きランニングタイツ", Price = 4800, JanCode = "4901005005005", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 20, Name = "GPSランニングウォッチ", Description = "高機能GPS付きランニングウォッチ", Price = 28000, JanCode = "4901005005006", Status = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 21, Name = "HG/AG兼用サッカースパイク", Description = "ハードグラウンド・人工芝対応シューズ　25.5cm", Price = 11800, JanCode = "4901002002005", Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // 販売終了商品の例
    new() { Id = 22, Name = "レガシーバスケットボールシューズ", Description = "旧モデルバスケットボールシューズ、在庫限り", Price = 6800, JanCode = "4901003003099", Status = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 23, Name = "ヴィンテージサッカーボール", Description = "コレクターアイテム、限定版サッカーボール", Price = 15000, JanCode = "4901002002099", Status = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
};
```

### 5.3 商品カテゴリ関連初期データ（最下層カテゴリのみ）
```csharp
// スポーツ関連商品カテゴリ対照サンプルデータ（最下層カテゴリのみに関連付け）
new ProductCategory[]
{
    // プロモデル硬式バット - 硬式バット（最下層）
    new() { ProductId = 1, CategoryId = 23, CreatedAt = DateTime.UtcNow },
    
    // 内野手用グローブ - 内野手用（最下層）
    new() { ProductId = 2, CategoryId = 25, CreatedAt = DateTime.UtcNow },
    
    // 硬式野球ボール - ボール（野球はさらなる下位なし）
    new() { ProductId = 3, CategoryId = 9, CreatedAt = DateTime.UtcNow },
    
    // 野球ユニフォーム上下セット - ウェア（野球はさらなる下位なし）
    new() { ProductId = 4, CategoryId = 8, CreatedAt = DateTime.UtcNow },
    
    // HGサッカースパイク - HGシューズ（最下層）
    new() { ProductId = 5, CategoryId = 28, CreatedAt = DateTime.UtcNow },
    
    // サッカーボール5号球 - ボール（サッカーはさらなる下位なし）
    new() { ProductId = 6, CategoryId = 12, CreatedAt = DateTime.UtcNow },
    
    // サッカーユニフォームセット - ウェア（サッカーはさらなる下位なし）
    new() { ProductId = 7, CategoryId = 11, CreatedAt = DateTime.UtcNow },
    
    // ゴールキーパーグローブ - ゴールキーパー用品（さらなる下位なし）
    new() { ProductId = 8, CategoryId = 13, CreatedAt = DateTime.UtcNow },
    
    // バスケットボールシューズハイカット - ハイカット（最下層）
    new() { ProductId = 9, CategoryId = 31, CreatedAt = DateTime.UtcNow },
    
    // バスケットボール7号球 - ボール（バスケットボールはさらなる下位なし）
    new() { ProductId = 10, CategoryId = 16, CreatedAt = DateTime.UtcNow },
    
    // バスケットボールユニフォーム - ウェア（バスケットボールはさらなる下位なし）
    new() { ProductId = 11, CategoryId = 15, CreatedAt = DateTime.UtcNow },
    
    // バレーボールシューズ - シューズ（バレーボールはさらなる下位なし）
    new() { ProductId = 12, CategoryId = 17, CreatedAt = DateTime.UtcNow },
    
    // バレーボール5号球 - ボール（バレーボールはさらなる下位なし）
    new() { ProductId = 13, CategoryId = 19, CreatedAt = DateTime.UtcNow },
    
    // バレーボールユニフォーム - ウェア（バレーボールはさらなる下位なし）
    new() { ProductId = 14, CategoryId = 18, CreatedAt = DateTime.UtcNow },
    
    // 中長距離用ランニングシューズ - 中距離用、長距離用（複数最下層カテゴリに所属）
    new() { ProductId = 15, CategoryId = 34, CreatedAt = DateTime.UtcNow },
    new() { ProductId = 15, CategoryId = 38, CreatedAt = DateTime.UtcNow },
    
    // 短距離スプリントシューズ - 短距離用（最下層）
    new() { ProductId = 16, CategoryId = 33, CreatedAt = DateTime.UtcNow },
    
    // トレイルランニングシューズ - トレイルラン用（最下層）
    new() { ProductId = 17, CategoryId = 39, CreatedAt = DateTime.UtcNow },
    
    // ランニングシャツ半袖 - 半袖（最下層）
    new() { ProductId = 18, CategoryId = 35, CreatedAt = DateTime.UtcNow },
    
    // ランニングタイツ - タイツ（最下層）
    new() { ProductId = 19, CategoryId = 37, CreatedAt = DateTime.UtcNow },
    
    // GPSランニングウォッチ - GPSウォッチ（最下層）
    new() { ProductId = 20, CategoryId = 40, CreatedAt = DateTime.UtcNow },
    
    // HG/AG兼用サッカースパイク - HGシューズ、AGシューズ（複数最下層カテゴリに所属）
    new() { ProductId = 21, CategoryId = 28, CreatedAt = DateTime.UtcNow },
    new() { ProductId = 21, CategoryId = 29, CreatedAt = DateTime.UtcNow },
    
    // レガシーバスケットボールシューズ - ローカット（最下層）
    new() { ProductId = 22, CategoryId = 32, CreatedAt = DateTime.UtcNow },
    
    // ヴィンテージサッカーボール - ボール（サッカーはさらなる下位なし）
    new() { ProductId = 23, CategoryId = 12, CreatedAt = DateTime.UtcNow }
};
```

### 5.4 商品画像初期データ（スポーツ関連）
```csharp
// スポーツ関連商品画像サンプルデータ（最大5枚まで）
new ProductImage[]
{
    // プロモデル硬式バットの画像
    new() { Id = 1, ProductId = 1, ImagePath = "/uploads/products/baseball_bat_main.jpg", DisplayOrder = 1, AltText = "硬式バットメイン画像", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 2, ProductId = 1, ImagePath = "/uploads/products/baseball_bat_grip.jpg", DisplayOrder = 2, AltText = "硬式バットグリップ部分", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 3, ProductId = 1, ImagePath = "/uploads/products/baseball_bat_logo.jpg", DisplayOrder = 3, AltText = "硬式バットロゴ部分", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // 内野手用グローブの画像
    new() { Id = 4, ProductId = 2, ImagePath = "/uploads/products/baseball_glove_main.jpg", DisplayOrder = 1, AltText = "内野手グローブメイン", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 5, ProductId = 2, ImagePath = "/uploads/products/baseball_glove_back.jpg", DisplayOrder = 2, AltText = "内野手グローブ背面", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 6, ProductId = 2, ImagePath = "/uploads/products/baseball_glove_palm.jpg", DisplayOrder = 3, AltText = "内野手グローブ手のひら", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // 硬式野球ボールの画像
    new() { Id = 7, ProductId = 3, ImagePath = "/uploads/products/baseball_ball.jpg", DisplayOrder = 1, AltText = "硬式野球ボール", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // 野球ユニフォーム上下セットの画像
    new() { Id = 8, ProductId = 4, ImagePath = "/uploads/products/baseball_uniform_set.jpg", DisplayOrder = 1, AltText = "野球ユニフォームセット", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 9, ProductId = 4, ImagePath = "/uploads/products/baseball_uniform_top.jpg", DisplayOrder = 2, AltText = "野球ユニフォームシャツ", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // HGサッカースパイクの画像
    new() { Id = 10, ProductId = 5, ImagePath = "/uploads/products/soccer_hg_spikes_main.jpg", DisplayOrder = 1, AltText = "HGサッカースパイクメイン", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 11, ProductId = 5, ImagePath = "/uploads/products/soccer_hg_spikes_sole.jpg", DisplayOrder = 2, AltText = "HGサッカースパイク靴底", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 12, ProductId = 5, ImagePath = "/uploads/products/soccer_hg_spikes_side.jpg", DisplayOrder = 3, AltText = "HGサッカースパイクサイド", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // サッカーボール5号球の画像
    new() { Id = 13, ProductId = 6, ImagePath = "/uploads/products/soccer_ball_5.jpg", DisplayOrder = 1, AltText = "サッカーボール5号球", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 14, ProductId = 6, ImagePath = "/uploads/products/soccer_ball_logo.jpg", DisplayOrder = 2, AltText = "サッカーボールFIFAロゴ", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // サッカーユニフォームセットの画像
    new() { Id = 15, ProductId = 7, ImagePath = "/uploads/products/soccer_uniform_set.jpg", DisplayOrder = 1, AltText = "サッカーユニフォームセット", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // ゴールキーパーグローブの画像
    new() { Id = 16, ProductId = 8, ImagePath = "/uploads/products/goalkeeper_gloves.jpg", DisplayOrder = 1, AltText = "ゴールキーパーグローブ", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 17, ProductId = 8, ImagePath = "/uploads/products/goalkeeper_gloves_grip.jpg", DisplayOrder = 2, AltText = "ゴールキーパーグローブグリップ", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // バスケットボールシューズハイカットの画像
    new() { Id = 18, ProductId = 9, ImagePath = "/uploads/products/basketball_highcut_main.jpg", DisplayOrder = 1, AltText = "バスケットボールシューズメイン", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 19, ProductId = 9, ImagePath = "/uploads/products/basketball_highcut_sole.jpg", DisplayOrder = 2, AltText = "バスケットボールシューズ靴底", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // バスケットボール7号球の画像
    new() { Id = 20, ProductId = 10, ImagePath = "/uploads/products/basketball_7.jpg", DisplayOrder = 1, AltText = "バスケットボール7号球", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // ロードランニングシューズの画像
    new() { Id = 21, ProductId = 15, ImagePath = "/uploads/products/running_road_main.jpg", DisplayOrder = 1, AltText = "ロードランニングシューズメイン", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 22, ProductId = 15, ImagePath = "/uploads/products/running_road_sole.jpg", DisplayOrder = 2, AltText = "ロードランニングシューズ靴底", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 23, ProductId = 15, ImagePath = "/uploads/products/running_road_side.jpg", DisplayOrder = 3, AltText = "ロードランニングシューズサイド", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // ランニングシャツ半袖の画像
    new() { Id = 24, ProductId = 16, ImagePath = "/uploads/products/running_shirt_main.jpg", DisplayOrder = 1, AltText = "ランニングシャツ半袖", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 25, ProductId = 16, ImagePath = "/uploads/products/running_shirt_back.jpg", DisplayOrder = 2, AltText = "ランニングシャツ背面", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    
    // GPSランニングウォッチの画像
    new() { Id = 26, ProductId = 18, ImagePath = "/uploads/products/gps_watch_main.jpg", DisplayOrder = 1, AltText = "GPSランニングウォッチメイン", IsMain = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 27, ProductId = 18, ImagePath = "/uploads/products/gps_watch_display.jpg", DisplayOrder = 2, AltText = "GPSランニングウォッチディスプレイ", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
    new() { Id = 28, ProductId = 18, ImagePath = "/uploads/products/gps_watch_side.jpg", DisplayOrder = 3, AltText = "GPSランニングウォッチサイド", IsMain = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
};
```

## 6. Entity Framework Core 設定

### 6.1 DbContext 設定例
**配置場所**: `Web.Essentials.Infrastructure.Data.ApplicationDbContext`

```csharp
namespace Web.Essentials.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 商品エンティティ設定
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // 外部キー設定
            entity.HasOne<Category>()
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // カテゴリエンティティ設定
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // ユニーク制約
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // 初期データ設定
        modelBuilder.Entity<Category>().HasData(/* 初期データ */);
        modelBuilder.Entity<Product>().HasData(/* 初期データ */);
    }
}
```

## 7. パフォーマンス考慮事項

### 7.1 検索最適化
- **商品名検索**: `IX_Products_Name` インデックス使用
- **カテゴリ別検索**: `IX_Products_CategoryId` インデックス使用
- **日付ソート**: `IX_Products_CreatedAt` インデックス使用

### 7.2 ページング設定
- **デフォルトページサイズ**: 10件
- **最大ページサイズ**: 50件
- **OFFSET/FETCH による効率的なページング**

## 8. 制約事項

### 8.1 削除制約
- **カテゴリ削除**: 
  - 子カテゴリが存在する場合は削除不可 (`ON DELETE RESTRICT`)
  - 関連商品が存在する場合は削除不可 (ProductCategoriesテーブル経由)
- **商品削除**: 
  - ProductCategoriesの関連レコードも自動削除 (`ON DELETE CASCADE`)
  - ProductImagesの関連レコードも自動削除 (`ON DELETE CASCADE`)

### 8.2 InMemoryデータベース制約
- アプリケーション再起動でデータリセット
- 複数プロセスでのデータ共有不可
- トランザクション機能制限あり