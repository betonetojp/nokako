﻿using NNostr.Client;
using NNostr.Client.Protocols;
using nokako.Properties;
using nokakoiCrypt;
using System.Diagnostics;

namespace nokako
{
    public partial class FormMain : Form
    {
        #region フィールド
        private readonly string _configPath = Path.Combine(Application.StartupPath, "nokako.config");

        private readonly FormSetting _formSetting = new();
        private FormManiacs _formManiacs = new();
        private FormRelayList _formRelayList = new();

        private string _nsec = string.Empty;
        private string _npubHex = string.Empty;

        /// <summary>
        /// フォロイー公開鍵のハッシュセット
        /// </summary>
        private readonly HashSet<string> _followeesHexs = [];
        /// <summary>
        /// ユーザー辞書
        /// </summary>
        internal Dictionary<string, User?> Users = [];
        /// <summary>
        /// キーワード通知
        /// </summary>
        internal KeywordNotifier Notifier = new();

        private bool _showOnlyFollowees;
        private string _nokakoiKey = string.Empty;
        private string _password = string.Empty;
        private bool _addClient;

        private double _tempOpacity = 1.00;

        // 重複イベントIDを保存するリスト
        private readonly LinkedList<string> _displayedEventIds = new();

        private List<Client> _clients = [];

        private DateTime _lastNotifyKakoiPostTime = DateTime.MinValue;
        private DateTime _lastNotifyPostTime = DateTime.MinValue;
        // 投稿間隔
        private static readonly TimeSpan NotifyInterval = TimeSpan.FromSeconds(60);
        // 投稿機能有効フラグ
        private bool _enablePost = true;
        #endregion

        #region コンストラクタ
        // コンストラクタ
        public FormMain()
        {
            InitializeComponent();

            // ボタンの画像をDPIに合わせて表示
            float scale = CreateGraphics().DpiX / 96f;
            int size = (int)(16 * scale);
            if (scale < 2.0f)
            {
                buttonRelayList.Image = new Bitmap(Resources.icons8_list_16, size, size);
                buttonStart.Image = new Bitmap(Resources.icons8_start_16, size, size);
                buttonStop.Image = new Bitmap(Resources.icons8_stop_16, size, size);
                buttonSetting.Image = new Bitmap(Resources.icons8_setting_16, size, size);
            }
            else
            {
                buttonRelayList.Image = new Bitmap(Resources.icons8_list_32, size, size);
                buttonStart.Image = new Bitmap(Resources.icons8_start_32, size, size);
                buttonStop.Image = new Bitmap(Resources.icons8_stop_32, size, size);
                buttonSetting.Image = new Bitmap(Resources.icons8_setting_32, size, size);
            }

            Setting.Load(_configPath);
            Users = Tools.LoadUsers();
            _clients = Tools.LoadClients();

            Location = Setting.Location;
            if (new Point(0, 0) == Location || Location.X < 0 || Location.Y < 0)
            {
                StartPosition = FormStartPosition.CenterScreen;
            }
            Size = Setting.Size;
            TopMost = Setting.TopMost;
            Opacity = Setting.Opacity;
            if (0 == Opacity)
            {
                Opacity = 1;
            }
            _tempOpacity = Opacity;
            //dataGridViewNotes.Columns["avatar"].Visible = _showAvatar;
            _showOnlyFollowees = Setting.ShowOnlyFollowees;
            _nokakoiKey = Setting.NokakoiKey;
            _addClient = Setting.AddClient;

            dataGridViewNotes.Columns["name"].Width = Setting.NameColumnWidth;
            dataGridViewNotes.GridColor = Tools.HexToColor(Setting.GridColor);
            dataGridViewNotes.DefaultCellStyle.SelectionBackColor = Tools.HexToColor(Setting.GridColor);

            _formManiacs.MainForm = this;
        }
        #endregion

        #region Startボタン
        // Startボタン
        private async void ButtonStart_Click(object sender, EventArgs e)
        {
            try
            {
                int connectCount;
                if (NostrAccess.Clients != null)
                {
                    connectCount = await NostrAccess.ConnectAsync();
                }
                else
                {
                    connectCount = await NostrAccess.ConnectAsync();
                    switch (connectCount)
                    {
                        case 0:
                            labelRelays.Text = "No relay enabled.";
                            toolTipRelays.SetToolTip(labelRelays, string.Empty);
                            break;
                        case 1:
                            labelRelays.Text = NostrAccess.Relays[0].ToString();
                            toolTipRelays.SetToolTip(labelRelays, string.Join("\n", NostrAccess.Relays.Select(r => r.ToString())));
                            break;
                        default:
                            labelRelays.Text = $"{NostrAccess.Relays.Length} relays";
                            toolTipRelays.SetToolTip(labelRelays, string.Join("\n", NostrAccess.Relays.Select(r => r.ToString())));
                            break;
                    }
                    if (NostrAccess.Clients != null)
                    {
                        NostrAccess.Clients.EventsReceived += OnClientOnEventsReceived2;
                        NostrAccess.Clients.EventsReceived += OnClientOnEventsReceived;
                    }
                }

                await NostrAccess.SubscribeAsync();

                buttonStart.Enabled = false;
                buttonStop.Enabled = true;
                dataGridViewNotes.Focus();

                // ログイン済みの時
                if (!string.IsNullOrEmpty(_npubHex))
                {
                    // フォロイーを購読をする
                    await NostrAccess.SubscribeFollowsAsync(_npubHex);
                }

                dataGridViewNotes.Rows.Clear();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Could not start.";
            }
        }
        #endregion

        #region ユーザー情報イベント受信時処理
        /// <summary>
        /// ユーザー情報イベント受信時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnClientOnEventsReceived2(object? sender, (string subscriptionId, NostrEvent[] events) args)
        {
            if (args.subscriptionId == NostrAccess.GetFolloweesSubscriptionId)
            {
                #region フォロイー購読
                foreach (var nostrEvent in args.events)
                {
                    // フォローリスト
                    if (3 == nostrEvent.Kind)
                    {
                        var tags = nostrEvent.Tags;
                        foreach (var tag in tags)
                        {
                            if ("p" == tag.TagIdentifier)
                            {
                                // 公開鍵をハッシュに保存
                                _followeesHexs.Add(tag.Data[0]);

                                // petnameをユーザー辞書に保存
                                if (2 < tag.Data.Count)
                                {
                                    Users.TryGetValue(tag.Data[0], out User? user);
                                    if (user != null)
                                    {
                                        user.PetName = tag.Data[2];
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            else if (args.subscriptionId == NostrAccess.GetProfilesSubscriptionId)
            {
                #region プロフィール購読
                foreach (var nostrEvent in args.events)
                {
                    if (RemoveCompletedEventIds(nostrEvent.Id))
                    {
                        continue;
                    }

                    // プロフィール
                    if (0 == nostrEvent.Kind && nostrEvent.Content != null && nostrEvent.PublicKey != null)
                    {
                        var newUserData = Tools.JsonToUser(nostrEvent.Content, nostrEvent.CreatedAt, Notifier.Settings.MuteMostr);
                        if (newUserData != null)
                        {
                            DateTimeOffset? cratedAt = DateTimeOffset.MinValue;
                            if (Users.TryGetValue(nostrEvent.PublicKey, out User? existingUserData))
                            {
                                cratedAt = existingUserData?.CreatedAt;
                            }
                            if (false == existingUserData?.Mute)
                            {
                                // 既にミュートオフのMostrアカウントのミュートを解除
                                newUserData.Mute = false;
                            }
                            if (cratedAt == null || (cratedAt < newUserData.CreatedAt))
                            {
                                newUserData.LastActivity = DateTime.Now;
                                newUserData.PetName = existingUserData?.PetName;
                                Tools.SaveUsers(Users);
                                // 辞書に追加（上書き）
                                Users[nostrEvent.PublicKey] = newUserData;
                                Debug.WriteLine($"cratedAt updated {cratedAt} -> {newUserData.CreatedAt}");
                                Debug.WriteLine($"プロフィール更新: {newUserData.DisplayName} @{newUserData.Name}");
                            }
                        }
                    }
                }
                #endregion
            }
        }
        #endregion

        #region タイムラインイベント受信時処理
        /// <summary>
        /// タイムラインイベント受信時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnClientOnEventsReceived(object? sender, (string subscriptionId, NostrEvent[] events) args)
        {
            if (args.subscriptionId == NostrAccess.SubscriptionId)
            {
                #region タイムライン購読
                foreach (var nostrEvent in args.events)
                {
                    if (RemoveCompletedEventIds(nostrEvent.Id))
                    {
                        continue;
                    }

                    var content = nostrEvent.Content;
                    if (content != null)
                    {
                        string userName = string.Empty;
                        User? user = null;

                        // フォロイーチェック
                        string headMark = "-";
                        if (_followeesHexs.Contains(nostrEvent.PublicKey))
                        {
                            headMark = "*";
                        }

                        #region リアクション
                        if (7 == nostrEvent.Kind)
                        {
                            // ログイン済みで自分へのリアクション
                            if (!string.IsNullOrEmpty(_npubHex) && nostrEvent.GetTaggedPublicKeys().Contains(_npubHex))
                            {
                                // プロフィール購読
                                await NostrAccess.SubscribeProfilesAsync([nostrEvent.PublicKey]);

                                // ユーザー取得
                                user = await GetUserAsync(nostrEvent.PublicKey);
                                // ユーザーが見つからない時は表示しない
                                if (user == null)
                                {
                                    continue;
                                }
                                // ユーザー表示名取得
                                userName = GetUserName(nostrEvent.PublicKey);

                                headMark = "+";

                                // グリッドに表示
                                DateTimeOffset dto = nostrEvent.CreatedAt ?? DateTimeOffset.Now;
                                dataGridViewNotes.Rows.Insert(
                                0,
                                dto.ToLocalTime(),
                                new Bitmap(1, 1),
                                $"{headMark} {userName}",
                                nostrEvent.Content,
                                nostrEvent.Id,
                                nostrEvent.PublicKey,
                                nostrEvent.Kind
                                );

                                // 背景色をリアクションカラーに変更
                                dataGridViewNotes.Rows[0].DefaultCellStyle.BackColor = Tools.HexToColor(Setting.ReactionColor);

                                // 行を装飾
                                EditRow(nostrEvent, userName);
                            }
                        }
                        #endregion

                        #region テキストノート
                        if (1 == nostrEvent.Kind)
                        {
                            // フォロイー限定表示オンでフォロイーじゃない時は表示しない
                            if (_showOnlyFollowees && !_followeesHexs.Contains(nostrEvent.PublicKey))
                            {
                                continue;
                            }
                            // ミュートしている時は表示しない
                            if (IsMuted(nostrEvent.PublicKey))
                            {
                                continue;
                            }
                            // pタグにミュートされている公開鍵が含まれている時は表示しない
                            if (nostrEvent.GetTaggedPublicKeys().Any(pk => IsMuted(pk)))
                            {
                                continue;
                            }

                            // キーワード通知
                            if (nostrEvent.PublicKey == _npubHex)
                            {
                                // 自分の時は抜ける
                                continue;
                            }
                            var settings = Notifier.Settings;
                            string whoToNotify = settings.Npub;
                            try
                            {
                                whoToNotify = settings.Npub.ConvertToHex();
                                if (nostrEvent.PublicKey == whoToNotify)
                                {
                                    // 通知先の時
                                    if (content == "on")
                                    {
                                        await PostAsync("通知を有効にしました", nostrEvent);
                                        _enablePost = true;
                                    }
                                    if (content == "off")
                                    {
                                        await PostAsync("通知を無効にしました", nostrEvent);
                                        _enablePost = false;
                                    }

                                    // 通知先の時は抜ける
                                    continue;
                                }

                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"通知先変換失敗: {ex.Message}");
                                continue;
                            }

                            if (!_enablePost)
                            {
                                // 通知機能がオフの時は抜ける
                                continue;
                            }

                            if (Setting.CheckUserClient)
                            {
                                // ポストのclientタグにnokakoiファミリーが含まれていたら通知投稿
                                List<string> clientsToCheck = ["kakoi", "nokako", "nokakoi"];
                                var userClients = nostrEvent.GetTaggedData("client");
                                if (userClients != null)
                                {
                                    foreach (var client in clientsToCheck)
                                    {
                                        if (userClients.Contains(client))
                                        {
                                            // プロフィール購読
                                            await NostrAccess.SubscribeProfilesAsync([nostrEvent.PublicKey]);

                                            // ユーザー取得
                                            user = await GetUserAsync(nostrEvent.PublicKey);
                                            // ユーザーが見つからない時は表示しない
                                            if (user == null)
                                            {
                                                continue;
                                            }
                                            // ユーザー表示名取得
                                            userName = GetUserName(nostrEvent.PublicKey);

                                            var rootEvent = new NostrEvent()
                                            {
                                                Id = nostrEvent.Id,
                                                PublicKey = nostrEvent.PublicKey
                                            };

                                            // 通知投稿
                                            await NotifyKakoiPostAsync(whoToNotify, client, rootEvent);

                                            bool isReply = false;
                                            var e = nostrEvent.GetTaggedData("e");
                                            var p = nostrEvent.GetTaggedData("p");
                                            var q = nostrEvent.GetTaggedData("q");
                                            if (e != null && 0 < e.Length ||
                                                p != null && 0 < p.Length ||
                                                q != null && 0 < q.Length)
                                            {
                                                isReply = true;
                                            }

                                            // グリッドに表示
                                            DateTimeOffset dto = nostrEvent.CreatedAt ?? DateTimeOffset.Now;
                                            dataGridViewNotes.Rows.Insert(
                                                0,
                                                dto.ToLocalTime(),
                                                new Bitmap(1, 1),
                                                $"{headMark} {userName}",
                                                nostrEvent.Content,
                                                nostrEvent.Id,
                                                nostrEvent.PublicKey,
                                                nostrEvent.Kind
                                                );

                                            // リプライの時は背景色変更
                                            if (isReply)
                                            {
                                                dataGridViewNotes.Rows[0].DefaultCellStyle.BackColor = Tools.HexToColor(Setting.ReplyColor);
                                            }

                                            // 行を装飾
                                            EditRow(nostrEvent, userName);
                                        }
                                    }
                                }
                            }

                            // キーワードのポストのみ表示
                            var keyword = Notifier.CheckPost(content);
                            if (!string.IsNullOrEmpty(keyword))
                            {
                                // プロフィール購読
                                await NostrAccess.SubscribeProfilesAsync([nostrEvent.PublicKey]);

                                // ユーザー取得
                                user = await GetUserAsync(nostrEvent.PublicKey);
                                // ユーザーが見つからない時は表示しない
                                if (user == null)
                                {
                                    continue;
                                }
                                // ユーザー表示名取得
                                userName = GetUserName(nostrEvent.PublicKey);

                                var rootEvent = new NostrEvent()
                                {
                                    Id = nostrEvent.Id,
                                    PublicKey = nostrEvent.PublicKey
                                };

                                // 通知投稿
                                await NotifyPostAsync(whoToNotify, keyword, rootEvent);

                                if (settings.Open)
                                {
                                    NIP19.NostrEventNote nostrEventNote = new()
                                    {
                                        EventId = nostrEvent.Id,
                                        Relays = [string.Empty],
                                    };
                                    var nevent = nostrEventNote.ToNIP19();
                                    var app = new ProcessStartInfo
                                    {
                                        FileName = settings.FileName + nevent,
                                        UseShellExecute = true
                                    };
                                    try
                                    {
                                        Process.Start(app);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine(ex.Message);
                                    }
                                }

                                bool isReply = false;
                                var e = nostrEvent.GetTaggedData("e");
                                var p = nostrEvent.GetTaggedData("p");
                                var q = nostrEvent.GetTaggedData("q");
                                if (e != null && 0 < e.Length ||
                                    p != null && 0 < p.Length ||
                                    q != null && 0 < q.Length)
                                {
                                    isReply = true;
                                }

                                // グリッドに表示
                                DateTimeOffset dto = nostrEvent.CreatedAt ?? DateTimeOffset.Now;
                                dataGridViewNotes.Rows.Insert(
                                    0,
                                    dto.ToLocalTime(),
                                    new Bitmap(1, 1),
                                    $"{headMark} {userName}",
                                    nostrEvent.Content,
                                    nostrEvent.Id,
                                    nostrEvent.PublicKey,
                                    nostrEvent.Kind
                                    );

                                // リプライの時は背景色変更
                                if (isReply)
                                {
                                    dataGridViewNotes.Rows[0].DefaultCellStyle.BackColor = Tools.HexToColor(Setting.ReplyColor);
                                }

                                // 行を装飾
                                EditRow(nostrEvent, userName);

                                // 改行をスペースに置き換えてログ表示
                                Debug.WriteLine($"{userName}: {content.Replace('\n', ' ')}");
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }
        }
        #endregion

        #region グリッド行装飾
        private void EditRow(NostrEvent nostrEvent, string userName)
        {
            // avatar列のToolTipに表示名を設定
            dataGridViewNotes.Rows[0].Cells["avatar"].ToolTipText = userName;

            // avastar列の背景色をpubkeyColorに変更
            var pubkeyColor = Tools.HexToColor(nostrEvent.PublicKey[..6]); // [i..j] で「i番目からj番目の範囲」
            dataGridViewNotes.Rows[0].Cells["avatar"].Style.BackColor = pubkeyColor;

            // クライアントタグによる背景色変更
            var userClient = nostrEvent.GetTaggedData("client");
            if (userClient != null && 0 < userClient.Length)
            {
                Color clientColor = Color.WhiteSmoke;

                // userClient[0]を_clientsから検索して色を取得
                var client = _clients.FirstOrDefault(c => c.Name == userClient[0]);
                if (client != null && client.ColorCode != null)
                {
                    clientColor = Tools.HexToColor(client.ColorCode);
                }
                // time列の背景色をclientColorに変更
                dataGridViewNotes.Rows[0].Cells["time"].Style.BackColor = clientColor;
            }
        }
        #endregion

        #region ユーザー取得
        private async Task<User?> GetUserAsync(string pubkey)
        {
            User? user = null;
            int retryCount = 0;
            while (retryCount < 10)
            {
                Debug.WriteLine($"retryCount = {retryCount} {GetUserName(pubkey)}");
                Users.TryGetValue(pubkey, out user);
                // ユーザーが見つかった場合、ループを抜ける
                if (user != null)
                {
                    break;
                }
                // 一定時間待機してから再試行
                await Task.Delay(100);
                retryCount++;
            }
            return user;
        }
        #endregion

        #region Stopボタン
        // Stopボタン
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }

            try
            {
                NostrAccess.CloseSubscriptions();
                labelRelays.Text = "Close subscription.";

                _ = NostrAccess.Clients.Disconnect();
                labelRelays.Text = "Disconnect.";
                NostrAccess.Clients.Dispose();
                NostrAccess.Clients = null;

                buttonStart.Enabled = true;
                buttonStart.Focus();
                buttonStop.Enabled = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Could not stop.";
            }
        }
        #endregion

        #region 投稿処理
        /// <summary>
        /// 投稿処理
        /// </summary>
        /// <returns></returns>
        private async Task PostAsync(string content, NostrEvent? rootEvent = null, bool isQuote = false)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }
            // create tags
            List<NostrEventTag> tags = [];
            if (rootEvent != null)
            {
                if (isQuote)
                {
                    tags.Add(new NostrEventTag() { TagIdentifier = "q", Data = [rootEvent.Id, string.Empty] });
                }
                else
                {
                    tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [rootEvent.Id, string.Empty] });
                    tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [rootEvent.PublicKey] });
                }
            }
            if (_addClient)
            {
                tags.Add(new NostrEventTag()
                {
                    TagIdentifier = "client",
                    Data = ["nokako"]
                });
            }
            // create a new event
            var newEvent = new NostrEvent()
            {
                Kind = 1,
                Content = content.Replace("\r\n", "\n"),
                Tags = tags
            };

            try
            {
                // load from an nsec string
                var key = _nsec.FromNIP19Nsec();
                // sign the event
                await newEvent.ComputeIdAndSignAsync(key);
                // send the event
                await NostrAccess.Clients.SendEventsAndWaitUntilReceived([newEvent], CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
        }
        #endregion

        #region kakoi通知投稿処理
        /// <summary>
        /// 投稿処理
        /// </summary>
        /// <returns></returns>
        private async Task NotifyKakoiPostAsync(string whoToNotify, string client, NostrEvent? rootEvent = null)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }

            // 連投制限
            if (DateTime.Now - _lastNotifyKakoiPostTime < NotifyInterval)
            {
                return;
            }
            _lastNotifyKakoiPostTime = DateTime.Now;

            string content = $"{client}での投稿ですよ";
            // create tags
            List<NostrEventTag> tags = [];
            tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [whoToNotify] });
            if (rootEvent != null)
            {
                var eventNote = new NIP19.NostrEventNote()
                {
                    EventId = rootEvent.Id,
                    Relays = [string.Empty],
                };
                tags.Add(new NostrEventTag() { TagIdentifier = "q", Data = [rootEvent.Id, string.Empty] });
                content = $"{GetUserName(rootEvent.PublicKey)}さんが{client}で投稿してくれました{Environment.NewLine}nostr:{eventNote.ToNIP19()}";
            }
            if (_addClient)
            {
                tags.Add(new NostrEventTag()
                {
                    TagIdentifier = "client",
                    Data = ["nokako"]
                });
            }
            // create a new event
            var newEvent = new NostrEvent()
            {
                Kind = 1,
                Content = content.Replace("\r\n", "\n"),
                Tags = tags
            };

            try
            {
                // load from an nsec string
                var key = _nsec.FromNIP19Nsec();
                // sign the event
                await newEvent.ComputeIdAndSignAsync(key);
                // send the event
                await NostrAccess.Clients.SendEventsAndWaitUntilReceived([newEvent], CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
        }
        #endregion

        #region 通知投稿処理
        /// <summary>
        /// 投稿処理
        /// </summary>
        /// <returns></returns>
        private async Task NotifyPostAsync(string whoToNotify, string? keyword, NostrEvent? rootEvent = null)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(keyword))
            {
                return;
            }

            // 連投制限
            if (DateTime.Now - _lastNotifyPostTime < NotifyInterval)
            {
                return;
            }
            _lastNotifyPostTime = DateTime.Now;

            string content = "うわさされてるみたいですよ";
            // create tags
            List<NostrEventTag> tags = [];
            tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [whoToNotify] });
            if (rootEvent != null)
            {
                var eventNote = new NIP19.NostrEventNote()
                {
                    EventId = rootEvent.Id,
                    Relays = [string.Empty],
                };
                tags.Add(new NostrEventTag() { TagIdentifier = "q", Data = [rootEvent.Id, string.Empty] });
                content = $"{GetUserName(rootEvent.PublicKey)}さんが{keyword}のことうわさしてるみたいですよ{Environment.NewLine}nostr:{eventNote.ToNIP19()}";
            }
            if (_addClient)
            {
                tags.Add(new NostrEventTag()
                {
                    TagIdentifier = "client",
                    Data = ["nokako"]
                });
            }
            // create a new event
            var newEvent = new NostrEvent()
            {
                Kind = 1,
                Content = content.Replace("\r\n", "\n"),
                Tags = tags
            };

            try
            {
                // load from an nsec string
                var key = _nsec.FromNIP19Nsec();
                // sign the event
                await newEvent.ComputeIdAndSignAsync(key);
                // send the event
                await NostrAccess.Clients.SendEventsAndWaitUntilReceived([newEvent], CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
        }
        #endregion

        #region Settingボタン
        // Settingボタン
        private async void ButtonSetting_Click(object sender, EventArgs e)
        {
            // 開く前
            Opacity = _tempOpacity;
            _formSetting.checkBoxTopMost.Checked = TopMost;
            _formSetting.trackBarOpacity.Value = (int)(Opacity * 100);
            _formSetting.checkBoxShowOnlyFollowees.Checked = _showOnlyFollowees;
            _formSetting.textBoxNokakoiKey.Text = _nokakoiKey;
            _formSetting.textBoxPassword.Text = _password;
            _formSetting.checkBoxAddClient.Checked = _addClient;

            // 開く
            _formSetting.ShowDialog(this);

            // 閉じた後
            TopMost = _formSetting.checkBoxTopMost.Checked;
            Opacity = _formSetting.trackBarOpacity.Value / 100.0;
            _tempOpacity = Opacity;
            //dataGridViewNotes.Columns["avatar"].Visible = _showAvatar;
            _showOnlyFollowees = _formSetting.checkBoxShowOnlyFollowees.Checked;
            _nokakoiKey = _formSetting.textBoxNokakoiKey.Text;
            _password = _formSetting.textBoxPassword.Text;
            _addClient = _formSetting.checkBoxAddClient.Checked;
            try
            {
                // 別アカウントログイン失敗に備えてクリアしておく
                _nsec = string.Empty;
                _npubHex = string.Empty;
                _followeesHexs.Clear();

                // 秘密鍵と公開鍵取得
                _nsec = NokakoiCrypt.DecryptNokakoiKey(_nokakoiKey, _password);
                _npubHex = _nsec.GetNpubHex();

                // ログイン済みの時
                if (!string.IsNullOrEmpty(_npubHex))
                {
                    int connectCount = await NostrAccess.ConnectAsync();
                    switch (connectCount)
                    {
                        case 0:
                            labelRelays.Text = "No relay enabled.";
                            toolTipRelays.SetToolTip(labelRelays, string.Empty);
                            break;
                        case 1:
                            labelRelays.Text = NostrAccess.Relays[0].ToString();
                            toolTipRelays.SetToolTip(labelRelays, string.Join("\n", NostrAccess.Relays.Select(r => r.ToString())));
                            break;
                        default:
                            labelRelays.Text = $"{NostrAccess.Relays.Length} relays";
                            toolTipRelays.SetToolTip(labelRelays, string.Join("\n", NostrAccess.Relays.Select(r => r.ToString())));
                            break;
                    }
                    if (0 == connectCount)
                    {
                        return;
                    }

                    // フォロイーを購読をする
                    await NostrAccess.SubscribeFollowsAsync(_npubHex);

                    // ログインユーザー表示名取得
                    var name = GetUserName(_npubHex);
                    //_formPostBar.textBoxPost.PlaceholderText = $"Post as {name}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }

            Setting.TopMost = TopMost;
            Setting.Opacity = Opacity;
            Setting.ShowOnlyFollowees = _showOnlyFollowees;
            Setting.NokakoiKey = _nokakoiKey;
            Setting.AddClient = _addClient;

            Setting.Save(_configPath);
            _clients = Tools.LoadClients();

            dataGridViewNotes.Focus();
        }
        #endregion

        #region 複数リレーからの処理済みイベントを除外
        /// <summary>
        /// 複数リレーからの処理済みイベントを除外
        /// </summary>
        /// <param name="eventId"></param>
        private bool RemoveCompletedEventIds(string eventId)
        {
            if (_displayedEventIds.Contains(eventId))
            {
                return true;
            }
            if (_displayedEventIds.Count >= 4096)
            {
                _displayedEventIds.RemoveFirst();
            }
            _displayedEventIds.AddLast(eventId);
            return false;
        }
        #endregion

        #region 透明解除処理
        // マウス入った時
        private void Control_MouseEnter(object sender, EventArgs e)
        {
            _tempOpacity = Opacity;
            Opacity = 1.00;
        }

        // マウス出た時
        private void Control_MouseLeave(object sender, EventArgs e)
        {
            Opacity = _tempOpacity;
        }
        #endregion

        #region ユーザー表示名を取得する
        /// <summary>
        /// ユーザー表示名を取得する
        /// </summary>
        /// <param name="publicKeyHex">公開鍵HEX</param>
        /// <returns>ユーザー表示名</returns>
        private string GetUserName(string publicKeyHex)
        {
            // 情報があれば表示名を取得
            Users.TryGetValue(publicKeyHex, out User? user);
            string? userName = "???";
            if (user != null)
            {
                userName = user.DisplayName;
                // display_nameが無い場合はnameとする
                if (userName == null || string.Empty == userName)
                {
                    userName = $"{user.Name}";
                }
                // petnameがある場合はpetnameとする
                if (!string.IsNullOrEmpty(user.PetName))
                {
                    userName = $"{user.PetName}";
                }
                // 取得日更新
                user.LastActivity = DateTime.Now;
                Tools.SaveUsers(Users);
                //Debug.WriteLine($"名前取得: {user.DisplayName} @{user.Name} 📛{user.PetName}");
            }
            return userName;
        }
        #endregion

        #region ミュートされているか確認する
        /// <summary>
        /// ミュートされているか確認する
        /// </summary>
        /// <param name="publicKeyHex">公開鍵HEX</param>
        /// <returns>ミュートフラグ</returns>
        private bool IsMuted(string publicKeyHex)
        {
            if (Users.TryGetValue(publicKeyHex, out User? user))
            {
                if (user != null)
                {
                    return user.Mute;
                }
            }
            return false;
        }
        #endregion

        #region 閉じる
        // 閉じる
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            NostrAccess.CloseSubscriptions();
            NostrAccess.DisconnectAndDispose();

            if (FormWindowState.Normal != WindowState)
            {
                // 最小化最大化状態の時、元の位置と大きさを保存
                Setting.Location = RestoreBounds.Location;
                Setting.Size = RestoreBounds.Size;
            }
            else
            {
                Setting.Location = Location;
                Setting.Size = Size;
            }
            Setting.NameColumnWidth = dataGridViewNotes.Columns["name"].Width;
            Setting.Save(_configPath);
            Tools.SaveUsers(Users);

            Application.Exit();
        }
        #endregion

        #region ロード時
        // ロード時
        private void FormMain_Load(object sender, EventArgs e)
        {
            ButtonStart_Click(sender, e);
        }
        #endregion

        #region 画面表示切替
        // 画面表示切替
        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            // F2キーでtime列の表示切替
            if (e.KeyCode == Keys.F2)
            {
                dataGridViewNotes.Columns["time"].Visible = !dataGridViewNotes.Columns["time"].Visible;
            }
            // F3キーでavatar列の表示切替
            if (e.KeyCode == Keys.F3)
            {
                dataGridViewNotes.Columns["avatar"].Visible = !dataGridViewNotes.Columns["avatar"].Visible;
            }
            // F4キーでname列の表示切替
            if (e.KeyCode == Keys.F4)
            {
                dataGridViewNotes.Columns["name"].Visible = !dataGridViewNotes.Columns["name"].Visible;
            }

            if (e.KeyCode == Keys.Escape)
            {
                ButtonSetting_Click(sender, e);
            }

            if (e.KeyCode == Keys.F10)
            {
                var ev = new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0);
                FormMain_MouseClick(sender, ev);
            }

            if (e.KeyCode == Keys.F9)
            {
                var ev = new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0);
                FormMain_MouseDoubleClick(sender, ev);
            }
        }
        #endregion

        #region マニアクス表示
        private void FormMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (_formManiacs == null || _formManiacs.IsDisposed)
                {
                    _formManiacs = new FormManiacs
                    {
                        MainForm = this
                    };
                }
                if (!_formManiacs.Visible)
                {
                    _formManiacs.Show(this);
                }
            }
        }
        #endregion

        #region リレーリスト表示
        private void ButtonRelayList_Click(object sender, EventArgs e)
        {
            _formRelayList = new FormRelayList();
            if (_formRelayList.ShowDialog(this) == DialogResult.OK)
            {
                ButtonStop_Click(sender, e);
                ButtonStart_Click(sender, e);
            }
            _formRelayList.Dispose();
        }
        #endregion

        #region グリッドキー入力
        private void DataGridViewNotes_KeyDown(object sender, KeyEventArgs e)
        {
            // Wキーで選択行を上に
            if (e.KeyCode == Keys.W)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index > 0)
                {
                    dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index - 1].Selected = true;
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", dataGridViewNotes.SelectedRows[0].Index];
                }
            }
            // Sキーで選択行を下に
            if (e.KeyCode == Keys.S)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index < dataGridViewNotes.Rows.Count - 1)
                {
                    dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index + 1].Selected = true;
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", dataGridViewNotes.SelectedRows[0].Index];
                }
            }
            // Shift + Wキーで選択行を最上部に
            if (e.KeyCode == Keys.W && e.Shift)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index > 0)
                {
                    dataGridViewNotes.Rows[0].Selected = true;
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", 0];
                }
            }
            // Shift + Sキーで選択行を最下部に
            if (e.KeyCode == Keys.S && e.Shift)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index < dataGridViewNotes.Rows.Count - 1)
                {
                    dataGridViewNotes.Rows[^1].Selected = true; // インデックス演算子 [^i] で「後ろからi番目の要素」
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", dataGridViewNotes.Rows.Count - 1];
                }
            }
            // Webビュー表示
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index >= 0)
                {
                    var mev = new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0);
                    var ev = new DataGridViewCellMouseEventArgs(0, dataGridViewNotes.SelectedRows[0].Index, 0, 0, mev);
                    DataGridViewNotes_CellMouseClick(sender, ev);
                }
            }
            // Zキーでnote列の折り返し切り替え
            if (e.KeyCode == Keys.Z)
            {
                var ev = new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0);
                FormMain_MouseDoubleClick(sender, ev);
            }
        }
        #endregion

        #region フォームマウスダブルクリック
        private void FormMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (dataGridViewNotes.Columns["note"].DefaultCellStyle.WrapMode != DataGridViewTriState.True)
            {
                dataGridViewNotes.Columns["note"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
            else
            {
                dataGridViewNotes.Columns["note"].DefaultCellStyle.WrapMode = DataGridViewTriState.NotSet;
            }
        }
        #endregion

        #region セル右クリック
        private void DataGridViewNotes_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                dataGridViewNotes.Rows[e.RowIndex].Selected = true;
                dataGridViewNotes.Rows[e.RowIndex].Cells["note"].Selected = true;

                var id = dataGridViewNotes.Rows[e.RowIndex].Cells["id"].Value.ToString() ?? string.Empty;
                NIP19.NostrEventNote nostrEventNote = new()
                {
                    EventId = id,
                    Relays = [string.Empty],
                };
                var nevent = nostrEventNote.ToNIP19();
                var settings = Notifier.Settings;
                var app = new ProcessStartInfo
                {
                    FileName = settings.FileName + nevent,
                    UseShellExecute = true
                };
                try
                {
                    Process.Start(app);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                Focus();
            }
        }
        #endregion

        #region フォーム最初の表示時
        private void FormMain_Shown(object sender, EventArgs e)
        {
            dataGridViewNotes.Focus();
        }
        #endregion
    }
}