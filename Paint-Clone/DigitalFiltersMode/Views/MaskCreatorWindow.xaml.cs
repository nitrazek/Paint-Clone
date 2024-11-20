using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Projekt_4
{
    public partial class MaskCreatorWindow : Window
    {
        public bool Status { get; private set; } = false;
        public double[] Mask { get; private set; } = null;
        public int MaskWidth { get; private set; } = 0;
        public int MaskHeight { get; private set; } = 0;

        private int RowsCount { get => MaskGrid.RowDefinitions.Count; }
        private int ColumnsCount { get => MaskGrid.ColumnDefinitions.Count; }

        public MaskCreatorWindow()
        {
            InitializeComponent();
            AddRow();
            AddColumn();
        }

        private void AddRow()
        {
            var rowDefs = MaskGrid.RowDefinitions;
            rowDefs.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            var newRowId = rowDefs.Count - 1;
            var colCnt = ColumnsCount;
            var chn = MaskGrid.Children;
            for (int i = 0; i < colCnt; i++)
            {
                var tb = new TextBox();
                Grid.SetColumn(tb, i);
                Grid.SetRow(tb, newRowId);
                chn.Add(tb);
            }
        }

        private void RemoveRow()
        {
            var lastRowId = RowsCount - 1;
            var chn = MaskGrid.Children;
            var toRmv = new LinkedList<TextBox>();
            foreach (var c in chn)
            {
                if (!(c is TextBox)) continue;
                var tb = (TextBox)c;
                var row = Grid.GetRow(tb);
                if (row == lastRowId)
                    toRmv.AddLast(tb);
            }
            foreach (var tb in toRmv)
                chn.Remove(tb);
            MaskGrid.RowDefinitions.RemoveAt(lastRowId);
        }

        private void AddColumn()
        {
            var colDefs = MaskGrid.ColumnDefinitions;
            colDefs.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var newColId = colDefs.Count - 1;
            var rowCnt = RowsCount;
            var chn = MaskGrid.Children;
            for (int i = 0; i < rowCnt; i++)
            {
                var tb = new TextBox();
                Grid.SetColumn(tb, newColId);
                Grid.SetRow(tb, i);
                chn.Add(tb);
            }
        }

        private void RemoveColumn()
        {
            var lastColId = ColumnsCount - 1;
            var chn = MaskGrid.Children;
            var toRmv = new LinkedList<TextBox>();
            foreach (var c in chn)
            {
                if (!(c is TextBox)) continue;
                var tb = (TextBox)c;
                var col = Grid.GetColumn(tb);
                if (col == lastColId)
                    toRmv.AddLast(tb);
            }
            foreach (var tb in toRmv)
                chn.Remove(tb);
            MaskGrid.ColumnDefinitions.RemoveAt(lastColId);
        }

        private void RowMinus_Click(object sender, RoutedEventArgs e)
        {
            if (RowsCount == 1) return;
            RemoveRow();
            RemoveRow();
        }

        private void RowPlus_Click(object sender, RoutedEventArgs e)
        {
            AddRow();
            AddRow();
        }

        private void ColumnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (ColumnsCount == 1) return;
            RemoveColumn();
            RemoveColumn();
        }

        private void ColumnPlus_Click(object sender, RoutedEventArgs e)
        {
            AddColumn();
            AddColumn();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Status = false;
            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            MaskWidth = MaskGrid.ColumnDefinitions.Count;
            MaskHeight = MaskGrid.RowDefinitions.Count;
            Mask = new double[MaskWidth * MaskHeight];
            var chn = MaskGrid.Children;
            foreach (var c in chn)
            {
                if (!(c is TextBox)) continue;
                var tb = (TextBox)c;
                var row = Grid.GetRow(tb);
                var col = Grid.GetColumn(tb);
                if (!double.TryParse(tb.Text, out double val))
                {
                    MessageBox.Show($"Podaj poprawną wartość w wierszu {row} i kolumnie {col}.");
                    return;
                }
                Mask[col + MaskWidth * row] = val;
            }
            Status = true;
            Close();
        }
    }
}
