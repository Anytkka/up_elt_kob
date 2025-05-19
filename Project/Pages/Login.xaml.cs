using Project.Classes;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project.Pages
{
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();

            txtUsername.GotFocus += RemoveText;
            txtUsername.LostFocus += AddText;
            txtPassword.GotFocus += RemoveText;
            txtPassword.LostFocus += AddText;

            AddText(txtUsername, null);
            AddText(txtPassword, null);
        }

        private void Bt_Login(object sender, RoutedEventArgs e)
        {
            string email = txtUsername.Text;
            string password = txtPassword.Text;

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Пожалуйста, введите email в формате xx@xx.xx");
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля");
                return;
            }

            User user = User.GetUserByCredentials(email, password);

            if (user != null)
            {
                App.CurrentUser = user;
                NavigationService?.Navigate(new Pages.PersonalAccount(user));
            }
            else
            {
                MessageBox.Show("Неверный email или пароль");
            }
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private void RegisterText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Registration());
        }

        public void RemoveText(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == "Почта xx@xx.xx" || textBox.Text == "Пароль")
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        public void AddText(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Name == "txtUsername")
                {
                    textBox.Text = "Почта xx@xx.xx";
                }
                else if (textBox.Name == "txtPassword")
                {
                    textBox.Text = "Пароль";
                }
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}