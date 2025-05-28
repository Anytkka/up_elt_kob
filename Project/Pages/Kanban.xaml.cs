using Project.Classes;
using Project.Main;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Linq;
using System;
using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System.Windows.Input;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Project.Pages
{
    public partial class Kanban : Page
    {
        private int _currentProjectId;
        private List<KanbanColumnContext> _kanbanColumns;
        private List<TaskContext> _tasks;
        private List<TaskUserContext> _taskUsers;
        private List<UserContext> _users;
        private string _userRole;

        public Kanban(int projectId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Инициализация Kanban для projectId {projectId}");
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("InitializeComponent завершен");
                _currentProjectId = projectId;
                LoadData();
                System.Diagnostics.Debug.WriteLine("LoadData завершен");
                InitializeKanbanBoard();
                System.Diagnostics.Debug.WriteLine("InitializeKanbanBoard завершен");
                this.DataContext = App.CurrentUser;
                InitializeAddTaskButtonVisibility();
                System.Diagnostics.Debug.WriteLine("InitializeAddTaskButtonVisibility завершен");
                LoadProfileImage();
                System.Diagnostics.Debug.WriteLine("LoadProfileImage завершен");
                System.Diagnostics.Debug.WriteLine("Инициализация Kanban завершена успешно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при инициализации Kanban: {ex.Message}\nStackTrace: {ex.StackTrace}");
                MessageBox.Show($"Ошибка при загрузке страницы Kanban: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService?.Navigate(new Project1());
            }
        }

        private void LoadProfileImage()
        {
            var leftProfileImageControl = this.leftProfileImage;
            if (leftProfileImageControl == null)
            {
                System.Diagnostics.Debug.WriteLine("Элемент leftProfileImage не найден в XAML");
                MessageBox.Show("Элемент изображения 'leftProfileImage' не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"App.CurrentUser: {(App.CurrentUser != null ? "Не null" : "Null")}");
                if (App.CurrentUser != null && !string.IsNullOrEmpty(App.CurrentUser.ProfileImagePath?.Trim()))
                {
                    System.Diagnostics.Debug.WriteLine($"Проверка существования файла: {App.CurrentUser.ProfileImagePath}");
                    if (File.Exists(App.CurrentUser.ProfileImagePath))
                    {
                        System.Diagnostics.Debug.WriteLine("Файл существует, загружаем...");
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(App.CurrentUser.ProfileImagePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        leftProfileImageControl.Source = bitmap;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Файл не найден по пути: {App.CurrentUser.ProfileImagePath}");
                        leftProfileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ProfileImagePath пустой или null, загружаем изображение по умолчанию.");
                    leftProfileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                leftProfileImageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
            }
        }

        private void LoadData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Загрузка данных для Kanban...");
                _kanbanColumns = KanbanColumnContext.Get().Where(k => k.ProjectId == _currentProjectId).ToList();
                System.Diagnostics.Debug.WriteLine($"Загружено столбцов: {_kanbanColumns.Count}");

                var allTasks = TaskContext.Get();
                _tasks = allTasks.Where(t =>
                {
                    if (string.IsNullOrWhiteSpace(t.ProjectCode))
                    {
                        System.Diagnostics.Debug.WriteLine($"Задача с ID {t.Id} имеет пустой ProjectCode");
                        return false;
                    }
                    return int.TryParse(t.ProjectCode, out int projectId) && projectId == _currentProjectId;
                }).ToList();
                System.Diagnostics.Debug.WriteLine($"Загружено задач: {_tasks.Count}");

                _taskUsers = TaskUserContext.Get();
                System.Diagnostics.Debug.WriteLine($"Загружено связей task_user: {_taskUsers.Count}");

                _users = UserContext.Get();
                System.Diagnostics.Debug.WriteLine($"Загружено пользователей: {_users.Count}");

                var project = ProjectContext.Get().FirstOrDefault(p => p.Id == _currentProjectId);
                if (project != null)
                {
                    var nameLabel = FindName("Name_project") as Label;
                    if (nameLabel != null)
                    {
                        nameLabel.Content = project.Name;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Элемент Name_project не найден в XAML");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Проект с ID {_currentProjectId} не найден");
                }

                _userRole = GetUserRole(_currentProjectId, App.CurrentUser?.Id ?? 0);
                System.Diagnostics.Debug.WriteLine($"Роль пользователя: {_userRole}");

                foreach (var task in _tasks)
                {
                    System.Diagnostics.Debug.WriteLine($"Задача: {task.Id} - {task.Name} (Статус: {task.Status})");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке данных: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw new Exception($"Не удалось загрузить данные: {ex.Message}", ex);
            }
        }

        private string GetUserRole(int projectId, int userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Получение роли для projectId {projectId}, userId {userId}");
                using (var connection = Connection.OpenConnection())
                {
                    if (connection == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Не удалось открыть соединение с базой данных");
                        return "Не в проекте";
                    }

                    string query = "SELECT role FROM project_user WHERE project = @projectId AND user = @userId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", projectId);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        var result = cmd.ExecuteScalar();
                        var role = result?.ToString() ?? "Не в проекте";
                        System.Diagnostics.Debug.WriteLine($"Получена роль: {role}");
                        return role;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при получении роли: {ex.Message}");
                return "Не в проекте";
            }
        }

        private void InitializeAddTaskButtonVisibility()
        {
            if (App.CurrentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("App.CurrentUser is null");
                return;
            }

            AddTaskButton.Visibility = (_userRole == "Создатель" || _userRole == "Администратор")
                ? Visibility.Visible
                : Visibility.Collapsed;
            System.Diagnostics.Debug.WriteLine($"AddTaskButton Visibility: {AddTaskButton.Visibility}");
        }

        private void InitializeKanbanBoard()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Инициализация Kanban Board...");
                var kanbanPanel = FindName("parent") as StackPanel;
                if (kanbanPanel == null)
                {
                    System.Diagnostics.Debug.WriteLine("Элемент parent не найден в XAML");
                    return;
                }

                kanbanPanel.Children.Clear();
                kanbanPanel.Orientation = Orientation.Horizontal;

                LoadData();

                foreach (var column in _kanbanColumns.OrderBy(c => c.Id))
                {
                    System.Diagnostics.Debug.WriteLine($"Создание столбца: {column.TitleStatus} (ID: {column.Id})");
                    var columnBorder = new Border
                    {
                        Width = 250,
                        Margin = new Thickness(5),
                        Background = System.Windows.Media.Brushes.White,
                        CornerRadius = new CornerRadius(5),
                        Padding = new Thickness(5),
                        AllowDrop = _userRole != "Не в проекте",
                        Tag = column.Id
                    };

                    columnBorder.DragEnter += (s, e) =>
                    {
                        if (e.Data.GetDataPresent("TaskCard") && _userRole != "Не в проекте")
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
                        if (e.Data.GetDataPresent("TaskCard") && _userRole != "Не в проекте")
                        {
                            int taskId = (int)e.Data.GetData("TaskCard");
                            int newColumnId = (int)((Border)s).Tag;

                            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                            if (task != null)
                            {
                                task.Status = newColumnId;
                                task.Update();
                                System.Diagnostics.Debug.WriteLine($"Task {taskId} moved to column {newColumnId}");
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
                    System.Diagnostics.Debug.WriteLine($"Задач в столбце {column.TitleStatus}: {columnTasks.Count}");
                    foreach (var task in columnTasks)
                    {
                        System.Diagnostics.Debug.WriteLine($"Создание TaskCard для задачи {task.Id}: {task.Name}");
                        var responsibleUsers = _taskUsers
                            .Where(tu => tu.TaskId == task.Id)
                            .Join(_users, tu => tu.UserId, u => u.Id, (tu, u) => u)
                            .ToList();
                        var responsibleNames = string.Join(", ", responsibleUsers.Select(u => u.FullName));

                        TaskCard taskCard = null;
                        try
                        {
                            taskCard = new TaskCard
                            {
                                TaskNumber = task.Id,
                                TaskName = task.Name,
                                Responsible = responsibleNames,
                                ProjectCode = task.ProjectCode,
                                ProjectName = task.ProjectName,
                                UserRole = _userRole,
                                Margin = new Thickness(0, 5, 0, 0)
                            };
                            System.Diagnostics.Debug.WriteLine($"TaskCard для задачи {task.Id} создан успешно");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Ошибка при создании TaskCard для задачи {task.Id}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                            throw;
                        }

                        taskCard.TaskButtonClicked += (sender, taskId) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Переход к подзадачам для задачи {taskId}");
                            NavigationService?.Navigate(new SubtaskKanban(taskId));
                        };

                        taskCard.DetailsButtonClicked += (sender, taskId) =>
                        {
                            NavigationService?.Navigate(new TaskDetails(taskId));
                        };

                        taskCard.EditButtonClicked += (sender, taskId) =>
                        {
                            if (_userRole == "Создатель" || _userRole == "Администратор")
                            {
                                NavigationService?.Navigate(new TaskEdit(taskId));
                            }
                        };

                        taskCard.DeleteButtonClicked += (sender, taskId) =>
                        {
                            if (_userRole == "Создатель")
                            {
                                if (MessageBox.Show("Вы уверены, что хотите удалить задачу?", "Подтверждение",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                {
                                    DeleteTask(taskId);
                                    LoadData();
                                    InitializeKanbanBoard();
                                }
                            }
                        };

                        columnStack.Children.Add(taskCard);
                    }

                    columnBorder.Child = columnStack;
                    kanbanPanel.Children.Add(columnBorder);
                }

                if (_userRole == "Создатель" || _userRole == "Администратор")
                {
                    var addColumnButton = new Button
                    {
                        Content = "Добавить столбец",
                        Width = 250,
                        Margin = new Thickness(5),
                        Background = System.Windows.Media.Brushes.DarkGray,
                        Foreground = System.Windows.Media.Brushes.Black,
                        FontWeight = FontWeights.Bold
                    };
                    addColumnButton.Click += Bt_AddColumn;
                    kanbanPanel.Children.Add(addColumnButton);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при инициализации Kanban Board: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw new Exception($"Не удалось инициализировать доску Kanban: {ex.Message}", ex);
            }
        }

        private void DeleteTask(int taskId)
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    string deleteTaskUserQuery = "DELETE FROM task_user WHERE task = @taskId";
                    using (var cmd = new MySqlCommand(deleteTaskUserQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@taskId", taskId);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteSubtasksQuery = "DELETE FROM subtask WHERE task = @taskId";
                    using (var cmd = new MySqlCommand(deleteSubtasksQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@taskId", taskId);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteTaskQuery = "DELETE FROM task WHERE id = @taskId";
                    using (var cmd = new MySqlCommand(deleteTaskQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@taskId", taskId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Bt_AddColumn(object sender, RoutedEventArgs e)
        {
            string columnName = ShowInputDialog("Введите название нового столбца", "Добавить столбец", "Новый столбец");
            if (!string.IsNullOrWhiteSpace(columnName))
            {
                try
                {
                    using (var connection = Connection.OpenConnection())
                    {
                        string query = "INSERT INTO kanbanColumn (title_status, project) VALUES (@title, @projectId); SELECT LAST_INSERT_ID();";
                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@title", columnName);
                            cmd.Parameters.AddWithValue("@projectId", _currentProjectId);
                            int newColumnId = Convert.ToInt32(cmd.ExecuteScalar());
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

        private int GetNewColumnId()
        {
            return _kanbanColumns.FirstOrDefault(c => c.TitleStatus == "Новые")?.Id ?? 1;
        }

        private void Bt7_AddTask(object sender, RoutedEventArgs e)
        {
            var currentUser = App.CurrentUser;
            if (currentUser == null) return;

            if (_userRole == "Создатель" || _userRole == "Администратор")
            {
                var createTaskPage = new CreateTask(_currentProjectId);
                createTaskPage.TaskCreated += (taskId) =>
                {
                    var task = TaskContext.GetById(taskId);
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
            else
            {
                MessageBox.Show("Только Создатель и Администратор могут создавать задачи.",
                                "Ошибка доступа",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить проект? Все связанные данные (задачи, подзадачи, столбцы) будут удалены.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            MySqlConnection connection = null;
            MySqlTransaction transaction = null;

            try
            {
                connection = Connection.OpenConnection();
                if (connection == null)
                {
                    MessageBox.Show("Не удалось подключиться к базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                transaction = connection.BeginTransaction();

                List<int> columnIds = new List<int>();
                string getColumnsQuery = "SELECT id FROM kanbanColumn WHERE project = @projectId";
                using (var cmd = new MySqlCommand(getColumnsQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@projectId", _currentProjectId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columnIds.Add(reader.GetInt32("id"));
                        }
                    }
                }

                List<int> taskIds = new List<int>();
                if (columnIds.Count > 0)
                {
                    string getTasksQuery = "SELECT id FROM task WHERE status IN (" + string.Join(",", columnIds) + ")";
                    using (var cmd = new MySqlCommand(getTasksQuery, connection, transaction))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                taskIds.Add(reader.GetInt32("id"));
                            }
                        }
                    }
                }

                if (taskIds.Count > 0)
                {
                    string deleteSubtasksQuery = "DELETE FROM subtask WHERE task IN (" + string.Join(",", taskIds) + ")";
                    using (var cmd = new MySqlCommand(deleteSubtasksQuery, connection, transaction))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                if (taskIds.Count > 0)
                {
                    string deleteTaskUserQuery = "DELETE FROM task_user WHERE task IN (" + string.Join(",", taskIds) + ")";
                    using (var cmd = new MySqlCommand(deleteTaskUserQuery, connection, transaction))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                if (taskIds.Count > 0)
                {
                    string deleteTasksQuery = "DELETE FROM task WHERE status IN (" + string.Join(",", columnIds) + ")";
                    using (var cmd = new MySqlCommand(deleteTasksQuery, connection, transaction))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                string deleteColumnsQuery = "DELETE FROM kanbanColumn WHERE project = @projectId";
                using (var cmd = new MySqlCommand(deleteColumnsQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@projectId", _currentProjectId);
                    cmd.ExecuteNonQuery();
                }

                string deleteProjectUserQuery = "DELETE FROM project_user WHERE project = @projectId";
                using (var cmd = new MySqlCommand(deleteProjectUserQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@projectId", _currentProjectId);
                    cmd.ExecuteNonQuery();
                }

                string deleteProjectQuery = "DELETE FROM project WHERE id = @projectId";
                using (var cmd = new MySqlCommand(deleteProjectQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@projectId", _currentProjectId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("Проект не был удалён. Возможно, он уже был удалён или не существует.");
                    }
                }

                transaction.Commit();
                MessageBox.Show("Проект успешно удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.Navigate(new Project1());
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                MessageBox.Show($"Ошибка при удалении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection != null)
                {
                    Connection.CloseConnection(connection);
                }
            }
        }

        private void Bt7_Projects(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Project1());
        }

        private void PAText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
    }
}