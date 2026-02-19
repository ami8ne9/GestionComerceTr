using Superete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GestionComerce.Main.Vente
{
    /// <summary>
    /// Logique d'interaction pour CMainV.xaml
    /// </summary>
    public partial class CMainV : UserControl
    {
        private StringBuilder quantityBuilder = new StringBuilder("1");
        private StringBuilder barcodeBuilder = new StringBuilder();
        private DateTime lastKeystroke = DateTime.Now;
        private ParametresGeneraux _parametres;
        public bool isCardLayout = false; // Track current layout mode
        public string currentSortCriteria = "Nom (A-Z)";
        private int cardsPerRow = 5; // Default for moyenne taille in Vente
        private string currentIconSize = "Moyennes";
        public CMainV(User u, List<Famille> lf, List<User> lu, List<Role> lr, MainWindow main, List<Article> la, List<Fournisseur> lfo)
        {
            InitializeComponent();

            // Set focus to the UserControl
            this.Focusable = true;
            this.Loaded += (s, e) => { this.Focus(); };

            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            CurrentUser.Text = u.UserName;
            this.u = u;
            this.lf = lf;
            this.la = la;
            this.lfo = lfo;
            this.main = main;
            FamillyContainer.Children.Clear();

            foreach (Famille famille in lf)
            {
                CSingleFamilly f = new CSingleFamilly(famille, this, lf);
                FamillyContainer.Children.Add(f);
            }

            // Charger les paramètres généraux AVANT de charger les articles et paiements
            ChargerParametres();

            LoadPayments(main.lp);

            foreach (Role r in lr)
            {
                if (u.RoleID == r.RoleID)
                {
                    if (r.Ticket == false)
                    {
                        Ticket.IsChecked = false;
                        Ticket.IsEnabled = false;
                    }
                    if (r.SolderClient == false)
                    {
                        HalfButton.IsEnabled = false;
                        CreditButton.IsEnabled = false;
                    }
                    if (r.CashClient == false)
                    {
                        CashButton.IsEnabled = false;
                    }
                    break;
                }
            }

            // Initialize empty cart state
            UpdateCartEmptyState();
        }

        public User u;
        List<Famille> lf;
        public List<Article> la;
        public MainWindow main;
        public decimal TotalNett = 0;
        public int NbrA = 0;
        public List<Fournisseur> lfo;

        /// <summary>
        /// Charge les paramètres généraux et applique les valeurs par défaut
        /// </summary>
        private void ChargerParametres()
        {
            try
            {
                // Utiliser la chaîne de connexion appropriée
                string connectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

                // Charger ou créer les paramètres pour l'utilisateur actuel
                _parametres = ParametresGeneraux.ObtenirOuCreerParametres(u.UserID, connectionString);

                // SI LES PARAMETRES VIENNENT D'ÊTRE CRÉÉS, FORCER LES BONNES VALEURS PAR DÉFAUT
                if (_parametres != null)
                {
                    bool needsUpdate = false;

                    // Forcer VueParDefaut à "Row" seulement si vide ou valeur invalide
                    if (string.IsNullOrEmpty(_parametres.VueParDefaut) ||
                        (_parametres.VueParDefaut != "Row" && _parametres.VueParDefaut != "Cartes"))
                    {
                        _parametres.VueParDefaut = "Row";
                        needsUpdate = true;
                    }

                    // Forcer TrierParDefaut à "Plus récent au plus ancien" SEULEMENT si vide
                    if (string.IsNullOrEmpty(_parametres.TrierParDefaut))
                    {
                        _parametres.TrierParDefaut = "Plus récent au plus ancien";
                        needsUpdate = true;
                    }

                    // Si on a modifié quelque chose, sauvegarder
                    if (needsUpdate)
                    {
                        _parametres.MettreAJourParametres(connectionString);
                    }
                }

                // Appliquer les paramètres
                AppliquerParametres();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des paramètres : {ex.Message}",
                    "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Utiliser des valeurs par défaut en cas d'erreur
                _parametres = null;
            }
        }

        /// <summary>
        /// Applique les paramètres chargés à l'interface
        /// </summary>
        private void AppliquerParametres()
        {
            if (_parametres == null) return;

            try
            {
                // 1. Appliquer la Vue par défaut (Cartes ou Row)
                // FORCE DEFAULT TO ROW IF EMPTY OR INVALID
                string vueParDefaut = string.IsNullOrEmpty(_parametres.VueParDefaut) ? "Row" : _parametres.VueParDefaut;

                if (vueParDefaut == "Cartes")
                {
                    isCardLayout = true;
                    if (CardLayoutButton != null && RowLayoutButton != null)
                    {
                        CardLayoutButton.Style = (Style)FindResource("ActiveToggleButtonStyle");
                        RowLayoutButton.Style = (Style)FindResource("ToggleButtonStyle");
                    }
                    if (TableHeader != null)
                    {
                        TableHeader.Visibility = Visibility.Collapsed;
                    }
                    // Show IconSizeComboBox for card layout
                    if (IconSizeComboBox != null)
                    {
                        IconSizeComboBox.Visibility = Visibility.Visible;
                    }
                }
                else // "Row"
                {
                    isCardLayout = false;
                    if (CardLayoutButton != null && RowLayoutButton != null)
                    {
                        RowLayoutButton.Style = (Style)FindResource("ActiveToggleButtonStyle");
                        CardLayoutButton.Style = (Style)FindResource("ToggleButtonStyle");
                    }
                    if (TableHeader != null)
                    {
                        TableHeader.Visibility = Visibility.Visible;
                    }
                    // Hide IconSizeComboBox for row layout
                    if (IconSizeComboBox != null)
                    {
                        IconSizeComboBox.Visibility = Visibility.Collapsed;
                    }
                }

                // 2. Appliquer le Tri par défaut
                // FORCE DEFAULT TO "Plus récent au plus ancien" IF EMPTY
                string trierParDefaut = string.IsNullOrEmpty(_parametres.TrierParDefaut) ? "Plus récent au plus ancien" : _parametres.TrierParDefaut;
                currentSortCriteria = trierParDefaut;

                if (SortComboBox != null && SortComboBox.Items.Count > 0)
                {
                    for (int i = 0; i < SortComboBox.Items.Count; i++)
                    {
                        if (SortComboBox.Items[i] is ComboBoxItem item)
                        {
                            string itemContent = item.Content.ToString();
                            if (itemContent == trierParDefaut)
                            {
                                SortComboBox.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }

                // 3. Appliquer la Taille des Icônes
                if (!string.IsNullOrEmpty(_parametres.TailleIcones))
                {
                    currentIconSize = _parametres.TailleIcones;

                    if (IconSizeComboBox != null && IconSizeComboBox.Items.Count > 0)
                    {
                        switch (_parametres.TailleIcones)
                        {
                            case "Grandes":
                                IconSizeComboBox.SelectedIndex = 0;
                                break;
                            case "Moyennes":
                                IconSizeComboBox.SelectedIndex = 1;
                                break;
                            case "Petites":
                                IconSizeComboBox.SelectedIndex = 2;
                                break;
                            default:
                                IconSizeComboBox.SelectedIndex = 1;
                                break;
                        }
                    }
                }

                // 4. Charger les articles avec les paramètres appliqués
                LoadArticles(la);

                // 5. Appliquer le paramètre d'impression de facture
                if (Facture != null)
                {
                    Facture.IsChecked = _parametres.ImprimerFactureParDefaut;
                }

                // 6. Appliquer le paramètre d'impression de ticket
                if (Ticket != null && Ticket.IsEnabled)
                {
                    Ticket.IsChecked = _parametres.ImprimerTicketParDefaut;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'application des paramètres : {ex.Message}",
                    "Avertissement", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        // Method to update the empty cart state visibility
        private void IconSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IconSizeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedSize = selectedItem.Content.ToString();

                // Mettre à jour currentIconSize
                if (selectedSize.Contains("Grandes"))
                {
                    currentIconSize = "Grandes";
                }
                else if (selectedSize.Contains("Moyennes"))
                {
                    currentIconSize = "Moyennes";
                }
                else if (selectedSize.Contains("Petites"))
                {
                    currentIconSize = "Petites";
                }

                // Recharger l'affichage si on a des articles
                if (la != null && la.Count > 0)
                {
                    LoadArticles(la);
                }
            }
        }

        public void UpdateCartEmptyState()
        {
            if (EmptyStateCart != null)
            {
                // Count actual article items (not the empty state itself)
                int itemCount = 0;
                foreach (UIElement element in SelectedArticles.Children)
                {
                    if (element is CSingleArticle2)
                    {
                        itemCount++;
                    }
                }

                // Show empty state only when cart is empty
                EmptyStateCart.Visibility = itemCount == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public List<Article> ApplySorting(List<Article> articles)
        {
            switch (currentSortCriteria)
            {
                case "Nom (A-Z)":
                    return articles.OrderBy(a => a.ArticleName).ToList();

                case "Nom (Z-A)":
                    return articles.OrderByDescending(a => a.ArticleName).ToList();

                case "Prix croissant":
                    return articles.OrderBy(a => a.PrixVente).ToList();

                case "Prix décroissant":
                    return articles.OrderByDescending(a => a.PrixVente).ToList();

                case "Quantité croissante":
                    return articles.OrderBy(a => a.Quantite).ToList();

                case "Quantité décroissante":
                    return articles.OrderByDescending(a => a.Quantite).ToList();

                case "Plus récent au plus ancien":
                    return articles.OrderByDescending(a => a.Date ?? DateTime.MinValue).ToList();

                case "Plus ancien au plus récent":
                    return articles.OrderBy(a => a.Date ?? DateTime.MaxValue).ToList();

                default:
                    return articles;
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - lastKeystroke;

            if (elapsed.TotalMilliseconds > 100)
            {
                barcodeBuilder.Clear();
            }

            lastKeystroke = DateTime.Now;

            if (e.Key == Key.Return)
            {
                string barcode = barcodeBuilder.ToString();
                OnBarcodeScanned(barcode);
                barcodeBuilder.Clear();
                e.Handled = true;
            }
            else
            {
                // Convert key to character
                string key = new KeyConverter().ConvertToString(e.Key);
                if (!string.IsNullOrEmpty(key) && key.Length == 1)
                {
                    barcodeBuilder.Append(key);
                }
            }
        }

        private void OnBarcodeScanned(string barcode)
        {
            // Search for the article with this barcode
            Article foundArticle = la.FirstOrDefault(a => a.Code.ToString() == barcode);

            if (foundArticle != null)
            {
                // Check if article is in stock
                if (foundArticle.Quantite <= 0)
                {
                    MessageBox.Show($"L'article '{foundArticle.ArticleName}' est en rupture de stock.");
                    return;
                }

                // Display the article in the SelectedArticle container (top preview)
                CSingleArticle1 previewArticle = new CSingleArticle1(foundArticle, this, lf, lfo, 0);
                SelectedArticle.Child = previewArticle;

                // Set the quantity to 1 for the scanned article
                ArticleQuantity.Text = "1";

                // Check if article already exists in cart
                foreach (UIElement element in SelectedArticles.Children)
                {
                    if (element is CSingleArticle2 item)
                    {
                        if (item.a.ArticleID == foundArticle.ArticleID)
                        {
                            if (item.qte + 1 > foundArticle.Quantite)
                            {
                                MessageBox.Show("La quantité en stock est insuffisante.");
                            }
                            else
                            {
                                item.Quantite.Text = (item.qte + 1).ToString();
                                item.qte += 1;
                                TotalNett += foundArticle.PrixVente;
                                TotalNet.Text = TotalNett.ToString("F2") + " DH";
                                NbrA += 1;
                                ArticleCount.Text = NbrA.ToString();
                            }
                            return;
                        }
                    }
                }

                // Add new article to cart
                TotalNett += foundArticle.PrixVente;
                TotalNet.Text = TotalNett.ToString("F2") + " DH";
                NbrA += 1;
                ArticleCount.Text = NbrA.ToString();
                CSingleArticle2 sa = new CSingleArticle2(foundArticle, 1, this);
                SelectedArticles.Children.Add(sa);
                UpdateCartEmptyState();
            }
            else
            {
                MessageBox.Show($"Aucun article trouvé avec le code: {barcode}");
            }
        }

        public void LoadArticles(List<Article> la)
        {
            var sortedArticles = ApplySorting(la);
            ArticlesContainer.Children.Clear();

            if (isCardLayout)
            {
                // Determine cards per row based on size
                int cardsPerRow = 5;
                switch (currentIconSize)
                {
                    case "Grandes":
                        cardsPerRow = 4;
                        break;
                    case "Moyennes":
                        cardsPerRow = 5;
                        break;
                    case "Petites":
                        cardsPerRow = 5;
                        break;
                }

                // Create Grid for proper layout
                var grid = new Grid();

                // Calculate rows needed
                int totalRows = (int)Math.Ceiling((double)sortedArticles.Count / cardsPerRow);

                // Add row definitions
                for (int i = 0; i < totalRows; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                // Add column definitions
                for (int i = 0; i < cardsPerRow; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // Add articles to grid
                int articleIndex = 0;
                foreach (Article a in sortedArticles)
                {
                    int row = articleIndex / cardsPerRow;
                    int col = articleIndex % cardsPerRow;

                    CSingleArticle1 ar = new CSingleArticle1(a, this, lf, lfo, 2, currentIconSize);

                    // Appliquer les marges appropriées selon la taille

                    // Appliquer les marges et alignement appropriés selon la taille
                    switch (currentIconSize)
                    {
                        case "Grandes":
                            ar.Margin = new Thickness(0, 0, 16, 16);
                            ar.Width = 280; // Fixed width for large icons
                            ar.HorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case "Moyennes":
                            ar.Margin = new Thickness(0, 0, 14, 14);
                            ar.HorizontalAlignment = HorizontalAlignment.Stretch;
                            break;
                        case "Petites":
                            ar.Margin = new Thickness(0, 0, 12, 12);
                            ar.HorizontalAlignment = HorizontalAlignment.Stretch;
                            break;
                        default:
                            ar.Margin = new Thickness(0, 0, 14, 14);
                            ar.HorizontalAlignment = HorizontalAlignment.Stretch;
                            break;
                    }

                    ar.VerticalAlignment = VerticalAlignment.Top;

                    Grid.SetRow(ar, row);
                    Grid.SetColumn(ar, col);

                    grid.Children.Add(ar);
                    articleIndex++;
                }

                ArticlesContainer.Children.Add(grid);
            }
            else
            {
                // Row layout (table mode)
                foreach (Article a in sortedArticles)
                {
                    CSingleArticle1 ar = new CSingleArticle1(a, this, lf, lfo, 0); // 0 = row mode
                    ArticlesContainer.Children.Add(ar);
                }
            }
        }
        private List<WPreCheckout.CartItem> ExtractCartItems()
        {
            var cartItems = new List<WPreCheckout.CartItem>();

            foreach (UIElement element in SelectedArticles.Children)
            {
                if (element is CSingleArticle2 item)
                {
                    cartItems.Add(new WPreCheckout.CartItem
                    {
                        Article = item.a,
                        Quantity = item.qte,
                        DiscountPercent = 0 // Default, user can edit in pre-checkout
                    });
                }
            }

            return cartItems;
        }
        private void UpdateCartFromPreCheckout(List<WPreCheckout.CartItem> updatedItems, decimal additionalDiscount)
        {
            // Clear current cart
            SelectedArticles.Children.Clear();
            TotalNett = 0;
            NbrA = 0;

            // Re-add items with updated quantities and discounts
            foreach (var item in updatedItems)
            {
                decimal itemTotal = item.Total; // Already includes item-level discount
                TotalNett += itemTotal;
                NbrA += item.Quantity;

                CSingleArticle2 sa = new CSingleArticle2(item.Article, item.Quantity, this);
                SelectedArticles.Children.Add(sa);
            }

            // Apply additional discount if any
            if (additionalDiscount > 0)
            {
                TotalNett -= additionalDiscount;
            }

            // Update UI
            TotalNet.Text = TotalNett.ToString("F2") + " DH";
            ArticleCount.Text = NbrA.ToString();
            UpdateCartEmptyState();
        }
        private void UpdateCardSizes()
        {
            if (!isCardLayout) return;

            // Get the available width (you'll need to measure the actual container)
            double availableWidth = ArticlesContainer.ActualWidth;
            if (availableWidth == 0) return;

            int cardsPerRow = currentIconSize == "Grandes" ? 4 : currentIconSize == "Moyennes" ? 5 : 7;
            double spacing = currentIconSize == "Grandes" ? 16 : currentIconSize == "Moyennes" ? 14 : 12;

            // Calculate card width to fill the space
            double cardWidth = (availableWidth - (spacing * (cardsPerRow - 1))) / cardsPerRow;

            // Update all cards
            foreach (var row in ArticlesContainer.Children)
            {
                if (row is WrapPanel panel)
                {
                    foreach (var child in panel.Children)
                    {
                        if (child is CSingleArticle1 card)
                        {
                            card.Width = cardWidth;
                        }
                    }
                }
            }
        }
        private void LayoutToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton == RowLayoutButton)
            {
                isCardLayout = false;
                RowLayoutButton.Style = (Style)FindResource("ActiveToggleButtonStyle");
                CardLayoutButton.Style = (Style)FindResource("ToggleButtonStyle");
                TableHeader.Visibility = Visibility.Visible;
                IconSizeComboBox.Visibility = Visibility.Collapsed; // Hide icon size selector
            }
            else if (clickedButton == CardLayoutButton)
            {
                isCardLayout = true;
                CardLayoutButton.Style = (Style)FindResource("ActiveToggleButtonStyle");
                RowLayoutButton.Style = (Style)FindResource("ToggleButtonStyle");
                TableHeader.Visibility = Visibility.Collapsed;
                IconSizeComboBox.Visibility = Visibility.Visible; // Show icon size selector
            }

            LoadArticles(la);
        }

        // Add sorting handler
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                currentSortCriteria = selectedItem.Content.ToString();

                if (la != null && la.Count > 0)
                {
                    LoadArticles(la);
                }
            }
        }

        public void LoadPayments(List<PaymentMethod> lp)
        {
            PaymentMethodComboBox.Items.Clear();
            foreach (PaymentMethod a in lp)
            {
                PaymentMethodComboBox.Items.Add(a.PaymentMethodName);
            }

            // Appliquer la méthode de paiement par défaut après le chargement
            // Utiliser Dispatcher pour s'assurer que le ComboBox est complètement rendu
            Dispatcher.BeginInvoke(new Action(() =>
            {
                AppliquerMethodePaiementParDefaut();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Applique la méthode de paiement par défaut depuis les paramètres
        /// </summary>
        private void AppliquerMethodePaiementParDefaut()
        {
            if (PaymentMethodComboBox == null || PaymentMethodComboBox.Items.Count == 0)
                return;

            try
            {
                // Si les paramètres existent et qu'une méthode par défaut est définie
                if (_parametres != null && !string.IsNullOrEmpty(_parametres.MethodePaiementParDefaut))
                {
                    string methodeCherchee = _parametres.MethodePaiementParDefaut.Trim();

                    // Rechercher et sélectionner la méthode de paiement
                    for (int i = 0; i < PaymentMethodComboBox.Items.Count; i++)
                    {
                        string itemText = PaymentMethodComboBox.Items[i].ToString().Trim();

                        // Comparaison insensible à la casse ET aux accents
                        if (RemoveAccents(itemText).Equals(RemoveAccents(methodeCherchee), StringComparison.OrdinalIgnoreCase))
                        {
                            PaymentMethodComboBox.SelectedIndex = i;
                            return;
                        }
                    }
                }

                // Si aucun paramètre ou aucune correspondance, sélectionner le premier élément
                if (PaymentMethodComboBox.Items.Count > 0)
                {
                    PaymentMethodComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                // En cas d'erreur, sélectionner simplement le premier élément
                if (PaymentMethodComboBox.Items.Count > 0)
                {
                    PaymentMethodComboBox.SelectedIndex = 0;
                }
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'application de la méthode de paiement: {ex.Message}");
            }
        }

        /// <summary>
        /// Supprime les accents d'une chaîne pour faciliter les comparaisons
        /// </summary>
        private string RemoveAccents(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            main.load_main(u);
        }

        private void MyBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WKeyPad wKeyPad = new WKeyPad(this);
            wKeyPad.ShowDialog();
        }

        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedArticle.Child is CSingleArticle1 sa1)
            {
                if (ArticleQuantity.Text == "")
                {
                    MessageBox.Show("Veuillez insérer une quantité");
                    return;
                }
                else if (Convert.ToInt32(ArticleQuantity.Text) == 0)
                {
                    MessageBox.Show("Veuillez insérer une quantité");
                    return;
                }
                else if (Convert.ToInt32(ArticleQuantity.Text) > sa1.a.Quantite)
                {
                    MessageBox.Show("La quantité insérée est plus grande que la quantité en stock");
                }
                else
                {
                    foreach (UIElement element in SelectedArticles.Children)
                    {
                        if (element is CSingleArticle2 item)
                        {
                            if (item.a.ArticleID == sa1.a.ArticleID)
                            {
                                if (Convert.ToInt32(ArticleQuantity.Text) + Convert.ToInt32(item.Quantite.Text) > sa1.a.Quantite)
                                {
                                    MessageBox.Show("La quantité dans le panier plus la quantité que vous voulez ajouter est plus grande que la quantité en stock");
                                }
                                else
                                {
                                    item.Quantite.Text = (Convert.ToInt32(ArticleQuantity.Text) + item.qte).ToString();
                                    item.qte += Convert.ToInt32(ArticleQuantity.Text);
                                    TotalNett += sa1.a.PrixVente * Convert.ToInt32(ArticleQuantity.Text);
                                    TotalNet.Text = TotalNett.ToString("F2") + " DH";
                                    NbrA += Convert.ToInt32(ArticleQuantity.Text);
                                    ArticleCount.Text = NbrA.ToString();
                                }
                                return;
                            }
                        }
                    }

                    TotalNett += sa1.a.PrixVente * Convert.ToInt32(ArticleQuantity.Text);
                    TotalNet.Text = TotalNett.ToString("F2") + " DH";
                    NbrA += Convert.ToInt32(ArticleQuantity.Text);
                    ArticleCount.Text = NbrA.ToString();
                    CSingleArticle2 sa = new CSingleArticle2(sa1.a, Convert.ToInt32(ArticleQuantity.Text), this);
                    SelectedArticles.Children.Add(sa);
                    UpdateCartEmptyState();
                    SelectedArticle.Child = new TextBlock
                    {
                        Name = "DesignationText",
                        Text = "Aucun produit sélectionné",
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 13,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#78350F")),
                        TextWrapping = TextWrapping.Wrap
                    };
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un article.");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedArticles.Children.Clear();
            TotalNett = 0;
            NbrA = 0;
            TotalNet.Text = "0.00 DH";
            ArticleCount.Text = "0";
            UpdateCartEmptyState();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ArticlesContainer.Children.Clear();
            foreach (Article a in la)
            {
                CSingleArticle1 ar = new CSingleArticle1(a, this, lf, lfo, 0);
                ArticlesContainer.Children.Add(ar);
            }
        }

        private void CodeBarreTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ArticlesContainer.Children.Clear();
            string code = sender is TextBox tb ? tb.Text : "";
            foreach (Article a in la)
            {
                if (a.Code.ToString().Contains(code) || a.ArticleName.ToLower().Contains(code.ToLower()))
                {
                    CSingleArticle1 ar = new CSingleArticle1(a, this, lf, lfo, 0);
                    ArticlesContainer.Children.Add(ar);
                }
            }
        }

        // Update these three methods in CMainV.cs:

        // Update these three methods in CMainV.cs:

        private async void CashButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedArticles.Children.Count == 0 || NbrA == 0)
            {
                MessageBox.Show("Aucun article sélectionné.");
                return;
            }

            if (PaymentMethodComboBox.Text == "")
            {
                MessageBox.Show("Veuillez sélectionner un mode de paiement, si il n'y a aucune méthode de paiement, ajoutez-la depuis les paramètres");
                return;
            }

            // Find Payment Method ID
            int MethodID = 0;
            foreach (PaymentMethod p in main.lp)
            {
                if (p.PaymentMethodName == PaymentMethodComboBox.SelectedValue.ToString())
                {
                    MethodID = p.PaymentMethodID;
                    break;
                }
            }

            // Extract cart items
            var cartItems = ExtractCartItems();

            // Open Pre-Checkout Window with payment type 0 (Cash)
            WPreCheckout preCheckout = new WPreCheckout(
                this,
                cartItems,
                PaymentMethodComboBox.SelectedValue.ToString(),
                MethodID,
                0, // 0 = Cash payment
                la,
                lf,
                lfo
            );

            preCheckout.ShowDialog();

            // Clean up after sale if confirmed
            if (preCheckout.DialogConfirmed)
            {
                SupprimerArticlesQuantiteZeroSiActive();
                UpdateCartEmptyState();
            }
        }

        private async void HalfButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedArticles.Children.Count == 0 || NbrA == 0)
            {
                MessageBox.Show("Aucun article sélectionné.");
                return;
            }

            if (PaymentMethodComboBox.Text == "")
            {
                MessageBox.Show("Veuillez sélectionner un mode de paiement, si il n'y a aucune méthode de paiement, ajoutez-la depuis les paramètres");
                return;
            }

            int MethodID = 0;
            foreach (PaymentMethod p in main.lp)
            {
                if (p.PaymentMethodName == PaymentMethodComboBox.SelectedValue.ToString())
                {
                    MethodID = p.PaymentMethodID;
                    break;
                }
            }

            var cartItems = ExtractCartItems();

            // Open Pre-Checkout Window with payment type 1 (Partial)
            WPreCheckout preCheckout = new WPreCheckout(
                this,
                cartItems,
                PaymentMethodComboBox.SelectedValue.ToString(),
                MethodID,
                1, // 1 = 50/50 payment
                la,
                lf,
                lfo
            );

            preCheckout.ShowDialog();

            // Clean up after sale if confirmed
            if (preCheckout.DialogConfirmed)
            {
                SupprimerArticlesQuantiteZeroSiActive();
                UpdateCartEmptyState();
            }
        }

        private async void CreditButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedArticles.Children.Count == 0 || NbrA == 0)
            {
                MessageBox.Show("Aucun article sélectionné.");
                return;
            }

            if (PaymentMethodComboBox.Text == "")
            {
                MessageBox.Show("Veuillez sélectionner un mode de paiement, si il n'y a aucune méthode de paiement, ajoutez-la depuis les paramètres");
                return;
            }

            int MethodID = 0;
            foreach (PaymentMethod p in main.lp)
            {
                if (p.PaymentMethodName == PaymentMethodComboBox.SelectedValue.ToString())
                {
                    MethodID = p.PaymentMethodID;
                    break;
                }
            }

            var cartItems = ExtractCartItems();

            // Open Pre-Checkout Window with payment type 2 (Credit)
            WPreCheckout preCheckout = new WPreCheckout(
                this,
                cartItems,
                PaymentMethodComboBox.SelectedValue.ToString(),
                MethodID,
                2, // 2 = Credit payment
                la,
                lf,
                lfo
            );

            preCheckout.ShowDialog();

            // Clean up after sale if confirmed
            if (preCheckout.DialogConfirmed)
            {
                SupprimerArticlesQuantiteZeroSiActive();
                UpdateCartEmptyState();
            }
        }

        // You can also REMOVE the UpdateCartFromPreCheckout method as it's no longer needed
        // You can also REMOVE the UpdateCartFromPreCheckout method as it's no longer needed
        /// <summary>
        /// Supprime automatiquement les articles avec quantité 0 si le paramètre est activé
        /// Cette méthode doit être appelée après une vente complétée
        /// </summary>
        private async void SupprimerArticlesQuantiteZeroSiActive()
        {
            if (_parametres == null || !_parametres.SupprimerArticlesQuantiteZero)
                return;

            try
            {
                // Trouver tous les articles avec quantité 0
                List<Article> articlesASupprimer = new List<Article>();

                foreach (Article article in la.ToList()) // ToList() pour éviter la modification pendant l'itération
                {
                    if (article.Quantite == 0)
                    {
                        articlesASupprimer.Add(article);
                    }
                }

                if (articlesASupprimer.Count > 0)
                {
                    // Désactiver automatiquement sans demander confirmation
                    int desactivationReussie = 0;

                    foreach (Article article in articlesASupprimer)
                    {
                        try
                        {
                            // Désactiver l'article (met Etat = 0)
                            int deleted = await article.DeleteArticleAsync();

                            if (deleted == 1)
                            {
                                // Retirer l'article de la liste locale
                                la.Remove(article);
                                desactivationReussie++;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log l'erreur mais continue avec les autres articles
                            System.Diagnostics.Debug.WriteLine($"Erreur lors de la désactivation de l'article {article.ArticleName}: {ex.Message}");
                        }
                    }

                    if (desactivationReussie > 0)
                    {
                        // Recharger l'affichage des articles
                        LoadArticles(la);

                        // Log silencieux pour le débogage
                        System.Diagnostics.Debug.WriteLine($"{desactivationReussie} article(s) désactivé(s) automatiquement.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log silencieux de l'erreur
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la désactivation automatique des articles : {ex.Message}");
            }
        }
    }
}