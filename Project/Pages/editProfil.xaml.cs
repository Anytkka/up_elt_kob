using Project.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using Project.Classes.Common;
using Project.Main;
using System;

namespace Project.Pages
{
    public partial class editProfil : Page
    {
        private User _currentUser;

        public editProfil(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadUserData();
        }

        private void LoadUserData()
        {
            txtFullName.Text = _currentUser.FullName;
            txtEmail.Text = _currentUser.Email;
            txtBio.Text = _currentUser.Biography;
        }

        private void Bt6_Edit(object sender, RoutedEventArgs e)
        {
            // Логика изменения аватара (можно реализовать позже)
            MessageBox.Show("Функция изменения аватара будет реализована в будущем");
        }

        private void Bt6_Delete(object sender, RoutedEventArgs e)
        {
            // Логика удаления аватара
            MessageBox.Show("Аватар удален");
        }

        private void Bt6_SavePA(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("ФИО не может быть пустым");
                return;
            }

            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Введите корректный email в формате xx@xx.xx");
                return;
            }

            try
            {
                _currentUser.FullName = txtFullName.Text;
                _currentUser.Email = txtEmail.Text;
                _currentUser.Biography = txtBio.Text;

                if (!string.IsNullOrEmpty(txtPassword.Password))
                {
                    _currentUser.Password = txtPassword.Password;
                }

                using (var connection = Connection.OpenConnection())
                {
                    string query = @"UPDATE users 
                                   SET full_name = @fullName, 
                                       email = @email, 
                                       biography = @biography" +
                                      (!string.IsNullOrEmpty(txtPassword.Password) ? ", password = @password" : "") +
                                   " WHERE id = @id";

                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@fullName", _currentUser.FullName);
                    command.Parameters.AddWithValue("@email", _currentUser.Email);
                    command.Parameters.AddWithValue("@biography", _currentUser.Biography);
                    command.Parameters.AddWithValue("@id", _currentUser.Id);

                    if (!string.IsNullOrEmpty(txtPassword.Password))
                    {
                        command.Parameters.AddWithValue("@password", _currentUser.Password);
                    }

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Данные успешно сохранены");
                        App.CurrentUser = _currentUser;
                        NavigationService?.Navigate(new PersonalAccount(_currentUser));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void Bt6_DeleteProfile(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить профиль? Это действие нельзя отменить.",
                                      "Подтверждение удаления",
                                      MessageBoxButton.YesNo,
                                      MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                NavigationService?.Navigate(new DeleteAccount(_currentUser));
            }
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}