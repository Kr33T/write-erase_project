using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Text.RegularExpressions;

namespace write_erase_project.windows
{
    /// <summary>
    /// Логика взаимодействия для editProductWindow.xaml
    /// </summary>
    public partial class editProductWindow : Window
    {
        public editProductWindow()
        {
            InitializeComponent();
            uploadFileds();
            actionBTN.Content = "Добавить";
        }

        Product product;
        bool isEditing = false;

        void uploadFileds()
        {
            productCategoryCB.ItemsSource = DBHelper.bE.ProductCategory.ToList();
            productCategoryCB.SelectedValuePath = "CategoryID";
            productCategoryCB.DisplayMemberPath = "CategoryName";

            productManufacturerCB.ItemsSource = DBHelper.bE.Manufacturer.ToList();
            productManufacturerCB.SelectedValuePath = "ManufacturerID";
            productManufacturerCB.DisplayMemberPath = "ManufacturerName";

            productProviderCB.ItemsSource = DBHelper.bE.Provider.ToList();
            productProviderCB.SelectedValuePath = "ProviderID";
            productProviderCB.DisplayMemberPath = "ProviderName";

            productUnitCB.ItemsSource = DBHelper.bE.Unit.ToList();
            productUnitCB.SelectedValuePath = "UnitID";
            productUnitCB.DisplayMemberPath = "UnitName";

            productCategoryCB.SelectedIndex = 0;
            productManufacturerCB.SelectedIndex = 0;
            productProviderCB.SelectedIndex = 0;
            productUnitCB.SelectedIndex = 0;
        }

        string file = "";

        public editProductWindow(Product product)
        {
            InitializeComponent();
            uploadFileds();
            this.product = product;
            isEditing = true;

            deleteBTN.Visibility = Visibility.Visible;
            actionBTN.Content = "Изменить";

            productNameTB.Text = product.ProductName;
            productCategoryCB.SelectedValue = product.ProductCategory;
            productManufacturerCB.SelectedValue = product.ProductManufacturer;
            productProviderCB.SelectedValue = product.ProductProvider;
            productCostTB.Text = product.ProductCost.ToString();
            productDiscountAmountTB.Text = product.ProductDiscountAmount.ToString();
            productQuantityInStockTB.Text = product.ProductQuantityInStock.ToString();
            productUnitCB.SelectedValue = product.ProductUnit;
            productDescriptionTB.Text = product.ProductDescription;

            file = product.ProductPhoto;


            if (!String.IsNullOrEmpty(product.ProductPhoto))
            {
                string path = Environment.CurrentDirectory.Replace("bin\\Debug", $"Resources\\{file}");
                productImage.Source = new BitmapImage(new Uri(path));
            }
            else
            {
                string path = Environment.CurrentDirectory.Replace("bin\\Debug", $"Resources\\picture.png");
                productImage.Source = new BitmapImage(new Uri(path));
            }
        }

        private void cancelBTN_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void deleteBTN_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Данный продукт удалится из всех существующих заказов.\nВы уверены что хотите удалить продукт?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                List<Order> orders = new List<Order>();
                foreach (var item in DBHelper.bE.OrderProduct.Where(x => x.Product.ProductArticleNumber == product.ProductArticleNumber))
                {
                    orders.Add(item.Order);
                    DBHelper.bE.OrderProduct.Remove(item);
                }
                DBHelper.bE.SaveChanges();

                foreach (var item in orders)
                {
                    int count = 0;
                    foreach (var item2 in DBHelper.bE.OrderProduct.Where(x=>x.OrderID == item.OrderID))
                    {
                        count++;
                    }
                    if (count == 0)
                    {
                        DBHelper.bE.Order.Remove(item);
                    }
                }

                DBHelper.bE.Product.Remove(product);
                DBHelper.bE.SaveChanges();
                MessageBox.Show("Продукт успешно удален!");
                this.Close();
            }
        }

        private void actionBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(productNameTB.Text) && !String.IsNullOrEmpty(productCostTB.Text) && !String.IsNullOrEmpty(productDiscountAmountTB.Text) && !String.IsNullOrEmpty(productQuantityInStockTB.Text))
            {
                if (Regex.IsMatch(productCostTB.Text, @"^[0-9 ]+$") && Regex.IsMatch(productDiscountAmountTB.Text, @"^[0-9 ]+$") && Regex.IsMatch(productQuantityInStockTB.Text, @"^[0-9 ]+$"))
                {
                    if (Convert.ToInt32(productDiscountAmountTB.Text) <= 100)
                    {
                        if (isEditing)
                        {
                            product.ProductName = productNameTB.Text;
                            product.ProductCategory = Convert.ToInt32(productCategoryCB.SelectedValue);
                            product.ProductManufacturer = Convert.ToInt32(productManufacturerCB.SelectedValue);
                            product.ProductProvider = Convert.ToInt32(productProviderCB.SelectedValue);
                            product.ProductCost = Convert.ToDecimal(productCostTB.Text);
                            product.ProductDiscountAmount = Convert.ToByte(productDiscountAmountTB.Text);
                            product.ProductQuantityInStock = Convert.ToInt32(productQuantityInStockTB.Text);
                            product.ProductUnit = Convert.ToInt32(productUnitCB.SelectedValue);
                            product.ProductDescription = productDescriptionTB.Text;
                            product.ProductPhoto = file;

                            DBHelper.bE.SaveChanges();
                            MessageBox.Show("Изменения внесены");
                            this.Close();
                        }
                        else
                        {
                            Product p = new Product();
                            Random rnd = new Random();

                            string article = "";
                            article += (char)rnd.Next('A', 'Z');
                            article += rnd.Next(100, 1000);
                            article += (char)rnd.Next('A', 'Z');
                            article += rnd.Next(1, 10);

                            p.ProductArticleNumber = article;
                            p.ProductName = productNameTB.Text;
                            p.ProductDescription = productDescriptionTB.Text;
                            p.ProductCategory = Convert.ToInt32(productCategoryCB.SelectedValue);
                            p.ProductPhoto = file;
                            p.ProductManufacturer = Convert.ToInt32(productManufacturerCB.SelectedValue);
                            p.ProductProvider = Convert.ToInt32(productProviderCB.SelectedValue);
                            p.ProductCost = Convert.ToDecimal(productCostTB.Text);
                            p.ProductDiscountAmount = Convert.ToByte(productDiscountAmountTB.Text);
                            p.ProductQuantityInStock = Convert.ToInt32(productQuantityInStockTB.Text);
                            p.ProductStatus = "";
                            p.ProductUnit = Convert.ToInt32(productUnitCB.SelectedValue);

                            DBHelper.bE.Product.Add(p);
                            DBHelper.bE.SaveChanges();
                            MessageBox.Show("Продукт добавлен");
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Скидка не может быть больше 100");
                    }
                }
                else
                {
                    MessageBox.Show("Проверьте правильность данных в полях \"Цена\", \"Скидка\", \"Количество на складе\"");
                }
            }
            else
            {
                MessageBox.Show("Заполнены не все необходимые поля!");
            }
        }

        private void deleteImageBTN_Click(object sender, RoutedEventArgs e)
        {
            file = "";

            string path = Environment.CurrentDirectory.Replace("bin\\Debug", $"Resources\\picture.png");
            productImage.Source = new BitmapImage(new Uri(path));
        }

        private void addImageBTN_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            file = ofd.FileName.Substring(ofd.FileName.LastIndexOf('\\') + 1);

            string path = Environment.CurrentDirectory.Replace("bin\\Debug", $"Resources");
            if (!File.Exists(path += $"\\{file}"))
            {
                File.Copy(ofd.FileName, path);
            }
            productImage.Source = new BitmapImage(new Uri(path));
        }
    }
}
