# Claude CodeをVSCode使ってVibe Cording

## 環境

- Windows 11 Home
  - アカウントはAdministrator
- VSCodeインストール済み
- Windowsターミナルを利用
- Claude Proサブスク購入済み

## 環境構築

1. 管理者モードでWindowsターミナル起動(PowerShell)  
以降、Windowsターミナル起動時はすべて管理者モードで起動している(普段からそうしているだけで、必須条件ではないかも)
2. WSLインストール  
`wsl --install`  
途中で入力要求されるUbuntuアカウント情報は任意に設定
3. Windowsターミナル再起動、次はUbuntuで起動
4. 以下コマンド実行してUbuntu内部のソフトウェアをアップデートする  
もしかすると不要な手順かもしれない  
`sudo apt update`
5. nodeバージョン管理ツールとしてvoltaインストール  
もしnvmとか入れたければお好みでどうぞ  
`curl https://get.volta.sh | bash`　　
あるいは、`sudo apt install nodejs npm -y`して生でnodeとnpm入れてもいい
6. voltaでnodeをインストール(作成時点のLTS版@22を入れた)  
`volta install node@22`
7. ターミナル再起動、再度Ubuntu開く
8. node, npmインストール確認
```
$ node -v
v22.16.0
$ npm -v
10.9.2
```
9. Claude Codeインストール  
`npm install -g @anthropic-ai/claude-code`

## セットアップ

[こちら](https://www.youtube.com/watch?v=6kBbbPDg12U&list=PLbt-3VxyQSh1kSk6BpRse7m1sAdm_BygA&index=8)動画の02:30～を参照  

※以降の「ターミナル」は、VSCodeのターミナルのことを指している

1. VSCode起動
2. ターミナルでUbuntu(WSL)起動
3. `claude`コマンド実行
4. テーマ選択(お好きなものでOK)
5. ログイン方法選択、事前に登録しておいたClaudeアカウント紐づけ

## SDK

VSCodeのPowershellで`dotnet`コマンドを実行するには.NET CLIをインストールする必要がある。  

- [.NET 8.0 SDK](https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0)
  - これはWSL環境にも入れる必要がある  

### WSL環境インストール

WSLターミナルで以下実行

Microsoftのパッケージリポジトリを追加
``` bash
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
```

必要なパッケージをインストール
``` bash
sudo apt-get update
sudo apt-get install -y apt-transport-https
```

.NET 8 SDKをインストール
``` bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

## VSCode拡張機能

.NET 8 C# MVC開発に導入した拡張機能：

### 必須
- **C# Dev Kit** (`ms-dotnettools.csdevkit`) - 公式のC#開発環境
  - IntelliSense、デバッグ、テスト実行機能を提供
- **C#** (`ms-dotnettools.csharp`) - 基本的なC#言語サポート

### 強く推奨
- **NuGet Package Manager** (`jmrog.vscode-nuget-package-manager`) - NuGetパッケージの管理
- **Auto Rename Tag** (`formulahendry.auto-rename-tag`) - HTMLタグの自動リネーム
- **GitLens** (`eamodio.gitlens`) - Git履歴の可視化とブランチ管理

### インストール方法
```bash
code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-dotnettools.csharp
code --install-extension jmrog.vscode-nuget-package-manager
code --install-extension formulahendry.auto-rename-tag
code --install-extension eamodio.gitlens
```

**注意**: Bracket Pair Colorizerは廃止されており、現在はVSCode組み込み機能で代替されています。

## 所感

- Claude Codeはゲームチェンジャ、シンギュラリティ。
- Claude Proだとトークン制限がきついかも
  - 今回レベルの内容でも2日間で2回上限に到達した。一度上限到達すると数時間待たされるのはきつい。
    - 変なところで中断されると、前回の内容をリテイクさせるのが難しい。  
    これがClaude側に今後の改善を期待したいところ
  - 合理的に情報を教えること、レールを引く事が人間に求められるスキルになる
    - 安い価格帯で最高の効率を求めるならそうなる
- AIが「知らない」ことを整備して、伝えてやることが今後最も開発の重要なポイントになるのは確実。
  - 知らないことは何か？
    - ドメイン層の業務知識
    - AIの管轄外で作成されたもの（パッと思い浮かぶところだと旧時代のDBとか）
- 向き合い方は以下のように実施した
  - AIエージェントをエンジニアと見立てて、この人に何を作ってもらうか依頼する
    - リーダー経験ある人材だと、使いこなすのには苦労しない印象  
    自分のチームに、エンジニアリング分野では自分よりも仕事が早くて技術的知識もあり、意見・質問・FBのアウトプットもうまいクッソ優秀なメンバが増えたと思えばいい
      - そんな人材が$20～$200/月で働いてくれるって?雇うに決まってんだろ！
    - 開発の一連の仕事（要件定義～リリース～運用）まで一通りのチーム開発をこなしてきた経験があれば、どういう情報をエンジニアに与えたらいいかは普通にわかる
  - この人が何を知れば作れるのかをヒアリングし、予測して情報を共有する
  - 要件定義にしろコードにしろ、成果物をレビューしてこの人が何を考えているか読み解く
    - こっちの想定と違っていたならば、訂正を要求する
- 昔からそうだが、製造に入る前段階までの事前準備が重要  
それに加えて、AIコーディングのために伝達するスキルが必要になった
- 人間に伝えるアウトプットとしては図化するのが非常に有効だったけど、AIコーディングにおいて図化がどの程度有効なのかは検証の必要がある