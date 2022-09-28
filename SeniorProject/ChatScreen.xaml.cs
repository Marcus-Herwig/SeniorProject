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
    /// Interaction logic for ChatScreen.xaml
    /// </summary>
    public partial class ChatScreen : Window
    {
        HomeScreen Home;
        TableClient client;
        string AzureStorageConnectionString;
        string AzureStorageKey;
        string StorageAccountName;
        string? CurrFriend;
        HashSet<string> sentMessagesHash = new();
        HashSet<string> recievedMessagesHash = new();
        bool friendIsSelected;
        public ChatScreen()
        {
            InitializeComponent();
            this.Home = new HomeScreen();
            this.AzureStorageConnectionString = "https://seniorproject.table.core.windows.net/";
            this.AzureStorageKey = "cy2AiZ+cJ9/Ft6uXeq7bFVgT2zcSKniQcOGXC955XTbjqvhg1xdN4S34f0ZH9tVEIc3doK4kbxld+AStm9DbtQ==";
            this.StorageAccountName = "seniorproject";
            this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));
            this.CreateFriendList();
            this.CurrFriend = null;
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

        private void CreateFriendList()
        {
            Pageable<TableEntity> queryForMyFriends = this.client.Query<TableEntity>(filter: $"RowKey eq '{App.Current.Properties["Username"]}Friend'");
            foreach (TableEntity entity in queryForMyFriends)
            {
                Pageable<TableEntity> queryForTheirFriends = this.client.Query<TableEntity>(filter: $"RowKey eq '{entity.GetString("FriendUsername")}Friend'");
                foreach (TableEntity friendEntity in queryForTheirFriends)
                {
                    if (entity.GetString("Username") == friendEntity.GetString("FriendUsername"))
                    {
                        ListBoxItem template = new ListBoxItem();
                        template.Foreground = new SolidColorBrush(Colors.White);
                        template.FontFamily = new FontFamily("Segoe UI");
                        template.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#140d4f"));
                        template.Content = entity.GetString("FriendUsername");
                        template.FontSize = 18;
                        template.BorderBrush = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#140d4f"));
                        template.Template = Application.Current.Resources["ListBoxItemTemplate"] as ControlTemplate;
                        template.Selected += ListBoxItem_Selected;
                        this.friendsList.Items.Add(template);
                    }
                }
            }
        }
        private void Button_Click_Logout(object sender, RoutedEventArgs e)
        {
            this.friendIsSelected = false;
            //App.Current.Properties["Username"] = null;
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void Button_Click_Home(object sender, RoutedEventArgs e)
        {
            this.friendIsSelected = false;
            this.Home.Show();
            this.Close();
            
        }
        private async void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            this.friendIsSelected = false;
            this.CurrFriend = null;
            this.ChatMessages.Children.Clear();
            
            foreach (ListBoxItem item in this.friendsList.Items)
            {
                if (item == sender)
                {
                    this.CurrFriend = (string)item.Content;
                }
            }
            this.FriendName.Content = this.CurrFriend;
            this.friendIsSelected = true;

            await Task.Run(() =>
            {
                try
                {
                    while (this.friendIsSelected == true)
                    {
                        Pageable<TableEntity> QueryForChats = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'chat'");
                        foreach (TableEntity chat in QueryForChats)
                        {
                            if (chat.GetString("Sender") == App.Current.Properties["Username"].ToString() && chat.GetString("Reciever") == this.CurrFriend)
                            {
                                if (!this.sentMessagesHash.Contains(chat.GetString("Message")))
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        TextBlock text = new TextBlock();
                                        text.Height = 30;
                                        text.TextWrapping = TextWrapping.Wrap;
                                        text.Margin = new Thickness(0, 15, 15, 0);
                                        text.HorizontalAlignment = HorizontalAlignment.Right;
                                        text.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#161554"));
                                        text.FontSize = 23;
                                        text.MaxWidth = 500;
                                        text.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                                        text.Text = chat.GetString("Message");
                                        this.ChatMessages.Children.Add(text);
                                        this.sentMessagesHash.Add(chat.GetString("Message"));
                                    });
                                }
                            }
                            else if (chat.GetString("Sender") == this.CurrFriend && chat.GetString("Reciever") == App.Current.Properties["Username"].ToString())
                            {
                                if (!this.recievedMessagesHash.Contains(chat.GetString("Message")))
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        TextBlock text = new TextBlock();
                                        text.Height = 30;
                                        text.TextWrapping = TextWrapping.Wrap;
                                        text.Margin = new Thickness(15, 15, 0, 0);
                                        text.HorizontalAlignment = HorizontalAlignment.Left;
                                        text.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#161554"));
                                        text.FontSize = 23;
                                        text.MaxWidth = 500;
                                        text.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                                        text.Text = chat.GetString("Message");
                                        this.ChatMessages.Children.Add(text);
                                        this.recievedMessagesHash.Add(chat.GetString("Message"));
                                    });
                                }
                            }
                        }
                        System.Threading.Thread.Sleep(3000);
                    }
                
                }
                catch (Exception broke)
                {
                    MessageBox.Show(broke.ToString());
                }
            });
        }
        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {
            try
            {
                int SentCount = 0;
                Pageable<TableEntity> QueryForChats = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'chat'");
                foreach (TableEntity chat in QueryForChats)
                {
                    if (chat.GetString("Sender") == App.Current.Properties["Username"].ToString() && chat.GetString("Reciever") == this.CurrFriend)
                    {
                        SentCount++;
                    }
                }
                string EntityRowKey = App.Current.Properties["Username"] + "to" + this.FriendName.Content + SentCount.ToString();
                var newchat = new TableEntity("chat", EntityRowKey)
                {
                    {"Sender", App.Current.Properties["Username"] },
                    {"Reciever", this.FriendName.Content },
                    {"Message", this.chatText.Text }
                };
                this.client.AddEntity(newchat);
                this.chatText.Text = "";
            }
            catch (Exception i)
            {
                MessageBox.Show(i.Message);
            }
            
        }
    }
}
