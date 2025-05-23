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
        }

        private void LoadData()
        {
            try
            {
                // Загружаем колонки Kanban для текущей задачи
                _kanbanColumns = KanbanColumnContext.Get()
                                  .Where(k => k.ProjectId == _currentTaskId)
                                  .ToList();

                // Загружаем все подзадачи для текущей задачи
                _subtasks = SubtaskContext.Get()
                            .Where(t => t.TaskId == _currentTaskId)
                            .ToList();

                _taskUsers = TaskUserContext.Get();
                _users = UserContext.Get();

                // Загружаем информацию о задаче
                var task = DocumentContext.GetById(_currentTaskId);
                if (task != null)
                {
                    var nameLabel = FindName("Name_task") as Label;
                    nameLabel.Content = task.Name;

                    // Добавим вывод в консоль для отладки
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

                // Очищаем панель перед повторной инициализацией
                kanbanPanel.Children.Clear();
                kanbanPanel.Orientation = Orientation.Horizontal;

                // Получаем роль текущего пользователя
                string userRole = GetUserRole(_currentTaskId, App.CurrentUser.Id);
                bool canEdit = userRole == "Создатель" || userRole == "Администратор";

                // Сортируем колонки по ID и создаем их
                foreach (var column in _kanbanColumns.OrderBy(c => c.Id))
                {
                    // Создаем контейнер для колонки
                    var columnBorder = new Border
                    {
                        Width = 250,
                        Margin = new Thickness(5),
                        Background = Brushes.White,
                        CornerRadius = new CornerRadius(5),
                        Padding = new Thickness(5),
                        AllowDrop = canEdit, // Разрешаем перетаскивание только для редакторов
                        Tag = column.Id // Сохраняем ID колонки в Tag
                    };

                    // Обработчики событий drag-and-drop
                    columnBorder.DragEnter += (s, e) =>
                    {
                        if (e.Data.GetDataPresent("SubtaskCard") && canEdit)
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

                        if (e.Data.GetDataPresent("SubtaskCard") && canEdit)
                        {
                            int subtaskId = (int)e.Data.GetData("SubtaskCard");
                            int newColumnId = (int)((Border)s).Tag;

                            var subtask = _subtasks.FirstOrDefault(t => t.Id == subtaskId);
                            if (subtask != null && subtask.StatusId != newColumnId) // Исправлено на StatusId
                            {
                                subtask.StatusId = newColumnId;
                                subtask.Update();

                                // Обновляем UI
                                LoadData();
                                InitializeKanbanBoard();
                            }
                        }
                    };

                    // Создаем содержимое колонки
                    var columnStack = new StackPanel();

                    // Заголовок колонки
                    var headerStack = new StackPanel { Orientation = Orientation.Horizontal };
                    var titleLabel = new Label
                    {
                        Content = $"{column.TitleStatus} ({_subtasks.Count(t => t.StatusId == column.Id)})", // Исправлено на StatusId
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Width = 200
                    };
                    headerStack.Children.Add(titleLabel);
                    columnStack.Children.Add(headerStack);

                    // Получаем подзадачи для текущей колонки
                    var columnSubtasks = _subtasks.Where(t => t.StatusId == column.Id).ToList(); // Исправлено на StatusId

                    if (columnSubtasks.Count == 0)
                    {
                        // Если подзадач нет, показываем заглушку
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
                        // Добавляем все подзадачи колонки
                        foreach (var subtask in columnSubtasks)
                        {
                            var responsibleUsers = _taskUsers
                                .Where(tu => tu.TaskId == subtask.Id)
                                .Join(_users, tu => tu.UserId, u => u.Id, (tu, u) => u)
                                .ToList();

                            var responsibleNames = responsibleUsers.Any()
                                ? string.Join(", ", responsibleUsers.Select(u => u.FullName))
                                : "Не назначены";

                            var subtaskCard = new SubtaskCard
                            {
                                SubtaskNumber = subtask.Id,
                                SubtaskName = subtask.Name,
                                Responsible = responsibleNames,
                                TaskCode = subtask.TaskId.ToString(),
                                TaskName = subtask.Name, // Исправлено: теперь отображается название подзадачи
                                Margin = new Thickness(0, 5, 0, 0)
                            };

                            columnStack.Children.Add(subtaskCard);
                        }
                    }

                    columnBorder.Child = columnStack;
                    kanbanPanel.Children.Add(columnBorder);
                }

                // Кнопка добавления новой подзадачи (только для редакторов)
                if (canEdit)
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации Kanban: {ex.Message}");
            }
        }

        private void Bt7_AddSubtask(object sender, RoutedEventArgs e)
        {
            var addSubtaskPage = new AddSubtask(_currentTaskId);
            addSubtaskPage.CreatedSubtask += OnSubtaskCreated;
            NavigationService?.Navigate(addSubtaskPage);
        }

        private void OnSubtaskCreated(SubtaskContext subtask)
        {
            if (subtask != null)
            {
                subtask.StatusId = GetNewColumnId(); // Исправлено на StatusId
                subtask.Update();
                LoadData();
                InitializeKanbanBoard();
            }
        }

        private int GetNewColumnId()
        {
            return _kanbanColumns.FirstOrDefault(c => c.TitleStatus == "Новые")?.Id ?? 1;
        }

        private void Bt7_Subtasks(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Kanban(_currentTaskId));
        }

        private void PAText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
    }
}