using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FontLibBuilder
{
    class StringUtils
    {
        public static string EscapeString(string source)
        {
            Log.D($"正在对字符串 {source} 进行转义");
            StringBuilder sb = new StringBuilder(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == '\\')
                {
                    i++;
                    if (i == source.Length)
                        throw new ArgumentException("Escape sequence starting at end of string", source);
                    switch (source[i])
                    {
                        case '\\':
                            sb.Append('\\');
                            break;
                        case '\'':
                            sb.Append('\'');
                            break;
                        case '"':
                            sb.Append('\"');
                            break;
                        default:
                            break;
                    }
                }
                else sb.Append(source[i]);
            }
            var result = sb.ToString();
            Log.D($"转义完成，结果：{result}");
            return result;
        }

        public static IEnumerable<string> SearchForStrings(string source)
        {
            
            Regex regex = new Regex("((?<![\\\\])['\"])((?:.(?!(?<![\\\\])\\1))*.?)\\1");
            var matches = regex.Matches(source);
            foreach (Match match in matches)
            {
                Log.D($"正在处理字符串 {match.Value}");
                bool ignore = false;
                char[] quotes = { '\'', '"' };
                char? currentQuoteMark = null;
                for (int i = match.Index - 1; i >= 0; i--)
                {
                    if (!currentQuoteMark.HasValue)
                    {
                        if (new string(new char[] { source[i], source[i + 1] }) == "//")
                        {
                            Log.D("找到注释，忽略此字符串。");
                            ignore = true;
                            break;
                        }
                        else if (source[i] == '#')
                        {
                            const string defineString = "#define";
                            if (new string(source.Skip(i).Take(defineString.Length).ToArray()) == defineString)
                            {
                                // 找到预处理器定义时暂不中断，以防注释。
                                Log.D("找到预处理器定义，暂不中断。");
                            }
                            else
                            {
                                Log.D("找到预处理器，忽略此字符串。");
                                ignore = true;
                                break;
                            }
                        }
                        else if (source[i] == '\n')
                        {
                            Log.D($"找到换行符，中断。");
                            break;
                        }
                        else if (quotes.Contains(source[i]))
                        {
                            currentQuoteMark = source[i];
                            Log.D($"进入引号（引号符：{currentQuoteMark.Value}）。");
                        }
                    }
                    else
                    {
                        if (i > 0)
                        {
                            // 如果当前字符是双引号，并且非转义后的结果
                            if (source[i] == currentQuoteMark.Value && source[i - 1] != '\\')
                            {
                                Log.D($"退出引号（引号符：{currentQuoteMark.Value}）。");
                                currentQuoteMark = null;
                            }
                        }
                        else
                        {
                            Log.D($"到达文件头。");
                            // 到了文件头
                            break;
                        }
                    }
                }
                var newString = new string(match.Value.Skip(1).Take(match.Value.Length - 2).ToArray());
                if (!ignore)
                {
                    Log.D($"包括字符串 \"{newString}\"");
                    yield return newString;
                }
                else
                {
                    Log.D($"忽略字符串 \"{newString}\"");
                }
            }
        }
    }
}
