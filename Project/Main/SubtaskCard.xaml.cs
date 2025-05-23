using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Project.Pages;

namespace Project.Main
{
    public partial class SubtaskCard : UserControl
    {
        public static readonly DependencyProperty SubtaskNumberProperty =
            DependencyProperty.Register("SubtaskNumber", typeof(int), typeof(SubtaskCard),
                new PropertyMetadata(0));

        public static readonly DependencyProperty SubtaskNameProperty =
            DependencyProperty.Register("SubtaskName", typeof(string), typeof(SubtaskCard),
                new PropertyMetadata(""));

        public static readonly DependencyProperty ResponsibleProperty =
            DependencyProperty.Register("Responsible", typeof(string), typeof(SubtaskCard),
                new PropertyMetadata(""));

        public static readonly DependencyProperty TaskCodeProperty =
            DependencyProperty.Register("TaskCode", typeof(string), typeof(SubtaskCard),
                new PropertyMetadata(""));

        public static readonly DependencyProperty TaskNameProperty =
            DependencyProperty.Register("TaskName", typeof(string), typeof(SubtaskCard),
                new PropertyMetadata(""));

        public int SubtaskNumber
        {
            get => (int)GetValue(SubtaskNumberProperty);
            set => SetValue(SubtaskNumberProperty, value);
        }

        public string SubtaskName
        {
            get => (string)GetValue(SubtaskNameProperty);
            set => SetValue(SubtaskNameProperty, value);
        }

        public string Responsible
        {
            get => (string)GetValue(ResponsibleProperty);
            set => SetValue(ResponsibleProperty, value);
        }

        public string TaskCode
        {
            get => (string)GetValue(TaskCodeProperty);
            set => SetValue(TaskCodeProperty, value);
        }

        public string TaskName
        {
            get => (string)GetValue(TaskNameProperty);
            set => SetValue(TaskNameProperty, value);
        }

        public SubtaskCard()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void SubtaskButton_Click(object sender, RoutedEventArgs e)
        {
            var subtaskId = SubtaskNumber;
            var subtaskDetailsPage = new SubtaskDetails(subtaskId);

            var navigationService = NavigationService.GetNavigationService(this);
            navigationService?.Navigate(subtaskDetailsPage);
        }

        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, new DataObject("SubtaskCard", SubtaskNumber), DragDropEffects.Move);
            }
        }

        private void Border_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, new DataObject("SubtaskCard", SubtaskNumber), DragDropEffects.Move);
            }
        }
    }
}