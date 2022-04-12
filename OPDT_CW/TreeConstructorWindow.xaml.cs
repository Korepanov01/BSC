using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System;
using Newtonsoft;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace OPDT_CW;

public partial class TreeWindow
{
    private readonly Dictionary<string, float> _elasticityCoefficients;

    public TreeWindow(Dictionary<string, float> elasticityCoefficients)
    {
        _elasticityCoefficients = elasticityCoefficients;

        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        foreach (var elasticityCoefficient in _elasticityCoefficients)
        {
            var listBoxItem = new ListBoxItem
            {
                Content = $"{elasticityCoefficient.Key} ({Math.Round(elasticityCoefficient.Value, 2)})",
                Tag = elasticityCoefficient
            };
            ParametersListBox.Items.Add(listBoxItem);
        }
    }

    private void AddNodeButton_Click(object sender, RoutedEventArgs e)
    {
        if (NodeNameTextBox.Text == "")
        {
            MessageBox.Show("Описание узла не может быть пустым!", "", MessageBoxButton.OK);
            return;
        }

        var parameters = new Dictionary<string, float>();
        foreach (var selectedItem in ParametersListBox.SelectedItems)
        {
            var (key, value) = (KeyValuePair<string, float>) ((ListBoxItem) selectedItem).Tag;
            parameters.Add(key, value);
        }
        var riskLevel = parameters.Sum(parameter => parameter.Value);

        var treeViewItem = new TreeViewItem
        {
            Header = $"{NodeNameTextBox.Text}" + (parameters.Keys.Count != 0 ? $"\n[{string.Join(",\n", parameters.Keys)}]" : ""),
            Tag = new Node(NodeNameTextBox.Text, parameters, riskLevel)
        };

        if (TreeTreeView.SelectedItem == null)
        {
            TreeTreeView.Items.Add(treeViewItem);
        }
        else
        {
            ((TreeViewItem) TreeTreeView.SelectedItem).Items.Add(treeViewItem);
        }

        NodeNameTextBox.Text = "";
        ParametersListBox.SelectedItems.Clear();
    }

    private void DeleteNodeButton_Click(object sender, RoutedEventArgs e)
    {
        if (TreeTreeView.SelectedItem == null)
        {
            MessageBox.Show("Не выбран узел!", "", MessageBoxButton.OK);
            return;
        }

        var result = MessageBox.Show("Вы уверены?", "Удалить узел", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;
        
        var selectedItem = (TreeViewItem) TreeTreeView.SelectedItem;
        if (selectedItem.Parent is TreeView)
        {
            TreeTreeView.Items.Remove(selectedItem);
        }
        else
        {
            ((TreeViewItem)selectedItem.Parent).Items.Remove(selectedItem);
        }
    }

    private void ShowTreeButton_Click(object sender, RoutedEventArgs e)
    {
        var nodes = new List<Node>();
        var edges = new List<(int, int)>();
        int nodeIndex = -1;

        void CalcNodesAndEdges(int parentNodeIndex, ItemCollection itemCollection)
        {
            var index = nodeIndex++;
            if (index != -1 && parentNodeIndex != -1)
            {
                edges.Add((parentNodeIndex, index));
            }
            foreach (var item in itemCollection)
            {
                var treeViewItem = (TreeViewItem)item;

                nodes.Add((Node)treeViewItem.Tag);

                CalcNodesAndEdges(index, treeViewItem.Items);
            }
        }
        CalcNodesAndEdges(nodeIndex, TreeTreeView.Items);

        var serializedNodes = Newtonsoft.Json.JsonConvert.SerializeObject((nodes, edges));

        string jsonFileName = "tree.json";
        using (var file = File.Create(jsonFileName))
        {
            var bytes = Encoding.UTF8.GetBytes(serializedNodes);
            file.Write(bytes, 0, bytes.Length);
        }

        Process.Start("TreeDrawer.exe");
    }
}