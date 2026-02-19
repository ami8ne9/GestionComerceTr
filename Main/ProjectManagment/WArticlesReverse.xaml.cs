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
    /// Interaction logic for WArticlesReverse.xaml
    /// </summary>
    public partial class WArticlesReverse : Window
    {
        public WArticlesReverse(WPlus plus)
        {
            InitializeComponent();
            this.plus = plus;
            LoadRArticles();
        }
        
        public WPlus plus;

        public void LoadRArticles()
        {
            foreach (OperationArticle oa in plus.so.main.main.loa)
            {

                if (oa.OperationID == plus.so.op.OperationID)
                {
                    CSingleArticleReverse cSingleArticleReverse = new CSingleArticleReverse(this,oa);
                    ArticlesContainer.Children.Add(cSingleArticleReverse);
                }
            }
        }

        private void FermerButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(CSingleArticleReverse sar in ArticlesContainer.Children)
            {
                sar.oa.Reversed= sar.inittialStat;
            }
            this.Close();
        }

        private void ConfirmerButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (CSingleArticleReverse sar in ArticlesContainer.Children)
            {
                sar.oa.UpdateOperationArticleAsync();
            }
            WReverseConfirmation wReverseConfirmation = new WReverseConfirmation(plus,this);
            wReverseConfirmation.Show();
        }
    }
}
