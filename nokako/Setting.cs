using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace nokako
{
    public static class Setting
    {
        private static Data _data = new();

        #region データクラス
        /// <summary>
        /// 設定データクラス
        /// </summary>
        public class Data
        {
            public Point Location { get; set; }
            public Size Size { get; set; } = new Size(400, 400);
            public int NameColumnWidth { get; set; } = 70;

            public bool TopMost { get; set; } = false;
            public double Opacity { get; set; } = 1.00;
            public bool ShowOnlyFollowees { get; set; } = false;
            public bool MinimizeToTray { get; set; } = false;
            public bool AddClient { get; set; } = true;
            public string GridColor { get; set; } = "#FF1493";
            public string ReactionColor { get; set; } = "#FFFFE0";
            public string ReplyColor { get; set; } = "#E6E6FA";
            public bool CheckUserClient { get; set; } = false;
        }
        #endregion

        #region プロパティ
        public static Point Location
        {
            get => _data.Location;
            set => _data.Location = value;
        }
        public static Size Size
        {
            get => _data.Size;
            set => _data.Size = value;
        }
        public static int NameColumnWidth
        {
            get => _data.NameColumnWidth;
            set => _data.NameColumnWidth = value;
        }

        public static bool TopMost
        {
            get => _data.TopMost;
            set => _data.TopMost = value;
        }
        public static double Opacity
        {
            get => _data.Opacity;
            set => _data.Opacity = value;
        }
        public static bool ShowOnlyFollowees
        {
            get => _data.ShowOnlyFollowees;
            set => _data.ShowOnlyFollowees = value;
        }
        public static bool MinimizeToTray
        {
            get
            {
                return _data.MinimizeToTray;
            }
            set
            {
                _data.MinimizeToTray = value;
            }
        }
        public static bool AddClient
        {
            get => _data.AddClient;
            set => _data.AddClient = value;
        }

        public static string GridColor
        {
            get => _data.GridColor;
            set => _data.GridColor = value;
        }
        public static string ReactionColor
        {
            get => _data.ReactionColor;
            set => _data.ReactionColor = value;
        }
        public static string ReplyColor
        {
            get => _data.ReplyColor;
            set => _data.ReplyColor = value;
        }
        public static bool CheckUserClient
        {
            get => _data.CheckUserClient;
            set => _data.CheckUserClient = value;
        }
        #endregion

        #region 設定ファイル操作
        /// <summary>
        /// 設定ファイル読み込み
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Load(string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Data));
                var xmlSettings = new XmlReaderSettings();
                using var streamReader = new StreamReader(path, Encoding.UTF8);
                using var xmlReader = XmlReader.Create(streamReader, xmlSettings);
                _data = serializer.Deserialize(xmlReader) as Data ?? _data;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 設定ファイル書き込み
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Save(string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Data));
                using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
                serializer.Serialize(streamWriter, _data);
                streamWriter.Flush();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        #endregion
    }
}
