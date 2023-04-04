using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ChemTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AppSettings _appSettings;
        private readonly NucleosideSettings _nucleosideSettings;

        public MainWindow(AppSettings appSettings, NucleosideSettings nucleosideSettings)
        {
            InitializeComponent();
            _appSettings = appSettings;
            _nucleosideSettings = nucleosideSettings;
            Title = $"ChemTools - v{Assembly.GetExecutingAssembly().GetName().Version}";

            ucBenzoicAcid.Settings = _appSettings;
            ucBenzoicAcid.Visibility = Visibility.Visible;

            ucNucleosides.NucleosideSettings = _nucleosideSettings;
            ucNucleosides.Visibility = Visibility.Collapsed;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ucBenzoicAcid.Visibility = Visibility.Collapsed;
            ucNucleosides.Visibility = Visibility.Collapsed;
            switch (((MenuItem)sender).Name)
            {
                case "miBenzoicAcid":
                    ucBenzoicAcid.Visibility = Visibility.Visible;
                    break;

                case "miNucleosides":
                    ucNucleosides.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}