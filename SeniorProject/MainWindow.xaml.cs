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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SeniorProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Startup Initializer = new Startup();
        ChatAppConfiguration _config;
        public MainWindow()
        {
            InitializeComponent();
            this._config = this.Initializer.ConfigureApp();
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
        }

        private void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            this.PassWordText.Text = this.UsernameText.Text;
            UsernameText.Text = "";
            
        }

        private void Button_Click_Register(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            this.Close();
            registerWindow.Show();
        }
    }
}
