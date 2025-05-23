using Project.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace Project.Pages
{
    public partial class PersonalAccount : Page
    {
        public PersonalAccount()
        {
            InitializeComponent();
            this.DataContext = App.CurrentUser;
            LoadProfileImage();
        }

        private void LoadProfileImage()
        {
            var profileImageControl = this.profileImage;
            if (App.CurrentUser != null && !string.IsNullOrEmpty(App.CurrentUser.ProfileImagePath) && System.IO.File.Exists(App.CurrentUser.ProfileImagePath))
            {
                profileImageControl.Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(App.CurrentUser.ProfileImagePath));
            }
            else
            {
                profileImageControl.Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("Image/avata.jpg"));
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
            NavigationService?.Navigate(new PersonalAccount());
        }
    }
}