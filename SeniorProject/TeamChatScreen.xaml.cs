using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SeniorProject
{
    /// <summary>
    /// Interaction logic for TeamChatScreen.xaml
    /// </summary>
    public partial class TeamChatScreen : Window
    {
        HomeScreen Home;
        TableClient client;
        string AzureStorageConnectionString;
        string AzureStorageKey;
        string StorageAccountName;
        string? groupNameString;
        bool isGroupSelected;
        HashSet<String> messages;
        HashSet<String> GroupNames;
        public TeamChatScreen()
        {
            InitializeComponent();
            this.Home = new HomeScreen();
            this.AzureStorageConnectionString = "https://seniorproject.table.core.windows.net/";
            this.AzureStorageKey = "cy2AiZ+cJ9/Ft6uXeq7bFVgT2zcSKniQcOGXC955XTbjqvhg1xdN4S34f0ZH9tVEIc3doK4kbxld+AStm9DbtQ==";
            this.StorageAccountName = "seniorproject";
            this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));
            this.creatGroupChatList();
            this.isGroupSelected = false;
            this.messages = new();
            this.GroupNames = new();
            this.UsernameLabel.Text = App.Current.Properties["Username"].ToString();
        }

        private void Button_Click_AddMember(object sender, RoutedEventArgs e)
        {
            if(this.GroupName.Content is not "" and not null)
            {
                AddMemberPage addMember = new(this.GroupName.Content.ToString());
                this.LabelError.Content = "";
                addMember.Show();
            }
            else
            {
                LabelError.Content = "Please select a groupchat first";
            }
        }

        private void Button_Click_CreateChat(object sender, RoutedEventArgs e)
        {
            CreateTeamChat creatGroup = new();
            creatGroup.Show();
        }

        private void Button_Click_Leave(object sender, RoutedEventArgs e)
        {
            LeaveGroupPopup leave = new(this.GroupName.Content.ToString());
            TeamChatScreen refresh = new();
            refresh.Show();
            this.Close();
            leave.Show();
        }
        private void Button_Click_Logout(object sender, RoutedEventArgs e)
        {
            //App.Current.Properties["Username"] = null;
            MainWindow login = new MainWindow();
            login.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }
        private void Button_Click_Home(object sender, RoutedEventArgs e)
        {
            this.Home.checkForPendingFriendRequests();
            this.Home.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void ExitApp(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }

        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.GroupName.Content is not null and not "")
                {
                    string message = chatText.Text;
                    LabelError.Content = "";
                    int count = 0;
                    Pageable<TableEntity> queryCounter = this.client.Query<TableEntity>(filter: $"PartitionKey eq '{this.GroupName.Content}ChatEntity'");
                    count = queryCounter.Count();
                    count = count + 1;
                    var newChat = new TableEntity($"{this.GroupName.Content}ChatEntity", count.ToString())
                    {
                        {"Message", message },
                        {"Sender", App.Current.Properties["Username"] },
                        {"TimeSent", DateTime.Now.ToString() },
                        {"TeamName", this.GroupName.Content }
                    };
                    this.client.AddEntity(newChat);
                    this.chatText.Text = "";
                }
                else
                {
                    this.chatText.Clear();
                    this.LabelError.Content = "Please selece a group chat to chat with";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        private void Button_Click_Chat(object sender, RoutedEventArgs e)
        {
            ChatScreen chatWin = new();
            chatWin.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }

        public async void creatGroupChatList()
        {
            try
            {
                await Task.Run(() =>
                {
                    bool shouldDelete = true;
                    for (; ; )
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            foreach (ListBoxItem item in this.GroupList.Items)
                            {
                                if (item.Name is not "" and not null)
                                {
                                    string queryName = item.Name;
                                    Pageable<TableEntity> NamesOfTeamsQuery = this.client.Query<TableEntity>(filter: $"PartitionKey eq '{queryName}Member'");
                                    foreach (TableEntity names in NamesOfTeamsQuery)
                                    {
                                        if (names.GetString("Username") == App.Current.Properties["Username"].ToString())
                                        {
                                            shouldDelete = false;
                                        }
                                    }
                                    if (shouldDelete)
                                    {
                                        this.GroupList.Items.Remove(item);
                                        break;
                                    }
                                }
                            }
                            
                        });
                        Pageable<TableEntity> queryForGroup = this.client.Query<TableEntity>(filter: $"RowKey eq '{App.Current.Properties["Username"]}'");
                        foreach (TableEntity entity in queryForGroup)
                        {
                            if (entity.GetString("TeamID") != null && entity.GetString("TeamID") != "0")
                            {
                                if (!this.GroupNames.Contains(entity.GetString("TeamID")))
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        this.GroupNames.Add(entity.GetString("TeamID"));
                                        ListBoxItem template = new ListBoxItem();
                                        template.Foreground = new SolidColorBrush(Colors.White);
                                        template.FontFamily = new FontFamily("Segoe UI");
                                        template.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#140d4f"));
                                        template.Content = entity.GetString("TeamName");
                                        template.FontSize = 18;
                                        template.BorderBrush = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#140d4f"));
                                        template.Template = Application.Current.Resources["ListBoxItemTemplate"] as ControlTemplate;
                                        template.Selected += ListBoxItem_Selected;
                                        template.Name = entity.GetString("TeamName");
                                        this.GroupList.Items.Add(template);
                                    });
                                }
                            }
                        }
                        System.Threading.Thread.Sleep(5000);
                    }
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            
        }
            

        private async void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            this.LabelError.Content = "";
            this.MembersListBox.Items.Clear();
            this.ChatMessages.Children.Clear();
            this.messages.Clear();
            this.isGroupSelected = false;
            this.groupNameString = null;
            foreach (ListBoxItem item in this.GroupList.Items)
            {
                if (item == sender)
                {
                    this.groupNameString = (string)item.Content;
                }
            }
            this.GroupName.Content = this.groupNameString;
            string memberQuery = $"{this.groupNameString}Member";
            Pageable<TableEntity> query = this.client.Query<TableEntity>(filter: $"PartitionKey eq '{memberQuery}'");
            foreach (TableEntity entity in query)
            {
                ListBoxItem template = new ListBoxItem();
                template.Foreground = new SolidColorBrush(Colors.White);
                template.FontFamily = new FontFamily("Segoe UI");
                template.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#140d4f"));
                template.Content = entity.GetString("Username");
                template.FontSize = 18;
                template.BorderBrush = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#140d4f"));
                template.Template = Application.Current.Resources["ListBoxItemTemplate"] as ControlTemplate;
                this.MembersListBox.Items.Add(template);
            }
            this.isGroupSelected = true;

            await Task.Run(() =>
            {
                try
                {
                    while (this.isGroupSelected == true)
                    {
                        Pageable<TableEntity> ChatQuery = this.client.Query<TableEntity>(filter: $"PartitionKey eq '{this.groupNameString}ChatEntity'");
                        foreach (TableEntity chat in ChatQuery)
                        {
                            if (!this.messages.Contains(chat.GetString("RowKey")))
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    TextBox text = new TextBox();
                                    text.Height = 30;
                                    text.TextWrapping = TextWrapping.Wrap;
                                    text.Margin = new Thickness(15, 15, 0, 0);
                                    text.HorizontalAlignment = HorizontalAlignment.Left;
                                    text.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#161554"));
                                    text.FontSize = 23;
                                    text.MaxWidth = 500;
                                    text.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                                    text.Text = chat.GetString("Sender") + ": " + chat.GetString("Message");
                                    text.IsReadOnly = true;
                                    text.BorderThickness = new Thickness(0);
                                    text.BorderBrush = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
                                    this.ChatMessages.Children.Add(text);
                                    this.messages.Add(chat.GetString("RowKey"));

                                    Label time = new Label();
                                    time.Height = 23;
                                    time.Margin = new Thickness(15, 0, 0, 0);
                                    time.HorizontalAlignment = HorizontalAlignment.Left;
                                    time.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#161554"));
                                    time.FontSize = 11;
                                    time.MaxWidth = 500;
                                    time.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                                    time.Content = chat.GetString("TimeSent");
                                    time.BorderBrush = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
                                    time.BorderThickness = new Thickness(0);
                                    this.ChatMessages.Children.Add(time);
                                });
                            }
                        }
                        System.Threading.Thread.Sleep(3000);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
        }

        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            TeamChatScreen refresh = new();
            refresh.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }

        private void Button_Click_Settings(object sender, RoutedEventArgs e)
        {
            SettingsMenu setting = new();
            setting.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }
    }
}