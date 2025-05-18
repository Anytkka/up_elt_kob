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
    /// Логика взаимодействия для Project1.xaml
    /// </summary>
    public partial class Project1 : Page
    {
        public Project1()
        {
            InitializeComponent();
        }

        private void Bt1_Projects(object sender, RoutedEventArgs e)
        {

        }

        private void Bt1_MyTasks(object sender, RoutedEventArgs e)
        {

        }

        private void Bt_CreateProject(object sender, RoutedEventArgs e)
        {

        }
        private void PAText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
    }
}
