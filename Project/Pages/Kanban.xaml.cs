using Project.Classes;
using Project.Main;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Linq;
using System;
using MySql.Data.MySqlClient;
using Project.Classes.Common;

namespace Project.Pages
{
    public partial class Kanban : Page
    {
        private int _currentProjectId;
        private List<KanbanColumnContext> _kanbanColumns;
        private List<DocumentContext> _tasks;
        private List<TaskUserContext> _taskUsers;
        private List<UserContext> _users;

        public Kanban(int projectId)
        {
            InitializeComponent();
            _currentProjectId = projectId;
            LoadData();
            InitializeKanbanBoard();
            this.DataContext = App.CurrentUser;
        }

        private void LoadData()
        {
            _kanbanColumns = KanbanColumnContext.Get().Where(k => k.ProjectId == _currentProjectId).ToList();
            _tasks = DocumentContext.Get().Where(t => int.TryParse(t.ProjectCode, out int projectId) && projectId == _currentProjectId).ToList();
            _taskUsers = TaskUserContext.Get();
            _users = UserContext.Get();
            var project = ProjectContext.Get().FirstOrDefault(p => p.Id == _currentProjectId);
            if (project != null)
            {
                var nameLabel = FindName("Name_project") as Label;
                if (nameLabel != null)
                {
                    nameLabel.Content = project.Name;
                }
            }
        }

        private string GetUserRole(int projectId, int userId)
        {
            using (var connection = Connection.OpenConnection())
            {
                string query = "SELECT role FROM project_user WHERE project = @projectId AND user = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@projectId", projectId);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "Не в проекте"; // Если роли нет, пользователь не в проекте
                }
            }
        }

        private void InitializeKanbanBoard()
        {
            var kanbanPanel = FindName("parent") as StackPanel;
            if (kanbanPanel == null) return;

            kanbanPanel.Children.Clear();
            kanbanPanel.Orientation = Orientation.Horizontal;

            string userRole = GetUserRole(_currentProjectId, App.CurrentUser.Id);

            foreach (var column in _kanbanColumns.OrderBy(c => c.Id))
            {
                var columnBorder = new Border
                {
                    Width = 250,
                    Margin = new Thickness(5),
                    Background = System.Windows.Media.Brushes.White,
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(5),
                    AllowDrop = userRole != "Не в проекте", // Разрешаем перетаскивание только для участников проекта
                    Tag = column.Id
                };

                columnBorder.DragEnter += (s, e) =>
                {
                    if (e.Data.GetDataPresent("TaskCard") && userRole != "Не в проекте")
                    {
                        columnBorder.Background = System.Windows.Media.Brushes.LightBlue;
                    }
                };
                columnBorder.DragLeave += (s, e) =>
                {
                    columnBorder.Background = System.Windows.Media.Brushes.White;
                };

                columnBorder.Drop += (s, e) =>
                {
                    if (e.Data.GetDataPresent("TaskCard") && userRole != "Не в проекте")
                    {
                        int taskId = (int)e.Data.GetData("TaskCard");
                        int newColumnId = (int)((Border)s).Tag;

                        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                        if (task != null)
                        {
                            task.Status = newColumnId;
                            task.Update();
                            Console.WriteLine($"Task {taskId} moved to column {newColumnId}");
                        }

                        LoadData();
                        InitializeKanbanBoard();
                    }
                    columnBorder.Background = System.Windows.Media.Brushes.White;
                };

                var columnStack = new StackPanel();

                var headerStack = new StackPanel { Orientation = Orientation.Horizontal };
                var titleLabel = new Label
                {
                    Content = column.TitleStatus,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                headerStack.Children.Add(titleLabel);
                columnStack.Children.Add(headerStack);

                var columnTasks = _tasks.Where(t => t.Status == column.Id).ToList();
                foreach (var task in columnTasks)
                {
                    var responsibleUsers = _taskUsers
                        .Where(tu => tu.TaskId == task.Id)
                        .Join(_users, tu => tu.UserId, u => u.Id, (tu, u) => u)
                        .ToList();
                    var responsibleNames = string.Join(", ", responsibleUsers.Select(u => u.FullName));

                    var taskCard = new TaskCard
                    {
                        TaskNumber = task.Id,
                        TaskName = task.Name,
                        Responsible = responsibleNames,
                        ProjectCode = task.ProjectCode,
                        ProjectName = task.ProjectName,
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    columnStack.Children.Add(taskCard);
                }

                columnBorder.Child = columnStack;
                kanbanPanel.Children.Add(columnBorder);
            }

            if (userRole == "Создатель" || userRole == "Администратор")
            {
                var addTaskButton = new Button
                {
                    Content = "Добавить задачу",
                    Width = 150,
                    Height = 40,
                    Margin = new Thickness(5),
                    Background = System.Windows.Media.Brushes.LightBlue,
                    Foreground = System.Windows.Media.Brushes.Black
                };
                addTaskButton.Click += Bt7_AddTask;
                kanbanPanel.Children.Add(addTaskButton);
            }

            if (userRole == "Создатель")
            {
                var deleteProjectButton = new Button
                {
                    Content = "Удалить проект",
                    Width = 150,
                    Height = 40,
                    Margin = new Thickness(5),
                    Background = System.Windows.Media.Brushes.Red,
                    Foreground = System.Windows.Media.Brushes.White
                };
                deleteProjectButton.Click += DeleteProject_Click;
                kanbanPanel.Children.Add(deleteProjectButton);
            }
        }

        private void Bt7_AddTask(object sender, RoutedEventArgs e)
        {
            var createTaskPage = new CreateTask(_currentProjectId);
            createTaskPage.TaskCreated += (taskId) =>
            {
                var task = DocumentContext.GetById(taskId);
                if (task != null)
                {
                    task.Status = GetNewColumnId();
                    task.Update();
                    LoadData();
                    InitializeKanbanBoard();
                }
            };
            NavigationService?.Navigate(createTaskPage);
        }

        private int GetNewColumnId()
        {
            return _kanbanColumns.FirstOrDefault(c => c.TitleStatus == "Новые")?.Id ?? 1;
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить проект?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using (var connection = Connection.OpenConnection())
                {
                    string deleteQuery = "DELETE FROM project WHERE id = @projectId";
                    using (var cmd = new MySqlCommand(deleteQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", _currentProjectId);
                        cmd.ExecuteNonQuery();
                    }
                }
                NavigationService?.Navigate(new Project1());
            }
        }

        private void Bt7_Projects(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Project1());
        }

        private void PAText_MouseDown(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
    }
}