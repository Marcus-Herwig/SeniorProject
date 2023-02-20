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
        HashSet<string> selfCheck = new();
        string? selectedUsername;
        public HomeScreen()
        {
            InitializeComponent();
            this.AzureStorageConnectionString = "xx";
            this.AzureStorageKey = "xx";
            this.StorageAccountName = "xx";
            this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));

        }

        public async void checkForPendingFriendRequests()
        {
            HashSet<string> myFriends = new();
            HashSet<string> theirFriends = new();
            try
            {
                this.WelcomeBox.Content = "Welcome " + App.Current.Properties["Username"];
                await Task.Run(() =>
                {
                    for (; ; )
                    {
                        Pageable<TableEntity> queryForMyFriend = this.client.Query<TableEntity>(filter: $"RowKey eq '{App.Current.Properties["Username"]}Friend'");
                        foreach (TableEntity self in queryForMyFriend)
                        {
                            bool CreateUIElement = true;
                            Pageable<TableEntity> queryForTheirFriend = this.client.Query<TableEntity>(filter: $"RowKey eq '{self.GetString("FriendUsername")}Friend'");
                            if (queryForTheirFriend.Count() == 0)
                            {
                                CreateUIElement = false;
                            }
                            foreach (TableEntity friendCheck in queryForTheirFriend)
                            {
                                if (friendCheck.GetString("FriendUsername") == App.Current.Properties["Username"].ToString())
                                {
                                    CreateUIElement = false;
                                }
                            }
                            if (CreateUIElement == true)
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
                    
                        myFriends.Clear();
                        theirFriends.Clear();
                        Pageable<TableEntity> queryForRecievedRequests = this.client.Query<TableEntity>(filter: $"PartitionKey ne 'Account'");
                        foreach (TableEntity recieved in queryForRecievedRequests)
                        {
                            if (recieved.GetString("FriendUsername") == App.Current.Properties["Username"].ToString() && recieved.GetString("Username") != App.Current.Properties["Username"].ToString())
                            {
                                foreach (TableEntity Check in queryForMyFriend)
                                {
                                    this.selfCheck.Add(Check.GetString("FriendUsername"));
                                }

                                if (!selfCheck.Contains(recieved.GetString("Username")))
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
                                this.selfCheck.Clear();
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
            chatWin.ProfilePicture.Source = new BitmapImage(new Uri($@"{System.AppContext.BaseDirectory}\Images\{App.Current.Properties["ProfilePicture"]}"));
            chatWin.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }

        private void Button_Click_Logout(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            
            App.Current.Properties["Username"] = null;
            login.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }

        private void Button_Click_AddFriends(object sender, RoutedEventArgs e)
        { 
            try
            {
                string friend = this.FriendSearch.Content.ToString();

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
                        this.FriendSearch.Content = "";
                    }
                    else
                    {
                        MessageBox.Show($"The username ({friend}) you are trying to add could not be found! ");
                        this.FriendSearch.Content = "";
                        return;
                    }
                }                                       
            }
            catch
            {
               MessageBox.Show("Error: already added this username");
            }
            
            //Pageable<TableEntity> queryResultsFilter = this.client.Query<TableEntity>(filter: $"PartitionKey eq '{App.Current.Properties["Username"]}FriendList'");

        }

        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            HomeScreen newMain = new HomeScreen();
            newMain.ProfilePicture.Source = new BitmapImage(new Uri($@"{System.AppContext.BaseDirectory}\Images\{App.Current.Properties["ProfilePicture"]}"));
            newMain.checkForPendingFriendRequests();
            newMain.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }

        private void Button_Click_RemoveFriend(object sender, RoutedEventArgs e)
        {
            string username = this.FriendSearch.Content.ToString();
            string myPartionKey = $"Friend{App.Current.Properties["Username"]}{username}";
            string myRowKey = $"{App.Current.Properties["Username"]}Friend";
            string friendPartionKey = $"Friend{username}{App.Current.Properties["Username"]}";
            string friendRowKey = $"{username}Friend";

            Pageable<TableEntity> queryResultsFilter = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account' and RowKey eq '{username}'");
            if(queryResultsFilter.Count() > 0 && queryResultsFilter.Count() < 2)
            {
                this.client.DeleteEntityAsync(myPartionKey, myRowKey);
                this.client.DeleteEntityAsync(friendPartionKey, friendRowKey);
                this.FriendSearch.Content = "";
            }
            else
            {
                this.FriendSearch.Content = "";
                MessageBox.Show($"The username entered does not exist");
            }
        }

        private void Button_Click_Teams(object sender, RoutedEventArgs e)
        {
            TeamChatScreen teamScreen = new TeamChatScreen();
            teamScreen.ProfilePicture.Source = new BitmapImage(new Uri($@"{System.AppContext.BaseDirectory}\Images\{App.Current.Properties["ProfilePicture"]}"));
            teamScreen.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }

        private void Button_Click_Settings(object sender, RoutedEventArgs e)
        {
            SettingsMenu setting = new();
            setting.ProfilePicture.Source = new BitmapImage(new Uri($@"{System.AppContext.BaseDirectory}\Images\{App.Current.Properties["ProfilePicture"]}"));
            setting.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }

        private bool CheckSimilarity(string a, string b)
        {
            int matchCount = 0;
            int strikeCount = 0;
            bool isMatch = false;
            int length = 0;
            if (a.Length == 0 || b.Length == 0)
            {
                return false;
            }
            if (a.Length > b.Length)
            {
                length = a.Length;
            }
            else if (a.Length < b.Length)
            {
                length = b.Length;
            }
            else if (a.Length == b.Length)
            {
                length = a.Length;
            }

            for (int i = 0; i < length; i++)
            {
                if (a.ElementAt(i).ToString().ToLower() == b.ElementAt(i).ToString().ToLower())
                {
                    matchCount++;
                }
                else
                {
                    strikeCount++;
                }
                if (matchCount == 3)
                {
                    isMatch = true;
                }
                if (strikeCount == 3)
                {
                    return false;
                }
                if (isMatch)
                {
                    return true;
                }
            }
            return true;
        }

        private void Button_Click_SearchUsernames(object sender, RoutedEventArgs e)
        {
            this.SearchResults.Items.Clear();
            Pageable<TableEntity> AccountQuery = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account'");
            string comparer = this.UsernameSearch.Text;
            foreach (TableEntity entity in AccountQuery)
            {
                if (entity.GetString("Username") != App.Current.Properties["Username"].ToString())
                {
                    if (this.CheckSimilarity(entity.GetString("Username"), comparer) == true)
                    {
                        ListBoxItem labelTemplate = new ListBoxItem();
                        labelTemplate.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                        labelTemplate.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
                        labelTemplate.FontSize = 22;
                        labelTemplate.Content = entity.GetString("Username");
                        labelTemplate.Selected += ListBoxItem_SearchResults;
                        this.SearchResults.Items.Add(labelTemplate);
                        this.UsernameSearch.Text = "";
                    }
                }
            }
            this.UsernameSearch.Text = "";
        }

        private void ListBoxItem_SearchResults(object sender, RoutedEventArgs e)
        {
            this.FriendSearch.BorderThickness = new Thickness(2);
            foreach (ListBoxItem item in this.SearchResults.Items)
            {
                if (item == sender)
                {
                    this.selectedUsername = (string)item.Content;
                }
            }
            this.FriendSearch.Content = this.selectedUsername;
        }
    }
}
