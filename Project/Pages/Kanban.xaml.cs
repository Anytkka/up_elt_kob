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
    /// Логика взаимодействия для Kanban.xaml
    /// </summary>
    public partial class Kanban : Page
    {
        private string projectInf;
        public Kanban(string projectInf)
        {
            InitializeComponent();
            this.projectInf = projectInf;
        }

        private void Bt7_Projects(object sender, RoutedEventArgs e)
        {

        }

        private void Bt7_MyTasks(object sender, RoutedEventArgs e)
        {

        }

        private void Bt7_AddTask(object sender, RoutedEventArgs e)
        {

        }
    }
}
