using Paint_Clone.FileFormatsMode.Viewmodels;
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

namespace Paint_Clone.FileFormatsMode.Views
{
    public partial class FileFormatsView : UserControl
    {
        FileFormatsViewModel viewModel;
        public FileFormatsView(FileFormatsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = this.viewModel = viewModel;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
