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

namespace write_erase_project
{
    /// <summary>
    /// Логика взаимодействия для main.xaml
    /// </summary>
    public partial class main : Window
    {
        public main(User user)
        {
            InitializeComponent();
            frameClass.mainFrame = mainF;
            userNameTB.Text = user.UserName + " " + user.UserSurname;
            frameClass.mainFrame.Navigate(new productListPage());
            if (user.UserRole == 3)
            {
                cartBTN.Visibility = Visibility.Collapsed;
            }
        }

        public main()
        {
            InitializeComponent();
            frameClass.mainFrame = mainF;
            userNameTB.Text = "Гость";
            frameClass.mainFrame.Navigate(new productListPage());
            cartBTN.Visibility = Visibility.Collapsed;
        }

        private void productListBTN_Click(object sender, RoutedEventArgs e)
        {
            frameClass.mainFrame.Navigate(new productListPage());
        }

        private void cartBTN_Click(object sender, RoutedEventArgs e)
        {
            frameClass.mainFrame.Navigate(new ordersListPage());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            values.products.Clear();
            values.selectedPoint = 0;
            values.user = null;
        }
    }
}
