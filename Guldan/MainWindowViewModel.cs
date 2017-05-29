using Guldan.Models;
using Guldan.Services;
using Guldan.Services.Interfaces;
using Guldan.ViewModel;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using Point = System.Windows.Point;

namespace Guldan
{
    public sealed class MainWindowViewModel : ObservableObject
    {
        #region Win32
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            LOGPIXELSX = 88,
            LOGPIXELSY = 90,
            DESKTOPVERTRES = 117
        }
        #endregion

        #region Construction / Initialization
        private static MainWindowViewModel instance;
        public static MainWindowViewModel Instance => instance ?? (instance = new MainWindowViewModel());

        private readonly IMessageService _messageService = ServiceManager.Instance.GetService<IMessageService>();
        private readonly IWarlockService _warlockService = ServiceManager.Instance.GetService<IWarlockService>();

        public MainWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            Servers = new ObservableCollection<Server>(_warlockService.Servers);
            SelectedServer = Servers.First();
        }
        #endregion

        #region Field

        #region Title
        private string _title = I18N.GetString("Warlock");
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        public string DummyTitle => "Dummy";

        public string CurrVersion => $"V{GetType().Assembly.GetName().Version}";
        #endregion

        #region Server
        private Server _selectedServer;
        public Server SelectedServer { get => _selectedServer; set { _selectedServer = value; OnPropertyChanged(); } }

        public ObservableCollection<Server> Servers { get => _warlockService.Servers; set { _warlockService.Servers = value; OnPropertyChanged(); } }
        #endregion

        #region AppModeTitle
        private string _appModeTitle = "Sabisu";
        public string AppModeTitle { get => _appModeTitle; set => SetProperty(ref _appModeTitle, value); }
        #endregion

        #region CurrentAppMode
        private AppMode _currentAppMode = AppMode.Servers;
        public AppMode CurrentAppMode { get => _currentAppMode; set { _currentAppMode = value; OnAppModeChanged(); OnPropertyChanged(); } }
        #endregion

        #region SummaryStatus
        private Status _status = Status.Ready;
        public Status Status { get => _status; set => SetProperty(ref _status, value); }

        private string _summaryStatus = "NaN";
        public string SummaryStatus { get => _summaryStatus; set => SetProperty(ref _summaryStatus, value); }
        #endregion

        #region TaskbarIcon
        private TaskbarIcon _taskbarIcon;
        public TaskbarIcon TaskbarIcon { get => _taskbarIcon; set { _taskbarIcon?.Dispose(); _taskbarIcon = value; OnPropertyChanged(); } }
        #endregion

        #region IsBusy
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        #endregion

        #region ImportAndExport

        #region InitExportStatus

        private string _initExportStatus = "おはよう";
        public string InitExportStatus { get => _initExportStatus; set => SetProperty(ref _initExportStatus, value); }

        #endregion

        #region ExportStatus

        private string _exportStatus = string.Empty;
        public string ExportStatus { get => _exportStatus; set => SetProperty(ref _exportStatus, value); }

        #endregion

        #region ExportItems

        private ObservableCollection<ImportExportVM> _exportItems;
        public ObservableCollection<ImportExportVM> ExportItems { get => _exportItems; set => SetProperty(ref _exportItems, value); }

        #endregion

        #region ImportSource

        private string _importSource = string.Empty;
        public string ImportSource { get => _importSource; set => SetProperty(ref _importSource, value); }

        #endregion

        #region ImportItems

        private ObservableCollection<ImportExportVM> _importItems;
        public ObservableCollection<ImportExportVM> ImportItems { get => _importItems; set => SetProperty(ref _importItems, value); }

        #endregion

        #endregion

        #endregion

        #region Window
        public void ShowMainWindow()
        {
            var app = Application.Current;
            if (app.MainWindow == null)
            {
                CurrentAppMode = AppMode.Servers;
                app.MainWindow = new MainWindow { DataContext = Instance };
                app.MainWindow.Show();
                app.MainWindow.Activate();
                app.MainWindow.Closing += OnMainWindowClosing;
            }
            else
            {
                app.MainWindow.Focus();
                app.MainWindow.Activate();
            }
            if (TaskbarIcon != null) TaskbarIcon.Visibility = Visibility.Collapsed;
        }
        private void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            ((Window)sender).Closing -= OnMainWindowClosing;
            Application.Current.MainWindow = null;
            CloseMainApplication(!Servers.Any(c => c.enabled));
        }
        public void CloseMainApplication(bool Shutdown)
        {
            if (Shutdown)
            {
                Application.Current.Shutdown();
            }
            else
            {
                MinimizeToTray();
            }
        }
        public void MinimizeToTray()
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Closing -= OnMainWindowClosing;
                mainWindow.Close();
            }

            if (TaskbarIcon == null)
            {
                TaskbarIcon = InitSystemTray();
            }
            TaskbarIcon.Visibility = Visibility.Visible;
        }

        public static TaskbarIcon InitSystemTray()
        {
            var tbIcon = (TaskbarIcon)Application.Current.FindResource("TrayIcon");
            var cm = (ContextMenu)Application.Current.FindResource("SystemTrayMenu");
            if (tbIcon == null || cm == null) throw new Exception("menu not found");
            foreach (var item in cm.Items)
            {
                if (item is MenuItem)
                {
                    var itm = item as MenuItem;
                    itm.Header = I18N.GetString((string)itm.Header);
                }
            }
            cm.MouseEnter += (sender, e) =>
            {
                GetCursorPos(out POINT lpPoint);
                var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                var desktop = g.GetHdc();
                var LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
                var PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
                var logpixelsy = GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSY);
                var screenScalingFactor = PhysicalScreenHeight / (float)LogicalScreenHeight;
                var dpiScalingFactor = logpixelsy / (float)96;
                g.ReleaseHdc();
                if (!(screenScalingFactor > 1) && !(dpiScalingFactor > 1)) return;
                cm.HorizontalOffset = lpPoint.X / dpiScalingFactor;
                cm.VerticalOffset = lpPoint.Y / dpiScalingFactor;
            };
            tbIcon.ContextMenu = cm;
            tbIcon.DataContext = Instance;
            return tbIcon;
        }
        #endregion

        #region Methods
        private async void OnAppModeChanged()
        {
            AppModeTitle = I18N.GetString(_currentAppMode.ToString());
            switch (_currentAppMode)
            {
                case AppMode.Servers:
                    break;
                case AppMode.PacList:
                    break;
                case AppMode.Import:
                    // Nothing to do as of now
                    break;
                case AppMode.Export:
                    await InitExportAsync().ConfigureAwait(false);
                    break;
            }
        }

        private async Task InitExportAsync()
        {
            IsBusy = true;

            await Task.Run(() =>
            {

                if (Servers.Any())
                {
                    var export = Servers.Where(c => !c.IsDefault).Select(svc => new ImportExportVM
                    {
                        Key = svc.Identifier,
                        Data = svc.GetSSUrl(),
                        IsSelected = true
                    }).ToList();
                    ExportItems = new ObservableCollection<ImportExportVM>(export);
                    InitExportStatus = I18N.GetSplitString("Select server(s) to export");
                }
                else
                {
                    InitExportStatus = I18N.GetSplitString("Nothing can export");
                }

            }).ConfigureAwait(false);

            IsBusy = false;
        }

        private void TrySetClipboard<T>(T content)
        {
            try
            {
                if (content is string)
                    Clipboard.SetText(content as string);
                else if (content is BitmapSource)
                    Clipboard.SetImage(content as BitmapSource);
            }
            catch (Exception e)
            {
                _messageService.ShowAsync(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task TrySetClipboardAsync<T>(T content)
        {
            try
            {
                if (content is string)
                    Clipboard.SetText(content as string);
                else if (content is BitmapSource)
                    Clipboard.SetImage(content as BitmapSource);
            }
            catch (Exception e)
            {
                await _messageService.ShowAsync(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        #endregion

        #region Commands
        private ICommand _saveCommand;
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new AsyncCommand<string>(async _ =>
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await Task.Delay(250).ConfigureAwait(false);
                _warlockService.Save();
            }
            catch (Exception e)
            {
                await _messageService.ShowAsync(I18N.GetSplitString(e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error).ConfigureAwait(true);
            }
            IsBusy = false;
        }));

        private ICommand _importFileCommand;
        public ICommand ImportFileCommand => _importFileCommand ?? (_importFileCommand = new AsyncCommand<string>(async status =>
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                if (status == "load")
                {
                    var fileName = await _messageService.ShowOpenFileDialogAsync("Select File", ".json", Tuple.Create("Json Files", "*.json")).ConfigureAwait(true);

                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        IsBusy = false;
                        return;
                    }
                    var cfg = OriginalConfig.Load(fileName);
                    var servers = cfg.configs.Select(svc => new ImportExportVM
                    {
                        Key = svc.FriendlyName,
                        Data = svc.GetSSUrl(),
                        IsSelected = true
                    });
                    ImportItems = new ObservableCollection<ImportExportVM>(servers);
                }
                else if (status == "import")
                {
                    if (ImportItems?.Any() ?? false)
                    {
                        var servers = ImportItems.Where(c => c.IsSelected ?? false).Select(c => c.Data).ToArray();
                        var svcs = Server.ParseMultipleServers(string.Join(Environment.NewLine, servers));
                        foreach (var server in svcs)
                        {
                            Servers.Add(server);
                        }
                        ImportItems.Clear();
                        CurrentAppMode = AppMode.Servers;
                    }
                }
            }
            catch (Exception e)
            {
                await _messageService.ShowAsync(I18N.GetSplitString(e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error).ConfigureAwait(true);
            }
            IsBusy = false;
        }));

        private ICommand _exportCommand;
        public ICommand ExportCommand => _exportCommand ?? (_exportCommand = new AsyncCommand<string>(async _ =>
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, ExportItems.Where(c => c.IsSelected ?? false).Select(c => c.Data)));
            }
            catch (Exception e)
            {
                await _messageService.ShowAsync(I18N.GetSplitString(e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error).ConfigureAwait(true);
            }
            IsBusy = false;
        }));

        private ICommand _explorerCommand;
        public ICommand ExplorerCommand => _explorerCommand ?? (_explorerCommand = new SimpleCommand<string>(x => System.Diagnostics.Process.Start("explorer.exe", x)));

        private ICommand _curdCommand;
        public ICommand CURDCommand => _curdCommand ?? (_curdCommand = new SimpleCommand<string>(x =>
        {
            if (IsBusy) return;
            if (string.IsNullOrEmpty(x)) return;
            switch (x)
            {
                case "c":
                    try
                    {
                        if (SelectedServer != null)
                            Server.CheckServer(SelectedServer);
                        var ns = new Server();
                        Servers.Add(ns);
                        SelectedServer = ns;
                    }
                    catch (Exception e)
                    {
                        _messageService.ShowAsync(I18N.GetSplitString(e.Message));
                    }
                    break;
                case "dup":
                    try
                    {
                        if (SelectedServer == null) return;
                        Server.CheckServer(SelectedServer);
                        var ns = new Server(SelectedServer?.GetSSUrl())
                        {
                            server = "",
                            remarks = SelectedServer?.remarks + DateTime.Now.ToString("yyyyMMddHHmmss")
                        };
                        Servers.Add(ns);
                        SelectedServer = ns;
                    }
                    catch (Exception e)
                    {
                        _messageService.ShowAsync(I18N.GetSplitString(e.Message));
                    }
                    break;
                case "d":
                    if (SelectedServer == null) return;
                    if (SelectedServer.enabled)
                    {
                        _messageService.ShowAsync(I18N.GetSplitString("Please stop first"));
                    }
                    else
                    {
                        Servers.Remove(SelectedServer);
                        SelectedServer = Servers.FirstOrDefault();
                    }
                    break;
            }
        }));

        private ICommand _appCommand;
        public ICommand AppCommand => _appCommand ?? (_appCommand = new SimpleCommand<string>(param =>
        {
            if (IsBusy) return;
            if (string.IsNullOrEmpty(param)) return;
            switch (param)
            {
                case "exit":
                    _warlockService.StopAll();
                    CloseMainApplication(true);
                    break;
                case "about":
                    break;
                case "show":
                    ShowMainWindow();
                    break;
            }
        }));

        private ICommand _clipBoardCommand;
        public ICommand ClipboardCommand => _clipBoardCommand ?? (_clipBoardCommand = new AsyncCommand<string>(async param =>
        {
            if (IsBusy) return;
            if (string.IsNullOrEmpty(param)) return;
            IsBusy = true;
            switch (param)
            {
                case "copytext":
                    await TrySetClipboardAsync(SelectedServer.GetSSUrl()).ConfigureAwait(true);
                    break;
                case "copyimage":
                    await TrySetClipboardAsync(SelectedServer.GetQRCode().ToBitmapSource()).ConfigureAwait(true);
                    break;
                case "paste":
                    var txt = Clipboard.GetText();
                    var prs = Server.ParseMultipleServers(txt);
                    if (prs?.Length > 0)
                    {
                        if (SelectedServer != null && SelectedServer.IsDefault)
                            Servers.Remove(SelectedServer);
                        foreach (var server in prs)
                        {
                            if (!Servers.Contains(server))
                                Servers.Add(server);
                        }
                        SelectedServer = prs.Last();
                    }
                    else
                    {
                        var bread = Clipboard.GetImage();
                        if (bread == null) break;
                        using (var bmp = BitmapFromSource(bread))
                        {
                            var source = new BitmapLuminanceSource(bmp);
                            var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                            var reader = new QRCodeReader();
                            var result = reader.decode(bitmap);
                            if (result != null)
                            {
                                txt = result.Text;
                                prs = Server.ParseMultipleServers("ss://" + txt);
                                if (prs?.Length > 0)
                                {
                                    if (SelectedServer != null && SelectedServer.IsDefault)
                                        Servers.Remove(SelectedServer);
                                    foreach (var server in prs)
                                    {
                                        if (!Servers.Contains(server))
                                            Servers.Add(server);
                                    }
                                    SelectedServer = prs.Last();
                                }
                            }
                        }
                    }
                    break;
            }
            IsBusy = false;
        }));
        #endregion
    }
}
