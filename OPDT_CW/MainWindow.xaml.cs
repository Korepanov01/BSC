using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BSC;
using IronXL;

namespace BSC
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddFinanceParameterButton_Click(object sender, RoutedEventArgs e)
        {
            AddParameter(FinanceParameterName, FinanceParameterValue, ParameterType.Finance, FinanceParametersListBox);
        }

        private void AddClientsParameterButton_Click(object sender, RoutedEventArgs e)
        {
            AddParameter(ClientsParameterName, ClientsParameterValue, ParameterType.Clients, ClientsParametersListBox);
        }

        private void AddInternalParameterButton_Click(object sender, RoutedEventArgs e)
        {
            AddParameter(InternalParameterName, InternalParameterValue, ParameterType.Internal, InternalParametersListBox);
        }

        private void AddEducationParameterButton_Click(object sender, RoutedEventArgs e)
        {
            AddParameter(EducationParameterName, EducationParameterValue, ParameterType.Education, EducationParametersListBox);
        }

        private void AddItParameterButton_Click(object sender, RoutedEventArgs e)
        {
            AddParameter(ItParameterName, ItParameterValue, ParameterType.It, ItParametersListBox);
        }


        private static void AddParameter(TextBox nameTextBox, TextBox valueTextBox, ParameterType parameterType, ListBox parametersListBox)
        {
            var name = nameTextBox.Text;
            nameTextBox.Text = "";
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Неверный формат имени!");
                return;
            }
            var valueText = valueTextBox.Text;
            valueTextBox.Text = "";
            if (!float.TryParse(valueText, out var value))
            {
                MessageBox.Show("Неверный формат значения!");
                return;
            }
            var treeViewItem = new ListBoxItem
            {
                Content = $"{name} (стоимость - {value}% от прибыли)",
                Tag = new Parameter(name, value, parameterType)
            };
            parametersListBox.Items.Add(treeViewItem);
        }

        private void DeleteFinanceParameterButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteParameter(FinanceParametersListBox);
        }

        private void DeleteClientsParameterButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteParameter(ClientsParametersListBox);
        }

        private void DeleteInternalParameterButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteParameter(InternalParametersListBox);
        }

        private void DeleteEducationParameterButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteParameter(EducationParametersListBox);
        }

        private void DeleteItParameterButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteParameter(ItParametersListBox);
        }

        private static void DeleteParameter(ListBox parametersListBox)
        {
            if (parametersListBox.SelectedIndex == -1)
            {
                return;
            }

            parametersListBox.Items.Remove(parametersListBox.SelectedItem);
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

        private readonly Dictionary<string, decimal[]> _npvParameters = new();
        private int _months;

        private void ReadExcel(string tableFilePath)
        {
            var workBook = WorkBook.Load(tableFilePath);
            var workSheet = workBook.DefaultWorkSheet;
            var dataTable = workSheet.ToDataTable(false);
            ExcelTable.DataContext = dataTable.DefaultView;

            _months = workSheet.Columns.Length - 1;

            _npvParameters.Clear();
            foreach (var row in workSheet.Rows[1..])
            {
                _npvParameters.Add(row.First().StringValue, row.Skip(1).Select(cell => cell.DecimalValue).ToArray());
            }

            workBook.Close();
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

            var npv = CalcNpv(_npvParameters);

            foreach (var paramName in _npvParameters.Keys)
            {
                var oldValues = _npvParameters[paramName];

                var newValues = oldValues.Select(value => value * 1.2m).ToArray();
                _npvParameters[paramName] = newValues;
                var newNpv = CalcNpv(_npvParameters);
                var dNpv = Math.Abs(npv - newNpv) / Math.Abs(npv / 100);
                npvParameters.Add(new Parameter(paramName, (float)dNpv / 20, ParameterType.Finance));

                _npvParameters[paramName] = oldValues;
            }

            return npvParameters;
        }

        private static List<Parameter> GetParameters(ListBox parametersListBox)
        {
            var parameters = new List<Parameter>();
            foreach (var listBoxItem in parametersListBox.Items)
            {
                var parameter = (Parameter)((ListBoxItem)listBoxItem).Tag;
                parameters.Add(parameter);
            }
            return parameters;
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var parameters = new[]
            {
                GetParameters(ItParametersListBox),
                GetParameters(FinanceParametersListBox).Union(CalcNpvParameters()).ToList(),
                GetParameters(ClientsParametersListBox),
                GetParameters(EducationParametersListBox),
                GetParameters(InternalParametersListBox)
            };

            Hide();
            new TreeConstructorWindow(parameters).ShowDialog();
            Show();
        }
    }
}