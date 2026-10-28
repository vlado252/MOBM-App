using System;
using System.Windows;
using MOBM_App.Services;
using MOBM_App.ViewModels;

namespace MOBM_App
{
    public partial class App : Application
    {
        public static OrderViewModel GlobalOrderVM { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // ✅ 1. Показываем прелоадер
                var loading = new Views.LoadingScreen();
                loading.Show();

                // ✅ 2. Выполняем инициализацию с прогрессом
                await System.Threading.Tasks.Task.Run(() =>
                {
                    loading.UpdateProgress(20, "Подключение к базе данных...");
                    System.Threading.Thread.Sleep(400);

                    loading.UpdateProgress(40, "Инициализация БД...");
                    DatabaseService.Initialize();
                    System.Threading.Thread.Sleep(400);

                    loading.UpdateProgress(70, "Загрузка данных...");
                    System.Threading.Thread.Sleep(400);

                    loading.UpdateProgress(90, "Подготовка интерфейса...");
                    System.Threading.Thread.Sleep(400);

                    loading.UpdateProgress(100, "Готово!");
                    System.Threading.Thread.Sleep(300);
                });

                // ✅ 3. Создаем глобальный ViewModel
                GlobalOrderVM = new OrderViewModel();

                // ✅ 4. Закрываем прелоадер
                loading.Close();

                // ✅ 5. Открываем окно входа
                var loginPage = new Views.LoginPage();
                loginPage.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при запуске:\n\n" + ex.Message,
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}