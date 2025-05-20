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
        }

        private void Bt_Projects(object sender, RoutedEventArgs e)
        {
            
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
        }
    }
}