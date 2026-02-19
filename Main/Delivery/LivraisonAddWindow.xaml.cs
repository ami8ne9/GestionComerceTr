using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GestionComerce.Main.Delivery
{
    public partial class LivraisonAddWindow : Window
    {
        private MainWindow main;
        private User u;
        private List<Livreur> livreurs;
        private List<Operation> operations;
        private List<Operation> selectedOperations;
        private List<PaymentMethod> paymentMethods;

        public event EventHandler LivraisonAdded;

        public LivraisonAddWindow(MainWindow main, User u)
        {
            InitializeComponent();
            this.main = main;
            this.u = u;
            this.selectedOperations = new List<Operation>();

            Loaded += LivraisonAddWindow_Loaded;
        }

        private async void LivraisonAddWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DateLivraisonPrevue.SelectedDate = DateTime.Now.AddDays(1);
            await LoadLivreursAsync();
            await LoadPaymentMethodsAsync();
            await LoadOperationsAsync();
        }

        private async Task LoadLivreursAsync()
        {
            try
            {
                Livreur livreur = new Livreur();
                livreurs = await livreur.GetLivreursDisponiblesAsync();
                CmbLivreur.ItemsSource = livreurs;

                if (livreurs.Count > 0)
                {
                    CmbLivreur.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des livreurs: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPaymentMethodsAsync()
        {
            try
            {
                PaymentMethod paymentMethod = new PaymentMethod();
                paymentMethods = await paymentMethod.GetPaymentMethodsAsync();

                CmbModePaiement.ItemsSource = paymentMethods;
                CmbModePaiement.DisplayMemberPath = "PaymentMethodName";
                CmbModePaiement.SelectedValuePath = "PaymentMethodID";

                if (paymentMethods.Count > 0)
                {
                    var defaultPayment = paymentMethods.FirstOrDefault(p =>
                        p.PaymentMethodName.Equals("Espèces", StringComparison.OrdinalIgnoreCase) ||
                        p.PaymentMethodName.Equals("Cash", StringComparison.OrdinalIgnoreCase) ||
                        p.PaymentMethodName.Equals("À la livraison", StringComparison.OrdinalIgnoreCase));

                    if (defaultPayment != null)
                    {
                        CmbModePaiement.SelectedItem = defaultPayment;
                    }
                    else
                    {
                        CmbModePaiement.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des modes de paiement: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadOperationsAsync()
        {
            try
            {
                Operation operation = new Operation();
                var allOperations = await operation.GetOperationsAsync();

                operations = allOperations
                    .Where(op =>
                        op.Etat &&
                        !op.Reversed &&
                        op.OperationType.StartsWith("Vente", StringComparison.OrdinalIgnoreCase) &&
                        op.OperationType != "VenteLiv" &&
                        op.OperationType != "Livraison Groupée" &&
                        !IsOperationAlreadyInDelivery(op.OperationID))
                    .OrderByDescending(op => op.DateOperation)
                    .ToList();

                DgOperations.ItemsSource = null;
                DgOperations.ItemsSource = operations;

                if (operations.Count == 0)
                {
                    MessageBox.Show(
                        "Aucune opération de vente disponible pour créer une livraison.\n\n" +
                        "Assurez-vous d'avoir des ventes actives qui ne sont pas déjà en livraison.",
                        "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des opérations: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsOperationAlreadyInDelivery(int operationId)
        {
            try
            {
                using (var connection = new System.Data.SqlClient.SqlConnection(
                    "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;"))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Livraison WHERE OperationID = @OperationID AND Etat = 1";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@OperationID", operationId);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private async void BtnRefreshOperations_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnRefreshOperations.IsEnabled = false;
                BtnRefreshOperations.Content = "⏳ Chargement...";

                selectedOperations.Clear();
                TxtSelectedCount.Text = "0";
                TxtTotalCommande.Text = "0.00";

                await LoadOperationsAsync();

                BtnRefreshOperations.Content = "✅ Actualisé";
                await Task.Delay(1000);
                BtnRefreshOperations.Content = "🔄 Actualiser";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'actualisation: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnRefreshOperations.Content = "🔄 Actualiser";
            }
            finally
            {
                BtnRefreshOperations.IsEnabled = true;
            }
        }

        private void ChkSelect_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedOperations();
        }

        private void ChkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox != null)
            {
                SetAllCheckboxes(checkbox.IsChecked == true);
            }
        }

        private void SetAllCheckboxes(bool isChecked)
        {
            foreach (var item in DgOperations.Items)
            {
                var row = DgOperations.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    var checkbox = FindVisualChild<CheckBox>(row);
                    if (checkbox != null)
                    {
                        checkbox.IsChecked = isChecked;
                    }
                }
            }
            UpdateSelectedOperations();
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void UpdateSelectedOperations()
        {
            selectedOperations.Clear();

            foreach (var item in DgOperations.Items)
            {
                var operation = item as Operation;
                if (operation == null) continue;

                var row = DgOperations.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    var checkbox = FindVisualChild<CheckBox>(row);
                    if (checkbox != null && checkbox.IsChecked == true)
                    {
                        selectedOperations.Add(operation);
                    }
                }
            }

            TxtSelectedCount.Text = selectedOperations.Count.ToString();
            CalculateTotalCommande();
        }

        private void CalculateTotalCommande()
        {
            decimal total = 0;
            foreach (var op in selectedOperations)
            {
                total += op.PrixOperation - op.Remise;
            }
            TxtTotalCommande.Text = total.ToString("N2");
        }

        private void CmbZoneLivraison_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbZoneLivraison.SelectedItem != null)
            {
                ComboBoxItem selected = (ComboBoxItem)CmbZoneLivraison.SelectedItem;
                TxtFraisLivraison.Text = selected.Tag.ToString();
            }
        }

        private async void BtnCreer_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOperations.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner au moins une opération.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtClientNom.Text))
            {
                MessageBox.Show("Le nom du client est obligatoire.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtClientNom.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtClientTelephone.Text))
            {
                MessageBox.Show("Le téléphone du client est obligatoire.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtClientTelephone.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtAdresse.Text))
            {
                MessageBox.Show("L'adresse de livraison est obligatoire.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtAdresse.Focus();
                return;
            }

            if (!decimal.TryParse(TxtTotalCommande.Text, out decimal totalCommande) || totalCommande <= 0)
            {
                MessageBox.Show("Le montant total de la commande est invalide.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateLivraisonPrevue.SelectedDate.HasValue)
            {
                MessageBox.Show("La date de livraison prévue est obligatoire.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DateLivraisonPrevue.Focus();
                return;
            }

            try
            {
                BtnCreer.IsEnabled = false;
                BtnCreer.Content = "⏳ Création en cours...";

                // Update selected operations type to VenteLiv
                await UpdateOperationsTypeToVenteLivAsync(selectedOperations);

                // Get ClientID (use first operation's ClientID if exists, otherwise null)
                int? clientId = selectedOperations.FirstOrDefault(o => o.ClientID.HasValue)?.ClientID;

                // Create delivery linked to first selected operation
                Livraison livraison = new Livraison
                {
                    OperationID = selectedOperations.First().OperationID,
                    ClientID = clientId,
                    ClientNom = TxtClientNom.Text.Trim(),
                    ClientTelephone = TxtClientTelephone.Text.Trim(),
                    AdresseLivraison = TxtAdresse.Text.Trim(),
                    Ville = TxtVille.Text.Trim(),
                    CodePostal = TxtCodePostal.Text.Trim(),
                    ZoneLivraison = CmbZoneLivraison.SelectedItem != null
                        ? ((ComboBoxItem)CmbZoneLivraison.SelectedItem).Content.ToString()
                        : null,
                    FraisLivraison = decimal.Parse(TxtFraisLivraison.Text),
                    DateLivraisonPrevue = DateLivraisonPrevue.SelectedDate.Value,
                    LivreurID = CmbLivreur.SelectedValue != null
                        ? (int?)CmbLivreur.SelectedValue
                        : null,
                    LivreurNom = CmbLivreur.SelectedItem != null
                        ? ((Livreur)CmbLivreur.SelectedItem).NomComplet
                        : null,
                    Statut = CmbStatut.SelectedItem != null
                        ? ((ComboBoxItem)CmbStatut.SelectedItem).Tag.ToString()
                        : "en_attente",
                    Notes = $"Livraison groupée de {selectedOperations.Count} commande(s): {string.Join(", ", selectedOperations.Select(o => $"Op#{o.OperationID}"))}\n{TxtNotes.Text.Trim()}",
                    TotalCommande = totalCommande,
                    ModePaiement = CmbModePaiement.SelectedItem != null
                        ? ((PaymentMethod)CmbModePaiement.SelectedItem).PaymentMethodName
                        : "Espèces",
                    PaiementStatut = "non_paye"
                };

                int livraisonID = await livraison.InsertLivraisonAsync();

                if (livraisonID > 0)
                {
                    string livreurNom = CmbLivreur.SelectedItem != null
                        ? ((Livreur)CmbLivreur.SelectedItem).NomComplet
                        : "Non assigné";

                    string heureLivraison = CmbHeureLivraison.SelectedItem != null
                        ? ((ComboBoxItem)CmbHeureLivraison.SelectedItem).Content.ToString()
                        : "";

                    DeliveryTicketWindow ticketWindow = new DeliveryTicketWindow(
                        livraison,
                        selectedOperations,
                        livreurNom,
                        heureLivraison
                    );
                    ticketWindow.ShowDialog();

                    LivraisonAdded?.Invoke(this, EventArgs.Empty);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Erreur lors de la création de la livraison.", "Erreur",
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
                BtnCreer.IsEnabled = true;
                BtnCreer.Content = "✅ Créer la Livraison";
            }
        }

        private async Task UpdateOperationsTypeToVenteLivAsync(List<Operation> operations)
        {
            try
            {
                using (var connection = new System.Data.SqlClient.SqlConnection(
                    "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;"))
                {
                    await connection.OpenAsync();

                    foreach (var op in operations)
                    {
                        string query = @"UPDATE Operation 
                                       SET OperationType = 'VenteLiv' 
                                       WHERE OperationID = @OperationID";

                        using (var cmd = new System.Data.SqlClient.SqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@OperationID", op.OperationID);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour des types d'opérations: {ex.Message}");
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Êtes-vous sûr de vouloir annuler? Les données non enregistrées seront perdues.",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}