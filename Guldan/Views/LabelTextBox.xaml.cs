using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;

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
            if (IsDigitalOnly)
            {
                if (int.TryParse(newValue, out int port))
                {//bla bla bla bla
                    if (port < 1 || port > 65535)
                    {
                        Value = txtContent.Text = oldValue;
                        txtContent.SelectionStart = Value.Length;
                        return;
                    }
                }
            }
            if (txtPasswordbox.Password != newValue)
                txtPasswordbox.Password = newValue;
            if (txtContent.Text != newValue)
                txtContent.Text = newValue;
        }

        #endregion

        #region IsPassword

        public static readonly DependencyProperty IsPasswordProperty =
            DependencyProperty.Register("IsPassword", typeof(bool), typeof(LabelTextBox),
                new PropertyMetadata(false, OnIsPasswordChanged));

        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        private static void OnIsPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (LabelTextBox)d;
            var oldHeader = (bool)e.OldValue;
            var newHeader = ctrl.IsPassword;
            ctrl.OnIsPasswordChanged(oldHeader, newHeader);
        }

        void OnIsPasswordChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                txtContent.Visibility = Visibility.Hidden;
                txtPasswordbox.Visibility = Visibility.Visible;
            }
            else
            {
                txtContent.Visibility = Visibility.Visible;
                txtPasswordbox.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #endregion

        public bool IsDigitalOnly { get; set; }

        private bool _isProtected;
        public bool IsProtected
        {
            get => _isProtected;
            set
            {
                _isProtected = value;
                if (_isProtected)
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
            if (IsDigitalOnly)
            {
                return Array.TrueForAll(text.ToCharArray(),
                    c => char.IsDigit(c) || char.IsControl(c));
            }
            return true;
        }

        private void TxtContent_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsPassword)
                Value = txtContent.Text;
        }

        private void TxtPasswordbox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (IsPassword)
                Value = txtPasswordbox.Password;
        }

        private void TxtPasswordbox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.IsKeyDown(Key.LeftCtrl) || e.Key == Key.C && Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;
                Clipboard.SetText(Value);
            }
        }
    }


    #region PwdHelper
    public static class PasswordHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password",
                typeof(string), typeof(PasswordHelper),
                new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",
                typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
                typeof(PasswordHelper));


        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject dp, string value)
        {
            dp.SetValue(PasswordProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static void OnPasswordPropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox == null) return;
            passwordBox.PasswordChanged -= PasswordChanged;

            if (!GetIsUpdating(passwordBox))
            {
                passwordBox.Password = (string)e.NewValue;
            }
            passwordBox.PasswordChanged += PasswordChanged;
        }

        private static void Attach(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;

            if (passwordBox == null)
                return;

            if ((bool)e.OldValue)
            {
                passwordBox.PasswordChanged -= PasswordChanged;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            SetIsUpdating(passwordBox, true);
            SetPassword(passwordBox, passwordBox?.Password);
            SetIsUpdating(passwordBox, false);
        }
    }
    #endregion
}
