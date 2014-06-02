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
    /// Interaction logic for IntSerie.xaml
    /// </summary>
    public partial class IntSerie : UserControl
    {
        private Serie se;
        private int num;
        private List<ScrollViewer> temporadas;
        private Boolean normal;
        private int tempActiva;
        public IntSerie(Serie s)
        {
            se = s;
            normal = true;
            InitializeComponent();
        }

        private void IntSeries_Initialized(object sender, EventArgs e)
        {
            //Fanart
            RenderOptions.SetBitmapScalingMode(FanArt, BitmapScalingMode.Fant);
            FanArt.Source = new BitmapImage(new Uri
            (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\FanArt\" + se.Nombre + ".jpg"));

            //Nombre
            Nombre.Text = se.Nombre;
            Nombre.Width = MeasureString(Nombre).Width + 20;
            RectNombre.Width = MeasureString(Nombre).Width + 20;

            //Episodio
            if (se.Temporada != 0 && se.Capitulo != 0)
                Episodio.Text = "S" + (se.Temporada < 10 ? "0" : "") + se.Temporada + "E" + (se.Capitulo < 10 ? "0" : "") + se.Capitulo;

            //NombreEp
         //   NombreEp.Text = se.Episodios[se.Temporada - 1][se.Capitulo - 1].NombreEp;

            //Vert
            Vert.Text = se.PorVer.ToString();

            //Descrt
            Desct.Text = se.Descargando.ToString();

            se.PropertyChanged += new PropertyChangedEventHandler(PropertyChanged);

            


        }

        public void AgregarEps()
        {
            //Episodios
            temporadas = new List<ScrollViewer>();
            tempActiva = se.Temporada - 1;
            int cont = tempActiva;
           for (int i = 0; i < se.Episodios.Count; i++)
            {
               //Se crea el StackPanel de cada temporada
                StackPanel aux = new StackPanel()
                {
                    Width = 350,
                    Height = 0,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,

                    CanVerticallyScroll = true,
                    
                };


               //Se crea el ScrollViewer de cada temporada
                ScrollViewer sv = new ScrollViewer()
                {
                    Margin = new Thickness(-350*cont, 46, 0, 0),
                    Width = 350,
                    Height = 150,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                   //Visibility = Visibility.Hidden,
                    Opacity = 1,
                };
            
               //Para cada episodio, se crea su stackpanel 
                foreach (Episodio ep in se.Episodios[i])
                {
                    string imagen;
                    //Dependiendo de su estado se pone la imagen
                    if (ep.Estado == 0) imagen = "Clock";
                    else if (ep.Estado == 1) imagen = "descs";
                    else if (ep.Estado == 2) imagen = "plays";
                    else imagen = "paloma";

                    //Se crea el stackpanel
                    StackPanel aux2 = new StackPanel()
                    {
                        Height = 17,
                        Width = 350,
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = System.Windows.VerticalAlignment.Top,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    };

                    //Se crea la imagen del estado
                    Image estado = new Image()
                    {
                        Stretch = Stretch.None,
                        Source = new BitmapImage(new Uri
                        (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\" + imagen + ".PNG")),
                        Margin = new Thickness(4,0,0,0),

                    };

                    //Se crea el texto de la imagen
                    TextBlock texto = new TextBlock()
                    {
                        Text = "E" + (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo + " - " + ep.NombreEp,
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(4, 0, 0, 0),
                        
                    };
                    aux2.Children.Add(estado);
                    aux2.Children.Add(texto);
                    aux.Height += 17;
                    aux.Children.Add(aux2);


                }
                sv.Content = aux;
                GPrinc.Children.Add(sv);
                temporadas.Add(sv);
                cont--;
            }

           SeleccT.Text = "Temporada " + se.Temporada;
            
        }

        //Eventos
        private void ZonaC_MouseEnter(object sender, MouseEventArgs e)
        {
            //Todo se hace visible
            InfoRect.Visibility = System.Windows.Visibility.Visible;
            Play.Visibility = System.Windows.Visibility.Visible;
            Episodio.Visibility = System.Windows.Visibility.Visible;
            NombreEp.Visibility = System.Windows.Visibility.Visible;
            Eye.Visibility = System.Windows.Visibility.Visible;
            Desc.Visibility = System.Windows.Visibility.Visible;
            Vert.Visibility = System.Windows.Visibility.Visible;
            Desct.Visibility = System.Windows.Visibility.Visible;
            Up.Visibility = System.Windows.Visibility.Visible;

            //Todo se va para arriba
            TimeSpan tiempo = new TimeSpan(0, 0, 0, 0, 250);
            InfoRect.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(InfoRect.Margin.Left, 153, 0, 0), tiempo));
            Play.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Play.Margin.Left, 160, 0, 0), tiempo));
            Episodio.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Episodio.Margin.Left, 153, 0, 0), tiempo));
            NombreEp.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(NombreEp.Margin.Left, 175, 0, 0), tiempo));
            Eye.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Eye.Margin.Left, 168, 0, 0), tiempo));
            Desc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Desc.Margin.Left, 163, 0, 0), tiempo));
            Vert.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Vert.Margin.Left, 162, 0, 0), tiempo));
            Desct.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Desct.Margin.Left, 162, 0, 0), tiempo));
            Up.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Up.Margin.Left,166, 0, 0), tiempo));

        }

        private void ZonaC_MouseLeave(object sender, MouseEventArgs e)
        {
            TimeSpan tiempo = new TimeSpan(0, 0, 0, 0, 250);
            if (!Play.IsMouseOver && !Up.IsMouseOver)
            {
                InfoRect.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(InfoRect.Margin.Left, 197, 0, 0), tiempo));
                Play.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Play.Margin.Left, 204, 0, 0), tiempo));
                Episodio.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Episodio.Margin.Left, 197, 0, 0), tiempo));
                NombreEp.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(NombreEp.Margin.Left, 219, 0, 0), tiempo));
                Eye.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Eye.Margin.Left, 212, 0, 0), tiempo));
                Desc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Desc.Margin.Left, 207, 0, 0), tiempo));
                Vert.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Vert.Margin.Left, 206, 0, 0), tiempo));
                Desct.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Desct.Margin.Left, 206, 0, 0), tiempo));
                Up.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Up.Margin.Left, 210, 0, 0), tiempo));
            }
       
 }

        private void Up_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Se declaran animacion de subida
            DoubleAnimation anima = new DoubleAnimation(170, new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation anima2 = new ThicknessAnimation(new Thickness(0, 27, 0, 0), new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation anima3 = new ThicknessAnimation(new Thickness(Up.Margin.Left, 5, 0, 0), new TimeSpan(0, 0, 0, 0, 250));

            //Se declaran animaciones de bajada
            DoubleAnimation animar = new DoubleAnimation(44, new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation anima2r = new ThicknessAnimation(new Thickness(0, 153, 0, 0), new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation anima3r = new ThicknessAnimation(new Thickness(Up.Margin.Left, 166, 0, 0), new TimeSpan(0, 0, 0, 0, 250));

            //Se declara animacioniones de rotacion
            RotateTransform m = new RotateTransform();
            Storyboard storyboard = new Storyboard();
            Storyboard story2 = new Storyboard();
            Up.RenderTransform = m;
            Up.RenderTransformOrigin = new Point(0.5, 0.5);
            DoubleAnimation rotateAnimation = new DoubleAnimation()
            {
                From = 0,
                To = 180,
                Duration = new TimeSpan(0, 0, 0, 0, 250),
            };
            DoubleAnimation rotateAnimation2 = new DoubleAnimation()
            {
                From = 180,
                To = 0,
                Duration = new TimeSpan(0, 0, 0, 0, 250),
            };
            Storyboard.SetTarget(rotateAnimation, Up);
            Storyboard.SetTarget(rotateAnimation2, Up);
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            Storyboard.SetTargetProperty(rotateAnimation2, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            storyboard.Children.Add(rotateAnimation);
            story2.Children.Add(rotateAnimation2);

            //Se declaran animaciones de desaparicion
            DoubleAnimation opac = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            DoubleAnimation opac2 = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));

            //La Opacidad vuelve a 100
            DoubleAnimation opac3 = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250));
            DoubleAnimation opac4 = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));


            if (normal)
            {

                ZonaC.Visibility = System.Windows.Visibility.Hidden;
                //Animacion de sube todo
                InfoRect.BeginAnimation(HeightProperty, anima);
                InfoRect.BeginAnimation(MarginProperty, anima2);
                Up.BeginAnimation(MarginProperty, anima3);

                //Animacion de rotacion de flecha
                storyboard.Begin();
                //Evento de termino de animacion
                opac2.Completed += (sender2, e2) =>
                {
                    //Todo se pone invisible
                    Play.Visibility = System.Windows.Visibility.Hidden;
                    Episodio.Visibility = System.Windows.Visibility.Hidden;
                    NombreEp.Visibility = System.Windows.Visibility.Hidden;
                    Eye.Visibility = System.Windows.Visibility.Hidden;
                    Desc.Visibility = System.Windows.Visibility.Hidden;
                    Vert.Visibility = System.Windows.Visibility.Hidden;
                    Desct.Visibility = System.Windows.Visibility.Hidden;

                    //Las temporadas se vuelven visibles
                    temporadas[tempActiva].Visibility = Visibility.Visible;
                    SeleccT.Visibility = Visibility.Visible;
                    Ant.Visibility = Visibility.Visible;
                    Sig.Visibility = Visibility.Visible;
                    temporadas[se.Temporada - 1].BeginAnimation(ScrollViewer.OpacityProperty, opac3);
                    SeleccT.BeginAnimation(TextBlock.OpacityProperty, opac3);
                    Ant.BeginAnimation(Image.OpacityProperty, opac3);
                    Sig.BeginAnimation(Image.OpacityProperty, opac3);

                };
                //Animacion de opacidad
                Play.BeginAnimation(OpacityProperty, opac);
                Episodio.BeginAnimation(OpacityProperty, opac);
                NombreEp.BeginAnimation(OpacityProperty, opac);
                Eye.BeginAnimation(OpacityProperty, opac);
                Desc.BeginAnimation(OpacityProperty, opac);
                Vert.BeginAnimation(OpacityProperty, opac);
                Desct.BeginAnimation(OpacityProperty, opac2);


                normal = false;
            }
            else
            {
                ZonaC.Visibility = System.Windows.Visibility.Visible;
                //Animacion de baja todo
                InfoRect.BeginAnimation(HeightProperty, animar);
                InfoRect.BeginAnimation(MarginProperty, anima2r);
                Up.BeginAnimation(MarginProperty, anima3r);
                //Animacion de rotacion de flecha
                story2.Begin();


                //Todo se va hasta abajo
                TimeSpan tiempo = new TimeSpan(0, 0, 0, 0, 250);
                InfoRect.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(InfoRect.Margin.Left, 197, 0, 0), tiempo));
                Play.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Play.Margin.Left, 204, 0, 0), tiempo));
                Episodio.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Episodio.Margin.Left, 197, 0, 0), tiempo));
                NombreEp.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(NombreEp.Margin.Left, 219, 0, 0), tiempo));
                Eye.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Eye.Margin.Left, 212, 0, 0), tiempo));
                Desc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Desc.Margin.Left, 207, 0, 0), tiempo));
                Vert.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Vert.Margin.Left, 206, 0, 0), tiempo));
                Desct.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Desct.Margin.Left, 206, 0, 0), tiempo));
                Up.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Up.Margin.Left, 210, 0, 0), tiempo));

                Play.BeginAnimation(OpacityProperty, opac3);
                Episodio.BeginAnimation(OpacityProperty, opac3);
                NombreEp.BeginAnimation(OpacityProperty, opac3);
                Eye.BeginAnimation(OpacityProperty, opac3);
                Desc.BeginAnimation(OpacityProperty, opac3);
                Vert.BeginAnimation(OpacityProperty, opac3);
                Desct.BeginAnimation(OpacityProperty, opac3);
                Play.BeginAnimation(OpacityProperty, opac3);

                //Las temporadas se vuelven invisibles
                opac4.Completed += (sender3, e3) =>
                {
                    temporadas[tempActiva].Visibility = Visibility.Hidden;
                    SeleccT.Visibility = Visibility.Hidden;
                    Ant.Visibility = Visibility.Hidden;
                    Sig.Visibility = Visibility.Hidden;
                };
                temporadas[se.Temporada - 1].BeginAnimation(ScrollViewer.OpacityProperty, opac);
                SeleccT.BeginAnimation(TextBlock.OpacityProperty, opac);
                Ant.BeginAnimation(Image.OpacityProperty, opac);
                Sig.BeginAnimation(Image.OpacityProperty, opac4);


               


                normal = true;

            }


        }

        private void Sig_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            temporadas[tempActiva].Visibility = System.Windows.Visibility.Hidden;
            tempActiva++;
            temporadas[tempActiva].Visibility = System.Windows.Visibility.Visible;
            SeleccT.Text = "Temporada " + (tempActiva + 1);

            if (tempActiva + 1 == se.Episodios.Count)
            {
                Sig.Visibility = System.Windows.Visibility.Hidden;
            }

            Ant.Visibility = System.Windows.Visibility.Visible;

        }

        private void Ant_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int i = 0;
            foreach (ScrollViewer s in temporadas)
            {
                s.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(s.Margin.Left + 350, s.Margin.Top, 0, 0), new TimeSpan(0,0,0,0,350)));
                i++;
            }
            /*
            temporadas[tempActiva].Visibility = System.Windows.Visibility.Hidden;
            tempActiva--;
            temporadas[tempActiva].Visibility = System.Windows.Visibility.Visible;
            temporadas[tempActiva].Opacity = 1;
            SeleccT.Text = "Temporada " + (tempActiva + 1);

            if (tempActiva == 0)
            {
                Ant.Visibility = System.Windows.Visibility.Hidden;
            }

            Sig.Visibility = System.Windows.Visibility.Visible;
             */

        }


        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Descargando")
                Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (ThreadStart)delegate { Desct.Text = se.Descargando.ToString(); });
            else
                if (e.PropertyName == "Temporada")
                    Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (ThreadStart)delegate { Episodio.Text = "S" + (se.Temporada < 10 ? "0" : "") + se.Temporada + "E" + (se.Capitulo < 10 ? "0" : "") + se.Capitulo;
                                             
                                             });
                else
                    if (e.PropertyName == "Capitulo")
                        Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        (ThreadStart)delegate { Episodio.Text = "S" + (se.Temporada < 10 ? "0" : "") + se.Temporada + "E" + (se.Capitulo < 10 ? "0" : "") + se.Capitulo;
                                                NombreEp.Text = se.Episodios[se.Temporada - 1][se.Capitulo - 1].NombreEp;
                                                });
                    else
                        if (e.PropertyName == "PorVer")
                            Application.Current.Dispatcher.Invoke(
                            DispatcherPriority.Normal,
                            (ThreadStart)delegate { Vert.Text = se.PorVer.ToString(); });



        }

        //Funciones
        private Size MeasureString(TextBlock textblock)
        {
            var formattedText = new FormattedText(
                textblock.Text,
                System.Globalization.CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(textblock.FontFamily, textblock.FontStyle, textblock.FontWeight, textblock.FontStretch),
                textblock.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

    }
}
