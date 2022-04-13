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
using BSC;

namespace BSC
{
    /// <summary>
    /// Логика взаимодействия для AddingParametersWindow.xaml
    /// </summary>
    public partial class AddingParametersWindow : Window
    {
        private readonly List<Parameter> _npvParameters;
        public AddingParametersWindow(List<Parameter> elasticityCoefficients)
        {
            _npvParameters = elasticityCoefficients;
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
                Content = $"{name} ({value}%)",
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

        private static void DeleteParameter(ListBox parametersListBox)
        {
            if (parametersListBox.SelectedIndex == -1)
            {
                return;
            }

            parametersListBox.Items.Remove(parametersListBox.SelectedItem);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var parameters = new[]
            {
                _npvParameters,
                GetParameters(FinanceParametersListBox),
                GetParameters(ClientsParametersListBox),
                GetParameters(EducationParametersListBox),
                GetParameters(InternalParametersListBox)
            };

            Hide();
            new TreeConstructorWindow(parameters).ShowDialog();
            Show();
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
    }
}