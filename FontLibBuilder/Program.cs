using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FontLibBuilder
{
    class Program
    {
        internal const int CharCodeLength = 3;

        static void Main(string[] args)
        {
            var options = new Options();
            bool success = Parser.Default.ParseArguments(args, options);

            if (options.Verbose)
            {
                Log.Level = LogType.Debug;
                Log.ShowTime = true;
            }
            else
            {
                Log.Level = LogType.Success;
                Log.ShowTime = false;
            }

            if (options.InputFiles == null)
                options.InputFiles = new string[0];
            if (options.InputDirectories == null)
                options.InputDirectories = new string[0];
            if (options.FileExtensions == null)
                options.FileExtensions = new string[0];
            if (options.Font == null)
                options.Font = "Arial";

            options.FileExtensions = options.FileExtensions.Select(ext => ext.ToLowerInvariant()).ToArray();
            var files = options.InputDirectories
                .Select(dirString => new DirectoryInfo(dirString))
                .SelectMany(dir => dir.EnumerateFiles("*", SearchOption.AllDirectories)
                    .Where(file => options.FileExtensions.Contains(file.Extension.ToLowerInvariant())))
                .Concat(options.InputFiles.Select(name => new FileInfo(name)));

            // var chars = new List<char>();
            var chars = new HashSet<char>();

            Encoding enc = Encoding.GetEncoding(options.Encoding);
            foreach (var file in files)
            {
                Log.I($"正在处理文件 {file.Name}");
                string content;
                using (StreamReader reader = new StreamReader(file.OpenRead(), enc, true))
                {
                    content = reader.ReadToEnd();
                    chars.UnionWith(StringUtils.SearchForStrings(content).Select(str => StringUtils.EscapeString(str)).SelectMany(str => str.ToCharArray()));
                }
                Log.S($"{file.Name} 处理完毕。");
            }

            Log.I($"共 {chars.Count} 个字符。");

            Font font = new Font(options.Font, options.FontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            // 键：表示 CRC8 值
            // 值 Item1: 表示第一个 CRC 值相等的字符在数据中的索引。
            // 值 Item2: 表示最后一个 CRC 值相等的字符在数据中的索引。用于在设置查询链表时寻找上一项。
            var indexTable = new Dictionary<byte, Pair<ushort, ushort>>();
            // Item2 是数据。
            var matrices = new List<Pair<CharacterProperties, byte[]>>();
            ushort currentPos = 0;
            int charImageSize = 0;
            foreach (var ch in chars)
            {
                Log.D($"为 '{ch}' 建立字模。当前序号：{currentPos}");
                // Initalize Properties
                CharacterProperties chProp = new CharacterProperties();

                byte[] chBytes = enc.GetBytes(new char[] { ch });
                Log.D($"编码后长度为 {chBytes.Length}。");

                chProp.CharCode = new byte[CharCodeLength];
                Array.Copy(chBytes, chProp.CharCode, Math.Min(chProp.CharCode.Length, chBytes.Length));

                chProp.NextOccurence = 0xFFFF;

                var image = TextUtils.GenerateImageForChar(ch, font, options.FontSize, options.AnchorToByte, OrderOrientation.Horizontal);
                Log.D($"字符实际宽度 {image.Item2} 像素。");
                chProp.Width = (byte)image.Item2;

                matrices.Add(Pair<CharacterProperties, byte[]>.Create(chProp, image.Item1));

                byte crc = CommonUtils.ComputeCrc8(chBytes);
                Log.D($"CRC 为 0x{crc.ToString("X2")}。");
                // 如果已经有与当前字符 CRC 相同的字符。
                if (indexTable.ContainsKey(crc))
                {
                    // 最后一个 CRC 值与当前字符相同的字符所在位置。
                    ushort lastCrcEqual = indexTable[crc].Item2;
                    matrices[lastCrcEqual].Item1.NextOccurence = currentPos;
                    indexTable[crc].Item2 = currentPos;
                    Log.D($"在第 {lastCrcEqual} 找到同 CRC 文字");
                }
                else
                {
                    // 新建索引。
                    Log.D($"未找到同 CRC 文字，新建索引。");
                    indexTable.Add(crc, Pair<ushort, ushort>.Create(currentPos, currentPos));
                }

                currentPos++;
            }
            Log.S("字模建立完毕。");
            byte[] dataArray;
            using (MemoryStream data = new MemoryStream())
            {
                /* 格式：
                 * [---][--][-][---((字体^2)/8)bytes---]
                 *  cur  nx wid
                 * cur: 3 字节，表示当前字符的编码。
                 * nx: 2 字节，下一个同 Hash 字符所在的位置（若无，则为 65535）。 
                 * wid: 1 字节，表示当前字符实际宽度。
                 */
                foreach (var item in matrices)
                {
                    byte[] propData = new byte[Marshal.SizeOf<CharacterProperties>()];
                    GCHandle handle = GCHandle.Alloc(propData, GCHandleType.Pinned);
                    Marshal.StructureToPtr(item.Item1, handle.AddrOfPinnedObject(), true);
                    handle.Free();
                    data.Write(propData, 0, propData.Length);
                    data.Write(item.Item2, 0, item.Item2.Length);
                }
                dataArray = data.ToArray();
            }

            ushort[] indexArray = Enumerable.Repeat((ushort)0xFFFF, 256).ToArray();
            foreach (var item in indexTable)
            {
                indexArray[item.Key] = item.Value.Item1;
            }
            byte[] indexData;
            using (MemoryStream index = new MemoryStream())
            {
                for (int i = 0; i < 0x100; i++)
                {
                    byte cur = (byte)i;
                    if (indexTable.ContainsKey(cur))
                    {
                        byte[] val = BitConverter.GetBytes(indexTable[cur].Item1);
                        index.Write(val, 0, val.Length);
                    }
                    else
                    {
                        index.Write(new byte[] { 0xFF, 0xFF }, 0, 2);
                    }
                }
                indexData = index.ToArray();
            }

            Log.S("数据生成完毕，开始写入。");
            foreach (var outcfg in options.Output)
            {
                string[] splitted = outcfg.Split(':');
                string templateFileName = splitted[0];
                string outputFileName = splitted[1];
                Log.I($"{templateFileName} -> {outputFileName}");
                string template = File.ReadAllText(splitted[0]);

                string result = template.ReplaceAll(new Replacement[]
                {
                    new Replacement("###Data###",() => dataArray.ToHexString() ),
                    new Replacement("###DataArray###", () => dataArray.ToCStyleArray()),
                    new Replacement("###DataLen###", () => dataArray.Length.ToString()),
                    new Replacement("###Index###", () => indexData.ToHexString()),
                    new Replacement("###IndexArray###", () => indexData.ToCStyleArray()),
                    new Replacement("###FontSize###", ()=>options.FontSize.ToString()),
                    new Replacement("###ImageSize###",() =>TextUtils.GetImageSize(options.FontSize, options.AnchorToByte).ToString())
                });

                File.WriteAllText(outputFileName, template);
                Log.S($"{outputFileName} 写入完成。");
            }
        }
    }
}
