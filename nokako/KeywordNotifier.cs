using nokako.Properties;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace nokako
{
    public class NotifierSettings
    {
        [JsonPropertyName("mute_mostr")]
        public bool MuteMostr { get; set; }
        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = [];
        [JsonPropertyName("balloon")]
        public bool Balloon { get; set; }
        [JsonPropertyName("open_file")]
        public bool Open { get; set; }
        [JsonPropertyName("file_name")]
        public string FileName { get; set; } = string.Empty;
        [JsonPropertyName("npub")]
        public string Npub { get; set; } = string.Empty;
    }

    public class KeywordNotifier
    {
        public NotifierSettings Settings { get; set; } = new();

        private bool _muteMostr = false;
        private List<string> _keywords = [];
        private bool _shouldShowBalloon = false;
        private bool _shouldOpenFile = false;
        private string _fileName = "https://lumilumi.app/";
        private string _npub = string.Empty;

        private readonly NotifyIcon _notifyIcon;
        private readonly string _keywordsJsonPath = Path.Combine(Application.StartupPath, "keywords.json");
        private readonly JsonSerializerOptions _options = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true,
        };

        public KeywordNotifier()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = Resources.nokakoi
            };
            LoadSettings();

            Settings = new NotifierSettings()
            {
                MuteMostr = _muteMostr,
                Keywords = _keywords,
                Balloon = _shouldShowBalloon,
                Open = _shouldOpenFile,
                FileName = _fileName,
                Npub = _npub
            };

            SaveSettings();
        }

        public void SaveSettings()
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(Settings, _options);
                File.WriteAllText(_keywordsJsonPath, jsonContent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void LoadSettings()
        {
            if (File.Exists(_keywordsJsonPath))
            {
                try
                {
                    var jsonContent = File.ReadAllText(_keywordsJsonPath);
                    var settings = JsonSerializer.Deserialize<NotifierSettings>(jsonContent, _options);
                    if (settings != null)
                    {
                        _muteMostr = settings.MuteMostr;
                        _keywords = settings.Keywords;
                        _shouldShowBalloon = settings.Balloon;
                        _shouldOpenFile = settings.Open;
                        _fileName = settings.FileName;
                        _npub = settings.Npub;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public string? CheckPost(string post)
        {
            foreach (var keyword in _keywords)
            {
                //// 正規表現パターンを作成し、単語境界を考慮
                //var pattern = $@"\b{Regex.Escape(keyword)}\b";
                //if (Regex.IsMatch(post, pattern))
                if (post.Contains(keyword))
                {
                    if (_shouldShowBalloon)
                    {
                        _notifyIcon.Visible = true;
                        _notifyIcon.BalloonTipTitle = "Keyword Notifier : " + keyword;
                        _notifyIcon.BalloonTipText = post;
                        _notifyIcon.ShowBalloonTip(3000);
                        _notifyIcon.Visible = false;
                    }
                    return keyword;
                }
            }
            return null;
        }
    }
}