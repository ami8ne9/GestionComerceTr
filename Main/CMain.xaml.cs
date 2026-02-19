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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GestionComerce.Main
{
    /// <summary>
    /// Logique d'interaction pour CMain.xaml
    /// </summary>
    public partial class CMain : UserControl
    {
        public CMain(MainWindow main, User u)
        {
            InitializeComponent();
            this.main = main;
            this.u = u;
            Name.Text = u.UserName;

            foreach (Role r in main.lr)
            {
                if (r.RoleID == u.RoleID)
                {
                    if (r.ViewSettings == false)
                    {
                        SettingsBtn.IsEnabled = false;
                        SetButtonGrayedOut(SettingsBtn);
                    }
                    if (r.ViewProjectManagment == false)
                    {
                        ProjectManagmentBtn.IsEnabled = false;
                        SetButtonGrayedOut(ProjectManagmentBtn);
                    }
                    if (r.ViewVente == false)
                    {
                        VenteBtn.IsEnabled = false;
                        SetButtonGrayedOut(VenteBtn);
                    }
                    if (r.ViewInventrory == false)
                    {
                        InventoryBtn.IsEnabled = false;
                        SetButtonGrayedOut(InventoryBtn);
                    }
                    if (r.ViewClientsPage == false)
                    {
                        ClientBtn.IsEnabled = false;
                        SetButtonGrayedOut(ClientBtn);
                    }
                    if (r.ViewFournisseurPage == false)
                    {
                        FournisseurBtn.IsEnabled = false;
                        SetButtonGrayedOut(FournisseurBtn);
                    }

                    // NEW: Check Facturation permission
                    if (r.AccessFacturation == false)
                    {
                        FacturationBtn.IsEnabled = false;
                        SetButtonGrayedOut(FacturationBtn);
                    }

                    // NEW: Check Livraison permission
                    if (r.AccessLivraison == false)
                    {
                        LivraisonBtn.IsEnabled = false;
                        SetButtonGrayedOut(LivraisonBtn);
                    }
                }
            }
        }

        // Helper method to set button appearance when disabled
        private void SetButtonGrayedOut(Button button)
        {
            if (button != null)
            {
                // Create a gray overlay effect
                var grayBrush = new SolidColorBrush(Color.FromArgb(180, 128, 128, 128));

                // Apply grayscale effect
                button.Opacity = 0.4;
                button.Cursor = Cursors.No;
            }
        }

        public MainWindow main;
        public User u;

        private void LivraisonBtn_Click(object sender, RoutedEventArgs e)
        {
            main.load_livraison(u);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            main.load_settings(u);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Article a = new Article();
            List<Article> la = await a.GetArticlesAsync();
            main.load_vente(u, la);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            main.load_inventory(u);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            main.load_ProjectManagement(u);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            main.load_client(u);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            main.load_fournisseur(u);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Exit exit = new Exit(this, 1);
            exit.ShowDialog();
        }

        private void FacturationBtn_Click(object sender, RoutedEventArgs e)
        {
            main.load_facturation(u);
        }

        private void AccountabilityBtn_Click(object sender, RoutedEventArgs e)
        {
            main.load_accountibility(u);
        }
    }
}