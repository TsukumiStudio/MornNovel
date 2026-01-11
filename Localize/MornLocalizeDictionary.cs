using System.Collections.Generic;

namespace MornLib
{
    internal sealed class MornLocalizeDictionary
    {
        private readonly string _sheetName;
        private readonly List<Dictionary<string, string>> _strings = new List<Dictionary<string, string>>();
        
        public MornLocalizeDictionary(string sheetName)
        {
            _sheetName = sheetName;
        }
        
        public void AddString(string key, string value, int index, string character)
        {
            var dic = new Dictionary<string, string>
            {
                {"key", key},
                {"string", value},
                {"index", index.ToString()},
                {"character", character}
            };
            _strings.Add(dic);
        }
        
        public string ToJson()
        {
            var json = $"{{\"sheetName\":\"{_sheetName}\",\"strings\":[";
            foreach (var dic in _strings)
            {
                json += "{";
                foreach (var pair in dic)
                {
                    json += $"\"{pair.Key}\":\"{EscapeJsonString(pair.Value)}\",";
                }
                json = json.TrimEnd(',');
                json += "},";
            }
            json = json.TrimEnd(',');
            json += "]}";
            return json;
        }
        
        private static string EscapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            
            return value
                .Replace("\\", "\\\\")  // バックスラッシュのエスケープ
                .Replace("\"", "\\\"")  // ダブルクォーテーションのエスケープ
                .Replace("\n", "\\n")   // 改行のエスケープ
                .Replace("\r", "\\r")   // キャリッジリターンのエスケープ
                .Replace("\t", "\\t");  // タブのエスケープ
        }
    }
}