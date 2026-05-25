using System;
using System.Windows;
using System.Windows.Navigation;
using TemplateFor_Vosmerka.Pages;

namespace TemplateFor_Vosmerka
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Pages.AuthPage());
            MainFrame.Navigated += MainFrame_Navigated;
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (Core.LoggedUser != null)
                TxtUserFio.Text = Core.LoggedUser.Name;
            else
                TxtUserFio.Text = "Гость";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack) MainFrame.GoBack();
        }

        private void MainFrame_ContentRendered(object sender, EventArgs e)
        {
            if (MainFrame.CanGoBack)
                BtnBack.Visibility = Visibility.Visible;
            else
                BtnBack.Visibility = Visibility.Hidden;
        }
    }
}
