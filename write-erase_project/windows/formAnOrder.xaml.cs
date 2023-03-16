using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Xml.Linq;

namespace write_erase_project
{
    /// <summary>
    /// Логика взаимодействия для formAnOrder.xaml
    /// </summary>
    public partial class formAnOrder : Window
    {
        public formAnOrder()
        {
            InitializeComponent();
            pickupPointCB.ItemsSource = DBHelper.bE.PickupPoint.ToList();
            pickupPointCB.SelectedValuePath = "PickupPointID";
            pickupPointCB.DisplayMemberPath = "fullNameOfPoint";
            if (String.IsNullOrEmpty(values.selectedPoint.ToString()))
            {
                pickupPointCB.SelectedIndex = 0;
            }
            else
            {
                pickupPointCB.SelectedIndex = values.selectedPoint;
            }
            refreshLV();
            if (values.user != null)
            {
                userNameTB.Text = "Пользователь: " + values.user.UserSurname + " " + values.user.UserName + " " + values.user.UserPatronymic;
            }
        }

        void refreshLV()
        {
            List<Product> p = new List<Product>();

            foreach (var item in values.products)
            {
                p.Add(item.product);
            }

            refreshCost();

            productLV.ItemsSource = p;
            productLV.SelectedValuePath = "ProductArticleNumber";

            if (values.products.Count == 0)
            {
                this.Close();
            }
        }

        void refreshCost()
        {
            try
            {
                double sumWithoutDiscount = 0, sumWithDiscount = 0, discountAmount = 0;

                foreach (var item in values.products)
                {
                    sumWithoutDiscount += (int)item.product.ProductCost * item.count;
                    sumWithDiscount += Convert.ToDouble(Math.Floor(Convert.ToDecimal(item.product.ProductCost - (item.product.ProductCost / 100 * item.product.ProductDiscountAmount)) * item.count));
                }
                discountAmount = (sumWithoutDiscount - sumWithDiscount) / (sumWithoutDiscount / 100);

                sumWithoutDiscountTB.Text = $"Стоимость без скидки: {Convert.ToInt32(sumWithoutDiscount)} руб.";
                sumWithDiscountTB.Text = $"Стоимость со скидкой: {Convert.ToInt32(sumWithDiscount)} руб.";
                discountAmountTB.Text = $"Размер скидки: {Convert.ToInt32(discountAmount)}%";
            }
            catch
            {

            }
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            string article = (sender as Border).Uid.ToString();
            Product p = DBHelper.bE.Product.FirstOrDefault(x => x.ProductArticleNumber == article);
            if (p.ProductDiscountAmount > 15)
            {
                (sender as Border).Background = new SolidColorBrush(Color.FromRgb(127, 255, 0));
            }
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            string artcl = (sender as Image).Uid.ToString();

            Product p = DBHelper.bE.Product.FirstOrDefault(x => x.ProductArticleNumber.Equals(artcl));
            if (!String.IsNullOrEmpty(p.ProductPhoto))
            {
                string path = Environment.CurrentDirectory.Replace("bin\\Debug", $"Resources\\{p.ProductPhoto}");
                (sender as Image).Source = new BitmapImage(new Uri(path));
            }
            else
            {
                string path = Environment.CurrentDirectory.Replace("bin\\Debug", $"Resources\\picture.png");
                (sender as Image).Source = new BitmapImage(new Uri(path));
            }
        }

        private void TextBlock_Loaded_1(object sender, RoutedEventArgs e)
        {
            string article = (sender as TextBlock).Uid.ToString();
            Product p = DBHelper.bE.Product.FirstOrDefault(x => x.ProductArticleNumber == article);
            if (!String.IsNullOrEmpty(p.ProductDiscountAmount.ToString()))
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

        private void deleteProductMI_Click(object sender, RoutedEventArgs e)
        {
            string article = productLV.SelectedValue.ToString();
            productsForOrder p = values.products.FirstOrDefault(x => x.product.ProductArticleNumber == article);
            values.products.Remove(p);

            refreshLV();
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            string id = (sender as TextBox).Uid.ToString();
            productsForOrder p = values.products.FirstOrDefault(x => x.product.ProductArticleNumber == id);
            (sender as TextBox).Text = p.count.ToString();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!(sender as TextBox).Text.Equals("0"))
                {
                    string article = (sender as TextBox).Uid.ToString();
                    values.products.FirstOrDefault(x => x.product.ProductArticleNumber == article).count = Convert.ToInt32((sender as TextBox).Text);
                    refreshCost();
                }
                else
                {
                    string article = (sender as TextBox).Uid.ToString();
                    productsForOrder p = values.products.FirstOrDefault(x => x.product.ProductArticleNumber == article);
                    values.products.Remove(p);

                    refreshLV();
                }
            }
            catch
            {

            }
        }

        private void formAnOrderBTN_Click(object sender, RoutedEventArgs e)
        {
            int orderId = DBHelper.bE.Order.Max(x => x.OrderID) + 1;
            Random rnd = new Random();

            Order order = new Order();
            order.OrderID = orderId;
            order.OrderStatusID = 1;
            order.OrderPickupPointID = Convert.ToInt32(pickupPointCB.SelectedValue);
            if (values.user == null)
            {
                order.OrderCustomerID = null;
            }
            else
            {
                order.OrderCustomerID = values.user.UserID;
            }
            order.ReceiptCode = rnd.Next(100, 1000);
            order.OrderDate = DateTime.Today;

            bool isProductEnough = true;
            foreach (var item in values.products)
            {
                if (item.count + 3 < item.product.ProductQuantityInStock)
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
                order.OrderDeliveryDate = DateTime.Today.AddDays(3);
            }
            else
            {
                order.OrderDeliveryDate = DateTime.Today.AddDays(6);
            }

            DBHelper.bE.Order.Add(order);

            foreach (var item in values.products)
            {
                OrderProduct op = new OrderProduct()
                {
                    OrderID = order.OrderID,
                    ProductArticleNumber = item.product.ProductArticleNumber,
                    CountProduct = item.count
                };
                DBHelper.bE.OrderProduct.Add(op);
            }
            DBHelper.bE.SaveChanges();
            values.products.Clear();
            values.selectedPoint = 0;
            MessageBox.Show("Заказ успешно оформлен!\nВам доступен талон для получения заказа.");
            formingPdfTicket(order.OrderID);
            this.Close();
        }

        void formingPdfTicket(int orderID)
        {
            Order order = DBHelper.bE.Order.FirstOrDefault(x=>x.OrderID == orderID);
            List<OrderProduct> op = DBHelper.bE.OrderProduct.Where(x => x.OrderID == order.OrderID).ToList();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF files(*.pdf)|*.pdf|All files(*.*)|*.*";
            if (sfd.ShowDialog() == true)
            {
                PdfDocument pdf = new PdfDocument();
                pdf.Info.Title = "Талон для получения заказа";
                PdfPage page = pdf.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
                XFont font = new XFont("Comic sans ms", 20);
                XFont fontCode = new XFont("Comic sans ms", 20, XFontStyle.Bold, options);

                gfx.DrawString("Дата заказа: " + order.OrderDate.ToString("dd MM yyyy"), font, XBrushes.Black, new XRect(0, -355, page.Width, page.Height), XStringFormat.Center);
                gfx.DrawString("Номер заказа: " + orderID.ToString(), font, XBrushes.Black, new XRect(0, -320, page.Width, page.Height), XStringFormat.Center);
                gfx.DrawString("Состав заказа:", font, XBrushes.Black, new XRect(0, -300, page.Width, page.Height), XStringFormat.Center);
                int height = -280;
                foreach (var item in DBHelper.bE.OrderProduct.Where(x => x.OrderID == order.OrderID))
                {
                    gfx.DrawString($"{item.Product.ProductArticleNumber}: {item.Product.ProductName} ({item.CountProduct} {item.Product.Unit.UnitName})", font, XBrushes.Black, new XRect(0, height, page.Width, page.Height), XStringFormat.Center);
                    height += 25;
                }

                double sum = 0, sumDis = 0;
                foreach (var item2 in DBHelper.bE.OrderProduct.Where(x => x.OrderID == order.OrderID))
                {
                    sum += (double)item2.Product.ProductCost * item2.CountProduct;
                    sumDis += (double)(item2.Product.ProductCost - (item2.Product.ProductCost / 100 * item2.Product.ProductDiscountAmount)) * item2.CountProduct;
                }
                height += 25;
                gfx.DrawString("Сумма заказа: " + sum.ToString(), font, XBrushes.Black, new XRect(0, height, page.Width, page.Height), XStringFormat.Center);
                height += 25;
                gfx.DrawString("Сумма со скидкой: " + sumDis.ToString(), font, XBrushes.Black, new XRect(0, height, page.Width, page.Height), XStringFormat.Center);
                height += 25;
                gfx.DrawString("Пункт выдачи: " + order.PickupPoint.fullNameOfPoint.ToString(), font, XBrushes.Black, new XRect(0, height, page.Width, page.Height), XStringFormat.Center);
                height += 25;
                if (order.User != null)
                {
                    gfx.DrawString("Пользователь: " + order.User.userFullName.ToString(), font, XBrushes.Black, new XRect(0, height, page.Width, page.Height), XStringFormat.Center);
                    height += 25;
                }
                else
                {
                    gfx.DrawString("Пользователь: Гость", font, XBrushes.Black, new XRect(0, height, page.Width, page.Height), XStringFormat.Center);
                    height += 25;
                }
                gfx.DrawString("Ваш код для получения заказа " + order.ReceiptCode.ToString(), fontCode, XBrushes.Black, new XRect(0, height, page.Width, page.Height), XStringFormat.Center);
                sfd.FileName = $"Талон для получения заказа №{order.OrderID}.pdf";
                pdf.Save(sfd.FileName);
                Process.Start(sfd.FileName);
            }
        }

        private void pickupPointCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            values.selectedPoint = pickupPointCB.SelectedIndex;
        }
    }
}
