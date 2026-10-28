using System.Windows;

namespace MOBM_App.Views
{
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}