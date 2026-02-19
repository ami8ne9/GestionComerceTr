using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GestionComerce;

namespace Superete.Main.Comptabilite
{
    public partial class Expenses : UserControl
    {
        // Alert settings
        private int alertValue = 5;
        private string alertUnit = "days"; // hours, days, weeks

        // Recurring types list
        private List<string> recurringTypes = new List<string>
        {
            "Une fois",
            "Quotidien",
            "Hebdomadaire",
            "Mensuel",
            "Trimestriel",
            "Semestriel",
            "Annuel"
        };

        public Expenses()
        {
            InitializeComponent();

            // Test database connection
            if (!DBHelper.TestConnection())
            {
                MessageBox.Show("❌ Impossible de se connecter à la base de données.\n\nVeuillez vérifier:\n• SQL Server est démarré\n• La chaîne de connexion est correcte\n• Les autorisations sont valides",
                    "Erreur de connexion", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            InitializeForm();
            LoadCategories();
            LoadRecurringTypes();
            LoadExpenses();
            CheckUpcomingPayments();
        }

        #region Initialization

        private void InitializeForm()
        {
            // Load alert settings from user preferences
            txtAlertValue.Text = alertValue.ToString();

            // Set default selected unit
            foreach (ComboBoxItem item in cmbAlertUnit.Items)
            {
                if (item.Tag.ToString() == alertUnit)
                {
                    cmbAlertUnit.SelectedItem = item;
                    break;
                }
            }

            // Set default date to today
            dpDueDate.SelectedDate = DateTime.Today;
        }

        private void LoadCategories()
        {
            try
            {
                List<ExpenseCategories> categories = ExpenseCategories.GetAllActive();

                cmbCategory.Items.Clear();
                cmbFilterCategory.Items.Clear();

                cmbFilterCategory.Items.Add(new ComboBoxItem { Content = "Toutes" });

                foreach (var category in categories)
                {
                    cmbCategory.Items.Add(category.CategoryName);
                    cmbFilterCategory.Items.Add(new ComboBoxItem { Content = category.CategoryName });
                }

                if (cmbCategory.Items.Count > 0)
                    cmbCategory.SelectedIndex = 0;

                cmbFilterCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors du chargement des catégories:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecurringTypes()
        {
            cmbRecurring.Items.Clear();
            foreach (var type in recurringTypes)
            {
                cmbRecurring.Items.Add(type);
            }
            cmbRecurring.SelectedIndex = 0;
        }

        #endregion

        #region Data Loading

        private void LoadExpenses()
        {
            try
            {
                List<GestionComerce.Expenses> expenses = GestionComerce.Expenses.GetAll();

                // Update status for overdue expenses
                foreach (var expense in expenses)
                {
                    if (expense.PaymentStatus == "Pending" && expense.DueDate < DateTime.Today)
                    {
                        expense.PaymentStatus = "Overdue";
                    }
                }

                dgExpenses.ItemsSource = null;
                dgExpenses.ItemsSource = expenses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors du chargement des dépenses:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckUpcomingPayments()
        {
            try
            {
                // Calculate days ahead based on alert settings
                int daysAhead = CalculateDaysAhead();

                List<GestionComerce.Expenses> upcomingExpenses = GestionComerce.Expenses.GetUpcoming(daysAhead);

                if (upcomingExpenses.Count > 0)
                {
                    AlertBanner.Visibility = Visibility.Visible;
                    AlertList.ItemsSource = upcomingExpenses;

                    // Update alert info text
                    string unitText = alertUnit == "hours" ? "heures" :
                                     alertUnit == "days" ? "jours" : "semaines";
                    txtAlertInfo.Text = $"Alerte: {alertValue} {unitText} avant échéance";
                }
                else
                {
                    AlertBanner.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors de la vérification des paiements:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CalculateDaysAhead()
        {
            switch (alertUnit)
            {
                case "hours":
                    return Math.Max(1, alertValue / 24);
                case "weeks":
                    return alertValue * 7;
                case "days":
                default:
                    return alertValue;
            }
        }

        #endregion

        #region Form Validation

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtExpenseName.Text))
            {
                MessageBox.Show("⚠️ Veuillez entrer le nom de la dépense.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtExpenseName.Focus();
                return false;
            }

            if (cmbCategory.SelectedItem == null && string.IsNullOrWhiteSpace(cmbCategory.Text))
            {
                MessageBox.Show("⚠️ Veuillez sélectionner ou entrer une catégorie.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbCategory.Focus();
                return false;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("⚠️ Veuillez entrer un montant valide supérieur à zéro.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
                return false;
            }

            if (!dpDueDate.SelectedDate.HasValue)
            {
                MessageBox.Show("⚠️ Veuillez sélectionner une date d'échéance.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDueDate.Focus();
                return false;
            }

            return true;
        }

        #endregion

        #region Event Handlers - Alert Settings

        private void BtnAlertSettings_Click(object sender, RoutedEventArgs e)
        {
            AlertSettingsCard.Visibility = AlertSettingsCard.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void BtnSaveAlertSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtAlertValue.Text, out int value) || value <= 0)
            {
                MessageBox.Show("⚠️ Veuillez entrer une valeur numérique valide supérieure à zéro.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ComboBoxItem selectedItem = cmbAlertUnit.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
            {
                MessageBox.Show("⚠️ Veuillez sélectionner une unité de temps.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            alertValue = value;
            alertUnit = selectedItem.Tag.ToString();

            MessageBox.Show($"✅ Paramètres d'alerte sauvegardés!\n\nAlerte: {alertValue} {selectedItem.Content} avant l'échéance.",
                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

            AlertSettingsCard.Visibility = Visibility.Collapsed;
            CheckUpcomingPayments();
        }

        #endregion

        #region Event Handlers - Add Expense

        private void BtnAddExpense_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                string categoryText = cmbCategory.SelectedItem != null
                    ? cmbCategory.SelectedItem.ToString()
                    : cmbCategory.Text.Trim();

                string recurringText = cmbRecurring.SelectedItem != null
                    ? cmbRecurring.SelectedItem.ToString()
                    : cmbRecurring.Text.Trim();

                GestionComerce.Expenses newExpense = new GestionComerce.Expenses
                {
                    ExpenseName = txtExpenseName.Text.Trim(),
                    Category = categoryText,
                    Amount = decimal.Parse(txtAmount.Text),
                    DueDate = dpDueDate.SelectedDate.Value,
                    RecurringType = recurringText,
                    Notes = txtNotes.Text.Trim()
                };

                bool success = newExpense.Add();

                if (success)
                {
                    MessageBox.Show("✅ Dépense ajoutée avec succès!",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    LoadExpenses();
                    CheckUpcomingPayments();
                }
                else
                {
                    MessageBox.Show("❌ Échec de l'ajout de la dépense.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors de l'ajout:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtExpenseName.Clear();
            txtAmount.Clear();
            txtNotes.Clear();
            dpDueDate.SelectedDate = DateTime.Today;

            if (cmbCategory.Items.Count > 0)
                cmbCategory.SelectedIndex = 0;

            cmbRecurring.SelectedIndex = 0;
            cmbPaymentMethod.SelectedIndex = 0;

            txtExpenseName.Focus();
        }

        #endregion

        #region Event Handlers - Category Management

        private void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddCategoryWindow();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ExpenseCategories newCategory = new ExpenseCategories
                    {
                        CategoryName = dialog.CategoryName,
                        Description = dialog.CategoryDescription,
                        IsActive = true
                    };

                    bool success = newCategory.Add();

                    if (success)
                    {
                        MessageBox.Show("✅ Catégorie ajoutée avec succès!",
                            "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadCategories();
                        cmbCategory.Text = dialog.CategoryName;
                    }
                    else
                    {
                        MessageBox.Show("❌ Échec de l'ajout de la catégorie.",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Erreur:\n\n{ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Event Handlers - Recurrence Management

        private void BtnAddRecurrence_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddRecurrenceWindow();
            if (dialog.ShowDialog() == true)
            {
                string newRecurrence = dialog.RecurrenceName;

                if (!recurringTypes.Contains(newRecurrence))
                {
                    recurringTypes.Add(newRecurrence);
                    cmbRecurring.Items.Add(newRecurrence);
                    cmbRecurring.Text = newRecurrence;

                    MessageBox.Show("✅ Type de récurrence ajouté avec succès!",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("⚠️ Ce type de récurrence existe déjà.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        #endregion

        #region Event Handlers - Expense Actions

        private void BtnPayExpense_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag == null) return;

            GestionComerce.Expenses expense = btn.Tag as GestionComerce.Expenses;
            if (expense == null) return;

            if (expense.PaymentStatus == "Paid")
            {
                MessageBox.Show("ℹ️ Cette dépense a déjà été payée.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"💰 Confirmer le paiement de:\n\n" +
                $"Dépense: {expense.ExpenseName}\n" +
                $"Montant: {expense.Amount:N2} MAD\n" +
                $"Catégorie: {expense.Category}\n\n" +
                $"Voulez-vous marquer cette dépense comme payée?",
                "Confirmation de paiement",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                string paymentMethod = cmbPaymentMethod.SelectedItem != null
                    ? ((ComboBoxItem)cmbPaymentMethod.SelectedItem).Content.ToString()
                    : "Cash";

                bool success = expense.MarkAsPaid(paymentMethod);

                if (success)
                {
                    MessageBox.Show("✅ Paiement enregistré avec succès!",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadExpenses();
                    CheckUpcomingPayments();
                }
                else
                {
                    MessageBox.Show("❌ Échec de l'enregistrement du paiement.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors du paiement:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditExpense_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag == null) return;

            GestionComerce.Expenses expense = btn.Tag as GestionComerce.Expenses;
            if (expense == null) return;

            var dialog = new EditExpenseWindow(expense);
            if (dialog.ShowDialog() == true)
            {
                LoadExpenses();
                CheckUpcomingPayments();
            }
        }

        private void BtnDeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag == null) return;

            GestionComerce.Expenses expense = btn.Tag as GestionComerce.Expenses;
            if (expense == null) return;

            var result = MessageBox.Show(
                $"⚠️ Êtes-vous sûr de vouloir supprimer cette dépense?\n\n" +
                $"Nom: {expense.ExpenseName}\n" +
                $"Montant: {expense.Amount:N2} MAD\n\n" +
                $"Cette action est irréversible!",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool success = expense.Delete();

                if (success)
                {
                    MessageBox.Show("✅ Dépense supprimée avec succès!",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadExpenses();
                    CheckUpcomingPayments();
                }
                else
                {
                    MessageBox.Show("❌ Échec de la suppression de la dépense.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors de la suppression:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Event Handlers - Filtering

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterPanel.Visibility = FilterPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadExpenses();
            CheckUpcomingPayments();
            MessageBox.Show("✅ Données actualisées!",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<GestionComerce.Expenses> allExpenses = GestionComerce.Expenses.GetAll();
                List<GestionComerce.Expenses> filteredExpenses = allExpenses;

                // Filter by status
                ComboBoxItem statusItem = cmbFilterStatus.SelectedItem as ComboBoxItem;
                if (statusItem != null && statusItem.Content.ToString() != "Tous")
                {
                    string status = statusItem.Content.ToString();
                    filteredExpenses = filteredExpenses.Where(exp => exp.PaymentStatus == status).ToList();
                }

                // Filter by category
                ComboBoxItem categoryItem = cmbFilterCategory.SelectedItem as ComboBoxItem;
                if (categoryItem != null && categoryItem.Content.ToString() != "Toutes")
                {
                    string category = categoryItem.Content.ToString();
                    filteredExpenses = filteredExpenses.Where(exp => exp.Category == category).ToList();
                }

                // Filter by period
                ComboBoxItem periodItem = cmbFilterPeriod.SelectedItem as ComboBoxItem;
                if (periodItem != null)
                {
                    string period = periodItem.Content.ToString();
                    DateTime now = DateTime.Now;

                    switch (period)
                    {
                        case "Ce mois":
                            filteredExpenses = filteredExpenses.Where(exp =>
                                exp.DueDate.Year == now.Year && exp.DueDate.Month == now.Month).ToList();
                            break;
                        case "Ce trimestre":
                            int currentQuarter = (now.Month - 1) / 3 + 1;
                            filteredExpenses = filteredExpenses.Where(exp =>
                            {
                                int expenseQuarter = (exp.DueDate.Month - 1) / 3 + 1;
                                return exp.DueDate.Year == now.Year && expenseQuarter == currentQuarter;
                            }).ToList();
                            break;
                        case "Cette année":
                            filteredExpenses = filteredExpenses.Where(exp =>
                                exp.DueDate.Year == now.Year).ToList();
                            break;
                    }
                }

                dgExpenses.ItemsSource = null;
                dgExpenses.ItemsSource = filteredExpenses;

                MessageBox.Show($"✅ Filtre appliqué!\n\n{filteredExpenses.Count} dépense(s) trouvée(s).",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors du filtrage:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Input Validation

        private void TxtAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            string newText = txtAmount.Text.Insert(txtAmount.CaretIndex, e.Text);
            e.Handled = !regex.IsMatch(newText);
        }

        #endregion
    }
}