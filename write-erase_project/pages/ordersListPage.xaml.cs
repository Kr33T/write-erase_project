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

namespace write_erase_project
{
    /// <summary>
    /// Логика взаимодействия для ordersListPage.xaml
    /// </summary>
    public partial class ordersListPage : Page
    {

        public ordersListPage()
        {
            InitializeComponent();
            refreshList();
            orderLV.ItemsSource = values.orders;
            sortCB.SelectedIndex = 0;
            filterCB.SelectedIndex = 0;
        }

        void refreshList()
        {
            foreach (var item in DBHelper.bE.Order.ToList())
            {
                orderProductList opl = new orderProductList();
                opl.order = item;
                double sum = 0, sumDis = 0;
                double discount = 0;
                foreach (var item2 in DBHelper.bE.OrderProduct.Where(x => x.OrderID == item.OrderID))
                {
                    sum += (double)item2.Product.ProductCost * item2.CountProduct;
                    sumDis += (double)(item2.Product.ProductCost - (item2.Product.ProductCost / 100 * item2.Product.ProductDiscountAmount)) * item2.CountProduct;
                }
                discount = (sum - sumDis) / (sum / 100);
                opl.cost = Convert.ToInt32(sum);
                opl.discount = Convert.ToInt32(discount);
                values.orders.Add(opl);
            }
        }

        private void totalCostTB_Loaded(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32((sender as TextBlock).Uid);
            List<OrderProduct> op = DBHelper.bE.OrderProduct.Where(x => x.OrderID == id).ToList();
            int totalSum = 0;
            foreach (var item in op)
            {
                totalSum += (int)item.Product.ProductCost * item.CountProduct;
            }
            (sender as TextBlock).Text = $"Общая сумма заказа: {totalSum} руб.";
        }

        private void totalDiscountTB_Loaded(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32((sender as TextBlock).Uid);
            List<OrderProduct> op = DBHelper.bE.OrderProduct.Where(x => x.OrderID == id).ToList();
            int totalDiscount = 0;
            foreach (var item in op)
            {
                totalDiscount += (int)(item.Product.ProductCost - (item.Product.ProductCost / 100 * item.Product.ProductDiscountAmount)) * item.CountProduct;
            }
            (sender as TextBlock).Text = $"Сумма заказа со скидкой: {totalDiscount} руб.";
        }

        private void userNameTB_Loaded(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32((sender as TextBlock).Uid);
            User user = DBHelper.bE.Order.FirstOrDefault(x => x.OrderID == id).User;

            if (user != null)
                (sender as TextBlock).Text = $"Пользователь: {user.userFullName}";
            else
                (sender as TextBlock).Text = "Пользователь: Гость";
        }

        private void orderListLV_Loaded(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32((sender as ItemsControl).Uid);
            (sender as ItemsControl).ItemsSource = DBHelper.bE.OrderProduct.Where(x=>x.OrderID == id).ToList();
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32((sender as Border).Uid);
            List<OrderProduct> op = DBHelper.bE.OrderProduct.Where(x => x.OrderID == id).ToList();

            bool isProductEnough = true;
            foreach (var item in op)
            {
                if (item.CountProduct + 3 < item.Product.ProductQuantityInStock)
                {
                    isProductEnough = true;
                }
                else
                {
                    isProductEnough = false;
                    break;
                }
            }

            if (isProductEnough)
            {
                (sender as Border).BorderBrush = new SolidColorBrush(Color.FromRgb(32, 178, 170));
            }
            else
            {
                (sender as Border).BorderBrush = new SolidColorBrush(Color.FromRgb(255, 140, 0));
            }
        }

        private void orderStatusCB_Loaded(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32((sender as ComboBox).Uid);
            Order order = DBHelper.bE.Order.FirstOrDefault(x => x.OrderID == id);
            (sender as ComboBox).ItemsSource = DBHelper.bE.OrderStatus.ToList();
            (sender as ComboBox).SelectedValuePath = "StatusID";
            (sender as ComboBox).DisplayMemberPath = "StatusName";
            (sender as ComboBox).SelectedValue = order.OrderStatusID;
        }

        private void orderStatusCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int id = Convert.ToInt32((sender as ComboBox).Uid);
            Order order = DBHelper.bE.Order.FirstOrDefault(x => x.OrderID == id);
            order.OrderStatusID = Convert.ToInt32((sender as ComboBox).SelectedValue);
            DBHelper.bE.SaveChanges();
        }

        private void deliveryDateDP_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            int id = Convert.ToInt32((sender as DatePicker).Uid);
            Order order = DBHelper.bE.Order.FirstOrDefault(x => x.OrderID == id);

            order.OrderDeliveryDate = Convert.ToDateTime((sender as DatePicker).SelectedDate);
            DBHelper.bE.SaveChanges();
        }

        private void deliveryDateDP_Loaded(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32((sender as DatePicker).Uid);
            Order order = DBHelper.bE.Order.FirstOrDefault(x => x.OrderID == id);

            (sender as DatePicker).SelectedDate = order.OrderDeliveryDate;
        }

        private void sortCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filter();
        }

        private void filterCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filter();
        }

        void filter()
        {
            values.orders.Clear();
            refreshList();

            if (sortCB.SelectedIndex != 0)
            {
                switch (sortCB.SelectedIndex)
                {
                    case 1:
                        values.orders = values.orders.OrderBy(x => x.cost).ToList();
                        break;
                    case 2:
                        values.orders = values.orders.OrderByDescending(x => x.cost).ToList();
                        break;
                }
            }

            if (filterCB.SelectedIndex != 0)
            {
                switch (filterCB.SelectedIndex)
                {
                    case 1:
                        values.orders = values.orders.Where(x => x.discount <= 10).ToList();
                        break;
                    case 2:
                        values.orders = values.orders.Where(x => x.discount > 10 && x.discount <= 14).ToList();
                        break;
                    case 3:
                        values.orders = values.orders.Where(x => x.discount >= 15).ToList();
                        break;
                }
            }

            orderLV.ItemsSource = values.orders;
        }
    }
}
