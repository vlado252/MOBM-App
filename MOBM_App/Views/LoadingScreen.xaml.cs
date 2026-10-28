using System.Windows;
using System.Windows.Threading;

namespace MOBM_App.Views
{
    public partial class LoadingScreen : Window
    {
        public LoadingScreen()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int percent, string status)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = percent;
                TxtStatus.Text = status;
            });
        }
    }
}