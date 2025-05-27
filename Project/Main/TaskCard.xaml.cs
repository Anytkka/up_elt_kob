using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using MySql.Data.MySqlClient;
using Project.Classes;
using Project.Classes.Common;
using Project.Pages;

namespace Project.Main
{
    public partial class TaskCard : UserControl
    {
        public event EventHandler<int> TaskButtonClicked;
        public event EventHandler<int> DetailsButtonClicked;
        public event EventHandler<int> EditButtonClicked;
        public event EventHandler<int> DeleteButtonClicked;

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

        public static readonly DependencyProperty UserRoleProperty =
            DependencyProperty.Register("UserRole", typeof(string), typeof(TaskCard), new PropertyMetadata(string.Empty));

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

        public string UserRole
        {
            get { return (string)GetValue(UserRoleProperty); }
            set { SetValue(UserRoleProperty, value); }
        }

        private bool _isDragging;
        private Point _startPoint;

        public TaskCard()
        {
            InitializeComponent();
            Console.WriteLine($"UserRole: {UserRole}");
            InitializeButtonVisibility();
        }

        private void InitializeButtonVisibility()
        {
            Console.WriteLine($"UserRole: {UserRole}");
            if (UserRole == "Создатель" || UserRole == "Администратор")
            {
                Console.WriteLine("Setting EditButton and DeleteButton visibility to Visible");
                EditButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }
            else
            {
                Console.WriteLine("Setting EditButton and DeleteButton visibility to Collapsed");
                EditButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
            }
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

        private void DetailsTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var taskDetailsPage = new TaskDetailsPage(TaskNumber);
            var navigationService = NavigationService.GetNavigationService(this);
            if (navigationService != null)
            {
                navigationService.Navigate(taskDetailsPage);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new TaskEditPage(TaskNumber);
            var navigationService = NavigationService.GetNavigationService(this);
            if (navigationService != null)
            {
                navigationService.Navigate(editPage);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить эту задачу?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var task = TaskContext.GetById(TaskNumber);
                    if (task != null)
                    {
                        // Удаляем все связи с пользователями
                        var taskUsers = TaskUserContext.Get()
                            .Where(tu => tu.TaskId == TaskNumber)
                            .ToList();

                        foreach (var taskUser in taskUsers)
                        {
                            taskUser.Delete();
                        }

                        // Удаляем саму задачу
                        task.Delete();

                        DeleteButtonClicked?.Invoke(this, TaskNumber);
                        MessageBox.Show("Задача успешно удалена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
