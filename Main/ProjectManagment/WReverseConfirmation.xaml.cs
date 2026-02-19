using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GestionComerce.Main.ProjectManagment
{
    /// <summary>
    /// Interaction logic for WReverseConfirmation.xaml
    /// </summary>
    public partial class WReverseConfirmation : Window
    {
        public WReverseConfirmation(WPlus plus, WArticlesReverse arts)
        {
            InitializeComponent();
            this.plus = plus; this.arts = arts;
            
        }
        WPlus plus; WArticlesReverse arts; int countRev; decimal RA; decimal RAA;
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<OperationArticle> oas = new List<OperationArticle>();
                countRev = 0;
                if (arts != null)
                {
                    foreach (CSingleArticleReverse sar in arts.ArticlesContainer.Children)
                    {
                        if (sar.oa.Reversed == true)
                        {
                            countRev++;


                        }
                        if (sar.inittialStat != sar.oa.Reversed)
                        {
                            foreach (Article a in plus.so.main.main.laa)
                            {
                                if (a.ArticleID == sar.oa.ArticleID)
                                {
                                    if (plus.so.op.OperationType.StartsWith("V"))
                                    {
                                        RA += sar.oa.QteArticle * a.PrixVente;
                                    }
                                    else if (plus.so.op.OperationType.StartsWith("A"))
                                    {
                                        RA += sar.oa.QteArticle * a.PrixAchat;
                                    }

                                }
                            }
                            sar.oa.UpdateOperationArticleAsync();
                            oas.Add(sar.oa);
                        }
                        else
                        {

                            if (sar.oa.Reversed == true)
                            {
                                foreach (Article a in plus.so.main.main.laa)
                                {
                                    if (a.ArticleID == sar.oa.ArticleID)
                                    {
                                        if (plus.so.op.OperationType.StartsWith("V"))
                                        {
                                            RAA += sar.oa.QteArticle * a.PrixVente;
                                        }
                                        else if (plus.so.op.OperationType.StartsWith("A"))
                                        {
                                            RAA += sar.oa.QteArticle * a.PrixAchat;
                                        }

                                    }
                                }
                            }
                        }

                    }

                    if (countRev == arts.ArticlesContainer.Children.Count) plus.so.op.Reversed = true;
                }
                if (plus.so.op.OperationType.StartsWith("V"))
                {
                    foreach (OperationArticle cs in oas)
                    {
                        foreach (Article a in plus.so.main.main.laa)
                        {
                            if (a.ArticleID == cs.ArticleID)
                            {
                                a.Quantite += cs.QteArticle;
                                a.UpdateArticleAsync();
                                break;
                            }
                        }

                    }
                    if (plus.so.op.OperationType.EndsWith("50"))
                    {
                        foreach (Client c in plus.so.main.main.lc)
                        {
                            if (c.ClientID == plus.so.op.ClientID)
                            {
                                foreach (Credit cr in plus.so.main.main.credits)
                                {

                                    if (cr.ClientID == plus.so.op.ClientID)
                                    {
                                        plus.so.op.CreditValue -= RAA;
                                        if (plus.so.op.CreditValue < 0)
                                        {
                                            plus.so.op.CreditValue = 0;
                                            break;
                                        }
                                        if (plus.so.op.CreditValue < RA) cr.Total -= plus.so.op.CreditValue;
                                        else cr.Total -= RA;
                                        cr.UpdateCreditAsync();
                                        break;
                                    }

                                }

                                break;

                            }
                        }
                    }
                    else if (plus.so.op.OperationType.EndsWith("Cr"))
                    {
                        foreach (Client c in plus.so.main.main.lc)
                        {
                            if (c.ClientID == plus.so.op.ClientID)
                            {
                                foreach (Credit cr in plus.so.main.main.credits)
                                {

                                    if (cr.ClientID == plus.so.op.ClientID)
                                    {
                                        if (RA < plus.so.op.CreditValue - RAA)
                                        {
                                            cr.Total -= RA;
                                        }
                                        else
                                        {
                                            if (plus.so.op.CreditValue - RAA > 0)
                                            {
                                                cr.Total -= plus.so.op.CreditValue - RAA;
                                            }

                                        }

                                        cr.UpdateCreditAsync();
                                        break;
                                    }

                                }
                                break;
                            }
                        }
                    }

                }
                else if (plus.so.op.OperationType.StartsWith("A"))
                {
                    foreach (OperationArticle cs in oas)
                    {
                        foreach (Article a in plus.so.main.main.laa)
                        {
                            if (a.ArticleID == cs.ArticleID)
                            {
                                a.Quantite -= cs.QteArticle;

                                if (a.Quantite == 0)
                                {
                                    a.Etat = false;
                                    a.DeleteArticleAsync();
                                }
                                else
                                {
                                    a.UpdateArticleAsync();
                                }
                                break;
                            }
                        }
                    }

                    if (plus.so.op.OperationType.EndsWith("50"))
                    {
                        foreach (Fournisseur c in plus.so.main.main.lfo)
                        {
                            if (c.FournisseurID == plus.so.op.FournisseurID)
                            {
                                foreach (Credit cr in plus.so.main.main.credits)
                                {

                                    if (cr.FournisseurID == plus.so.op.FournisseurID)
                                    {
                                        plus.so.op.CreditValue -= RAA;
                                        if (plus.so.op.CreditValue < 0)
                                        {
                                            plus.so.op.CreditValue = 0;
                                            break;
                                        }
                                        if (plus.so.op.CreditValue < RA) cr.Total -= plus.so.op.CreditValue;
                                        else cr.Total -= RA;
                                        cr.UpdateCreditAsync();
                                        break;
                                    }

                                }

                                break;

                            }
                        }
                    }
                    else if (plus.so.op.OperationType.EndsWith("Cr"))
                    {
                        foreach (Fournisseur c in plus.so.main.main.lfo)
                        {
                            if (c.FournisseurID == plus.so.op.FournisseurID)
                            {
                                foreach (Credit cr in plus.so.main.main.credits)
                                {

                                    if (cr.FournisseurID == plus.so.op.FournisseurID)
                                    {
                                        if (RA < plus.so.op.CreditValue - RAA)
                                        {
                                            cr.Total -= RA;
                                        }
                                        else
                                        {
                                            if (plus.so.op.CreditValue - RAA > 0)
                                            {
                                                cr.Total -= plus.so.op.CreditValue - RAA;
                                            }

                                        }

                                        cr.UpdateCreditAsync();
                                        break;

                                    }

                                }
                                break;
                            }
                        }
                    }
                }
                //change this
                else if (plus.so.op.OperationType.StartsWith("M"))
                {
                    foreach (OperationArticle cs in plus.so.main.main.loa)
                    {
                        foreach (Article a in plus.so.main.main.laa)
                        {
                            if (a.ArticleID == cs.ArticleID && plus.so.op.OperationID == cs.OperationID)
                            {
                                a.Quantite = cs.QteArticle;
                                plus.so.op.Reversed = true;
                                cs.Reversed = true;
                                a.UpdateArticleAsync();
                                break;
                            }
                        }
                    }
                }
                else if (plus.so.op.OperationType.StartsWith("D"))

                {
                    foreach (OperationArticle cs in plus.so.main.main.loa)
                    {
                        foreach (Article a in plus.so.main.main.laa)
                        {
                            if (a.ArticleID == cs.ArticleID && plus.so.op.OperationID == cs.OperationID)
                            {
                                a.Etat = true;
                                plus.so.op.Reversed = true;
                                cs.Reversed = true;
                                a.BringBackArticleAsync();
                                break;
                            }
                        }
                    }
                }
                else if (plus.so.op.OperationType.StartsWith("S"))
                {
                    foreach (Credit cr in plus.so.main.main.credits)
                    {
                        if (plus.so.op.CreditID == cr.CreditID)
                        {
                            cr.Paye += plus.so.op.CreditValue;
                            plus.so.op.Reversed = true;
                            cr.UpdateCreditAsync();

                        }
                    }
                }
                else if (plus.so.op.OperationType.StartsWith("P"))
                {
                    foreach (Credit cr in plus.so.main.main.credits)
                    {
                        if (plus.so.op.CreditID == cr.CreditID)
                        {
                            cr.Paye += plus.so.op.CreditValue;
                            plus.so.op.Reversed = true;
                            cr.UpdateCreditAsync();

                        }
                    }
                }
                plus.so.op.UpdateOperationAsync();
                plus.so.main.LoadOperations(plus.so.main.main.lo);
                plus.so.main.LoadMouvments(plus.so.main.main.loa);
                plus.so.main.LoadStatistics();
                //plus.Close();
                //arts?.Close();
                //this.Close();

                plus.LoadArticles();
                WCongratulations wCongratulations = new WCongratulations("Reverse réussie", "Reverse a ete effectue avec succes", 1);
                wCongratulations.ShowDialog();
            }
            catch(Exception ex)
            {

                WCongratulations wCongratulations = new WCongratulations("Reverse échoué", "Reverse n'a pas ete effectue ", 0);
                wCongratulations.ShowDialog();
            }
        }
    }
}
