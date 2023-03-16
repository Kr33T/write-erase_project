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
using System.Drawing;
using write_erase_project.windows;

namespace write_erase_project
{
    /// <summary>
    /// Логика взаимодействия для productListPage.xaml
    /// </summary>
    public partial class productListPage : Page
    {
        public productListPage()
        {
            InitializeComponent();
            productLV.ItemsSource = DBHelper.bE.Product.ToList();
            productLV.SelectedValuePath = "ProductArticleNumber";
            sortCB.SelectedIndex = 0;
            discountFilterCB.SelectedIndex = 0;

            if (values.user != null)
            {
                if (values.user.UserRole == 3 || values.user.UserRole == 2)
                {
                    addProductBTN.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                addProductBTN.Visibility = Visibility.Collapsed;
            }

            int count = DBHelper.bE.Product.Count();
            recordsCounterTB.Text = $"{count} из {count}";
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            string artcl = (sender as System.Windows.Controls.Image).Uid.ToString();

            Product p = DBHelper.bE.Product.FirstOrDefault(x => x.ProductArticleNumber.Equals(artcl));
            if (!String.IsNullOrEmpty(p.ProductPhoto))
            {
                string path = Environment.CurrentDirectory.Replace("bin\\Debug", $"Resources\\{p.ProductPhoto}");
                (sender as System.Windows.Controls.Image).Source = new BitmapImage(new Uri(path));
            }
            else
            {
                string path = Environment.CurrentDirectory.Replace("bin\\Debug", $"Resources\\picture.png");
                (sender as System.Windows.Controls.Image).Source = new BitmapImage(new Uri(path));
            }
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            string article = (sender as TextBlock).Uid.ToString();
            Product p = DBHelper.bE.Product.FirstOrDefault(x=>x.ProductArticleNumber == article);
            if (!String.IsNullOrEmpty(p.ProductDiscountAmount.ToString()))
            {
                (sender as TextBlock).Text = $"Скидка {p.ProductDiscountAmount}%";
            }
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            string article = (sender as Border).Uid.ToString();
            Product p = DBHelper.bE.Product.FirstOrDefault(x => x.ProductArticleNumber == article);
            if (p.ProductDiscountAmount > 15)
            {
                (sender as Border).Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(127, 255, 0));
            }
        }

        private void TextBlock_Loaded_1(object sender, RoutedEventArgs e)
        {
            string article = (sender as TextBlock).Uid.ToString();
            Product p = DBHelper.bE.Product.FirstOrDefault(x => x.ProductArticleNumber == article);
            if(!String.IsNullOrEmpty(p.ProductDiscountAmount.ToString()))
            {
                (sender as TextBlock).Text = Convert.ToInt32(p.ProductCost).ToString() + " руб.";
                (sender as TextBlock).TextDecorations = TextDecorations.Strikethrough;
            }
            else
            {
                (sender as TextBlock).Text = Convert.ToInt32(p.ProductCost).ToString() + " руб.";
            }
        }

        private void TextBlock_Loaded_2(object sender, RoutedEventArgs e)
        {
            string article = (sender as TextBlock).Uid.ToString();
            Product p = DBHelper.bE.Product.FirstOrDefault(x => x.ProductArticleNumber == article);
            if (!String.IsNullOrEmpty(p.ProductDiscountAmount.ToString()))
            {
                (sender as TextBlock).Text = Math.Floor(Convert.ToDecimal(p.ProductCost - (p.ProductCost / 100 * p.ProductDiscountAmount))).ToString() + " руб.";
            }
        }

        void filter()
        {
            List<Product> products = DBHelper.bE.Product.ToList();
            int countAll = products.Count;

            switch (sortCB.SelectedIndex)
            {
                case 1:
                    products = products.OrderBy(x => x.ProductCost).ToList();
                    break;
                case 2:
                    products = products.OrderByDescending(x => x.ProductCost).ToList();
                    break;
            }

            switch(discountFilterCB.SelectedIndex)
            {
                case 1:
                    products = products.Where(x => x.ProductDiscountAmount < 10).ToList();
                    break;
                case 2:
                    products = products.Where(x => x.ProductDiscountAmount >= 10 && x.ProductDiscountAmount < 15).ToList();
                    break;
                case 3:
                    products = products.Where(x => x.ProductDiscountAmount >= 15).ToList();
                    break;
            }

            if (!String.IsNullOrEmpty(searchTB.Text))
            {
                products = products.Where(x => x.ProductName.ToLower().Contains(searchTB.Text.ToLower())).ToList();
            }

            productLV.ItemsSource = products;
            recordsCounterTB.Text = $"{products.Count} из {countAll}";
        }

        private void discountFilterCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filter();
        }

        private void searchTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            filter();
        }

        private void sortCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filter();
        }
        private void addToOrderMI_Click(object sender, RoutedEventArgs e)
        {
            Product p = DBHelper.bE.Product.FirstOrDefault(x => x.ProductArticleNumber.Equals(productLV.SelectedValue.ToString()));
            productsForOrder pfo = new productsForOrder()
            {
                product = p,
                count = 1
            };

            if(values.products.Where(x=>x.product == p).Count() > 0)
            {
                MessageBox.Show("Такой продукт уже добавлен в заказ");
            }
            else
            {
                values.products.Add(pfo);
            }

            if (values.products.Count > 0)
            {
                formAnOrderBTN.Visibility = Visibility.Visible;
            }
            else
            {
                formAnOrderBTN.Visibility = Visibility.Collapsed;
            }
        }

        bool isWindowOpened = false;

        private void formAnOrderBTN_Click(object sender, RoutedEventArgs e)
        {
            formAnOrder window = new formAnOrder();
            if (isWindowOpened)
            {
                MessageBox.Show("Окно уже открыто");
            }
            else
            {
                window.Show();
                isWindowOpened = true;
            }

            window.Closing += (obj, args) =>
            {
                isWindowOpened = false;
                if (values.products.Count == 0)
                {
                    formAnOrderBTN.Visibility = Visibility.Collapsed;
                }
            };
        }

        private void addProductBTN_Click(object sender, RoutedEventArgs e)
        {
            editProductWindow window = new editProductWindow();
            if (isWindowOpened)
            {
                MessageBox.Show("Окно уже открыто");
            }
            else
            {
                window.Show();
                isWindowOpened = true;
            }

            window.Closing += (obj, args) =>
            {
                isWindowOpened = false;
                productLV.ItemsSource = DBHelper.bE.Product.ToList();
                productLV.SelectedValuePath = "ProductArticleNumber";

                sortCB.SelectedIndex = 0;
                discountFilterCB.SelectedIndex = 0;
                searchTB.Text = "";
            };
        }

        private void updateProductBTN_Loaded(object sender, RoutedEventArgs e)
        {
            if (values.user != null)
            {
                if (values.user.UserRole == 3 || values.user.UserRole == 2)
                {
                    (sender as Button).Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                (sender as Button).Visibility = Visibility.Collapsed;
            }
        }

        private void updateProductBTN_Click(object sender, RoutedEventArgs e)
        {
            string id = (sender as Button).Uid.ToString();
            Product p = DBHelper.bE.Product.FirstOrDefault(x => x.ProductArticleNumber.Equals(id));
            editProductWindow window = new editProductWindow(p);

            if (isWindowOpened)
            {
                MessageBox.Show("Окно уже открыто");
            }
            else
            {
                window.Show();
                isWindowOpened = true;
            }

            window.Closing += (obj, args) =>
            {
                isWindowOpened = false;
                productLV.ItemsSource = DBHelper.bE.Product.ToList();
                productLV.SelectedValuePath = "ProductArticleNumber";

                sortCB.SelectedIndex = 0;
                discountFilterCB.SelectedIndex = 0;
                searchTB.Text = "";
            };
        }
    }
}
