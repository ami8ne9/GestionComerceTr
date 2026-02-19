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

namespace GestionComerce.Main.ProjectManagment
{
    /// <summary>
    /// Interaction logic for CSingleArticleReverse.xaml
    /// </summary>
    public partial class CSingleArticleReverse : UserControl
    {
        public CSingleArticleReverse(WArticlesReverse Arts,OperationArticle oa)
        {
            InitializeComponent();
            foreach (Article a in Arts.plus.so.main.main.laa) {
                if (oa.ArticleID == a.ArticleID)
                {
                    ArticleName.Text=a.ArticleName;
                    this.oa = oa;
                }
            }
            inittialStat=oa.Reversed;
            if (oa.Reversed == true)
            {
                Reverse.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4d4d4d"));
                Reverse.IsEnabled = false;

            }

        }
        public OperationArticle oa;public bool inittialStat;
        private void ReverseArticle_Click(object sender, RoutedEventArgs e)
        {
            if (oa.Reversed == true)
            {
                oa.Reversed =false;
                Reverse.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#EF4444"));
                Reverse.Content = "Reverse";
            }
            else
            {
                oa.Reversed = true;
                Reverse.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4d4d4d"));
                Reverse.Content = "Reversed";
            }
        }
    }
}
