using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Guldan
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Events

        enum SplitViewMenuWidth
        {
            Narrow = 48,
            Wide = 240
        }

        private int GetColumnZeroWidth()
        {
            return WindowState == WindowState.Maximized ? (int)SplitViewMenu.Width : (int)SplitViewMenuWidth.Narrow;
        }

        private void OnMenuButtonClicked(object sender, RoutedEventArgs e)
        {
            SplitViewMenu.Width = (int)SplitViewMenu.Width == (int)SplitViewMenuWidth.Narrow ? (int)SplitViewMenuWidth.Wide : (int)SplitViewMenuWidth.Narrow;
            RootGrid.ColumnDefinitions[0].Width = new GridLength(GetColumnZeroWidth());
        }

        private void OnStackPanelButtonChecked(object sender, RoutedEventArgs e)
        {
            var c = DataContext as MainWindowViewModel;
            if (c == null) return;
            var btn = sender as RadioButton;
            if (btn == null) return;
            SplitViewMenu.Width = GetColumnZeroWidth();
            if (Enum.TryParse(btn.Name.Replace("Button", ""), out AppMode def))
                c.CurrentAppMode = def;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void OnImportSourceChanged(object sender, DataTransferEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            SetTooltip(textBlock);
        }

        private void OnTextSizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            SetTooltip(textBlock);
        }

        private static void SetTooltip(TextBlock textBlock)
        {
            if (textBlock == null)
                return;

            textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var width = textBlock.DesiredSize.Width;

            ToolTipService.SetToolTip(textBlock, textBlock.ActualWidth < width ? textBlock.Text : null);
        }
        #endregion
    }
}
