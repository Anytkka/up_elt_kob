using MySql.Data.MySqlClient;
using Project.Classes.Common;
using Project.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace Project.Pages
{
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Bt_Login(object sender, RoutedEventArgs e)
        {
            string email = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (!email.Contains("@") || !email.Contains(".") || email.Length < 5)
            {
                MessageBox.Show("Email должен быть в формате xx@xx.xx");
                return;
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    var SQL = new MySqlCommand(
                        "SELECT * FROM user WHERE email = @email AND password = @password LIMIT 1",
                        connection);

                    SQL.Parameters.AddWithValue("@email", email);
                    SQL.Parameters.AddWithValue("@password", password);

                    using (var reader = SQL.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string profileImagePath = reader.IsDBNull(reader.GetOrdinal("image")) ? null : reader.GetString("image");

                            App.CurrentUser = new User(
                                reader.GetInt32("id"),
                                reader.GetString("email"),
                                reader.GetString("password"),
                                reader.GetString("fullName"),
                                reader.IsDBNull(reader.GetOrdinal("biography")) ? null : reader.GetString("biography"),
                                profileImagePath
                            );

                            Console.WriteLine($"Успешный вход. App.CurrentUser.ProfileImagePath: {App.CurrentUser.ProfileImagePath}");
                            NavigationService?.Navigate(new PersonalAccount());
                        }
                        else
                        {
                            MessageBox.Show("Неверный email или пароль");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}");
            }
        }

        private void RegisterText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Registration());
        }
    }
}

