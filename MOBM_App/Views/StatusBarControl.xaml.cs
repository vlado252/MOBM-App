using System;
using System.Windows.Controls;
using System.Windows.Threading;
using MOBM_App.Helpers;
using MOBM_App.Models;

namespace MOBM_App.Views
{
    public partial class StatusBarControl : UserControl
    {
        private DispatcherTimer _timer;

        public StatusBarControl()
        {
            InitializeComponent();
            UpdateUserInfo();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => UpdateDateTime();
            _timer.Start();
            UpdateDateTime();
        }

        private void UpdateUserInfo()
        {
            var user = DataStorage.CurrentUser;
            if (user != null)
            {
                TxtUser.Text = "👤 " + user.Name;
                TxtRole.Text = GetRoleName(user.UserRole);
            }
        }

        private void UpdateDateTime()
        {
            TxtDateTime.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private string GetRoleName(Role role)
        {
            switch (role)
            {
                case Role.Admin: return "🔑 Администратор";
                case Role.Manager: return "💼 Менеджер";
                case Role.Warehouse: return "📦 Кладовщик";
                default: return "Неизвестно";
            }
        }

        public void SetStatus(string message)
        {
            TxtStatus.Text = message;
        }

        public void UpdateStatusByRoleAndSection(string sectionName)
        {
            var user = DataStorage.CurrentUser;
            if (user == null) return;

            string status = "";
            switch (user.UserRole)
            {
                case Role.Admin:
                    status = "🔑 Администратор | " + sectionName + " | Полный доступ ко всем функциям";
                    break;
                case Role.Manager:
                    status = "💼 Менеджер | " + sectionName + " | Работа с заказами и клиентами";
                    break;
                case Role.Warehouse:
                    status = "📦 Кладовщик | " + sectionName + " | Управление складскими операциями";
                    break;
            }

            TxtStatus.Text = status;
        }
    }
}