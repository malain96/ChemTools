using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ChemTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow(AppSettings appSettings, NucleosideSettings nucleosideSettings)
        {
            InitializeComponent();
            Title = $"ChemTools - v{Assembly.GetExecutingAssembly().GetName().Version}";

            UcBenzoicAcid.Settings = appSettings;
            UcBenzoicAcid.Visibility = Visibility.Visible;

            UcNucleosides.NucleosideSettings = nucleosideSettings;
            UcNucleosides.Visibility = Visibility.Collapsed;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UcBenzoicAcid.Visibility = Visibility.Collapsed;
            UcNucleosides.Visibility = Visibility.Collapsed;
            UcTripletsFinder.Visibility = Visibility.Collapsed;
            switch (((MenuItem)sender).Name)
            {
                case "MiBenzoicAcid":
                    UcBenzoicAcid.Visibility = Visibility.Visible;
                    break;

                case "MiNucleosides":
                    UcNucleosides.Visibility = Visibility.Visible;
                    break;

                case "MiTripletsFinder":
                    UcTripletsFinder.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}