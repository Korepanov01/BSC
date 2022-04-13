using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BSC;
using IronXL;

namespace BSC
{
    public partial class MainWindow
    {
        private readonly Dictionary<string, decimal[]> _parameters = new();
        private int _months;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "excel table (*.xlsx)|*.xlsx"
            };

            var result = fileDialog.ShowDialog();

            if (result is not true) return;
            
            NextButton.IsEnabled = true;
            ReadExcel(fileDialog.FileName);
        }

        private void ReadExcel(string tableFilePath)
        {
            var workBook = WorkBook.Load(tableFilePath);
            var workSheet = workBook.DefaultWorkSheet;
            var dataTable = workSheet.ToDataTable(true);
            ExcelTable.DataContext = dataTable.DefaultView;

            _months = workSheet.Columns.Length - 1;
            
            _parameters.Clear();
            foreach (var row in workSheet.Rows[1..])
            {
                _parameters.Add(row.First().StringValue, row.Skip(1).Select(cell => cell.DecimalValue).ToArray());
            }

            workBook.Close();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            
            new AddingParametersWindow(CalcNpvParameters()).ShowDialog();
            
            Show();
        }

        private decimal CalcNpv(Dictionary<string, decimal[]> parameters)
        {
            var cashFlows = new decimal[_months];
            for (var month = 0; month < _months; month++)
            {
                foreach (var parameter in parameters)
                {
                    cashFlows[month] += parameter.Value[month];
                }
            }

            for (var month = 1; month < _months; month++)
            {
                cashFlows[month] += cashFlows[month - 1];
            }

            return cashFlows.Sum();
        }

        private List<Parameter> CalcNpvParameters()
        {
            var npvParameters = new List<Parameter>();
            
            var npv = CalcNpv(_parameters);

            foreach (var paramName in _parameters.Keys)
            {
                var oldValues = _parameters[paramName];

                var newValues = oldValues.Select(value => value * 1.2m).ToArray();
                _parameters[paramName] = newValues;
                var newNpv = CalcNpv(_parameters);
                var dNpv = Math.Abs(npv - newNpv) / Math.Abs(npv / 100);
                npvParameters.Add(new Parameter(paramName, (float)dNpv / 20, ParameterType.Npv));
                
                _parameters[paramName] = oldValues;
            }

            return npvParameters;
        }
    }
}