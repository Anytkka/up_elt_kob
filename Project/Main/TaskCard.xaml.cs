using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using Project.Pages;
using System.Windows.Navigation;

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

        private bool _isDragging;
        private Point _startPoint;

        public TaskCard()
        {
            InitializeComponent();
        }

        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            // Переход на Kanban доску подзадач
            var subtaskKanbanPage = new SubtaskKanban(TaskNumber);
            var navigationService = NavigationService.GetNavigationService(this);
            if (navigationService != null)
            {
                navigationService.Navigate(subtaskKanbanPage);
            }
        }

        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void Border_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _isDragging)
                return;

            Point currentPosition = e.GetPosition(null);
            Vector diff = _startPoint - currentPosition;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDragging = true;
                DataObject data = new DataObject("TaskCard", TaskNumber);
                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            }
        }
    }
}
