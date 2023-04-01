using ChemTools.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChemTools.Controls
{
    //TODO Update readme

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
            lvPossibleNucleotides.ItemsSource = possibleNucleotides.DistinctBy(x => x.OderedCode).Where(x => x.ErrorMargin >= -8 && x.ErrorMargin <= 8).OrderByDescending(x => x.ErrorMargin);
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private List<Nucleotide> CalculateNucleotides(int counter, Nucleotide previousNucleotide, double massResult)
        {
            var massStandard = previousNucleotide?.Mass ?? 0;
            var code = previousNucleotide?.Code ?? string.Empty;
            var nucleotides = new List<Nucleotide>();

            if (counter % 2 == 0)
            {
                foreach (var nuc in NucleosideSettings.Nucleosides)
                {
                    nucleotides.Add(new Nucleotide
                    {
                        Code = code + nuc.Code,
                        Mass = CalculateMass(nuc.Mass, massStandard),
                        MassResult = massResult,
                        Count = counter
                    });
                }
            }
            else
            {
                foreach (var pg in NucleosideSettings.PhosphateGroups)
                {
                    nucleotides.Add(new Nucleotide
                    {
                        Code = code + pg.Code,
                        Mass = CalculateMass(pg.Mass, massStandard, pg.HydrogenCount),
                        MassResult = massResult,
                        Count = counter
                    });
                }
            }

            var addedNucleotides = nucleotides.Where(x => x.Count == counter).ToList();
            foreach (var carb in NucleosideSettings.Carbamates)
            {
                foreach (var nuc in addedNucleotides)
                {
                    nucleotides.Add(new Nucleotide
                    {
                        Code = code + carb.Code,
                        Mass = CalculateMass(carb.Mass, massStandard, carb.HydrogenCount),
                        MassResult = massResult,
                        Count = counter
                    });
                }
            }

            return nucleotides;
        }

        private static double CalculateMass(double mass, double massStandard, int hydrogeneCount = 0)
        {
            return Math.Round(massStandard, 4) + Math.Round(mass, 4) - Math.Round(1.0078 * hydrogeneCount, 4);
        }

        private void TbMass_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("^[.,][0-9]+$|^[0-9]*[.,]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox)?.Text.Insert((sender as TextBox)?.SelectionStart ?? 0, e.Text));
        }
    }
}