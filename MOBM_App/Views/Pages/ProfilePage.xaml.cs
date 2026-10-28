using System.Windows;
using System.Windows.Controls;
using MOBM_App.Helpers;

namespace MOBM_App.Views.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            var user = DataStorage.CurrentUser;
            if (user != null)
            {
                TxtName.Text = user.Name;
                TxtEmail.Text = user.Email;
                TxtRole.Text = user.UserRole.ToString();
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
            // Логика возврата на экран входа должна быть реализована в App.xaml.cs или MainWindow
        }
    }
}