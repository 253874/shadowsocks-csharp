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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Guldan.Views
{
    /// <summary>
    /// LabelTextBox.xaml の相互作用ロジック
    /// </summary>
    public partial class LabelTextBox
    {
        public LabelTextBox()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(LabelTextBox),
                new PropertyMetadata(string.Empty, OnHeaderChanged));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (LabelTextBox)d;
            var oldHeader = (string)e.OldValue;
            var newHeader = ctrl.Header;
            ctrl.OnHeaderChanged(oldHeader, newHeader);
        }

        void OnHeaderChanged(string oldHeader, string newHeader)
        {
            txtHeader.Text = newHeader;
        }

        #endregion

        #region Value

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(LabelTextBox),
                new PropertyMetadata(string.Empty, OnValueChanged));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (LabelTextBox)d;
            var oldHeader = (string)e.OldValue;
            var newHeader = ctrl.Value;
            ctrl.OnValueChanged(oldHeader, newHeader);
        }

        void OnValueChanged(string oldValue, string newValue)
        {
            txtContent.Text = newValue;
        }

        #endregion

        #endregion

        private bool _isPassword;

        public bool IsPassword
        {
            get => _isPassword;
            set
            {
                _isPassword = value;
                if (_isPassword)
                {
                    BlurEffect.Radius = 4;
                    txtContent.GotFocus += UIElement_OnGotFocus;
                    txtContent.LostFocus += UIElement_OnLostFocus;
                }
                else
                {
                    BlurEffect.Radius = 0;
                    txtContent.GotFocus -= UIElement_OnGotFocus;
                    txtContent.LostFocus -= UIElement_OnLostFocus;
                }
            }
        }

        public bool IsOnlyDigital { get; set; }
        private async void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var ui = sender as UIElement;
            if (ui?.Effect != null)
            {
                var ef = (BlurEffect)ui.Effect;
                var lp = ef.Radius;
                for (var i = lp; i > 0; i -= 0.8)
                {
                    await Task.Delay(20).ConfigureAwait(true);
                    ef.Radius = Math.Max(i, 0);
                }
            }
        }

        private async void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var ui = sender as UIElement;
            if (ui?.Effect != null)
            {
                var ef = (BlurEffect)ui.Effect;
                for (double i = 0; i < 4; i += 0.8)
                {
                    await Task.Delay(20).ConfigureAwait(true);
                    ef.Radius = i;
                }
            }
        }

        private void TxtContent_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsTextAllowed(e.Text)) e.Handled = true;
        }

        private void TxtContent_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text)) e.CancelCommand();
            }
            else e.CancelCommand();
        }

        private bool IsTextAllowed(string text)
        {
            if (IsOnlyDigital)
            {
                return Array.TrueForAll(text.ToCharArray(),
                    c => char.IsDigit(c) || char.IsControl(c));
            }
            return true;
        }

        private void TxtContent_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Value = txtContent.Text;
        }
    }
}
