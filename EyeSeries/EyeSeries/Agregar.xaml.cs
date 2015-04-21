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
using System.Xml;
using TVDBSharp;

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

            Princ.Tapa.MouseUp += Tacha_MouseUp;

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
                //Se declara la anmacion para bajar la lona
                DoubleAnimation baja = new DoubleAnimation()
                {
                    To = 343,
                    Duration = TimeSpan.FromMilliseconds(250),

                };

                //Se declara la animacion para que aparezcan las siguientes opciones (temporada, etc)
                DoubleAnimation anima = new DoubleAnimation()
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(250),
                    BeginTime = TimeSpan.FromMilliseconds(250),
                };

                //Se declara el worker para crear la serie
                BackgroundWorker hacerSerie = new BackgroundWorker();

                //El worker lo que hace es crear la serie y alimentar los numeros de temporada
                hacerSerie.DoWork += (sender4, e4) =>
                    {
                        Opciones.Dispatcher.Invoke(new Action(() =>
                        {
                            if (s != null)
                            {
                                File.Delete(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\FanArt\" + s.Nombre + ".jpg");                                                               
                            }

                            //Se crea la serie seleccionada
                            s = new Serie(claves[Opciones.SelectedIndex], Princ.getCantSeries(), Princ.getControlador());
                        }
                        ));   
                        
                        List<int> numeros = new List<int>();

                        //Se poblan los numeros dependiendo de las temporadas que haya
                        for (int i = 1; i <= s.EpisodiosVistos.Count; i++)
                        {
                            if (s.EpisodiosVistos[i - 1][0].Fecha > DateTime.Now)
                            {
                                break;
                            }
                            numeros.Add(i);
                        }

                        //Se añaden los numeros al combobox
                        Temp.Dispatcher.Invoke(new Action(() =>
                        {
                            Temp.ItemsSource = numeros;
                        }
                        ));                       
                    };


                hacerSerie.RunWorkerCompleted += (sender4, e4) =>
                {
                    //Cuando se completa el worker, se hace visible la temporada
                    Temp.Visibility = System.Windows.Visibility.Visible;
                    Temp.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                };


                anima.Completed += (sender3, e3) =>
                {
                    hacerSerie.RunWorkerAsync();
                };

                baja.Completed += (sender2, e2) =>
                    {  
                        //Se pone la imagen del banner cuando baja la lona
                        if (banner[Opciones.SelectedIndex] != "")
                            Banner.Source = new BitmapImage(new Uri
                               (@"http://thetvdb.com/banners/" + banner[Opciones.SelectedIndex]));

                        //Aparece la imagen
                        Banner.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(250)));                                    
                    };


                //Se hace visible la etapa dos
                Etapa2.Visibility = System.Windows.Visibility.Visible;

                //Se echan a andar ambas animacionaes
                Lona.BeginAnimation(HeightProperty, baja);
                Etapa2.BeginAnimation(OpacityProperty, anima);
        


            }
            else
            {

                DoubleAnimation desaparece = new DoubleAnimation()
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(250),
                };

                DoubleAnimation baja = new DoubleAnimation()
                {
                    To = 216,
                    Duration = TimeSpan.FromMilliseconds(250),
                    BeginTime = TimeSpan.FromMilliseconds(250),
                };

                DoubleAnimation cambiarValora0 = new DoubleAnimation()
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(1),
                };

                

                desaparece.Completed += (sender2, e2) =>
                    {
                        
                        Etapa2.Visibility = System.Windows.Visibility.Hidden;
                        Etapa3.Visibility = System.Windows.Visibility.Hidden;

                        resetearCampos();


                    };


                Etapa2.BeginAnimation(OpacityProperty, desaparece);
                Etapa3.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
                Lona.BeginAnimation(HeightProperty, baja);
                

            }
        }

        private void Temp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Temp.SelectedIndex != -1)
            {
                List<int> caps = new List<int>();

                DoubleAnimation aparece = new DoubleAnimation()
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(250),
                };


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
                Cap.BeginAnimation(OpacityProperty, aparece);
                Capitulo.BeginAnimation(OpacityProperty, aparece);
            }
        }

        private void Cap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Cap.SelectedIndex != -1)
            {

                DoubleAnimation baja = new DoubleAnimation()
                {
                    To = 378,
                    Duration = TimeSpan.FromMilliseconds(250),
                };

                DoubleAnimation aparece = new DoubleAnimation()
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(250),
                    BeginTime = TimeSpan.FromMilliseconds(250),
                };
                
                Etapa3.Visibility = Visibility.Visible;

                Lona.BeginAnimation(HeightProperty, baja);
                Etapa3.BeginAnimation(OpacityProperty, aparece);
            }

            
        }

        private void Tacha_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation desapareceElementos = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                
            };

            DoubleAnimation subeLona = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                BeginTime = TimeSpan.FromMilliseconds(250),
            };

            ThicknessAnimation subeHeader = new ThicknessAnimation()
            {
                To = new Thickness(0, -34, 0, 0),
                Duration = TimeSpan.FromMilliseconds(125),
                BeginTime = TimeSpan.FromMilliseconds(500),
            };

            DoubleAnimation desapareceTapa = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(125),
                BeginTime = TimeSpan.FromMilliseconds(750),
            };

            DoubleAnimation apareceIconoAgregar = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(125),
                BeginTime = TimeSpan.FromMilliseconds(750),
            };


            desapareceTapa.Completed += (sender5, e5) =>
                {
                    Etapa1.Visibility = Visibility.Hidden;
                    Etapa2.Visibility = Visibility.Hidden;
                    Etapa3.Visibility = Visibility.Hidden;

                    Lona.Visibility = System.Windows.Visibility.Hidden;

                    Princ.Tapa.Visibility = System.Windows.Visibility.Hidden;
                    Princ.IconoAgregar.Visibility = Visibility.Visible;

                    if (s != null)
                    {
                        File.Delete(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\FanArt\" + s.Nombre + ".jpg");                                                              
                    }

                };

            Etapa1.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Etapa2.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Etapa3.BeginAnimation(OpacityProperty, desapareceElementos);

            Lona.BeginAnimation(HeightProperty, subeLona);

            Header.BeginAnimation(MarginProperty, subeHeader);

            Princ.Tapa.BeginAnimation(OpacityProperty, desapareceTapa);
            Princ.IconoAgregar.BeginAnimation(OpacityProperty, apareceIconoAgregar);

            resetearCampos();




        }

        private void Aceptar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int temp = 1;
            int cap = 1;
            if ((bool)Selecciona.IsChecked)
            {
                temp = (int)Temp.SelectedItem;
                cap = (int)Cap.SelectedItem;

                s.TemporadaUltEpisodioVisto = temp;
                s.CapituloUltEpisodioVisto = cap;

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


            DoubleAnimation desapareceElementos = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),

            };

            DoubleAnimation subeLona = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                BeginTime = TimeSpan.FromMilliseconds(250),
            };

            ThicknessAnimation subeHeader = new ThicknessAnimation()
            {
                To = new Thickness(0, -34, 0, 0),
                Duration = TimeSpan.FromMilliseconds(125),
                BeginTime = TimeSpan.FromMilliseconds(500),
            };

            DoubleAnimation desapareceTapa = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(125),
                BeginTime = TimeSpan.FromMilliseconds(750),
            };

            DoubleAnimation apareceIconoAgregar = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(125),
                BeginTime = TimeSpan.FromMilliseconds(750),
            };


            desapareceTapa.Completed += (sender5, e5) =>
            {
                Etapa1.Visibility = Visibility.Hidden;
                Etapa2.Visibility = Visibility.Hidden;
                Etapa3.Visibility = Visibility.Hidden;

                Lona.Visibility = System.Windows.Visibility.Hidden;

                Princ.Tapa.Visibility = System.Windows.Visibility.Hidden;

                resetearCampos();

                Princ.agregarNuevaSerie(s, temp, cap);

                s = null;
            };


            Etapa1.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Etapa2.BeginAnimation(OpacityProperty, new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 250)));
            Etapa3.BeginAnimation(OpacityProperty, desapareceElementos);

            Lona.BeginAnimation(HeightProperty, subeLona);

            Header.BeginAnimation(MarginProperty, subeHeader);

            Princ.Tapa.BeginAnimation(OpacityProperty, desapareceTapa);

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

            DoubleAnimation desaoareceEtapa3 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
            };
            DoubleAnimation subeLona = new DoubleAnimation()
            {
                To = 343,
                Duration = TimeSpan.FromMilliseconds(250),
                BeginTime = TimeSpan.FromMilliseconds(250),
            };

            desaoareceEtapa3.Completed += (sender2, e2) =>
                {
                    Etapa3.Visibility = Visibility.Hidden;                        
                };

            Etapa3.BeginAnimation(OpacityProperty, desaoareceEtapa3);
            Lona.BeginAnimation(HeightProperty, subeLona);
            
        }

        private void Ninguno_Click(object sender, RoutedEventArgs e)
        {
            Temp.IsEnabled = false;
            Cap.IsEnabled = false;
            Cap.SelectedIndex = -1;
            Temp.SelectedIndex = -1;
            Etapa3.Visibility = Visibility.Visible;

            DoubleAnimation baja = new DoubleAnimation()
            {
                To = 378,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            DoubleAnimation aparece = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(250),
                BeginTime = TimeSpan.FromMilliseconds(250)
            };


            Lona.BeginAnimation(HeightProperty, baja);
            Etapa3.BeginAnimation(OpacityProperty, aparece);
            

        }

        private void resetearCampos()
        {
            DoubleAnimation cambiarValora0 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(1),
            };

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

            Temp.Visibility = System.Windows.Visibility.Hidden;
            Capitulo.Visibility = System.Windows.Visibility.Hidden;
            Cap.Visibility = System.Windows.Visibility.Hidden;

            Temp.BeginAnimation(OpacityProperty, cambiarValora0);
            Banner.BeginAnimation(OpacityProperty, cambiarValora0);
            Capitulo.BeginAnimation(OpacityProperty, cambiarValora0);
            Cap.BeginAnimation(OpacityProperty, cambiarValora0);

        }

    }
}
