using GestionComerce.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GestionComerce.Main.Facturation.FacturesEnregistrees
{
    public partial class CMainEnregistrees : UserControl
    {
        private User user;
        private MainWindow main;
        private FactureEnregistree selectedInvoice;
        private List<SavedInvoiceDisplay> allInvoices;

        // Display class that includes supplier name
        public class SavedInvoiceDisplay
        {
            public int SavedInvoiceID { get; set; }
            public string InvoiceImagePath { get; set; }
            public int FournisseurID { get; set; }
            public decimal TotalAmount { get; set; }
            public DateTime InvoiceDate { get; set; }
            public string Description { get; set; }
            public string InvoiceReference { get; set; }
            public string Notes { get; set; }
        }

        // Display class for Fournisseur in ComboBox
        private class FournisseurDisplay
        {
            public int FournisseurID { get; set; }
            public string DisplayText { get; set; }
        }

        public CMainEnregistrees(User u, MainWindow mainWindow)
        {
            InitializeComponent();
            this.user = u;
            this.main = mainWindow;

            allInvoices = new List<SavedInvoiceDisplay>();
            dpInvoiceDate.SelectedDate = DateTime.Now;

            this.Loaded += CMainEnregistrees_Loaded;
        }

        private async void CMainEnregistrees_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                // Load suppliers
                await LoadFournisseurs();

                // Load all saved invoices
                var invoices = FactureEnregistree.GetAllSavedInvoicesAsync();

                // Map to display format
                allInvoices = invoices.Select(i => new SavedInvoiceDisplay
                {
                    SavedInvoiceID = i.SavedInvoiceID,
                    InvoiceImagePath = i.ImageFileName,
                    FournisseurID = i.FournisseurID,
                    TotalAmount = i.TotalAmount,
                    InvoiceDate = i.InvoiceDate,
                    Description = i.Description,
                    InvoiceReference = i.InvoiceReference,
                    Notes = i.Notes
                }).ToList();

                if (dgInvoices != null)
                {
                    dgInvoices.ItemsSource = allInvoices;
                    UpdateResultsCount(allInvoices?.Count ?? 0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadFournisseurs()
        {
            try
            {
                // Load fournisseurs from database
                Fournisseur fournisseur = new Fournisseur();
                var fournisseurs = await fournisseur.GetFournisseursAsync();

                var fournisseurList = fournisseurs.Select(f => new FournisseurDisplay
                {
                    FournisseurID = f.FournisseurID,
                    DisplayText = f.Nom
                }).ToList();

                cmbSupplier.ItemsSource = fournisseurList;

                // For filter dropdown, add "All Fournisseurs" option
                var filterList = new List<FournisseurDisplay>
                {
                    new FournisseurDisplay { FournisseurID = 0, DisplayText = "Tous les fournisseurs" }
                };
                filterList.AddRange(fournisseurList);
                cmbFilterSupplier.ItemsSource = filterList;
                cmbFilterSupplier.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des fournisseurs: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateResultsCount(int count)
        {
            if (txtResultsCount != null)
            {
                txtResultsCount.Text = count == 0 ? "Aucune facture trouvée" :
                    count == 1 ? "1 facture trouvée" :
                    $"{count} factures trouvées";
            }
        }

        private void UpdateButtonVisibility()
        {
            if (selectedInvoice != null)
            {
                // Hide Add button, show Update and Delete buttons
                btnAdd.Visibility = Visibility.Collapsed;
                btnUpdate.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;
            }
            else
            {
                // Show Add button, hide Update and Delete buttons
                btnAdd.Visibility = Visibility.Visible;
                btnUpdate.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
            }
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.pdf|Tous les fichiers|*.*",
                Title = "Sélectionner l'image de la facture"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Read the image as bytes
                    byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                    string fileName = Path.GetFileName(openFileDialog.FileName);

                    // Store in a temporary field or tag
                    txtImagePath.Text = openFileDialog.FileName;
                    txtImagePath.Tag = imageBytes; // Store bytes in Tag for later use
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la lecture de l'image: {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PreviewImage_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtImagePath.Text) && File.Exists(txtImagePath.Text))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = txtImagePath.Text,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ouverture de l'image: {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une image d'abord.",
                    "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void AddInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                byte[] imageBytes = txtImagePath.Tag as byte[];
                if (imageBytes == null && !string.IsNullOrEmpty(txtImagePath.Text))
                {
                    imageBytes = File.ReadAllBytes(txtImagePath.Text);
                }

                FactureEnregistree newInvoice = new FactureEnregistree
                {
                    InvoiceImage = imageBytes,
                    ImageFileName = Path.GetFileName(txtImagePath.Text),
                    FournisseurID = (int)cmbSupplier.SelectedValue,
                    TotalAmount = decimal.Parse(txtAmount.Text.Trim()),
                    InvoiceDate = dpInvoiceDate.SelectedDate ?? DateTime.Now,
                    Description = txtDescription.Text.Trim(),
                    InvoiceReference = txtReference.Text.Trim(),
                    Notes = txtNotes.Text.Trim()
                };

                bool result =  newInvoice.InsertSavedInvoiceAsync();
                if (result)
                {
                    MessageBox.Show("Facture enregistrée avec succès!", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    await LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdateInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (selectedInvoice == null)
            {
                MessageBox.Show("Veuillez sélectionner une facture à modifier.",
                    "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateForm())
                return;

            try
            {
                byte[] imageBytes = txtImagePath.Tag as byte[];
                if (imageBytes == null && !string.IsNullOrEmpty(txtImagePath.Text) && File.Exists(txtImagePath.Text))
                {
                    imageBytes = File.ReadAllBytes(txtImagePath.Text);
                }

                FactureEnregistree invoice = new FactureEnregistree
                {
                    SavedInvoiceID = selectedInvoice.SavedInvoiceID,
                    InvoiceImage = imageBytes ?? selectedInvoice.InvoiceImage,
                    ImageFileName = !string.IsNullOrEmpty(txtImagePath.Text) ? Path.GetFileName(txtImagePath.Text) : selectedInvoice.ImageFileName,
                    FournisseurID = (int)cmbSupplier.SelectedValue,
                    TotalAmount = decimal.Parse(txtAmount.Text.Trim()),
                    InvoiceDate = dpInvoiceDate.SelectedDate ?? DateTime.Now,
                    Description = txtDescription.Text.Trim(),
                    InvoiceReference = txtReference.Text.Trim(),
                    Notes = txtNotes.Text.Trim()
                };

                bool result = invoice.UpdateSavedInvoiceAsync();
                if (result)
                {
                    MessageBox.Show("Facture modifiée avec succès!", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    await LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la modification: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (selectedInvoice == null)
            {
                MessageBox.Show("Veuillez sélectionner une facture à supprimer.",
                    "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer cette facture?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    FactureEnregistree invoice = new FactureEnregistree { SavedInvoiceID = selectedInvoice.SavedInvoiceID };
                    bool deleteResult =  invoice.DeleteSavedInvoiceAsync();
                    if (deleteResult)
                    {
                        MessageBox.Show("Facture supprimée avec succès!", "Succès",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        ClearForm();
                        await LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression: {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            await LoadData();
        }

        private void ResetForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }
        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            await PerformSearch();
        }

        private async void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            await PerformSearch();
        }

        private async Task PerformSearch()
        {
            try
            {
                if (allInvoices == null)
                    allInvoices = new List<SavedInvoiceDisplay>();

                string searchText = txtSearch.Text.Trim().ToLower();

                if (string.IsNullOrEmpty(searchText))
                {
                    if (dgInvoices != null)
                    {
                        dgInvoices.ItemsSource = allInvoices;
                        UpdateResultsCount(allInvoices?.Count ?? 0);
                    }
                }
                else
                {
                    var results = allInvoices.Where(i =>
                        (i.InvoiceReference != null && i.InvoiceReference.ToLower().Contains(searchText)) ||
                        (i.Description != null && i.Description.ToLower().Contains(searchText)) ||
                        (i.FournisseurID.ToString().Contains(searchText))
                    ).ToList();

                    if (dgInvoices != null)
                    {
                        dgInvoices.ItemsSource = results;
                        UpdateResultsCount(results?.Count ?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la recherche: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void FilterSupplier_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded || allInvoices == null || cmbFilterSupplier.SelectedValue == null)
                return;

            try
            {
                int fournisseurID = (int)cmbFilterSupplier.SelectedValue;

                if (fournisseurID == 0) // All fournisseurs
                {
                    dgInvoices.ItemsSource = allInvoices;
                    UpdateResultsCount(allInvoices?.Count ?? 0);
                }
                else
                {
                    var filtered = allInvoices.Where(i => i.FournisseurID == fournisseurID).ToList();
                    dgInvoices.ItemsSource = filtered;
                    UpdateResultsCount(filtered.Count);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du filtrage: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgInvoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgInvoices.SelectedItem != null)
            {
                var selected = dgInvoices.SelectedItem as SavedInvoiceDisplay;
                if (selected != null)
                {
                    // Load the full invoice from database to get the image bytes
                    selectedInvoice =  FactureEnregistree.GetById(selected.SavedInvoiceID);
                    PopulateForm(selected);
                    UpdateButtonVisibility();
                }
            }
        }

        private void dgInvoices_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (selectedInvoice != null && selectedInvoice.InvoiceImage != null)
            {
                ViewInvoiceImage(selectedInvoice.InvoiceImage);
            }
        }

        private void ViewImage_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var invoiceDisplay = ((FrameworkElement)sender).DataContext as SavedInvoiceDisplay;

            if (invoiceDisplay != null)
            {
                // Load the full invoice to get the image
                var invoice = FactureEnregistree.GetById(invoiceDisplay.SavedInvoiceID);
                if (invoice != null && invoice.InvoiceImage != null)
                {
                    ViewInvoiceImage(invoice.InvoiceImage);
                }
                else
                {
                    MessageBox.Show("Aucune image disponible pour cette facture.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ViewInvoiceImage(byte[] imageBytes)
        {
            try
            {
                // Create a temporary file to display the image
                string tempPath = Path.Combine(Path.GetTempPath(), $"invoice_{Guid.NewGuid()}.jpg");
                File.WriteAllBytes(tempPath, imageBytes);

                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture de l'image: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateForm(SavedInvoiceDisplay invoice)
        {
            if (invoice == null) return;

            txtReference.Text = invoice.InvoiceReference;
            cmbSupplier.SelectedValue = invoice.FournisseurID;
            txtAmount.Text = invoice.TotalAmount.ToString("F2");
            dpInvoiceDate.SelectedDate = invoice.InvoiceDate;
            txtImagePath.Text = invoice.InvoiceImagePath ?? "Image enregistrée dans la base de données";
            txtImagePath.Tag = null; // Clear tag since we're loading existing
            txtDescription.Text = invoice.Description;
            txtNotes.Text = invoice.Notes;
        }

        private void ClearForm()
        {
            selectedInvoice = null;
            txtReference.Clear();
            cmbSupplier.SelectedIndex = -1;
            txtAmount.Clear();
            dpInvoiceDate.SelectedDate = DateTime.Now;
            txtImagePath.Clear();
            txtImagePath.Tag = null; // Clear the image bytes tag
            txtDescription.Clear();
            txtNotes.Clear();
            dgInvoices.SelectedItem = null;
            UpdateButtonVisibility();
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtImagePath.Text))
            {
                MessageBox.Show("Veuillez sélectionner une image de la facture.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbSupplier.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un fournisseur.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbSupplier.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAmount.Text))
            {
                MessageBox.Show("Veuillez entrer le montant total.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
                return false;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Veuillez entrer un montant valide.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
                return false;
            }

            if (dpInvoiceDate.SelectedDate == null)
            {
                MessageBox.Show("Veuillez sélectionner une date.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpInvoiceDate.Focus();
                return false;
            }

            return true;
        }
    }
}