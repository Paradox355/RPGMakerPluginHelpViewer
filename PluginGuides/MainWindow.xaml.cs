using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using FontFamily = System.Windows.Media.FontFamily;
using MySystemIcons = PluginGuides.SystemIcons;

namespace PluginGuides
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string ProjectFile { get; set; }
        public string PluginsPath { get; set; }
        public string PluginConfigFile { get; set; }
        public string PluginFile { get; set; }
        public List<PluginItem> PluginsList = new List<PluginItem>();
        public FontFamily HelpFontFamily { get; set; }
        public int HelpFontSize { get; set; }
        public bool HelpTextBlockActive { get; set; }
        public bool BoldEnabled { get; set; }

        public MainWindow()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                string message = "You must provide a valid PRG Maker project file to continue.";
                string caption = "RPG Maker Plugin Help Viewer";
                MessageBoxButton msgButton = MessageBoxButton.OKCancel;
                MessageBoxImage msgImage = MessageBoxImage.Exclamation;

                MessageBoxResult result = MessageBox.Show(message, caption, msgButton, msgImage);
                switch (result)
                {
                    case MessageBoxResult.OK:
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "RPG Maker Projects (*.rpgproject)|*.rpgproject";
                        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        if (openFileDialog.ShowDialog() == true)
                        {
                            ProjectFile = openFileDialog.FileName;
                        }
                        else
                        {
                            App.Current.Shutdown();
                        }
                        break;
                    case MessageBoxResult.Cancel:
                        App.Current.Shutdown();
                        break;
                }
            }
            else if (args.Length > 2)
            {
                for (var i = 1; i < args.Length; i++)
                {
                    if (i > 1)
                        ProjectFile += " ";
                    ProjectFile += args[i];
                }
            }
            else
            {
                ProjectFile = args[1];
            }
            PluginsPath = Path.GetDirectoryName(ProjectFile) + @"\js\plugins";
            PluginConfigFile = Path.GetDirectoryName(ProjectFile) + @"\js\plugins.js";

            InitializeComponent();
            BoldEnabled = (bool)tbBold.IsChecked;
            ProjectIcon.Source = MySystemIcons.ToImageSource(MySystemIcons.FolderFilesIcon);
            RefreshIcon.Source = MySystemIcons.ToImageSource(MySystemIcons.RefreshIcon);

            if (HelpTextBlock != null)
            {
                HelpTextBlockActive = true;
            }
            int[] intFontSizes = {8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72};
            string[] strFontSizes = intFontSizes.Select(x => x.ToString()).ToArray();
            cmbFontSize.ItemsSource = strFontSizes;
            Get_Plugins();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {

            HelpTextBlock.Text = "NOT IMPLEMENTED";

        }

        public class PluginItem
        {
            public string Title { get; set; }
        }

        private void LbPluginList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbPluginList.SelectedItem != null)
            {
                bool foundStart = false;
                bool foundHelp = false;
                bool foundHelpEnd = false;
                string helpText = "";
                string tempLine = "";
                const string Pattern = @"/*:\w{2,}";
                string pluginName = (lbPluginList.SelectedItem as PluginItem).Title;
                PluginFile = pluginName + ".js";

                string pluginFileFullPath = PluginsPath + $"\\{PluginFile}";
                string[] pluginFileContents = File.ReadAllLines(pluginFileFullPath);
                foreach (string line in pluginFileContents)
                {
                    if (foundHelpEnd)
                        break;

                    if (foundHelp)
                    {
                        if (line.Contains("*/") || line.Contains("@"))
                        {
                            foundHelpEnd = true;
                            continue;
                        }

                        tempLine = line.Replace("*", "");
                        helpText += tempLine;
                        helpText += "\r\n";
                    }

                    if (line.Contains("/*:"))
                    {
                        if (Regex.IsMatch(line, Pattern))
                            continue;

                        foundStart = true;
                    }

                    if (foundStart)
                    {
                        if (line.Contains("@help"))
                        {
                            foundHelp = true;
                            if (line.Length > 10)
                            {
                                tempLine = line.Replace("@help","");
                                tempLine = tempLine.Replace("*", "");
                                helpText += tempLine;
                                helpText += "\r\n";
                                //break;
                            }
                            continue;
                        }
                    }
                }

                HelpTextBlock.Text = helpText;
                HelpTextScollViewer.ScrollToTop();
            }

        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            Get_Plugins();
            HelpTextBlock.Text = "";
        }

        private void Get_Plugins()
        {
            PluginsList.Clear();
            lbPluginList.SelectedItem = null;
            lbPluginList.ItemsSource = null;

            string configFileText = "";
            string[] pluginConfigFileContents = System.IO.File.ReadAllLines(PluginConfigFile);
            foreach (var line in pluginConfigFileContents)
            {
                if (line.StartsWith("{") || line.StartsWith("[") || line.StartsWith("]"))
                {
                    if (line.Contains("];"))
                    {
                        configFileText += line.Replace("];", "]");
                    }
                    else
                    {
                        configFileText += line.Replace(":true", ":\"true\"");
                    }
                }
            }
            int x = 0;
            JArray jsonObj = JArray.Parse(configFileText);
            string pluginName = "";
            JToken pluginStatus;
            for (int i = 0; i < jsonObj.Count; i++)
            {
                pluginStatus = jsonObj[i].First.Next;
                if (pluginStatus.ToString().Contains("true"))
                {
                    pluginName = "";
                    pluginName = jsonObj[i].First.ToString();
                    pluginName = pluginName.Replace("\"name\":", "");
                    pluginName = pluginName.Replace("\"", "");
                    pluginName = pluginName.Trim();
                    PluginsList.Add(new PluginItem() { Title = pluginName });
                }
            }

            lbPluginList.ItemsSource = PluginsList;
        }

        private void ProjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "RPG Maker Projects (*.rpgproject)|*.rpgproject";
            openFileDialog.InitialDirectory = Path.GetDirectoryName(ProjectFile);
            if (openFileDialog.ShowDialog() == true)
            {
                ProjectFile = openFileDialog.FileName;
                PluginsPath = Path.GetDirectoryName(ProjectFile) + @"\js\plugins";
                PluginConfigFile = Path.GetDirectoryName(ProjectFile) + @"\js\plugins.js";
                Get_Plugins();
                HelpTextBlock.Text = "";
            }
        }

        private void CmbFontName_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontName.SelectedItem != null)
            {
                if (HelpTextBlockActive)
                {
                    HelpTextBlock.FontFamily = new FontFamily(cmbFontName.SelectedItem.ToString());
                }
            }
        }

        private void CmbFontSize_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontSize.SelectedItem != null)
            {
                HelpFontSize = Int32.Parse(cmbFontSize.SelectedValue.ToString());
                HelpTextBlock.FontSize = HelpFontSize;
            }
        }

        private void TbBold_OnChecked(object sender, RoutedEventArgs e)
        {
            if (HelpTextBlockActive)
            {
                HelpTextBlock.FontWeight = FontWeights.Bold;
            }
        }

        private void TbBold_OnUnchecked(object sender, RoutedEventArgs e)
        {
            HelpTextBlock.FontWeight = FontWeights.Normal;
        }
    }
}
