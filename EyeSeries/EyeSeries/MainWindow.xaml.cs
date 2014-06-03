using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EyeSeries
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            

        }

        private void EyeSeries_Initialized(object sender, EventArgs e)
        {
            Serie s = new Serie(76290, "24", 0, 'c', "9:00 PM");

            IntSerie so = new IntSerie(s);
            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (sp, ee) =>
                {
                    s.AlimentarEps(9, 6);
                    
                };
            b.RunWorkerCompleted += (Epa, lala) =>
                {
                    so.AgregarEps();
                };
            b.RunWorkerAsync();
           
            gPrinc.Children.Add(so);

        }
    }
}
