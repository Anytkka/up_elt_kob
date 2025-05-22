using Project.Classes;
using Project.Main;
using Project.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
        }

        private void LoadData()
        {
            // Загрузка данных из БД
            _kanbanColumns = KanbanColumnContext.Get().Where(k => k.ProjectId == _currentProjectId).ToList();
            _tasks = DocumentContext.Get().Where(t => t.ProjectId == _currentProjectId).ToList();
            _taskUsers = TaskUserContext.Get();
            _users = UserContext.Get();

            // Установка названия проекта
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

            foreach (var column in _kanbanColumns.OrderBy(c => c.Id))
            {
                // Создание столбца Kanban
                var columnBorder = new Border
                {
                    Width = 250,
                    Margin = new Thickness(5),
                    Background = System.Windows.Media.Brushes.White,
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(5)
                };

                var columnStack = new StackPanel();

                // Заголовок столбца с кнопкой редактирования
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

                // Добавление задач в столбец
                var columnTasks = _tasks.Where(t => t.Status == column.Id).ToList();
                foreach (var task in columnTasks)
                {
                    // Получение ответственных за задачу
                    var responsibleUsers = _taskUsers
                        .Where(tu => tu.TaskId == task.Id)
                        .Join(_users, tu => tu.UserId, u => u.Id, (tu, u) => u)
                        .ToList();

                    var responsibleNames = string.Join(", ", responsibleUsers.Select(u => u.FullName));

                    // Создание карточки задачи
                    var taskCard = new TaskCard
                    {
                        TaskNumber = task.Id,
                        TaskName = task.Name,
                        Responsible = responsibleNames,
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    columnStack.Children.Add(taskCard);
                }

                // Кнопка добавления задачи
                var addTaskButton = new Button
                {
                    Content = "+ Добавить задачу",
                    Margin = new Thickness(0, 10, 0, 0),
                    Tag = column.Id,
                    Style = (Style)FindResource("AddButtonStyle")
                };
                addTaskButton.Click += (s, e) => AddTaskToColumn(column.Id);

                columnStack.Children.Add(addTaskButton);
                columnBorder.Child = columnStack;
                kanbanPanel.Children.Add(columnBorder);
            }

            // Кнопка добавления нового столбца
            var addColumnButton = new Button
            {
                Content = "+ Добавить столбец",
                Width = 250,
                Margin = new Thickness(5),
                Style = (Style)FindResource("AddButtonStyle")
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
                    _kanbanColumns.Max(c => c.Id) + 1,
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
                // Обновляем статус задачи после создания
                var task = DocumentContext.GetById(taskId);
                if (task != null)
                {
                    task.Status = columnId;
                    task.Update();
                    LoadData();
                    InitializeKanbanBoard();
                }
            };

            NavigationService?.Navigate(createTaskPage);
        }

        private void Bt7_Projects(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Project1());
        }

        private void Bt7_AddTask(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CreateTask());
        }
    }
}