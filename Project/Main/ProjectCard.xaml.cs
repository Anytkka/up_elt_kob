using System;
using System.Windows;
using System.Windows.Controls;

namespace Project.Main
{
    public partial class ProjectCard : UserControl
    {
        public event EventHandler<int> ProjectClicked;

        public static readonly DependencyProperty ProjectNumberProperty =
            DependencyProperty.Register("ProjectNumber", typeof(int), typeof(ProjectCard), new PropertyMetadata(0));

        public static readonly DependencyProperty ProjectNameProperty =
            DependencyProperty.Register("ProjectName", typeof(string), typeof(ProjectCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CreatorNameProperty =
            DependencyProperty.Register("CreatorName", typeof(string), typeof(ProjectCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty UserRoleProperty =
            DependencyProperty.Register("UserRole", typeof(string), typeof(ProjectCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsPublicProperty =
            DependencyProperty.Register("IsPublic", typeof(bool), typeof(ProjectCard), new PropertyMetadata(false));

        public bool IsPublic
        {
            get { return (bool)GetValue(IsPublicProperty); }
            set { SetValue(IsPublicProperty, value); }
        }
        public string UserRole
        {
            get { return (string)GetValue(UserRoleProperty); }
            set { SetValue(UserRoleProperty, value); }
        }
        public int ProjectNumber
        {
            get { return (int)GetValue(ProjectNumberProperty); }
            set { SetValue(ProjectNumberProperty, value); }
        }
        public string ProjectName
        {
            get { return (string)GetValue(ProjectNameProperty); }
            set { SetValue(ProjectNameProperty, value); }
        }
        public string CreatorName
        {
            get { return (string)GetValue(CreatorNameProperty); }
            set { SetValue(CreatorNameProperty, value); }
        }
        public ProjectCard()
        {
            InitializeComponent();
        }

        private void ProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectClicked?.Invoke(this, ProjectNumber);
        }
    }
}