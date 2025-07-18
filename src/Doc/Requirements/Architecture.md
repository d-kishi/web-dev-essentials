# アーキテクチャ設計仕様書

## 目次

1. [概要](#1-概要)
2. [クリーンアーキテクチャ構成](#2-クリーンアーキテクチャ構成)
   - [2.1 レイヤー構造](#21-レイヤー構造)
   - [2.2 依存関係の方向](#22-依存関係の方向)
   - [2.3 教育ポイント](#23-教育ポイント)
3. [プロジェクト分割](#3-プロジェクト分割)
   - [3.1 ソリューション構成](#31-ソリューション構成)
   - [3.2 プロジェクト詳細](#32-プロジェクト詳細)
   - [3.3 プロジェクト参照関係](#33-プロジェクト参照関係)
4. [ネームスペース設計](#4-ネームスペース設計)
5. [フォルダ構造](#5-フォルダ構造)
6. [技術要件](#6-技術要件)

---

## 1. 概要

### 目的
- **クリーンアーキテクチャ**の基礎理解を目的とした教育用アプリケーション
- **関心の分離**と**依存関係逆転の原則**の実践
- **保守性**と**テスタビリティ**の向上

### 対象システム
- ECサイト商品管理システム
- .NET 8 MVC アプリケーション
- Entity Framework Core InMemory使用

## 2. クリーンアーキテクチャ構成

### 2.1 レイヤー構造

```
┌─────────────────────────────────┐
│     Presentation Layer          │
│   (Web.Essentials.App)          │
│  - Controllers                  │
│  - ViewModels                   │
│  - Views                        │
│  - DTOs (API Response)          │
└─────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────┐
│     Application Layer           │
│   (Web.Essentials.App)          │
│  - Application Services         │
│  - Interfaces                   │
│  - Mappings                     │
└─────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────┐
│     Domain Layer                │
│   (Web.Essentials.Domain)       │
│  - Entities                     │
│  - Value Objects                │
│  - Domain Services              │
│  - Repository Interfaces        │
└─────────────────────────────────┘
            ▲
            │
┌─────────────────────────────────┐
│     Infrastructure Layer        │
│   (Web.Essentials.Infrastructure)│
│  - Repository Implements        │
│  - DbContext                    │
│  - External Services            │
└─────────────────────────────────┘
```

#### 2.1.1 Presentation Layer (プレゼンテーション層)
**配置**: `Web.Essentials.App`

**責務**:
- ユーザーインターフェースの制御
- HTTPリクエスト/レスポンスの処理
- ビューの表示制御

**構成要素**:
- **Controllers**: MVCコントローラー (`Controllers/`)
- **ViewModels**: 画面表示用モデル (`ViewModels/`)
  - 特性: MVC画面専用、バリデーション属性あり、ファイルアップロード対応
  - 用途: Razorビューでの画面表示、フォーム操作
- **Views**: Razorビュー (`Views/`)
- **DTOs**: API応答用データ転送オブジェクト (`DTOs/`)
  - 特性: API応答専用、軽量、バリデーション属性なし
  - 用途: Ajax通信のJSON応答、API間データ転送

#### 2.1.2 Application Layer (アプリケーション層)
**配置**: `Web.Essentials.App`

**責務**:
- アプリケーション固有のビジネスロジック
- データ変換とマッピング
- ドメインサービスの調整

**構成要素**:
- **Services**: アプリケーションサービス (`Services/`)
- **Interfaces**: サービスインターフェース (`Interfaces/`)
- **Mappings**: エンティティ⇔ViewModel/DTO変換

#### 2.1.3 Domain Layer (ドメイン層)
**配置**: `Web.Essentials.Domain`

**責務**:
- ビジネスドメインの中核ロジック
- エンティティと値オブジェクトの定義
- ドメインルールの実装

**構成要素**:
- **Entities**: ドメインエンティティ (`Entities/`)
- **Value Objects**: 値オブジェクト (`ValueObjects/`)
- **Domain Services**: ドメインサービス (`Services/`)
- **Repository Interfaces**: リポジトリインターフェース (`Repositories/`)

#### 2.1.4 Infrastructure Layer (インフラストラクチャ層)
**配置**: `Web.Essentials.Infrastructure`

**責務**:
- データ永続化
- 外部システムとの連携
- 技術的な実装詳細

**構成要素**:
- **Repositories**: リポジトリ実装 (`Repositories/`)
- **DbContext**: Entity Framework DbContext (`Data/`)
- **External Services**: 外部サービス連携 (`Services/`)

### 2.2 依存関係の方向

```
Web.Essentials.App
├── → Web.Essentials.Domain
└── → Web.Essentials.Infrastructure
    └── → Web.Essentials.Domain
```

**重要な原則**:
- **依存関係逆転の原則**: Infrastructure層はDomain層のインターフェースに依存
- **Domain層は独立**: 他のレイヤーに依存しない
- **Application層**: Domain層のみに依存（Infrastructure層のインターフェースは使用しない）

### 2.3 教育ポイント

1. **関心の分離**: 各レイヤーの責務を明確に分離
2. **依存関係逆転**: インターフェースを使った疎結合
3. **単一責任原則**: クラス・メソッドの責務の単一化
4. **テスタビリティ**: モックによる単体テストの容易さ

## 3. プロジェクト分割

### 3.1 ソリューション構成

**ソリューション名**: `Web.Essentials.sln`

**プロジェクト構成**:
```
Web.Essentials.sln
├── Web.Essentials.Domain (Class Library)
├── Web.Essentials.Infrastructure (Class Library)
└── Web.Essentials.App (ASP.NET Core MVC)
```

### 3.2 プロジェクト詳細

#### 3.2.1 Web.Essentials.Domain
- **プロジェクトタイプ**: Class Library (.NET 8)
- **役割**: ドメイン層の実装
- **依存関係**: なし（純粋なドメインロジック）
- **ターゲットフレームワーク**: net8.0

#### 3.2.2 Web.Essentials.Infrastructure
- **プロジェクトタイプ**: Class Library (.NET 8)
- **役割**: インフラストラクチャ層の実装
- **依存関係**: Web.Essentials.Domain
- **主要パッケージ**: 
  - Microsoft.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.InMemory
- **ターゲットフレームワーク**: net8.0

#### 3.2.3 Web.Essentials.App
- **プロジェクトタイプ**: ASP.NET Core MVC (.NET 8)
- **役割**: プレゼンテーション層とアプリケーション層
- **依存関係**: 
  - Web.Essentials.Domain
  - Web.Essentials.Infrastructure
- **主要パッケージ**: 
  - Microsoft.AspNetCore.Mvc
  - Microsoft.EntityFrameworkCore.Design
- **ターゲットフレームワーク**: net8.0

### 3.3 プロジェクト参照関係

```xml
<!-- Web.Essentials.Infrastructure.csproj -->
<ProjectReference Include="../Web.Essentials.Domain/Web.Essentials.Domain.csproj" />

<!-- Web.Essentials.App.csproj -->
<ProjectReference Include="../Web.Essentials.Domain/Web.Essentials.Domain.csproj" />
<ProjectReference Include="../Web.Essentials.Infrastructure/Web.Essentials.Infrastructure.csproj" />
```

## 4. ネームスペース設計

### 4.1 基本規則
- **ルートネームスペース**: `Web.Essentials`
- **プロジェクト名**: `Web.Essentials.{Layer}`
- **機能別サブネームスペース**: `Web.Essentials.{Layer}.{Feature}`

### 4.2 ネームスペース構成

#### Domain Layer
```
Web.Essentials.Domain
├── Web.Essentials.Domain.Entities
├── Web.Essentials.Domain.ValueObjects
├── Web.Essentials.Domain.Services
├── Web.Essentials.Domain.Repositories
└── Web.Essentials.Domain.Common
```

#### Infrastructure Layer
```
Web.Essentials.Infrastructure
├── Web.Essentials.Infrastructure.Data
├── Web.Essentials.Infrastructure.Repositories
├── Web.Essentials.Infrastructure.Services
└── Web.Essentials.Infrastructure.Common
```

#### Application/Presentation Layer
```
Web.Essentials.App
├── Web.Essentials.App.Controllers
│   ├── Web.Essentials.App.Controllers.Mvc
│   └── Web.Essentials.App.Controllers.Api
├── Web.Essentials.App.ViewModels
├── Web.Essentials.App.DTOs
├── Web.Essentials.App.Services
├── Web.Essentials.App.Interfaces
└── Web.Essentials.App.Common
```

## 5. フォルダ構造

### 5.1 Web.Essentials.Domain
```
Web.Essentials.Domain/
├── Entities/
├── ValueObjects/
├── Services/
├── Repositories/
└── Common/
```

**各ディレクトリの目的**:
- **Entities/**: ドメインエンティティ。ビジネスの中心的なオブジェクトとそのライフサイクルを管理
- **ValueObjects/**: 値オブジェクト。不変でエンティティの属性として使用される値の集合
- **Services/**: ドメインサービス。複数のエンティティにまたがるビジネスロジックを実装
- **Repositories/**: リポジトリインターフェース。データ永続化の抽象化を定義
- **Common/**: 共通基底クラスとドメイン横断的な仕組み

### 5.2 Web.Essentials.Infrastructure
```
Web.Essentials.Infrastructure/
├── Data/
│   ├── Configurations/
│   └── SeedData/
├── Repositories/
└── Services/
```

**各ディレクトリの目的**:
- **Data/**: Entity Framework関連（DbContext、設定クラス、初期データ）
  - **Configurations/**: エンティティの詳細設定（テーブル名、関係、制約など）
  - **SeedData/**: 開発・テスト用の初期データ定義
- **Repositories/**: ドメインのリポジトリインターフェースの具体実装
- **Services/**: 外部システム連携やインフラ層固有のサービス実装

### 5.3 Web.Essentials.App
```
Web.Essentials.App/
├── Controllers/
│   ├── Mvc/
│   └── Api/
├── ViewModels/
│   ├── Products/
│   ├── Categories/
│   └── Common/
├── DTOs/
│   └── Common/
├── Services/
├── Interfaces/
├── Views/
│   ├── Products/
│   ├── Categories/
│   ├── Home/
│   └── Shared/
│       └── Components/
└── wwwroot/
    ├── js/
    │   ├── products/
    │   ├── categories/
    │   ├── common/
    │   └── lib/
    ├── css/
    │   ├── products/
    │   ├── categories/
    │   └── common/
    └── uploads/
```

**各ディレクトリの目的**:
- **Controllers/**: HTTPリクエスト処理
  - **Mvc/**: 画面表示用のMVCコントローラー（HTML応答）
  - **Api/**: Ajax/API用コントローラー（JSON応答）
- **ViewModels/**: MVC画面専用モデル（バリデーション、ファイルアップロード対応）
  - **機能別フォルダ**: 各機能の画面用ViewModel（Index、Create、Edit、Details）
  - **Common/**: 複数機能で共通利用されるViewModel
- **DTOs/**: API応答専用の軽量データ転送オブジェクト
  - **Common/**: API横断的なレスポンス形式（ApiResponse、エラー情報など）
- **Services/**: アプリケーションサービス（ドメインサービスの調整、データ変換）
- **Interfaces/**: アプリケーションサービスのインターフェース定義
- **Views/**: Razorビューテンプレート
  - **機能別フォルダ**: 各機能の画面テンプレート
  - **Shared/**: 共通レイアウトと部分ビュー
    - **Components/**: 再利用可能なビューコンポーネント
- **wwwroot/**: 静的ファイル（JavaScript、CSS、画像、アップロードファイル）
  - **js/**: クライアントサイドJavaScript
    - **機能別フォルダ**: 各機能専用のスクリプト
    - **common/**: 共通ユーティリティ（API通信、バリデーション、ページネーション）
    - **lib/**: 外部ライブラリ
  - **css/**: スタイルシート（機能別・共通別）
  - **uploads/**: ユーザーアップロードファイルの保存先

## 6. 技術要件

### 6.1 フレームワーク・ライブラリ
- **.NET 8**: 最新の.NET機能を活用
- **Entity Framework Core**: ORM、InMemoryプロバイダー使用
- **ASP.NET Core MVC**: Webアプリケーションフレームワーク

### 6.2 開発環境
- **VSCode**: メイン開発環境
- **Visual Studio 2022**: 互換性確保
- **WSL2**: Linux環境での開発対応

### 6.3 アーキテクチャ原則
- **依存関係逆転の原則**: インターフェースによる抽象化
- **単一責任の原則**: 各クラスは一つの責務のみ
- **開放閉鎖の原則**: 拡張に対して開放、修正に対して閉鎖
- **インターフェース分離の原則**: 使用しないメソッドへの依存を避ける