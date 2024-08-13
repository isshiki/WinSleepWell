
# WinSleepWell
このプロジェクトには、Windowsがスリープから復帰したときにそれを検知して再度スリープさせるサービスと、マウスの動きによるスリープからの復帰を最小限に抑えるタスクバー常駐アプリケーションが含まれています。

## フォルダー構成

```
/MyProject
│
├── /src
│   ├── /WinSleepWell          # Windows（WPF）タスクバー常駐アプリケーションのプロジェクト
│   ├── /WinSleepWellService   # Windowsサービスのプロジェクト
│   ├── /WinSleepWellLib       # 共通ライブラリのプロジェクト
│   └── WinSleepWell.sln       # ソリューションファイル
│
├── /bin
│   └── /Release               # リリースされたプログラム内容
│       ├── /App               # Windowsアプリケーション
│       ├── /Service           # Windowsサービス
│       ├── settings.json      # 共通の設定ファイル
│       ├── setup.ps1          # ユーザー用のセットアップスクリプト（プロジェクトルートにあるスクリプトと同じものです）
│       ├── UserGuide.txt      # 英語のユーザーガイド
│       └── UserGuide-ja.txt   # 日本語のユーザーガイド
│
├── /images                     # プロジェクトで使用される画像
│
├── .gitignore
├── build.ps1                   # ビルドスクリプト
├── setup.ps1                   # 開発者用のセットアップスクリプトで、サービスやタスクをインストールでき、アンインストールできる
├── README.md                   # 英語のREADMEファイル
├── README-ja.md                # 日本語のREADMEファイル
├── LICENSE                     # ライセンスファイル
└── CONTRIBUTING.md             # コントリビューション（開発貢献）ガイドライン
```

## セットアップ方法
このセクションは今後追加予定です。

## 使用方法
セットアップが完了すると、設定画面が自動的に表示されます。表示されない場合は、タスクバーに常駐しているアイコンをダブルクリックしてください。設定が完了すると、その設定内容に基づいて、サービスがWindowsのスリープを適切に維持するようになります。

## PowerShellスクリプトを実行するための前提条件
`build.ps1` および `setup.ps1` スクリプトを実行する前に、実行ポリシーが適切に設定されていることを確認してください。

1. **Developer PowerShell for VS 2022** を管理者権限で開きます。スタートメニューの **すべてのプログラム > Visual Studio 2022 > Developer PowerShell for VS 2022** から実行できます。
2. 現在の実行ポリシーを確認して保存します：
   ```powershell
   $OriginalPolicy = Get-ExecutionPolicy
   Write-Host "現在の実行ポリシー: $OriginalPolicy"
   ```
3. 実行ポリシーが `Unrestricted` に設定されていない場合、次のコマンドで設定します：
   ```powershell
   Set-ExecutionPolicy Unrestricted -Scope CurrentUser
   ```
   **注意:** 実行ポリシーを `Unrestricted` に設定すると、すべてのスクリプトが実行可能になりますが、インターネットからダウンロードされたスクリプトを実行する際に警告が表示される場合があります。実行ポリシーの変更には管理者権限が必要です。本プロジェクトの作者は、実行ポリシーの変更により生じる問題について責任を負いません。
4. setupやbuildのプロセスが完了したら、頻繁にこれらのスクリプトを実行する予定がない場合、次のコマンドを実行して元のポリシーに戻すことができます：
   ```powershell
   Set-ExecutionPolicy $OriginalPolicy -Scope CurrentUser
   ```

## 開発者向けセットアップ方法
プロジェクトを開発環境でセットアップするには、以下の手順に従ってください。

1. **Developer PowerShell for VS 2022** を管理者権限で開きます。スタートメニューの **すべてのプログラム > Visual Studio 2022 > Developer PowerShell for VS 2022** から実行できます。
2. プロジェクトのルートディレクトリに移動します。
3. ビルドスクリプトを実行して、すべてのプロジェクトをコンパイルします：
   ```powershell
   ./build.ps1
   ```
4. ビルドが完了したら、セットアップスクリプトを実行してサービスとタスクをインストールします：
   ```powershell
   ./setup.ps1 -i
   ```
5. ビルドされた成果物は `bin\Debug` および `bin\Release` ディレクトリに配置され、サービスが自動的にインストールされて開始されます。
6. サービスとタスクをアンインストールするには、次のコマンドを実行します：
   ```powershell
   ./setup.ps1 -u
   ```

## 開発方法
デバッグを行うためには、Visual Studioを管理者権限で実行してください。このプログラムは管理者権限を必要とします。

## ビルド方法
プロジェクトをビルドするには、`build.ps1` スクリプトを使用できます。このスクリプトはすべてのプロジェクトをコンパイルし、成果物を適切な `bin` ディレクトリに配置します。

### ビルドスクリプトの使用方法

1. **Developer PowerShell for VS 2022** を管理者権限で開きます。スタートメニューの **すべてのプログラム > Visual Studio 2022 > Developer PowerShell for VS 2022** から実行できます。
2. プロジェクトのルートディレクトリに移動します。
3. ビルドスクリプトを実行します：
   ```powershell
   ./build.ps1
   ```
4. ビルドされた成果物は `bin\Debug` および `bin\Release` ディレクトリに配置されます。

## コントリビュート
コントリビューションは歓迎します！このプロジェクトに貢献する方法については、[CONTRIBUTING.md](CONTRIBUTING.md) ファイルを参照してください。

## ライセンス
このプロジェクトはApache 2.0ライセンスの下でライセンスされています。詳細は [LICENSE](LICENSE) ファイルを参照してください。
