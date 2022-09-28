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
    /// Interaction logic for HomeScreen.xaml
    /// </summary>
    public partial class HomeScreen : Window
    {
        TableClient client;
        string AzureStorageConnectionString;
        string AzureStorageKey;
        string StorageAccountName;
        public HashSet<string> addedFriends = new HashSet<string>();
        public HashSet<string> PendingFriends = new HashSet<string>();
        public HomeScreen()
        {
            InitializeComponent();
            this.AzureStorageConnectionString = "https://seniorproject.table.core.windows.net/";
            this.AzureStorageKey = "cy2AiZ+cJ9/Ft6uXeq7bFVgT2zcSKniQcOGXC955XTbjqvhg1xdN4S34f0ZH9tVEIc3doK4kbxld+AStm9DbtQ==";
            this.StorageAccountName = "seniorproject";
            this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));

        }

        public async void checkForPendingFriendRequests()
        {
            try
            {
                this.WelcomeBox.Text = "Welcome " + App.Current.Properties["Username"];
                await Task.Run(() =>
                {
                    for(; ; )
                    {
                        Pageable<TableEntity> queryForMyFriend = this.client.Query<TableEntity>(filter: $"RowKey eq '{App.Current.Properties["Username"]}Friend'");
                        foreach (TableEntity self in queryForMyFriend)
                        {
                            Pageable<TableEntity> queryForTheirFriend = this.client.Query<TableEntity>(filter: $"RowKey eq '{self.GetString("FriendUsername")}Friend'");
                            if (queryForTheirFriend.Count() == 0)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    if (!PendingFriends.Contains(self.GetString("FriendUsername")))
                                    {
                                        Label labelTemplate = new Label();
                                        labelTemplate.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                                        labelTemplate.FontSize = 20;
                                        labelTemplate.Content = self.GetString("FriendUsername");
                                        this.PendingFriendRequests.Children.Add(labelTemplate);
                                        PendingFriends.Add(self.GetString("FriendUsername"));
                                    }

                                });
                            }
                            else
                            {
                                foreach (TableEntity friend in queryForTheirFriend)
                                {
                                    if (self.GetString("FriendUsername") == friend.GetString("Username") && self.GetString("Username") == friend.GetString("FriendUsername"))
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            if (!addedFriends.Contains(self.GetString("FriendUsername")))
                                            {
                                                Label labelTemplate = new Label();
                                                labelTemplate.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                                                labelTemplate.FontSize = 20;
                                                labelTemplate.Content = self.GetString("FriendUsername");
                                                this.FriendsList.Children.Add(labelTemplate);
                                                addedFriends.Add(self.GetString("FriendUsername"));
                                            }
                                        });
                                    }
                                }
                            }
                        }
                        Pageable<TableEntity> queryForRecievedRequests = this.client.Query<TableEntity>(filter: $"PartionKey ne 'Accounts'");
                        foreach (TableEntity recieved in queryForRecievedRequests)
                        {
                            if (recieved.GetString("FriendUsername") == App.Current.Properties["Username"].ToString())
                            {
                                if (queryForMyFriend.Count() > 0)
                                {
                                    foreach (TableEntity selfcheck in queryForMyFriend)
                                    {
                                        if (selfcheck.GetString("FriendUsername") != recieved.GetString("Username"))
                                        {
                                            this.Dispatcher.Invoke(() =>
                                            {
                                                if (!PendingFriends.Contains(recieved.GetString("Username")))
                                                {
                                                    Label labelTemplate = new Label();
                                                    labelTemplate.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                                                    labelTemplate.FontSize = 20;
                                                    labelTemplate.Content = recieved.GetString("Username") + " (Added you)";
                                                    this.PendingFriendRequests.Children.Add(labelTemplate);
                                                    PendingFriends.Add(recieved.GetString("Username"));
                                                }
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        if (!PendingFriends.Contains(recieved.GetString("Username")))
                                        {
                                            Label labelTemplate = new Label();
                                            labelTemplate.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                                            labelTemplate.FontSize = 20;
                                            labelTemplate.Content = recieved.GetString("Username") + " (Added you)";
                                            this.PendingFriendRequests.Children.Add(labelTemplate);
                                            PendingFriends.Add(recieved.GetString("Username"));
                                        }
                                    });
                                }
                            }
                        }
                        System.Threading.Thread.Sleep(3000);
                    }
                });
            }
            catch (Exception ex)
            {
                
            }
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

        private void Button_Click_Chat(object sender, RoutedEventArgs e)
        {
            ChatScreen chatWin = new();
            chatWin.Show();
            this.Close();
        }

        private void Button_Click_Logout(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            App.Current.Properties["Username"] = null;
            login.Show();
            this.Close();
        }

        private void Button_Click_AddFriends(object sender, RoutedEventArgs e)
        { 
            try
            {
                string friend = this.FriendSearch.Text;

                Pageable<TableEntity> queryResultsFilter = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account' and RowKey eq '{friend}'");
                foreach (TableEntity entity in queryResultsFilter)
                {
                    if (entity.GetString("Username") == friend && entity.GetString("Username") != App.Current.Properties["Username"].ToString())
                    {
                        var newFriendEntity = new TableEntity($"Friend{App.Current.Properties["Username"]}{friend}", $"{App.Current.Properties["Username"]}Friend")
                        {
                            {"FriendUsername", friend },
                            {"Username", App.Current.Properties["Username"]}
                        };
                        //this.client.CreateIfNotExistsAsync();
                        this.client.AddEntity(newFriendEntity);
                        this.FriendSearch.Text = "";
                    }
                    else
                    {
                        MessageBox.Show($"The username ({friend}) you are trying to add could not be found! ");
                        this.FriendSearch.Text = "";
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
               MessageBox.Show("Error: already added this username");
            }
            
            //Pageable<TableEntity> queryResultsFilter = this.client.Query<TableEntity>(filter: $"PartitionKey eq '{App.Current.Properties["Username"]}FriendList'");

        }

        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            HomeScreen newMain = new HomeScreen();
            newMain.checkForPendingFriendRequests();
            newMain.Show();
            this.Close();
        }

        private void Button_Click_RemoveFriend(object sender, RoutedEventArgs e)
        {
            string username = this.FriendSearch.Text;
            string myPartionKey = $"Friend{App.Current.Properties["Username"]}{username}";
            string myRowKey = $"{App.Current.Properties["Username"]}Friend";
            string friendPartionKey = $"Friend{username}{App.Current.Properties["Username"]}";
            string friendRowKey = $"{username}Friend";

            Pageable<TableEntity> queryResultsFilter = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account' and RowKey eq '{username}'");
            if(queryResultsFilter.Count() > 0 && queryResultsFilter.Count() < 2)
            {
                this.client.DeleteEntityAsync(myPartionKey, myRowKey);
                this.client.DeleteEntityAsync(friendPartionKey, friendRowKey);
                this.FriendSearch.Text = "";
            }
            else
            {
                this.FriendSearch.Text = "";
                MessageBox.Show($"The username entered does not exist");
            }
        }
    }
}
