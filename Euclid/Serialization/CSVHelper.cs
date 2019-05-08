using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Euclid.Serialization
{
    /// <summary>
    /// CSV Helper
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>The culture's list separator</summary>
        public static string Separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        /// <summary>Parses a Csv to a jagged array</summary>
        /// <param name="fileName"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[][] CsvContent(string fileName, string separator)
        {
            string[] lines = File.ReadAllLines(fileName);

            List<string[]> result = new List<string[]>();
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmedLine = lines[i].Trim();
                if (trimmedLine.Length > 0)
                {
                    string[] currentLine = trimmedLine.Split(new string[] { separator }, StringSplitOptions.None);
                    if (currentLine.Length > 0)
                        result.Add(currentLine.Select(s => s.Trim()).ToArray());
                }
            }
            return result.ToArray();
        }

        public static bool IsWellFormed(string[][] data)
        {
            if (data.Length == 0) return false;
            return data.All(a => a.Length == data[0].Length);
        }
    }
}
