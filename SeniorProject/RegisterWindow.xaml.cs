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
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        string AzureStorageConnectionString;
        string AzureStorageKey;
        string StorageAccountName;
        MainWindow LoginScreen;
        TableClient? client;
        public RegisterWindow()
        {
            InitializeComponent();
            this.LoginScreen = new MainWindow();
            this.AzureStorageConnectionString = "https://seniorproject.table.core.windows.net/";
            this.AzureStorageKey = "cy2AiZ+cJ9/Ft6uXeq7bFVgT2zcSKniQcOGXC955XTbjqvhg1xdN4S34f0ZH9tVEIc3doK4kbxld+AStm9DbtQ==";
            this.StorageAccountName = "seniorproject";
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Button_Click_close(object sender, RoutedEventArgs e)
        {
            Close();
            Environment.Exit(0);
        }

        private void Button_Click_BackToLogin(object sender, RoutedEventArgs e)
        {
            this.LoginScreen.Show();
            this.Close();
        }

        private void Button_Click_CreateAccount(object sender, RoutedEventArgs e)
        {
            try
            {
                this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));
                // Check if Username exists
                Pageable<TableEntity> queryResultsFilter = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account'");
                foreach (TableEntity entity in queryResultsFilter)
                {
                    if (entity.GetString("Username") == UsernameRegister.Text)
                    {
                        this.ErrorMessage.Content = "Username already exists!!!";
                        this.PasswordRegister.Text = "";
                        this.ConfirmPassWordText.Text = "";
                        this.UsernameRegister.Text = "";
                        return;
                    }
                }

                    // Create Account
                    if (UsernameRegister.Text != null && UsernameRegister.Text != "" && PasswordRegister.Text != null && PasswordRegister.Text != "" && ConfirmPassWordText.Text == PasswordRegister.Text)
                    {
                        var entity = new TableEntity("Account", this.UsernameRegister.Text)
                        {
                            {"Username", this.UsernameRegister.Text },
                            {"Password", this.PasswordRegister.Text }
                        };
                        this.client.CreateIfNotExistsAsync();
                        this.client.AddEntity(entity);
                    }
                this.LoginScreen.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                this.UsernameRegister.Text = ex.Message;
            }
            
            
        }
    }
}
