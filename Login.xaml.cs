using GestionComerce.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace GestionComerce
{
    /// <summary>
    /// Logique d'interaction pour Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        private StringBuilder passwordBuilder = new StringBuilder();

        public Login(MainWindow main)
        {
            InitializeComponent();
            this.main = main;

            Btn0.Click += NumericButton_Click;
            Btn1.Click += NumericButton_Click;
            Btn2.Click += NumericButton_Click;
            Btn3.Click += NumericButton_Click;
            Btn4.Click += NumericButton_Click;
            Btn5.Click += NumericButton_Click;
            Btn6.Click += NumericButton_Click;
            Btn7.Click += NumericButton_Click;
            Btn8.Click += NumericButton_Click;
            Btn9.Click += NumericButton_Click;
            BtnClear.Click += BtnClear_Click;
            BtnDelete.Click += BtnDelete_Click;

            PasswordInput.KeyDown += PasswordInput_KeyDown_Enter;

            this.Loaded += (s, e) =>
            {
                PasswordInput.Focus();
                // Sync icon with current window state and keep it in sync
                var win = Window.GetWindow(this);
                if (win != null)
                    win.StateChanged += (ws, we) => UpdateWinStateIcon();
                UpdateWinStateIcon();
            };
        }

        MainWindow main;

        private void NumericButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string digit)
            {
                passwordBuilder.Append(digit);
                PasswordInput.Password = passwordBuilder.ToString();
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            passwordBuilder.Clear();
            PasswordInput.Password = string.Empty;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (passwordBuilder.Length > 0)
            {
                passwordBuilder.Remove(passwordBuilder.Length - 1, 1);
                PasswordInput.Password = passwordBuilder.ToString();
            }
        }

        private void PasswordInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow digits
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void PasswordInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Block spaces
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        // New method to handle Enter key press
        private void PasswordInput_KeyDown_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Trigger the login button click
                BtnEnter_Click(sender, e);
                e.Handled = true;
            }
        }

        private async void BtnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordInput.Password == "")
            {
                MessageBox.Show("write a password");
                return;
            }

            User u = new User();
            List<User> lu = await u.GetUsersAsync();

            foreach (User user in lu)
            {
                if (user.Code.ToString() == PasswordInput.Password)
                {
                    main.load_main(user);
                    return;
                }
            }

            passwordBuilder.Clear();
            PasswordInput.Password = string.Empty;
            MessageBox.Show("Wrong Code");
        }

        // ── Card drag (WindowStyle="None" removes OS title bar) ──────────────
        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var win = Window.GetWindow(this);
            if (win != null && win.WindowState == WindowState.Normal)
                win.DragMove();
        }

        // ── Windowed ↔ Full-Screen toggle ────────────────────────────────────
        private void WinStateBtn_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this);
            if (win == null) return;

            if (win.WindowState == WindowState.Maximized)
            {
                var area = SystemParameters.WorkArea;
                win.WindowState = WindowState.Normal;
                win.Width  = area.Width  * 0.90;
                win.Height = area.Height * 0.90;
                win.Left   = area.Left + (area.Width  - win.Width)  / 2;
                win.Top    = area.Top  + (area.Height - win.Height) / 2;
            }
            else
            {
                win.WindowState = WindowState.Maximized;
            }
        }

        private void UpdateWinStateIcon()
        {
            var win = Window.GetWindow(this);
            if (win == null) return;

            var icon  = WinStateBtn.Template?.FindName("WinStateIcon",  WinStateBtn) as TextBlock;
            var label = WinStateBtn.Template?.FindName("WinStateLabel", WinStateBtn) as TextBlock;
            if (icon == null || label == null) return;

            if (win.WindowState == WindowState.Maximized)
            {
                icon.Text  = "\uE923"; // BackToWindow
                label.Text = "Fenêtré";
            }
            else
            {
                icon.Text  = "\uE922"; // Maximize
                label.Text = "Plein écran";
            }
        }
        // ─────────────────────────────────────────────────────────────────────

        private void BtnShutdown_Click(object sender, RoutedEventArgs e)
        {
            Exit exit = new Exit(null, 0);
            exit.ShowDialog();
        }
    }
}