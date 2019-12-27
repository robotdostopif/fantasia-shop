using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShoppingUI
{
    public class Product
    {
        public enum PriceUnits { l, kg, st }
        public PriceUnits PriceUnit { get; set; }

        private string name;
        private string description;
        private int price;
        public string Name
        {
            get { return name; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    name = value;
                }
            }
        }
        public string Description
        {
            get { return description; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    description = value;
                }
                else
                {
                    description = "Ingen beskrivning kunde hittas för den här produkten.";
                }
            }
        }
        public int Price
        {
            get { return price; }
            private set
            {
                if (value > 0)
                {
                    price = value;
                }
                else
                {
                    throw new Exception("Priset kan inte vara 0.");
                }
            }
        }

        public Product(string name, string description, int price, string unit)
        {
            Name = name;
            Description = description;
            Price = price;
            switch (unit.ToLower())
            {
                case "l":
                    PriceUnit = PriceUnits.l;
                    break;
                case "kg":
                    PriceUnit = PriceUnits.kg;
                    break;
                case "st":
                    PriceUnit = PriceUnits.st;
                    break;
                default:
                    PriceUnit = PriceUnits.st;
                    break;
            }
        }
    }

    public class ShoppingCart
    {
        public Dictionary<string, int> Orders { get; set; } = new Dictionary<string, int>();

        private const string PATH_CART = @"C:\Windows\Temp\Cart.txt";

        public void SaveCartToFile(bool showConfirmationMessage = true)
        {
            if (!File.Exists(PATH_CART))
            {
                File.Create(PATH_CART);
            }

            List<string> cartToFile = new List<string>();

            Orders.ToList().ForEach(x => cartToFile.Add(x.Key + ":" + x.Value));

            File.WriteAllLines(PATH_CART, cartToFile);
            if (showConfirmationMessage)
            {
                MessageBox.Show("Din kundvagn har sparats utan bekymmer");
            }
        }

        public void LoadCartFromFile(List<Product> availableProducts)
        {
            // If no cart, creates an empty cart file
            if (!File.Exists(PATH_CART))
            {
                File.Create(PATH_CART);
                return;
            }

            string[] cartFromFile;

            try
            {
                cartFromFile = File.ReadAllLines(PATH_CART);
            }
            catch
            {
                // Returns if no previous saved cart to read
                return;
            }

            foreach (var line in cartFromFile)
            {
                string[] cartItem = line.Split(':');

                try
                {
                    if(string.IsNullOrEmpty(cartItem[0]))
                    {
                        throw new ArgumentNullException();
                    }
                    int.Parse(cartItem[1]);
                }
                catch (ArgumentNullException e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
                catch(FormatException e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }

                if (availableProducts.Any(x => x.Name == cartItem[0]))
                {
                    Orders.Add(cartItem[0], int.Parse(cartItem[1]));
                }
            }

            // Saved cart session is emptied and overwritten with a blank file
            File.Create(PATH_CART);
        }

        public void EmptyCart()
        {
            Orders = new Dictionary<string, int>();
        }

        public void AddToCart(string productName, int amount)
        {
            if (Orders.ContainsKey(productName))
            {
                Orders[productName] += amount;
            }
            else
            {
                Orders.Add(productName, amount);
            }
        }

        public void RemoveFromCart(string productName, int amount)
        {
            productName = productName.Replace("[", "").Split(',').First();
            Orders[productName] -= amount;

            if (Orders[productName] <= 0)
            {
                Orders.Remove(productName);
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ReceiptWindow : Window
    {
        public ReceiptWindow()
        {
            Icon = BitmapFrame.Create(new Uri(@"pack://application:,,,/ShoppingUI;component/Assets/icon.png", UriKind.RelativeOrAbsolute));
            Width = 400;
            Height = 300;
            Title = "Ditt kvitto";

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Content = new Grid();
            Grid rwGrid = (Grid)Content;
            rwGrid.Margin = new Thickness(5);

            rwGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            rwGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50, GridUnitType.Pixel) });

            TextBlock textBlock_Receipt = new TextBlock()
            {
                FontSize = 16
            };
            textBlock_Receipt.Text = MainWindow.PrintCart();
            Grid.SetColumn(textBlock_Receipt, 0);
            Grid.SetRow(textBlock_Receipt, 0);

            Button button_Close = new Button()
            {
                Content = "Stäng fönster",
                Width = 150,
                Padding = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(button_Close, 0);
            Grid.SetRow(button_Close, 1);
            button_Close.Click += Close_Click;

            rwGrid.Children.Add(textBlock_Receipt);
            rwGrid.Children.Add(button_Close);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public partial class MainWindow : Window
    {
        private Grid sGrid;

        private ListBox listBox_Products;
        private ListBox listBox_Cart;

        private StackPanel stackPanel_OrderControls;
        private StackPanel stackPanel_Amount;
        private TextBlock textBlock_Amount;
        private Slider slider_Amount;
        private Button button_AddProduct;
        private Button button_RemoveProduct;
        private Button button_ClearCart;
        private Button button_SaveCart;
        private Button button_CheckOut;

        private StackPanel stackPanel_ProductDescription;
        private TextBlock textBlock_ProductInfo;
        private TextBlock textBlock_ProductPrice;

        private StackPanel stackPanel_CheckOut;
        private TextBlock textBlock_CheckOut;
        private Button button_Payment;

        private StackPanel stackPanel_DiscountControls;
        private StackPanel stackPanel_Discount;
        private Label label_Discount;
        private TextBox textBox_Discount;
        private Button button_Discount;

        private static List<Product> productAssortment;
        private static ShoppingCart cart;
        private List<string> availableCodes;

        // Controls to disable and hide parts of the UI
        private bool shoppingIsEnabled = true;
        private List<UIElement> lockedUI;

        private static bool hasDiscount = false;
        private string usedDiscountCode = "";

        private bool savedCart = true;   

        private const string PATH_DISCOUNT = @"Discount.txt";
        private const string PATH_PRODUCTS = @"Products.txt";

        public MainWindow()
        {
            InitializeComponent();
            Run();
        }

        private void Run()
        {
            productAssortment = new List<Product>();
            productAssortment = LoadProductsFromFile();

            availableCodes = new List<string>();
            availableCodes = LoadDiscountCodes(); 

            cart = new ShoppingCart();
            cart.LoadCartFromFile(productAssortment);

            lockedUI = new List<UIElement>();

            LoadGraphics();
            InitUIElements();
            PlaceUIElements();
        }

        public List<Product> LoadProductsFromFile()
        {
            List<Product> products = new List<Product>();

            string[] productsFromFile = File.ReadAllLines(PATH_PRODUCTS);

            if(productsFromFile.Length <= 0)
            {
                throw new Exception("Produktfilen är tom eller finns inte.");
            }

            foreach (var line in productsFromFile)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    string[] productToAdd = line.Split(':');

                    try
                    {
                        if (productToAdd[0].Length <= 0)
                        {
                            throw new ArgumentException("Produkten måste ha ett namn.");
                        }

                        if (products.Where(x => x.Name == productToAdd[0]).Any())
                        {
                            throw new ArgumentException("Upptäckte dubblett vid inmatning av produkter");
                        }

                        if (productToAdd.Length != 4)
                        {
                            throw new ArgumentException("Produkten har inte tillräckligt många fält");
                        }

                        if (!int.TryParse(productToAdd[2], out int res))
                        {
                            throw new ArgumentException("Produktens pris är felaktigt");
                        }

                        products.Add(new Product(productToAdd[0], productToAdd[1], int.Parse(productToAdd[2]), productToAdd[3]));
                    }
                    catch (ArgumentException e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
            return products;
        }

        public List<string> LoadDiscountCodes()
        {
            List<string> availableCodes = new List<string>();

            string[] codesFromFile = File.ReadAllLines(PATH_DISCOUNT);

            foreach (string line in codesFromFile)
            {
                bool valid = (line.Length == 4) ? true : false;

                for (int i = 0; i < line.Length && valid; i++)
                {
                    if (i % 2 == 0 && Char.IsLetter(line[i]) && Char.IsUpper(line[i]))
                    {
                        valid = valid && true;
                    }
                    else if (Char.IsDigit(line[i]))
                    {
                        valid = valid && true;
                    }
                    else
                    {
                        valid = false;
                    }
                }
                if (valid)
                {
                    availableCodes.Add(line);
                }
                else
                {
                    MessageBox.Show("Kod stämmer ej");
                }
            }
            return availableCodes;
        }

        public bool AddDiscount(string discountCode)
        {
            if (hasDiscount)
            {
                MessageBox.Show("Du har redan lagt till en rabattkod");

                textBox_Discount.Text = "";
                return true;
            }

            bool validDiscount = availableCodes.Contains(discountCode);

            if (validDiscount)
            {
                MessageBox.Show("Din rabattkod har lagts till!");

                usedDiscountCode = discountCode;
                hasDiscount = true;
            }
            else if (!string.IsNullOrEmpty(discountCode))
            {
                MessageBox.Show("Felaktig rabattkod.");
            }

            textBox_Discount.Text = "";
            return validDiscount;
        }

        private void LoadGraphics()
        {
            Icon = BitmapFrame.Create(new Uri(@"pack://application:,,,/ShoppingUI;component/Assets/icon.png", UriKind.RelativeOrAbsolute));
            Title = "Fried Chicken and Gasoline";
            Width = 920;
            Height = 740;

            Grid mainGrid = (Grid)Content;
            sGrid = new Grid();
            mainGrid.Children.Add(sGrid);

            sGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(6, GridUnitType.Star) });
            sGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) });
            sGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) });
            sGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(6, GridUnitType.Star) });

            sGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            sGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
            sGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            sGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
            sGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            sGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40, GridUnitType.Pixel) });
            sGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45, GridUnitType.Pixel) });
        }

        private void InitUIElements()
        {
            listBox_Products = new ListBox() { Background = Brushes.AliceBlue };
            listBox_Products.ItemsSource = productAssortment.Select(x => x.Name);
            listBox_Products.SelectionChanged += ProductInfo_OnSelect;

            textBlock_Amount = new TextBlock()
            {
                Text = "1",
                Margin = new Thickness(0, 0, 5, 0),
                Foreground = Brushes.Gray,
                FontWeight = FontWeights.Bold
            };
            stackPanel_Amount = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel_OrderControls = new StackPanel() { VerticalAlignment = VerticalAlignment.Top };
            slider_Amount = new Slider()
            {
                Width = 150,
                IsSnapToTickEnabled = true,
                Value = 1,
                Maximum = 5,
                Minimum = 1,
                VerticalAlignment = VerticalAlignment.Center
            };
            slider_Amount.ValueChanged += Amount_Changed;

            button_AddProduct = CreateButton("Lägg till i Varukorg →");
            button_AddProduct.Click += AddToCart_Click;

            button_RemoveProduct = CreateButton("← Ta bort från Varukorg");
            button_RemoveProduct.Click += RemoveFromCart_Click;

            button_ClearCart = CreateButton("Rensa Varukorg");
            button_ClearCart.Click += ClearCart_Click;

            button_SaveCart = CreateButton("Spara Varukorg");
            button_SaveCart.Click += SaveCart_Click;

            button_CheckOut = CreateButton("Gå till Kassan");
            button_CheckOut.Click += CheckOut_Click;

            listBox_Cart = new ListBox() { Background = Brushes.AliceBlue };
            listBox_Cart.ItemsSource = cart.Orders.Select(x => x.Key + ", " + x.Value);

            stackPanel_ProductDescription = new StackPanel
            {
                Background = Brushes.AliceBlue,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel_Discount = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 5, 0)
            };
            stackPanel_DiscountControls = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            label_Discount = CreateLabel("Rabattkod");
            label_Discount.Margin = new Thickness(0);
            textBox_Discount = new TextBox
            {
                Height = 30,
                Width = 105,
                Margin = new Thickness(0, 0, 5, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            button_Discount = CreateButton("Använd", 16, 110, 30);
            button_Discount.Click += Discount_Click;

            stackPanel_CheckOut = new StackPanel
            {
                Background = Brushes.AliceBlue
            };
            button_Payment = CreateButton("Betala", 16, 165, 30);
            button_Payment.Visibility = Visibility.Hidden;
            button_Payment.VerticalAlignment = VerticalAlignment.Bottom;
            button_Payment.Margin = new Thickness(0, 0, 0, 18);
            button_Payment.Click += Payment_Click;

            // Add to lockedUI the controls that we want to "toggle" 
            lockedUI.Add(listBox_Products);
            lockedUI.Add(listBox_Cart);
            lockedUI.Add(slider_Amount);
            lockedUI.Add(button_AddProduct);
            lockedUI.Add(button_RemoveProduct);
            lockedUI.Add(button_ClearCart);
            lockedUI.Add(button_SaveCart);
        }

        private void PlaceUIElements()
        {
            PlaceObject(new StackPanel()
            {
                Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/ShoppingUI;component/Assets/banner.jpg", UriKind.RelativeOrAbsolute)))
            }, 0, 0, 4);
            PlaceObject(new StackPanel()
            {
                Background = Brushes.AntiqueWhite
            }, 0, 1, 4, 6);

            PlaceObject(CreateLabel("Produktutbud"), 0, 1);
            PlaceObject(listBox_Products, 0, 2);

            stackPanel_Amount.Children.Add(textBlock_Amount);
            stackPanel_Amount.Children.Add(slider_Amount);

            stackPanel_OrderControls.Children.Add(stackPanel_Amount);
            stackPanel_OrderControls.Children.Add(button_AddProduct);
            stackPanel_OrderControls.Children.Add(button_RemoveProduct);
            stackPanel_OrderControls.Children.Add(button_ClearCart);
            stackPanel_OrderControls.Children.Add(button_SaveCart);
            stackPanel_OrderControls.Children.Add(button_CheckOut);
            PlaceObject(stackPanel_OrderControls, 1, 2, 2, 3);

            PlaceObject(CreateLabel("Varukorg"), 3, 1);
            PlaceObject(listBox_Cart, 3, 2);

            PlaceObject(CreateLabel("Produktinformation"), 0, 3);
            stackPanel_ProductDescription.Children.Add(textBlock_ProductInfo = new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                Padding = new Thickness(10)
            });
            stackPanel_ProductDescription.Children.Add(textBlock_ProductPrice = new TextBlock()
            {
                Padding = new Thickness(10)
            });
            PlaceObject(stackPanel_ProductDescription, 0, 4, 1, 3);
            PlaceObject(textBlock_ProductPrice = CreateTextBlock("", new Thickness(10)), 0, 6);

            stackPanel_Discount.Children.Add(label_Discount);
            stackPanel_Discount.Children.Add(stackPanel_DiscountControls);
            stackPanel_DiscountControls.Children.Add(textBox_Discount);
            stackPanel_DiscountControls.Children.Add(button_Discount);
            PlaceObject(stackPanel_Discount, 1, 5, 2, 2);

            stackPanel_CheckOut.Children.Add(textBlock_CheckOut = CreateTextBlock("", new Thickness(10)));
            PlaceObject(CreateLabel("Kassa"), 3, 3);
            PlaceObject(stackPanel_CheckOut, 3, 4, 1, 3);
            PlaceObject(button_Payment, 3, 5, 1, 2);
        }

        private void PlaceObject(UIElement obj, int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            Grid.SetColumn(obj, column);
            Grid.SetRow(obj, row);
            Grid.SetColumnSpan(obj, columnSpan);
            Grid.SetRowSpan(obj, rowSpan);
            sGrid.Children.Add(obj);
        }

        private TextBlock CreateTextBlock(string text, Thickness padding)
        {
            return new TextBlock()
            {
                Text = text,
                Padding = padding
            };
        }

        private Button CreateButton(string text, int fontsize = 16, double width = 185, double height = 32)
        {
            return new Button()
            {
                Content = text,
                Width = width,
                Height = height,
                FontSize = fontsize,
                Padding = new Thickness(5, 0, 5, 0),
                Margin = new Thickness(0, 5, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
        }

        private Label CreateLabel(string content, int size = 20)
        {
            return new Label
            {
                Content = content,
                FontSize = size,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkRed,
                VerticalAlignment = VerticalAlignment.Bottom
            };
        }

        private void RefreshVisualObjects()
        {
            // Updates information in certain GUI elements
            listBox_Cart.ItemsSource = cart.Orders.Select(x => x.Key + ", " + x.Value);

            if (listBox_Products.SelectedItem != null)
            {
                var product = productAssortment.Where(x => x.Name == listBox_Products.SelectedItem.ToString()).First();
                textBlock_ProductInfo.Text = product.Description;
                textBlock_ProductPrice.Text = product.Price + "kr / " + product.PriceUnit.ToString();
            }
        }

        public static string PrintCart()
        {
            string receipt = String.Empty;
            double totalPrice = 0;

            foreach (var order in cart.Orders)
            {
                Product product = productAssortment.Where(x => x.Name == order.Key).First();
                
                int price = product.Price * order.Value;
                receipt += order.Key + " " + order.Value +" " + product.PriceUnit.ToString() +": " + price + " kr\n";
                totalPrice = totalPrice + price;
            }

            if (hasDiscount)
            {
                receipt += "\nDu sparar: " + (int)(totalPrice * 0.1) + " kr";
                totalPrice = totalPrice * 0.9;
            }

            receipt += "\nTotal kostnad: " + (int)totalPrice + "kr";
            return receipt;
        }


        private void ToggleShopping()
        {
            // Enables all UI Controls when user wants to shop
            // Disables some UI Controls when user wants to check out
            shoppingIsEnabled = !shoppingIsEnabled;
            lockedUI.ForEach(x => x.IsEnabled = shoppingIsEnabled);

            button_CheckOut.Content = (shoppingIsEnabled) ? "Gå till Kassan" : "Fortsätt handla";

            stackPanel_Discount.Visibility = (shoppingIsEnabled) ? Visibility.Hidden : Visibility.Visible;
            button_Payment.Visibility = (shoppingIsEnabled) ? Visibility.Hidden : Visibility.Visible;

            textBlock_CheckOut.Text = (shoppingIsEnabled) ? "" : PrintCart();
        }

        private void ProductInfo_OnSelect(object sender, RoutedEventArgs e)
        {
            RefreshVisualObjects();
        }

        public void Amount_Changed(object sender, RoutedEventArgs e)
        {
            textBlock_Amount.Text = slider_Amount.Value.ToString();
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_Products.SelectedItem != null)
            {
                cart.AddToCart(listBox_Products.SelectedItem.ToString(), int.Parse(slider_Amount.Value.ToString()));
                savedCart = false;
                RefreshVisualObjects();

                listBox_Cart.SelectedIndex = cart.Orders.Count() - 1;
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
              if (listBox_Cart.SelectedItem != null)
              {
                cart.RemoveFromCart(listBox_Cart.SelectedValue.ToString(), int.Parse(slider_Amount.Value.ToString()));
                savedCart = false;
                RefreshVisualObjects();

                listBox_Cart.SelectedIndex = cart.Orders.Count() - 1;
             }
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (!cart.Orders.Any())
            {
                return;
            }

            cart.EmptyCart();
            RefreshVisualObjects();
        }

        private void SaveCart_Click(object sender, RoutedEventArgs e)
        {
            if (!cart.Orders.Any())
            {
                return;
            }
            cart.SaveCartToFile();
            savedCart = true;
        }

        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            if (!cart.Orders.Any())
            {
                return;
            }

            ToggleShopping();
        }

        private void Discount_Click(object sender, RoutedEventArgs e)
        {
            var foundDiscount = AddDiscount(textBox_Discount.Text);

            if (foundDiscount)
            {
                textBlock_CheckOut.Text = PrintCart();
            }
        }

        private void Payment_Click(object sender, RoutedEventArgs e)
        {
            ReceiptWindow rw = new ReceiptWindow();
            rw.Show();

            if (hasDiscount)
            {
                availableCodes.Remove(usedDiscountCode);
            }

            cart.EmptyCart();

            ToggleShopping();
            RefreshVisualObjects();
            hasDiscount = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cart.Orders.Any() && !savedCart)
            {
                MessageBoxResult result = MessageBox.Show("Du har varor i varukorgen. Vill du spara innan du avslutar?", "Hörru", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    cart.SaveCartToFile(false);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}