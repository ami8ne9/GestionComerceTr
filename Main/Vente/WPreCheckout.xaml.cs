using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GestionComerce.Vente; // For WFacturePreview and FactureSettings
using GestionComerce.Main.ClientPage; // For ClientFormWindow
using GestionComerce.Main.Facturation.CreateFacture; // For WFacturePage
using GestionComerce.Main.Facturation; // For InvoiceArticle

namespace GestionComerce.Main.Vente
{
    public partial class WPreCheckout : Window
    {
        public class CartItem
        {
            public Article Article { get; set; }
            public int Quantity { get; set; }
            public decimal DiscountPercent { get; set; }
            public decimal PrixUnitaire => Article.PrixVente;
            public decimal SubTotal => PrixUnitaire * Quantity;
            public decimal DiscountAmount => SubTotal * (DiscountPercent / 100);
            public decimal Total => SubTotal - DiscountAmount;
        }

        public List<CartItem> CartItems { get; set; }
        public decimal AdditionalDiscount { get; set; }
        public decimal ClientDiscount { get; set; }
        public bool IsPercentageDiscount { get; set; }
        public string PaymentMethodName { get; set; }
        public int PaymentMethodID { get; set; }
        public int PaymentType { get; set; }
        public Client SelectedClient { get; set; }
        public decimal CreditAmount { get; set; }

        private CMainV _parentVente;
        private List<Article> _availableArticles;
        private List<Famille> _familles;
        private List<Fournisseur> _fournisseurs;
        private List<Client> _allClients;

        public bool DialogConfirmed { get; private set; }

        public WPreCheckout(
            CMainV parentVente,
            List<CartItem> cartItems,
            string paymentMethod,
            int paymentMethodID,
            int paymentType,
            List<Article> availableArticles,
            List<Famille> familles,
            List<Fournisseur> fournisseurs)
        {
            InitializeComponent();

            _parentVente = parentVente;
            CartItems = new List<CartItem>(cartItems);
            PaymentMethodName = paymentMethod;
            PaymentMethodID = paymentMethodID;
            PaymentType = paymentType;
            _availableArticles = availableArticles;
            _familles = familles;
            _fournisseurs = fournisseurs;

            // Set window to maximized state for better visibility
            this.WindowState = WindowState.Maximized;
            this.MinWidth = 1200;
            this.MinHeight = 700;

            PaymentMethodText.Text = paymentMethod;

            // Set payment type label
            switch (paymentType)
            {
                case 0:
                    PaymentTypeText.Text = "VENTE COMPTANT";
                    CreditSection.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    PaymentTypeText.Text = "VENTE PARTIELLE (50/50)";
                    CreditSection.Visibility = Visibility.Visible;
                    ClientSelectionRequired.Visibility = Visibility.Visible;
                    break;
                case 2:
                    PaymentTypeText.Text = "VENTE À CRÉDIT";
                    CreditSection.Visibility = Visibility.Collapsed;
                    ClientSelectionRequired.Visibility = Visibility.Visible;
                    break;
            }

            // Load clients and wait for completion before loading cart
            this.Loaded += async (s, e) =>
            {
                await LoadClients();
                LoadCartItems();
                UpdateSummary();
            };
        }

        private async System.Threading.Tasks.Task LoadClients()
        {
            try
            {
                Client clientHelper = new Client();
                _allClients = await clientHelper.GetClientsAsync();

                FilterAndLoadClients("");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des clients: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterAndLoadClients(string searchText)
        {
            ClientComboBox.Items.Clear();
            ClientComboBox.Items.Add("Sans Client");

            if (_allClients == null) return;

            // Filter clients based on search text
            var filteredClients = _allClients;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                filteredClients = _allClients.Where(c =>
                    c.Nom.ToLower().Contains(searchText) ||
                    (c.Telephone != null && c.Telephone.Contains(searchText)) ||
                    (c.ICE != null && c.ICE.ToLower().Contains(searchText))
                ).ToList();
            }

            foreach (var client in filteredClients)
            {
                ClientComboBox.Items.Add($"{client.Nom} - {client.Telephone}");
            }

            if (ClientComboBox.Items.Count > 0)
            {
                ClientComboBox.SelectedIndex = 0;
            }
        }

        private void ClientSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = ClientSearchBox.Text;
            FilterAndLoadClients(searchText);
        }

        private void LoadCartItems()
        {
            ArticlesContainer.Children.Clear();

            if (CartItems == null || CartItems.Count == 0)
            {
                return;
            }

            foreach (var item in CartItems)
            {
                var itemControl = CreateCartItemControl(item);
                ArticlesContainer.Children.Add(itemControl);
            }
        }

        private Border CreateCartItemControl(CartItem item)
        {
            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = (SolidColorBrush)FindResource("BorderLight"),
                BorderThickness = new Thickness(1, 1, 1, 1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 16, 16, 16),
                Margin = new Thickness(0, 0, 0, 12)
            };

            // Main container with two rows: Info row and Controls row
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) }); // Spacer
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // ===== TOP ROW: Article Information =====
            var infoGrid = new Grid();
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star), MinWidth = 250 }); // Name & Details
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 180 }); // Stock & Package Info
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 180 }); // Codes & Additional Info
            Grid.SetRow(infoGrid, 0);

            // Left column: Article Name and Basic Details
            var leftPanel = new StackPanel { Orientation = Orientation.Vertical };

            var nameText = new TextBlock
            {
                Text = item.Article.ArticleName,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = (SolidColorBrush)FindResource("TextPrimary"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 6)
            };
            leftPanel.Children.Add(nameText);

            // Article details
            var detailsPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0, 4, 0, 0) };

            if (!string.IsNullOrWhiteSpace(item.Article.marque))
            {
                detailsPanel.Children.Add(CreateInfoText($"Marque: {item.Article.marque}", 11));
            }

            if (!string.IsNullOrWhiteSpace(item.Article.Description))
            {
                var descText = item.Article.Description.Length > 80
                    ? item.Article.Description.Substring(0, 80) + "..."
                    : item.Article.Description;
                detailsPanel.Children.Add(CreateInfoText(descText, 11, true));
            }

            if (item.Article.DateExpiration.HasValue)
            {
                var expiryDays = (item.Article.DateExpiration.Value - DateTime.Now).Days;
                var expiryText = $"Expiration: {item.Article.DateExpiration.Value:dd/MM/yyyy}";
                var expiryColor = expiryDays < 30 ? new SolidColorBrush(Color.FromRgb(239, 68, 68)) :
                                 expiryDays < 90 ? new SolidColorBrush(Color.FromRgb(245, 158, 11)) :
                                 (SolidColorBrush)FindResource("TextSecondary");
                detailsPanel.Children.Add(CreateInfoText(expiryText, 11, false, expiryColor));
            }

            leftPanel.Children.Add(detailsPanel);
            Grid.SetColumn(leftPanel, 0);

            // Middle column: Stock & Package Information
            var middlePanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(16, 0, 0, 0) };

            // Stock info with visual indicator
            var stockPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            var stockLabel = new TextBlock
            {
                Text = "Stock: ",
                FontSize = 11,
                Foreground = (SolidColorBrush)FindResource("TextSecondary")
            };
            var stockValue = new TextBlock
            {
                Text = item.Article.GetStockDisplayString(),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = item.Article.IsLowStock() ? new SolidColorBrush(Color.FromRgb(239, 68, 68)) :
                            item.Article.IsOverstock() ? new SolidColorBrush(Color.FromRgb(245, 158, 11)) :
                            new SolidColorBrush(Color.FromRgb(34, 197, 94))
            };
            stockPanel.Children.Add(stockLabel);
            stockPanel.Children.Add(stockValue);
            middlePanel.Children.Add(stockPanel);

            // Package information
            if (item.Article.PiecesPerPackage > 1)
            {
                var packagesNeeded = (int)Math.Ceiling((double)item.Quantity / item.Article.PiecesPerPackage);
                middlePanel.Children.Add(CreateInfoText($"📦 {item.Article.PiecesPerPackage} pcs/paquet", 11));
                middlePanel.Children.Add(CreateInfoText($"Paquets: {packagesNeeded}", 11, false, new SolidColorBrush(Color.FromRgb(59, 130, 246))));
            }

            if (!string.IsNullOrWhiteSpace(item.Article.PackageType))
            {
                middlePanel.Children.Add(CreateInfoText($"Type: {item.Article.PackageType}", 11));
            }

            if (!string.IsNullOrWhiteSpace(item.Article.UnitOfMeasure) && item.Article.UnitOfMeasure != "piece")
            {
                middlePanel.Children.Add(CreateInfoText($"Unité: {item.Article.UnitOfMeasure}", 11));
            }

            if (!string.IsNullOrWhiteSpace(item.Article.StorageLocation))
            {
                middlePanel.Children.Add(CreateInfoText($"📍 {item.Article.StorageLocation}", 11));
            }

            Grid.SetColumn(middlePanel, 1);

            // Right column: Codes & Additional Info
            var rightPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(16, 0, 0, 0) };

            if (item.Article.Code > 0)
            {
                rightPanel.Children.Add(CreateInfoText($"Code-barres: {item.Article.Code}", 11, false, new SolidColorBrush(Color.FromRgb(59, 130, 246))));
            }

            if (!string.IsNullOrWhiteSpace(item.Article.SKU))
            {
                rightPanel.Children.Add(CreateInfoText($"SKU: {item.Article.SKU}", 11));
            }

            if (!string.IsNullOrWhiteSpace(item.Article.numeroLot))
            {
                rightPanel.Children.Add(CreateInfoText($"Lot: {item.Article.numeroLot}", 11));
            }

            if (item.Article.tva > 0)
            {
                rightPanel.Children.Add(CreateInfoText($"TVA: {item.Article.tva}%", 11));
            }

            // Wholesale price info
            if (item.Article.MinQuantityForGros > 0 && item.Article.PrixGros > 0)
            {
                var isWholesale = item.Quantity >= item.Article.MinQuantityForGros;
                if (isWholesale)
                {
                    rightPanel.Children.Add(CreateInfoText($"💰 Prix Gros actif!", 11, false, new SolidColorBrush(Color.FromRgb(34, 197, 94))));
                }
                else
                {
                    rightPanel.Children.Add(CreateInfoText($"Prix Gros: {item.Article.MinQuantityForGros}+ unités", 10, true));
                }
            }

            Grid.SetColumn(rightPanel, 2);

            infoGrid.Children.Add(leftPanel);
            infoGrid.Children.Add(middlePanel);
            infoGrid.Children.Add(rightPanel);

            // ===== BOTTOM ROW: Controls (Price, Qty, Discount, Total, Delete) =====
            var controlsGrid = new Grid();
            controlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) }); // Price
            controlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Quantity
            controlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Discount
            controlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Spacer
            controlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) }); // Total
            controlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); // Delete
            Grid.SetRow(controlsGrid, 2);

            // Create totalText first so it can be referenced
            var totalText = new TextBlock
            {
                Text = item.Total.ToString("F2") + " DH",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (SolidColorBrush)FindResource("PrimaryBlue"),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right
            };
            Grid.SetColumn(totalText, 4);

            // Price input
            var pricePanel = CreateControlPanel("Prix Unit.", priceTextBox =>
            {
                priceTextBox.Text = item.PrixUnitaire.ToString("F2");
                priceTextBox.TextChanged += (s, e) =>
                {
                    if (decimal.TryParse(priceTextBox.Text, out decimal newPrice) && newPrice >= 0)
                    {
                        item.Article.PrixVente = newPrice;
                        totalText.Text = item.Total.ToString("F2") + " DH";
                        UpdateSummary();
                    }
                };
            });
            Grid.SetColumn(pricePanel, 0);

            // Quantity input
            var qtyPanel = CreateControlPanel("Quantité", qtyTextBox =>
            {
                qtyTextBox.Text = item.Quantity.ToString();
                qtyTextBox.TextAlignment = TextAlignment.Center;
                qtyTextBox.TextChanged += (s, e) =>
                {
                    if (int.TryParse(qtyTextBox.Text, out int newQty) && newQty > 0)
                    {
                        if (item.Article.IsUnlimitedStock || newQty <= item.Article.Quantite)
                        {
                            item.Quantity = newQty;
                            totalText.Text = item.Total.ToString("F2") + " DH";
                            UpdateSummary();
                        }
                        else
                        {
                            MessageBox.Show($"Quantité disponible: {item.Article.Quantite}", "Stock Insuffisant", MessageBoxButton.OK, MessageBoxImage.Warning);
                            qtyTextBox.Text = item.Quantity.ToString();
                        }
                    }
                };
            });
            Grid.SetColumn(qtyPanel, 1);

            // Discount input
            var discountPanel = CreateControlPanel("Remise %", discountTextBox =>
            {
                discountTextBox.Text = item.DiscountPercent.ToString("F2");
                discountTextBox.TextAlignment = TextAlignment.Center;
                discountTextBox.TextChanged += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(discountTextBox.Text))
                    {
                        item.DiscountPercent = 0;
                        discountTextBox.Text = "0";
                        totalText.Text = item.Total.ToString("F2") + " DH";
                        UpdateSummary();
                        return;
                    }

                    if (decimal.TryParse(discountTextBox.Text, out decimal newDiscount))
                    {
                        if (newDiscount >= 0 && newDiscount <= 100)
                        {
                            item.DiscountPercent = newDiscount;
                            totalText.Text = item.Total.ToString("F2") + " DH";
                            UpdateSummary();
                        }
                        else if (newDiscount > 100)
                        {
                            item.DiscountPercent = 100;
                            discountTextBox.Text = "100";
                            totalText.Text = item.Total.ToString("F2") + " DH";
                            UpdateSummary();
                        }
                        else
                        {
                            item.DiscountPercent = 0;
                            discountTextBox.Text = "0";
                            totalText.Text = item.Total.ToString("F2") + " DH";
                            UpdateSummary();
                        }
                    }
                };
            });
            Grid.SetColumn(discountPanel, 2);

            // Delete button
            var deleteButton = new Button
            {
                Content = "🗑️",
                FontSize = 18,
                Width = 42,
                Height = 42,
                Background = new SolidColorBrush(Color.FromRgb(254, 226, 226)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            deleteButton.Click += (s, e) =>
            {
                var result = MessageBox.Show($"Supprimer '{item.Article.ArticleName}' du panier?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CartItems.Remove(item);
                    LoadCartItems();
                    UpdateSummary();
                }
            };
            Grid.SetColumn(deleteButton, 5);

            controlsGrid.Children.Add(pricePanel);
            controlsGrid.Children.Add(qtyPanel);
            controlsGrid.Children.Add(discountPanel);
            controlsGrid.Children.Add(totalText);
            controlsGrid.Children.Add(deleteButton);

            mainGrid.Children.Add(infoGrid);
            mainGrid.Children.Add(controlsGrid);

            border.Child = mainGrid;
            return border;
        }

        private TextBlock CreateInfoText(string text, double fontSize, bool isItalic = false, SolidColorBrush customColor = null)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal,
                Foreground = customColor ?? (SolidColorBrush)FindResource("TextSecondary"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 2)
            };
        }

        private StackPanel CreateControlPanel(string label, Action<TextBox> configureTextBox)
        {
            var panel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0, 0, 8, 0) };

            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 10,
                Foreground = (SolidColorBrush)FindResource("TextSecondary"),
                Margin = new Thickness(0, 0, 0, 4)
            };
            panel.Children.Add(labelText);

            var textBox = new TextBox
            {
                Height = 40,
                FontSize = 13,
                TextAlignment = TextAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = Brushes.White,
                BorderBrush = (SolidColorBrush)FindResource("BorderLight"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10, 8, 10, 8)
            };

            configureTextBox(textBox);
            panel.Children.Add(textBox);

            return panel;
        }

        private void UpdateSummary()
        {
            if (CartItems == null || CartItems.Count == 0)
            {
                SubtotalText.Text = "0.00 DH";
                TotalDiscountText.Text = "0.00 DH";
                TotalTTCText.Text = "0.00 DH";
                return;
            }

            decimal subtotal = CartItems.Sum(item => item.SubTotal);
            decimal itemDiscounts = CartItems.Sum(item => item.DiscountAmount);
            decimal subtotalAfterItemDiscounts = subtotal - itemDiscounts;

            // Calculate additional discount (this includes client remise if populated in the field)
            decimal additionalDiscountAmount = 0;
            if (decimal.TryParse(AdditionalDiscountTextBox.Text, out decimal addDiscount) && addDiscount > 0)
            {
                if (DiscountTypeComboBox.SelectedIndex == 0) // Percentage
                {
                    additionalDiscountAmount = subtotalAfterItemDiscounts * (addDiscount / 100m);
                }
                else // Fixed amount
                {
                    additionalDiscountAmount = addDiscount;
                }
            }

            decimal totalDiscount = itemDiscounts + additionalDiscountAmount;
            decimal totalTTC = subtotal - totalDiscount;

            // Ensure total doesn't go negative
            if (totalTTC < 0) totalTTC = 0;

            SubtotalText.Text = subtotal.ToString("F2") + " DH";
            TotalDiscountText.Text = totalDiscount.ToString("F2") + " DH";
            TotalTTCText.Text = totalTTC.ToString("F2") + " DH";

            AdditionalDiscount = additionalDiscountAmount;
            ClientDiscount = 0; // Not needed separately since it's in additional discount
            IsPercentageDiscount = DiscountTypeComboBox.SelectedIndex == 0;
        }

        private void ClientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientComboBox.SelectedIndex == 0 || ClientComboBox.SelectedIndex == -1)
            {
                SelectedClient = null;
                ClientDiscount = 0;
                ClientInfoPanel.Visibility = Visibility.Collapsed;

                // Clear the additional discount when no client is selected
                AdditionalDiscountTextBox.Text = "0";
            }
            else
            {
                // Get the selected client name from ComboBox
                string selectedItem = ClientComboBox.SelectedItem.ToString();
                string clientName = selectedItem.Split('-')[0].Trim();

                // Find the client in the filtered or all clients list
                Client selectedClient = _allClients.FirstOrDefault(c => c.Nom == clientName);

                if (selectedClient != null)
                {
                    SelectedClient = selectedClient;

                    // Get the percentage value
                    decimal remisePercentage = SelectedClient.Remise ?? 0;

                    ClientInfoPanel.Visibility = Visibility.Visible;
                    ClientNameText.Text = SelectedClient.Nom;
                    ClientPhoneText.Text = SelectedClient.Telephone;
                    ClientDiscountValue.Text = remisePercentage.ToString("F2") + " %";

                    // Automatically populate the "Remise Supplémentaire" field with client's remise
                    AdditionalDiscountTextBox.Text = remisePercentage.ToString("F2");

                    // Make sure the discount type is set to percentage
                    DiscountTypeComboBox.SelectedIndex = 0; // 0 = Percentage
                }
            }

            UpdateSummary();
        }

        private void AdditionalDiscountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CartItems != null && CartItems.Count > 0)
            {
                UpdateSummary();
            }
        }

        private void DiscountTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Recalculate when discount type changes
            if (CartItems != null && CartItems.Count > 0)
            {
                UpdateSummary();
            }
        }

        private void CreditTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(CreditTextBox.Text, out decimal credit))
            {
                CreditAmount = credit;
            }
            else
            {
                CreditAmount = 0;
            }
        }

        private void AddArticleButton_Click(object sender, RoutedEventArgs e)
        {
            var selectWindow = new WSelectArticle(_availableArticles, _familles, _fournisseurs);
            if (selectWindow.ShowDialog() == true && selectWindow.SelectedArticle != null)
            {
                var selectedArticle = selectWindow.SelectedArticle;
                var selectedQty = selectWindow.SelectedQuantity;

                var existingItem = CartItems.FirstOrDefault(i => i.Article.ArticleID == selectedArticle.ArticleID);
                if (existingItem != null)
                {
                    if (existingItem.Quantity + selectedQty <= selectedArticle.Quantite)
                    {
                        existingItem.Quantity += selectedQty;
                    }
                    else
                    {
                        MessageBox.Show($"Quantité disponible: {selectedArticle.Quantite}",
                            "Stock Insuffisant", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    CartItems.Add(new CartItem
                    {
                        Article = selectedArticle,
                        Quantity = selectedQty,
                        DiscountPercent = 0
                    });
                }

                LoadCartItems();
                UpdateSummary();
            }
        }

        private async void NewClientButton_Click(object sender, RoutedEventArgs e)
        {
            // Use the ClientFormWindow instead of WAddCleint
            ClientFormWindow addClientWindow = new ClientFormWindow(_parentVente.main);
            bool? result = addClientWindow.ShowDialog();

            // Only reload and select if the dialog was successful
            if (result == true)
            {
                // Reload clients after adding
                await LoadClients();

                // Select the newly added client (it will be the last one in the list)
                if (ClientComboBox.Items.Count > 1)
                {
                    ClientComboBox.SelectedIndex = ClientComboBox.Items.Count - 1;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogConfirmed = false;
            this.Close();
        }

        private async void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("Le panier est vide!", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate client selection for credit operations
            if ((PaymentType == 1 || PaymentType == 2) && SelectedClient == null)
            {
                MessageBox.Show("Veuillez sélectionner un client pour une vente à crédit ou partielle.",
                    "Client requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate credit amount for partial payment
            if (PaymentType == 1)
            {
                if (string.IsNullOrWhiteSpace(CreditTextBox.Text))
                {
                    MessageBox.Show("Veuillez entrer le montant du crédit.",
                        "Crédit requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal totalTTC = decimal.Parse(TotalTTCText.Text.Replace(" DH", ""));
                if (CreditAmount > totalTTC)
                {
                    MessageBox.Show("Le montant du crédit ne peut pas dépasser le total.",
                        "Montant invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // For full credit, set credit amount to total
            if (PaymentType == 2)
            {
                decimal totalTTC = decimal.Parse(TotalTTCText.Text.Replace(" DH", ""));
                CreditAmount = totalTTC;
            }

            // Proceed with the operation
            try
            {
                await ProcessSale();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du traitement de la vente: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task ProcessSale()
        {
            // Disable button to prevent double-click
            ContinueButton.IsEnabled = false;

            try
            {
                List<TicketArticleData> ticketArticles = new List<TicketArticleData>();

                foreach (var item in CartItems)
                {
                    ticketArticles.Add(new TicketArticleData
                    {
                        ArticleName = item.Article.ArticleName,
                        Quantity = item.Quantity,
                        UnitPrice = item.PrixUnitaire,
                        Total = item.Total,
                        TVA = item.Article.tva
                    });
                }

                decimal subtotal = CartItems.Sum(item => item.SubTotal);
                decimal totalDiscount = AdditionalDiscount + ClientDiscount + CartItems.Sum(item => item.DiscountAmount);
                decimal totalTTC = subtotal - totalDiscount;

                string operationTypeLabel = PaymentType == 0 ? "VENTE COMPTANT" :
                                           PaymentType == 1 ? "VENTE PARTIELLE" : "VENTE À CRÉDIT";

                // Show ticket preview if enabled
                if (_parentVente.Ticket != null && _parentVente.Ticket.IsChecked == true)
                {
                    FactureSettings settings = await FactureSettings.LoadSettingsAsync();
                    if (settings == null) settings = new FactureSettings();

                    WFacturePreview factureWindow = new WFacturePreview(
                        settings, 0, DateTime.Now, SelectedClient,
                        ticketArticles, totalTTC, totalDiscount, CreditAmount,
                        PaymentMethodName, operationTypeLabel
                    );

                    bool? result = factureWindow.ShowDialog();
                    if (result == false || !factureWindow.ShouldPrint)
                    {
                        MessageBox.Show("Opération annulée par l'utilisateur.",
                            "Opération annulée", MessageBoxButton.OK, MessageBoxImage.Information);
                        ContinueButton.IsEnabled = true;
                        return;
                    }
                }

                // Create operation
                int operationId = await CreateOperation(totalTTC, totalDiscount);

                // Print ticket if confirmed
                if (_parentVente.Ticket != null && _parentVente.Ticket.IsChecked == true)
                {
                    await PrintTicket(operationId, ticketArticles, totalTTC, totalDiscount, operationTypeLabel);
                }

                // Clear cart
                _parentVente.SelectedArticles.Children.Clear();
                _parentVente.TotalNet.Text = "0.00 DH";
                _parentVente.ArticleCount.Text = "0";
                _parentVente.TotalNett = 0;
                _parentVente.NbrA = 0;
                _parentVente.UpdateCartEmptyState();

                // Close this window first
                DialogConfirmed = true;
                this.Close();

                // Check if we should show facture AFTER closing this window
                bool shouldShowFacture = _parentVente.Facture != null && _parentVente.Facture.IsChecked == true;

                if (shouldShowFacture)
                {
                    try
                    {
                        // Prepare invoice data
                        Dictionary<string, string> factureInfo = await PrepareFactureInfo(operationId, totalTTC, totalDiscount, operationTypeLabel);
                        List<InvoiceArticle> invoiceArticles = PrepareInvoiceArticles();

                        // Open WFacturePage with NULL for CMainFa parameter
                        WFacturePage facturePage = new WFacturePage(null, factureInfo, invoiceArticles);
                        facturePage.ShowDialog();
                    }
                    catch (Exception navEx)
                    {
                        MessageBox.Show($"Erreur lors de l'affichage de la facture: {navEx.Message}",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    // Only show congrats if facture checkbox is NOT checked
                    WCongratulations wCongratulations = new WCongratulations(
                        "Opération réussie", "Opération a été effectuée avec succès", 1);
                    wCongratulations.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ContinueButton.IsEnabled = true;
                WCongratulations wCongratulations = new WCongratulations(
                    "Opération échouée", $"Opération n'a pas été effectuée: {ex.Message}", 0);
                wCongratulations.ShowDialog();
            }
        }

        private async System.Threading.Tasks.Task<int> CreateOperation(decimal totalTTC, decimal totalDiscount)
        {
            int operationId = 0;

            Operation operation = new Operation
            {
                PaymentMethodID = PaymentMethodID,
                PrixOperation = totalTTC + totalDiscount, // Original price before discount
                Remise = totalDiscount,
                UserID = _parentVente.u.UserID,
                ClientID = SelectedClient?.ClientID
            };

            if (PaymentType == 0) // Cash
            {
                operation.OperationType = "VenteCa";
                operationId = await operation.InsertOperationAsync();
            }
            else if (PaymentType == 1) // Partial
            {
                operation.OperationType = "Vente50";
                operation.CreditValue = CreditAmount;

                int creditId = await UpdateOrCreateCredit(CreditAmount);
                operation.CreditID = creditId;

                operationId = await operation.InsertOperationAsync();
            }
            else // Full Credit
            {
                operation.OperationType = "VenteCr";
                operation.CreditValue = CreditAmount;

                int creditId = await UpdateOrCreateCredit(CreditAmount);
                operation.CreditID = creditId;

                operationId = await operation.InsertOperationAsync();
            }

            // Insert operation articles and update stock
            foreach (var item in CartItems)
            {
                OperationArticle oca = new OperationArticle
                {
                    ArticleID = item.Article.ArticleID,
                    OperationID = operationId,
                    QteArticle = item.Quantity
                };

                await oca.InsertOperationArticleAsync();

                item.Article.Quantite -= item.Quantity;
                await item.Article.UpdateArticleAsync();

                // Update local article list
                var localArticle = _parentVente.la.FirstOrDefault(a => a.ArticleID == item.Article.ArticleID);
                if (localArticle != null)
                {
                    localArticle.Quantite = item.Article.Quantite;
                }
            }

            _parentVente.LoadArticles(_parentVente.la);

            return operationId;
        }

        private async System.Threading.Tasks.Task<int> UpdateOrCreateCredit(decimal amount)
        {
            Credit creditHelper = new Credit();
            List<Credit> credits = await creditHelper.GetCreditsAsync();

            var existingCredit = credits.FirstOrDefault(c => c.ClientID == SelectedClient.ClientID);

            if (existingCredit != null)
            {
                existingCredit.Total += amount;
                await existingCredit.UpdateCreditAsync();
                return existingCredit.CreditID;
            }
            else
            {
                Credit newCredit = new Credit
                {
                    ClientID = SelectedClient.ClientID,
                    Total = amount
                };
                return await newCredit.InsertCreditAsync();
            }
        }

        private async System.Threading.Tasks.Task<Dictionary<string, string>> PrepareFactureInfo(int operationId, decimal totalTTC, decimal totalDiscount, string operationType)
        {
            decimal subtotal = CartItems.Sum(item => item.SubTotal);
            decimal itemDiscounts = CartItems.Sum(item => item.DiscountAmount);
            decimal totalHT = subtotal - itemDiscounts;

            // Calculate average TVA rate (weighted by item totals)
            decimal tvaRate = 0;
            if (CartItems.Count > 0)
            {
                decimal totalBeforeTVA = CartItems.Sum(item => item.SubTotal);
                decimal weightedTVA = CartItems.Sum(item => (item.SubTotal / totalBeforeTVA) * item.Article.tva);
                tvaRate = weightedTVA;
            }

            decimal tvaAmount = totalHT * (tvaRate / 100);
            decimal totalWithTVA = totalHT + tvaAmount;

            // Load company information from Facture
            string companyName = "";
            string companyICE = "";
            string companyVAT = "";
            string companyPhone = "";
            string companyAddress = "";
            string companyEtatJuridique = "";
            string companyId = "";
            string companySiege = "";
            string companyLogo = "";

            try
            {
                Superete.Facture facture = new Superete.Facture();
                var factureData = await facture.GetFactureAsync();
                if (factureData != null)
                {
                    companyName = factureData.Name ?? "";
                    companyICE = factureData.ICE ?? "";
                    companyVAT = factureData.VAT ?? "";
                    companyPhone = factureData.Telephone ?? "";
                    companyAddress = factureData.Adresse ?? "";
                    companyEtatJuridique = factureData.EtatJuridic ?? "";
                    companyId = factureData.CompanyId ?? "";
                    companySiege = factureData.SiegeEntreprise ?? "";
                    companyLogo = factureData.LogoPath ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des informations de l'entreprise: {ex.Message}",
                    "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            var factureInfo = new Dictionary<string, string>
            {
                // Invoice info
                ["NFacture"] = $"INV-{operationId}",
                ["Date"] = DateTime.Now.ToString("dd/MM/yyyy"),
                ["Type"] = "Facture",
                ["IndexDeFacture"] = operationId.ToString(),

                // Client info
                ["NomC"] = SelectedClient?.Nom ?? "Client Anonyme",
                ["ICEC"] = SelectedClient?.ICE ?? "",
                ["VATC"] = "", // Add if client has VAT field
                ["TelephoneC"] = SelectedClient?.Telephone ?? "",
                ["AdressC"] = SelectedClient?.Adresse ?? "",
                ["EtatJuridiqueC"] = SelectedClient?.EtatJuridique ?? "",
                ["IdSocieteC"] = SelectedClient?.Code ?? "",
                ["SiegeEntrepriseC"] = SelectedClient?.SiegeEntreprise ?? "",

                // User/Company info (from Facture table)
                ["NomU"] = companyName,
                ["ICEU"] = companyICE,
                ["VATU"] = companyVAT,
                ["TelephoneU"] = companyPhone,
                ["AdressU"] = companyAddress,
                ["EtatJuridiqueU"] = companyEtatJuridique,
                ["IdSocieteU"] = companyId,
                ["SiegeEntrepriseU"] = companySiege,

                // Payment info
                ["PaymentMethod"] = PaymentMethodName,
                ["GivenBy"] = _parentVente.u?.UserName ?? "",
                ["ReceivedBy"] = SelectedClient?.Nom ?? "Client",
                ["Device"] = "DH",

                // Financial info
                ["MontantTotal"] = totalHT.ToString("F2"),
                ["TVA"] = tvaRate.ToString("F2"),
                ["MontantTVA"] = tvaAmount.ToString("F2"),
                ["MontantApresTVA"] = totalWithTVA.ToString("F2"),
                ["Remise"] = totalDiscount.ToString("F2"),
                ["MontantApresRemise"] = totalTTC.ToString("F2"),

                // Additional info
                ["Object"] = operationType,
                ["Description"] = GetOperationDescription(),
                ["AmountInLetters"] = ConvertNumberToWords(totalTTC),
                ["Reversed"] = "Normal",
                ["EtatFature"] = "1",
                ["Logo"] = companyLogo,

                // Credit info (if applicable)
                ["CreditClientName"] = (PaymentType == 1 || PaymentType == 2) ? (SelectedClient?.Nom ?? "") : "",
                ["CreditMontant"] = (PaymentType == 1 || PaymentType == 2) ? CreditAmount.ToString("F2") : "0.00"
            };

            return factureInfo;
        }

        private List<InvoiceArticle> PrepareInvoiceArticles()
        {
            List<InvoiceArticle> invoiceArticles = new List<InvoiceArticle>();

            foreach (var item in CartItems)
            {
                invoiceArticles.Add(new InvoiceArticle
                {
                    OperationID = 0, // Will be set after operation is created
                    ArticleID = item.Article.ArticleID,
                    ArticleName = item.Article.ArticleName,
                    Prix = item.Article.PrixVente,
                    Quantite = item.Quantity,
                    TVA = item.Article.tva,
                    Reversed = false,
                    InitialQuantity = item.Quantity,
                    ExpeditionTotal = 0
                });
            }

            return invoiceArticles;
        }

        private string GetOperationDescription()
        {
            switch (PaymentType)
            {
                case 0:
                    return "Vente au comptant - Paiement intégral reçu";
                case 1:
                    return $"Vente partielle - Crédit de {CreditAmount:F2} DH";
                case 2:
                    return $"Vente à crédit - Montant total à crédit: {CreditAmount:F2} DH";
                default:
                    return "Vente";
            }
        }

        private string ConvertNumberToWords(decimal amount)
        {
            // Simple French number to words conversion
            // You can implement a more sophisticated version
            int integerPart = (int)amount;
            int decimalPart = (int)((amount - integerPart) * 100);

            string[] units = { "", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf" };
            string[] teens = { "dix", "onze", "douze", "treize", "quatorze", "quinze", "seize", "dix-sept", "dix-huit", "dix-neuf" };
            string[] tens = { "", "", "vingt", "trente", "quarante", "cinquante", "soixante", "soixante-dix", "quatre-vingt", "quatre-vingt-dix" };

            string result = "";

            if (integerPart == 0)
            {
                result = "zéro";
            }
            else if (integerPart < 10)
            {
                result = units[integerPart];
            }
            else if (integerPart < 20)
            {
                result = teens[integerPart - 10];
            }
            else if (integerPart < 100)
            {
                int tensPart = integerPart / 10;
                int unitsPart = integerPart % 10;
                result = tens[tensPart];
                if (unitsPart > 0)
                {
                    result += " " + units[unitsPart];
                }
            }
            else if (integerPart < 1000)
            {
                int hundreds = integerPart / 100;
                int remainder = integerPart % 100;

                if (hundreds == 1)
                    result = "cent";
                else
                    result = units[hundreds] + " cent";

                if (remainder > 0)
                {
                    if (remainder < 10)
                        result += " " + units[remainder];
                    else if (remainder < 20)
                        result += " " + teens[remainder - 10];
                    else
                    {
                        int t = remainder / 10;
                        int u = remainder % 10;
                        result += " " + tens[t];
                        if (u > 0)
                            result += " " + units[u];
                    }
                }
            }
            else
            {
                // For numbers >= 1000, simplify
                result = integerPart.ToString();
            }

            result += " dirhams";

            if (decimalPart > 0)
            {
                result += " et " + decimalPart + " centimes";
            }

            return result;
        }

        private async System.Threading.Tasks.Task PrintTicket(
            int operationId, List<TicketArticleData> articles,
            decimal total, decimal discount, string operationType)
        {
            try
            {
                FactureSettings settings = await FactureSettings.LoadSettingsAsync();
                if (settings == null) settings = new FactureSettings();

                WFacturePreview factureWindow = new WFacturePreview(
                    settings, operationId, DateTime.Now, SelectedClient,
                    articles, total, discount, CreditAmount,
                    PaymentMethodName, operationType
                );

                factureWindow.PrintFacture();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'impression: {ex.Message}",
                    "Erreur d'impression", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}