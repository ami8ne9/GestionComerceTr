using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Superete; // For Facture class
using GestionComerce;

namespace GestionComerce.Main.Delivery
{
    // Helper class pour afficher les articles dans le DataGrid
    public class ArticleDisplay
    {
        public string ArticleName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }

    public partial class LivraisonDetailsWindow : Window
    {
        private MainWindow main;
        private User u;
        private Livraison livraison;
        private Operation operation;

        // Event pour notifier que la livraison a été mise à jour
        public event EventHandler LivraisonUpdated;

        public LivraisonDetailsWindow(MainWindow main, User u, Livraison livraison)
        {
            InitializeComponent();
            this.main = main;
            this.u = u;
            this.livraison = livraison;

            Loaded += LivraisonDetailsWindow_Loaded;
        }

        private async void LivraisonDetailsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLivraisonDetailsAsync();
        }

        // Charger les détails de la livraison
        private async Task LoadLivraisonDetailsAsync()
        {
            try
            {
                // Recharger les données depuis la base
                Livraison fresh = new Livraison();
                livraison = await fresh.GetLivraisonByIDAsync(livraison.LivraisonID);

                if (livraison == null)
                {
                    MessageBox.Show("Livraison introuvable.", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Header
                TxtHeaderTitle.Text = $"📦 Détails de la Livraison #{livraison.LivraisonID}";
                TxtHeaderSubtitle.Text = $"Créée le {livraison.DateCreation:dd/MM/yyyy à HH:mm}";

                // Statut Badge
                UpdateStatutBadge(livraison.Statut);

                // Charger les infos de l'opération
                await LoadOperationInfoAsync();

                // Client Info
                TxtClientNom.Text = livraison.ClientNom ?? "-";
                TxtClientTelephone.Text = livraison.ClientTelephone ?? "-";
                TxtClientID.Text = livraison.ClientID?.ToString() ?? "-";

                // Adresse
                TxtAdresse.Text = livraison.AdresseLivraison ?? "-";
                TxtVille.Text = livraison.Ville ?? "-";
                TxtCodePostal.Text = livraison.CodePostal ?? "-";
                TxtZone.Text = livraison.ZoneLivraison ?? "-";

                // Détails Livraison
                TxtDatePrevue.Text = livraison.DateLivraisonPrevue?.ToString("dd/MM/yyyy HH:mm") ?? "-";
                TxtDateEffective.Text = livraison.DateLivraisonEffective?.ToString("dd/MM/yyyy HH:mm") ?? "-";
                TxtLivreur.Text = livraison.LivreurNom ?? "Non assigné";
                TxtModePaiement.Text = livraison.ModePaiement ?? "-";
                TxtNotes.Text = string.IsNullOrWhiteSpace(livraison.Notes) ? "Aucune note" : livraison.Notes;

                // Financier
                TxtTotalCommande.Text = $"{livraison.TotalCommande:N2} DH";
                TxtFraisLivraison.Text = $"{livraison.FraisLivraison:N2} DH";
                decimal total = livraison.TotalCommande + livraison.FraisLivraison;
                TxtTotal.Text = $"{total:N2} DH";

                // Sélectionner le statut actuel dans le ComboBox
                SelectStatutInComboBox(livraison.Statut);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des détails: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Charger les informations de l'opération et ses articles
        private async Task LoadOperationInfoAsync()
        {
            try
            {
                Operation op = new Operation { OperationID = livraison.OperationID };
                var operations = await op.GetOperationsAsync();
                operation = operations.FirstOrDefault(o => o.OperationID == livraison.OperationID);

                if (operation != null)
                {
                    TxtOperationID.Text = operation.OperationID.ToString();
                    TxtOperationType.Text = operation.OperationType ?? "Vente";
                    TxtOperationDate.Text = operation.DateOperation.ToString("dd/MM/yyyy HH:mm");

                    // Charger les articles de l'opération
                    await LoadOperationArticlesAsync();
                }
                else
                {
                    TxtOperationID.Text = livraison.OperationID.ToString();
                    TxtOperationType.Text = "-";
                    TxtOperationDate.Text = "-";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'opération: {ex.Message}");
            }
        }

        // Charger les articles de l'opération
        private async Task LoadOperationArticlesAsync()
        {
            try
            {
                OperationArticle opArticle = new OperationArticle();
                var allOpArticles = await opArticle.GetOperationArticlesAsync();

                // Filtrer par OperationID
                var operationArticles = allOpArticles
                    .Where(oa => oa.OperationID == livraison.OperationID)
                    .ToList();

                // Créer une liste d'affichage avec les noms des articles
                List<ArticleDisplay> displayList = new List<ArticleDisplay>();

                foreach (var opArt in operationArticles)
                {
                    Article article = new Article();
                    var articles = await article.GetArticlesAsync();
                    var foundArticle = articles.FirstOrDefault(a => a.ArticleID == opArt.ArticleID);

                    if (foundArticle != null)
                    {
                        displayList.Add(new ArticleDisplay
                        {
                            ArticleName = foundArticle.ArticleName,
                            Quantity = opArt.QteArticle,
                            UnitPrice = foundArticle.PrixVente,
                            Total = foundArticle.PrixVente * opArt.QteArticle
                        });
                    }
                }

                DgArticles.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des articles: {ex.Message}");
            }
        }

        // Mettre à jour le badge de statut
        private void UpdateStatutBadge(string statut)
        {
            switch (statut)
            {
                case "en_attente":
                    StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7"));
                    TxtStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400E"));
                    TxtStatut.Text = "EN ATTENTE";
                    break;
                case "confirmee":
                    StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
                    TxtStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E40AF"));
                    TxtStatut.Text = "CONFIRMÉE";
                    break;
                case "en_preparation":
                    StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E7FF"));
                    TxtStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4338CA"));
                    TxtStatut.Text = "EN PRÉPARATION";
                    break;
                case "en_cours":
                    StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
                    TxtStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D4ED8"));
                    TxtStatut.Text = "EN COURS";
                    break;
                case "livree":
                    StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1FAE5"));
                    TxtStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46"));
                    TxtStatut.Text = "LIVRÉE";
                    break;
                case "annulee":
                    StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                    TxtStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B"));
                    TxtStatut.Text = "ANNULÉE";
                    break;
                default:
                    StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"));
                    TxtStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
                    TxtStatut.Text = "INCONNU";
                    break;
            }
        }

        // Sélectionner le statut dans le ComboBox
        private void SelectStatutInComboBox(string statut)
        {
            foreach (ComboBoxItem item in CmbNouveauStatut.Items)
            {
                if (item.Tag.ToString() == statut)
                {
                    CmbNouveauStatut.SelectedItem = item;
                    break;
                }
            }
        }

        // Bouton Mettre à jour le Statut
        private async void BtnUpdateStatut_Click(object sender, RoutedEventArgs e)
        {
            if (CmbNouveauStatut.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un nouveau statut.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedItem = (ComboBoxItem)CmbNouveauStatut.SelectedItem;
            string nouveauStatut = selectedItem.Tag.ToString();

            if (nouveauStatut == livraison.Statut)
            {
                MessageBox.Show("Le statut sélectionné est déjà le statut actuel.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                BtnUpdateStatut.IsEnabled = false;
                BtnUpdateStatut.Content = "⏳ Mise à jour...";

                string commentaire = TxtCommentaire.Text.Trim();
                int result = await livraison.UpdateStatutAsync(nouveauStatut, commentaire);

                if (result > 0)
                {
                    MessageBox.Show("✅ Statut mis à jour avec succès!", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Recharger les détails
                    await LoadLivraisonDetailsAsync();

                    // Vider le commentaire
                    TxtCommentaire.Clear();

                    // Déclencher l'événement
                    LivraisonUpdated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show("Erreur lors de la mise à jour du statut.", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnUpdateStatut.IsEnabled = true;
                BtnUpdateStatut.Content = "✅ Mettre à jour";
            }
        }

        // Bouton Appeler Client
        private void BtnCallClient_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(livraison.ClientTelephone))
            {
                try
                {
                    // Ouvrir l'application téléphone avec le numéro
                    Process.Start($"tel:{livraison.ClientTelephone}");
                }
                catch
                {
                    // Si ça ne marche pas, copier dans le presse-papier
                    Clipboard.SetText(livraison.ClientTelephone);
                    MessageBox.Show($"📞 Numéro copié dans le presse-papier:\n{livraison.ClientTelephone}",
                        "Téléphone", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Aucun numéro de téléphone disponible.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Bouton Voir Itinéraire
        private void BtnViewRoute_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(livraison.AdresseLivraison))
            {
                try
                {
                    // Construire l'URL Google Maps
                    string address = $"{livraison.AdresseLivraison}, {livraison.Ville}, {livraison.CodePostal}";
                    string encodedAddress = Uri.EscapeDataString(address);
                    string mapsUrl = $"https://www.google.com/maps/search/?api=1&query={encodedAddress}";

                    // Ouvrir dans le navigateur
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = mapsUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ouverture de Google Maps: {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Aucune adresse disponible.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Bouton Fermer
        private void BtnFermer_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        // Bouton Voir Bon de Livraison
        private async void BtnViewTicket_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the operation to retrieve selected operations
                Operation op = new Operation { OperationID = livraison.OperationID };
                var operations = await op.GetOperationsAsync();
                var selectedOperation = operations.FirstOrDefault(o => o.OperationID == livraison.OperationID);

                if (selectedOperation != null)
                {
                    // Create a list with just this operation
                    List<Operation> operationsList = new List<Operation> { selectedOperation };

                    // Get delivery time if available from notes
                    string heureCrenneau = ExtractTimeFromNotes(livraison.Notes);

                    // Open the ticket window
                    DeliveryTicketWindow ticketWindow = new DeliveryTicketWindow(
                        livraison,
                        operationsList,
                        livraison.LivreurNom ?? "Non assigné",
                        heureCrenneau
                    );
                    ticketWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Impossible de charger les informations de l'opération.", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture du bon de livraison: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper method to extract time from notes
        private string ExtractTimeFromNotes(string notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return "À définir";

            // Try to find time pattern like "10:00 - 12:00"
            var timePatterns = new[] {
        "08:00 - 10:00", "10:00 - 12:00", "12:00 - 14:00",
        "14:00 - 16:00", "16:00 - 18:00", "18:00 - 20:00"
    };

            foreach (var pattern in timePatterns)
            {
                if (notes.Contains(pattern))
                    return pattern;
            }

            return "À définir";
        }

        // Bouton Sauvegarder comme Facture
        private async void BtnSaveAsInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnSaveAsInvoice.IsEnabled = false;
                BtnSaveAsInvoice.Content = "⏳ Sauvegarde...";

                // Load company info
                Superete.Facture facture = new Superete.Facture();
                facture = await facture.GetFactureAsync();

                // Load client info
                Client client = null;
                if (livraison.ClientID.HasValue)
                {
                    Client clientObj = new Client();
                    var clients = await clientObj.GetClientsAsync();
                    client = clients.FirstOrDefault(c => c.ClientID == livraison.ClientID.Value);
                }

                // Create invoice repository
                var invoiceRepo = new GestionComerce.Main.Facturation.InvoiceRepository("");

                // Generate unique invoice number
                string invoiceNumber = await GenerateUniqueInvoiceNumber(invoiceRepo);

                // Create invoice object
                var invoice = new GestionComerce.Main.Facturation.Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = DateTime.Now,
                    InvoiceType = "Bon de Livraison",
                    InvoiceIndex = livraison.LivraisonID.ToString(),

                    // Company info
                    UserName = facture?.Name ?? "Votre Entreprise",
                    UserICE = facture?.ICE ?? "",
                    UserVAT = facture?.VAT ?? "",
                    UserPhone = facture?.Telephone ?? "",
                    UserAddress = facture?.Adresse ?? "",
                    UserEtatJuridique = facture?.EtatJuridic ?? "",
                    UserIdSociete = facture?.CompanyId ?? "",
                    UserSiegeEntreprise = facture?.SiegeEntreprise ?? "",

                    // Client info
                    ClientName = livraison.ClientNom ?? "",
                    ClientICE = client?.ICE ?? "",
                    ClientVAT = client?.EtatJuridique ?? "",
                    ClientPhone = livraison.ClientTelephone ?? "",
                    ClientAddress = $"{livraison.AdresseLivraison}, {livraison.Ville} {livraison.CodePostal}",

                    // Financial info
                    Currency = "DH",
                    TVARate = 0,
                    TotalHT = livraison.TotalCommande,
                    TotalTVA = 0,
                    TotalTTC = livraison.TotalCommande + livraison.FraisLivraison,
                    Remise = 0,
                    TotalAfterRemise = livraison.TotalCommande + livraison.FraisLivraison,

                    EtatFacture = 1,
                    IsReversed = false,
                    Description = $"Bon de livraison #{livraison.LivraisonID} - {livraison.Notes}",
                    LogoPath = facture?.LogoPath ?? "",
                    CreatedBy = u?.UserID
                };

                // Load articles from operation
                await LoadArticlesForInvoice(invoice);

                // Save invoice
                int invoiceId = await invoiceRepo.CreateInvoiceAsync(invoice);

                if (invoiceId > 0)
                {
                    MessageBox.Show($"✅ Bon de livraison sauvegardé comme facture!\n\nNuméro de facture: {invoiceNumber}",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Erreur lors de la sauvegarde de la facture.", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnSaveAsInvoice.IsEnabled = true;
                BtnSaveAsInvoice.Content = "💾 Sauvegarder comme Facture";
            }
        }

        // Generate unique invoice number
        private async Task<string> GenerateUniqueInvoiceNumber(GestionComerce.Main.Facturation.InvoiceRepository repo)
        {
            string prefix = $"BL-{DateTime.Now:yyyyMMdd}";
            int counter = 1;
            string invoiceNumber;

            do
            {
                invoiceNumber = $"{prefix}-{counter:D3}";
                counter++;
            }
            while (await repo.InvoiceNumberExistsAsync(invoiceNumber));

            return invoiceNumber;
        }

        // Load articles for invoice
        private async Task LoadArticlesForInvoice(GestionComerce.Main.Facturation.Invoice invoice)
        {
            try
            {
                OperationArticle opArticle = new OperationArticle();
                var allOpArticles = await opArticle.GetOperationArticlesAsync();

                // Get articles for this operation
                var operationArticles = allOpArticles
                    .Where(oa => oa.OperationID == livraison.OperationID && !oa.Reversed)
                    .ToList();

                // Load article details
                Article articleObj = new Article();
                var allArticles = await articleObj.GetArticlesAsync();

                foreach (var opArt in operationArticles)
                {
                    var article = allArticles.FirstOrDefault(a => a.ArticleID == opArt.ArticleID);
                    if (article != null)
                    {
                        invoice.Articles.Add(new GestionComerce.Main.Facturation.Invoice.InvoiceArticle
                        {
                            OperationID = opArt.OperationID,
                            ArticleID = article.ArticleID,
                            ArticleName = article.marque ?? article.ArticleName ?? "Article",
                            PrixUnitaire = article.PrixVente,
                            Quantite = opArt.QteArticle,
                            TVA = 0,
                            IsReversed = false
                        });
                    }
                }

                // Calculate totals
                invoice.CalculateTotals();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des articles: {ex.Message}");
            }
        }
    }
}