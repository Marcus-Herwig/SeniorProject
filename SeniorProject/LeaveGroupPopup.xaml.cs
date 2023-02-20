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
    /// Interaction logic for LeaveGroupPopup.xaml
    /// </summary>
    public partial class LeaveGroupPopup : Window
    {
        string AzureStorageConnectionString;
        string AzureStorageKey;
        string StorageAccountName;
        TableClient client;
        string GroupName;
        public LeaveGroupPopup(string group)
        {
            InitializeComponent();
            this.GroupName = group;
            this.AzureStorageConnectionString = "xx";
            this.AzureStorageKey = "xx";
            this.StorageAccountName = "xx";
            this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));
            this.NameLabel.Content = $"Are you sure you want to leave the group: {this.GroupName}";
        }

        private void Button_Click_LeaveGroup(object sender, RoutedEventArgs e)
        {
            this.client.DeleteEntityAsync($"{this.GroupName}Member", App.Current.Properties["Username"].ToString());
            this.Close();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Button_Click_Stay(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
