# nokako

Tiny nostr notification bot client for windows.

## Usage
- ESCキーで開く設定画面でbotのNostr秘密鍵（nsec1...）を入力してください。
- F10キーで開く設定画面で通知するキーワードと通知先（あなた）のnpubを入力してください。
- キーワードを含む投稿があるとあなたにメンションします。ただし、bot自身とあなたの投稿は除外されます。

## Tips
- botのnsecはnostterをシークレットウィンドウで開くと手軽に作成できます。
- 通知は1分間に1回のみ行われます。
- off と投稿すると通知を停止します。
- on と投稿すると通知を再開します。
- 去年 と投稿するとnostterで去年の投稿を表示するリンクを返信します。
- 昨日 と投稿するとnostterで昨日の投稿を表示するリンクを返信します。

## 利用NuGetパッケージ
- [CredentialManagement](https://www.nuget.org/packages/CredentialManagement)

## Nostrクライアントライブラリ
- [NNostr](https://github.com/Kukks/NNostr) 内のNNostr.Client Ver0.0.49を一部変更して利用しています。
