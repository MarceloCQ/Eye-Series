using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Serie> Series;
        List<IntSerie> IntSeries;
        List<Grid> Pags;
        public Image Agregar;
        public Rectangle tapa;
        private Agregar Agr;
        private int PagAct;
        private Image sig;
        private Image ant;
        DispatcherTimer empieza;
        UTorrentClient uClient;
        public DispatcherTimer control;
        private int Cargado;
        private bool terminoActualizacion;
        private bool SeriesEnEspera;
        private List<Serie> SeriesaBorrar;
        private List<IntSerie> InterfacesaBorrar;
        private DateTime UltimaActualizacion;
        private bool Actualizar;


        private bool TerminoActualizacion
        {
            get { return terminoActualizacion; }
            set
            {
                if (value != terminoActualizacion)
                {
                    terminoActualizacion = value;
                    if (value)
                    {
                        if (SeriesEnEspera)
                        {
                            BorrarSeriesenEspera();
                        }
                    }
                }
            }
        }

        public MainWindow()
        {
            
            InitializeComponent();
            uClient = new UTorrentClient(new Uri("http://127.0.0.1:8080/gui/"), "admin", "admin", 1000000);
            control = new DispatcherTimer()
            {
                Interval = new TimeSpan(0,0,1),
            };
            control.Tick += Actualiza;
            PagAct = 0;
            Cargado = 0;
            TerminoActualizacion = true;
            SeriesEnEspera = false;
            SeriesaBorrar = new List<Serie>();
            InterfacesaBorrar = new List<IntSerie>();
           

        }
        
        private void EyeSeries_Initialized(object sender, EventArgs e)
        {
          
        }

        private void EyeSeries_ContentRendered(object sender, EventArgs e)
        {
            
        }

        private void EyeSeries_Loaded(object sender, RoutedEventArgs e)
        {
            Pags = new List<Grid>();
            Series = new List<Serie>();
            IntSeries = new List<IntSerie>();
            CrearPag(0);
            StreamReader leer = new StreamReader(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\General.txt");
            int temp = 0;
            int capi = 0;
            int i = 0;

            //Revisa si la ultima actualizacion fue hace mas de 14 dias
            string ultact = leer.ReadLine();
            UltimaActualizacion = new DateTime(Convert.ToInt32(ultact.Split('/')[2]), Convert.ToInt32(ultact.Split('/')[1]), Convert.ToInt32(ultact.Split('/')[0]));            
            Actualizar = (DateTime.Now - UltimaActualizacion).TotalDays > 14;


            while (!leer.EndOfStream)
            {
                string nombre = leer.ReadLine();
                int id = Convert.ToInt32(leer.ReadLine());
                int subid = Convert.ToInt32(leer.ReadLine());
                string cap = leer.ReadLine();
                temp = Convert.ToInt32(cap.Split(' ')[0]);
                capi = Convert.ToInt32(cap.Split(' ')[1]);
                char estado = Convert.ToChar(leer.ReadLine());
                string hora = leer.ReadLine();

                Serie s = new Serie(id, nombre, i, estado, hora, subid);
                Series.Add(s);
                AgregarSerie(s, temp, capi, i);
                leer.ReadLine();
                i++;

            }

            leer.Close();

            Agregar = new Image()
            {
                Height = 120,
                Width = 120,
                Source = new BitmapImage(new Uri
                         (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Plus.png")),
                Stretch = Stretch.None,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            };

            Agregar.MouseUp += new MouseButtonEventHandler(Agregar_MouseUp);

            tapa = new Rectangle()
            {
                Width = 1066,
                Height = 599,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Fill = new SolidColorBrush(Colors.Black),
                Opacity = 0,
                Visibility = Visibility.Hidden,
            };


            int pag = i / 9;
            int fil = (i % 9) / 3;
            int col = (i % 9) % 3;
            if (i % 9 == 0 && i != 0)
                CrearPag(pag);

            Agr = new Agregar(this);
            Agr.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Agr.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Grid.SetColumnSpan(Agr, 3);
            Grid.SetRowSpan(Agr, 3);




            Pags[pag].Children.Add(tapa);
            Pags[pag].Children.Add(Agr);
            Grid.SetColumnSpan(tapa, 3);
            Grid.SetRowSpan(tapa, 3);


            Pags[pag].Children.Add(Agregar);
            Grid.SetRow(Agregar, fil);
            Grid.SetColumn(Agregar, col);


            sig = new Image()
            {
                Width = 81,
                Height = 81,
                Source = new BitmapImage(new Uri
                         (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\sigp.png")),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Visibility = System.Windows.Visibility.Hidden,
                Tag = "sig",
                Opacity = 0,

            };

            ant = new Image()
            {
                Width = 81,
                Height = 81,
                Source = new BitmapImage(new Uri
                         (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\antp.png")),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                Visibility = System.Windows.Visibility.Hidden,
                Tag = "ant",
                Opacity = 0,

            };
            sig.MouseUp += Sig_MouseUp;
            ant.MouseUp += Ant_MouseUp;
            sig.MouseEnter += SigAnt_MouseOver;
            ant.MouseEnter += SigAnt_MouseOver;
            sig.MouseLeave += SigAnt_MouseLeave;
            ant.MouseLeave += SigAnt_MouseLeave;



            Pags[0].Children.Add(sig);
            Pags[0].Children.Add(ant);
            Grid.SetRow(sig, 1);
            Grid.SetColumn(sig, 2);
            Grid.SetRow(ant, 1);
            Grid.SetColumn(ant, 0);

            if (Pags.Count > 1) sig.Visibility = System.Windows.Visibility.Visible;
            empieza = new DispatcherTimer()
            {
                Interval = new TimeSpan(0,0,3),
            };
            empieza.Tick += new EventHandler(Emp);
            control.Start();

            
         
        }

        private void Agregar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation desap = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            desap.Completed += (sender2, e2) =>
                {
                    Agregar.Visibility = System.Windows.Visibility.Hidden;
                    Agr.AnimaInicio();
                };
            tapa.Visibility = Visibility.Visible;
            tapa.BeginAnimation(OpacityProperty, new DoubleAnimation(.7, new TimeSpan(0, 0, 0, 0, 250)));
            Agregar.BeginAnimation(OpacityProperty, desap);
            

        }

        private void Sig_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PagAct++;
            gPrinc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(gPrinc.Margin.Left - 1058, 0, gPrinc.Margin.Right + 1058, 0), new TimeSpan(0, 0, 0, 0, 500)));
            Pags[PagAct - 1].Children.Remove(sig);
            Pags[PagAct].Children.Add(sig);
            Pags[PagAct - 1].Children.Remove(ant);
            Pags[PagAct].Children.Add(ant);
            ant.Visibility = System.Windows.Visibility.Visible;
            if (PagAct == Pags.Count - 1)
                sig.Visibility = System.Windows.Visibility.Hidden;



        }

        private void Ant_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PagAct--;
            gPrinc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(gPrinc.Margin.Left + 1058, 0, gPrinc.Margin.Right - 1058, 0), new TimeSpan(0, 0, 0, 0, 500)));
            Pags[PagAct + 1].Children.Remove(ant);
            Pags[PagAct].Children.Add(ant);
            Pags[PagAct + 1].Children.Remove(sig);
            Pags[PagAct].Children.Add(sig);
            sig.Visibility = System.Windows.Visibility.Visible;
            if (PagAct == 0)
                ant.Visibility = System.Windows.Visibility.Hidden;
        }

        private void SigAnt_MouseOver(object sender, MouseEventArgs e)
        {
            Image im = (Image)sender;
         
            if (sig.IsMouseOver)
            {
                if (IntSeries[5 * (PagAct + 1)].normal)
                    im.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
            }
            else
            {
                if (IntSeries[3 * (PagAct + 1)].normal)
                    im.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
            }



        }

        private void SigAnt_MouseLeave(object sender, MouseEventArgs e)
        {
            Image im = (Image)sender;
            im.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
        }



        public void CrearPag(int num)
        {
          
            Grid aux = new Grid()
            {
                Height = 599,
                Width = 1058,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new Thickness(1058 * num, 0, 0, 0),

            };

            for (int i = 0; i < 3; i++) aux.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < 3; i++) aux.ColumnDefinitions.Add(new ColumnDefinition());

           
            for (int i = 0; i < 2; i++)
            {
                Rectangle vertical = new Rectangle()
                {
                    Width = 4,
                    Height = 599,
                    Fill = new SolidColorBrush(Colors.Black),
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    Margin = new Thickness(i == 0? 350 : 704, 0,0,0),
                };

                aux.Children.Add(vertical);
                vertical.SetValue(Grid.ColumnSpanProperty, 3);
                vertical.SetValue(Grid.RowSpanProperty, 3);
                
            }

            for (int i = 0; i < 2; i++)
            {
                Rectangle horizontal = new Rectangle()
                {
                    Width = 1058,
                    Height = 4,
                    Fill = new SolidColorBrush(Colors.Black),
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    Margin = new Thickness(0, i == 0 ? 197 : 398, 0, 0),
                };

                aux.Children.Add(horizontal);
                horizontal.SetValue(Grid.ColumnSpanProperty, 3);
                horizontal.SetValue(Grid.RowSpanProperty, 3);
            }

            






            Pags.Add(aux);
            gPrinc.Children.Add(aux);  
            if (num != 0)
            {
                gPrinc.Margin = new Thickness(gPrinc.Margin.Left, 0, PagAct == 0? -1068 * num : -1068, 0);               
            }
            if (num != 0 && PagAct != 0)
            {
                gPrinc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(gPrinc.Margin.Left, 0, -1068, 0), new TimeSpan(0, 0, 0, 0, 1)));
            }

     


            
        }

        private void AgregarSerie(Serie s, int temp, int capi, int num)
        {
            int pag = num / 9;
            int fil = (num % 9) / 3;
            int col = (num % 9) % 3;
            if (num % 9 == 0 && num != 0)
            {
                CrearPag(pag);
            }

            IntSerie aux;
            aux = new IntSerie(s, this, uClient);
            if (col == 1) aux.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            else if (col == 2) aux.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

            if (fil == 1) aux.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            else if (fil == 2) aux.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
           
            Pags[pag].Children.Add(aux);
            
            IntSeries.Add(aux);
            Grid.SetRow(aux, fil);
            Grid.SetColumn(aux, col);
            Grid.SetRowSpan(aux, 1);
            Grid.SetColumnSpan(aux, 1);

            
            
            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (ThreadStart)delegate
                    {
                       aux.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 500))); 
                    });
                    
                    s.AlimentarEps(temp, capi);
                    Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (ThreadStart)delegate
                    {
                       
                        aux.AgregarEps();
            
                    });
                };
            b.RunWorkerCompleted += (sender2, e2) =>
                {
                    Cargado++;
                    if (Cargado == Series.Count)
                    {
                        
                        empieza.Start();
                        if (Actualizar)
                        {
                            TerminoActualizacion = false;
                            ActualizarBD();
                        }
                        
                        

                    }
                };
                b.RunWorkerAsync();
               


        }

        public void AgregarNSerie(Serie s, int temp, int capi)
        
        {
           
            int num = Series.Count;
            int pag = num / 9;
            int fil = (num % 9) / 3;
            int col = (num % 9) % 3;
            Series.Add(s);
            s.addSerie(temp, capi);
            control.Stop();
            IntSerie nueva = new IntSerie(s, this, uClient);
            Grid.SetColumn(nueva, col);
            Grid.SetRow(nueva, fil);

            Pags[pag].Children.Add(nueva);


            if (col == 1) nueva.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            else if (col == 2) nueva.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

            if (fil == 1) nueva.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            else if (fil == 2) nueva.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(
                   DispatcherPriority.Normal,
                   (ThreadStart)delegate
                   {
                       nueva.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 500)));
                   });

                s.Descarga();
                
                    Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (ThreadStart)delegate
                    {
                        nueva.AgregarEps();
                    });

                };
            b.RunWorkerCompleted += (sender2, e2) =>
                {
                    IntSeries.Add(nueva);
                    control.Start();
                };
            b.RunWorkerAsync();
            CrearArchivo();
            num++;

            Pags[pag].Children.Remove(sig);
            Pags[pag].Children.Add(sig);
            Pags[pag].Children.Remove(ant);
            Pags[pag].Children.Add(ant);

            pag = num / 9;
            fil = (num % 9) / 3;
            col = (num % 9) % 3;
            Grid.SetColumn(Agregar, col);
            Grid.SetRow(Agregar, fil);

            if (num % 9 == 0 && num != 0)
            {
                CrearPag(pag);
                Pags[pag - 1].Children.Remove(Agregar);
                Pags[pag].Children.Add(Agregar);
                Pags[pag - 1].Children.Remove(tapa);
                Pags[pag].Children.Add(tapa);
                Pags[pag - 1].Children.Remove(Agr);
                Pags[pag].Children.Add(Agr);
                sig.Visibility = System.Windows.Visibility.Visible;
            }

            
            Pags[pag].Children.Remove(tapa);
            Pags[pag].Children.Add(tapa);
            Pags[pag].Children.Remove(Agr);
            Pags[pag].Children.Add(Agr);


            

            Agregar.Visibility = System.Windows.Visibility.Visible;
            Agregar.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));





        }

        public void EliminarSerie(int num)
        {
            control.Stop();
            //Interfaz de la serie a eliminar
            IntSerie EliminaInt = IntSeries[num];

            //Serie de la serie a eliminar
            Serie EliminaS = Series[num];

            //Pagina en que se encuentra la serie a eliminar
            int pag = num / 9;

            //Revisa los episodios de la serie a eliminar a ver si hay alguno descargandose
            if (EliminaS.Temporada != 0 && EliminaS.Capitulo != 0)
            {
                for (int m = EliminaS.Temporada - 1; m < EliminaS.Episodios.Count; m++)
                {
                    for (int j = (m == EliminaS.Temporada - 1 ? EliminaS.Capitulo - 1 : 0); j < EliminaS.Episodios[m].Count; j++)
                    {
                        Episodio aux = EliminaS.Episodios[m][j];
                        if (aux.Estado == 1)
                        {
                            uClient.Torrents.Remove(aux.Hash, TorrentRemovalOptions.TorrentFileAndData);
                        }
                    }
                }
            }

            //Declaracion animacion de desaparece
            DoubleAnimation desaparece = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));



            //Cuando se termina de desaparecer la serie
            desaparece.Completed += (sender, e) =>
                {
                    //Se esconde la interfaz de la serie
                    EliminaInt.Visibility = System.Windows.Visibility.Hidden;

                    //Se quita de la pagina
                    Pags[pag].Children.Remove(EliminaInt);
                    
                    //Se itera sobre las series para cambiarlas de posición
                    //El entero i corresponde a el nuevo numserie de las series iteradas
                    int i = num;
                    while(i < Series.Count - 1)
                    {
                        //Serie de serie a mover
                        Serie temps = Series[i + 1];
                        //Interfaz de serie a mover
                        IntSerie tempi = IntSeries[i + 1];

                        //Nueva fila, pagina y columna de la serie a mover
                        int npag = i / 9;
                        int nfil = (i % 9) / 3;
                        int ncol = (i % 9) % 3;

                        //Se decrementa el numero interno de la serie
                        temps.Numserie--;

                        //Se reposiciona la serie
                        Grid.SetColumn(tempi, ncol);
                        Grid.SetRow(tempi, nfil);

                        //Se reposicionan conforme a su posicion en el grid (por columna y fila)
                        if (ncol == 0) tempi.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        else if (ncol == 1) tempi.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        else if (ncol == 2) tempi.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

                        if (nfil == 0) tempi.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        else if (nfil == 1) tempi.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        else if (nfil == 2) tempi.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

                        //Si la serie es la primera de una nueva pagina entonces se va a la pagina anterior
                        if ((i + 1) % 9 == 0 && i != 0)
                        {
                            Pags[npag + 1].Children.Remove(tempi);
                            Pags[npag].Children.Add(tempi);
                        }

                        i++;
                    
                 
                    }

                    //Se mueve de posicion el boton de agregr
                    int npagagr = i / 9;
                    int nfilagr = (i % 9) / 3;
                    int ncolagr = (i % 9) % 3;

                    //Se reposiciona la imagen
                    Grid.SetRow(Agregar, nfilagr);
                    Grid.SetColumn(Agregar, ncolagr);

                    //Si agregar es la primera de una nueva pagina entonces se va a la pagina anterior, junto con la interfaz
                    //de agregar
                    if ((i + 1) % 9 == 0 && i != 0)
                    {
                        Pags[npagagr + 1].Children.Remove(Agregar);
                        Pags[npagagr].Children.Add(Agregar);
                        Pags[npagagr + 1].Children.Remove(tapa);
                        Pags[npagagr].Children.Add(tapa);
                        Pags[npagagr + 1].Children.Remove(Agr);
                        Pags[npagagr].Children.Add(Agr);

                        //Se elimina la pagina 
                        gPrinc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(gPrinc.Margin.Left, 0, gPrinc.Margin.Right + 1058, 0), new TimeSpan(0, 0, 0, 0, 1)));
                        gPrinc.Children.Remove(Pags[Pags.Count - 1]);
                        Pags.RemoveAt(Pags.Count - 1);

                        if (PagAct == Pags.Count - 1) sig.Visibility = System.Windows.Visibility.Hidden;
                        
                        
                    }

                    //Checa si termina la actualizacion
                    if (TerminoActualizacion)
                    {
                        //Se quita la serie de la lista de series
                        Series.Remove(EliminaS);
                        //Se quita la serie de las interfaces
                        IntSeries.Remove(EliminaInt);
                        control.Start();
                        CrearArchivo();
                        System.IO.File.Delete(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\" + EliminaS.Nombre + ".txt");
                       
                    }
                    //Si no ha terminado, se meten en la lista de espera
                    else
                    {
                        SeriesEnEspera = true;
                        SeriesaBorrar.Add(Series[num]);
                        InterfacesaBorrar.Add(IntSeries[num]);

                    }
                    

                };
            
            EliminaInt.BeginAnimation(OpacityProperty, desaparece);
        }

        private void BorrarSeriesenEspera()
        {
            for (int i = 0; i < SeriesaBorrar.Count; i++)
            {
                Series.Remove(SeriesaBorrar[i]);
                IntSeries.Remove(InterfacesaBorrar[i]);
            }
            SeriesEnEspera = false;
        }

        public void CrearArchivo()
        {
            StreamWriter escribe = new StreamWriter(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\General.txt");
            escribe.WriteLine(UltimaActualizacion.Day.ToString() + "/" + UltimaActualizacion.Month.ToString() + "/" + UltimaActualizacion.Year.ToString());
            foreach (Serie s in Series)
            {
                escribe.WriteLine(s.Imprimir());
            }

            escribe.Close();
        }

        private void Emp(object sender, EventArgs e)
        {
            foreach (IntSerie s in IntSeries)
            {
                if (s.normal && !s.ZonaC.IsMouseOver)
                    s.ZonaC_MouseLeave(null, null);
            }
            empieza.Stop();
        }

        private void Actualiza(object sender, EventArgs e)
        {
            foreach (IntSerie s in IntSeries)
            {
                if (s.se.Temporada != 0 && s.se.Capitulo != 0)
                 s.Actualiza();
            }
        }

        private void ActualizarBD()
        {
            Application.Current.Dispatcher.Invoke(
            DispatcherPriority.Normal,
            (ThreadStart)delegate
            {
                BackgroundWorker b = new BackgroundWorker();
                    b.DoWork += (sender, e) =>
                        {
                            int i = 0;
                            foreach (Serie s in Series)
                            {
                                s.ActualizarBD(IntSeries[i]);
                                i++;
                            }
                            UltimaActualizacion = DateTime.Now;
                            TerminoActualizacion = true;
                            CrearArchivo();
                        };

                    b.RunWorkerAsync();
            });
            
        }

    }
}
