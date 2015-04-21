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
        //Serie de la interfaz 
        public Serie Se; 

        private List<StackPanel> temporadas;

        public Boolean episodiosMostrados;

        private int tempActiva;

        private MainWindow Principal;

        private DispatcherTimer actualizarInterfazEpPrinc;
        private DispatcherTimer actualizarInterfazEpRestantes;

        public IntSerie(Serie s, MainWindow m)
        {
            Se = s;
            episodiosMostrados = false;
            Principal = m;

            actualizarInterfazEpPrinc = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 1)
            };

            actualizarInterfazEpPrinc.Tick += actualizarInterfazEpPr;

            InitializeComponent();
        }

        private void IntSeries_Initialized(object sender, EventArgs e)
        {
           

            


        }

        public void agregarDesgloseEpisodios(bool escondido)
        {

            actualizarInterfazEpRestantes = new DispatcherTimer()
            {
                Interval = new TimeSpan(0,0,1),            
            };

            actualizarInterfazEpRestantes.Tick += actualizarInterfazEpRes;

            //Episodios
            temporadas = new List<StackPanel>();
           
            foreach (List<Episodio> temporada in Se.EpisodiosVistos)
            {
                agregarTemporadaAlDesglose();
                foreach (Episodio ep in temporada)
                {
                    agregarEpisodioAlDesglose(ep);
                }
            }

            foreach (Episodio ep in Se.EpisodiosNoVistos)
            {
                if (ep.Capitulo == 1) agregarTemporadaAlDesglose();
                agregarEpisodioAlDesglose(ep);
            }


            if (Se.Temporada != 0)
            {
                tempActiva = Se.Temporada - 1;
                Temporadas.Height = temporadas[tempActiva].Height;
                ScrollEpisodios.ScrollToVerticalOffset(17 * Se.EpisodiosNoVistos[0].Capitulo);
                if (tempActiva + 1 == Se.EpisodiosNoVistos[Se.EpisodiosNoVistos.Count - 1].Temporada)
                {
                    Sig.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    if (tempActiva == 0) Ant.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            else
            {
                tempActiva = Se.EpisodiosVistos.Count - 1;
                Temporadas.Height = temporadas[tempActiva].Height;
                ScrollEpisodios.ScrollToVerticalOffset(17 * Se.EpisodiosVistos[tempActiva].Count - 9);

                if (tempActiva + 1 == Se.EpisodiosVistos.Count)
                {
                    Sig.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    if (tempActiva == 0) Ant.Visibility = System.Windows.Visibility.Hidden;
                }
            }


           Temporadas.Margin = new Thickness(tempActiva * -350, 0, 0, 0);

           ScrollEpisodios.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
           ScrollEpisodios.UpdateLayout();
           

           //Se pone la temporada seleccionada como texto
           SeleccT.Text = "Temporada " + (tempActiva + 1);

           temporadas[tempActiva].Opacity = 1;


           if (!escondido)
           {
               DesgloseEpisodios.Visibility = System.Windows.Visibility.Visible;
               DesgloseEpisodios.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
           }

           actualizarInterfazEpRestantes.Start();

           Se.episodiosCargados = true;
            
        }

        public void agregarTemporadaAlDesglose()
        {
            if (temporadas.Count != 0)
            {
                Temporadas.Width += 350;
                
            }

            //Se crea el StackPanel de la temporada
            StackPanel temporada = new StackPanel()
            {
                Width = 350,
                Height = 0,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Opacity = 0,
                CanVerticallyScroll = true,

            };

            Temporadas.Children.Add(temporada);
            temporadas.Add(temporada);

            

        }

        public void agregarEpisodioAlDesglose(Episodio ep)
        {
            
            StackPanel temporada = temporadas[temporadas.Count - 1];
            InterfazDesgloseEp intEpisodio = new InterfazDesgloseEp(ep, this);
          /*  string imagen;

            //Dependiendo de su estado se pone la imagen
            switch (ep.Estado)
            {
                case 0:
                    imagen = "Clocks";
                    break;
                case 1:
                    imagen = "descs";
                    break;
                case 2:
                    imagen = "plays";
                    break;
                default:
                    imagen = "paloma";
                    break;
            }

            //Se crea el stackpanel del episodio
            StackPanel spEpisodio = new StackPanel()
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
                texto.Text += " - " + Principal.getProgesoTorrent(ep.Hash) + "%";
            }

            spEpisodio.Children.Add(estado);
            spEpisodio.Children.Add(texto);
           */

            //Se añade el stackPanel del episodio a la temporada
            temporada.Height += 17;
            temporada.Children.Add(intEpisodio);

            if (tempActiva == temporadas.Count - 1)
            {
                Temporadas.Height = temporadas[tempActiva].Height;
            }

        }

        public void subirLona(bool primeraVez)
        {

            GPrinc.MouseEnter -= GPrinc_MouseEnter;
            GPrinc.MouseLeave -= GPrinc_MouseLeave;
            DoubleAnimation alto = new DoubleAnimation(170, new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation thick = new ThicknessAnimation(new Thickness(0, -126, 0, 0), new TimeSpan(0,0,0,0,250));
            ThicknessAnimation flecharriba = new ThicknessAnimation(new Thickness(Up.Margin.Left, -148, 0, 0), new TimeSpan(0, 0, 0, 0, 250));
            DoubleAnimation invisible = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            DoubleAnimation invisible2 = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            
            RotateTransform m = new RotateTransform();
            Storyboard storyboard = new Storyboard();
            Up.RenderTransform = m;
            Up.RenderTransformOrigin = new Point(0.5, 0.5);
            DoubleAnimation rotateAnimation = new DoubleAnimation()
            {
                From = 0,
                To = 180,
                Duration = new TimeSpan(0, 0, 0, 0, 250),
            };
            Storyboard.SetTarget(rotateAnimation, Up);
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            storyboard.Children.Add(rotateAnimation);

            InfoRect.BeginAnimation(HeightProperty, alto);
            InfoRect.BeginAnimation(MarginProperty, thick);
            Up.BeginAnimation(MarginProperty, flecharriba);
            storyboard.Begin();

            invisible.Completed += (sernder, e) =>
                {
                    Informacion.Visibility = System.Windows.Visibility.Hidden;
                    Ald.Visibility = System.Windows.Visibility.Hidden;
                    Trian.Visibility = System.Windows.Visibility.Hidden;
                    BarraProgreso.Visibility = System.Windows.Visibility.Hidden;
                    if (!primeraVez)
                    {
                        DesgloseEpisodios.Visibility = System.Windows.Visibility.Visible;
                        DesgloseEpisodios.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                    }

                };

            Informacion.BeginAnimation(OpacityProperty, invisible);
            Ald.BeginAnimation(OpacityProperty, invisible2);
            Trian.BeginAnimation(OpacityProperty, invisible2);
            BarraProgreso.BeginAnimation(OpacityProperty, invisible2);

        }

        public void bajarLona()
        {

            DoubleAnimation alto = new DoubleAnimation(44, new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation thick = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation flechabajo = new ThicknessAnimation(new Thickness(Up.Margin.Left, 13, 0, 0), new TimeSpan(0, 0, 0, 0, 250));
            DoubleAnimation visible = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250));
            DoubleAnimation invisible = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            Storyboard storyboard = new Storyboard();
            DoubleAnimation rotateAnimation = new DoubleAnimation()
            {
                From = 180,
                To = 0,
                Duration = new TimeSpan(0, 0, 0, 0, 250),
            };
            Storyboard.SetTarget(rotateAnimation, Up);
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            storyboard.Children.Add(rotateAnimation);
            Informacion.Visibility = System.Windows.Visibility.Visible;

            if (Se.EpisodiosNoVistos.Count != 0 && Se.EpisodiosNoVistos[0].Estado == 1)
            {
                BarraProgreso.Visibility = System.Windows.Visibility.Visible;
                BarraProgreso.BeginAnimation(WidthProperty, new DoubleAnimation(Principal.getProgesoTorrent(Se.EpisodiosNoVistos[0].Hash) * 3.5, TimeSpan.FromMilliseconds(0)));
            }


            invisible.Completed += (sender, e) =>
                {
                    InfoRect.BeginAnimation(HeightProperty, alto);
                    InfoRect.BeginAnimation(MarginProperty, thick);
                    Up.BeginAnimation(MarginProperty, flechabajo);
                    storyboard.Begin();
                };

            flechabajo.Completed += (sender, e) =>
                {
                    Informacion.BeginAnimation(OpacityProperty, visible);
                    BarraProgreso.BeginAnimation(OpacityProperty, new DoubleAnimation(.2, TimeSpan.FromMilliseconds(250)));
                    DesgloseEpisodios.Visibility = System.Windows.Visibility.Hidden;
                    if (Se.AlDia)
                    {
                        Ald.Visibility = System.Windows.Visibility.Visible;
                        Trian.Visibility = System.Windows.Visibility.Visible;
                        Ald.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0,0,0,0,250)));
                        Trian.BeginAnimation(OpacityProperty, new DoubleAnimation(.6, new TimeSpan(0,0,0,0,250)));
                    }
                };

            DesgloseEpisodios.BeginAnimation(OpacityProperty, invisible);

            GPrinc.MouseEnter += GPrinc_MouseEnter;
            GPrinc.MouseLeave += GPrinc_MouseLeave;


        }

        //Eventos

        private void Up_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (!episodiosMostrados)
            {
                if (actualizarInterfazEpRestantes != null)
                {
                    actualizarInterfazEpRestantes.Start();
                }
                Principal.desplegarEpisodios(this);
                
            }
            else
            {
                actualizarInterfazEpRestantes.Stop();
                bajarLona();
            }

            episodiosMostrados = !episodiosMostrados;

        }

        private void Sig_MouseUp(object sender, MouseButtonEventArgs e)
        {
            actualizarInterfazEpRestantes.Stop();
            tempActiva++;
            temporadas[tempActiva].Opacity = 1;
            temporadas[tempActiva].Visibility = System.Windows.Visibility.Visible;
            ThicknessAnimation recorrer = new ThicknessAnimation(new Thickness(Temporadas.Margin.Left - 350, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 350));
            SeleccT.Text = "Temporada " + (tempActiva + 1);
            recorrer.Completed += (sender2, e2) =>
            {
                Temporadas.Height = temporadas[tempActiva].Height;
                temporadas[tempActiva - 1].Visibility = System.Windows.Visibility.Hidden;
                temporadas[tempActiva - 1].Opacity = 0;
                
            };
            Temporadas.BeginAnimation(MarginProperty, recorrer);

            if (Se.Temporada != 0)
            {
                if (tempActiva + 1 == Se.EpisodiosNoVistos[Se.EpisodiosNoVistos.Count - 1].Temporada)
                {
                    Sig.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            else
            {
                if (tempActiva + 1 == Se.EpisodiosVistos[tempActiva].Count)
                {
                    Sig.Visibility = System.Windows.Visibility.Hidden;
                }
            }

            Ant.Visibility = System.Windows.Visibility.Visible;
            actualizarInterfazEpRestantes.Start();

        }

        private void Ant_MouseUp(object sender, MouseButtonEventArgs e)
        {
            actualizarInterfazEpRestantes.Stop();
            tempActiva--;
            temporadas[tempActiva].Opacity = 1;
            temporadas[tempActiva].Visibility = System.Windows.Visibility.Visible;
            ThicknessAnimation recorrer = new ThicknessAnimation(new Thickness(Temporadas.Margin.Left + 350, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 350));
            SeleccT.Text = "Temporada " + (tempActiva + 1);
            recorrer.Completed += (sender2, e2) =>
                {
                    Temporadas.Height = temporadas[tempActiva].Height;
                    temporadas[tempActiva + 1].Visibility = System.Windows.Visibility.Hidden;
                    temporadas[tempActiva + 1].Opacity = 0;
                };
            Temporadas.BeginAnimation(MarginProperty, recorrer);

            if (tempActiva == 0) Ant.Visibility = System.Windows.Visibility.Hidden;
            Sig.Visibility = System.Windows.Visibility.Visible;

            actualizarInterfazEpRestantes.Start();

        }

        private void Play_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Se.EpisodiosNoVistos.Count != 0 && Se.EpisodiosNoVistos[0].Estado == 2)
            {
                Episodio ep = Se.EpisodiosNoVistos[0];
                string dir = @"C:\Users\Marcelo\Videos\Series\" + Se.Nombre + @"\Temporada " + ep.Temporada + @"\Episodio " + ep.Capitulo + ".mkv";
                System.Diagnostics.Process.Start(dir);
                ep.PropertyChanged -= PropertyChangedEpisodioPrincipal;

                Se.siguienteEpisodio();

                if (ep.Doble)
                {
                    Se.siguienteEpisodio();
                }

                if (Se.EpisodiosNoVistos.Count > 0)
                {
                    ep = Se.EpisodiosNoVistos[0];
                    ep.PropertyChanged += PropertyChangedEpisodioPrincipal;
                }
            }
        }

        private void IntSeries_Loaded(object sender, RoutedEventArgs e)
        {
            //Fanart
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\FanArt\" + Se.Nombre + ".jpg");
            image.EndInit();
            RenderOptions.SetBitmapScalingMode(FanArt, BitmapScalingMode.Fant);
            FanArt.Source = image;

            //Nombre
            Nombre.Text = Se.Nombre;
            Nombre.Width = MeasureString(Nombre).Width + 20;
            RectNombre.Width = MeasureString(Nombre).Width + 20;

            //Episodio principal
            Episodio ep = null;

            //Episodio
            if (Se.EpisodiosNoVistos.Count != 0)
            {
                ep = Se.EpisodiosNoVistos[0];
                Episodio.Text = "S" + (ep.Temporada < 10 ? "0" : "") + ep.Temporada + "E" + (ep.Capitulo < 10 ? "0" : "") + Se.Capitulo;
                NombreEp.Text = ep.NombreEp;

                //Si el episodio no ha salido
                if (ep.Estado == 0)
                {
                    //Se pone la imagen del Reloj
                    Play.Source = new BitmapImage(new Uri
                                   (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Clockb2.png"));

                    //Se calcula el tiempo que falta para que salga
                    DateTime hoy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                    DateTime fechaep = new DateTime(ep.Fecha.Year, ep.Fecha.Month, ep.Fecha.Day, 12, 0, 0);
                    TimeSpan tiempo = fechaep - hoy;

                    //Se pone el texto correspondiente
                    NombreEp.Text += " - " + (tiempo.TotalDays < 1 ? "Hoy" : tiempo.TotalDays < 2 ? "Mañana" : (tiempo.Days + " Días"));
                    //Se ajusta su tamaño
                    Ajustar(NombreEp);
                }
                else
                {
                    //Si el episodio se esta descargando
                    if (ep.Estado == 1)
                    {
                        Play.Source = new BitmapImage(new Uri
                                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Descb.png"));
                    }

                    //Si el episodio ya se descargo
                    else
                    {
                        Play.Source = new BitmapImage(new Uri
                                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\play.png"));
                    }
                }
            }
            else
            {                
                if (Se.Estado == 'c')
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

            if (Se.AlDia)
            {
                Ald.Visibility = System.Windows.Visibility.Visible;
                Trian.Visibility = System.Windows.Visibility.Visible;
                Ald.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                Trian.BeginAnimation(OpacityProperty, new DoubleAnimation(.6, new TimeSpan(0, 0, 0, 0, 500)));
            }

            //Se pone el texto de descargando y por ver
            Desct.Text = Se.Descargando.ToString();
            Vert.Text = Se.PorVer.ToString();

            //Se añaden los propertychanged
            Se.PropertyChanged += PropertyChangedSerie;

            if (ep != null)
            {
                ep.PropertyChanged += PropertyChangedEpisodioPrincipal;
            }

            //Empieza el actualizador de interfaz
            actualizarInterfazEpPrinc.Start();
            

        }

        private void Pest_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GPrinc.MouseLeave -= GPrinc_MouseLeave;
            GPrinc.MouseEnter -= GPrinc_MouseEnter;


            DoubleAnimation desaparecePestEli = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 125));

            DoubleAnimation apareceTapa = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 125));
            apareceTapa.BeginTime = TimeSpan.FromMilliseconds(125);

            DoubleAnimation subeTapa = new DoubleAnimation(49, new TimeSpan(0, 0, 0, 0, 75));
            subeTapa.BeginTime = TimeSpan.FromMilliseconds(250);

            DoubleAnimation grandeTapa = new DoubleAnimation(350, new TimeSpan(0, 0, 0, 0, 100));
            grandeTapa.BeginTime = TimeSpan.FromMilliseconds(325);

            DoubleAnimation apareceOpciones = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250));
            apareceOpciones.BeginTime = TimeSpan.FromMilliseconds(425);

            Tapa.Visibility = System.Windows.Visibility.Visible;
            Elit.Visibility = System.Windows.Visibility.Visible;
            Cant.Visibility = System.Windows.Visibility.Visible;


            Pest.BeginAnimation(OpacityProperty, desaparecePestEli);
            Eli.BeginAnimation(OpacityProperty, desaparecePestEli);

            Tapa.BeginAnimation(OpacityProperty, apareceTapa);

            Tapa.BeginAnimation(HeightProperty, subeTapa);

            Tapa.BeginAnimation(WidthProperty, grandeTapa);

            Elit.BeginAnimation(OpacityProperty, apareceOpciones);
            Cant.BeginAnimation(OpacityProperty, apareceOpciones);


        }

        private void Cant_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GPrinc.MouseEnter += GPrinc_MouseEnter;
            GPrinc.MouseLeave += GPrinc_MouseLeave;

            //Se declara animacion para esconder cancelar
            DoubleAnimation desapareceOpciones = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));

            DoubleAnimation angostaTapa = new DoubleAnimation(6, new TimeSpan(0, 0, 0, 0, 100));
            angostaTapa.BeginTime = TimeSpan.FromMilliseconds(250);

            DoubleAnimation bajaTapa = new DoubleAnimation(8, new TimeSpan(0, 0, 0, 0, 75));
            bajaTapa.BeginTime = TimeSpan.FromMilliseconds(350);

            DoubleAnimation desapareceTapa = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 75));
            desapareceTapa.BeginTime = TimeSpan.FromMilliseconds(425);

            DoubleAnimation aparecePestEli = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 125));
            aparecePestEli.BeginTime = TimeSpan.FromMilliseconds(495);

            desapareceTapa.Completed += (sender2, e2) =>
                {
                    Elit.Visibility = System.Windows.Visibility.Hidden;
                    Cant.Visibility = System.Windows.Visibility.Hidden;
                    Tapa.Visibility = System.Windows.Visibility.Hidden;

                };
            
            //Inicia animacion de esconder eliminar y cancelar
            Elit.BeginAnimation(OpacityProperty, desapareceOpciones);
            Cant.BeginAnimation(OpacityProperty, desapareceOpciones);

            Tapa.BeginAnimation(WidthProperty, angostaTapa);

            Tapa.BeginAnimation(HeightProperty, bajaTapa);

            Tapa.BeginAnimation(OpacityProperty, desapareceTapa);

            Pest.BeginAnimation(OpacityProperty, aparecePestEli);
            Eli.BeginAnimation(OpacityProperty, aparecePestEli);

            actualizarInterfazEpPrinc.Start();

        }

        private void Elit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            actualizarInterfazEpPrinc.Stop();
            Principal.eliminarSerie(Se.Numserie);
        }

        private void PropertyChangedSerie(object sender, PropertyChangedEventArgs e)
        {

            Application.Current.Dispatcher.Invoke(
            DispatcherPriority.Normal,
            (ThreadStart)delegate
            {
                switch (e.PropertyName)
                {
                    case "Descargando":                    
                        Desct.Text = Se.Descargando.ToString();
                        break;
                
                    case "PorVer":
                        Vert.Text = Se.PorVer.ToString();
                        break;

                    case "Capitulo":
                        UpdateTextCapitulo();
                        break;

                    case "AlDia":
                        MostraroEsconderAlDia(Se.AlDia);
                        break;

                    case "Estado":
                        if (Se.Estado == 'e')
                        {
                            Episodio.Text = "Serie";
                            NombreEp.Text = "Finalizada";
                            Play.Source = new BitmapImage(new Uri
                            (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\End.png"));
                        
                        }
                        else
                        {                      
                            if (Se.Temporada == 0 && Se.Capitulo == 0)
                            {
                                Episodio.Text = "TBA";
                                NombreEp.Text = "Se anunciará";
                                Play.Source = new BitmapImage(new Uri
                                (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Mega.png"));
                            }
                        }
                        break;
                }

            });


           



        }   

        private void PropertyChangedEpisodioPrincipal(object sender, PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(
            DispatcherPriority.Normal,
            (ThreadStart)delegate
            {
                
                Episodio ep = (Episodio)sender;

                switch (e.PropertyName)
                {
                    case "Estado":

                        switch (ep.Estado)
                        {
                            case 1:
                                Play.Source = new BitmapImage(new Uri
                                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Descb.png"));
                                NombreEp.Text = ep.NombreEp + " - 0%";
                                Ajustar(NombreEp);
                                break;

                            case 2:
                                Play.Source = new BitmapImage(new Uri
                                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\play.png"));
                                NombreEp.Text = ep.NombreEp;
                                BarraProgreso.BeginAnimation(WidthProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(1)));
                                BarraProgreso.Visibility = System.Windows.Visibility.Hidden;
                                break;
                        }
                        break;

                    case "NombreEp":

                        NombreEp.Text = ep.NombreEp;
                        if (ep.Estado == 0)
                        {
                            DateTime hoy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                            DateTime fechaep = new DateTime(ep.Fecha.Year, ep.Fecha.Month, ep.Fecha.Day, 12, 0, 0);
                            TimeSpan tiempo = fechaep - hoy;
                            NombreEp.Text += " - " + (tiempo.TotalDays < 1 ? "Hoy" : tiempo.TotalDays < 2 ? "Mañana" : (tiempo.Days + " Días"));
                            Ajustar(NombreEp);
                        }
                        break;

                    case "Fecha":

                        DateTime hoy2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                        DateTime fechaep2 = new DateTime(ep.Fecha.Year, ep.Fecha.Month, ep.Fecha.Day, 12, 0, 0);
                        TimeSpan tiempo2 = fechaep2 - hoy2;
                        NombreEp.Text = ep.NombreEp + " - " + (tiempo2.TotalDays < 1 ? "Hoy" : tiempo2.TotalDays < 2 ? "Mañana" : (tiempo2.Days + " Días"));
                        Ajustar(NombreEp);
                        break;              
                }

        /*        //Checa si el episodio principal esta cargado para actualizarlo
                if (Se.episodiosCargados)
                {


                    StackPanel epi = (StackPanel)temporadas[ep.Temporada - 1].Children[ep.Capitulo - 1];
                    Image estado = (Image)epi.Children[0];
                    TextBlock texto = (TextBlock)epi.Children[1];

                    switch (e.PropertyName)
                    {
                        case "Estado":
                            switch (ep.Estado)
                            {
                                case 1:
                                    estado.Source = new BitmapImage(new Uri
                                        (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\descs.png"));
                                    texto.Text = "E" + (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo + " - " + ep.NombreEp + " - 0%";
                                    break;

                                case 2:
                                    estado.Source = new BitmapImage(new Uri
                                        (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\plays.png"));
                                    texto.Text = "E" + (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo + " - " + ep.NombreEp;
                                    break;

                                case 3:
                                    estado.Source = new BitmapImage(new Uri
                                        (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\paloma.png"));
                                    break;
                            }
                            break;

                        case "NombreEp":
                            texto.Text = "E" + (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo + " - " + ep.NombreEp;
                            break;

                        case "Fecha":
                            texto.Text = "E" + (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo + " - " + ep.NombreEp + " - " + ep.Fecha.ToShortDateString();
                            break;
                    }

                }      */
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
            if (t.FontSize == 12) t.Margin = new Thickness(t.Margin.Left, 24, 0, 0);
            else t.Margin = new Thickness(t.Margin.Left, 22, 0, 0);
          
        }

        private void UpdateTextCapitulo()
        {
            if (Se.EpisodiosNoVistos.Count != 0)
            {
                Episodio ep = Se.EpisodiosNoVistos[0];
                Episodio.Text = "S" + (Se.Temporada < 10 ? "0" : "") + Se.Temporada + "E" + (Se.Capitulo < 10 ? "0" : "") + Se.Capitulo;
                NombreEp.Text = ep.NombreEp;

                //Si el episodio no ha salido
                if (ep.Estado == 0)
                {
                    //Se pone la imagen del Reloj
                    Play.Source = new BitmapImage(new Uri
                                   (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Clockb2.png"));

                    //Se calcula el tiempo que falta para que salga
                    DateTime hoy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                    DateTime fechaep = new DateTime(ep.Fecha.Year, ep.Fecha.Month, ep.Fecha.Day, 12, 0, 0);
                    TimeSpan tiempo = fechaep - hoy;

                    //Se pone el texto correspondiente
                    NombreEp.Text += " - " + (tiempo.TotalDays < 1 ? "Hoy" : tiempo.TotalDays < 2 ? "Mañana" : (tiempo.Days + " Días"));
                    //Se ajusta su tamaño
                    Ajustar(NombreEp);
                }
                else
                {
                    //Si el episodio se esta descargando
                    if (ep.Estado == 1)
                    {
                        double progreso = Principal.getProgesoTorrent(ep.Hash);
                        Play.Source = new BitmapImage(new Uri
                                   (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Descb.png"));

                        //Se pone el nombre + el progreso
                        NombreEp.Text += " - " + progreso + "%";

                        if (!episodiosMostrados)
                        {
                            BarraProgreso.BeginAnimation(WidthProperty, new DoubleAnimation(progreso * 3.5, TimeSpan.FromMilliseconds(1)));
                            BarraProgreso.Visibility = System.Windows.Visibility.Visible;
                        }

                        //Se ajusta
                        Ajustar(NombreEp);
                    }
                    else
                    {
                        if (ep.Estado == 2)
                        {
                            Play.Source = new BitmapImage(new Uri
                                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\play.png"));
                        }

                    }

                }
            }
            else
            {

                if (Se.Estado == 'c')
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

            
        }

        private void MostraroEsconderAlDia(bool mostrar)
        {
            if (mostrar)
            {
                if (!episodiosMostrados)
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
        }

        private void actualizarInterfazEpPr(object sender, EventArgs e)
        {
            if (!Se.AlDia && Se.EpisodiosNoVistos[0].Estado == 1)
            {
                BarraProgreso.Visibility = System.Windows.Visibility.Visible;

                double progreso = Principal.getProgesoTorrent(Se.EpisodiosNoVistos[0].Hash);
                //Se pone el nombre + el progreso
                NombreEp.Text = Se.EpisodiosNoVistos[0].NombreEp + " - " + progreso + "%";
                //Se ajusta
                Ajustar(NombreEp);

                TimeSpan tiempo = BarraProgreso.Width == 0 ? TimeSpan.FromMilliseconds(1) : TimeSpan.FromSeconds(1);

                //Se hace actualiza la barra de progreso
                BarraProgreso.BeginAnimation(WidthProperty, new DoubleAnimation(progreso * 3.5, tiempo));
              



            }
        }

        private void actualizarInterfazEpRes(object sender, EventArgs e)
        {
            if (!Se.AlDia)
            {
                foreach (Episodio ep in Se.EpisodiosNoVistos)
                {
                    if (ep.Estado == 1)
                    {
                        InterfazDesgloseEp epi = (InterfazDesgloseEp)temporadas[ep.Temporada - 1].Children[ep.Capitulo - 1];
                        epi.ActualizarPorcentaje(Principal.getProgesoTorrent(ep.Hash));
                        
                    }
                    else
                    {
                        if (ep.Estado == 0)
                        {
                            break;
                        }
                    }

                }
            }
        }

        private void GPrinc_MouseLeave(object sender, MouseEventArgs e)
        {



            TimeSpan tiempo = new TimeSpan(0, 0, 0, 0, 250);

            actualizarInterfazEpPrinc.Stop();

            ThicknessAnimation paraAbajo = new ThicknessAnimation(new Thickness(InfoRect.Margin.Left, 197, 0, 0), tiempo);
            paraAbajo.Completed += (sender2, e2) =>
                {
                    BarraProgreso.Visibility = System.Windows.Visibility.Hidden;
                    BarraProgreso.BeginAnimation(WidthProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(0)));
                };


            InformacionCaps.BeginAnimation(MarginProperty, paraAbajo);
        }

        private void GPrinc_MouseEnter(object sender, MouseEventArgs e)
        {            
            if (Se.EpisodiosNoVistos.Count != 0 && Se.EpisodiosNoVistos[0].Estado == 1)
            {
                BarraProgreso.Visibility = System.Windows.Visibility.Visible;
            }

            actualizarInterfazEpPr(null, null);
            actualizarInterfazEpPrinc.Start();

            InformacionCaps.Visibility = System.Windows.Visibility.Visible;
            TimeSpan tiempo = new TimeSpan(0, 0, 0, 0, 250);
            ThicknessAnimation paraArriba = new ThicknessAnimation(new Thickness(InfoRect.Margin.Left, 153, 0, 0), tiempo);
            InformacionCaps.BeginAnimation(MarginProperty, paraArriba);
        }

        public void agregarHandlerAEpisodioPrincipal()
        {
            Se.EpisodiosNoVistos[0].PropertyChanged += PropertyChangedEpisodioPrincipal;
        }

        public void quitarHandlerAEpisodioPrincipal()
        {
            Se.EpisodiosNoVistos[0].PropertyChanged -= PropertyChangedEpisodioPrincipal;
        }

        public void bajarInformacion()
        {
            if (!GPrinc.IsMouseOver && !episodiosMostrados)
            {
                GPrinc_MouseLeave(null, null);
            }


            if (!episodiosMostrados)
            {
                GPrinc.MouseEnter += GPrinc_MouseEnter;
                GPrinc.MouseLeave += GPrinc_MouseLeave;
            }
        }

        public void cambiarEpPrincipal(Episodio ep)
        {
            if (Se.Temporada != 0)
            {
                Se.EpisodiosNoVistos[0].PropertyChanged -= PropertyChangedEpisodioPrincipal;
            }

            Principal.cambiarEpPrincipal(ep);

            if (Se.Temporada != 0)
            {
                Se.EpisodiosNoVistos[0].PropertyChanged += PropertyChangedEpisodioPrincipal;
            }


        }

        public double getProgresoTorrent(string hash)
        {
            return Principal.getProgesoTorrent(hash);
        }



    }
}
