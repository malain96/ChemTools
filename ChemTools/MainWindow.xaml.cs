using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChemTools.Models;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace ChemTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = "ChemTools";
        }

        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                    lbFiles.Items.Add(filename);
            }
        }

        private void lbFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {

                if (lbFiles.SelectedIndex != -1)
                {
                    for (int i = lbFiles.SelectedItems.Count - 1; i >= 0; i--)
                        lbFiles.Items.Remove(lbFiles.SelectedItems[i]);
                }
            }
        }

        private void btnChromatogram_Click(object sender, RoutedEventArgs e)
        {
            OnClick(Actions.Chromatogram);
        }

        private void tbIncrement_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.,][0-9]+$|^[0-9]*[.,]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            lbFiles.Items.Clear();
        }

        private void btnIntegration_Click(object sender, RoutedEventArgs e)
        {
            OnClick(Actions.Integration);
        }

        private void OnClick(Actions action)
        {
            if (lbFiles.Items.Count == 0)
            {
                MessageBox.Show("Please select at least one file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                string file = "";
                switch (action)
                {
                    case Actions.Chromatogram:
                        file = HandleChromatogram();
                        break;
                    case Actions.Integration:
                        file = HandleIntegration();
                        break;
                }
                MessageBox.Show(string.Format("File generated: {0}", file), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("An error occured - Blame Nastia!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private string HandleChromatogram()
        {
            Dictionary<string, List<ChromatogramItem>> chromatograms = ParseChromatogramFiles();
            return GenerateChromatogramExcel(chromatograms);
        }

        private string HandleIntegration()
        {
            Dictionary<string, List<IntegrationItem>> integrations = ParseIntegrationFiles();
            return GenerateIntegrationExcel(integrations);
        }

        private Dictionary<string, List<string[]>> ReadFiles()
        {
            Dictionary<string, List<string[]>> data = new Dictionary<string, List<string[]>>();
            foreach (string file in lbFiles.Items)
            {
                StreamReader reader = File.OpenText(file);
                string line;
                List<string[]> lines = new List<string[]>();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] items = line.Split('\t');
                    lines.Add(items);

                }
                data.Add(Path.GetFileNameWithoutExtension(file), lines);
            }
            return data;
        }

        private Dictionary<string, List<ChromatogramItem>> ParseChromatogramFiles()
        {
            Dictionary<string, List<ChromatogramItem>> chromatograms = new Dictionary<string, List<ChromatogramItem>>();
            Dictionary<string, List<string[]>> data = ReadFiles();
            foreach (KeyValuePair<string, List<string[]>> file in data)
            {
                bool parse = false;
                List<ChromatogramItem> chromatogram = new List<ChromatogramItem>();
                foreach (string[] items in file.Value)
                {
                    if (items[0] == "Time (min)")
                        parse = true;
                    else if (parse)
                    {
                        chromatogram.Add(new ChromatogramItem
                        {
                            Time = ParseDecimal(items[0]).Value,
                            Value = ParseDecimal(items[2]).Value,
                        });
                    }
                }
                chromatograms.Add(file.Key, chromatogram);
            }
            return chromatograms;
        }

        private Dictionary<string, List<IntegrationItem>> ParseIntegrationFiles()
        {
            Dictionary<string, List<IntegrationItem>> intergrations = new Dictionary<string, List<IntegrationItem>>();
            Dictionary<string, List<string[]>> data = ReadFiles();
            foreach (KeyValuePair<string, List<string[]>> file in data)
            {
                bool parse = false;
                List<IntegrationItem> intergration = new List<IntegrationItem>();
                foreach (string[] items in file.Value)
                {
                    if (items[0].Trim() == "1")
                        parse = true;
                    if (parse && IsNumeric(items[0]))
                    {
                        intergration.Add(new IntegrationItem
                        {
                            Number = int.Parse(items[0]),
                            RetentionTime = ParseDecimal(items[2]),
                            Area = ParseDecimal(items[3]),
                            Height = ParseDecimal(items[4]),
                            RelativeArea = ParseDecimal(items[5]),
                            RelativeHeight = ParseDecimal(items[6]),
                        });
                    }
                }
                intergrations.Add(file.Key, intergration);
            }
            return intergrations;
        }

        private string GenerateChromatogramExcel(Dictionary<string, List<ChromatogramItem>> chromatograms)
        {

            string fileName = GetFileNameWithTimestamp("chromatogram");
            FileInfo newFile = new FileInfo(fileName);
            using (ExcelPackage xlPackage = new ExcelPackage(newFile))
            {
                ExcelWorksheet wsChart = xlPackage.Workbook.Worksheets.Add("Chart");
                ExcelWorksheet wsData = xlPackage.Workbook.Worksheets.Add("Data");
                ExcelChart chart = wsChart.Drawings.AddChart("PressureChart", eChartType.XYScatterLinesNoMarkers);
                chart.SetSize(800, 400);
                decimal incrementY = string.IsNullOrEmpty(tbIncrementY.Text) ? 0 : decimal.Parse(tbIncrementY.Text);
                decimal incrementX = string.IsNullOrEmpty(tbIncrementX.Text) ? 0 : decimal.Parse(tbIncrementX.Text);
                int count = 0;
                foreach (KeyValuePair<string, List<ChromatogramItem>> chromatogram in chromatograms)
                {
                    int col = count * 3;
                    int colTime = col + 1;
                    int colValue = col + 2;
                    int itemCount = chromatogram.Value.Count;
                    for (int i = 0; i < itemCount; i++)
                    {
                        wsData.Cells[i + 1, colTime].Value = chromatogram.Value[i].Time + (incrementX * count);
                        wsData.Cells[i + 1, colValue].Value = chromatogram.Value[i].Value + (incrementY * count);
                    }
                    ExcelScatterChartSerie serie = (ExcelScatterChartSerie)chart.Series.Add(wsData.Cells[1, colValue, itemCount, colValue], wsData.Cells[1, colTime, itemCount, colTime]);
                    serie.Header = chromatogram.Key;
                    count++;
                }
                xlPackage.Save();
            }
            return fileName;
        }

        private string GenerateIntegrationExcel(Dictionary<string, List<IntegrationItem>> integrations)
        {
            string fileName = GetFileNameWithTimestamp("integration");
            FileInfo newFile = new FileInfo(fileName);
            using (ExcelPackage xlPackage = new ExcelPackage(newFile))
            {
                foreach (KeyValuePair<string, List<IntegrationItem>> integration in integrations)
                {

                    ExcelWorksheet ws = xlPackage.Workbook.Worksheets.Add(integration.Key);
                    ws.Cells[1, 1].Value = "No.";
                    ws.Cells[1, 2].Value = "Retention Time";
                    ws.Cells[1, 3].Value = "item.Area";
                    ws.Cells[1, 4].Value = "item.Height";
                    ws.Cells[1, 5].Value = "item.RelativeArea";
                    ws.Cells[1, 6].Value = "item.RelativeHeight";
                    int row = 2;
                    foreach(IntegrationItem item in integration.Value)
                    {
                        ws.Cells[row, 1].Value = item.Number;
                        ws.Cells[row, 2].Value = ConvertNullable(item.RetentionTime);
                        ws.Cells[row, 3].Value = ConvertNullable(item.Area);
                        ws.Cells[row, 4].Value = ConvertNullable(item.Height);
                        ws.Cells[row, 5].Value = ConvertNullable(item.RelativeArea);
                        ws.Cells[row, 6].Value = ConvertNullable(item.RelativeHeight);
                        row++;
                    }
                }
                xlPackage.Save();
            }
            return fileName;
        }

        private string GetFileNameWithTimestamp(string name)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string directory = string.Format("{0}\\ChemToolsFiles", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return string.Format("{0}\\{1}-{2}.xlsx", directory, name,DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        }

        private bool IsNumeric(string s)
        {
            return float.TryParse(s, out _);
        }

        private decimal? ParseDecimal(string str)
        {
            bool isNumeric = decimal.TryParse(Regex.Replace(str, @"\s", ""), out decimal n);
            if (!isNumeric)
                return null;
            return n;
        }

        private object ConvertNullable(object val)
        {
            return val ?? "n.a.";
        }

        enum Actions
        {
            Integration,
            Chromatogram,
        }
    }
}
