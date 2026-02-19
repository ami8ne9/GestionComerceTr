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

            // Add KeyDown event handler for Enter key
            PasswordInput.KeyDown += PasswordInput_KeyDown_Enter;

            // Set focus to password input when control loads
            this.Loaded += (s, e) => PasswordInput.Focus();
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

        private void BtnShutdown_Click(object sender, RoutedEventArgs e)
        {
            Exit exit = new Exit(null, 0);
            exit.ShowDialog();
        }
    }
}