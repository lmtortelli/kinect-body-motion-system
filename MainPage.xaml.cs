using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace LightBuzz.Vituvius.Exergames.Kimos.WPF
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void Angle_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AnglePage());
        }


        private void JointSelection_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new JointSelectionPage());
        }

        private void Recording_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RecordingPage());
        }

    }
}
