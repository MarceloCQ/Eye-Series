using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EyeSeries
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class InterfazDesgloseEp : UserControl
    {
        Episodio Episodio;
        IntSerie InterfazSerie;

        public InterfazDesgloseEp(Episodio ep, IntSerie intS)
        {
            InitializeComponent();
            Episodio = ep;
            InterfazSerie = intS;


            string imagen; 
            //Dependiendo de su estado se pone la imagen
            if (ep.Estado == 0) imagen = "Clocks";
            else if (ep.Estado == 1) imagen = "descs";
            else if (ep.Estado == 2) imagen = "plays";
            else imagen = "paloma";

            Estado.Source = new BitmapImage(new Uri
                (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\" + imagen + ".PNG"));
            Estado.Tag = ep;

            Nombre.Text = "E" + (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo + " - " + ep.NombreEp;

            if (ep.Estado == 0) Nombre.Text += " - " + ep.Fecha.ToShortDateString();
            else if (ep.Estado == 1)
            {
                double progreso = InterfazSerie.getProgresoTorrent(ep.Hash);
                Nombre.Text += " - " + progreso + "%";
                BarraProgreso.Visibility = System.Windows.Visibility.Visible;
                BarraProgreso.Width = progreso * 3.35;
            }

            ep.PropertyChanged += PropertyChangedEpisodio;



        }

        private void Estado_MouseUp(object sender, MouseButtonEventArgs e)
        {
            InterfazSerie.cambiarEpPrincipal(Episodio);
        }

        private void PropertyChangedEpisodio(object sender, PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(
           DispatcherPriority.Normal,
           (ThreadStart)delegate
           {

               switch (e.PropertyName)
               {
                   case "Estado":
                       string imagen;
                       Nombre.Text = "E" + (Episodio.Capitulo < 10 ? "0" : "") + Episodio.Capitulo + " - " + Episodio.NombreEp;
                       
                   switch (Episodio.Estado)
                       {
    
                           case 1:
                               imagen = "descs.png";
                               Nombre.Text += " - 0%";
                               BarraProgreso.Visibility = System.Windows.Visibility.Visible;
                               break;

                           case 2:

                               DoubleAnimation desapareceProg = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250));
                               desapareceProg.Completed += (sender2, e2) =>
                                   {
                                       BarraProgreso.Visibility = System.Windows.Visibility.Hidden;
                                       BarraProgreso.Width = 0;
                                       BarraProgreso.BeginAnimation(OpacityProperty, new DoubleAnimation(0.2, TimeSpan.FromMilliseconds(250)));

                                   };
                               BarraProgreso.BeginAnimation(OpacityProperty, desapareceProg);
                               imagen = "plays.png";                              
                               break;

                           default:
                               imagen = "paloma.png";
                               break;
                       }

                       Estado.Source = new BitmapImage(new Uri
                         (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\" + imagen));
                       
                         break;

                   case "NombreEp":
                       if (Episodio.Estado == 0)
                       {
                           Nombre.Text = "E" + (Episodio.Capitulo < 10 ? "0" : "") + Episodio.Capitulo + " - " + Episodio.NombreEp + " - " + Episodio.Fecha.ToShortDateString();
                       }
                       else
                       {
                           Nombre.Text = "E" + (Episodio.Capitulo < 10 ? "0" : "") + Episodio.Capitulo + " - " + Episodio.NombreEp;
                       }
                       break;

                   case "Fecha":
                       Nombre.Text = "E" + (Episodio.Capitulo < 10 ? "0" : "") + Episodio.Capitulo + " - " + Episodio.NombreEp + " - " + Episodio.Fecha.ToShortDateString();
                       break;
               }
           });
        }

        public void ActualizarPorcentaje(double porcentaje)
        {
            double progreso = InterfazSerie.getProgresoTorrent(Episodio.Hash);
            Nombre.Text = "E" + (Episodio.Capitulo < 10 ? "0" : "") + Episodio.Capitulo + " - " + Episodio.NombreEp + " - " + porcentaje + "%";
            BarraProgreso.Width = progreso * 3.35;
        }


    }
}
