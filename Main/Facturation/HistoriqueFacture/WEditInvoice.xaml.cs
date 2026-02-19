using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GestionComerce.Main.Facturation.CreateFacture;

namespace GestionComerce.Main.Facturation.HistoriqueFacture
{
    public partial class WEditInvoice : Window
    {
        private Invoice _invoice;
        private InvoiceRepository _invoiceRepository;
        private ObservableCollection<Invoice.InvoiceArticle> _articles;
        private CMainHf _parentControl;

        public WEditInvoice(Invoice invoice, InvoiceRepository repository, CMainHf parent)
        {
            InitializeComponent();
            _invoice = invoice;
            _invoiceRepository = repository;
            _parentControl = parent;

            // Handle null Articles list
            if (_invoice.Articles == null)
            {
                _articles = new ObservableCollection<Invoice.InvoiceArticle>();
            }
            else
            {
                _articles = new ObservableCollection<Invoice.InvoiceArticle>(_invoice.Articles);
            }

            LoadInvoiceData();
        }

        private void LoadInvoiceData()
        {
            // Invoice Information
            txtInvoiceNumber.Text = $"N° {_invoice.InvoiceNumber}";
            txtEditInvoiceNumber.Text = _invoice.InvoiceNumber;
            dpEditInvoiceDate.SelectedDate = _invoice.InvoiceDate;

            // Set invoice type
            foreach (ComboBoxItem item in cmbEditInvoiceType.Items)
            {
                if (item.Content.ToString() == _invoice.InvoiceType)
                {
                    cmbEditInvoiceType.SelectedItem = item;
                    break;
                }
            }

            // NEW FIELDS
            txtEditObjet.Text = _invoice.Objet ?? "";
            txtEditNumberLetters.Text = _invoice.NumberLetters ?? "";
            txtEditNameFactureGiven.Text = _invoice.NameFactureGiven ?? "";
            txtEditNameFactureReceiver.Text = _invoice.NameFactureReceiver ?? "";
            txtEditReferenceClient.Text = _invoice.ReferenceClient ?? "";
            txtEditPaymentMethod.Text = _invoice.PaymentMethod ?? "";

            // Client Information
            txtEditClientName.Text = _invoice.ClientName;
            txtEditClientICE.Text = _invoice.ClientICE;
            txtEditClientVAT.Text = _invoice.ClientVAT;
            txtEditClientPhone.Text = _invoice.ClientPhone;
            txtEditClientAddress.Text = _invoice.ClientAddress;

            // Articles
            dgEditArticles.ItemsSource = _articles;

            // Financial Information
            txtEditTVARate.Text = _invoice.TVARate.ToString("0.00");
            txtEditRemise.Text = _invoice.Remise.ToString("0.00");
            txtEditDescription.Text = _invoice.Description;

            // Calculate totals
            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            // Safety check
            if (txtEditTotalHT == null || txtEditTVAAmount == null || txtEditTotalTTC == null)
                return;

            decimal totalHT = 0;
            decimal totalTVA = 0;

            if (_articles != null)
            {
                foreach (var article in _articles)
                {
                    totalHT += article.PrixUnitaire * article.Quantite;
                    totalTVA += (article.TVA / 100) * (article.PrixUnitaire * article.Quantite);
                }
            }

            decimal remise = 0;
            if (txtEditRemise != null && !string.IsNullOrWhiteSpace(txtEditRemise.Text))
            {
                decimal.TryParse(txtEditRemise.Text, out remise);
            }

            decimal tvaRate = 0;
            if (txtEditTVARate != null && !string.IsNullOrWhiteSpace(txtEditTVARate.Text))
            {
                decimal.TryParse(txtEditTVARate.Text, out tvaRate);
            }

            decimal totalAfterRemise = totalHT - remise;
            decimal tvaAfterRemise = (tvaRate / 100) * totalAfterRemise;
            decimal totalTTC = totalAfterRemise + tvaAfterRemise;

            // Update UI
            txtEditTotalHT.Text = totalHT.ToString("0.00") + " DH";
            txtEditTVAAmount.Text = tvaAfterRemise.ToString("0.00") + " DH";
            txtEditTotalTTC.Text = totalTTC.ToString("0.00") + " DH";
        }

        private void FinancialField_Changed(object sender, TextChangedEventArgs e)
        {
            RecalculateTotals();
        }

        private void btnAddArticle_Click(object sender, RoutedEventArgs e)
        {
            // Create a simple dialog to add new article
            WAddArticleDialog dialog = new WAddArticleDialog();
            if (dialog.ShowDialog() == true)
            {
                var newArticle = new Invoice.InvoiceArticle
                {
                    InvoiceID = _invoice.InvoiceID,
                    ArticleID = dialog.ArticleID,
                    ArticleName = dialog.ArticleName,
                    PrixUnitaire = dialog.PrixUnitaire,
                    Quantite = dialog.Quantite,
                    TVA = dialog.TVA,
                    IsReversed = false
                };

                _articles.Add(newArticle);
                RecalculateTotals();
            }
        }

        private void btnDeleteArticle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Invoice.InvoiceArticle article)
            {
                var result = MessageBox.Show(
                    $"Supprimer l'article '{article.ArticleName}' ?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _articles.Remove(article);
                    RecalculateTotals();
                }
            }
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create temporary invoice for preview
                Invoice previewInvoice = GetUpdatedInvoice();

                // Prepare invoice info dictionary
                Dictionary<string, string> factureInfo = new Dictionary<string, string>()
                {
                    { "NFacture", previewInvoice.InvoiceNumber ?? "" },
                    { "Date", previewInvoice.InvoiceDate.ToString("dd/MM/yyyy") },
                    { "Type", previewInvoice.InvoiceType ?? "" },
                    { "Objet", previewInvoice.Objet ?? "" },
                    { "NumberLetters", previewInvoice.NumberLetters ?? "" },
                    { "NameFactureGiven", previewInvoice.NameFactureGiven ?? "" },
                    { "NameFactureReceiver", previewInvoice.NameFactureReceiver ?? "" },
                    { "ReferenceClient", previewInvoice.ReferenceClient ?? "" },
                    { "PaymentMethod", previewInvoice.PaymentMethod ?? "" },
                    { "NomU", previewInvoice.UserName ?? "" },
                    { "ICEU", previewInvoice.UserICE ?? "" },
                    { "VATU", previewInvoice.UserVAT ?? "" },
                    { "TelephoneU", previewInvoice.UserPhone ?? "" },
                    { "EtatJuridiqueU", previewInvoice.UserEtatJuridique ?? "" },
                    { "IdSocieteU", previewInvoice.UserIdSociete ?? "" },
                    { "SiegeEntrepriseU", previewInvoice.UserSiegeEntreprise ?? "" },
                    { "AdressU", previewInvoice.UserAddress ?? "" },
                    { "NomC", previewInvoice.ClientName ?? "" },
                    { "ICEC", previewInvoice.ClientICE ?? "" },
                    { "VATC", previewInvoice.ClientVAT ?? "" },
                    { "TelephoneC", previewInvoice.ClientPhone ?? "" },
                    { "EtatJuridiqueC", previewInvoice.ClientEtatJuridique ?? "" },
                    { "IdSocieteC", previewInvoice.ClientIdSociete ?? "" },
                    { "SiegeEntrepriseC", previewInvoice.ClientSiegeEntreprise ?? "" },
                    { "AdressC", previewInvoice.ClientAddress ?? "" },
                    { "EtatFature", previewInvoice.IsReversed ? "Annulée" : "Normale" },
                    { "Device", previewInvoice.Currency ?? "DH" },
                    { "TVA", previewInvoice.TVARate.ToString("0.00") },
                    { "MontantTotal", previewInvoice.TotalHT.ToString("0.00") + " DH" },
                    { "MontantTVA", previewInvoice.TotalTVA.ToString("0.00") + " DH" },
                    { "MontantApresTVA", previewInvoice.TotalTTC.ToString("0.00") + " DH" },
                    { "MontantApresRemise", previewInvoice.TotalAfterRemise.ToString("0.00") + " DH" },
                    { "IndexDeFacture", previewInvoice.InvoiceIndex ?? "" },
                    { "Description", previewInvoice.Description ?? "" },
                    { "Logo", previewInvoice.LogoPath ?? "" },
                    { "Reversed", previewInvoice.IsReversed ? "Oui" : "Non" },
                    { "Remise", previewInvoice.Remise.ToString("0.00") }
                };

                // Convert to InvoiceArticle for compatibility
                List<InvoiceArticle> articles = previewInvoice.Articles.Select(a => new InvoiceArticle
                {
                    OperationID = a.OperationID ?? 0,
                    ArticleID = a.ArticleID,
                    ArticleName = a.ArticleName,
                    Prix = a.PrixUnitaire,
                    Quantite = a.Quantite,
                    TVA = a.TVA,
                    Reversed = a.IsReversed
                }).ToList();

                // Open preview
                WFacturePage wFacturePage = new WFacturePage(null, factureInfo, articles);
                wFacturePage.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de la prévisualisation: {ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(txtEditInvoiceNumber.Text))
                {
                    MessageBox.Show("Le numéro de facture est requis", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_articles.Count == 0)
                {
                    MessageBox.Show("Au moins un article est requis", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get updated invoice
                Invoice updatedInvoice = GetUpdatedInvoice();

                // Save to database
                bool success = await _invoiceRepository.UpdateInvoiceAsync(updatedInvoice);

                if (success)
                {
                    MessageBox.Show(
                        "Facture mise à jour avec succès!",
                        "Succès",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Refresh parent list
                    await _parentControl.LoadInvoicesAsync();

                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Échec de la mise à jour de la facture",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de l'enregistrement: {ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private Invoice GetUpdatedInvoice()
        {
            decimal.TryParse(txtEditTVARate.Text, out decimal tvaRate);
            decimal.TryParse(txtEditRemise.Text, out decimal remise);

            // Calculate totals
            decimal totalHT = _articles.Sum(a => a.PrixUnitaire * a.Quantite);
            decimal totalTVA = _articles.Sum(a => (a.TVA / 100) * (a.PrixUnitaire * a.Quantite));
            decimal totalTTC = totalHT + totalTVA;
            decimal totalAfterRemise = totalHT - remise + ((tvaRate / 100) * (totalHT - remise));

            return new Invoice
            {
                InvoiceID = _invoice.InvoiceID,
                InvoiceNumber = txtEditInvoiceNumber.Text,
                InvoiceDate = dpEditInvoiceDate.SelectedDate ?? DateTime.Now,
                InvoiceType = (cmbEditInvoiceType.SelectedItem as ComboBoxItem)?.Content.ToString(),
                InvoiceIndex = _invoice.InvoiceIndex,

                // NEW FIELDS - Get from UI
                Objet = txtEditObjet.Text,
                NumberLetters = txtEditNumberLetters.Text,
                NameFactureGiven = txtEditNameFactureGiven.Text,
                NameFactureReceiver = txtEditNameFactureReceiver.Text,
                ReferenceClient = txtEditReferenceClient.Text,
                PaymentMethod = txtEditPaymentMethod.Text,

                // Keep original user info
                UserName = _invoice.UserName,
                UserICE = _invoice.UserICE,
                UserVAT = _invoice.UserVAT,
                UserPhone = _invoice.UserPhone,
                UserAddress = _invoice.UserAddress,
                UserEtatJuridique = _invoice.UserEtatJuridique,
                UserIdSociete = _invoice.UserIdSociete,
                UserSiegeEntreprise = _invoice.UserSiegeEntreprise,

                // Updated client info
                ClientName = txtEditClientName.Text,
                ClientICE = txtEditClientICE.Text,
                ClientVAT = txtEditClientVAT.Text,
                ClientPhone = txtEditClientPhone.Text,
                ClientAddress = txtEditClientAddress.Text,
                ClientEtatJuridique = _invoice.ClientEtatJuridique,
                ClientIdSociete = _invoice.ClientIdSociete,
                ClientSiegeEntreprise = _invoice.ClientSiegeEntreprise,

                Currency = _invoice.Currency,
                TVARate = tvaRate,
                TotalHT = totalHT,
                TotalTVA = totalTVA,
                TotalTTC = totalTTC,
                Remise = remise,
                TotalAfterRemise = totalAfterRemise,

                EtatFacture = _invoice.EtatFacture,
                IsReversed = _invoice.IsReversed,
                Description = txtEditDescription.Text,
                LogoPath = _invoice.LogoPath,

                CreatedDate = _invoice.CreatedDate,
                CreatedBy = _invoice.CreatedBy,
                ModifiedDate = DateTime.Now,
                ModifiedBy = _invoice.CreatedBy,

                Articles = _articles.ToList()
            };
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Annuler les modifications?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            btnCancel_Click(sender, e);
        }
    }

    // Simple dialog for adding articles
    public partial class WAddArticleDialog : Window
    {
        public int ArticleID { get; private set; }
        public string ArticleName { get; private set; }
        public decimal PrixUnitaire { get; private set; }
        public decimal Quantite { get; private set; }
        public decimal TVA { get; private set; }

        public WAddArticleDialog()
        {
            Width = 500;
            Height = 450;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Title = "Ajouter un Article";

            Grid grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Article Name
            grid.Children.Add(CreateLabel("Nom de l'article", 0));
            TextBox txtName = CreateTextBox(2);
            grid.Children.Add(txtName);

            // Prix Unitaire
            grid.Children.Add(CreateLabel("Prix Unitaire (DH)", 4));
            TextBox txtPrice = CreateTextBox(6);
            grid.Children.Add(txtPrice);

            // Quantité
            grid.Children.Add(CreateLabel("Quantité", 8));
            TextBox txtQty = CreateTextBox(10);
            grid.Children.Add(txtQty);

            // TVA
            grid.Children.Add(CreateLabel("TVA (%)", 12));
            TextBox txtTVA = CreateTextBox(14);
            txtTVA.Text = "20";
            grid.Children.Add(txtTVA);

            // Buttons
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(buttonPanel, 16);

            Button btnCancel = new Button
            {
                Content = "Annuler",
                Width = 100,
                Height = 35,
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            Button btnOk = new Button
            {
                Content = "Ajouter",
                Width = 100,
                Height = 35
            };
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Le nom est requis", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                {
                    MessageBox.Show("Prix invalide", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtQty.Text, out decimal qty) || qty <= 0)
                {
                    MessageBox.Show("Quantité invalide", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtTVA.Text, out decimal tva) || tva < 0)
                {
                    MessageBox.Show("TVA invalide", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ArticleID = new Random().Next(10000, 99999);
                ArticleName = txtName.Text;
                PrixUnitaire = price;
                Quantite = qty;
                TVA = tva;

                DialogResult = true;
                Close();
            };

            buttonPanel.Children.Add(btnCancel);
            buttonPanel.Children.Add(btnOk);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }

        private TextBlock CreateLabel(string text, int row)
        {
            var label = new TextBlock
            {
                Text = text,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 6)
            };
            Grid.SetRow(label, row);
            return label;
        }

        private TextBox CreateTextBox(int row)
        {
            var textBox = new TextBox
            {
                Height = 35,
                Padding = new Thickness(10),
                FontSize = 14
            };
            Grid.SetRow(textBox, row);
            return textBox;
        }
    }
}