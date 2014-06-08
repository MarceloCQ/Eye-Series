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
        List<Serie> Series;
        List<IntSerie> IntSeries;
        List<Grid> Pags;
        Serie s;
        Agregar Agr;
        public Image Agregar;
        public Rectangle tapa;
        DispatcherTimer empieza;



        public MainWindow()
        {
            InitializeComponent();
            

        }

        private void EyeSeries_Initialized(object sender, EventArgs e)
        {
           // StreamReader leer = new StreamReader(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\General.txt");
           // int temp = 0;
           // int capi = 0;

           // while (!leer.EndOfStream)
           // {
           //     string nombre = leer.ReadLine();
           //     int id = Convert.ToInt32(leer.ReadLine());
           //     string cap = leer.ReadLine();
           //     temp = Convert.ToInt32(cap.Split(' ')[0]);
           //     capi = Convert.ToInt32(cap.Split(' ')[1]);
           //     char estado = Convert.ToChar(leer.ReadLine());
           //     string hora = leer.ReadLine();
                
           //     s = new Serie(id, nombre, 0, estado, hora);
           //     leer.ReadLine();
           // }
            
           //// Serie s = new Serie(121361, "Game of Thrones", 0, 'c', "9:00 PM");
           //// Serie s = new Serie(121361);
           // //s.addSerie(4, 3);
           // s.AlimentarEps(temp, capi);
           // IntSerie so = new IntSerie(s);

           // BackgroundWorker b = new BackgroundWorker();
           // b.DoWork += (sp, ee) =>
           //     {
           //     //    s.Descarga();
           //     };
           // b.RunWorkerCompleted += (Epa, lala) =>
           //     {
           //         so.AgregarEps();  
           //     };
            
           // b.RunWorkerAsync();
           
           // gPrinc.Children.Add(so);
           // leer.Close();
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
            while (!leer.EndOfStream)
            {
                string nombre = leer.ReadLine();
                int id = Convert.ToInt32(leer.ReadLine());
                string cap = leer.ReadLine();
                temp = Convert.ToInt32(cap.Split(' ')[0]);
                capi = Convert.ToInt32(cap.Split(' ')[1]);
                char estado = Convert.ToChar(leer.ReadLine());
                string hora = leer.ReadLine();

                s = new Serie(id, nombre, 0, estado, hora);
                AgregarSerie(s, temp, capi, i);
                Series.Add(s);
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
            
            Pags[pag].Children.Add(tapa);
            Grid.SetColumnSpan(tapa, 3);
            Grid.SetRowSpan(tapa, 3);
            

            Pags[pag].Children.Add(Agregar);
            Grid.SetRow(Agregar, fil);
            Grid.SetColumn(Agregar, col);
          
            Agr = new Agregar(this);
            Agr.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Agr.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Pags[0].Children.Add(Agr);
            Grid.SetColumnSpan(Agr, 3);
            Grid.SetRowSpan(Agr, 3);

            empieza = new DispatcherTimer()
            {
                Interval = new TimeSpan(0,0,5),
            };
            empieza.Tick += new EventHandler(Emp);
            empieza.Start();

            
         
        }

        private void Agregar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation desap = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            desap.Completed += (sender2, e2) =>
                {
                    Agr.AnimaInicio();
                };
            tapa.Visibility = Visibility.Visible;
            tapa.BeginAnimation(OpacityProperty, new DoubleAnimation(.7, new TimeSpan(0, 0, 0, 0, 250)));
            Agregar.BeginAnimation(OpacityProperty, desap);
            

        }

        private void CrearPag(int num)
        {
            Grid aux = new Grid()
            {
                Height = 599,
                Width = 1058,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new Thickness(1066 * num, 0, 0, 0),

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
                    Margin = new Thickness(0, i == 0 ? 196 : 398, 0, 0),
                };

                aux.Children.Add(horizontal);
                horizontal.SetValue(Grid.ColumnSpanProperty, 3);
                horizontal.SetValue(Grid.RowSpanProperty, 3);
            }

            Pags.Add(aux);
            gPrinc.Children.Add(aux);   


            
        }

        private void AgregarSerie(Serie s, int temp, int capi, int num)
        {
            int pag = num / 9;
            int fil = (num % 9) / 3;
            int col = (num % 9) % 3;
            IntSerie aux;
            aux = new IntSerie(s, this);
            if (col == 1) aux.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            else if (col == 2) aux.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

            if (fil == 1) aux.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            else if (fil == 2) aux.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            
            Pags[Pags.Count - 1].Children.Add(aux);
            IntSeries.Add(aux);
            Grid.SetRow(aux, fil);
            Grid.SetColumn(aux, col);
            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (sender, e) =>
                {        
                    s.AlimentarEps(temp, capi);
                    Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (ThreadStart)delegate
                    {
                        ((IntSerie)Pags[Pags.Count - 1].Children[num % 9 + 4]).AgregarEps();
                    });
                    };
            b.RunWorkerCompleted += (sender2, e2) =>
                {
                   // ((IntSerie)Pags[Pags.Count - 1].Children[num % 9 + 4]).AgregarEps();
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
            IntSerie nueva = new IntSerie(s, this);
            Pags[pag].Children.Add(nueva);
            Grid.SetColumn(nueva, col);
            Grid.SetRow(nueva, fil);

            if (col == 1) nueva.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            else if (col == 2) nueva.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

            if (fil == 1) nueva.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            else if (fil == 2) nueva.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (sender, e) =>
                {
                    s.Descarga();
                    Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    (ThreadStart)delegate
                    {
                        nueva.AgregarEps();
                    });

                };
            b.RunWorkerAsync();
            CrearArchivo();

        }

        public void CrearArchivo()
        {
            StreamWriter escribe = new StreamWriter(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\General.txt");
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
                if (s.normal)
                    s.ZonaC_MouseLeave(null, null);
            }
            empieza.Stop();
        }

    }
}
