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
    /// Interaction logic for CreateTeamChat.xaml
    /// </summary>
    public partial class CreateTeamChat : Window
    {
        string AzureStorageConnectionString;
        string AzureStorageKey;
        string StorageAccountName;
        TableClient client;
        public CreateTeamChat()
        {
            InitializeComponent();
            this.AzureStorageConnectionString = "xx";
            this.AzureStorageKey = "xx";
            this.StorageAccountName = "xx";
            this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));
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
        }

        private void Button_Click_CreateChat(object sender, RoutedEventArgs e)
        {
            string GroupName = this.GroupName.Text;
            Pageable<TableEntity> existingGroups = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'TeamChat'");
            foreach (TableEntity entity in existingGroups)
            {
                if (entity.GetString("RowKey") == GroupName)
                {
                    this.ErrorMessage.Content = "Group name already exists";
                    this.GroupName.Text = "";
                    break;
                }
            }
            int count = existingGroups.Count() + 1;
            var newGroup = new TableEntity("TeamChat", GroupName)
            {
                {"TeamID", count.ToString() }
            };
            var newGroupMember = new TableEntity($"{GroupName}Member", App.Current.Properties["Username"].ToString())
            {
                {"Username", App.Current.Properties["Username"].ToString() },
                {"TeamID", count.ToString() },
                {"TeamName", GroupName }
            };
            this.client.AddEntity(newGroup);
            this.client.AddEntity(newGroupMember);
            this.Close();
        }
    }
}
