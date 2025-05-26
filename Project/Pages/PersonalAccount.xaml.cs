using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;

namespace Project.Pages
{
    public partial class PersonalAccount : Page
    {
        public PersonalAccount()
        {
            InitializeComponent();
            RefreshUserData();
        }

        private void RefreshUserData()
        {
            this.DataContext = App.CurrentUser ?? new object(); // Защита от null
            LoadProfileImage();
        }

        private void LoadProfileImage()
        {
            var profileImageControl = this.profileImage;
            var leftProfileImageControl = this.leftProfileImage;
            if (profileImageControl == null || leftProfileImageControl == null)
            {
                MessageBox.Show("Один из элементов изображения 'profileImage' или 'leftProfileImage' не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Console.WriteLine($"App.CurrentUser: {App.CurrentUser != null}");
                if (App.CurrentUser != null && !string.IsNullOrEmpty(App.CurrentUser.ProfileImagePath?.Trim()))
                {
                    Console.WriteLine($"Попытка загрузки изображения из: {App.CurrentUser.ProfileImagePath}");
                    if (File.Exists(App.CurrentUser.ProfileImagePath))
                    {
                        Console.WriteLine("Файл существует, загружаем...");
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(App.CurrentUser.ProfileImagePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        profileImageControl.Source = bitmap;
                        leftProfileImageControl.Source = bitmap;
                    }
                    else
                    {
                        Console.WriteLine($"Файл не найден по пути: {App.CurrentUser.ProfileImagePath}");
                        profileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
                        leftProfileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
                    }
                }
                else
                {
                    Console.WriteLine("ProfileImagePath пустой или null, загружаем изображение по умолчанию.");
                    profileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
                    leftProfileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                profileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
                leftProfileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
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
            RefreshUserData();
        }
    }
}
