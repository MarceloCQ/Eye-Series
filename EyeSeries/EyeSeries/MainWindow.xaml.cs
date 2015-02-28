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

        public Image IconoAgregar;
        public Rectangle Tapa;
        private Agregar InterfazAgregar;

        private int PagAct;

        private Image IconoSiguiente;
        private Image IconoAnterior;

        DispatcherTimer empieza;
        public DispatcherTimer control;
        private int Cargado;
        private bool terminoActualizacion;
        private bool SeriesEnEspera;
        private List<Serie> SeriesaBorrar;
        private List<IntSerie> InterfacesaBorrar;
        private DateTime UltimaActualizacion;
        private bool Actualizar;

        private Controlador controlador;

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
            control = new DispatcherTimer()
            {
                Interval = new TimeSpan(0,0,1),
            };
       //     control.Tick += Actualiza;
            PagAct = 0;
            Cargado = 0;
            TerminoActualizacion = true;
            SeriesEnEspera = false;
            SeriesaBorrar = new List<Serie>();
            Pags = new List<Grid>();
            IntSeries = new List<IntSerie>();
            InterfacesaBorrar = new List<IntSerie>();
            controlador = new Controlador(this);
           

        }
        
        private void EyeSeries_Initialized(object sender, EventArgs e)
        {
          
        }

        private void EyeSeries_ContentRendered(object sender, EventArgs e)
        {
             
        }

        private void EyeSeries_Loaded(object sender, RoutedEventArgs e)
        {

            controlador.iniciarAplicacion();

            /*
            Series = new List<Serie>();
            
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

                Serie s = new Serie(id, nombre,temp, capi, i, estado, hora, subid);
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


            IconoSiguiente = new Image()
            {
                Width = 81,
                Height = 81,
                Source = new BitmapImage(new Uri
                         (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\sigp.png")),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Visibility = System.Windows.Visibility.Hidden,
                Tag = "IconoSiguiente",
                Opacity = 0,

            };

            IconoAnterior = new Image()
            {
                Width = 81,
                Height = 81,
                Source = new BitmapImage(new Uri
                         (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\antp.png")),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                Visibility = System.Windows.Visibility.Hidden,
                Tag = "IconoAnterior",
                Opacity = 0,

            };
            IconoSiguiente.MouseUp += Sig_MouseUp;
            IconoAnterior.MouseUp += Ant_MouseUp;
            IconoSiguiente.MouseEnter += SigAnt_MouseOver;
            IconoAnterior.MouseEnter += SigAnt_MouseOver;
            IconoSiguiente.MouseLeave += SigAnt_MouseLeave;
            IconoAnterior.MouseLeave += SigAnt_MouseLeave;



            Pags[0].Children.Add(IconoSiguiente);
            Pags[0].Children.Add(IconoAnterior);
            Grid.SetRow(IconoSiguiente, 1);
            Grid.SetColumn(IconoSiguiente, 2);
            Grid.SetRow(IconoAnterior, 1);
            Grid.SetColumn(IconoAnterior, 0);

            if (Pags.Count > 1) IconoSiguiente.Visibility = System.Windows.Visibility.Visible;
            empieza = new DispatcherTimer()
            {
                Interval = new TimeSpan(0,0,3),
            };
            empieza.Tick += new EventHandler(Emp);
            control.Start();

          /*  string path = @"C:\Users\Marcelo\Videos\Series\";
            string print = "";
            foreach (Serie s in Series)
            {
                for (int p = s.Temporada - 1; p < s.EpisodiosVistos.Count; p++)
                {
                    int j = (p == s.Temporada - 1 ? s.Capitulo - 1 : 0);
                    while (j < s.EpisodiosVistos[i].Count)
                    {
                        Episodio aux = s.EpisodiosVistos[p][j];
                        if (aux.Estado == 2)
                        {
                            bool b = System.IO.File.Exists(@"C:\Users\Marcelo\Videos\Series\" + s.Nombre + @"\Temporada " + aux.Temporada + @"\Episodio " + aux.Capitulo + ".mkv");
                            if (!b)
                            {
                                print += s.Nombre + s.Temporada + " " + s.Capitulo + "\r\n";
                            }
                        }
                    }
                }
            }
            MessageBox.Show(print);*/
            
         
        }

        private void IconoAgregar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation desap = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            desap.Completed += (sender2, e2) =>
                {
                    IconoAgregar.Visibility = System.Windows.Visibility.Hidden;
                    InterfazAgregar.animaEtapa1();
                };
            Tapa.Visibility = Visibility.Visible;
            Tapa.BeginAnimation(OpacityProperty, new DoubleAnimation(.7, new TimeSpan(0, 0, 0, 0, 250)));
            IconoAgregar.BeginAnimation(OpacityProperty, desap);
            

        }

        private void Sig_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PagAct++;
            gPrinc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(gPrinc.Margin.Left - 1058, 0, gPrinc.Margin.Right + 1058, 0), new TimeSpan(0, 0, 0, 0, 500)));
            Pags[PagAct - 1].Children.Remove(IconoSiguiente);
            Pags[PagAct].Children.Add(IconoSiguiente);
            Pags[PagAct - 1].Children.Remove(IconoAnterior);
            Pags[PagAct].Children.Add(IconoAnterior);
            IconoAnterior.Visibility = System.Windows.Visibility.Visible;
            if (PagAct == Pags.Count - 1)
                IconoSiguiente.Visibility = System.Windows.Visibility.Hidden;



        }

        private void Ant_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PagAct--;
            gPrinc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(gPrinc.Margin.Left + 1058, 0, gPrinc.Margin.Right - 1058, 0), new TimeSpan(0, 0, 0, 0, 500)));
            Pags[PagAct + 1].Children.Remove(IconoAnterior);
            Pags[PagAct].Children.Add(IconoAnterior);
            Pags[PagAct + 1].Children.Remove(IconoSiguiente);
            Pags[PagAct].Children.Add(IconoSiguiente);
            IconoSiguiente.Visibility = System.Windows.Visibility.Visible;
            if (PagAct == 0)
                IconoAnterior.Visibility = System.Windows.Visibility.Hidden;
        }

        private void SigAnt_MouseOver(object sender, MouseEventArgs e)
        {
            Image im = (Image)sender;
         
            if (IconoSiguiente.IsMouseOver)
            {
                if (!IntSeries[5 * (PagAct + 1)].episodiosMostrados)
                    im.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
            }
            else
            {
                if (!IntSeries[3 * (PagAct + 1)].episodiosMostrados)
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
                Thickness nueva = new Thickness(gPrinc.Margin.Left, 0, PagAct == 0 ? -1068 * num : -1068, 0);
                gPrinc.BeginAnimation(MarginProperty, new ThicknessAnimation(nueva, TimeSpan.FromMilliseconds(1)));             
            }
            if (num != 0 && PagAct != 0)
            {
                gPrinc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(gPrinc.Margin.Left, 0, -1068, 0), new TimeSpan(0, 0, 0, 0, 1)));
            }

     


            
        }


        public void agregarSerie(Serie s, int num)
        {
            
            int pag = num / 9;     //Se calcula la pagina en la que va a estar la serie
            int fil = (num % 9) / 3; //Se calcula la fila en la que se va a ubicar
            int col = (num % 9) % 3;  //Se calcula la columna en la que se va a poner
             
            IntSerie aux = new IntSerie(s, this);  //Se crea la interfaz de la nueva serie

            //Se alinea depndiendo de en donde se coloque en el vértice horizontal
            if (col == 1) aux.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            else if (col == 2) aux.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

            //Se alinea dependiendo de en donde se coloque en el vértice vertical
            if (fil == 1) aux.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            else if (fil == 2) aux.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;


            Pags[pag].Children.Add(aux);  //Se añade a los hijos del GRID de la página correspondiente
            IntSeries.Add(aux);           //Se añade a la lista de interfaces Seri.

            //Se pone en la fila y columna correspondiente
            Grid.SetRow(aux, fil);
            Grid.SetColumn(aux, col);

            //Se pone que no se pueda salir ni de la fila ni de la columna
            Grid.SetRowSpan(aux, 1);
            Grid.SetColumnSpan(aux, 1);

            //Se anima para que aparezca
            aux.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 500)));

            /*
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
            */




        }


        public void agregarNuevaSerie(Serie s, int temp, int cap)
        {
            controlador.agregarNuevaSerie(s, temp, cap);
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
                for (int m = EliminaS.Temporada - 1; m < EliminaS.EpisodiosVistos.Count; m++)
                {
                    for (int j = (m == EliminaS.Temporada - 1 ? EliminaS.Capitulo - 1 : 0); j < EliminaS.EpisodiosVistos[m].Count; j++)
                    {
                        Episodio aux = EliminaS.EpisodiosVistos[m][j];
                        if (aux.Estado == 1)
                        {
                            //uClient.Torrents.Remove(aux.Hash, TorrentRemovalOptions.TorrentFileAndData);
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
                    Grid.SetRow(IconoAgregar, nfilagr);
                    Grid.SetColumn(IconoAgregar, ncolagr);

                    //Si agregar es la primera de una nueva pagina entonces se va a la pagina anterior, junto con la interfaz
                    //de agregar
                    if ((i + 1) % 9 == 0 && i != 0)
                    {
                        Pags[npagagr + 1].Children.Remove(IconoAgregar);
                        Pags[npagagr].Children.Add(IconoAgregar);
                        Pags[npagagr + 1].Children.Remove(Tapa);
                        Pags[npagagr].Children.Add(Tapa);
                        Pags[npagagr + 1].Children.Remove(InterfazAgregar);
                        Pags[npagagr].Children.Add(InterfazAgregar);

                        //Se elimina la pagina 
                        gPrinc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(gPrinc.Margin.Left, 0, gPrinc.Margin.Right + 1058, 0), new TimeSpan(0, 0, 0, 0, 1)));
                        gPrinc.Children.Remove(Pags[Pags.Count - 1]);
                        Pags.RemoveAt(Pags.Count - 1);

                        if (PagAct == Pags.Count - 1) IconoSiguiente.Visibility = System.Windows.Visibility.Hidden;
                        
                        
                    }

                    //Checa si termina la actualizacion
                    if (TerminoActualizacion)
                    {
                        //Se quita la serie de la lista de series
                        Series.Remove(EliminaS);
                        //Se quita la serie de las interfaces
                        IntSeries.Remove(EliminaInt);
                        control.Start();
                        crearArchivo();
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

        public void eliminarSeriedeInterfaz(int num)
        {
            //Interfaz de la serie a eliminar
            IntSerie eliminaInt = IntSeries[num];

            //Serie de la serie a eliminar
            Serie eliminaS = eliminaInt.Se;

            //Pagina en que se encuentra la serie a eliminar
            int pag = num / 9;

            //Declaracion animacion de desaparece
            DoubleAnimation desapareceSerie = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));

            desapareceSerie.Completed += (sender, e) =>
                {
                    //Se esconde la interfaz de la serie
                    eliminaInt.Visibility = System.Windows.Visibility.Hidden;

                    //Se quita de la pagina
                    Pags[pag].Children.Remove(eliminaInt);

                    //Se reacomodan las series
                    reacomodarSeries(num);

                    //Se reacomoda la interfaz añadir
                    reacomodarInterfazAñadir(IntSeries.Count - 1, false);

                    IntSeries.Remove(eliminaInt);

                    


                };

            eliminaInt.BeginAnimation(OpacityProperty, desapareceSerie);


        }

        public void eliminarSerie(int num)
        {
            controlador.eliminarSerie(num);
        }

        private void reacomodarSeries(int num)
        {
            //Se itera sobre las series para cambiarlas de posición
            //El entero i corresponde a el nuevo numserie de las series iteradas
            int i = num;
            while (i < IntSeries.Count - 1)
            {
                //Interfaz de serie a mover
                IntSerie tempi = IntSeries[i + 1];

                tempi.Se.Numserie--;

                //Nueva fila, pagina y columna de la serie a mover
                int npag = i / 9;
                int nfil = (i % 9) / 3;
                int ncol = (i % 9) % 3;

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

        public void crearArchivo()
        {
            StreamWriter escribe = new StreamWriter(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\General.txt");
            escribe.WriteLine(UltimaActualizacion.Day.ToString() + "/" + UltimaActualizacion.Month.ToString() + "/" + UltimaActualizacion.Year.ToString());
            foreach (Serie s in Series)
            {
                escribe.WriteLine(s.Imprimir());
            }

            escribe.Close();
        }

        public void bajarInformacion()
        {
            DispatcherTimer bajaTodo = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(5),
            };

            bajaTodo.Tick += bajarInfoEvento;
            bajaTodo.Start();
        }

        public void bajarInfoEvento(object sender, EventArgs e)
        {
            foreach (IntSerie i in IntSeries)
            {
                i.bajarInformacion();
            }

            ((DispatcherTimer)sender).Stop();
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
                            crearArchivo();
                        };

                    b.RunWorkerAsync();
            });
            
        }

        private void EyeSeries_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.F1)
            {
                
                
               string path = @"C:\Users\Marcelo\Videos\Series\";
               string print = "";
               foreach (Serie s in Series)
               {
                   if (!s.AlDia)
                   {
                       for (int p = s.Temporada - 1; p < s.EpisodiosVistos.Count; p++)
                       {
                           int j = (p == s.Temporada - 1 ? s.Capitulo - 1 : 0);
                           while (j < s.EpisodiosVistos[p].Count)
                           {
                               Episodio aux = s.EpisodiosVistos[p][j];
                               if (aux.Estado == 2)
                               {
                                   bool b = System.IO.File.Exists(path + s.Nombre + @"\Temporada " + aux.Temporada + @"\Episodio " + aux.Capitulo + ".mkv");
                                   if (!b)
                                   {
                                       print += s.Nombre + s.Temporada + " " + s.Capitulo + "\r\n";
                                   }
                               }
                               j++;
                           }
                       }
                   }
               }
               MessageBox.Show(print);
                
            }
            else
            {
                if (e.Key == Key.F2)
                {
                    controlador.Series[0].EpisodiosNoVistos[0].Fecha = controlador.Series[0].EpisodiosNoVistos[0].Fecha.AddDays(1);
                }
                else
                {
                    if (e.Key == Key.F3)
                    {

                        foreach (Serie s in Series)
                        {
                            string pathEscribir = @"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\EpisodiosNoVistos\" + s.Nombre + ".txt";
                            StreamWriter esc = new StreamWriter(pathEscribir);
                            if (!s.AlDia || s.Temporada != 0)
                            {
                                for (int p = s.Temporada - 1; p < s.EpisodiosVistos.Count; p++)
                                {
                                    int j = (p == s.Temporada - 1 ? s.Capitulo - 1 : 0);
                                    while (j < s.EpisodiosVistos[p].Count)
                                    {
                                        Episodio aux = s.EpisodiosVistos[p][j];
                                        esc.WriteLine(aux.Imprimir());
                                        j++;
                                    }
                                }
                            }

                            esc.Close();

                        }


                        MessageBox.Show("Listo");
                    }
                    else
                    {
                        if (e.Key == Key.F4)
                        {
                           controlador.prueba();
                        }
                    }
                }
            }
            }

        public double getProgesoTorrent(string hash)
        {
            return controlador.getProgresoTorrent(hash);
        }

        public void agregarInterfazAñadir()
        {
            //Se saca la posicion del icono agregar
            int posicion = IntSeries.Count;

            //Se calcula en que pagina, fila y columna estará colocado
            int pag = posicion / 9;
            int fil = (posicion % 9) / 3;
            int col = (posicion % 9) % 3;

            //Se crea la imagen del ícono
            IconoAgregar = new Image()
            {
                Height = 120,
                Width = 120,
                Source = new BitmapImage(new Uri
                         (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Plus.png")),
                Stretch = Stretch.None,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            };

            //Se coloca en la fila y columna correspondiente
            Grid.SetRow(IconoAgregar, fil);
            Grid.SetColumn(IconoAgregar, col);


            //Se le agrega su evento de click
            IconoAgregar.MouseUp += IconoAgregar_MouseUp;

            //Se crea el rectangulo tapa que es el que cubrira la interfaz cuando esté activo el agregar
            Tapa = new Rectangle()
            {
                Width = 1066,
                Height = 599,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Fill = new SolidColorBrush(Colors.Black),
                Opacity = 0,
                Visibility = Visibility.Hidden,
            };

            //Se pone su columnspan y rowspan en 3 para que pueda cubrir todo
            Grid.SetColumnSpan(Tapa, 3);
            Grid.SetRowSpan(Tapa, 3);
           


            //Se crea la interfaz agregar
            InterfazAgregar = new Agregar(this);
            InterfazAgregar.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            InterfazAgregar.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Grid.SetColumnSpan(InterfazAgregar, 3);
            Grid.SetRowSpan(InterfazAgregar, 3);

            //Si el icono agregar va en una nueva página, se crea
            if (posicion % 9 == 0 && posicion != 0)
                CrearPag(pag);

            //Se añade a la interfaz la tapa, la interfaz y el ícono
            Pags[pag].Children.Add(Tapa);
            Pags[pag].Children.Add(InterfazAgregar);
            Pags[pag].Children.Add(IconoAgregar);





        }

        public void reacomodarInterfazAñadir(int posicion, bool agregar)
        {

            //Se calcula en que pagina, fila y columna estará colocado
            int pag = posicion / 9;
            int fil = (posicion % 9) / 3;
            int col = (posicion % 9) % 3;

            //Se coloca en la fila y columna correspondiente
            Grid.SetRow(IconoAgregar, fil);
            Grid.SetColumn(IconoAgregar, col);

            //Si el icono agregar va en una nueva página, se crea
            if (agregar)
            {
                if (posicion % 9 == 0 && posicion != 0)
                {
                    CrearPag(pag);

                    Pags[pag - 1].Children.Remove(Tapa);
                    Pags[pag - 1].Children.Remove(InterfazAgregar);
                    Pags[pag - 1].Children.Remove(IconoAgregar);

                }
                else
                {
                    Pags[pag].Children.Remove(Tapa);
                    Pags[pag].Children.Remove(InterfazAgregar);
                    Pags[pag].Children.Remove(IconoAgregar);
                }

                Pags[pag].Children.Add(Tapa);
                Pags[pag].Children.Add(InterfazAgregar);
                Pags[pag].Children.Add(IconoAgregar);

                if (PagAct < Pags.Count - 1) IconoSiguiente.Visibility = System.Windows.Visibility.Visible;

            }
            else
            {
                if ((posicion + 1) % 9 == 0 && posicion != 0)
                {
                    Pags[pag + 1].Children.Remove(IconoAgregar);
                    Pags[pag].Children.Add(IconoAgregar);
                    Pags[pag + 1].Children.Remove(Tapa);
                    Pags[pag].Children.Add(Tapa);
                    Pags[pag + 1].Children.Remove(InterfazAgregar);
                    Pags[pag].Children.Add(InterfazAgregar);

                    //Se elimina la pagina 
                    gPrinc.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(gPrinc.Margin.Left, 0, gPrinc.Margin.Right + 1058, 0), new TimeSpan(0, 0, 0, 0, 1)));
                    gPrinc.Children.Remove(Pags[Pags.Count - 1]);
                    Pags.RemoveAt(Pags.Count - 1);

                    if (PagAct == Pags.Count - 1) IconoSiguiente.Visibility = System.Windows.Visibility.Hidden;
                }
            }



            IconoAgregar.Visibility = Visibility.Visible;
            IconoAgregar.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(250)));



        }

        public void agregarSigAnt()
        {
            //Se crea la imagen del icono siguiente
            IconoSiguiente = new Image()
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

            //Se crea la imagen del icono anterior
            IconoAnterior = new Image()
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

            //Se le añaden sus eventos al icono siguiente
            IconoSiguiente.MouseUp += Sig_MouseUp;
            IconoSiguiente.MouseEnter += SigAnt_MouseOver;
            IconoSiguiente.MouseLeave += SigAnt_MouseLeave;

            //Se le añaden sus eventos al icono anterior
            IconoAnterior.MouseUp += Ant_MouseUp;            
            IconoAnterior.MouseEnter += SigAnt_MouseOver;            
            IconoAnterior.MouseLeave += SigAnt_MouseLeave;

            //Se añaden a la interfaz
            Pags[0].Children.Add(IconoSiguiente);
            Pags[0].Children.Add(IconoAnterior);

            //Se ponen en la columna y fila correspondientes
            Grid.SetRow(IconoSiguiente, 1);
            Grid.SetColumn(IconoSiguiente, 2);
            Grid.SetRow(IconoAnterior, 1);
            Grid.SetColumn(IconoAnterior, 0);

            //Si hay mas de una página el siguiente se muestra
            if (Pags.Count > 1) IconoSiguiente.Visibility = System.Windows.Visibility.Visible;

        }

        public void subirLona(IntSerie s, bool primeraVez)
        {
            s.subirLona(primeraVez);
        }

        public void agregarDesgloseEpisodios(IntSerie s, bool escondido)
        {
            s.agregarDesgloseEpisodios(escondido);
        }

        public IntSerie getInterfazSerie(int num)
        {
            return IntSeries[num];
        }

        public void desplegarEpisodios(IntSerie s)
        {
            controlador.desplegarEpisodios(s);
        }

        public void cambiarEpPrincipal(Episodio ep)
        {
            controlador.cambiarEpPrincipal(ep);
        }

        public int getCantSeries()
        {
            return controlador.Series.Count;
        }

        public Controlador getControlador()
        {
            return controlador;
        }

    }
}
