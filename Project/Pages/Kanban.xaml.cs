using Project.Classes;
using Project.Main;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Linq;
using System;

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
            Console.WriteLine($"Filtered columns for project {_currentProjectId}: {_kanbanColumns.Count}");
            _tasks = DocumentContext.Get().Where(t => int.TryParse(t.ProjectCode, out int projectId) && projectId == _currentProjectId).ToList();
            Console.WriteLine($"Loaded tasks count: {_tasks.Count}");
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

        private void InitializeKanbanBoard()
        {
            var kanbanPanel = FindName("parent") as StackPanel;
            if (kanbanPanel == null) return;

            kanbanPanel.Children.Clear();
            kanbanPanel.Orientation = Orientation.Horizontal;

            Console.WriteLine($"Columns count: {_kanbanColumns.Count}, Tasks count: {_tasks.Count}");

            foreach (var column in _kanbanColumns.OrderBy(c => c.Id))
            {
                var columnBorder = new Border
                {
                    Width = 250,
                    Margin = new Thickness(5),
                    Background = System.Windows.Media.Brushes.White,
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(5),
                    AllowDrop = true,
                    Tag = column.Id 
                };

                columnBorder.DragEnter += (s, e) =>
                {
                    if (e.Data.GetDataPresent("TaskCard"))
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
                    if (e.Data.GetDataPresent("TaskCard"))
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

                var editButton = new Button
                {
                    Content = "✏️",
                    Margin = new Thickness(5, 0, 0, 0),
                    Tag = column.Id,
                    ToolTip = "Редактировать столбец",
                    Style = (Style)FindResource("TransparentButtonStyle")
                };
                editButton.Click += EditColumn_Click;

                headerStack.Children.Add(titleLabel);
                headerStack.Children.Add(editButton);
                columnStack.Children.Add(headerStack);

                var columnTasks = _tasks.Where(t => t.Status == column.Id).ToList();
                Console.WriteLine($"Column '{column.TitleStatus}' (ID: {column.Id}) has {columnTasks.Count} tasks");

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

            var addColumnButton = new Button
            {
                Content = "+ Добавить столбец",
                Width = 350,
                Height = 40,
                Margin = new Thickness(5),
                Background = System.Windows.Media.Brushes.LightBlue,
                Foreground = System.Windows.Media.Brushes.Black
            };
            addColumnButton.Click += AddColumn_Click;
            kanbanPanel.Children.Add(addColumnButton);

        }

        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            var inputDialog = new InputDialog("Добавить столбец", "Введите название столбца:");
            if (inputDialog.ShowDialog() == true)
            {
                var newColumn = new KanbanColumnContext(
                    0,
                    inputDialog.Answer,
                    _currentProjectId
                );
                newColumn.Add();
                LoadData();
                InitializeKanbanBoard();
            }
        }

        private void EditColumn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int columnId)
            {
                var column = _kanbanColumns.FirstOrDefault(c => c.Id == columnId);
                if (column != null)
                {
                    var inputDialog = new InputDialog("Редактировать столбец", "Введите новое название:", column.TitleStatus);
                    if (inputDialog.ShowDialog() == true)
                    {
                        column.TitleStatus = inputDialog.Answer;
                        column.Update();
                        LoadData();
                        InitializeKanbanBoard();
                    }
                }
            }
        }

        private void AddTaskToColumn(int columnId)
        {
            var createTaskPage = new CreateTask();
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
        private void Bt7_Projects(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Project1());
        }

        private void Bt7_AddTask(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CreateTask());
        }
        private void PAText_MouseDown(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
    }
}