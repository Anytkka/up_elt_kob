using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project.Pages
{
    public partial class PersonalAccount : Page
    {
        public PersonalAccount()
        {
            InitializeComponent();
        }

        private void Bt_Projects(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Project1());
        }

        private void Bt_MyTasks(object sender, RoutedEventArgs e)
        {

        }

        private void Bt_EditProfile(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new editProfil());
        }

        private void Bt_Exit(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Login());

        }

        private void PAText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
    }
}