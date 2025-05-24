using Project.Classes;
using Project.Main;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Linq;
using System;
using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System.Windows.Media;
using System.Windows.Input;

namespace Project.Pages
{
    public partial class SubtaskKanban : Page
    {
        private int _currentTaskId;
        private List<KanbanColumnContext> _kanbanColumns;
        private List<SubtaskContext> _subtasks;
        private List<TaskUserContext> _taskUsers;
        private List<UserContext> _users;

        public SubtaskKanban(int taskId)
        {
            InitializeComponent();
            _currentTaskId = taskId;
            LoadData();
            InitializeKanbanBoard();
            this.DataContext = App.CurrentUser;
            InitializeAddSubtaskButtonVisibility(); // Управление видимостью кнопки
        }

        private void LoadData()
        {
            try
            {
                // Получаем ProjectId для текущей задачи
                var task = TaskContext.GetById(_currentTaskId);
                if (task == null)
                {
                    MessageBox.Show("Задача не найдена");
                    return;
                }
                int projectId = int.Parse(task.ProjectCode);

                // Загружаем колонки Kanban для проекта
                _kanbanColumns = KanbanColumnContext.Get()
                                  .Where(k => k.ProjectId == projectId)
                                  .ToList();

                // Загружаем подзадачи для текущей задачи
                _subtasks = SubtaskContext.GetByTaskId(_currentTaskId);

                _taskUsers = TaskUserContext.Get();
                _users = UserContext.Get();

                if (task != null)
                {
                    var nameLabel = FindName("Name_task") as Label;
                    nameLabel.Content = task.Name;

                    Console.WriteLine($"Загружено подзадач: {_subtasks.Count}");
                    foreach (var subtask in _subtasks)
                    {
                        Console.WriteLine($"Подзадача: {subtask.Id} - {subtask.Name} (Статус: {subtask.StatusId})");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
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
                    return result?.ToString() ?? "Не в проекте";
                }
            }
        }

        private void InitializeAddSubtaskButtonVisibility()
        {
            if (App.CurrentUser == null) return;

            var task = TaskContext.GetById(_currentTaskId);
            if (task == null) return;
            int projectId = int.Parse(task.ProjectCode);

            string userRole = GetUserRole(projectId, App.CurrentUser.Id);
            // Показываем кнопку только для "Создатель" и "Администратор"
            AddSubtaskButton.Visibility = (userRole == "Создатель" || userRole == "Администратор")
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void InitializeKanbanBoard()
        {
            try
            {
                var kanbanPanel = FindName("parent") as StackPanel;
                if (kanbanPanel == null)
                {
                    MessageBox.Show("Не найден StackPanel 'parent' для отображения Kanban");
                    return;
                }

                kanbanPanel.Children.Clear();
                kanbanPanel.Orientation = Orientation.Horizontal;

                var task = TaskContext.GetById(_currentTaskId);
                if (task == null) return;
                int projectId = int.Parse(task.ProjectCode);
                string userRole = GetUserRole(projectId, App.CurrentUser.Id);
                bool canEditColumns = userRole == "Создатель" || userRole == "Администратор"; // Для управления столбцами
                bool canMoveSubtasks = userRole != "Не в проекте"; // Для перемещения подзадач

                Console.WriteLine($"Найдено колонок: {_kanbanColumns.Count}");
                foreach (var column in _kanbanColumns.OrderBy(c => c.Id))
                {
                    Console.WriteLine($"Колонка: {column.Id} - {column.TitleStatus}");
                    var columnBorder = new Border
                    {
                        Width = 250,
                        Margin = new Thickness(5),
                        Background = Brushes.White,
                        CornerRadius = new CornerRadius(5),
                        Padding = new Thickness(5),
                        AllowDrop = canMoveSubtasks, // Разрешаем перемещение всем участникам проекта
                        Tag = column.Id
                    };

                    columnBorder.DragEnter += (s, e) =>
                    {
                        if (e.Data.GetDataPresent("SubtaskCard") && canMoveSubtasks)
                        {
                            columnBorder.Background = Brushes.LightBlue;
                            e.Effects = DragDropEffects.Move;
                        }
                        else
                        {
                            e.Effects = DragDropEffects.None;
                        }
                        e.Handled = true;
                    };

                    columnBorder.DragLeave += (s, e) =>
                    {
                        columnBorder.Background = Brushes.White;
                    };

                    columnBorder.Drop += (s, e) =>
                    {
                        columnBorder.Background = Brushes.White;

                        if (e.Data.GetDataPresent("SubtaskCard") && canMoveSubtasks)
                        {
                            int subtaskId = (int)e.Data.GetData("SubtaskCard");
                            int newColumnId = (int)((Border)s).Tag;

                            var subtask = _subtasks.FirstOrDefault(t => t.Id == subtaskId);
                            if (subtask != null && subtask.StatusId != newColumnId)
                            {
                                subtask.StatusId = newColumnId;
                                subtask.Update();
                                Console.WriteLine($"Подзадача {subtaskId} перемещена в колонку {newColumnId}");
                                LoadData();
                                InitializeKanbanBoard();
                            }
                        }
                    };

                    var columnStack = new StackPanel();

                    var headerStack = new StackPanel { Orientation = Orientation.Horizontal };
                    var titleLabel = new Label
                    {
                        Content = $"{column.TitleStatus} ({_subtasks.Count(t => t.StatusId == column.Id)})",
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Width = 200
                    };
                    headerStack.Children.Add(titleLabel);
                    columnStack.Children.Add(headerStack);

                    var columnSubtasks = _subtasks.Where(t => t.StatusId == column.Id).ToList();
                    Console.WriteLine($"Подзадачи в колонке {column.TitleStatus}: {columnSubtasks.Count}");

                    if (columnSubtasks.Count == 0)
                    {
                        var emptyText = new TextBlock
                        {
                            Text = "Нет подзадач",
                            FontStyle = FontStyles.Italic,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 10, 0, 0)
                        };
                        columnStack.Children.Add(emptyText);
                    }
                    else
                    {
                        Console.WriteLine($"Всего пользователей в _users: {_users.Count}");
                        foreach (var user in _users)
                        {
                            Console.WriteLine($"Пользователь в _users: ID = {user.Id}, FullName = {user.FullName}");
                        }

                        foreach (var subtask in columnSubtasks)
                        {
                            Console.WriteLine($"Подзадача {subtask.Id}: UserId = {subtask.UserId}");

                            // Находим ответственного напрямую через UserId из SubtaskContext
                            var responsibleUser = _users.FirstOrDefault(u => u.Id == subtask.UserId);
                            var responsibleName = responsibleUser != null ? responsibleUser.FullName : "Не назначен";
                            Console.WriteLine($"Ответственный для подзадачи {subtask.Id}: {responsibleName}");

                            var subtaskCard = new SubtaskCard
                            {
                                SubtaskNumber = subtask.Id,
                                SubtaskName = subtask.Name,
                                Responsible = responsibleName,
                                TaskCode = subtask.TaskId.ToString(),
                                TaskName = subtask.Name,
                                Margin = new Thickness(0, 5, 0, 0)
                            };

                            columnStack.Children.Add(subtaskCard);
                        }
                    }

                    columnBorder.Child = columnStack;
                    kanbanPanel.Children.Add(columnBorder);
                }

                // Добавляем кнопки в зависимости от роли
                if (canEditColumns)
                {
                    var addSubtaskButton = new Button
                    {
                        Content = "Добавить подзадачу",
                        Width = 150,
                        Height = 40,
                        Margin = new Thickness(5),
                        Background = Brushes.LightBlue,
                        Foreground = Brushes.Black,
                        FontWeight = FontWeights.Bold
                    };
                    addSubtaskButton.Click += Bt7_AddSubtask;
                    kanbanPanel.Children.Add(addSubtaskButton);

                    // Добавляем кнопку "Добавить столбец"
                    var addColumnButton = new Button
                    {
                        Content = "Добавить столбец",
                        Width = 150,
                        Height = 40,
                        Margin = new Thickness(5),
                        Background = Brushes.LightBlue,
                        Foreground = Brushes.Black,
                        FontWeight = FontWeights.Bold
                    };
                    addColumnButton.Click += Bt_AddColumn;
                    kanbanPanel.Children.Add(addColumnButton);
                    Console.WriteLine("Кнопка 'Добавить столбец' добавлена в kanbanPanel на SubtaskKanban");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации Kanban: {ex.Message}");
            }
        }

        private void Bt_AddColumn(object sender, RoutedEventArgs e)
        {
            string columnName = ShowInputDialog("Введите название нового столбца", "Добавить столбец", "Новый столбец");
            if (!string.IsNullOrWhiteSpace(columnName))
            {
                try
                {
                    var task = TaskContext.GetById(_currentTaskId);
                    if (task == null)
                    {
                        MessageBox.Show("Задача не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    int projectId = int.Parse(task.ProjectCode);

                    using (var connection = Connection.OpenConnection())
                    {
                        string query = "INSERT INTO kanbanColumn (title_status, project) VALUES (@title, @projectId); SELECT LAST_INSERT_ID();";
                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@title", columnName);
                            cmd.Parameters.AddWithValue("@projectId", projectId);
                            int newColumnId = Convert.ToInt32(cmd.ExecuteScalar());
                            Console.WriteLine($"Добавлен новый столбец: ID = {newColumnId}, Название = {columnName}");

                            // Обновляем данные и перерисовываем доску
                            LoadData();
                            InitializeKanbanBoard();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении столбца: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string ShowInputDialog(string prompt, string title, string defaultValue)
        {
            Window inputWindow = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(10) };
            panel.Children.Add(new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 5) });
            TextBox inputBox = new TextBox { Text = defaultValue, Margin = new Thickness(0, 0, 0, 5), Width = 200 };
            panel.Children.Add(inputBox);

            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            Button okButton = new Button { Content = "OK", Width = 60, Margin = new Thickness(0, 0, 5, 0) };
            Button cancelButton = new Button { Content = "Отмена", Width = 60 };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            panel.Children.Add(buttonPanel);

            inputWindow.Content = panel;

            bool? result = null;
            okButton.Click += (s, e) => { result = true; inputWindow.Close(); };
            cancelButton.Click += (s, e) => { result = false; inputWindow.Close(); };
            inputWindow.ShowDialog();

            return result == true ? inputBox.Text : null;
        }

        private void Bt7_AddSubtask(object sender, RoutedEventArgs e)
        {
            var currentUser = App.CurrentUser;
            if (currentUser == null) return;

            var task = TaskContext.GetById(_currentTaskId);
            if (task == null) return;
            int projectId = int.Parse(task.ProjectCode);

            string userRole = GetUserRole(projectId, currentUser.Id);

            // Разрешаем создание подзадач только Создателям и Администраторам
            if (userRole == "Создатель" || userRole == "Администратор")
            {
                var addSubtaskPage = new AddSubtask(_currentTaskId, null, projectId);
                addSubtaskPage.CreatedSubtask += OnSubtaskCreated;
                NavigationService?.Navigate(addSubtaskPage);
            }
            else
            {
                MessageBox.Show("Только Создатель и Администратор могут создавать подзадачи.",
                                "Ошибка доступа",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void OnSubtaskCreated(SubtaskContext subtask)
        {
            if (subtask != null)
            {
                LoadData();
                InitializeKanbanBoard();
            }
        }

        private int GetNewColumnId()
        {
            var task = TaskContext.GetById(_currentTaskId);
            if (task == null) return 1;
            int projectId = int.Parse(task.ProjectCode);
            return KanbanColumnContext.Get()
                    .FirstOrDefault(c => c.ProjectId == projectId && c.TitleStatus == "Новые")?.Id ?? 1;
        }

        private void Bt7_Subtasks(object sender, RoutedEventArgs e)
        {
            var task = TaskContext.GetById(_currentTaskId);
            if (task != null)
            {
                int projectId = int.Parse(task.ProjectCode);
                NavigationService?.Navigate(new Kanban(projectId));
            }
            else
            {
                MessageBox.Show("Задача не найдена, возвращение на страницу проектов.");
                NavigationService?.Navigate(new Project1());
            }
        }

        private void PAText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
    }
}