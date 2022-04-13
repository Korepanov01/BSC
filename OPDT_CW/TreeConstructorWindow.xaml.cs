using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System;
using Newtonsoft;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows.Media;
using System.Runtime.Serialization.Formatters.Binary;

namespace BSC;

public partial class TreeConstructorWindow
{
    private readonly List<Parameter>[] _parameters;

    public TreeConstructorWindow(List<Parameter>[] parameters)
    {
        _parameters = parameters;

        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        for (var i = 0; i < _parameters.Length; i++)
        {
            foreach (var parameter in _parameters[i])
            {
                var listBoxItem = new ListBoxItem
                {
                    Content = $"{parameter.Name} ({Math.Round(parameter.Value, 2)}) [{parameter.ParameterType}]",
                    Tag = parameter
                };
                ParametersListBox.Items.Add(listBoxItem);
            }
        }
    }

    private void AddNodeButton_Click(object sender, RoutedEventArgs e)
    {
        if (NodeNameTextBox.Text == "")
        {
            MessageBox.Show("Описание узла не может быть пустым!", "", MessageBoxButton.OK);
            return;
        }

        var parameters = new[] 
        { 
            new List<Parameter>(), 
            new List<Parameter>(),
            new List<Parameter>(),
            new List<Parameter>(),
            new List<Parameter>() 
        };
        foreach (var selectedItem in ParametersListBox.SelectedItems)
        {
            var parameter = (Parameter)((ListBoxItem)selectedItem).Tag;
            parameters[(int)parameter.ParameterType].Add(parameter);
        }

        var treeViewItem = new TreeViewItem
        {
            Header = NodeNameTextBox.Text,
            Tag = new Node(NodeNameTextBox.Text, parameters)
        };

        if (TreeTreeView.SelectedItem == null)
        {
            TreeTreeView.Items.Add(treeViewItem);
        }
        else
        {
            ((TreeViewItem)TreeTreeView.SelectedItem).Items.Add(treeViewItem);
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

        var selectedItem = (TreeViewItem)TreeTreeView.SelectedItem;
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
        var edges = new List<Edge>();
        var nodeIndex = -1;

        void CalcNodesAndEdges(int parentNodeIndex, ItemCollection itemCollection)
        {
            var index = nodeIndex++;
            if (index != -1 && parentNodeIndex != -1)
            {
                edges.Add(new Edge(parentNodeIndex, index));
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

    private void TreeTreeView_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ((TreeViewItem)TreeTreeView.SelectedItem).IsSelected = false;
    }

    private void SaveTreeButton_Click(object sender, RoutedEventArgs e)
    {
        var binaryFormatter = new BinaryFormatter();
        using (var fileStream = new FileStream("tree.tree", FileMode.Create))
        {
            binaryFormatter.Serialize(fileStream, TreeTreeView.Items);
        }
    }

    private void LoadTreeButton_Click(object sender, RoutedEventArgs e)
    {
        var fileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "tree (*.tree)|*.tree"
        };

        var result = fileDialog.ShowDialog();

        if (result is not true) return;

        ItemCollection itemCollection; 

        var binaryFormatter = new BinaryFormatter();
        using(var fileStream = new FileStream(fileDialog.FileName, FileMode.Open))
        {
            itemCollection = (ItemCollection)binaryFormatter.Deserialize(fileStream);
        }
        TreeTreeView.ItemsSource = itemCollection;
    }
}