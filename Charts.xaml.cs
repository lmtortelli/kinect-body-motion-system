using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;

namespace LightBuzz.Vituvius.Exergames.Kimos.WPF
{
    /// <summary>
    /// Lógica interna para Charts.xaml
    /// </summary>
    public partial class Charts : Page
    {


        public Charts()
        {
            InitializeComponent();
        }

        public void AtualizaGrafico(SeriesCollection newCollection) {
            //chart1.Series = newCollection;
        }


    }
}
