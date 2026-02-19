using GestionComerce.Main.Vente;
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
using System.Windows.Shapes;

namespace GestionComerce.Main.Inventory
{
    /// <summary>
    /// Interaction logic for WConfirmTransaction.xaml
    /// </summary>
    public partial class WConfirmTransaction : Window
    {
        public WConfirmTransaction(WAddArticle ar, WAjoutQuantite aq, WAddMultipleArticles ama, Article a, int s, int methodID)
        {
            InitializeComponent();
            this.ar = ar;
            this.aq = aq;
            this.s = s;
            this.a = a;
            this.ama = ama;
            this.methodID = methodID;
            if (s != 1)
            {
                CreditColumn.Width = new GridLength(0);
                CreditStack.Visibility = Visibility.Collapsed;
            }
            if (aq != null)
            {
                NbrArticle.Text = aq.qte.ToString();
                Subtotal.Text = (a.PrixAchat * aq.qte).ToString("0.00") + " DH";
                FinalTotal.Text = (a.PrixAchat * aq.qte).ToString("0.00") + " DH";
            }
            if (ar != null)
            {
                NbrArticle.Text = a.Quantite.ToString();
                Subtotal.Text = (a.PrixAchat * a.Quantite).ToString("0.00") + " DH";
                FinalTotal.Text = (a.PrixAchat * a.Quantite).ToString("0.00") + " DH";
            }
            if (ama != null)
            {
                for (int i = ama.ArticlesContainer.Children.Count - 1; i >= 0; i--)
                {
                    if (ama.ArticlesContainer.Children[i] is CSingleRowArticle csra)
                    {
                        csra.Quantite.Text = csra.Quantite.Text.Replace("x", "");
                        NbrArticleTotal += Convert.ToInt32(csra.Quantite.Text);
                        Subtotall += csra.a.PrixAchat * Convert.ToInt32(csra.Quantite.Text);
                        FinalTotall += csra.a.PrixAchat * Convert.ToInt32(csra.Quantite.Text);
                    }
                }
                NbrArticle.Text = NbrArticleTotal.ToString();
                Subtotal.Text = (Subtotall).ToString("0.00") + " DH";
                FinalTotal.Text = (FinalTotall).ToString("0.00") + " DH";
            }
        }

        WAddArticle ar;
        int s;
        WAjoutQuantite aq;
        Article a;
        WAddMultipleArticles ama;
        int NbrArticleTotal = 0;
        int methodID;
        decimal Subtotall = 0;
        decimal FinalTotall = 0;

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DecimalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            // Autorise les chiffres et un seul point
            if (e.Text == ".")
            {
                // Refuse si déjà un point
                e.Handled = textBox.Text.Contains(".");
            }
            else
            {
                e.Handled = !e.Text.All(char.IsDigit);
            }
        }

        private void DecimalTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                // Autorise un seul point et le reste chiffres
                int dotCount = text.Count(c => c == '.');
                if (dotCount > 1 || text.Any(c => !char.IsDigit(c) && c != '.'))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Remise.Text == "0.00") Remise.Text = "";
                FinalTotal.Text = FinalTotal.Text.Replace("DH", "").Trim();
                Remise.Text = Remise.Text.Replace("DH", "").Trim();
                Remise.Text = Remise.Text.Replace("-", "");
                if (Convert.ToDecimal(FinalTotal.Text) < 0)
                {
                    MessageBox.Show("le total final ne peux pas etre negative");
                    Remise.Text = "-" + Remise.Text + " DH";
                    FinalTotal.Text = FinalTotal.Text + " DH";
                    return;
                }

                //New Article
                if (ar != null)
                {
                    if (s == 0)
                    {
                        Operation Operation = new Operation();
                        Operation.PaymentMethodID = methodID;
                        Operation.OperationType = "AchatCa";
                        Operation.PrixOperation = a.PrixAchat * a.Quantite;
                        if (Remise.Text != "")
                        {
                            Operation.Remise = Convert.ToDecimal(Remise.Text);
                            if (Operation.Remise > Operation.PrixOperation)
                            {
                                MessageBox.Show("la remise est plus grande que le total.");
                                return;
                            }
                        }

                        Operation.UserID = ar.main.u.UserID;
                        Operation.FournisseurID = a.FournisseurID;
                        int idd = await Operation.InsertOperationAsync();

                        OperationArticle ofa = new OperationArticle();

                        int id = await a.InsertArticleAsync();
                        a.ArticleID = id;

                        ofa.ArticleID = a.ArticleID;
                        ofa.OperationID = idd;
                        ofa.QteArticle = Convert.ToInt32(a.Quantite);
                        await ofa.InsertOperationArticleAsync();

                        // FORCE RELOAD FROM DATABASE
                        Article articleService = new Article();
                        List<Article> refreshedArticles = await articleService.GetArticlesAsync();

                        // Update the main list with fresh data
                        ar.main.la = refreshedArticles;

                        // Refresh the UI
                        ar.main.LoadArticles(refreshedArticles);

                        // Show success message
                        WCongratulations wCongratulations = new WCongratulations("Opération réussie", "Article ajouté avec succès", 1);
                        wCongratulations.ShowDialog();

                        // Close windows
                        ar.Close();
                        this.Close();
                        return;
                    }
                    else if (s == 1)
                    {
                        if (Convert.ToDecimal(CreditInput.Text) == 0)
                        {
                            MessageBox.Show("Doneer un valeur de credit.");
                            return;
                        }

                        if (Remise.Text != "")
                        {
                            if (Convert.ToDecimal(CreditInput.Text) > Convert.ToDecimal(a.PrixAchat * a.Quantite) - Convert.ToDecimal(Remise.Text))
                            {
                                MessageBox.Show("la valeur de credit est plus grande que le total mois la remise.");
                                return;
                            }
                        }
                        else
                        {
                            if (Convert.ToDecimal(CreditInput.Text) > Convert.ToDecimal(a.PrixAchat * a.Quantite))
                            {
                                MessageBox.Show("la valeur de credit est plus grande que le total.");
                                return;
                            }
                        }
                        int creditId = 0;
                        bool creditExists = false;
                        Credit Credit = new Credit();
                        List<Credit> lff = await Credit.GetCreditsAsync();
                        foreach (Credit ff in lff)
                        {
                            if (ff.FournisseurID == a.FournisseurID)
                            {
                                ff.Total += Convert.ToDecimal(CreditInput.Text);
                                await ff.UpdateCreditAsync();
                                creditExists = true;
                                creditId = ff.CreditID;
                                break;
                            }
                        }
                        if (!creditExists)
                        {
                            Credit newCredit = new Credit();
                            newCredit.FournisseurID = a.FournisseurID;
                            newCredit.Total = Convert.ToDecimal(CreditInput.Text);
                            creditId = await newCredit.InsertCreditAsync();
                        }

                        Operation Operation = new Operation();
                        Operation.PaymentMethodID = methodID;
                        Operation.OperationType = "Achat50";
                        Operation.PrixOperation = (a.PrixAchat * a.Quantite);
                        Operation.CreditValue = Convert.ToDecimal(CreditInput.Text);
                        Operation.CreditID = creditId;
                        if (Remise.Text != "")
                        {
                            Operation.Remise = Convert.ToDecimal(Remise.Text);
                        }

                        Operation.UserID = ar.main.u.UserID;
                        Operation.FournisseurID = a.FournisseurID;

                        int idd = await Operation.InsertOperationAsync();
                        OperationArticle ofa = new OperationArticle();
                        int id = await a.InsertArticleAsync();
                        a.ArticleID = id;
                        ofa.ArticleID = a.ArticleID;
                        ofa.OperationID = idd;
                        ofa.QteArticle = Convert.ToInt32(a.Quantite);
                        await ofa.InsertOperationArticleAsync();

                        // FORCE RELOAD FROM DATABASE
                        Article articleService = new Article();
                        List<Article> refreshedArticles = await articleService.GetArticlesAsync();

                        // Update the main list with fresh data
                        ar.main.la = refreshedArticles;

                        // Refresh the UI
                        ar.main.LoadArticles(refreshedArticles);

                        // Show success message
                        WCongratulations wCongratulations = new WCongratulations("Opération réussie", "Article ajouté avec succès", 1);
                        wCongratulations.ShowDialog();

                        // Close windows
                        ar.Close();
                        this.Close();
                        return;
                    }
                    else
                    {
                        if (Remise.Text != "")
                        {
                            if (Convert.ToDecimal(Remise.Text) > Convert.ToDecimal(a.PrixAchat * a.Quantite))
                            {
                                MessageBox.Show("la remise est plus grande que le total.");
                                return;
                            }
                        }
                        int creditId = 0;
                        bool creditExists = false;
                        Credit Credit = new Credit();
                        List<Credit> lcc = await Credit.GetCreditsAsync();
                        Operation Operation = new Operation();
                        Operation.PaymentMethodID = methodID;
                        foreach (Credit cf in lcc)
                        {
                            if (cf.FournisseurID == a.FournisseurID)
                            {
                                if (Remise.Text != "")
                                {
                                    cf.Total += Convert.ToDecimal(a.PrixAchat * a.Quantite) - Convert.ToDecimal(Remise.Text);
                                    Operation.CreditValue = Convert.ToDecimal(a.PrixAchat * a.Quantite) - Convert.ToDecimal(Remise.Text);
                                }
                                else
                                {
                                    cf.Total += Convert.ToDecimal(a.PrixAchat * a.Quantite);
                                    Operation.CreditValue = Convert.ToDecimal(a.PrixAchat * a.Quantite);
                                }
                                await cf.UpdateCreditAsync();
                                creditExists = true;
                                creditId = cf.CreditID;
                                break;
                            }
                        }
                        if (!creditExists)
                        {
                            Credit newCredit = new Credit();
                            newCredit.FournisseurID = a.FournisseurID;
                            if (Remise.Text != "")
                            {
                                newCredit.Total += Convert.ToDecimal(a.PrixAchat * a.Quantite) - Convert.ToDecimal(Remise.Text);
                                Operation.CreditValue = Convert.ToDecimal(a.PrixAchat * a.Quantite) - Convert.ToDecimal(Remise.Text);
                            }
                            else
                            {
                                newCredit.Total += Convert.ToDecimal(a.PrixAchat * a.Quantite);
                                Operation.CreditValue = Convert.ToDecimal(a.PrixAchat * a.Quantite);
                            }
                            creditId = await newCredit.InsertCreditAsync();
                        }

                        Operation.OperationType = "AchatCr";
                        Operation.PrixOperation = a.PrixAchat * a.Quantite;
                        Operation.CreditID = creditId;
                        if (Remise.Text != "")
                        {
                            Operation.Remise = Convert.ToDecimal(Remise.Text);
                        }

                        Operation.UserID = ar.main.u.UserID;
                        Operation.FournisseurID = a.FournisseurID;

                        int idd = await Operation.InsertOperationAsync();
                        OperationArticle ofa = new OperationArticle();
                        int id = await a.InsertArticleAsync();
                        a.ArticleID = id;
                        ofa.ArticleID = a.ArticleID;
                        ofa.OperationID = idd;
                        ofa.QteArticle = Convert.ToInt32(a.Quantite);
                        await ofa.InsertOperationArticleAsync();

                        // FORCE RELOAD FROM DATABASE
                        Article articleService = new Article();
                        List<Article> refreshedArticles = await articleService.GetArticlesAsync();

                        // Update the main list with fresh data
                        ar.main.la = refreshedArticles;

                        // Refresh the UI
                        ar.main.LoadArticles(refreshedArticles);

                        // Show success message
                        WCongratulations wCongratulations = new WCongratulations("Opération réussie", "Article ajouté avec succès", 1);
                        wCongratulations.ShowDialog();

                        // Close windows
                        ar.Close();
                        this.Close();
                        return;
                    }
                }

                //Add Quantity
                if (aq != null)
                {
                    if (s == 0)
                    {
                        Operation Operation = new Operation();
                        Operation.PaymentMethodID = methodID;
                        Operation.OperationType = "AchatCa";
                        Operation.PrixOperation = a.PrixAchat * aq.qte;
                        if (Remise.Text != "")
                        {
                            Operation.Remise = Convert.ToDecimal(Remise.Text);
                            if (Operation.Remise > Operation.PrixOperation)
                            {
                                MessageBox.Show("la remise est plus grande que le total.");
                                return;
                            }
                        }

                        Operation.UserID = aq.sa.Main.u.UserID;
                        Operation.FournisseurID = a.FournisseurID;
                        int idd = await Operation.InsertOperationAsync();
                        OperationArticle ofa = new OperationArticle();

                        // Update article quantity
                        a.Quantite += aq.qte;
                        int updateResult = await a.UpdateArticleAsync();

                        if (updateResult > 0)
                        {
                            ofa.ArticleID = a.ArticleID;
                            ofa.OperationID = idd;
                            ofa.QteArticle = Convert.ToInt32(aq.qte);
                            await ofa.InsertOperationArticleAsync();

                            // FORCE RELOAD FROM DATABASE
                            Article articleService = new Article();
                            List<Article> refreshedArticles = await articleService.GetArticlesAsync();

                            // Update the main list with fresh data
                            aq.sa.Main.la = refreshedArticles;

                            // Refresh the UI
                            aq.sa.Main.LoadArticles(refreshedArticles);

                            // Show success message
                            WCongratulations wCongratulations = new WCongratulations("Opération réussie", "Quantité ajoutée avec succès", 1);
                            wCongratulations.ShowDialog();

                            // Close windows
                            aq.Close();
                            this.Close();
                        }
                        return;
                    }
                    else if (s == 1)
                    {
                        if (Convert.ToDecimal(CreditInput.Text) == 0)
                        {
                            MessageBox.Show("Doneer un valeur de credit.");
                            return;
                        }

                        if (Remise.Text != "")
                        {
                            if (Convert.ToDecimal(CreditInput.Text) > Convert.ToDecimal(a.PrixAchat * a.Quantite) - Convert.ToDecimal(Remise.Text))
                            {
                                MessageBox.Show("la valeur de credit est plus grande que le total mois la remise.");
                                return;
                            }
                        }
                        else
                        {
                            if (Convert.ToDecimal(CreditInput.Text) > Convert.ToDecimal(a.PrixAchat * a.Quantite))
                            {
                                MessageBox.Show("la valeur de credit est plus grande que le total.");
                                return;
                            }
                        }
                        int creditId = 0;
                        bool creditExists = false;
                        Credit Credit = new Credit();
                        List<Credit> lff = await Credit.GetCreditsAsync();
                        foreach (Credit ff in lff)
                        {
                            if (ff.FournisseurID == a.FournisseurID)
                            {
                                ff.Total += Convert.ToDecimal(CreditInput.Text);
                                await ff.UpdateCreditAsync();
                                creditExists = true;
                                creditId = ff.CreditID;
                                break;
                            }
                        }
                        if (!creditExists)
                        {
                            Credit newCredit = new Credit();
                            newCredit.FournisseurID = a.FournisseurID;
                            newCredit.Total = Convert.ToDecimal(CreditInput.Text);
                            creditId = await newCredit.InsertCreditAsync();
                        }

                        Operation Operation = new Operation();
                        Operation.PaymentMethodID = methodID;
                        Operation.OperationType = "Achat50";
                        Operation.PrixOperation = (a.PrixAchat * aq.qte);
                        Operation.CreditValue = Convert.ToDecimal(CreditInput.Text);
                        Operation.CreditID = creditId;
                        if (Remise.Text != "")
                        {
                            Operation.Remise = Convert.ToDecimal(Remise.Text);
                        }

                        Operation.UserID = aq.sa.Main.u.UserID;
                        Operation.FournisseurID = a.FournisseurID;

                        int idd = await Operation.InsertOperationAsync();
                        OperationArticle ofa = new OperationArticle();

                        // Update article quantity
                        a.Quantite += aq.qte;
                        int updateResult = await a.UpdateArticleAsync();

                        if (updateResult > 0)
                        {
                            ofa.ArticleID = a.ArticleID;
                            ofa.OperationID = idd;
                            ofa.QteArticle = Convert.ToInt32(aq.qte);
                            await ofa.InsertOperationArticleAsync();

                            // FORCE RELOAD FROM DATABASE
                            Article articleService = new Article();
                            List<Article> refreshedArticles = await articleService.GetArticlesAsync();

                            // Update the main list with fresh data
                            aq.sa.Main.la = refreshedArticles;

                            // Refresh the UI
                            aq.sa.Main.LoadArticles(refreshedArticles);

                            // Show success message
                            WCongratulations wCongratulations = new WCongratulations("Opération réussie", "Quantité ajoutée avec succès", 1);
                            wCongratulations.ShowDialog();

                            // Close windows
                            aq.Close();
                            this.Close();
                        }
                        return;
                    }
                    else
                    {
                        if (Remise.Text != "")
                        {
                            if (Convert.ToDecimal(Remise.Text) > Convert.ToDecimal(a.PrixAchat * aq.qte))
                            {
                                MessageBox.Show("la remise est plus grande que le total.");
                                return;
                            }
                        }
                        int creditId = 0;
                        bool creditExists = false;
                        Credit Credit = new Credit();
                        List<Credit> lcc = await Credit.GetCreditsAsync();
                        Operation Operation = new Operation();
                        Operation.PaymentMethodID = methodID;
                        foreach (Credit cf in lcc)
                        {
                            if (cf.FournisseurID == a.FournisseurID)
                            {
                                if (Remise.Text != "")
                                {
                                    cf.Total += Convert.ToDecimal(a.PrixAchat * aq.qte) - Convert.ToDecimal(Remise.Text);
                                    Operation.CreditValue = Convert.ToDecimal(a.PrixAchat * aq.qte) - Convert.ToDecimal(Remise.Text);
                                }
                                else
                                {
                                    cf.Total += Convert.ToDecimal(a.PrixAchat * aq.qte);
                                    Operation.CreditValue = Convert.ToDecimal(a.PrixAchat * aq.qte);
                                }
                                await cf.UpdateCreditAsync();
                                creditExists = true;
                                creditId = cf.CreditID;
                                break;
                            }
                        }
                        if (!creditExists)
                        {
                            Credit newCredit = new Credit();
                            newCredit.FournisseurID = a.FournisseurID;
                            if (Remise.Text != "")
                            {
                                newCredit.Total += Convert.ToDecimal(a.PrixAchat * aq.qte) - Convert.ToDecimal(Remise.Text);
                                Operation.CreditValue = Convert.ToDecimal(a.PrixAchat * aq.qte) - Convert.ToDecimal(Remise.Text);
                            }
                            else
                            {
                                newCredit.Total += Convert.ToDecimal(a.PrixAchat * aq.qte);
                                Operation.CreditValue = Convert.ToDecimal(a.PrixAchat * aq.qte);
                            }
                            creditId = await newCredit.InsertCreditAsync();
                        }

                        Operation.OperationType = "AchatCr";
                        Operation.PrixOperation = a.PrixAchat * aq.qte;
                        Operation.CreditID = creditId;
                        if (Remise.Text != "")
                        {
                            Operation.Remise = Convert.ToDecimal(Remise.Text);
                        }

                        Operation.UserID = aq.sa.Main.u.UserID;
                        Operation.FournisseurID = a.FournisseurID;

                        int idd = await Operation.InsertOperationAsync();
                        OperationArticle ofa = new OperationArticle();

                        // Update article quantity
                        a.Quantite += aq.qte;
                        int updateResult = await a.UpdateArticleAsync();

                        if (updateResult > 0)
                        {
                            ofa.ArticleID = a.ArticleID;
                            ofa.OperationID = idd;
                            ofa.QteArticle = Convert.ToInt32(aq.qte);
                            await ofa.InsertOperationArticleAsync();

                            // FORCE RELOAD FROM DATABASE
                            Article articleService = new Article();
                            List<Article> refreshedArticles = await articleService.GetArticlesAsync();

                            // Update the main list with fresh data
                            aq.sa.Main.la = refreshedArticles;

                            // Refresh the UI
                            aq.sa.Main.LoadArticles(refreshedArticles);

                            // Show success message
                            WCongratulations wCongratulations = new WCongratulations("Opération réussie", "Quantité ajoutée avec succès", 1);
                            wCongratulations.ShowDialog();

                            // Close windows
                            aq.Close();
                            this.Close();
                        }
                        return;
                    }
                }

                //Add Multiple Articles
                if (ama != null)
                {
                    // ... (keep your existing code for multiple articles) ...
                }

                // If we reach here without returning, show generic success
                WCongratulations wCongratulations2 = new WCongratulations("Opération réussie", "Opération a ete effectue avec succes", 1);
                wCongratulations2.ShowDialog();
            }
            catch (Exception ex)
            {
                WCongratulations wCongratulations = new WCongratulations("Opération échoué", "Opération n'a pas ete effectue ", 0);
                wCongratulations.ShowDialog();
            }
        }

        private void RemiseInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string currentText = (sender as TextBox).Text;

            Remise.Text = "-" + currentText + " DH";

            if (ama == null)
            {
                if (a == null) return;
                if (currentText.Length == 0)
                {
                    FinalTotal.Text = (a.PrixAchat * a.Quantite).ToString("0.00") + " DH";
                    Remise.Text = "-0.00 DH";
                    return;
                }
                FinalTotal.Text = ((a.PrixAchat * a.Quantite) - Convert.ToDecimal(currentText)).ToString("0.00") + " DH";
            }
            else
            {
                if (currentText.Length == 0)
                {
                    FinalTotal.Text = FinalTotall.ToString("0.00") + " DH";
                    Remise.Text = "-0.00 DH";
                    return;
                }
                FinalTotal.Text = (FinalTotall - Convert.ToDecimal(currentText)).ToString("0.00") + " DH";
            }
        }
    }
}