using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Paint_Clone.viewmodels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Shape> Shapes { get; set; } = new ObservableCollection<Shape>();

        public MainWindowViewModel()
        {

        }

        private void AddText()
        {

        }

        private void SaveImage()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = ""
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}