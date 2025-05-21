using Project.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Project.Pages
{
    public partial class AddSubtask : Page
    {
        private int _taskId;
        private SubtaskContext _editingSubtask;
        private List<UserContext> _responsiblePersons;

        public AddSubtask(int taskId, SubtaskContext subtask = null)
        {
            InitializeComponent();
            _taskId = taskId;
            _editingSubtask = subtask;

            LoadResponsiblePersons();
            InitializeFields();
        }

        private void LoadResponsiblePersons()
        {
            try
            {
                _responsiblePersons = UserContext.Get();
                кesponsible.ItemsSource = _responsiblePersons;

                if (_responsiblePersons.Count > 0)
                    кesponsible.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке ответственных: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void InitializeFields()
        {
            if (_editingSubtask != null)
            {
                name.Text = _editingSubtask.Name;
                description.Text = _editingSubtask.Description;

                var responsible = _responsiblePersons.FirstOrDefault(u => u.Id == _editingSubtask.UserId);
                if (responsible != null)
                    кesponsible.SelectedItem = responsible;
            }
            else
            {
                description.Text = "Опишите подробности";
                description.Foreground = Brushes.Gray;
            }
        }

        private void Bt_Cancel(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Bt_AddS(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(name.Text))
            {
                MessageBox.Show("Введите наименование подзадачи");
                return;
            }

            try
            {
                var selectedUser = кesponsible.SelectedItem as UserContext;
                if (selectedUser == null)
                {
                    MessageBox.Show("Выберите ответственного");
                    return;
                }

                if (_editingSubtask == null)
                {
                    var newSubtask = new SubtaskContext(
                        0,
                        name.Text,
                        description.Text == "Опишите подробности" ? "" : description.Text,
                        DateTime.Now.AddDays(7),
                        _taskId,
                        selectedUser.Id
                    );

                    newSubtask.Add();
                    MessageBox.Show("Подзадача успешно добавлена");
                }
                else
                {
                    // Обновляем существующую подзадачу
                    _editingSubtask.Name = name.Text;
                    _editingSubtask.Description = description.Text;
                    _editingSubtask.UserId = selectedUser.Id;

                    _editingSubtask.Update();
                    MessageBox.Show("Подзадача успешно обновлена");
                }

                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении подзадачи: {ex.Message}",
                              "Ошибка");
            }
        }
    }
}