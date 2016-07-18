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

namespace FotoLifeDownLoader
{
    /// <summary>
    /// Progress.xaml の相互作用ロジック
    /// </summary>
    public partial class Progress : Window
    {
        public Progress()
        {
            InitializeComponent();
            MouseLeftButtonDown += (sender, e) => DragMove();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
