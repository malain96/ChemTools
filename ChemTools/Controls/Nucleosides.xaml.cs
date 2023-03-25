using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChemTools.Controls
{
    /// <summary>
    /// Interaction logic for Nucleosides.xaml
    /// </summary>
    public partial class Nucleosides : UserControl
    {
        public NucleosideSettings NucleosideSettings { get; set; }

        public Nucleosides()
        {
            InitializeComponent();
        }

        //TODO When same letters but different order -> filter and only keep one (break character by character and order and compare - for example U>PA and A>PU becomes APU>) - Test 996.1201 - G>PG>PU>P
        //TODO Handle carbomade - Remove CONH2 from PhosphateGroup and create new one - Test 372.0583 - A>Pcarb

        private void BtnCalculateMass_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var massResult = Math.Round(double.Parse(tbMass.Text), 4);
            var possibleNucleotides = CalculateNucleotides(0, null, massResult);
            for (var counter = 1; !possibleNucleotides.Any(x => x.ErrorMargin >= -8 && x.ErrorMargin <= 8); counter++)
            {
                var listToAdd = new List<Nucleotide>();
                foreach (var previousNucleotide in possibleNucleotides.Where(x => x.Count == counter - 1))
                {
                    listToAdd.AddRange(CalculateNucleotides(counter, previousNucleotide, massResult));
                }
                possibleNucleotides.AddRange(listToAdd);
            }
            lvPossibleNucleotides.ItemsSource = possibleNucleotides.Where(x => x.ErrorMargin >= -8 && x.ErrorMargin <= 8).OrderByDescending(x => x.ErrorMargin);
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private List<Nucleotide> CalculateNucleotides(int counter, Nucleotide previousNucleotide, double massResult)
        {
            const double hydrogeneMass = 1.0078;
            var massStandard = previousNucleotide?.Mass ?? 0;
            var code = previousNucleotide?.Code ?? string.Empty;
            var nucleotides = new List<Nucleotide>();

            if (counter % 2 == 0)
            {
                foreach (var nuc in NucleosideSettings.Nucleosides)
                {
                    var mass = Math.Round(massStandard, 4) + Math.Round(nuc.Mass, 4);
                    nucleotides.Add(new Nucleotide
                    {
                        Code = code + nuc.Code,
                        Mass = Math.Round(mass, 4),
                        ErrorMargin = Math.Round((mass - massResult) / mass * 1000000, 4),
                        Count = counter
                    });
                }
            }
            else
            {
                foreach (var pg in NucleosideSettings.PhosphateGroups)
                {
                    var mass = Math.Round(massStandard, 4) + Math.Round(pg.Mass, 4) - Math.Round(hydrogeneMass * pg.HydrogenCount, 4);
                    nucleotides.Add(new Nucleotide
                    {
                        Code = code + pg.Code,
                        Mass = Math.Round(mass, 4),
                        ErrorMargin = Math.Round((mass - massResult) / mass * 1000000, 4),
                        Count = counter
                    });
                }
            }

            return nucleotides;
        }

        private void TbMass_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.,][0-9]+$|^[0-9]*[.,]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox)?.Text.Insert((sender as TextBox)?.SelectionStart ?? 0, e.Text));
        }
    }

    public class Nucleotide
    {
        public string Code { get; set; }
        public double Mass { get; set; }
        public double ErrorMargin { get; set; }
        public int Count { get; set; }
    }
}