using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChemTools.Entities;
using Microsoft.Win32;
using OfficeOpenXml;

namespace ChemTools.Controls;

public partial class TripletsFinder
{
    private const double DefaultTolerance = 0.1;
    private const double DefaultMinDistance = 5;
    private const double DefaultMaxDistance = 9;

    public TripletsFinder()
    {
        InitializeComponent();
        TbTolerance.Text = DefaultTolerance.ToString(CultureInfo.InvariantCulture);
        TbMinDistance.Text = DefaultMinDistance.ToString(CultureInfo.InvariantCulture);
        TbMaxDistance.Text = DefaultMaxDistance.ToString(CultureInfo.InvariantCulture);
    }
    
    private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
    {
        var filePaths = OpenFilePicker();
        if (filePaths.Length == 0)
            return;

        var peaks = LoadPeaksFromExcel(filePaths[0]);
        if (peaks.Count == 0)
        {
            MessageBox.Show("No valid peaks found in the file.", "Warning", MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var tolerance = ParseNumericTextBoxOrDefault(TbTolerance, DefaultTolerance);
        var minDistance = ParseNumericTextBoxOrDefault(TbMinDistance, DefaultMinDistance);
        var maxDistance = ParseNumericTextBoxOrDefault(TbMaxDistance, DefaultMaxDistance);
        var triplets = FindTriplets(peaks, tolerance, minDistance, maxDistance);

        LvPeaks.ItemsSource = triplets;
    }

    private static string[] OpenFilePicker()
    {
        var openFileDialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
        };

        return openFileDialog.ShowDialog() == true ? openFileDialog.FileNames : []
        ;
    }

    private static List<double> LoadPeaksFromExcel(string filePath)
    {
        var peaks = new List<double>();
        var fileInfo = new FileInfo(filePath);

        using var package = new ExcelPackage(fileInfo);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null) return peaks;

        var startRow = 2;
        var totalRows = worksheet.Dimension?.Rows ?? 0;

        ExtractPeaksFromColumn(worksheet, startRow, totalRows, 3, peaks); // Column D
        ExtractPeaksFromColumn(worksheet, startRow, totalRows, 13, peaks); // Column N

        return peaks;
    }

    private static void ExtractPeaksFromColumn(ExcelWorksheet ws, int startRow, int totalRows, int col,
        List<double> peaks)
    {
        for (var row = startRow; row <= totalRows; row++)
        {
            var type = ws.Cells[row, col + 4].Text; // Column + 4 to check for "Impurity"
            if (!string.IsNullOrWhiteSpace(type) &&
                !type.Equals("Impurity", StringComparison.InvariantCultureIgnoreCase) &&
                double.TryParse(ws.Cells[row, col].Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                peaks.Add(value);
            }
        }
    }

    private static double ParseNumericTextBoxOrDefault(TextBox textBox, double defaultValue)
    {
        if (double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        textBox.Text = defaultValue.ToString(CultureInfo.InvariantCulture);
        return defaultValue;
    }

    private static List<Triplet> FindTriplets(List<double> peaks, double tolerance, double minDistance,
        double maxDistance)
    {
        var result = new List<Triplet>();
        var sortedPeaks = peaks.OrderBy(p => p).ToList();

        for (var i = 0; i < sortedPeaks.Count - 2; i++)
        {
            for (var j = i + 1; j < sortedPeaks.Count - 1; j++)
            {
                var delta1 = sortedPeaks[j] - sortedPeaks[i];
                if (delta1 < minDistance || delta1 > maxDistance) continue;

                for (var k = j + 1; k < sortedPeaks.Count; k++)
                {
                    var delta2 = sortedPeaks[k] - sortedPeaks[j];
                    var errorMargin = Math.Abs(delta1 - delta2);

                    if (errorMargin <= tolerance)
                    {
                        result.Add(new Triplet
                        {
                            Peak1 = sortedPeaks[i],
                            Peak2 = sortedPeaks[j],
                            Peak3 = sortedPeaks[k],
                            ErrorMargin = Math.Round(errorMargin, 2),
                            Distance = Math.Round(delta1, 2)
                        });
                    }
                }
            }
        }

        return result;
    }

    private void AnyNumber(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(c => char.IsDigit(c) || c == '.');
        base.OnPreviewTextInput(e);
    }
}