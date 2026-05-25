using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TemplateFor_Vosmerka.Pages
{
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = Core.Context.User.FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == PbPassword.Password);
            if (user != null)
            {
                Core.LoggedUser = user;
                NavigationService.Navigate(new ProductPage());
            }
            else
            {
                MessageHelper.ShowError("Неверный логин или пароль");
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductPage());
        }
    }
}
