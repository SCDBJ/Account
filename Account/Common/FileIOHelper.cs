using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Account.Common
{
    public class FileIOHelper
    {
        /// <summary>
        /// 遍历文件夹下的文件
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static IList<string>? FindDirectory(string filepath)
        {
            DirectoryInfo theFolder = new DirectoryInfo(filepath);
            if (!theFolder.Exists)
            {
                return null;
            }
            IList<string> jsonList = new List<string>();
            foreach (FileInfo file in theFolder.GetFiles().OrderBy(t => t.Name))
            {
                var text = ReadText(file.FullName);
                jsonList.Add(text);
            }
            return jsonList;
        }
        /// <summary>
        /// 读取文本
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadText(string path)
        {
            var content = File.ReadAllText(path, Encoding.UTF8);
            var decodeText = HttpUtility.UrlDecode(content);
            var json = Regex.Match(decodeText, @"salarydetail{(?<msg>[\s\S]*?),""salaryItem""", RegexOptions.Multiline).Groups["msg"].Value;
            json = "{" + json + "}";
            return json;
        }
    }
}
