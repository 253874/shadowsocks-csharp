using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Guldan
{
    public enum AppMode
    {
        None = 0,
        Servers,
        PacList,
        Import,
        Export,
        About
    }

    public enum Status
    {
        Busy = 0,
        Ready,
        Disabled
    }

    #region Global

    public static class G
    {
        static G()
        {
            var fileName = Process.GetCurrentProcess().MainModule.FileName;
            CD = Path.GetDirectoryName(fileName);
            MyName = Path.GetFileNameWithoutExtension(fileName);
        }
        public static string CD { get; }
        public static string MyName { get; }
        private class JsonSerializerStrategy : SimpleJson.PocoJsonSerializerStrategy
        {
            // convert string to int
            public override object DeserializeObject(object value, Type type)
            {
                if (type == typeof(int) && value is string)
                {
                    return int.Parse(value.ToString());
                }
                return base.DeserializeObject(value, type);
            }
        }
        public static string SerializeToJsonString(object obj)
        {
            return SimpleJson.SimpleJson.SerializeObject(obj, new JsonSerializerStrategy());
        }
        public static T DeSerializeJsonObject<T>(string jsonString)
        {
            return SimpleJson.SimpleJson.DeserializeObject<T>(jsonString, new JsonSerializerStrategy());
        }
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
        public static BitmapSource ToBitmapSource(this Bitmap bmp)
        {
            BitmapSource returnSource;
            IntPtr hBitmap = IntPtr.Zero;
            try
            {
                hBitmap = bmp.GetHbitmap();
                returnSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                returnSource = null;
            }
            finally
            {
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
            }
            return returnSource;
        }
    }

    #endregion

    #region I18N

    public class I18N : System.Windows.Data.IValueConverter
    {
        protected static Dictionary<string, string> Strings;

        static I18N()
        {
            Strings = new Dictionary<string, string>
            {
                //Status
                {"busy", "忙碌"},
                {"ready", "准备就绪"},
                {"disabled", "已停止"},
                //ContextMenu
                {"show", "显示"},
                {"exit", "退出"},
                //Mainwindow
                {"warlock", "Shadowfucks"},
                {"servers", "服务器"},
                {"paclist", "PAC管理"},
                {"import", "导入"},
                {"export", "导出"},
                {"about", "关于"},
                {"save", "保存"},
                {"undo", "撤销"},
                //Head
                {"new", "新增"},
                {"duplicate", "复制"},
                {"copytext", "复制文字"},
                {"copyimage", "复制图像"},
                {"readclipboard", "剪贴板读取"},
                {"delete", "删除"},
                {"qrcode", "QR码"},
                {"remark", "备注"},
                {"localaddr", "本地地址"},
                {"localport", "本地端口"},
                {"onetimeauth", "一次性验证"},
                {"selectfile", "选择文件"},
                //Single word
                {"atarashii", "新的"},
                {"server", "服务器"},
                {"local", "本地"},
                {"port", "端口"},
                {"method", "加密方式"},
                {"password", "密码"},
                {"saba", "服务器"},
                {"current", "当前"},
                {"status", "状态"},
                {"cannot", "不能"},
                {"outofrange", "超出范围"},
                {"invalid", "无效的"},
                {"ipaddress", "IP地址"},
                {"select", "选择"},
                {"server(s)", "服务器"},
                {"blank", "空"},
                {"please", "请"},
                {"stop", "停止"},
                {"file", "文件"},
                {"first", "先"},
                {"nothing", "没有东西"},
                {"can", "可以"},
                {"to", "来"},
                {"be", "为"},
                {"is", "为"}
            };
        }

        public static string GetString(string key)
        {
            var lk = key.ToLower();
            return Strings.ContainsKey(lk) ? Strings[lk] : key;
        }

        public static string GetSplitString(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var s in input.Split(' '))
            {
                if (string.IsNullOrWhiteSpace(s)) continue;
                sb.Append(GetString(s));
            }
            return sb.ToString();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Strings == null) return parameter;
            var p = (parameter as string)?.ToLower();
            if (p != null && Strings.ContainsKey(p)) return Strings[p];
            return parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion
}
