using Paint_Clone.MorphologicalFiltersMode.Viewmodels;
using System;
using System.Collections;
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

namespace Paint_Clone.MorphologicalFiltersMode.Views;

public partial class MorphologicalFiltersView : UserControl
{
    MorphologicalFiltersViewModel viewModel;
    Point origin;
    Point start;

    public MorphologicalFiltersView(MorphologicalFiltersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this.viewModel = viewModel;
    }
}
