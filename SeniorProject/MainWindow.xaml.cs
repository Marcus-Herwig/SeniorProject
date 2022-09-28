using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SeniorProject.Configuration;

namespace SeniorProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TableClient? client;
        string AzureStorageConnectionString;
        string AzureStorageKey;
        string StorageAccountName;
        public MainWindow()
        {
            InitializeComponent();
            this.UsernameText.Focus();
            this.AzureStorageConnectionString = "https://seniorproject.table.core.windows.net/";
            this.AzureStorageKey = "cy2AiZ+cJ9/Ft6uXeq7bFVgT2zcSKniQcOGXC955XTbjqvhg1xdN4S34f0ZH9tVEIc3doK4kbxld+AStm9DbtQ==";
            this.StorageAccountName = "seniorproject";
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Button_Click_close(object sender, RoutedEventArgs e)
        {
            Close();
            Environment.Exit(0);
        }

        private void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            try
            {
                string inputUsername = this.UsernameText.Text;
                string inputPassword = this.PassWordText.Text;
                this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));
                Pageable<TableEntity> queryResultsFilter = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account'");
                foreach (TableEntity entity in queryResultsFilter)
                {
                    if (entity.GetString("RowKey") == inputUsername)
                    {
                        if (entity.GetString("Username") == inputUsername && entity.GetString("Password") == inputPassword)
                        {
                            App.Current.Properties["Username"] = this.UsernameText.Text.ToString();
                            HomeScreen MainMenu = new HomeScreen();
                            MainMenu.Show();
                            MainMenu.checkForPendingFriendRequests();
                            this.Close();
                            break;
                        }
                        else
                        {
                            this.LoginWarning.Content = "Please enter a valid username or password";
                            this.UsernameText.Text = "";
                            this.PassWordText.Text = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.UsernameText.Text = ex.Message;
            }
            
        }

        private void Button_Click_Register(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            this.Close();
            registerWindow.Show();
        }
    }
}
