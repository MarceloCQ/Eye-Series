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
using UTorrentAPI;

namespace EyeSeries
{
    /// <summary>
    /// Interaction logic for IntSerie.xaml
    /// </summary>
    public partial class IntSerie : UserControl
    {
        public Serie se;
        private ScrollViewer sv;
        private List<StackPanel> temporadas;
        private StackPanel master;
        public Boolean normal;
        private Boolean desplegado;
        private int tempActiva;
        private UTorrentClient uClient;
        private MainWindow Principal;
        public IntSerie(Serie s, MainWindow m, UTorrentClient u)
        {
            se = s;
            normal = true;
            desplegado = true;
            Principal = m;
            uClient = u;

            InitializeComponent();
        }

        private void IntSeries_Initialized(object sender, EventArgs e)
        {
           

            


        }

        public void AgregarEps()
        {
            //Episodios
            temporadas = new List<StackPanel>();
            if (se.Temporada != 0)
            {
                tempActiva = se.Temporada - 1;
            }
            else
            {
                tempActiva = se.Episodios.Count - 1;
            }
            
            //Se crea el ScrollViewer
            sv = new ScrollViewer()
            {
                Margin = new Thickness(0, 46, 0, 0),
                Width = 350,
                Height = 151,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                //Visibility = Visibility.Hidden,
                Opacity = 0,
                Visibility = System.Windows.Visibility.Hidden,
            };

            master = new StackPanel()
            {
                Width = 350 * se.Episodios.Count,
                Margin = new Thickness(-350 * (tempActiva), 0, 0, 0),
                Height = 151,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Orientation = Orientation.Horizontal,
                

            };



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
                    Opacity = 0,
                    
                };

               //Para cada episodio, se crea su stackpanel 
                foreach (Episodio ep in se.Episodios[i])
                {
                    string imagen;
                    ep.PropertyChanged += new PropertyChangedEventHandler(PropertyChandeE);
                    //Dependiendo de su estado se pone la imagen
                    if (ep.Estado == 0) imagen = "Clocks";
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
                        Tag = ep,

                    };
                    estado.MouseUp += new MouseButtonEventHandler(Play_MouseUp);
                    //Se crea el texto de la imagen
                    TextBlock texto = new TextBlock()
                    {
                        Text = "E" + (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo + " - " + ep.NombreEp,
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(4, 0, 0, 0),
                        
                    };
                    if (ep.Estado == 0) texto.Text += " - " + ep.Fecha.ToShortDateString();
                    else if (ep.Estado == 1)
                    {

                        double prog = (double)uClient.Torrents[ep.Hash].ProgressInMils / 10.0;
                        texto.Text += " - " + Math.Round(prog, 1) + "%";
                    }
                    aux2.Children.Add(estado);
                    aux2.Children.Add(texto);
                    aux.Height += 17;
                    aux.Children.Add(aux2);


                }
                master.Children.Add(aux);
                temporadas.Add(aux);
                //cont--;
            }
           sv.Content = master;
           GPrinc.Children.Add(sv);
           master.Height = temporadas[tempActiva].Height;
           sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
           sv.UpdateLayout();
           sv.ScrollToVerticalOffset(sv.ExtentHeight - sv.ViewportHeight - 10);
            //Se pone la temporada seleccionada como texto
           SeleccT.Text = "Temporada " + (tempActiva + 1);
           Up.Visibility = System.Windows.Visibility.Visible;
           Up.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 500)));
            
        }

        public void AgregarEp(Episodio ep, bool nuevatemp)
        {
            StackPanel aux;
            if (nuevatemp)
            {
                master.Width += 350;
                master.Margin = new Thickness(master.Margin.Left - 350, 0, 0, 0);
               
                //Se crea el StackPanel de cada temporada
                aux = new StackPanel()
                {
                    Width = 350,
                    Height = 0,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    CanVerticallyScroll = true,
                    Opacity = 0,

                };
            }
            else aux = temporadas[temporadas.Count - 1];

            string imagen;
            ep.PropertyChanged += new PropertyChangedEventHandler(PropertyChandeE);
            //Dependiendo de su estado se pone la imagen
            if (ep.Estado == 0) imagen = "Clocks";
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
                Margin = new Thickness(4, 0, 0, 0),
                Tag = ep,

            };
            estado.MouseUp += new MouseButtonEventHandler(Play_MouseUp);
            //Se crea el texto de la imagen
            TextBlock texto = new TextBlock()
            {
                Text = "E" + (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo + " - " + ep.NombreEp,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(4, 0, 0, 0),

            };
            if (ep.Estado == 0) texto.Text += " - " + ep.Fecha.ToShortDateString();
            else if (ep.Estado == 1)
            {

                double prog = (double)uClient.Torrents[ep.Hash].ProgressInMils / 10.0;
                texto.Text += " - " + Math.Round(prog, 1) + "%";
            }
            aux2.Children.Add(estado);
            aux2.Children.Add(texto);
            aux.Height += 17;
            aux.Children.Add(aux2);
            if (nuevatemp)
            {
                master.Children.Add(aux);
                temporadas.Add(aux);
                if (se.Temporada == se.Episodios.Count)
                {
                    tempActiva = se.Episodios.Count - 1;
                    master.Height = temporadas[tempActiva].Height;
                    SeleccT.Text = "Temporada " + (tempActiva + 1);
                }
            }

            master.Height = temporadas[tempActiva].Height;


            sv.UpdateLayout();

        }

        //Eventos
        private void ZonaC_MouseEnter(object sender, MouseEventArgs e)
        {
            Principal.control.Stop();
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
            Pest.Visibility = System.Windows.Visibility.Visible;
            Eli.Visibility = System.Windows.Visibility.Visible;

            //Todo se va para arriba
            TimeSpan tiempo = new TimeSpan(0, 0, 0, 0, 250);
            InfoRect.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(InfoRect.Margin.Left, 153, 0, 0), tiempo));
            int alinea;
            if (se.Temporada != 0 && se.Capitulo != 0)
            {
                Episodio ep = se.Episodios[se.Temporada - 1][se.Capitulo - 1];
                if (ep.Estado == 1)
                {
                    double prog = (double)uClient.Torrents[ep.Hash].ProgressInMils / 10.0;
                    NombreEp.Text = ep.NombreEp + " - " + Math.Round(prog, 1) + "%";
                    Ajustar(NombreEp);
                }
                alinea = 160;
            }
            else
            {
                alinea = 160;
            }
            Play.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Play.Margin.Left, alinea, 0, 0), tiempo));
            Episodio.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Episodio.Margin.Left, 153, 0, 0), tiempo));
            alinea = NombreEp.FontSize == 12 ? 177 : 175; 
            NombreEp.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(NombreEp.Margin.Left, alinea, 0, 0), tiempo));
            Eye.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Eye.Margin.Left, 168, 0, 0), tiempo));
            Desc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Desc.Margin.Left, 163, 0, 0), tiempo));
            Vert.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Vert.Margin.Left, 162, 0, 0), tiempo));
            Desct.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Desct.Margin.Left, 162, 0, 0), tiempo));
            Up.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Up.Margin.Left,166, 0, 0), tiempo));
            Pest.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Pest.Margin.Left, 179, 0, 0), tiempo));
            Eli.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Eli.Margin.Left, 188, 0, 0), tiempo));
            desplegado = true;
            Principal.control.Start();
        }

        public void ZonaC_MouseLeave(object sender, MouseEventArgs e)
        {
            Principal.control.Stop();
            TimeSpan tiempo = new TimeSpan(0, 0, 0, 0, 250);
            if (!Play.IsMouseOver && !Up.IsMouseOver && !Eli.IsMouseOver && !Pest.IsMouseOver)
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
                Pest.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Pest.Margin.Left, 223, 0, 0), tiempo));
                Eli.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Eli.Margin.Left, 232, 0, 0), tiempo));
                
            }
            desplegado = false;
            Principal.control.Start();
       
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
                desplegado = false;
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
                    Ald.Visibility = System.Windows.Visibility.Hidden;
                    Trian.Visibility = System.Windows.Visibility.Hidden;
                    Eli.Visibility = System.Windows.Visibility.Hidden;
                    Pest.Visibility = System.Windows.Visibility.Hidden;

                    //Las temporadas se vuelven visibles
                    temporadas[tempActiva].Visibility = Visibility.Visible;
                    sv.Visibility = Visibility.Visible;
                    SeleccT.Visibility = Visibility.Visible;
                    if (tempActiva != 0) Ant.Visibility = Visibility.Visible;
                    if (tempActiva != se.Episodios.Count - 1) Sig.Visibility = Visibility.Visible;
                    temporadas[tempActiva].BeginAnimation(ScrollViewer.OpacityProperty, opac3);
                    sv.BeginAnimation(OpacityProperty, opac3);
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
                Ald.BeginAnimation(OpacityProperty, opac);
                Trian.BeginAnimation(OpacityProperty, opac);
                Pest.BeginAnimation(OpacityProperty, opac);
                Eli.BeginAnimation(OpacityProperty, opac);
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
                Pest.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Pest.Margin.Left, 223, 0, 0), tiempo));
                Eli.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Eli.Margin.Left, 232, 0, 0), tiempo));



              

                Play.BeginAnimation(OpacityProperty, opac3);
                Episodio.BeginAnimation(OpacityProperty, opac3);
                NombreEp.BeginAnimation(OpacityProperty, opac3);
                Eye.BeginAnimation(OpacityProperty, opac3);
                Desc.BeginAnimation(OpacityProperty, opac3);
                Vert.BeginAnimation(OpacityProperty, opac3);
                Desct.BeginAnimation(OpacityProperty, opac3);
                Eli.BeginAnimation(OpacityProperty, opac3);
                Pest.BeginAnimation(OpacityProperty, opac3);
                if (se.AlDia)
                { 
                    Ald.Visibility = System.Windows.Visibility.Visible;
                    Trian.Visibility = System.Windows.Visibility.Visible;
                    Ald.BeginAnimation(OpacityProperty, opac3);
                    Trian.BeginAnimation(OpacityProperty, new DoubleAnimation(.6, new TimeSpan(0, 0, 0, 0, 500)));
                }




                //Las temporadas se vuelven invisibles
                opac4.Completed += (sender3, e3) =>
                {
                    temporadas[tempActiva].Visibility = Visibility.Hidden;
                    sv.Visibility = System.Windows.Visibility.Hidden;
                    SeleccT.Visibility = Visibility.Hidden;
                    Ant.Visibility = Visibility.Hidden;
                    Sig.Visibility = Visibility.Hidden;
                };
                temporadas[tempActiva].BeginAnimation(OpacityProperty, opac);
                sv.BeginAnimation(ScrollViewer.OpacityProperty, opac);
                SeleccT.BeginAnimation(TextBlock.OpacityProperty, opac);
                Ant.BeginAnimation(Image.OpacityProperty, opac);
                Sig.BeginAnimation(Image.OpacityProperty, opac4);


               


                normal = true;

            }


        }

        private void Sig_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Principal.control.Stop();
            tempActiva++;
            temporadas[tempActiva].Opacity = 1;
            temporadas[tempActiva].Visibility = System.Windows.Visibility.Visible;
            ThicknessAnimation recorrer = new ThicknessAnimation(new Thickness(master.Margin.Left - 350, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 350));
            SeleccT.Text = "Temporada " + (tempActiva + 1);
            recorrer.Completed += (sender2, e2) =>
            {
                master.Height = (master.Children[tempActiva] as StackPanel).Height;
                temporadas[tempActiva - 1].Visibility = System.Windows.Visibility.Hidden;
                temporadas[tempActiva - 1].Opacity = 0;
                
            };
            master.BeginAnimation(MarginProperty, recorrer);

            if (tempActiva + 1 == se.Episodios.Count) Sig.Visibility = System.Windows.Visibility.Hidden;
            Ant.Visibility = System.Windows.Visibility.Visible;
            Principal.control.Start();

        }

        private void Ant_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Principal.control.Stop();
            tempActiva--;
            temporadas[tempActiva].Opacity = 1;
            temporadas[tempActiva].Visibility = System.Windows.Visibility.Visible;
            ThicknessAnimation recorrer = new ThicknessAnimation(new Thickness(master.Margin.Left + 350, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 350));
            SeleccT.Text = "Temporada " + (tempActiva + 1);
            recorrer.Completed += (sender2, e2) =>
                {
                    master.Height = (master.Children[tempActiva] as StackPanel).Height;
                    temporadas[tempActiva + 1].Visibility = System.Windows.Visibility.Hidden;
                    temporadas[tempActiva + 1].Opacity = 0;
                };
            master.BeginAnimation(MarginProperty, recorrer);

            if (tempActiva == 0) Ant.Visibility = System.Windows.Visibility.Hidden;
            Sig.Visibility = System.Windows.Visibility.Visible;
            Principal.control.Start();

        }

        private void Play_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Image i = (Image)sender;
            Episodio ep = (Episodio)i.Tag;
            if (ep.Estado == 2)
            {
                string dir = @"C:\Users\Marcelo\Videos\Series\" + se.Nombre + @"\Temporada " + ep.Temporada + @"\Episodio " + ep.Capitulo + ".mkv";
                System.Diagnostics.Process.Start(dir);
               /* int m = ep.Temporada - 1;
                while (m >= 0)
                {
                    int j = m == ep.Temporada - 1 ? ep.Capitulo - 1 : se.Episodios[m].Count - 1;
                    if (se.Episodios[m][j].Estado != 2)
                        break;
                    while (j >= 0 && se.Episodios[m][j].Estado == 2)
                    {
                        se.SiguienteEp();
                        se.Episodios[m][j].Estado = 3;
                        se.PorVer--;
                        j--;
                    }
                    
                    m--;

                }
                */

                bool salto = false;
                for (int p = ep.Temporada - 1; p >= 0; p--)
                {
                    for (int m = (p == ep.Temporada - 1 ? ep.Capitulo - 1 : se.Episodios[p].Count - 1); m >= 0; m--)
                    {
                        Episodio aux = se.Episodios[p][m];
                        if (aux.Estado == 3)
                        {
                            salto = true;
                            break;
                        }
                        else
                        {
                            if (aux.Estado == 2)
                            {
                                se.SiguienteEp();
                                aux.Estado = 3;
                                se.PorVer--;
                            }
                            else
                            {
                                if (aux.Estado == 1)
                                {
                                    aux.Estado = 3;                                    
                                    se.SiguienteEp();
                                    uClient.Torrents.Remove(aux.Hash, TorrentRemovalOptions.TorrentFileAndData);
                                    aux.Hash = "-1";
                                    se.Descargando--;

                                }
                            }
                        }
                    }
                    if (!salto) break;
                }



            }
            else
                if (ep.Estado == 3)
                {
                    bool salto = false;
                    for (int p = ep.Temporada - 1; p < se.Episodios.Count; p++)
                    {
                        for (int m = (p == ep.Temporada - 1 ? ep.Capitulo - 1 : 0); m < se.Episodios[p].Count; m++)
                        {
                            Episodio aux = se.Episodios[p][m];
                            if (aux.Estado == 0)
                            {
                                salto = true;
                                break;

                            }
                            else
                            {
                                if (aux.Estado == 3)
                                {
                                    if (System.IO.File.Exists(@"C:\Users\Marcelo\Videos\Series\" + aux.Serie.Nombre + @"\Temporada " + aux.Temporada + @"\Episodio " + aux.Capitulo + ".mkv"))
                                    {
                                        aux.Estado = 2;
                                        aux.Serie.PorVer++;

                                    }
                                    else
                                    {
                                        aux.getMagnet();
                                        aux.Estado = 1;
                                        aux.Serie.Descargando++;
                                        

                                    }
                                }
                            }
                        }
                        if (salto) break;
                    }
                    se.Temporada = ep.Temporada;
                    se.Capitulo = ep.Capitulo;

                }



            se.CrearArchivo();
            Principal.CrearArchivo();
        }

        private void Play2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (se.Temporada != 0 && se.Capitulo != 0)
            {
                Episodio ep = se.Episodios[se.Temporada - 1][se.Capitulo - 1];
                if (ep.Estado == 2)
                {
                    string dir = @"C:\Users\Marcelo\Videos\Series\" + se.Nombre + @"\Temporada " + ep.Temporada + @"\Episodio " + ep.Capitulo + ".mkv";
                    System.Diagnostics.Process.Start(dir);
                    se.SiguienteEp();
                    ep.Estado = 3;
                    se.PorVer--;
                    Principal.CrearArchivo();
                    se.CrearArchivo();
                }
            }
        }

        private void IntSeries_Loaded(object sender, RoutedEventArgs e)
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
            {
                Episodio ep = se.Episodios[se.Temporada - 1][se.Capitulo - 1];
                Episodio.Text = "S" + (se.Temporada < 10 ? "0" : "") + se.Temporada + "E" + (se.Capitulo < 10 ? "0" : "") + se.Capitulo;
                NombreEp.Text = ep.NombreEp;

                if (DateTime.Now < ep.Fecha)
                {
                    Play.Source = new BitmapImage(new Uri
                                   (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Clockb2.png"));
                    DateTime hoy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                    DateTime fechaep = new DateTime(ep.Fecha.Year, ep.Fecha.Month, ep.Fecha.Day, 12, 0, 0);
                    TimeSpan tiempo = fechaep - hoy;
                    NombreEp.Text += " - " + (tiempo.TotalDays < 1 ? "Hoy" : tiempo.TotalDays < 2 ? "Mañana" : (tiempo.Days + " Días"));
                    Ajustar(NombreEp);
                }
                else
                {
                    Play.Source = new BitmapImage(new Uri
                                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Descb.png"));
                }


            }
            else
            {
                if (se.AlDia)
                {
                    Ald.Visibility = System.Windows.Visibility.Visible;
                    Trian.Visibility = System.Windows.Visibility.Visible;
                    Ald.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                    Trian.BeginAnimation(OpacityProperty, new DoubleAnimation(.6, new TimeSpan(0, 0, 0, 0, 500)));
                }
                if (se.Estado == 'c')
                {
                    Episodio.Text = "TBA";
                    NombreEp.Text = "Se anunciará";
                    Play.Source = new BitmapImage(new Uri
                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Mega.png"));
                }
                else
                {
                    Episodio.Text = "Serie";
                    NombreEp.Text = "Finalizada";
                    Play.Source = new BitmapImage(new Uri
                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\End.png"));
                }
            }


            //Vert
            Vert.Text = se.PorVer.ToString();

            //Descrt
            Desct.Text = se.Descargando.ToString();

            se.PropertyChanged += new PropertyChangedEventHandler(PropertyChangedS);
        }

        private void Pest_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation opac = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 125));

            ZonaC.Visibility = System.Windows.Visibility.Hidden;
            opac.Completed += (sender2, e2) =>
            {
                DoubleAnimation opac2 = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 125));
                opac2.Completed += (sender3, e3) =>
                {
                    DoubleAnimation sube = new DoubleAnimation(49, new TimeSpan(0, 0, 0, 0, 75));

                    sube.Completed += (sender4, e4) =>
                    {
                        DoubleAnimation grande = new DoubleAnimation(350, new TimeSpan(0, 0, 0, 0, 100));
                        grande.Completed += (sender5, e5) =>
                        {
                            Elit.Visibility = System.Windows.Visibility.Visible;
                            Cant.Visibility = System.Windows.Visibility.Visible;
                            DoubleAnimation opc = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250));
                            Elit.BeginAnimation(OpacityProperty, opc);
                            Cant.BeginAnimation(OpacityProperty, opc);
                        };
                        Tapa.BeginAnimation(WidthProperty, grande);
                        Tapa.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0, Tapa.Margin.Top, 0, 0), new TimeSpan(0, 0, 0, 0, 100)));
                    };
                    Tapa.BeginAnimation(HeightProperty, sube);
                    Tapa.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Tapa.Margin.Left, 148, 0, 0), new TimeSpan(0, 0, 0, 0, 75)));
                };
                Tapa.Visibility = System.Windows.Visibility.Visible;
                Tapa.BeginAnimation(OpacityProperty, opac2);

            };

            Pest.BeginAnimation(OpacityProperty, opac);
            Eli.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 125)));


        }

        private void Cant_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Se declara animacion para esconder cancelar
            DoubleAnimation opac = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));

            //Cuando se completa la animacion para esconder eliminar y opacidad...
            opac.Completed += (sender2, e2) =>
            {
                //Se esconde cancelar y eliminar
                Elit.Visibility = System.Windows.Visibility.Hidden;
                Cant.Visibility = System.Windows.Visibility.Hidden;

                //Se declara animacion para hacer angosta la tapa
                DoubleAnimation chico = new DoubleAnimation(6, new TimeSpan(0, 0, 0, 0, 100));

                //Cuando se completa animacion de hacer angosta la tapa
                chico.Completed += (sender3, e3) =>
                {
                    //Se declara animacion para bajar la tapa
                    DoubleAnimation baja = new DoubleAnimation(8, new TimeSpan(0, 0, 0, 0, 75));

                    //Cuando se completa animacion de bajar la tapa
                    baja.Completed += (sender4, e4) =>
                    {
                        //Se declara animacion para esconder la tapa
                        DoubleAnimation opac2 = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 125));

                        opac2.Completed += (sender5, e5) =>
                        {
                            Tapa.Visibility = System.Windows.Visibility.Hidden;
                            Pest.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 125)));
                            Eli.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 125)));
                            ZonaC.Visibility = System.Windows.Visibility.Visible;
                            //     MessageBox.Show("Margin: " + Tapa.Margin.Left + " " + Tapa.Margin.Top);
                        };

                        Tapa.BeginAnimation(OpacityProperty, opac2);

                    };

                    //Inicia animacion de bajar la tapa

                    Tapa.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Tapa.Margin.Left, 189, 0, 0), new TimeSpan(0, 0, 0, 0, 75)));
                    Tapa.BeginAnimation(HeightProperty, baja);
                };

                //Se inicia animacion de hacer angosta la tapa

                Tapa.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(344, Tapa.Margin.Top, 0, 0), new TimeSpan(0, 0, 0, 0, 100)));
                Tapa.BeginAnimation(WidthProperty, chico);
            };

            //Inicia animacion de esconder eliminar y cancelar
            Elit.BeginAnimation(OpacityProperty, opac);
            Cant.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));

        }

        private void Elit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Principal.EliminarSerie(se.Numserie);
        }

        private void PropertyChangedS(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Descargando")
                Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (ThreadStart)delegate
                {
                    Desct.Text = se.Descargando.ToString();
                });
                else
                    if (e.PropertyName == "Capitulo")
                        Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        (ThreadStart)delegate
                        {
                            if (se.Temporada != 0 && se.Capitulo != 0)
                            {
                                Episodio ep = se.Episodios[se.Temporada - 1][se.Capitulo - 1];
                                Episodio.Text = "S" + (se.Temporada < 10 ? "0" : "") + se.Temporada + "E" + (se.Capitulo < 10 ? "0" : "") + se.Capitulo;
                                NombreEp.Text = ep.NombreEp;
                                string nombre = "";

                                if (ep.Estado == 0)
                                {
                                    nombre = "Clockb2.png";
                                    Play.Margin = new Thickness(10, Play.Margin.Top, 0, 0);
                                    DateTime hoy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                                    DateTime fechaep = new DateTime(ep.Fecha.Year, ep.Fecha.Month, ep.Fecha.Day, 12, 0, 0);
                                    TimeSpan tiempo = fechaep - hoy;
                                    NombreEp.Text += " - " + (tiempo.TotalDays < 1 ? "Hoy" : tiempo.TotalDays < 2 ? "Mañana" : (tiempo.Days + " Días"));
                                    Ajustar(NombreEp);
                                }
                                else
                                {
                                    if (ep.Estado == 1)
                                    {
                                        nombre = "Descb.PNG";
                                        double prog = (double)uClient.Torrents[ep.Hash].ProgressInMils / 10.0;
                                        Play.Margin = new Thickness(10, Play.Margin.Top, 0, 0);
                                        NombreEp.Text += " - " + Math.Round(prog, 1) + "%";
                                        Ajustar(NombreEp);
                                    }
                                    else
                                    {
                                        if (ep.Estado == 2)
                                        {
                                            nombre = "play.png";
                                            Play.Margin = new Thickness(10, Play.Margin.Top, 0, 0);
                                        }

                                    }

                                }
                                if (ep.Estado != 3)
                                {
                                    Play.Source = new BitmapImage(new Uri
                                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\" + nombre));
                                }
                            }
                            else
                            {
                               
                                if (se.Estado == 'c')
                                {
                                    Episodio.Text = "TBA";
                                    NombreEp.Text = "Se anunciará";
                                    Play.Source = new BitmapImage(new Uri
                                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Mega.png"));
                                }
                                else
                                {
                                    Episodio.Text = "Serie";
                                    NombreEp.Text = "Finalizada";
                                    Play.Source = new BitmapImage(new Uri
                                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\End.png"));
                                }
                            }

                            if ((se.Temporada == 0 && se.Capitulo == 0) || se.Episodios[se.Temporada - 1][se.Capitulo - 1].Estado == 0)
                            {
                                se.AlDia = true;
                            }
                            else
                            {
                                se.AlDia = false;
                            }
                            
                                               
                         });
                    else
                        if (e.PropertyName == "PorVer")
                        {
                            Application.Current.Dispatcher.Invoke(
                            DispatcherPriority.Normal,
                            (ThreadStart)delegate
                            {
                                Vert.Text = se.PorVer.ToString();
                            });
                        }
                        else
                            if (e.PropertyName == "AlDia")
                            {
                                Application.Current.Dispatcher.Invoke(
                                DispatcherPriority.Normal,
                                (ThreadStart)delegate
                                {
                                    if (se.AlDia == true)
                                    {
                                        if (normal)
                                        {
                                            Ald.Visibility = System.Windows.Visibility.Visible;
                                            Trian.Visibility = System.Windows.Visibility.Visible;
                                            Ald.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                                            Trian.BeginAnimation(OpacityProperty, new DoubleAnimation(.6, new TimeSpan(0, 0, 0, 0, 250)));
                                        }
                                    }
                                    else
                                    {
                                        DoubleAnimation anima = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
                                        anima.Completed += (sender10, e10) =>
                                            {
                                                Ald.Visibility = System.Windows.Visibility.Hidden;
                                                Trian.Visibility = System.Windows.Visibility.Hidden;
                                            };


                                        Ald.BeginAnimation(OpacityProperty, anima);
                                        Trian.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));



                                    }



                                });
                            }
                            else
                                if (e.PropertyName == "Estado")
                                {
                                    if (se.Estado == 'e')
                                    {


                                        Application.Current.Dispatcher.Invoke(
                                        DispatcherPriority.Normal,
                                        (ThreadStart)delegate
                                        {
                                            Episodio.Text = "Serie";
                                            NombreEp.Text = "Finalizada";
                                            Play.Source = new BitmapImage(new Uri
                                            (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\End.png"));
                                        });
                                    }
                                    else
                                    {
                                        Application.Current.Dispatcher.Invoke(
                                        DispatcherPriority.Normal,
                                        (ThreadStart)delegate
                                        {
                                            if (se.Temporada == 0 && se.Capitulo == 0)
                                            {
                                                Episodio.Text = "TBA";
                                                NombreEp.Text = "Se anunciará";
                                                Play.Source = new BitmapImage(new Uri
                                                (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Mega.png"));
                                            }
                                        });
                                    }
                                }



        }

        private void PropertyChandeE(object sender, PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(
            DispatcherPriority.Normal,
            (ThreadStart)delegate
            {
                
            Episodio ep = (Episodio)sender;
            StackPanel epi = (StackPanel)temporadas[ep.Temporada - 1].Children[ep.Capitulo - 1];
            Image estado = (Image)epi.Children[0];
            TextBlock texto = (TextBlock)epi.Children[1];
            string nombre = "";
            

            //Episodio principal
            if (ep.Temporada == se.Temporada && ep.Capitulo == se.Capitulo)
            {

                NombreEp.Text = ep.NombreEp;
                if (ep.Estado == 0)
                {
                    nombre = "Clockb2.png";
                    DateTime hoy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                    DateTime fechaep = new DateTime(ep.Fecha.Year, ep.Fecha.Month, ep.Fecha.Day, 12, 0, 0);
                    TimeSpan tiempo = fechaep - hoy;
                    NombreEp.Text += " - " + (tiempo.TotalDays < 1 ? "Hoy" : tiempo.TotalDays < 2 ? "Mañana" : (tiempo.Days + " Días"));
                    Ajustar(NombreEp);
                }
                else
                    if (ep.Estado == 1)
                    {
                        nombre = "Descb.PNG";
                        double prog = (double)uClient.Torrents[ep.Hash].ProgressInMils / 10.0;
                        NombreEp.Text += " - " + Math.Round(prog, 1) + "%";
                    }
                    else
                    {
                        nombre = "play.PNG";
                    }
                Play.Source = new BitmapImage(new Uri(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\" + nombre));
            }

            texto.Text = "E" + (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo + " - " + ep.NombreEp;

            if (ep.Estado == 0)
            {
                nombre = "Clocks.png";
                texto.Text += " - " + ep.Fecha.ToShortDateString();
            }
            else
            {
                if (ep.Estado == 1)
                {
                    nombre = "descs.png";
                    double prog = (double)uClient.Torrents[ep.Hash].ProgressInMils / 10.0;
                    texto.Text += " - " + Math.Round(prog, 1) + "%";
                }
                else
                {
                    if (ep.Estado == 2)
                    {
                        nombre = "plays.png";
                    }
                    else
                        if (ep.Estado == 3)
                        {
                            nombre = "paloma.PNG";
                        }
                }
            }

            estado.Source = new BitmapImage(new Uri(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\" + nombre));



            });

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

        private void Ajustar(TextBlock t)
        {
            t.FontSize = 14;
            while (MeasureString(t).Width > 129 && t.FontSize > 12)
            {
                t.FontSize--;
                if (t.FontSize == 12 && MeasureString(t).Width > 1)
                {
                    int empiezo = t.Text.LastIndexOf(" - ");
                    t.Text = t.Text.Insert(empiezo, "...");
                    int i = 1;
                    while (MeasureString(t).Width > 131)
                    {
                        t.Text = t.Text.Remove(empiezo - i, 1);
                        i++;
                    }
                    break;
                }
            }
            if (t.FontSize == 12) t.Margin = new Thickness(t.Margin.Left, 177, 0, 0);
            else t.Margin = new Thickness(t.Margin.Left, 175, 0, 0);
          
        }

        public void Actualiza()
        {

            //Actualiza el episodio actual si es necesario
            Episodio act = se.Episodios[se.Temporada - 1][se.Capitulo - 1];
            if (act.Estado == 1 && desplegado)
            {
                double prog = (double)uClient.Torrents[act.Hash].ProgressInMils / 10.0;
                NombreEp.Text = act.NombreEp + " - " + Math.Round(prog, 1) + "%";
                Ajustar(NombreEp);
            }

            //Revisa todos los episodios
            bool sal = false;
            for (int m = se.Temporada - 1; m < se.Episodios.Count; m++)
            {
                for (int j = (m == se.Temporada - 1 ? se.Capitulo - 1 : 0); j < se.Episodios[m].Count; j++)
                {
                    Episodio aux = se.Episodios[m][j];
                    if (aux.Estado != 2)
                    {
                        if (aux.Estado == 0)
                        {
                            sal = true;
                            break;
                        }
                        else
                        {
                            if (aux.Estado == 1)
                            {
                                if (uClient.Torrents[aux.Hash].RemainingBytes == 0 && uClient.Torrents[aux.Hash].DownloadedBytes > 0 || uClient.Torrents[aux.Hash].ProgressInMils == 1000)
                                {
                                    aux.Mover(uClient, false);
                                }
                                else
                                {
                                    if (!normal && tempActiva == aux.Temporada - 1 && (double)uClient.Torrents[aux.Hash].ProgressInMils != 0)
                                    {
                                        double prog = (double)uClient.Torrents[aux.Hash].ProgressInMils / 10.0;
                                        ((temporadas[tempActiva].Children[aux.Capitulo - 1] as StackPanel).Children[1] as TextBlock).Text = "E" + (aux.Capitulo < 10 ? "0" : "") + aux.Capitulo + " - " + aux.NombreEp + " - " + Math.Round(prog, 1) + "%";
                                    }
                                }
                            }
                        }

                    }
                    if (sal) break;
                }
            }
        }

  



    }
}
