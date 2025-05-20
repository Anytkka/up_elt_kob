using MySql.Data.MySqlClient;
using Project.Classes;
using Project.Classes.Common;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Project.Pages
{
    public partial class Registration : Page
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void Bt_Register(object sender, RoutedEventArgs e)
        {
            string fullName = FOI.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все обязательные поля");
                return;
            }

            if (!IsValidFullName(fullName))
            {
                MessageBox.Show("ФИО содержит только буквы латинского алфавита");
                FOI.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают");
                txtPassword.Clear();
                txtConfirmPassword.Clear();
                txtPassword.Focus();
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный email: xx@xx.xx");
                txtEmail.Focus();
                return;
            }

            if (UserExists(email))
            {
                MessageBox.Show("Пользователь с таким email уже зарегистрирован");
                txtEmail.Focus();
                return;
            }

            try
            {
                RegisterUser(fullName, email, password);
                MessageBox.Show("Регистрация прошла успешно!");
                NavigationService?.Navigate(new Login());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return regex.IsMatch(email);
        }

        private bool IsValidFullName(string fullName)
        {
            Regex regex = new Regex(@"^[\p{IsCyrillic}\s]+$");
            return regex.IsMatch(fullName);
        }

        private bool UserExists(string email)
        {
            using (var connection = Connection.OpenConnection())
            {
                string query = "SELECT COUNT(*) FROM user WHERE email = @email";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private void RegisterUser(string fullName, string email, string password)
        {
            var newUser = new UserContext(0, email, password, fullName, "");

            newUser.Add();
        }

        private void LoginText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Login());
        }
    }
}