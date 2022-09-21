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
        public HomeScreen()
        {
            InitializeComponent();
            this.AzureStorageConnectionString = "https://seniorproject.table.core.windows.net/";
            this.AzureStorageKey = "cy2AiZ+cJ9/Ft6uXeq7bFVgT2zcSKniQcOGXC955XTbjqvhg1xdN4S34f0ZH9tVEIc3doK4kbxld+AStm9DbtQ==";
            this.StorageAccountName = "seniorproject";
            this.WelcomeBox.Text = "Welcome " + App.Current.Properties["Username"];
            this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));

        }

        public async void checkForPendingFriendRequests()
        {
            try
            {
                await Task.Run(() =>
                {
                    for(; ; )
                    {
                        Pageable<TableEntity> queryForMyFriend = this.client.Query<TableEntity>(filter: $"PartitionKey eq '{App.Current.Properties["Username"].ToString()}Friend'");
                        foreach (TableEntity entity in queryForMyFriend)
                        {
                            Pageable<TableEntity> queryForTheirFriend = this.client.Query<TableEntity>(filter: $"PartitionKey eq '{entity.GetString("Username")}Friend'");
                            foreach (TableEntity entity2 in queryForTheirFriend)
                            {
                                if (entity2.GetString("Username") == App.Current.Properties["Username"].ToString())
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        
                                    });
                                }
                            }
                        }
                        System.Threading.Thread.Sleep(5000);
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

                Pageable<TableEntity> queryResultsFilter = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account'");
                foreach (TableEntity entity in queryResultsFilter)
                {
                    if (entity.GetString("Username") == friend && entity.GetString("Username") != App.Current.Properties["Username"].ToString())
                    {
                        var newFriendEntity = new TableEntity($"{App.Current.Properties["Username"].ToString()}Friend", "FriendEntity")
                        {
                            {"Username", friend }
                        };
                        //this.client.CreateIfNotExistsAsync();
                        this.client.AddEntity(newFriendEntity);
                        this.FriendSearch.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                this.FriendSearch.Text = ex.Message;
            }
            
            //Pageable<TableEntity> queryResultsFilter = this.client.Query<TableEntity>(filter: $"PartitionKey eq '{App.Current.Properties["Username"]}FriendList'");

        }
    }
}
