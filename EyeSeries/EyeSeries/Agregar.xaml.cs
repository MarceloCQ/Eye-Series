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
using System.Xml;

namespace EyeSeries
{
    /// <summary>
    /// Interaction logic for Agregar.xaml
    /// </summary>
    public partial class Agregar : UserControl
    {
        List<BackgroundWorker> bwJalando;
        System.Timers.Timer terminodeEscribir;

        List<int> claves;
        List<string> banner;

        Serie s;
        MainWindow Princ;


        public Agregar(MainWindow p)
        {
            InitializeComponent();

            bwJalando = new List<BackgroundWorker>();

            claves = new List<int>();
            banner = new List<string>();

            Princ = p;

            terminodeEscribir = new System.Timers.Timer()
            {
                Interval = 400,
            };

            terminodeEscribir.Elapsed += buscarQuery;
               

        }

        private void Nombre_TextChanged(object sender, TextChangedEventArgs e)
        {


            if (bwJalando.Count != 0)
            {
                bwJalando[bwJalando.Count - 1].CancelAsync();
            }

            banner.Clear();
            claves.Clear();

            terminodeEscribir.Stop();

            if (Nombre.Text != "")
            {
                terminodeEscribir.Start();
            }
            else
            {
                Opciones.ItemsSource = null;
            }

        }

        private void buscarQuery(object sender, EventArgs e)
        {
         //   MessageBox.Show("hola");
            terminodeEscribir.Stop();

            BackgroundWorker nuevo = new BackgroundWorker();
            nuevo.WorkerSupportsCancellation = true;
            nuevo.DoWork += new DoWorkEventHandler(Update);
            bwJalando.Add(nuevo);
            nuevo.RunWorkerAsync();
        }

        private void Update(object sender, DoWorkEventArgs e)
        {
            string text = "";
            Nombre.Dispatcher.Invoke(new Action(() =>
                {
                    text = Nombre.Text;
                }
            ));
            BackgroundWorker aux = (BackgroundWorker)sender;
            List<string> texto = new List<string>();

            if (text != "")
            {

                XmlDocument x = new XmlDocument();
                x.Load(@"http://thetvdb.com/api/GetSeries.php?seriesname=" + text);

                XmlNodeList results = x.SelectNodes("/Data/Series");

                if (aux.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                foreach (XmlNode r in results)
                {

                    texto.Add(r.SelectSingleNode("SeriesName").InnerText);
                    claves.Add(Convert.ToInt32(r.SelectSingleNode("seriesid").InnerText));
                    XmlNode l = r.SelectSingleNode("banner");
                    if (l != null)
                        banner.Add(l.InnerText);
                    else
                        banner.Add("");
                }

                Opciones.Dispatcher.Invoke(new Action(() =>
                {
                    if (aux.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    Opciones.ItemsSource = texto;
                }
                ));
            }

        }

        private void Opciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Opciones.SelectedIndex != -1)
            {
                DoubleAnimation baja = new DoubleAnimation()
                {
                    To = 343,
                    Duration = TimeSpan.FromMilliseconds(250),

                };
                DoubleAnimation anima = new DoubleAnimation()
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(250),
                    BeginTime = TimeSpan.FromMilliseconds(250),
                };




                BackgroundWorker hacerSerie = new BackgroundWorker();
                hacerSerie.DoWork += (sender4, e4) =>
                    {
                        Opciones.Dispatcher.Invoke(new Action(() =>
                        {
                            s = new Serie(claves[Opciones.SelectedIndex], Princ.getCantSeries());
                        }
                        ));   
                        
                        List<int> numeros = new List<int>();
                        for (int i = 1; i <= s.EpisodiosVistos.Count; i++)
                        {
                            if (s.EpisodiosVistos[i - 1][0].Fecha > DateTime.Now)
                            {
                                break;
                            }
                            numeros.Add(i);
                        }

                        Temp.Dispatcher.Invoke(new Action(() =>
                        {
                            Temp.ItemsSource = numeros;
                        }
                        ));                       
                    };

                hacerSerie.RunWorkerCompleted += (sender4, e4) =>
                {
                    Temp.Visibility = System.Windows.Visibility.Visible;
                    Temp.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                };

                

                Etapa2.Visibility = System.Windows.Visibility.Visible;

                anima.Completed += (sender3, e3) =>
                {
                    hacerSerie.RunWorkerAsync();
                };

                baja.Completed += (sender2, e2) =>
                    {                        
                        if (banner[Opciones.SelectedIndex] != "")
                            Banner.Source = new BitmapImage(new Uri
                               (@"http://thetvdb.com/banners/" + banner[Opciones.SelectedIndex]));

                        Banner.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(250)));

                        RenderOptions.SetBitmapScalingMode(Banner, BitmapScalingMode.Fant);                                          
                    };

                Lona.BeginAnimation(HeightProperty, baja);
                Etapa2.BeginAnimation(OpacityProperty, anima);
        


            }
            else
            {
                Banner.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
                Temp.ItemsSource = null;
                DoubleAnimation desap = new DoubleAnimation(0, new TimeSpan(0,0,0,0,250));
                DoubleAnimation baja1 = new DoubleAnimation(216, new TimeSpan(0, 0, 0, 0, 250));
                desap.Completed += (sender2, e2) =>
                    {
                        Lona.BeginAnimation(HeightProperty, baja1);
                        Temp.SelectedIndex = -1;
                        Cap.SelectedIndex = -1;
                        Ninguno.IsChecked = false;
                        Selecciona.IsChecked = true;
                        Temp.IsEnabled = true;
                        Cap.IsEnabled = true;
                        Temp.Visibility = System.Windows.Visibility.Hidden;
                        Cap.Visibility = System.Windows.Visibility.Hidden;
                        Temporada.Visibility = System.Windows.Visibility.Hidden;
                        Capitulo.Visibility = System.Windows.Visibility.Hidden;
                        Aceptar.Visibility = System.Windows.Visibility.Hidden;
                        Ultepv.Visibility = System.Windows.Visibility.Hidden;
                        Selecciona.Visibility = System.Windows.Visibility.Hidden;
                        Ninguno.Visibility = System.Windows.Visibility.Hidden;

                    };


                Temp.BeginAnimation(OpacityProperty, desap);
                Cap.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
                Temporada.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
                Capitulo.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
                Aceptar.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
                Ultepv.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
                Selecciona.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
                Ninguno.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));


                //Banner.Source = new BitmapImage();
            }
        }

        private void Temp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Temp.SelectedIndex != -1)
            {
                List<int> caps = new List<int>();
                int i = 1;
                foreach (Episodio ep in s.EpisodiosVistos[Temp.SelectedIndex])
                {
                    if (ep.Fecha > DateTime.Now)
                    {
                        break;
                    }
                    caps.Add(i);
                    i++;
                }

                Cap.ItemsSource = caps;
                Cap.Visibility = System.Windows.Visibility.Visible;
                Capitulo.Visibility = System.Windows.Visibility.Visible;
                Cap.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                Capitulo.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
            }
        }

        private void Cap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Cap.SelectedIndex != -1)
            {

                DoubleAnimation baja = new DoubleAnimation(378, new TimeSpan(0, 0, 0, 0, 250));
                baja.Completed += (sender2, e2) =>
                    {
                        Aceptar.Visibility = Visibility.Visible;
                        Aceptar.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                    };

                Lona.BeginAnimation(HeightProperty, baja);
            }

            
        }

        private void Tacha_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation desap = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            DoubleAnimation sube = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation sube2 = new ThicknessAnimation(new Thickness(0, -34, 0, 0), new TimeSpan(0,0,0,0,125));
            DoubleAnimation desap2 = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 125));

           
            desap.Completed += (sender2, e2) =>
                {
                    Nombre.Visibility = Visibility.Hidden;
                    Nombrel.Visibility = Visibility.Hidden;
                    Seleccional.Visibility = Visibility.Hidden;
                    Opciones.Visibility = Visibility.Hidden;
                    Temporada.Visibility = Visibility.Hidden;
                    Temp.Visibility = Visibility.Hidden;
                    Capitulo.Visibility = Visibility.Hidden;
                    Cap.Visibility = Visibility.Hidden;
                    Aceptar.Visibility = Visibility.Hidden;
                    Banner.Visibility = Visibility.Hidden;
                    Ultepv.Visibility = Visibility.Hidden;
                    Selecciona.Visibility = Visibility.Hidden;
                    Ninguno.Visibility = Visibility.Hidden;
                    Lona.BeginAnimation(HeightProperty, sube);
                };

            sube.Completed += (sender3, e3) =>
                {
                    Lona.Visibility = System.Windows.Visibility.Hidden;
                    RectAdd.BeginAnimation(MarginProperty, sube2);
                    LabAdd.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0, -34, 0, 0), new TimeSpan(0,0,0,0,125)));
                    Tacha.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Tacha.Margin.Left, -34, 0, 0), new TimeSpan(0, 0, 0, 0, 125)));
                };

            sube2.Completed += (sender4, e4) =>
                {
                    Princ.Tapa.BeginAnimation(OpacityProperty, desap2);
                    Princ.IconoAgregar.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 125)));
                };

            desap2.Completed += (sender5, e5) =>
                {
                    Princ.Tapa.Visibility = System.Windows.Visibility.Hidden;
                    Princ.IconoAgregar.Visibility = Visibility.Visible;
                };

            Nombre.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Nombrel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Seleccional.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Opciones.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Temporada.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Temp.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Capitulo.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Cap.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Aceptar.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Banner.BeginAnimation(OpacityProperty, desap);

            Nombre.TextChanged -= Nombre_TextChanged;
            Opciones.SelectionChanged -= Opciones_SelectionChanged;
            Nombre.Text = "";
            Temp.IsEnabled = true;
            Cap.IsEnabled = true;
            Opciones.ItemsSource = null;
            Temp.SelectedIndex = -1;
            Cap.SelectedIndex = -1;
            Selecciona.IsChecked = true;
            Ninguno.IsChecked = false;
            Nombre.TextChanged += Nombre_TextChanged;
            Opciones.SelectionChanged += Opciones_SelectionChanged;



        }

        private void Aceptar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int temp = 1;
            int cap = 1;
            if ((bool)Selecciona.IsChecked)
            {
                temp = (int)Temp.SelectedItem;
                cap = (int)Cap.SelectedItem;

                if (cap + 1 > s.EpisodiosVistos[temp - 1].Count)
                {
                    if (temp + 1 > s.EpisodiosVistos.Count)
                    {
                        temp = 0;
                        cap = 0;
                    }
                    else
                    {
                        temp++;
                        cap = 1;
                    }
                }
                else
                {
                    cap++;
                }



            }


            DoubleAnimation desap = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            DoubleAnimation sube = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation sube2 = new ThicknessAnimation(new Thickness(0, -34, 0, 0), new TimeSpan(0, 0, 0, 0, 125));
            DoubleAnimation desap2 = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 125));


            desap.Completed += (sender2, e2) =>
            {
                Nombre.Visibility = Visibility.Hidden;
                Nombrel.Visibility = Visibility.Hidden;
                Seleccional.Visibility = Visibility.Hidden;
                Opciones.Visibility = Visibility.Hidden;
                Temporada.Visibility = Visibility.Hidden;
                Temp.Visibility = Visibility.Hidden;
                Capitulo.Visibility = Visibility.Hidden;
                Cap.Visibility = Visibility.Hidden;
                Aceptar.Visibility = Visibility.Hidden;
                Banner.Visibility = Visibility.Hidden;
                Ninguno.Visibility = Visibility.Hidden;
                Selecciona.Visibility = Visibility.Hidden;
                Ultepv.Visibility = Visibility.Hidden;

                Lona.BeginAnimation(HeightProperty, sube);
            };

            sube.Completed += (sender3, e3) =>
            {
                Lona.Visibility = System.Windows.Visibility.Hidden;
                RectAdd.BeginAnimation(MarginProperty, sube2);
                LabAdd.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0, -34, 0, 0), new TimeSpan(0, 0, 0, 0, 125)));
                Tacha.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Tacha.Margin.Left, -34, 0, 0), new TimeSpan(0, 0, 0, 0, 125)));
            };

            sube2.Completed += (sender4, e4) =>
            {


                Princ.Tapa.BeginAnimation(OpacityProperty, desap2);
            };

            desap2.Completed += (sender5, e5) =>
            {
                Princ.Tapa.Visibility = System.Windows.Visibility.Hidden;
                Princ.agregarNuevaSerie(s, temp, cap);
            };

            Nombre.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Nombrel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Seleccional.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Opciones.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Temporada.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Temp.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Capitulo.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Cap.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Aceptar.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Selecciona.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Ninguno.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Ultepv.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Banner.BeginAnimation(OpacityProperty, desap);

            Nombre.TextChanged -= Nombre_TextChanged;
            Opciones.SelectionChanged -= Opciones_SelectionChanged;
            Nombre.Text = "";
            Opciones.ItemsSource = null;
            Temp.SelectedIndex = -1;
            Cap.SelectedIndex = -1;
            Nombre.TextChanged += Nombre_TextChanged;
            Opciones.SelectionChanged += Opciones_SelectionChanged;

           

            
            
        }

        public void animaEtapa1()
        {
            
            DoubleAnimation apareceEtapa1 = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250));
            DoubleAnimation bajaLona = new DoubleAnimation(216, new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation bajaHeader = new ThicknessAnimation(new Thickness(0), new TimeSpan(0, 0, 0, 0, 125));

            bajaLona.BeginTime = new TimeSpan(0, 0, 0, 0, 125);
            apareceEtapa1.BeginTime = new TimeSpan(0, 0, 0, 0, 375);

            Lona.Visibility = System.Windows.Visibility.Visible;
            Etapa1.Visibility = System.Windows.Visibility.Visible;

            FocusManager.SetFocusedElement(this, Nombre); 

            Header.BeginAnimation(MarginProperty, bajaHeader);
            Lona.BeginAnimation(HeightProperty, bajaLona);
            Etapa1.BeginAnimation(OpacityProperty, apareceEtapa1);

          
        }

        private void Selecciona_Click(object sender, RoutedEventArgs e)
        {
            Temp.IsEnabled = true;
            Cap.IsEnabled = true;


            if (Lona.Height != 0)
            {
                DoubleAnimation anima = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250));
                anima.Completed += (sender2, e2) =>
                    {
                        DoubleAnimation baja = new DoubleAnimation(343, new TimeSpan(0, 0, 0, 0, 250));
                        Lona.BeginAnimation(HeightProperty, baja);
                    };

                Aceptar.BeginAnimation(OpacityProperty, anima);
            }
            

            

            

        }

        private void Ninguno_Click(object sender, RoutedEventArgs e)
        {
            Temp.IsEnabled = false;
            Cap.IsEnabled = false;
            Cap.SelectedIndex = -1;
            Temp.SelectedIndex = -1;


            DoubleAnimation baja = new DoubleAnimation(378, new TimeSpan(0, 0, 0, 0, 250));
            baja.Completed += (sender2, e2) =>
            {
                Aceptar.Visibility = Visibility.Visible;
                Aceptar.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
            };

            Lona.BeginAnimation(HeightProperty, baja);







        }

    }
}
