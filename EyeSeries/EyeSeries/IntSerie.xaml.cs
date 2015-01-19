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

        private ScrollViewer sv;
        private List<StackPanel> temporadas;
        private StackPanel master;

        public Boolean episodiosMostrados;
        private Boolean desplegado;

        private int tempActiva;

        private UTorrentClient uClient;
        private MainWindow Principal;

        private DispatcherTimer actualizarInterfazEpPrinc;
        private DispatcherTimer actualizarInterfazEpRestantes;

        public IntSerie(Serie s, MainWindow m)
        {
            Se = s;
            episodiosMostrados = false;
            desplegado = true;
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

        public void agregarDesgloseEpisodios()
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

                if (tempActiva + 1 == Se.EpisodiosVistos[tempActiva].Count)
                {
                    Sig.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    if (tempActiva == 0) Ant.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            
           

           ScrollEpisodios.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
           ScrollEpisodios.UpdateLayout();
           

           //Se pone la temporada seleccionada como texto
           SeleccT.Text = "Temporada " + (tempActiva + 1);

           temporadas[tempActiva].Opacity = 1;

           DesgloseEpisodios.Visibility = System.Windows.Visibility.Visible;
           DesgloseEpisodios.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));

           actualizarInterfazEpRestantes.Start();
            
        }

        public void agregarTemporadaAlDesglose()
        {
            if (temporadas.Count != 0)
            {
                Temporadas.Width += 350;
                Temporadas.Margin = new Thickness(Temporadas.Margin.Left - 350, 0, 0, 0);
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

            string imagen;

            //Dependiendo de su estado se pone la imagen
            if (ep.Estado == 0) imagen = "Clocks";
            else if (ep.Estado == 1) imagen = "descs";
            else if (ep.Estado == 2) imagen = "plays";
            else imagen = "paloma";

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

            //Se añade el stackPanel del episodio a la temporada
            temporada.Height += 17;
            temporada.Children.Add(spEpisodio);

            if (ep.Temporada != Se.Temporada || ep.Capitulo != Se.Capitulo)
            {
                ep.PropertyChanged += PropertyChangedEpisodiosRestantes;
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
                    if (!primeraVez)
                    {
                        DesgloseEpisodios.Visibility = System.Windows.Visibility.Visible;
                        DesgloseEpisodios.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                    }

                };

            Informacion.BeginAnimation(OpacityProperty, invisible);
            Ald.BeginAnimation(OpacityProperty, invisible2);
            Trian.BeginAnimation(OpacityProperty, invisible2);

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
                    DesgloseEpisodios.Visibility = System.Windows.Visibility.Hidden;
                    if (Se.AlDia)
                    {
                        Ald.Visibility = System.Windows.Visibility.Visible;
                        Trian.Visibility = System.Windows.Visibility.Visible;
                        Ald.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0,0,0,0,250)));
                        Trian.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0,0,0,0,250)));
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

        }

        private void Ant_MouseUp(object sender, MouseButtonEventArgs e)
        {

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

        }

        private void Play_MouseUp(object sender, MouseButtonEventArgs e)
        {

            
            Image im = (Image)sender;
            Episodio ep = (Episodio)im.Tag;
            
            Principal.cambiarEpPrincipal(ep);
            

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
                /*
                bool salto = false;
                for (int p = ep.Temporada - 1; p >= 0; p--)
                {
                    for (int m = (p == ep.Temporada - 1 ? ep.Capitulo - 1 : Se.EpisodiosVistos[p].Count - 1); m >= 0; m--)
                    {
                        Episodio aux = Se.EpisodiosVistos[p][m];
                        if (aux.Estado == 3)
                        {
                            salto = true;
                            break;
                        }
                        else
                        {
                            if (aux.Estado == 2)
                            {
                                Se.siguienteEpisodio();
                                aux.Estado = 3;
                                Se.PorVer--;
                            }
                            else
                            {
                                if (aux.Estado == 1)
                                {
                                    aux.Estado = 3;                                    
                                    Se.siguienteEpisodio();
                                    uClient.Torrents.Remove(aux.Hash, TorrentRemovalOptions.TorrentFileAndData);
                                    aux.Hash = "-1";
                                    Se.Descargando--;

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
                    for (int p = ep.Temporada - 1; p < Se.EpisodiosVistos.Count; p++)
                    {
                        for (int m = (p == ep.Temporada - 1 ? ep.Capitulo - 1 : 0); m < Se.EpisodiosVistos[p].Count; m++)
                        {
                            Episodio aux = Se.EpisodiosVistos[p][m];
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
                    Se.Temporada = ep.Temporada;
                    Se.Capitulo = ep.Capitulo;
*/
                                  
        }

        private void Play2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Se.EpisodiosNoVistos.Count != 0 && Se.EpisodiosNoVistos[0].Estado == 2)
            {
                Episodio ep = Se.EpisodiosNoVistos[0];
                string dir = @"C:\Users\Marcelo\Videos\Series\" + Se.Nombre + @"\Temporada " + ep.Temporada + @"\Episodio " + ep.Capitulo + ".mkv";
                System.Diagnostics.Process.Start(dir);
                ep.PropertyChanged -= PropertyChangedEpisodioPrincipal;
                ep.PropertyChanged += PropertyChangedEpisodiosRestantes;

                Se.siguienteEpisodio();
                ep = Se.EpisodiosNoVistos[0];

                ep.PropertyChanged -= PropertyChangedEpisodiosRestantes;
                ep.PropertyChanged += PropertyChangedEpisodioPrincipal;           
            }
        }

        private void IntSeries_Loaded(object sender, RoutedEventArgs e)
        {
            //Fanart
            RenderOptions.SetBitmapScalingMode(FanArt, BitmapScalingMode.Fant);
            FanArt.Source = new BitmapImage(new Uri
            (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\FanArt\" + Se.Nombre + ".jpg"));

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
            Se.PropertyChanged += new PropertyChangedEventHandler(PropertyChangedSerie);

            if (ep != null)
            {
                ep.PropertyChanged += new PropertyChangedEventHandler(PropertyChangedEpisodioPrincipal);
            }

            //Empieza el actualizador de interfaz
            actualizarInterfazEpPrinc.Start();
            

        }

        private void Pest_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GPrinc.MouseLeave -= GPrinc_MouseLeave;
            GPrinc.MouseEnter -= GPrinc_MouseEnter;
            DoubleAnimation opac = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 125));

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
                        Tapa.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 100)));
                    };
                    Tapa.BeginAnimation(HeightProperty, sube);
                    Tapa.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Tapa.Margin.Left, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 75)));
                };
                Tapa.Visibility = System.Windows.Visibility.Visible;
                Tapa.BeginAnimation(OpacityProperty, opac2);

            };

            Pest.BeginAnimation(OpacityProperty, opac);
            Eli.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 125)));


        }

        private void Cant_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GPrinc.MouseEnter += GPrinc_MouseEnter;
            GPrinc.MouseLeave += GPrinc_MouseLeave;
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
                        };

                        Tapa.BeginAnimation(OpacityProperty, opac2);

                    };

                    //Inicia animacion de bajar la tapa

                    Tapa.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Tapa.Margin.Left, 36, 0, 0), new TimeSpan(0, 0, 0, 0, 75)));
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
            Principal.EliminarSerie(Se.Numserie);
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
                                break;
                        }
                        break;

                    case "NombreEp":

                        NombreEp.Text = ep.NombreEp;
                        break;

                    case "Fecha":

                        DateTime hoy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                        DateTime fechaep = new DateTime(ep.Fecha.Year, ep.Fecha.Month, ep.Fecha.Day, 12, 0, 0);
                        TimeSpan tiempo = fechaep - hoy;
                        NombreEp.Text = ep.NombreEp + " - " + (tiempo.TotalDays < 1 ? "Hoy" : tiempo.TotalDays < 2 ? "Mañana" : (tiempo.Days + " Días"));
                        Ajustar(NombreEp);
                        break;              
                }

                //Checa si el episodio principal esta cargado para actualizarlo
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

                }      
            });

        }

        private void PropertyChangedEpisodiosRestantes(object sender, PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(
           DispatcherPriority.Normal,
           (ThreadStart)delegate
           {

               Episodio ep = (Episodio)sender;
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
                        Play.Source = new BitmapImage(new Uri
                                   (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Descb.png"));

                        //Se pone el nombre + el progreso
                        NombreEp.Text += " - " + Principal.getProgesoTorrent(ep.Hash) + "%";

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
                if (episodiosMostrados)
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
                //Se pone el nombre + el progreso
                NombreEp.Text = Se.EpisodiosNoVistos[0].NombreEp + " - " + Principal.getProgesoTorrent(Se.EpisodiosNoVistos[0].Hash) + "%";
                //Se ajusta
                Ajustar(NombreEp);
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
                        StackPanel epi = (StackPanel)temporadas[ep.Temporada - 1].Children[ep.Capitulo - 1];
                        TextBlock texto = (TextBlock)epi.Children[1];

                        texto.Text = "E" + (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo + " - " + ep.NombreEp + " - " + Principal.getProgesoTorrent(ep.Hash) + "%";
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

        public void Actualiza()
        {

            //Actualiza el episodio actual si es necesario
            Episodio act = Se.EpisodiosVistos[Se.Temporada - 1][Se.Capitulo - 1];
            if (act.Estado == 1 && desplegado)
            {
                double prog = (double)uClient.Torrents[act.Hash].ProgressInMils / 10.0;
                NombreEp.Text = act.NombreEp + " - " + Math.Round(prog, 1) + "%";
                Ajustar(NombreEp);
            }

            //Revisa todos los episodios
            bool sal = false;
            for (int m = Se.Temporada - 1; m < Se.EpisodiosVistos.Count; m++)
            {
                for (int j = (m == Se.Temporada - 1 ? Se.Capitulo - 1 : 0); j < Se.EpisodiosVistos[m].Count; j++)
                {
                    Episodio aux = Se.EpisodiosVistos[m][j];
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
                                    if (!episodiosMostrados && tempActiva == aux.Temporada - 1 && (double)uClient.Torrents[aux.Hash].ProgressInMils != 0)
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

        private void GPrinc_MouseLeave(object sender, MouseEventArgs e)
        {

            TimeSpan tiempo = new TimeSpan(0, 0, 0, 0, 250);

            actualizarInterfazEpPrinc.Stop();

            ThicknessAnimation paraAbajo = new ThicknessAnimation(new Thickness(InfoRect.Margin.Left, 197, 0, 0), tiempo);
            InformacionCaps.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(InfoRect.Margin.Left, 197, 0, 0), tiempo));
            desplegado = false;
        }

        private void GPrinc_MouseEnter(object sender, MouseEventArgs e)
        {
            actualizarInterfazEpPr(null, null);
            actualizarInterfazEpPrinc.Start();

            InformacionCaps.Visibility = System.Windows.Visibility.Visible;
            TimeSpan tiempo = new TimeSpan(0, 0, 0, 0, 250);
            ThicknessAnimation paraArriba = new ThicknessAnimation(new Thickness(InfoRect.Margin.Left, 153, 0, 0), tiempo);
            InformacionCaps.BeginAnimation(MarginProperty, paraArriba);
            desplegado = true;
        }

  



    }
}
