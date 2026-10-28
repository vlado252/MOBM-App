using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MOBM_App.Models;
using MOBM_App.Services;

namespace MOBM_App.Views.Pages
{
    public class RequestViewModel
    {
        public EmployeeRequest Request { get; set; }
        public int Id => Request.Id;
        public string FullName => Request.FullName;
        public string Login => Request.Login;
        public string Email => Request.Email;
        public string Phone => Request.Phone;
        public string Position => Request.Position;
        public string DesiredRoleName
        {
            get
            {
                switch (Request.DesiredRole)
                {
                    case Role.Admin: return "Администратор";
                    case Role.Manager: return "Менеджер";
                    case Role.Warehouse: return "Кладовщик";
                    default: return "Неизвестно";
                }
            }
        }
        public string Status => Request.Status;
        public string SubmittedDateStr => Request.SubmittedDate.ToString("dd.MM.yyyy HH:mm");

        public SolidColorBrush StatusColor
        {
            get
            {
                switch (Request.Status)
                {
                    case "На рассмотрении":
                        return new SolidColorBrush(Color.FromRgb(255, 145, 0));
                    case "Одобрено":
                        return new SolidColorBrush(Color.FromRgb(0, 230, 118));
                    case "Отклонено":
                        return new SolidColorBrush(Color.FromRgb(255, 23, 68));
                    default:
                        return new SolidColorBrush(Color.FromRgb(176, 176, 176));
                }
            }
        }
    }

    public partial class EmployeeRequestsPage : Page
    {
        private List<RequestViewModel> _requests;

        public EmployeeRequestsPage()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void LoadRequests()
        {
            try
            {
                var requests = DatabaseService.GetEmployeeRequests();
                _requests = requests.Select(r => new RequestViewModel { Request = r }).ToList();

                RequestsList.ItemsSource = _requests;
                TxtTotalRequests.Text = "Всего заявок: " + _requests.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заявок: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }

        private void BtnViewRequest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId)
            {
                var request = _requests.FirstOrDefault(r => r.Id == requestId)?.Request;
                if (request != null)
                {
                    ShowRequestDetails(request);
                }
            }
        }

        private void ShowRequestDetails(EmployeeRequest request)
        {
            var detailsWindow = new Window
            {
                Title = "Заявка #" + request.Id,
                Width = 600,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Foreground = Brushes.White,
                ResizeMode = ResizeMode.NoResize
            };

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(20)
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int row = 0;

            // Заголовок
            var headerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(41, 121, 255)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20, 20, 20, 20),
                Margin = new Thickness(0, 0, 0, 20)
            };
            var headerText = new TextBlock
            {
                Text = "ЗАЯВКА #" + request.Id,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            headerBorder.Child = headerText;
            grid.Children.Add(headerBorder);
            Grid.SetRow(headerBorder, row++);

            // Функция добавления поля
            Action<string, string, string> AddField = (label, value, icon) =>
            {
                var fieldBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(15, 15, 15, 15),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var fieldGrid = new Grid();
                fieldGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                fieldGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var labelText = new TextBlock
                {
                    Text = icon + " " + label + ":",
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 230, 118)),
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                var valueText = new TextBlock
                {
                    Text = value,
                    FontSize = 14,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Top
                };

                fieldGrid.Children.Add(labelText);
                Grid.SetColumn(labelText, 0);
                fieldGrid.Children.Add(valueText);
                Grid.SetColumn(valueText, 1);

                fieldBorder.Child = fieldGrid;
                grid.Children.Add(fieldBorder);
                Grid.SetRow(fieldBorder, row++);
            };

            AddField("ФИО полностью", request.FullName, "👤");
            AddField("Логин", request.Login, "🔑");
            AddField("Email", request.Email, "📧");
            AddField("Телефон", request.Phone, "📱");
            AddField("Желаемая должность", request.Position, "💼");
            AddField("Желаемая роль", GetRoleName(request.DesiredRole), "🎯");
            AddField("Дата подачи", request.SubmittedDate.ToString("dd.MM.yyyy HH:mm"), "📅");

            // Статус
            var statusBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15, 15, 15, 15),
                Margin = new Thickness(0, 0, 0, 10)
            };
            var statusGrid = new Grid();
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var statusLabel = new TextBlock
            {
                Text = "🎯 Статус:",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 230, 118)),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var statusValue = new TextBlock
            {
                Text = request.Status,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = GetStatusColor(request.Status),
                VerticalAlignment = VerticalAlignment.Top
            };

            statusGrid.Children.Add(statusLabel);
            Grid.SetColumn(statusLabel, 0);
            statusGrid.Children.Add(statusValue);
            Grid.SetColumn(statusValue, 1);
            statusBorder.Child = statusGrid;
            grid.Children.Add(statusBorder);
            Grid.SetRow(statusBorder, row++);

            // Комментарий
            var commentBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15, 15, 15, 15),
                Margin = new Thickness(0, 0, 0, 20)
            };
            var commentGrid = new Grid();
            commentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            commentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var commentLabel = new TextBlock
            {
                Text = "💬 Комментарий администратора:",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 230, 118)),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var commentBox = new TextBox
            {
                Text = request.AdminComment ?? "",
                FontSize = 13,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(68, 68, 68)),
                BorderThickness = new Thickness(1, 1, 1, 1),
                Padding = new Thickness(10, 10, 10, 10),
                Height = 80,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            commentGrid.Children.Add(commentLabel);
            Grid.SetRow(commentLabel, 0);
            commentGrid.Children.Add(commentBox);
            Grid.SetRow(commentBox, 1);
            commentBorder.Child = commentGrid;
            grid.Children.Add(commentBorder);
            Grid.SetRow(commentBorder, row++);

            // Кнопки
            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var btnCancel = new Button
            {
                Content = "❌ Отклонить",
                Background = new SolidColorBrush(Color.FromRgb(255, 23, 68)),
                Foreground = Brushes.White,
                Padding = new Thickness(25, 12, 25, 12),
                Margin = new Thickness(0, 0, 10, 0),
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0, 0, 0, 0),
                FontSize = 14
            };

            var btnApprove = new Button
            {
                Content = "✅ Одобрить и создать пользователя",
                Background = new SolidColorBrush(Color.FromRgb(0, 230, 118)),
                Foreground = Brushes.White,
                Padding = new Thickness(25, 12, 25, 12),
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0, 0, 0, 0),
                FontSize = 14
            };

            btnCancel.Click += (s, e) =>
            {
                if (MessageBox.Show("Отклонить заявку #" + request.Id + "?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        DatabaseService.UpdateRequestStatus(request.Id, "Отклонено", commentBox.Text);
                        MessageBox.Show("✅ Заявка отклонена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        detailsWindow.Close();
                        LoadRequests();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };

            btnApprove.Click += (s, e) =>
            {
                var roleWindow = new Window
                {
                    Title = "Выбор роли",
                    Width = 450,
                    Height = 250,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = detailsWindow,
                    Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                    Foreground = Brushes.White,
                    ResizeMode = ResizeMode.NoResize
                };

                var roleGrid = new Grid { Margin = new Thickness(20, 20, 20, 20) };
                roleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                roleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                roleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var infoText = new TextBlock
                {
                    Text = "Выберите роль для создания пользователя:",
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 15),
                    TextWrapping = TextWrapping.Wrap
                };
                roleGrid.Children.Add(infoText);
                Grid.SetRow(infoText, 0);

                var roleCombo = new ComboBox
                {
                    Padding = new Thickness(12, 10, 12, 10),
                    FontSize = 14,
                    Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                roleCombo.Items.Add(new ComboBoxItem { Content = "📦 Кладовщик", Tag = 2 });
                roleCombo.Items.Add(new ComboBoxItem { Content = "💼 Менеджер", Tag = 1 });
                roleCombo.Items.Add(new ComboBoxItem { Content = "🔑 Администратор", Tag = 0 });
                ((ComboBoxItem)roleCombo.Items[0]).IsSelected = true;
                roleGrid.Children.Add(roleCombo);
                Grid.SetRow(roleCombo, 1);

                var btnConfirm = new Button
                {
                    Content = "Создать пользователя",
                    Background = new SolidColorBrush(Color.FromRgb(0, 230, 118)),
                    Foreground = Brushes.White,
                    Padding = new Thickness(25, 12, 25, 12),
                    Cursor = Cursors.Hand,
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                btnConfirm.Click += (s2, e2) =>
                {
                    if (roleCombo.SelectedItem is ComboBoxItem selectedRole)
                    {
                        int roleId = (int)selectedRole.Tag;
                        try
                        {
                            DatabaseService.CreateUserFromRequest(request.Id, (Role)roleId);
                            DatabaseService.UpdateRequestStatus(request.Id, "Одобрено", commentBox.Text);

                            MessageBox.Show("✅ Пользователь создан!\n\n" +
                                "Логин: " + request.Login + "\n" +
                                "Роль: " + GetRoleName((Role)roleId),
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                            roleWindow.Close();
                            detailsWindow.Close();
                            LoadRequests();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                };

                roleGrid.Children.Add(btnConfirm);
                Grid.SetRow(btnConfirm, 2);
                roleWindow.Content = roleGrid;
                roleWindow.ShowDialog();
            };

            buttonsPanel.Children.Add(btnCancel);
            buttonsPanel.Children.Add(btnApprove);
            grid.Children.Add(buttonsPanel);
            Grid.SetRow(buttonsPanel, row);

            scrollViewer.Content = grid;
            detailsWindow.Content = scrollViewer;
            detailsWindow.ShowDialog();
        }

        private string GetRoleName(Role role)
        {
            switch (role)
            {
                case Role.Admin: return "Администратор";
                case Role.Manager: return "Менеджер";
                case Role.Warehouse: return "Кладовщик";
                default: return "Неизвестно";
            }
        }

        private SolidColorBrush GetStatusColor(string status)
        {
            switch (status)
            {
                case "На рассмотрении":
                    return new SolidColorBrush(Color.FromRgb(255, 145, 0));
                case "Одобрено":
                    return new SolidColorBrush(Color.FromRgb(0, 230, 118));
                case "Отклонено":
                    return new SolidColorBrush(Color.FromRgb(255, 23, 68));
                default:
                    return new SolidColorBrush(Color.FromRgb(176, 176, 176));
            }
        }
    }
}