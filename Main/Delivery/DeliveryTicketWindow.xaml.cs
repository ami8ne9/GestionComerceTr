using Superete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GestionComerce.Main.Delivery
{
    public partial class DeliveryTicketWindow : Window
    {
        private Livraison livraison;
        private List<Operation> operations;
        private string livreurName;
        private string heureCrenneau;

        public DeliveryTicketWindow(
            Livraison livraison,
            List<Operation> operations,
            string livreurName,
            string heureCrenneau)
        {
            InitializeComponent();
            this.livraison = livraison;
            this.operations = operations;
            this.livreurName = livreurName;
            this.heureCrenneau = heureCrenneau;

            Loaded += DeliveryTicketWindow_Loaded;
        }

        private async void DeliveryTicketWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await PopulateTicketDataAsync();
        }

        private async System.Threading.Tasks.Task PopulateTicketDataAsync()
        {
            try
            {
                // ========== LOAD COMPANY INFO FROM FACTURE ==========
                Facture facture = new Facture();
                facture = await facture.GetFactureAsync();

                if (facture != null)
                {
                    TxtCompanyName.Text = facture.Name ?? "VOTRE ENTREPRISE";
                    TxtCompanyAddress.Text = facture.Adresse ?? "Adresse de l'entreprise";
                    TxtCompanyPhone.Text = $"Tél: {facture.Telephone ?? "+212 XXX XXX XXX"}";

                    // Optional: Display ICE/VAT if needed
                    // TxtCompanyAddress.Text = $"{facture.Adresse}\nICE: {facture.ICE} | TVA: {facture.VAT}";
                }
                else
                {
                    TxtCompanyName.Text = "VOTRE ENTREPRISE";
                    TxtCompanyAddress.Text = "123 Rue Principale, Tangier";
                    TxtCompanyPhone.Text = "Tél: +212 XXX XXX XXX";
                }

                // ========== GENERATE UNIQUE BON NUMBER ==========
                // Format: LIV-YYYYMMDD-ID (e.g., LIV-20260102-00005)
                string bonNumber = $"LIV-{DateTime.Now:yyyyMMdd}-{livraison.LivraisonID:D5}";
                TxtBonNumber.Text = bonNumber;

                // Delivery info
                TxtDate.Text = livraison.DateLivraisonPrevue?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
                TxtLivreur.Text = livreurName ?? "Non assigné";
                TxtHeure.Text = heureCrenneau ?? "À définir";

                // Client info
                TxtClientName.Text = livraison.ClientNom ?? "";
                TxtClientPhone.Text = $"Tél: {livraison.ClientTelephone ?? ""}";
                TxtClientAddress.Text = livraison.AdresseLivraison ?? "";
                TxtClientCity.Text = $"{livraison.Ville ?? ""}, {livraison.CodePostal ?? ""}";

                // Load articles
                await LoadArticlesAsync();

                // Totals
                TxtTotalCommande.Text = $"{livraison.TotalCommande:N2} DH";
                TxtFraisLivraison.Text = $"{livraison.FraisLivraison:N2} DH";
                decimal total = livraison.TotalCommande + livraison.FraisLivraison;
                TxtTotal.Text = $"{total:N2} DH";

                // Payment & Zone
                TxtModePaiement.Text = livraison.ModePaiement ?? "Espèces";
                TxtZone.Text = livraison.ZoneLivraison ?? "";

                // Notes
                if (!string.IsNullOrWhiteSpace(livraison.Notes))
                {
                    NotesSection.Visibility = Visibility.Visible;
                    TxtNotes.Text = livraison.Notes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // REPLACE the entire LoadArticlesAsync method with this:
        private async System.Threading.Tasks.Task LoadArticlesAsync()
        {
            try
            {
                ArticlesPanel.Children.Clear();

                // Get all operation articles
                OperationArticle opArticle = new OperationArticle();
                var allOpArticles = await opArticle.GetOperationArticlesAsync();

                // Get articles from selected operations
                var operationIds = operations.Select(o => o.OperationID).ToList();
                var relevantArticles = allOpArticles
                    .Where(oa => operationIds.Contains(oa.OperationID) && !oa.Reversed)
                    .GroupBy(oa => oa.ArticleID)
                    .Select(g => new
                    {
                        ArticleID = g.Key,
                        TotalQuantity = g.Sum(x => x.QteArticle)
                    })
                    .ToList();

                if (relevantArticles.Count == 0)
                {
                    TextBlock noArticles = new TextBlock
                    {
                        Text = "Aucun article trouvé",
                        FontFamily = new FontFamily("Courier New"),
                        FontSize = 13,
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 10, 0, 10)
                    };
                    ArticlesPanel.Children.Add(noArticles);
                    return;
                }

                // Load article details from BOTH Article tables
                Article article = new Article();
                var allArticles = await article.GetArticlesAsync();

                // Also try to get from main.la if available (ArticleAll class)
                List<Article> allArticlesExtended = new List<Article>();
                try
                {
                    Article articleAll = new Article();
                    allArticlesExtended = await articleAll.GetArticlesAsync();
                }
                catch
                {
                    // If ArticleAll doesn't exist or fails, continue with regular Article
                }

                int index = 1;
                foreach (var artGroup in relevantArticles)
                {
                    // Try to find article in regular Article table first
                    var articleInfo = allArticles.FirstOrDefault(a => a.ArticleID == artGroup.ArticleID);

                    // If not found, try ArticleAll
                    Article articleAllInfo = null;
                    if (articleInfo == null && allArticlesExtended.Count > 0)
                    {
                        articleAllInfo = allArticlesExtended.FirstOrDefault(a => a.ArticleID == artGroup.ArticleID);
                    }

                    // Determine article name from available sources
                    string articleName = "Article inconnu";
                    decimal unitPrice = 0;

                    if (articleInfo != null)
                    {
                        // Try multiple properties for the name
                        articleName = !string.IsNullOrWhiteSpace(articleInfo.ArticleName)
                            ? articleInfo.ArticleName
                            : (!string.IsNullOrWhiteSpace(articleInfo.ArticleName)
                                ? articleInfo.ArticleName
                                : $"Article #{articleInfo.ArticleID}");
                        unitPrice = articleInfo.PrixVente;
                    }
                    else if (articleAllInfo != null)
                    {
                        articleName = !string.IsNullOrWhiteSpace(articleAllInfo.ArticleName)
                            ? articleAllInfo.ArticleName
                            : $"Article #{articleAllInfo.ArticleID}";
                        unitPrice = articleAllInfo.PrixVente;
                    }
                    else
                    {
                        // Fallback - article not found in any table
                        articleName = $"Article #{artGroup.ArticleID}";
                        unitPrice = 0;
                    }

                    // Create article line
                    Grid articleGrid = new Grid { Margin = new Thickness(0, 0, 0, 5) };
                    articleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
                    articleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    articleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                    articleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                    // Index
                    TextBlock txtIndex = new TextBlock
                    {
                        Text = $"{index}.",
                        FontFamily = new FontFamily("Courier New"),
                        FontSize = 13,
                        FontWeight = FontWeights.Bold
                    };
                    Grid.SetColumn(txtIndex, 0);

                    // Article name
                    TextBlock txtName = new TextBlock
                    {
                        Text = articleName,
                        FontFamily = new FontFamily("Courier New"),
                        FontSize = 13,
                        TextWrapping = TextWrapping.Wrap
                    };
                    Grid.SetColumn(txtName, 1);

                    // Quantity
                    TextBlock txtQty = new TextBlock
                    {
                        Text = $"x{artGroup.TotalQuantity}",
                        FontFamily = new FontFamily("Courier New"),
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetColumn(txtQty, 2);

                    // Price
                    decimal lineTotal = unitPrice * artGroup.TotalQuantity;
                    TextBlock txtPrice = new TextBlock
                    {
                        Text = $"{lineTotal:N2} DH",
                        FontFamily = new FontFamily("Courier New"),
                        FontSize = 13,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    Grid.SetColumn(txtPrice, 3);

                    articleGrid.Children.Add(txtIndex);
                    articleGrid.Children.Add(txtName);
                    articleGrid.Children.Add(txtQty);
                    articleGrid.Children.Add(txtPrice);

                    ArticlesPanel.Children.Add(articleGrid);

                    index++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des articles: {ex.Message}\n\nDétails: {ex.StackTrace}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

                // Show error in the panel
                TextBlock errorBlock = new TextBlock
                {
                    Text = $"Erreur: {ex.Message}",
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 13,
                    Foreground = System.Windows.Media.Brushes.Red,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                ArticlesPanel.Children.Add(errorBlock);
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Get the ticket content
                    printDialog.PrintVisual(TicketBorder, "Bon de Livraison");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'impression: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSavePDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"BonLivraison_{livraison.LivraisonID}_{DateTime.Now:yyyyMMdd}.pdf",
                    DefaultExt = ".pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Note: For PDF export, you'll need to install a NuGet package like:
                    // - PdfSharp or iTextSharp or similar
                    // For now, we'll show a message
                    MessageBox.Show("Pour exporter en PDF, veuillez installer un package NuGet comme PdfSharp ou iTextSharp.\n\nVous pouvez utiliser 'Imprimer' et choisir 'Microsoft Print to PDF' comme imprimante.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}