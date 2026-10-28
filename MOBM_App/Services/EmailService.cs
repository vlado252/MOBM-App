using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MOBM_App.Models;

namespace MOBM_App.Services
{
    public static class EmailService
    {
        // Настройки SMTP (замени на свои!)
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "your_email@gmail.com";
        private const string SenderPassword = "your_app_password";

        public static bool SendEmail(string to, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(SmtpServer, SmtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(SenderEmail, SenderPassword);

                    var mail = new MailMessage(SenderEmail, to, subject, body);
                    mail.IsBodyHtml = true;

                    client.Send(mail);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отправки email: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static void SendOrderDetails(Order order, System.Collections.Generic.List<OrderItem> items)
        {
            string itemsText = string.Join("<br>",
                items.Select(i => "• " + i.Product.Name + " x" + i.Quantity + " = " + i.Sum.ToString("N2") + " BYN"));

            string emailBody = "<h2>ЗАКАЗ №" + order.Id + "</h2>" +
                              "<p><b>Дата:</b> " + order.Date.ToString("dd.MM.yyyy HH:mm") + "<br>" +
                              "<b>Оформил:</b> " + order.UserName + "<br>" +
                              "<b>Статус:</b> " + order.Status + "</p>" +
                              "<h3>Товары:</h3><p>" + itemsText + "</p>" +
                              "<h2 style='color:green'>ИТОГО: " + order.TotalPrice.ToString("N2") + " BYN</h2>";

            var emailWindow = new Window
            {
                Title = "Отправка чека по почте",
                Width = 400,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Foreground = Brushes.White
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Margin = new Thickness(20, 20, 20, 20);

            grid.Children.Add(new TextBlock { Text = "Email получателя:", Margin = new Thickness(0, 0, 0, 5) });
            Grid.SetRow(grid.Children[0], 0);

            var emailInput = new TextBox
            {
                Text = "customer@example.com",
                Padding = new Thickness(8, 8, 8, 8),
                Margin = new Thickness(0, 0, 0, 15)
            };
            grid.Children.Add(emailInput);
            Grid.SetRow(emailInput, 1);

            var sendBtn = new Button
            {
                Content = "Отправить",
                Background = new SolidColorBrush(Color.FromRgb(41, 121, 255)),
                Foreground = Brushes.White,
                Padding = new Thickness(20, 10, 20, 10),
                HorizontalAlignment = HorizontalAlignment.Right,
                BorderThickness = new Thickness(0, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            grid.Children.Add(sendBtn);
            Grid.SetRow(sendBtn, 2);

            sendBtn.Click += (s, e) =>
            {
                if (SendEmail(emailInput.Text,
                    "Чек заказа №" + order.Id, emailBody))
                {
                    MessageBox.Show("✅ Email успешно отправлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    emailWindow.Close();
                }
            };

            emailWindow.Content = grid;
            emailWindow.ShowDialog();
        }
    }
}