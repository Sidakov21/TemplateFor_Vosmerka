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
            MainFrame.Navigate(new AuthPage());
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
