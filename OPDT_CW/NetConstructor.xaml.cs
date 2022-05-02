using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace BSC
{
    /// <summary>
    /// Логика взаимодействия для NetConstructor.xaml
    /// </summary>
    public partial class NetConstructor : Window
    {
        private readonly List<Node> _leavses;

        internal NetConstructor(List<Node> leavse)
        {
            InitializeComponent();

            _leavses = leavse;

            for (var i = 0; i < _leavses.Count; i++)
            {
                var listBoxItem = new ListBoxItem
                {
                    Content = _leavses[i].Description,
                    Tag = i
                };
                LeavesListBox.Items.Add(listBoxItem);
            }
        }

        private void ShowNetButton_Click(object sender, RoutedEventArgs e)
        {
            var edges = new List<Edge>();   
            foreach(var listBoxItem in NetConnectionsListBox.Items)
            {
                edges.Add((Edge)((ListBoxItem)listBoxItem).Tag);
            }

            var serializedNodes = Newtonsoft.Json.JsonConvert.SerializeObject((_leavses, edges));

            string jsonFileName = "net.json";
            using (var file = File.Create(jsonFileName))
            {
                var bytes = Encoding.UTF8.GetBytes(serializedNodes);
                file.Write(bytes, 0, bytes.Length);
            }

            Process.Start("NetDrawer.exe");
        }

        private void AddNetConnection_Click(object sender, RoutedEventArgs e)
        {
            if (LeavesListBox.SelectedItems.Count != 2)
            {
                MessageBox.Show("Должно быть выбрано два элемента!", "", MessageBoxButton.OK);
                return;
            }

            var firstIndex = (int)((ListBoxItem)LeavesListBox.SelectedItems[0]).Tag;
            var secondIndex = (int)((ListBoxItem)LeavesListBox.SelectedItems[1]).Tag;
            var first = _leavses[firstIndex];
            var second = _leavses[secondIndex];

            var connectionsListBoxItem = new ListBoxItem
            {
                Content = $"{first.Description} — {second.Description}",
                Tag = new Edge(firstIndex, secondIndex)
            };

            NetConnectionsListBox.Items.Add(connectionsListBoxItem);

            LeavesListBox.SelectedItems.Clear();
        }

        private void DeleteNetConnection_Click(object sender, RoutedEventArgs e)
        {
            if (NetConnectionsListBox.SelectedItem == null)
            {
                MessageBox.Show("Не выбран узел!", "", MessageBoxButton.OK);
                return;
            }

            var result = MessageBox.Show("Вы уверены?", "Удалить узел", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            var selectedItem = (ListBoxItem)NetConnectionsListBox.SelectedItem;
            NetConnectionsListBox.Items.Remove(selectedItem);
        }

        private void NetConnectionsListBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            NetConnectionsListBox.SelectedItems.Clear();
        }
    }
}
