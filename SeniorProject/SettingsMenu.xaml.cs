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
    /// Interaction logic for SettingsMenu.xaml
    /// </summary>
    public partial class SettingsMenu : Window
    {
        TableClient client;
        string AzureStorageConnectionString;
        string AzureStorageKey;
        string StorageAccountName;
        string? currentSelectedPicture;
        public SettingsMenu()
        {
            InitializeComponent();
            this.WelcomeBox.Text = App.Current.Properties["Username"].ToString();
            this.AzureStorageConnectionString = "https://seniorproject.table.core.windows.net/";
            this.AzureStorageKey = "cy2AiZ+cJ9/Ft6uXeq7bFVgT2zcSKniQcOGXC955XTbjqvhg1xdN4S34f0ZH9tVEIc3doK4kbxld+AStm9DbtQ==";
            this.StorageAccountName = "seniorproject";
            this.client = new TableClient(new Uri(this.AzureStorageConnectionString), "Accounts", new TableSharedKeyCredential(this.StorageAccountName, this.AzureStorageKey));
            this.WelcomeBox.Text = "Settings";
            this.currentSelectedPicture = null;
        }

        private void Button_Click_ShowChangeUsername(object sender, RoutedEventArgs e)
        {
            this.Messenger.Content = "";
            this.DeleteAccount.Visibility = System.Windows.Visibility.Hidden;
            this.ChangePassword.Visibility = System.Windows.Visibility.Hidden;
            this.ChangeProfilePic.Visibility = System.Windows.Visibility.Hidden;
            this.ChangeUsername.Visibility = System.Windows.Visibility.Visible;
        }
        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            SettingsMenu newMain = new SettingsMenu();
            newMain.ProfilePicture.Source = new BitmapImage(new Uri($@"{System.AppContext.BaseDirectory}\Images\{App.Current.Properties["ProfilePicture"]}"));
            newMain.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
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

        private void Button_Click_Teams(object sender, RoutedEventArgs e)
        {
            TeamChatScreen teamScreen = new TeamChatScreen();
            teamScreen.ProfilePicture.Source = new BitmapImage(new Uri($@"{System.AppContext.BaseDirectory}\Images\{App.Current.Properties["ProfilePicture"]}"));
            teamScreen.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }
        private void Button_Click_ChangeUsername(object sender, RoutedEventArgs e)
        {
            try
            {
                string key = "";
                Pageable<TableEntity> password = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account'");
                foreach (TableEntity passwordGet in password)
                {
                    if (passwordGet.GetString("Username") == App.Current.Properties["Username"].ToString())
                    {
                        key = passwordGet.GetString("Password");
                    }
                }
                if (key is null or "")
                {
                    MessageBox.Show("Error in the program when trying to reset username");
                    return;
                }
                if (this.UsernameText.Text is not null and not "" && this.UsernameText.Text == App.Current.Properties["Username"].ToString())
                {
                    this.Messenger.Content = "";
                    if (this.NewUsernameText.Text is not null and not "")
                    {
                        Pageable<TableEntity> newUsername = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account'");
                        foreach (TableEntity entity in newUsername)
                        {
                            if (entity.GetString("Username") == this.NewUsernameText.Text)
                            {
                                this.Messenger.Content = "The username you are trying to change to already exists";
                                this.NewUsernameText.Text = "";
                                return;
                            }
                        }
                        this.Messenger.Content = "";
                        if (this.Password.Text == key)
                        {
                            this.Messenger.Content = "";
                            var newAccount = new TableEntity("Account", this.NewUsernameText.Text)
                            {
                                {"Username", this.NewUsernameText.Text },
                                {"Password", key },
                                {"ProfilePicID", App.Current.Properties["ProfilePicture"] }
                            };
                            this.client.DeleteEntityAsync("Account", App.Current.Properties["Username"].ToString());
                            this.client.AddEntityAsync(newAccount);

                            Pageable<TableEntity> query = this.client.Query<TableEntity>(filter: $"PartitionKey ne 'account'");
                            foreach (TableEntity entry in query)
                            {
                                if (entry.GetString("Username") == App.Current.Properties["Username"].ToString() && entry.GetString("FriendUsername") is not null)
                                {
                                    var NewFriendEntity = new TableEntity($"Friend{this.NewUsernameText.Text}{entry.GetString("FriendUsername")}", $"{this.NewUsernameText.Text}Friend")
                                    {
                                        {"Username", this.NewUsernameText.Text },
                                        {"FriendUsername", entry.GetString("FriendUsername") }
                                    };
                                    this.client.DeleteEntityAsync(entry.GetString("PartitionKey"), entry.GetString("RowKey"));
                                    this.client.AddEntityAsync(NewFriendEntity);
                                }

                                if (entry.GetString("FriendUsername") == App.Current.Properties["Username"].ToString() && entry.GetString("Username") is not null)
                                {
                                    var NewFriendEntity = new TableEntity($"Friend{entry.GetString("Username")}{this.NewUsernameText.Text}", $"{entry.GetString("Username")}Friend")
                                    {
                                        {"Username", entry.GetString("Username") },
                                        {"FriendUsername", this.NewUsernameText.Text }
                                    };
                                    this.client.DeleteEntityAsync(entry.GetString("PartitionKey"), entry.GetString("RowKey"));
                                    this.client.AddEntityAsync(NewFriendEntity);
                                }
          
                                if (entry.GetString("Username") == App.Current.Properties["Username"].ToString() && entry.GetString("TeamID") is not null)
                                {
                                    var NewMemberEntity = new TableEntity(entry.GetString("PartitionKey"), this.NewUsernameText.Text)
                                    {
                                        {"Username", this.NewUsernameText.Text },
                                        {"TeamID", entry.GetString("TeamID") },
                                        {"TeamName", entry.GetString("TeamName") }
                                    };
                                    this.client.DeleteEntityAsync(entry.GetString("PartitionKey"), entry.GetString("RowKey"));
                                    this.client.AddEntityAsync(NewMemberEntity);
                                }

                                if (entry.GetString("Reciever") == App.Current.Properties["Username"].ToString() || entry.GetString("Sender") == App.Current.Properties["Username"].ToString() && entry.GetString("PartitionKey") == "chat" && entry.GetString("Message") != null)
                                {
                                    string newRowKeyNum = entry.GetString("RowKey").Substring(entry.GetString("RowKey").Length - 1);
                                    if (entry.GetString("Sender") == App.Current.Properties["Username"].ToString())
                                    {
                                        string rowKey = this.NewUsernameText.Text + "to" + entry.GetString("Reciever") + newRowKeyNum;
                                        var NewMemberEntity = new TableEntity("chat", rowKey)
                                        {
                                            {"Message", entry.GetString("Message") },
                                            {"Reciever", entry.GetString("Reciever") },
                                            {"Sender", this.NewUsernameText.Text },
                                            {"TimeSent", entry.GetString("TimeSent") }
                                        };
                                        this.client.DeleteEntityAsync(entry.GetString("PartitionKey"), entry.GetString("RowKey"));
                                        this.client.AddEntityAsync(NewMemberEntity);
                                    }
                                    else if (entry.GetString("Reciever") == App.Current.Properties["Username"].ToString())
                                    {
                                        string rowKey = entry.GetString("Reciever") + "to" + this.NewUsernameText.Text + newRowKeyNum;
                                        var NewMemberEntity = new TableEntity("chat", rowKey)
                                        {
                                            {"Message", entry.GetString("Message") },
                                            {"Reciever", this.NewUsernameText.Text },
                                            {"Sender", entry.GetString("Sender") },
                                            {"TimeSent", entry.GetString("TimeSent") }
                                        };
                                        this.client.DeleteEntityAsync(entry.GetString("PartitionKey"), entry.GetString("RowKey"));
                                        this.client.AddEntityAsync(NewMemberEntity);
                                    }
                                }

                            }


                            App.Current.Properties["Username"] = this.NewUsernameText.Text;
                            this.NewUsernameText.Text = "";
                            this.UsernameText.Text = "";
                            this.Password.Text = "";
                            this.Messenger.Content = "Successfully changed username";
                        }
                        else
                        {
                            this.Messenger.Content = "Password is incorrect";
                            this.Password.Text = "";
                        }
                    }
                    else
                    {
                        this.Messenger.Content = "Enter a new username";
                    }
                }
                else
                {
                    this.Messenger.Content = "Current username does not equal your username";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        
        private void Button_Click_ShowChangePassword(object sender, RoutedEventArgs e)
        {
            this.MessengerChangePassword.Content = "";
            this.DeleteAccount.Visibility = System.Windows.Visibility.Hidden;
            this.ChangeUsername.Visibility = System.Windows.Visibility.Hidden;
            this.ChangeProfilePic.Visibility = System.Windows.Visibility.Hidden;
            this.ChangePassword.Visibility = System.Windows.Visibility.Visible;
        }
        private void Button_Click_ChangePassword(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.CurrentPassword.Text is not null and not "")
                {
                    this.MessengerChangePassword.Content = "";
                    if (this.ConfirmPasswordChangeText.Text is not null and not "")
                    {
                        this.MessengerChangePassword.Content = "";
                        if (this.ConfirmPassword.Text == this.ConfirmPasswordChangeText.Text)
                        {
                            this.MessengerChangePassword.Content = "";
                            Pageable<TableEntity> query = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account'");
                            foreach (TableEntity entity in query)
                            {
                                if (entity.GetString("Username") == App.Current.Properties["Username"].ToString())
                                {
                                    if (entity.GetString("Password") == this.CurrentPassword.Text)
                                    {
                                        this.client.DeleteEntityAsync("Account", App.Current.Properties["Username"].ToString());
                                        var newEntity = new TableEntity("Account", App.Current.Properties["Username"].ToString())
                                        {
                                            {"Username", App.Current.Properties["Username"] },
                                            {"Password", this.ConfirmPassword.Text },
                                            {"ProfilePicID", App.Current.Properties["ProfilePicture"] }
                                        };
                                        this.client.AddEntityAsync(newEntity);
                                        this.CurrentPassword.Text = "";
                                        this.ConfirmPassword.Text = "";
                                        this.ConfirmPasswordChangeText.Text = "";
                                        this.MessengerChangePassword.Content = "Password successfully changed";
                                        System.Threading.Thread.Sleep(400);
                                        break;
                                    }
                                }
                                this.MessengerChangePassword.Content = "Password could not be changed";
                            }
                        }
                        else
                        {
                            this.MessengerChangePassword.Content = "Confirm Password does not equal new password";
                        }
                    }
                    else
                    {
                        this.MessengerChangePassword.Content = "Enter a new password";
                    }
                }
                else
                {
                    this.MessengerChangePassword.Content = "Enter your current password";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Button_Click_ShowDeleteAccount(object sender, RoutedEventArgs e)
        {
            this.MessengerDeleteAccount.Content = "";
            this.ChangeUsername.Visibility = System.Windows.Visibility.Hidden;
            this.ChangeProfilePic.Visibility = System.Windows.Visibility.Hidden;
            this.ChangePassword.Visibility = System.Windows.Visibility.Hidden;
            this.DeleteAccount.Visibility = System.Windows.Visibility.Visible;
        }
        private void Button_Click_DeleteAccount(object sender, RoutedEventArgs e)
        {
            try
            {
                Pageable<TableEntity> query = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account'");
                foreach (TableEntity entity in query)
                {
                    if (entity.GetString("Username") == App.Current.Properties["Username"].ToString())
                    {
                        if (this.DeleteAccountPasswordBox.Text is not null and not "")
                        {
                            if (entity.GetString("Password") == this.DeleteAccountPasswordBox.Text)
                            {
                                this.client.DeleteEntityAsync("Account", App.Current.Properties["Username"].ToString());
                                Pageable<TableEntity> queryfortable = this.client.Query<TableEntity>(filter: $"PartitionKey ne 'Account'");
                                foreach (TableEntity entity2 in queryfortable)
                                {
                                    if (entity2.GetString("Username") == App.Current.Properties["Username"].ToString())
                                    {
                                        this.client.DeleteEntityAsync(entity2.GetString("PartitionKey"), entity2.GetString("RowKey"));
                                    }
                                    if (entity2.GetString("FriendUsername") == App.Current.Properties["Username"].ToString())
                                    {
                                        this.client.DeleteEntityAsync(entity2.GetString("PartitionKey"), entity2.GetString("RowKey"));
                                    }
                                    if (entity2.GetString("Reciever") == App.Current.Properties["Username"].ToString() && entity2.GetString("PartitionKey") == "chat")
                                    {
                                        this.client.DeleteEntityAsync(entity2.GetString("PartitionKey"), entity2.GetString("RowKey"));
                                    }
                                    if (entity2.GetString("Sender") == App.Current.Properties["Username"].ToString() && entity2.GetString("PartitionKey") == "chat")
                                    {
                                        this.client.DeleteEntityAsync(entity2.GetString("PartitionKey"), entity2.GetString("RowKey"));
                                    }
                                }
                                this.DeleteAccountPasswordBox.Text = "";
                                App.Current.Properties["Username"] = "";
                                MainWindow login = new();
                                login.Show();
                                this.Close();
                            }
                            else
                            {
                                this.MessengerDeleteAccount.Content = "Password is incorrect";
                                this.DeleteAccountPasswordBox.Text = "";
                            }
                        }
                        else
                        {
                            this.MessengerDeleteAccount.Content = "Enter your password to delete account";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HomeScreen Home = new();
            Home.ProfilePicture.Source = new BitmapImage(new Uri($@"{System.AppContext.BaseDirectory}\Images\{App.Current.Properties["ProfilePicture"]}"));
            Home.checkForPendingFriendRequests();
            Home.Show();
            System.Threading.Thread.Sleep(200);
            this.Close();
        }

        private void Button_Click_ShowChangeProfilePicture(object sender, RoutedEventArgs e)
        {
            this.ChangeUsername.Visibility = System.Windows.Visibility.Hidden;
            this.ChangePassword.Visibility = System.Windows.Visibility.Hidden;
            this.DeleteAccount.Visibility = System.Windows.Visibility.Hidden;
            this.ChangeProfilePic.Visibility = System.Windows.Visibility.Visible;
        }

        private void Button_ClickKenobi(object sender, RoutedEventArgs e)
        {
            this.currentSelectedPicture = "kenobi-avatar.png";
            foreach (Button element in this.PicturePanel.Children)
            {
                element.BorderBrush = Brushes.Transparent;
            }
            this.kenobi.BorderBrush = Brushes.LightBlue;
        }
        private void Button_ClickDefaultPic(object sender, RoutedEventArgs e)
        {
            this.currentSelectedPicture = "DefaultPicture.png";
            foreach (Button element in this.PicturePanel.Children)
            {
                element.BorderBrush = Brushes.Transparent;
            }
            this.DefaultPic.BorderBrush = Brushes.LightBlue;
        }
        private void Button_ClickVader(object sender, RoutedEventArgs e)
        {
            this.currentSelectedPicture = "DarthVader.png";
            foreach (Button element in this.PicturePanel.Children)
            {
                element.BorderBrush = Brushes.Transparent;
            }
            this.Vader.BorderBrush = Brushes.LightBlue;
        }
        private void Button_ClickMando(object sender, RoutedEventArgs e)
        {
            this.currentSelectedPicture = "Mando.png";
            foreach (Button element in this.PicturePanel.Children)
            {
                element.BorderBrush = Brushes.Transparent;
            }
            this.Mando.BorderBrush = Brushes.LightBlue;
        }
        private void Button_Clickbatman(object sender, RoutedEventArgs e)
        {
            this.currentSelectedPicture = "batman.png";
            foreach (Button element in this.PicturePanel.Children)
            {
                element.BorderBrush = Brushes.Transparent;
            }
            this.batman.BorderBrush = Brushes.LightBlue;
        }
        private void Button_ClickMasterChief(object sender, RoutedEventArgs e)
        {
            this.currentSelectedPicture = "MasterChief.jpg";
            foreach (Button element in this.PicturePanel.Children)
            {
                element.BorderBrush = Brushes.Transparent;
            }
            this.MasaterChief.BorderBrush = Brushes.LightBlue;
        }
        private void Button_ClickKratos(object sender, RoutedEventArgs e)
        {
            this.currentSelectedPicture = "Kratos.jpg";
            foreach (Button element in this.PicturePanel.Children)
            {
                element.BorderBrush = Brushes.Transparent;
            }
            this.Kratos.BorderBrush = Brushes.LightBlue;
        }
        private void Button_ClickWalter(object sender, RoutedEventArgs e)
        {
            this.currentSelectedPicture = "WalterWhite.png";
            foreach (Button element in this.PicturePanel.Children)
            {
                element.BorderBrush = Brushes.Transparent;
            }
            this.Walter.BorderBrush = Brushes.LightBlue;
        }
        private void Button_ClickJessie(object sender, RoutedEventArgs e)
        {
            this.currentSelectedPicture = "Jessie.jpg";
            foreach (Button element in this.PicturePanel.Children)
            {
                element.BorderBrush = Brushes.Transparent;
            }
            this.Jessie.BorderBrush = Brushes.LightBlue;
        }

        private void Button_Click_ChangePicture(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.currentSelectedPicture is not null or "")
                {
                    Pageable<TableEntity> query = this.client.Query<TableEntity>(filter: $"PartitionKey eq 'Account'");
                    foreach (TableEntity entity in query)
                    {
                        if (entity.GetString("Username") == App.Current.Properties["Username"].ToString())
                        {
                            this.ProfileMessenger.Content = "";
                            App.Current.Properties["ProfilePicture"] = this.currentSelectedPicture;
                            var newAccountEntity = new TableEntity("Account", App.Current.Properties["Username"].ToString())
                            {
                                {"Username", entity.GetString("Username") },
                                {"Password", entity.GetString("Password") },
                                {"ProfilePicID", this.currentSelectedPicture }
                            };
                            this.client.DeleteEntityAsync("Account", entity.GetString("Username"));
                            System.Threading.Thread.Sleep(400);
                            this.client.AddEntityAsync(newAccountEntity);
                            this.currentSelectedPicture = "";
                            SettingsMenu newMenu = new();
                            newMenu.ProfilePicture.Source = new BitmapImage(new Uri($@"{System.AppContext.BaseDirectory}\Images\{App.Current.Properties["ProfilePicture"]}"));
                            newMenu.Show();
                            System.Threading.Thread.Sleep(200);
                            this.Close();
                        }
                    }
                }
                else
                {
                    this.ProfileMessenger.Content = "Please select a picture to change to";
                }
            }
            catch (Exception ex)
            {
                this.ProfileMessenger.Content = ex.Message;
            }
        }
    }
}
