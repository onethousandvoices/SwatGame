using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SWAT.Utility
{
    public static class CSVReader
    {
        private const string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private const string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        private static readonly char[] TRIM_CHARS = { '\"' };
 
        public static List<Dictionary<string, object>> Read(string file)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            TextAsset data = Resources.Load(file) as TextAsset;
            
            string[] lines = Regex.Split(data!.text, LINE_SPLIT_RE);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] arr = line.Split(',');
                arr = arr.SkipLast(1).ToArray();
                lines[i] = string.Join(',', arr);
            }

            if (lines.Length <= 1) return list;
 
            string[] header = Regex.Split(lines[0], SPLIT_RE);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || values[0] == "") continue;
 
                Dictionary<string, object> entry = new Dictionary<string, object>();
                for (int j = 0; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    object finalValue = value;

                    if (int.TryParse(value, out int n))
                        finalValue = n;
                    else if (float.TryParse(value, out float f))
                        finalValue = f;
                    
                    entry[header[j]] = finalValue;
                }
                list.Add(entry);
            }
            return list;
        }
    }
}