using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using Project.Pages;
using System.Windows.Navigation;
using Project.Classes;
using System.Linq;
using MySql.Data.MySqlClient;
using Project.Classes.Common;

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
        private string _currentUserRole;

        public TaskCard()
        {
            InitializeComponent();
            LoadUserRole();
            InitializeButtonVisibility();
        }

        private void LoadUserRole()
        {
            if (App.CurrentUser == null) return;

            using (var connection = Connection.OpenConnection())
            {
                string query = @"SELECT role FROM project_user 
                           WHERE project = @projectId AND user = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@projectId", int.Parse(ProjectCode));
                    cmd.Parameters.AddWithValue("@userId", App.CurrentUser.Id);
                    _currentUserRole = cmd.ExecuteScalar()?.ToString() ?? "Не в проекте";
                }
            }
        }
        private void InitializeButtonVisibility()
        {
            if (_currentUserRole == "Создатель" || _currentUserRole == "Администратор") return;

            EditButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
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
