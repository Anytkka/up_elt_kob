using System.Windows.Controls;
using System.Windows;

namespace Project.Main
{
    public partial class TaskCard : UserControl
    {
        public static readonly DependencyProperty TaskNumberProperty =
            DependencyProperty.Register("TaskNumber", typeof(int), typeof(TaskCard), new PropertyMetadata(0));

        public static readonly DependencyProperty TaskNameProperty =
            DependencyProperty.Register("TaskName", typeof(string), typeof(TaskCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ResponsibleProperty =
            DependencyProperty.Register("Responsible", typeof(string), typeof(TaskCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ProjectCodeProperty =
            DependencyProperty.Register("ProjectCode", typeof(string), typeof(TaskCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ProjectNameProperty =
            DependencyProperty.Register("ProjectName", typeof(string), typeof(TaskCard), new PropertyMetadata(string.Empty));

        public int TaskNumber
        {
            get { return (int)GetValue(TaskNumberProperty); }
            set { SetValue(TaskNumberProperty, value); }
        }

        public string TaskName
        {
            get { return (string)GetValue(TaskNameProperty); }
            set { SetValue(TaskNameProperty, value); }
        }

        public string Responsible
        {
            get { return (string)GetValue(ResponsibleProperty); }
            set { SetValue(ResponsibleProperty, value); }
        }

        public string ProjectCode
        {
            get { return (string)GetValue(ProjectCodeProperty); }
            set { SetValue(ProjectCodeProperty, value); }
        }

        public string ProjectName
        {
            get { return (string)GetValue(ProjectNameProperty); }
            set { SetValue(ProjectNameProperty, value); }
        }

        public TaskCard()
        {
            InitializeComponent();
        }

        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}