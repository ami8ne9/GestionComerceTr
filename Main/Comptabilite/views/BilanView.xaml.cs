using GestionComerce;
using GestionComerce.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Superete.Main.Comptabilite.Views
{
    public partial class BilanView : UserControl
    {
        private User currentUser;
        private ComptabiliteService comptabiliteService;
        private BilanDTO currentBilan;

        public BilanView(User u)
        {
            InitializeComponent();
            currentUser = u;
            comptabiliteService = new ComptabiliteService();
            DateBilan.SelectedDate = DateTime.Now;
            LoadBilan();
        }

        private void LoadBilan()
        {
            try
            {
                DateTime dateBilan = DateBilan.SelectedDate ?? DateTime.Now;
                currentBilan = comptabiliteService.GenererBilan(dateBilan);

                TxtPeriode.Text = $"Au {dateBilan:dd/MM/yyyy}";

                ActifGrid.ItemsSource = currentBilan.Actifs.OrderBy(a => a.CodeCompte);
                PassifGrid.ItemsSource = currentBilan.Passifs.OrderBy(p => p.CodeCompte);

                TxtTotalActif.Text = $"{currentBilan.TotalActif:N2} DH";
                TxtTotalPassif.Text = $"{currentBilan.TotalPassif:N2} DH";

                decimal difference = Math.Abs(currentBilan.TotalActif - currentBilan.TotalPassif);
                if (difference < 0.01m)
                {
                    TxtEquilibre.Text = "✅ Bilan équilibré";
                    TxtEquilibre.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#48BB78"));
                }
                else
                {
                    TxtEquilibre.Text = $"⚠️ Déséquilibre: {difference:N2} DH";
                    TxtEquilibre.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F56565"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DateBilan_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadBilan();
        }

        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem != null)
            {
                var ligne = grid.SelectedItem as BilanLigneDTO;
                MessageBox.Show($"Compte: {ligne.CodeCompte}\nLibellé: {ligne.Libelle}\nMontant: {ligne.Montant:N2} DH",
                    "Détails", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create print dialog
                PrintDialog printDialog = new PrintDialog();

                if (printDialog.ShowDialog() != true) return;

                // Create FlowDocument for printing
                var flowDoc = new System.Windows.Documents.FlowDocument();
                flowDoc.PagePadding = new Thickness(50);
                flowDoc.ColumnWidth = printDialog.PrintableAreaWidth;

                // Title
                var titlePara = new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run("BILAN COMPTABLE"))
                {
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                flowDoc.Blocks.Add(titlePara);

                // Date
                var datePara = new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run(TxtPeriode.Text))
                {
                    FontSize = 14,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                flowDoc.Blocks.Add(datePara);

                // ACTIF Table
                var actifHeader = new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run("ACTIF"))
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                };
                flowDoc.Blocks.Add(actifHeader);

                var actifTable = CreateBilanTable(currentBilan.Actifs.OrderBy(a => a.CodeCompte).ToList());
                flowDoc.Blocks.Add(actifTable);

                // Total Actif
                var totalActifPara = new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run($"TOTAL ACTIF: {currentBilan.TotalActif:N2} DH"))
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(0, 5, 0, 20)
                };
                flowDoc.Blocks.Add(totalActifPara);

                // PASSIF Table
                var passifHeader = new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run("PASSIF"))
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                };
                flowDoc.Blocks.Add(passifHeader);

                var passifTable = CreateBilanTable(currentBilan.Passifs.OrderBy(p => p.CodeCompte).ToList());
                flowDoc.Blocks.Add(passifTable);

                // Total Passif
                var totalPassifPara = new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run($"TOTAL PASSIF: {currentBilan.TotalPassif:N2} DH"))
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                flowDoc.Blocks.Add(totalPassifPara);

                // Print
                System.Windows.Documents.IDocumentPaginatorSource idpSource = flowDoc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Bilan Comptable");

                MessageBox.Show("Impression lancée!", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'impression:\n\n{ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper method for creating tables in print document
        private System.Windows.Documents.Table CreateBilanTable(List<BilanLigneDTO> lignes)
        {
            var table = new System.Windows.Documents.Table
            {
                CellSpacing = 0,
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1)
            };

            // Define columns
            table.Columns.Add(new System.Windows.Documents.TableColumn { Width = new GridLength(100) });
            table.Columns.Add(new System.Windows.Documents.TableColumn { Width = new GridLength(300) });
            table.Columns.Add(new System.Windows.Documents.TableColumn { Width = new GridLength(150) });

            var rowGroup = new System.Windows.Documents.TableRowGroup();

            // Header row
            var headerRow = new System.Windows.Documents.TableRow
            {
                Background = System.Windows.Media.Brushes.LightGray
            };

            headerRow.Cells.Add(CreateTableCell("Code", true));
            headerRow.Cells.Add(CreateTableCell("Libellé", true));
            headerRow.Cells.Add(CreateTableCell("Montant (DH)", true));

            rowGroup.Rows.Add(headerRow);

            // Data rows
            foreach (var ligne in lignes)
            {
                var dataRow = new System.Windows.Documents.TableRow();
                dataRow.Cells.Add(CreateTableCell(ligne.CodeCompte, false));
                dataRow.Cells.Add(CreateTableCell(ligne.Libelle, false));
                dataRow.Cells.Add(CreateTableCell($"{ligne.Montant:N2}", false, TextAlignment.Right));
                rowGroup.Rows.Add(dataRow);
            }

            table.RowGroups.Add(rowGroup);
            return table;
        }

        // Helper method for creating table cells
        private System.Windows.Documents.TableCell CreateTableCell(string text, bool isBold, TextAlignment alignment = TextAlignment.Left)
        {
            var cell = new System.Windows.Documents.TableCell(
                new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(text)))
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(5)
            };

            if (isBold)
                cell.FontWeight = FontWeights.Bold;

            cell.TextAlignment = alignment;

            return cell;
        }

        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"Bilan_{DateTime.Now:yyyyMMdd}.xlsx",
                    DefaultExt = ".xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Bilan");

                        // Title
                        worksheet.Cells["A1"].Value = "BILAN COMPTABLE";
                        worksheet.Cells["A1:D1"].Merge = true;
                        worksheet.Cells["A1"].Style.Font.Size = 16;
                        worksheet.Cells["A1"].Style.Font.Bold = true;
                        worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        worksheet.Cells["A2"].Value = TxtPeriode.Text;
                        worksheet.Cells["A2:D2"].Merge = true;
                        worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        int row = 4;

                        // ACTIF Header
                        worksheet.Cells[row, 1].Value = "ACTIF";
                        worksheet.Cells[row, 1, row, 2].Merge = true;
                        worksheet.Cells[row, 1].Style.Font.Bold = true;

                        // PASSIF Header
                        worksheet.Cells[row, 4].Value = "PASSIF";
                        worksheet.Cells[row, 4, row, 5].Merge = true;
                        worksheet.Cells[row, 4].Style.Font.Bold = true;

                        row++;

                        // Column Headers
                        worksheet.Cells[row, 1].Value = "Code";
                        worksheet.Cells[row, 2].Value = "Libellé";
                        worksheet.Cells[row, 3].Value = "Montant (DH)";
                        worksheet.Cells[row, 4].Value = "Code";
                        worksheet.Cells[row, 5].Value = "Libellé";
                        worksheet.Cells[row, 6].Value = "Montant (DH)";

                        for (int i = 1; i <= 6; i++)
                        {
                            worksheet.Cells[row, i].Style.Font.Bold = true;
                        }

                        row++;
                        int startDataRow = row;

                        // Data - ACTIF and PASSIF side by side
                        int maxRows = Math.Max(currentBilan.Actifs.Count, currentBilan.Passifs.Count);

                        for (int i = 0; i < maxRows; i++)
                        {
                            // ACTIF
                            if (i < currentBilan.Actifs.Count)
                            {
                                var actif = currentBilan.Actifs.OrderBy(a => a.CodeCompte).ToList()[i];
                                worksheet.Cells[row, 1].Value = actif.CodeCompte;
                                worksheet.Cells[row, 2].Value = actif.Libelle;
                                worksheet.Cells[row, 3].Value = actif.Montant;
                                worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                            }

                            // PASSIF
                            if (i < currentBilan.Passifs.Count)
                            {
                                var passif = currentBilan.Passifs.OrderBy(p => p.CodeCompte).ToList()[i];
                                worksheet.Cells[row, 4].Value = passif.CodeCompte;
                                worksheet.Cells[row, 5].Value = passif.Libelle;
                                worksheet.Cells[row, 6].Value = passif.Montant;
                                worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                            }

                            row++;
                        }

                        // Totals
                        row++;
                        worksheet.Cells[row, 2].Value = "TOTAL ACTIF";
                        worksheet.Cells[row, 2].Style.Font.Bold = true;
                        worksheet.Cells[row, 3].Value = $"=SUM(C{startDataRow}:C{row - 1})";
                        worksheet.Cells[row, 3].Style.Font.Bold = true;
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";

                        worksheet.Cells[row, 5].Value = "TOTAL PASSIF";
                        worksheet.Cells[row, 5].Style.Font.Bold = true;
                        worksheet.Cells[row, 6].Value = $"=SUM(F{startDataRow}:F{row - 1})";
                        worksheet.Cells[row, 6].Style.Font.Bold = true;
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";

                        // Auto-fit columns
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        // Add borders
                        var dataRange = worksheet.Cells[4, 1, row, 6];
                        dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        // Save
                        package.SaveAs(new FileInfo(saveDialog.FileName));
                    }

                    MessageBox.Show($"Bilan exporté avec succès !\n\n{saveDialog.FileName}",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'export Excel:\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}