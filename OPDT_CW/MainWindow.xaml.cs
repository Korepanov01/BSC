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
        public MainWindow()
        {
            InitializeComponent();

            Hide();

            new AddingParametersWindow().Show();

            Close();
        }
    }
}