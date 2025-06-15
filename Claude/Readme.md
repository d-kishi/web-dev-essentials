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

## VSCode拡張機能

.NET 9 C# MVC開発に導入した拡張機能：

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