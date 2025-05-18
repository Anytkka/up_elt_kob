using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project.Pages
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Bt_Login(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
        private void RegisterText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new Registration());
        }
    }
}
