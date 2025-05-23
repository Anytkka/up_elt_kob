using Project.Classes;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Project.Classes.Common;

namespace Project.Pages
{
    public partial class editProfil : Page
    {
        private User _currentUser;
        private string _tempImagePath;

        public editProfil(User user)
        {
            InitializeComponent();
            _currentUser = user ?? new User(0, "", "", "", "");
            LoadProfileImage();
            LoadUserData();
            InitializePlaceholders();
        }

        private void LoadProfileImage()
        {
            try
            {
                if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.ProfileImagePath?.Trim()))
                {
                    Console.WriteLine($"ProfileImagePath: {_currentUser.ProfileImagePath}, Exists: {File.Exists(_currentUser.ProfileImagePath)}");
                    if (File.Exists(_currentUser.ProfileImagePath))
                    {
                        profileImage.Source = new BitmapImage(new Uri(_currentUser.ProfileImagePath, UriKind.Absolute));
                    }
                    else
                    {
                        Console.WriteLine($"File does not exist at: {_currentUser.ProfileImagePath}");
                        profileImage.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
                    }
                }
                else
                {
                    Console.WriteLine("ProfileImagePath is null or empty.");
                    profileImage.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                profileImage.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
            }
        }

        private void LoadUserData()
        {
            txtFullName.Text = string.IsNullOrWhiteSpace(_currentUser.FullName) ? "Фамилия Имя Отчество" : _currentUser.FullName;
            txtEmail.Text = string.IsNullOrWhiteSpace(_currentUser.Email) ? "client@example.com" : _currentUser.Email;
            txtBio.Text = _currentUser.Biography ?? "";
        }

        private void InitializePlaceholders()
        {
            txtFullName.GotFocus += RemoveText;
            txtFullName.LostFocus += AddText;
            txtEmail.GotFocus += RemoveText;
            txtEmail.LostFocus += AddText;

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
                txtFullName.Text = "Фамилия Имя Отчество";
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
                txtEmail.Text = "client@example.com";
        }

        private void Bt6_Edit_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите изображение профиля"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                    if (fileInfo.Length > 5 * 1024 * 1024)
                    {
                        MessageBox.Show("Размер изображения не должен превышать 5MB",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(openFileDialog.FileName);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();

                    profileImage.Source = image;
                    _tempImagePath = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Bt6_Delete_Click(object sender, RoutedEventArgs e)
        {
            _currentUser.ProfileImagePath = null;
            profileImage.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
            _tempImagePath = null;
        }

        private void SaveProfileImage()
{
    try
    {
        if (!string.IsNullOrEmpty(_currentUser.ProfileImagePath) && File.Exists(_currentUser.ProfileImagePath))
        {
            File.Delete(_currentUser.ProfileImagePath);
        }

        if (_tempImagePath != null)
        {
            string projectImagePath = @"C:\Users\User-PC\Source\Repos\up_elt_kobук\Project\Image"; // Замените на ваш путь
            Directory.CreateDirectory(projectImagePath); // Убедимся, что папка существует

            string baseFileName = Path.GetFileNameWithoutExtension(_tempImagePath);
            string extension = Path.GetExtension(_tempImagePath);
            string newFileName = $"{_currentUser.Id}_{baseFileName}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            string newImagePath = Path.Combine(projectImagePath, newFileName);

            File.Copy(_tempImagePath, newImagePath, true);
            _currentUser.ProfileImagePath = newImagePath;

            if (File.Exists(newImagePath))
            {
                Console.WriteLine($"File successfully copied to: {newImagePath}");
            }
            else
            {
                Console.WriteLine($"Failed to copy file to: {newImagePath}");
                throw new IOException($"Не удалось скопировать файл изображения в {newImagePath}.");
            }
        }
        else if (profileImage.Source.ToString().Contains("avata.jpg"))
        {
            _currentUser.ProfileImagePath = null;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Ошибка при сохранении изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || txtFullName.Text == "Фамилия Имя Отчество")
            {
                MessageBox.Show("Пожалуйста, введите ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || txtEmail.Text == "client@example.com")
            {
                MessageBox.Show("Пожалуйста, введите email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Пожалуйста, введите email в формате xx@xx.xx",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (txtEmail.Text != _currentUser.Email && IsEmailExists(txtEmail.Text))
            {
                MessageBox.Show("Пользователь с таким email уже существует",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsEmailExists(string email)
        {
            var connection = Connection.OpenConnection();
            try
            {
                string query = "SELECT COUNT(*) FROM user WHERE email = @email AND id != @id";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@id", _currentUser.Id);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке email: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            finally
            {
                Connection.CloseConnection(connection);
            }
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private void UpdateUserData()
        {
            _currentUser.Email = txtEmail.Text;
            _currentUser.FullName = txtFullName.Text;
            _currentUser.Biography = string.IsNullOrWhiteSpace(txtBio.Text) ? null : txtBio.Text;

            if (!string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                _currentUser.Password = txtPassword.Password;
            }
        }

        private void Bt6_SavePA_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            SaveProfileImage();
            UpdateUserData();

            try
            {
                var userContext = new UserContext(
                    _currentUser.Id,
                    _currentUser.Email,
                    _currentUser.Password,
                    _currentUser.FullName,
                    _currentUser.Biography,
                    _currentUser.ProfileImagePath
                );

                userContext.Update();

                App.CurrentUser = _currentUser;
                MessageBox.Show("Данные профиля успешно обновлены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.Navigate(new PersonalAccount());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Bt6_DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить свой профиль? Все ваши данные будут безвозвратно утеряны.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (!string.IsNullOrEmpty(_currentUser.ProfileImagePath) && File.Exists(_currentUser.ProfileImagePath))
                    {
                        File.Delete(_currentUser.ProfileImagePath);
                    }

                    var userContext = new UserContext(
                        _currentUser.Id,
                        _currentUser.Email,
                        _currentUser.Password,
                        _currentUser.FullName,
                        _currentUser.Biography,
                        _currentUser.ProfileImagePath
                    );

                    userContext.Delete();

                    App.CurrentUser = null;
                    NavigationService?.Navigate(new Login());

                    MessageBox.Show("Ваш профиль был успешно удален.", "Удаление завершено",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении профиля: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemoveText(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == "Фамилия Имя Отчество" || textBox.Text == "client@example.com")
                {
                    textBox.Text = "";
                    textBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }

        private void AddText(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (textBox.Name == "txtFullName")
                    {
                        textBox.Text = "Фамилия Имя Отчество";
                    }
                    else if (textBox.Name == "txtEmail")
                    {
                        textBox.Text = "client@example.com";
                    }
                    textBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
            }
        }
    }
}