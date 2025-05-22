using System.Windows;

namespace Project.Pages
{
    public partial class InputDialog : Window
    {
        public string Answer { get; private set; }

        public InputDialog(string title, string question, string defaultAnswer = "")
        {
            InitializeComponent();
            Title = title;
            QuestionText.Text = question;
            AnswerBox.Text = defaultAnswer;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Answer = AnswerBox.Text;
            DialogResult = true;
        }
    }
}