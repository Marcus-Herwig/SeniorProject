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
    /// Interaction logic for AddMemberPage.xaml
    /// </summary>
    public partial class AddMemberPage : Window
    {
        string AzureStorageConnectionString;
        string AzureStorageKey;
        string StorageAccountName;
        TableClient client;
        int? TeamID;
        string GroupToAddTo;
        public AddMemberPage(string groupName)
        {
            InitializeComponent();
            this.GroupToAddTo = groupName;
            this.AzureStorageConnectionString = "https://seniorproject.table.core.windows.net/";
            this.AzureStorageKey = "cy2AiZ+cJ9/Ft6uXeq7bFVgT2zcSKniQcOGXC955XTbjqvhg1xdN4S34f0ZH9tVEIc3doK4kbxld+AStm9DbtQ==";
            this.StorageAccountName = "seniorproject";
            this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));
        }
        private void Button_Click_AddMember(object sender, RoutedEventArgs e)
        {
            try
            {
                string MemberName = this.MemberUsername.Text;
                Pageable<TableEntity> queryForID = this.client.Query<TableEntity>(filter: $"RowKey eq '{this.GroupToAddTo}'");
                foreach (TableEntity group in queryForID)
                {
                    if (group.GetString("RowKey") == this.GroupToAddTo)
                    {
                        this.TeamID = Int32.Parse(group.GetString("TeamID"));
                    }
                }
                Pageable<TableEntity> query = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account'");
                foreach (TableEntity entity in query)
                {
                    if (entity.GetString("Username") == MemberName)
                    {
                        var memberEntity = new TableEntity($"{this.GroupToAddTo}Member", MemberName)
                        {
                            {"Username", MemberName },
                            {"TeamID", this.TeamID.ToString() },
                            {"TeamName", this.GroupToAddTo }
                        };
                        this.client.AddEntity(memberEntity);
                        this.MemberUsername.Text = "";
                        this.ErrorMessage.Content = "Added successfully";
                        break;
                    }
                    else
                    {
                        this.ErrorMessage.Content = "Username does not exist";
                        this.MemberUsername.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
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
    }
}
