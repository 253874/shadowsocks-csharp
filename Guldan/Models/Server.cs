using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Guldan.Services;
using Guldan.Services.Interfaces;
using Shadowsocks;

namespace Guldan.Models
{
    public class Server : ObservableObject,IDisposable
    {
        public Server()
        {
            server = "";
            server_port = 8388;
            local = "127.0.0.1";
            local_port = 1083;
            method = "aes-256-cfb";
            password = "";
            remarks = "NaN";
            auth = false;
        }

        #region properties

        private string _server;
        public string server { get => _server; set { _server = value; GenerateQR(); Identifier = string.IsNullOrEmpty(_server) ? null : $"{_server}:{server_port}({remarks})"; OnPropertyChanged(); } }
        private int _server_port;
        public int server_port { get => _server_port; set { _server_port = value; GenerateQR(); Identifier = string.IsNullOrEmpty(_server) ? null : $"{_server}:{server_port}({remarks})"; OnPropertyChanged(); } }
        private string _local;
        public string local { get => _local; set { _local = value; OnPropertyChanged(); } }
        private int _local_port;
        public int local_port { get => _local_port; set { _local_port = value; OnPropertyChanged(); } }
        private string _password;
        public string password { get => _password; set { _password = value; GenerateQR(); OnPropertyChanged(); } }
        private string _method;
        public string method { get => _method; set { _method = value; GenerateQR(); OnPropertyChanged(); } }
        private string _remarks;
        public string remarks { get => _remarks; set { _remarks = value; Identifier = string.IsNullOrEmpty(_server) ? null : $"{_server}:{server_port}({remarks})"; OnPropertyChanged(); } }
        private bool _auth;
        public bool auth { get => _auth; set { _auth = value; OnPropertyChanged(); } }
        private bool _enabled;
        public bool enabled { get => _enabled; set { _enabled = value; OnPropertyChanged(); } }

        #endregion

        #region withoutsave

        private bool _busy;
        [SimpleJson.Ignore]
        public bool Busy { get => _busy; set { _busy = value; OnPropertyChanged(); } }
        private bool _okay;
        [SimpleJson.Ignore]
        public bool Okay { get => _okay; set { _okay = value; OnPropertyChanged(); } }
        private BitmapImage _qrCode;
        [SimpleJson.Ignore]
        public BitmapImage QRCode { get => _qrCode; set { _qrCode = value; OnPropertyChanged(); } }
        private string _identifier;
        [SimpleJson.Ignore]
        public string Identifier { get => _identifier ?? I18N.GetSplitString("Atarashii Saba"); set { _identifier = value; OnPropertyChanged(); } }

        #endregion

        #region methods

        private void GenerateQR()
        {
            try
            {
                CheckServer(server);
                CheckPort(server_port);
                CheckPassword(password);
                CheckMethod(method);
                using (var bitmap = GetQRCode())
                using (var memory = new MemoryStream())
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    QRCode = bitmapImage;
                }
                Okay = true;
            }
            catch
            {
                QRCode = null;
                Okay = false;
                //TODO
            }
        }

        private string ToBase64()
        {
            string parts = method + ":" + password + "@" + server + ":" + server_port;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(parts));
        }

        public string GetSSUrl()
        {
            return "ss://" + ToBase64();
        }

        public System.Drawing.Bitmap GetQRCode(int height = 1024)
        {
            var code = ZXing.QrCode.Internal.Encoder.encode(ToBase64(), ZXing.QrCode.Internal.ErrorCorrectionLevel.M);
            var m = code.Matrix;
            int blockSize = Math.Max(height / m.Height, 1);
            var drawArea = new System.Drawing.Bitmap(m.Width * blockSize, m.Height * blockSize);
            using (var g = System.Drawing.Graphics.FromImage(drawArea))
            {
                g.Clear(System.Drawing.Color.White);
                using (var b = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
                {
                    for (int row = 0; row < m.Width; row++)
                    {
                        for (int col = 0; col < m.Height; col++)
                        {
                            if (m[row, col] != 0)
                            {
                                g.FillRectangle(b, blockSize * row, blockSize * col, blockSize, blockSize);
                            }
                        }
                    }
                }
            }
            return drawArea;
        }


        private readonly IMessageService _messageService = ServiceManager.Instance.GetService<IMessageService>();
        private bool started;

        public void Run(bool isStart)
        {
            if (isStart)
            {
                warlock = Warlock.Affliction(server, server_port, password, method, auth, local, local_port);
                warlock.Start();
                started = true;
            }
            else
            {
                warlock?.Stop();
                started = false;
            }
        }
        private ICommand _startCommand;
        [SimpleJson.Ignore]
        public ICommand StartCommand => _startCommand ?? (_startCommand = new AsyncCommand(async _ =>
        {
            Busy = true;
            await Task.Delay(250).ConfigureAwait(false);
            try
            {
                if (started)
                {
                    enabled = false;
                    Run(false);
                    started = false;
                    MainWindowViewModel.Instance.SaveCommand.Execute(null);
                }
                else
                {
                    if (Okay)
                    {
                        CheckLocal(local);
                        CheckLocalPort(local_port);
                        enabled = true;
                        Run(true);
                        started = true;
                        Busy = false;
                        MainWindowViewModel.Instance.SaveCommand.Execute(null);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                await _messageService.ShowAsync(I18N.GetSplitString(e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error).ConfigureAwait(true);
            }
            Busy = enabled = started = false;
        }));

        private Warlock warlock;
        public void Dispose()
        {
            warlock?.Dispose();
        }
        #endregion

        #region ctor

        public Server(string ssURL) : this()
        {
            string[] r1 = Regex.Split(ssURL, "ss://", RegexOptions.IgnoreCase);
            string base64 = r1[1];
            byte[] bytes = null;
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    bytes = Convert.FromBase64String(base64);
                    break;
                }
                catch (FormatException)
                {
                    base64 += "=";
                }
            }
            if (bytes == null)
            {
                throw new FormatException();
            }
            try
            {
                string data = Encoding.UTF8.GetString(bytes);
                int indexLastAt = data.LastIndexOf('@');

                string afterAt = data.Substring(indexLastAt + 1);
                int indexLastColon = afterAt.LastIndexOf(':');
                server_port = int.Parse(afterAt.Substring(indexLastColon + 1));
                server = afterAt.Substring(0, indexLastColon);

                string beforeAt = data.Substring(0, indexLastAt);
                string[] parts = beforeAt.Split(':');
                method = parts[0];
                password = beforeAt.Remove(0, method.Length + 1);

                //TODO: read one_time_auth
            }
            catch (IndexOutOfRangeException)
            {
                throw new FormatException();
            }
        }
        private static readonly Regex isbase64 = new Regex("[a-zA-z0-9=]+", RegexOptions.Compiled);
        public static Server[] ParseMultipleServers(string input)
        {
            var svcs = new List<Server>();
            try
            {
                var r1 = Regex.Split(input, "ss://", RegexOptions.IgnoreCase);
                foreach (var s in r1)
                {
                    if (string.IsNullOrWhiteSpace(s)) continue;
                    var momoda = isbase64.Match(s);
                    if (!momoda.Success) continue;
                    try
                    {
                        var prs = new Server($"ss://{momoda.Value}");
                        if (!svcs.Contains(prs))
                        {
                            svcs.Add(prs);
                        }
                    }
                    catch
                    {
                        //TODO hehe
                    }
                }
            }
            catch
            {
                //TODO hehe
            }
            return svcs.Count == 0 ? null : svcs.ToArray();
        }

        #endregion

        #region check

        public static void CheckServer(Server server)
        {
            CheckServer(server.server);
            CheckLocal(server.local);
            CheckPort(server.server_port);
            CheckLocalPort(server.local_port);
            CheckPassword(server.password);
            CheckMethod(server.method);
        }

        private static void CheckMethod(string method)
        {
            if (!Warlock.EncryptorList.Contains(method))
                throw new ArgumentException("Invalid method");
        }

        private static void CheckPort(int port)
        {
            if (port <= 0 || port > 65535)
                throw new ArgumentException("Port OutOfRange");
        }

        private static void CheckLocalPort(int port)
        {
            CheckPort(port);
            if (Utils.CheckIfPortInUse(port))
                throw new ArgumentException("Local port Busy");
        }

        private static void CheckPassword(string pwd)
        {
            if (string.IsNullOrEmpty(pwd))
                throw new ArgumentException("Password cannot be blank");
        }

        private static void CheckServer(string server)
        {
            if (string.IsNullOrEmpty(server))
                throw new ArgumentException("Server cannot be blank");
        }

        private static void CheckLocal(string server)
        {
            if (string.IsNullOrEmpty(server))
                throw new ArgumentException("LocalAddr : Local server cannot be blank");
            if (string.CompareOrdinal(server, "localhost") == 0) return;
            if(!IPAddress.TryParse(server,out IPAddress ip))
                throw new ArgumentException("LocalAddr : Invalid IPAddress");
        }

        #endregion
    }
}
