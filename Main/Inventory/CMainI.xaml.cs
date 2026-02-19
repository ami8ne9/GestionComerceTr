using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Superete;

namespace GestionComerce.Main.Inventory
{
    /// <summary>
    /// Logique d'interaction pour CMainI.xaml
    /// </summary>
    public partial class CMainI : UserControl
    {
        public List<Famille> lf;
        public MainWindow main;
        public User u;
        public List<Fournisseur> lfo;
        public List<Article> la;
        private int cardsPerRow = 7; // Default for moyenne taille
        private string currentIconSize = "Moyennes";
        private int articlesPerPage = 12;
        private int currentlyLoadedCount = 0;
        private List<Article> filteredArticles;
        private bool isCardLayout = false; // Track current layout mode
        private string currentSortCriteria = "Plus récent au plus ancien"; // Default sort
        private ParametresGeneraux _parametres;

        public CMainI(User u, List<Article> la, List<Famille> lf, List<Fournisseur> lfo, MainWindow main)
        {
            InitializeComponent();
            this.lf = lf;
            this.main = main;
            this.u = u;
            this.la = la;
            this.lfo = lfo;
            this.filteredArticles = new List<Article>();

            // Charger les paramètres utilisateur
            ChargerParametres();

            foreach (Role r in main.lr)
            {
                if (u.RoleID == r.RoleID)
                {
                    if (r.ViewFamilly == false && r.AddFamilly == false)
                    {
                        ManageFamillies.IsEnabled = false;
                    }
                    if (r.AddArticle == false)
                    {
                        NewArticleButton.IsEnabled = false;
                        AddMultipleArticlesButton.IsEnabled = false;
                    }
                    if (r.ViewFournisseur == false)
                    {
                        FournisseurManage.IsEnabled = false;
                    }
                    if (r.ViewArticle == true)
                    {
                        RefreshArticlesList(la, true);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Charge les paramètres généraux de l'utilisateur
        /// </summary>
        private void ChargerParametres()
        {
            try
            {
                string connectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";
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
                _parametres = null;
            }
        }
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
                                cardsPerRow = 6;
                                articlesPerPage = 12;
                                break;
                            case "Moyennes":
                                IconSizeComboBox.SelectedIndex = 1;
                                cardsPerRow = 8;
                                articlesPerPage = 14;
                                break;
                            case "Petites":
                                IconSizeComboBox.SelectedIndex = 2;
                                cardsPerRow = 12;
                                articlesPerPage = 22;
                                break;
                            default:
                                IconSizeComboBox.SelectedIndex = 1;
                                cardsPerRow = 8;
                                articlesPerPage = 14;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'application des paramètres : {ex.Message}");
            }
        }
        // REPLACE your existing IconSizeComboBox_SelectionChanged method with this:

        private void IconSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IconSizeComboBox == null || IconSizeComboBox.SelectedIndex < 0)
                return;

            int selectedIndex = IconSizeComboBox.SelectedIndex;

            // Update based on index (0 = Large, 1 = Medium, 2 = Small)
            switch (selectedIndex)
            {
                case 0: // Grandes (Large - 3 big dots)
                    currentIconSize = "Grandes";
                    cardsPerRow = 6;
                    articlesPerPage = 12;
                    break;

                case 1: // Moyennes (Medium - 3 medium dots) 
                    currentIconSize = "Moyennes";
                    cardsPerRow = 7;
                    articlesPerPage = 14;
                    break;

                case 2: // Petites (Small - 3 small dots)
                    currentIconSize = "Petites";
                    cardsPerRow = 11;
                    articlesPerPage = 22;
                    break;

                default:
                    currentIconSize = "Moyennes";
                    cardsPerRow = 7;
                    articlesPerPage = 14;
                    break;
            }

            // Save preference to database if _parametres is available
            if (_parametres != null)
            {
                try
                {
                    _parametres.TailleIcones = currentIconSize;
                    string connectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";
                    _parametres.MettreAJourParametres(connectionString);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde de la taille des icônes : {ex.Message}");
                }
            }

            // Recharger l'affichage si on a des articles ET qu'on est en mode carte
            if (la != null && la.Count > 0 && isCardLayout)
            {
                RefreshArticlesList(la, false);
            }
        }

        // Main public method - called from other windows
        public void LoadArticles(List<Article> la)
        {
            // SIMPLE FIX: Just refresh with the provided list
            this.la = la;
            RefreshArticlesList(la, false);
        }

        // Internal refresh method
        private void RefreshArticlesList(List<Article> la, bool resetPagination)
        {
            foreach (Role r in main.lr)
            {
                if (u.RoleID == r.RoleID)
                {
                    if (r.ViewArticle == true)
                    {
                        this.la = la;

                        // Apply sorting to the list
                        var sortedList = ApplySorting(la);

                        filteredArticles = new List<Article>();

                        foreach (Article a in sortedList)
                        {
                            if (a.Etat)
                            {
                                filteredArticles.Add(a);
                            }
                        }

                        int previousCount = resetPagination ? 0 : currentlyLoadedCount;
                        ArticlesContainer.Children.Clear();
                        UpdateTotalStats();

                        int articlesToLoad;
                        if (resetPagination || previousCount == 0)
                        {
                            articlesToLoad = Math.Min(articlesPerPage, filteredArticles.Count);
                        }
                        else
                        {
                            articlesToLoad = Math.Min(previousCount, filteredArticles.Count);
                        }

                        currentlyLoadedCount = articlesToLoad;
                        RefreshCurrentView();
                    }
                    break;
                }
            }
        }

        // Apply sorting based on current sort criteria
        private List<Article> ApplySorting(List<Article> articles)
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

        private void LoadMoreArticles()
        {
            int articlesToLoad = Math.Min(articlesPerPage, filteredArticles.Count - currentlyLoadedCount);
            currentlyLoadedCount += articlesToLoad;
            RefreshCurrentView();
        }

        private void UpdateViewMoreButtonVisibility()
        {
            if (ViewMoreButton != null)
            {
                ViewMoreButton.Visibility = (currentlyLoadedCount < filteredArticles.Count)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
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

        // Sorting ComboBox selection changed
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                currentSortCriteria = selectedItem.Content.ToString();

                // Refresh the list with new sorting
                if (la != null && la.Count > 0)
                {
                    RefreshArticlesList(la, false);
                }
            }
        }

        private void RefreshCurrentView()
        {
            ArticlesContainer.Children.Clear();

            if (isCardLayout)
            {
                // Determine cards per row based on size
                int cardsPerRow = 0;
                switch (currentIconSize)
                {
                    case "Grandes":
                        cardsPerRow = 6;
                        break;
                    case "Moyennes":
                        cardsPerRow = 7;
                        break;
                    case "Petites":
                        cardsPerRow = 11;
                        break;
                }

                // Create Grid for proper layout
                var grid = new Grid();

                // Calculate rows needed
                int articlesToShow = Math.Min(currentlyLoadedCount, filteredArticles.Count);
                int totalRows = (int)Math.Ceiling((double)articlesToShow / cardsPerRow);

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
                for (int i = 0; i < articlesToShow; i++)
                {
                    Article a = filteredArticles[i];
                    int row = i / cardsPerRow;
                    int col = i % cardsPerRow;

                    CSingleArticleI ar = new CSingleArticleI(a, la, this, lf, lfo, true, currentIconSize);
                    ar.HorizontalAlignment = HorizontalAlignment.Stretch;
                    ar.VerticalAlignment = VerticalAlignment.Top;
                    ar.MaxWidth = currentIconSize == "Petites" ? 140 : (currentIconSize == "Grandes" ? 300 : 270);
                    ar.HorizontalAlignment = HorizontalAlignment.Center;

                    Grid.SetRow(ar, row);
                    Grid.SetColumn(ar, col);

                    grid.Children.Add(ar);
                }

                ArticlesContainer.Children.Add(grid);
            }
            else
            {
                // Row layout - direct children
                int articlesToShow = Math.Min(currentlyLoadedCount, filteredArticles.Count);

                for (int i = 0; i < articlesToShow; i++)
                {
                    Article a = filteredArticles[i];
                    CSingleArticleI ar = new CSingleArticleI(a, la, this, lf, lfo, false);
                    ArticlesContainer.Children.Add(ar);
                }
            }

            UpdateViewMoreButtonVisibility();
        }

        private void UpdateTotalStats()
        {
            // Count all articles in the main list with Etat == true
            List<Article> allActiveArticles = new List<Article>();
            foreach (Article a in la)
            {
                if (a.Etat)
                {
                    allActiveArticles.Add(a);
                }
            }

            int count = allActiveArticles.Count;
            Decimal PrixATotall = 0;
            Decimal PrixMPTotall = 0;
            Decimal PrixVTotall = 0;
            int QuantiteTotall = 0;

            ArticlesTotal.Text = count.ToString();

            foreach (Article a in allActiveArticles)
            {
                PrixATotall += a.PrixAchat * a.Quantite;
                PrixMPTotall += a.PrixMP * a.Quantite;
                PrixVTotall += a.PrixVente * a.Quantite;
                QuantiteTotall += a.Quantite;
            }

            PrixATotal.Text = PrixATotall.ToString("0.00") + " DH";
            PrixMPTotal.Text = PrixMPTotall.ToString("0.00") + " DH";
            PrixVTotal.Text = PrixVTotall.ToString("0.00") + " DH";
            QuantiteTotal.Text = QuantiteTotall.ToString();
        }

        // View More Button Click
        private void ViewMoreButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMoreArticles();
        }

        // Back button in header
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            main.load_main(u);
        }

        // ComboBox for search criteria selection
        private void SearchCriteriaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Reapply filter when criteria changes
            if (SearchTextBox != null && ArticlesContainer != null)
            {
                ApplySearchFilter();
            }
        }

        // TextBox for search text
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        // Search button (loupe)
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySearchFilter();
        }

        // Clear button
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            // Filter will be applied automatically through TextChanged event
        }

        // Main search filter method
        private void ApplySearchFilter()
        {
            string searchText = SearchTextBox.Text.Trim();

            // If search is empty, show all articles with pagination (reset pagination on search clear)
            if (string.IsNullOrEmpty(searchText))
            {
                filteredArticles = new List<Article>();
                foreach (Article a in la)
                {
                    if (a.Etat)
                    {
                        filteredArticles.Add(a);
                    }
                }

                // Apply sorting to filtered results
                filteredArticles = ApplySorting(filteredArticles);

                currentlyLoadedCount = 0;
                ArticlesContainer.Children.Clear();
                LoadMoreArticles();
                return;
            }

            // Get selected search criteria
            var selectedItem = SearchCriteriaComboBox.SelectedItem as ComboBoxItem;
            string criteria = selectedItem?.Content.ToString() ?? "Code";

            // Filter articles based on criteria
            filteredArticles = new List<Article>();

            foreach (Article a in la)
            {
                // Skip articles where Etat is false
                if (!a.Etat)
                    continue;

                bool matches = false;

                switch (criteria)
                {
                    case "Code":
                        matches = a.Code.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        break;

                    case "Article":
                        matches = a.ArticleName != null &&
                                   a.ArticleName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        break;

                    case "Fournisseur":
                        string fournisseurName = GetFournisseurName(a.FournisseurID);
                        matches = !string.IsNullOrEmpty(fournisseurName) &&
                                   fournisseurName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        break;

                    case "Famille":
                        string familleName = GetFamilleName(a.FamillyID);
                        matches = !string.IsNullOrEmpty(familleName) &&
                                   familleName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        break;

                    case "Numero de Lot":
                    case "Numéro de Lot":
                        matches = !string.IsNullOrEmpty(a.numeroLot) &&
                                   a.numeroLot.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        break;

                    case "Bon de Livraison":
                        matches = !string.IsNullOrEmpty(a.bonlivraison) &&
                                   a.bonlivraison.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        break;

                    case "Marque":
                        matches = !string.IsNullOrEmpty(a.marque) &&
                                   a.marque.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        break;
                }

                if (matches)
                {
                    filteredArticles.Add(a);
                }
            }

            // Apply sorting to filtered results
            filteredArticles = ApplySorting(filteredArticles);

            // Reset and load filtered results with pagination
            currentlyLoadedCount = 0;
            ArticlesContainer.Children.Clear();
            LoadMoreArticles();
        }

        // Helper method to get Fournisseur name by ID
        private string GetFournisseurName(int fournisseurId)
        {
            foreach (var fournisseur in lfo)
            {
                if (fournisseur.FournisseurID == fournisseurId)
                {
                    return fournisseur.Nom;
                }
            }
            return string.Empty;
        }

        // Helper method to get Famille name by ID
        private string GetFamilleName(int familleId)
        {
            foreach (var famille in lf)
            {
                if (famille.FamilleID == familleId)
                {
                    return famille.FamilleName;
                }
            }
            return string.Empty;
        }

        // Nouveau Article button
        private void NewArticleButton_Click(object sender, RoutedEventArgs e)
        {
            WNouveauStock wNouveauStock = new WNouveauStock(lf, la, lfo, this, 1, null, null);
            wNouveauStock.ShowDialog();
        }

        // Fournisseur management button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            main.load_fournisseur(u);
        }

        // Manage Famillies button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WManageFamillies wManageFamillies = new WManageFamillies(lf, la, this);
            wManageFamillies.ShowDialog();
        }

        // Add Multiple Articles button
        private void AddMultipleArticlesButton_Click(object sender, RoutedEventArgs e)
        {
            WAddMultipleArticles wAddMultipleArticles = new WAddMultipleArticles(this);
            wAddMultipleArticles.ShowDialog();
        }
        private void GenerateDevisButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if user has permission
            bool hasPermission = false;
            foreach (Role r in main.lr)
            {
                if (u.RoleID == r.RoleID)
                {
                    if (r.ViewArticle == true)
                    {
                        hasPermission = true;
                    }
                    break;
                }
            }

            if (!hasPermission)
            {
                MessageBox.Show("Vous n'avez pas la permission de générer un devis.",
                    "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if there are articles
            if (la == null || la.Count == 0)
            {
                MessageBox.Show("Aucun article disponible pour générer un devis.",
                    "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Open article selection window directly
            WManualArticleSelection selectionWindow = new WManualArticleSelection(la, lf, lfo);

            bool? selectionResult = selectionWindow.ShowDialog();

            if (selectionResult == true)
            {
                List<Article> selectedArticles = selectionWindow.SelectedArticles;

                if (selectedArticles == null || selectedArticles.Count == 0)
                {
                    MessageBox.Show("Aucun article sélectionné.",
                        "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Open customization window
                WDevisCustomization customizationWindow = new WDevisCustomization(
                    selectedArticles, lf, lfo);
                customizationWindow.ShowDialog();
            }
        }

    }
}