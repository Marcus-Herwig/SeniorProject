﻿using Azure;
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
            this.AzureStorageConnectionString = "xx";
            this.AzureStorageKey = "xx";
            this.StorageAccountName = "xx";
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
                    if (entity.GetString("RowKey").ToLower() == inputUsername.ToLower())
                    {
                        if (entity.GetString("Username").ToLower() == inputUsername.ToLower() && entity.GetString("Password") == inputPassword)
                        {
                            App.Current.Properties["ProfilePicture"] = entity.GetString("ProfilePicID");
                            App.Current.Properties["Username"] = entity.GetString("Username");
                            HomeScreen MainMenu = new HomeScreen();
                            MainMenu.ProfilePicture.Source = new BitmapImage(new Uri($@"{System.AppContext.BaseDirectory}\Images\{App.Current.Properties["ProfilePicture"]}"));
                            MainMenu.Show();
                            MainMenu.checkForPendingFriendRequests();
                            this.Close();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.UsernameText.Text = ex.Message;
            }
            this.LoginWarning.Content = "Please enter a valid username or password";
            this.UsernameText.Text = "";
            this.PassWordText.Text = "";
        }

        private void Button_Click_Register(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            this.Close();
            registerWindow.Show();
        }
    }
}
