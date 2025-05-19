using Project.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project.Pages
{
    public partial class PersonalAccount : Page
    {
        private User _currentUser;

        public PersonalAccount(User user)
        {
            InitializeComponent();
            _currentUser = user;
            DataContext = _currentUser;

            InitializeUserData();
        }

        private void InitializeUserData()
        {
            if (_currentUser != null)
            {
            }
        }

        private void Bt_Projects(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Project1(_currentUser));
        }

        private void Bt_EditProfile(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new editProfil(_currentUser));
        }

        private void Bt_Exit(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            NavigationService?.Navigate(new Login());
        }

        private void PAText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount(_currentUser));
        }
    }
}