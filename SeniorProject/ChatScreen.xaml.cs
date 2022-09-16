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
    /// Interaction logic for ChatScreen.xaml
    /// </summary>
    public partial class ChatScreen : Window
    {
        MainWindow login;
        HomeScreen Home;
        public ChatScreen()
        {
            InitializeComponent();
            this.login = new MainWindow();
            this.Home = new HomeScreen();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.friendsList.Items.Add("hi");
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

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem template = new ListBoxItem();
            template.Foreground = new SolidColorBrush(Colors.White);
            template.FontFamily = new FontFamily("Segoe UI");
            template.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#140d4f"));
            template.Content = "Hi";
            template.FontSize = 18;
            template.BorderBrush = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#140d4f"));
            template.Template = Application.Current.Resources["ListBoxItemTemplate"] as ControlTemplate;
            template.Selected += ListBoxItem_Selected;
            this.friendsList.Items.Add(template);
            
        }
        private void Button_Click_Logout(object sender, RoutedEventArgs e)
        {
            //App.Current.Properties["Username"] = null;
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void Button_Click_Home(object sender, RoutedEventArgs e)
        {
            this.Close();
            this.Home.Show();
        }
    }
}
