using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project.Pages;
using Project.Classes;
using System.Windows.Navigation;

namespace Project.Main
{
    public partial class SubtaskCard : UserControl
    {
        public static readonly DependencyProperty SubtaskNumberProperty =
            DependencyProperty.Register("SubtaskNumber", typeof(int), typeof(SubtaskCard),
                new PropertyMetadata(0));

        public static readonly DependencyProperty SubtaskNameProperty =
            DependencyProperty.Register("SubtaskName", typeof(string), typeof(SubtaskCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ResponsibleProperty =
            DependencyProperty.Register("Responsible", typeof(string), typeof(SubtaskCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TaskCodeProperty =
            DependencyProperty.Register("TaskCode", typeof(string), typeof(SubtaskCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TaskNameProperty =
            DependencyProperty.Register("TaskName", typeof(string), typeof(SubtaskCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty UserRoleProperty =
            DependencyProperty.Register("UserRole", typeof(string), typeof(SubtaskCard),
                new PropertyMetadata(string.Empty));

        public event EventHandler<int> EditButtonClicked;
        public event EventHandler<int> DeleteButtonClicked;

        public int SubtaskNumber
        {
            get => (int)GetValue(SubtaskNumberProperty);
            set => SetValue(SubtaskNumberProperty, value);
        }

        public string SubtaskName
        {
            get => (string)GetValue(SubtaskNameProperty);
            set => SetValue(SubtaskNameProperty, value);
        }

        public string Responsible
        {
            get => (string)GetValue(ResponsibleProperty);
            set => SetValue(ResponsibleProperty, value);
        }

        public string TaskCode
        {
            get => (string)GetValue(TaskCodeProperty);
            set => SetValue(TaskCodeProperty, value);
        }

        public string TaskName
        {
            get => (string)GetValue(TaskNameProperty);
            set => SetValue(TaskNameProperty, value);
        }

        public string UserRole
        {
            get => (string)GetValue(UserRoleProperty);
            set => SetValue(UserRoleProperty, value);
        }

        public SubtaskCard()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, new DataObject("SubtaskCard", SubtaskNumber), DragDropEffects.Move);
            }
        }

        private void Border_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, new DataObject("SubtaskCard", SubtaskNumber), DragDropEffects.Move);
            }
        }

        private void DetailsTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Navigating to SubtaskDetailsPage for Subtask ID: {SubtaskNumber}");
            var subtaskDetailsPage = new SubtaskDetailsPage(SubtaskNumber);
            var navigationService = NavigationService.GetNavigationService(this);
            navigationService?.Navigate(subtaskDetailsPage);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Edit button clicked for Subtask ID: {SubtaskNumber}");
            EditButtonClicked?.Invoke(this, SubtaskNumber);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Delete button clicked for Subtask ID: {SubtaskNumber}");
            DeleteButtonClicked?.Invoke(this, SubtaskNumber);
        }
    }
}