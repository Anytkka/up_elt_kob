using System.Windows;
using System.Windows.Controls;
using Project.Classes;

namespace Project.Pages
{
    public partial class SubtaskDetails : Page
    {
        private int _subtaskId;

        public SubtaskDetails(int subtaskId)
        {
            InitializeComponent();
            _subtaskId = subtaskId;
            LoadSubtaskDetails();
        }

        private void LoadSubtaskDetails()
        {
            var subtask = SubtaskContext.GetById(_subtaskId);
            if (subtask != null)
            {
                // Заполнение интерфейса данными подзадачи
            }
        }

        private void Bt_Close(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
