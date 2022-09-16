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
        MainWindow login;
        ChatScreen chatWin;
        public HomeScreen()
        {
            InitializeComponent();
            this.login = new MainWindow();
            this.chatWin = new ChatScreen();
            //this.WelcomeBox.Text = "Welcome " + App.Current.Properties["Username"];
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
            chatWin.Show();
            this.Close();
        }

        private void Button_Click_Logout(object sender, RoutedEventArgs e)
        {
            //App.Current.Properties["Username"] = null;
            login.Show();
            this.Close();
        }
    }
}
