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
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filename in openFileDialog.FileNames)
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

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if(lbFiles.Items.Count == 0)
            {
                MessageBox.Show("Please select at least one file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                var chromatograms = ParseChromatogramFiles();
                var file = GenerateChromatogramExcel(chromatograms);
                MessageBox.Show(string.Format("File generated: {0}", file), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("An error occured during the generation", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {

                Mouse.OverrideCursor = Cursors.Arrow;
            }


        }

        private List<List<ChromatogramItem>> ParseChromatogramFiles()
        {
            var chromatograms = new List<List<ChromatogramItem>>();
            foreach (string file in lbFiles.Items)
            {
                var reader = File.OpenText(file);
                string line;
                var parse = false;
                var chromatogram = new List<ChromatogramItem>();
                while ((line = reader.ReadLine()) != null)
                {
                    var items = line.Split('\t');
                    if (items[0] == "Time (min)")
                        parse = true;
                    else if (parse)
                    {
                        chromatogram.Add(new ChromatogramItem
                        {
                            Time = decimal.Parse(items[0]),
                            Value = decimal.Parse(items[2]),
                        });
                    }

                }
                chromatograms.Add(chromatogram);
            }
            return chromatograms;
        }

        private string GenerateChromatogramExcel(List<List<ChromatogramItem>> chromatograms)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var directory = string.Format("{0}\\ChemToolsFiles", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var fileName = string.Format("{0}\\chromatogram-{1}.xlsx", directory, DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            var newFile = new FileInfo(fileName);
            using (var xlPackage = new ExcelPackage(newFile))
            {
                var wsChart = xlPackage.Workbook.Worksheets.Add("Chart");
                var wsData = xlPackage.Workbook.Worksheets.Add("Data");
                var chartPressure = wsChart.Drawings.AddChart("PressureChart", eChartType.XYScatterLines);
                chartPressure.SetSize(800, 400);
                var increment = string.IsNullOrEmpty(tbIncrement.Text) ? 0 : decimal.Parse(tbIncrement.Text);
                var count = 0;
                foreach (var chromatogram in chromatograms)
                {
                    var col = count * 3;
                    var colTime = col + 1;
                    var colValue = col + 2;
                    for (var i = 0; i < chromatogram.Count; i++)
                    {
                        wsData.Cells[i + 1, colTime].Value = chromatogram[i].Time;
                        wsData.Cells[i + 1, colValue].Value = chromatogram[i].Value + (increment * count);
                    }
                    var serie = (ExcelScatterChartSerie)chartPressure.Series.Add(wsData.Cells[1, colValue, chromatogram.Count, colValue], wsData.Cells[1, colTime, chromatogram.Count, colTime]);
                    serie.Header = count.ToString();
                    count++;
                }
                xlPackage.Save();
            }
            return fileName;
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
    }
}
