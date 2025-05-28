using MySql.Data.MySqlClient;
using Project.Classes;
using Project.Classes.Common;
using Project.Main;
using Project.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Project.Pages
{
    public partial class SubtaskKanban : Page
    {
        private int _currentTaskId;
        private List<KanbanColumnContext> _kanbanColumns;
        private List<SubtaskContext> _subtasks;
        private List<UserContext> _users;
        private string _userRole;
        private object navigationService;

        public SubtaskKanban(int taskId)
        {
            InitializeComponent();
            _currentTaskId = taskId;

            try
            {
                LoadData();
                InitializeUserRole();
                InitializeKanbanBoard();
                InitializeAddSubtaskButtonVisibility();
                LoadProfileImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            DataContext = App.CurrentUser;
        }

        private void InitializeUserRole()
        {
            if (App.CurrentUser == null) return;

            var task = TaskContext.GetById(_currentTaskId);
            if (task == null) return;

            int projectId = int.Parse(task.ProjectCode);
            _userRole = GetUserRoleWithLogging(projectId, App.CurrentUser.Id);
            Console.WriteLine($"Текущая роль пользователя: {_userRole}");
        }

        private void LoadProfileImage()
        {
            try
            {
                if (leftProfileImage == null) return;

                if (App.CurrentUser != null && !string.IsNullOrEmpty(App.CurrentUser.ProfileImagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(App.CurrentUser.ProfileImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    leftProfileImage.Source = bitmap;
                }
                else
                {
                    leftProfileImage.Source = new BitmapImage(
                        new Uri("pack://application:,,,/Image/avata.jpg", UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
            }
        }

        public void LoadData()
        {
            try
            {
                var task = TaskContext.GetById(_currentTaskId) ?? throw new Exception("Задача не найдена");
                int projectId = int.Parse(task.ProjectCode);

                _kanbanColumns = KanbanColumnContext.Get()
                    .Where(k => k.ProjectId == projectId)
                    .ToList();

                _subtasks = SubtaskContext.GetByTaskId(_currentTaskId);
                _users = UserContext.Get();

                if (FindName("Name_task") is Label nameLabel)
                {
                    nameLabel.Content = task.Name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
        private string GetUserRole(int projectId, int userId)
        {
            return GetUserRoleWithLogging(projectId, userId);
        }
        private string GetUserRoleWithLogging(int projectId, int userId)
        {
            Console.WriteLine($"Запрос роли для проекта {projectId}, пользователь {userId}");

            using (var connection = Connection.OpenConnection())
            {
                const string query = @"SELECT role FROM project_user 
                                      WHERE project = @projectId 
                                      AND user = @userId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@projectId", projectId);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    var result = cmd.ExecuteScalar()?.ToString() ?? "Не в проекте";
                    Console.WriteLine($"Результат запроса роли: {result}");

                    return result;
                }
            }
        }

        private void InitializeAddSubtaskButtonVisibility()
        {
            if (App.CurrentUser == null) return;

            AddSubtaskButton.Visibility = (_userRole == "Создатель" || _userRole == "Администратор")
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public void InitializeKanbanBoard()
        {
            try
            {
                if (!(FindName("parent") is StackPanel kanbanPanel)) return;

                kanbanPanel.Children.Clear();
                kanbanPanel.Orientation = Orientation.Horizontal;

                var task = TaskContext.GetById(_currentTaskId) ?? throw new Exception("Задача не найдена");
                int projectId = int.Parse(task.ProjectCode);

                bool canEditColumns = _userRole == "Создатель" || _userRole == "Администратор";
                bool canMoveSubtasks = _userRole != "Не в проекте";

                foreach (var column in _kanbanColumns.OrderBy(c => c.Id))
                {
                    var columnBorder = CreateColumnBorder(canMoveSubtasks, column.Id);
                    var columnStack = new StackPanel();

                    columnStack.Children.Add(CreateColumnHeader(column));
                    columnStack.Children.Add(CreateSubtasksPanel(column, task));

                    columnBorder.Child = columnStack;
                    kanbanPanel.Children.Add(columnBorder);
                }

                if (canEditColumns)
                {
                    AddManagementButtons(kanbanPanel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации доски: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateColumnBorder(bool allowDrop, int columnId)
        {
            return new Border
            {
                Width = 250,
                Margin = new Thickness(5),
                Background = Brushes.White,
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(5),
                AllowDrop = allowDrop,
                Tag = columnId
            };
        }

        private StackPanel CreateColumnHeader(KanbanColumnContext column)
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Label
                    {
                        Content = $"{column.TitleStatus} ({_subtasks.Count(t => t.StatusId == column.Id)})",
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Width = 200
                    }
                }
            };
        }

        private UIElement CreateSubtasksPanel(KanbanColumnContext column, TaskContext task)
        {
            var panel = new StackPanel();
            var columnSubtasks = _subtasks.Where(t => t.StatusId == column.Id).ToList();

            if (!columnSubtasks.Any())
            {
                panel.Children.Add(new TextBlock
                {
                    Text = "Нет подзадач",
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0)
                });
                return panel;
            }

            foreach (var subtask in columnSubtasks)
            {
                var responsibleUser = _users.FirstOrDefault(u => u.Id == subtask.UserId);
                var subtaskCard = new SubtaskCard
                {
                    SubtaskNumber = subtask.Id,
                    SubtaskName = subtask.Name,
                    Responsible = responsibleUser?.FullName ?? "Не назначен",
                    TaskCode = subtask.TaskId.ToString(),
                    TaskName = task.Name,
                    UserRole = _userRole,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                subtaskCard.EditButtonClicked += (senderObj, taskId) =>
                {
                    if (_userRole == "Создатель" || _userRole == "Администратор")
                    {
                        var page = new SubtaskEdit(taskId);
                        this.NavigationService?.Navigate(page);
                    }
                };

                // Исправленный обработчик удаления
                subtaskCard.DeleteButtonClicked += (senderObj, taskId) =>
                {
                    if (_userRole == "Создатель" || _userRole == "Администратор")
                    {
                        if (MessageBox.Show("Удалить подзадачу?", "Подтверждение",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            try
                            {
                                var subtaskToDelete = _subtasks.FirstOrDefault(s => s.Id == taskId);
                                if (subtaskToDelete != null)
                                {
                                    subtaskToDelete.Delete();
                                    _subtasks.Remove(subtaskToDelete);
                                    InitializeKanbanBoard();
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка удаления: {ex.Message}");
                            }
                        }
                    }
                };


                panel.Children.Add(subtaskCard);
            }

            return panel;
        }
        private void DeleteTask(int id)
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    

                    string deleteTaskQuery = "DELETE FROM Subtask WHERE id = @id";
                    using (var cmd = new MySqlCommand(deleteTaskQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void HandleSubtaskDrop(DragEventArgs e, int newStatusId)
        {
            if (!(e.Data.GetData("SubtaskCard") is int subtaskId)) return;

            var subtask = _subtasks.FirstOrDefault(t => t.Id == subtaskId);
            if (subtask?.StatusId == newStatusId) return;

            try
            {
                subtask.StatusId = newStatusId;
                subtask.Update();
                RefreshKanbanBoard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перемещения подзадачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshKanbanBoard()
        {
            LoadData();
            InitializeKanbanBoard();
        }

        private void AddManagementButtons(Panel parentPanel)
        {
            var addColumnButton = new Button
            {
                Content = "Добавить столбец",
                Width = 250,
                Margin = new Thickness(5),
                Background = Brushes.LightBlue,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold
            };
            addColumnButton.Click += Bt_AddColumn;
            parentPanel.Children.Add(addColumnButton);
        }

        private Button CreateActionButton(string text, RoutedEventHandler handler)
        {
            return new Button
            {
                Content = text,
                Width = 150,
                Height = 40,
                Margin = new Thickness(5),
                Background = Brushes.LightBlue,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand
            };
        }

        private void DeleteSubtask(int subtaskId)
        {
            if (_userRole != "Создатель" && _userRole != "Администратор") return;

            if (MessageBox.Show("Удалить подзадачу?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            try
            {
                var subtask = _subtasks.FirstOrDefault(s => s.Id == subtaskId);
                subtask?.Delete();
                RefreshKanbanBoard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void PAText_MouseDown(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
       
    }
}
