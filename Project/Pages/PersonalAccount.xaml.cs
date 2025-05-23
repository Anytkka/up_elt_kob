using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Project.Pages
{
    public partial class PersonalAccount : Page
    {
        public PersonalAccount()
        {
            InitializeComponent();
            this.DataContext = App.CurrentUser ?? new object(); // Защита от null
            LoadProfileImage();
        }

        private void LoadProfileImage()
        {
            var profileImageControl = this.profileImage;
            if (profileImageControl == null)
            {
                MessageBox.Show("Элемент изображения 'profileImage' не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (App.CurrentUser != null && !string.IsNullOrEmpty(App.CurrentUser.ProfileImagePath) && System.IO.File.Exists(App.CurrentUser.ProfileImagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(App.CurrentUser.ProfileImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    profileImageControl.Source = bitmap;
                }
                else
                {
                    // Используем pack:// для доступа к ресурсу в проекте
                    profileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg"));
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку и загружаем изображение по умолчанию
                Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                profileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg"));
            }
        }

        private void Bt_Projects(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Project1());
        }

        private void Bt_EditProfile(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new editProfil(App.CurrentUser));
        }

        private void Bt_Exit(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            NavigationService?.Navigate(new Login());
        }

        private void PAText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Вместо создания новой страницы просто обновляем текущую
            LoadProfileImage();
            this.DataContext = App.CurrentUser ?? new object();
        }
    }
}