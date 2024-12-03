using Paint_Clone.BasicDrawingMode.ViewModels;
using Paint_Clone.BezierCurveMode.Viewmodels;
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

namespace Paint_Clone.BezierCurveMode.Views
{

    public partial class BezierCurveView : UserControl
    {
        BezierCurveViewmodel viewModel;
        public BezierCurveView(BezierCurveViewmodel viewModel)
        {
            InitializeComponent();
            DataContext = this.viewModel = viewModel;
        }
    }
}
