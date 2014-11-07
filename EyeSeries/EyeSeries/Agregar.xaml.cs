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
        List<BackgroundWorker> update;
        List<int> claves;
        List<string> banner;
        Serie s;
        MainWindow Princ;
        public Agregar(MainWindow p)
        {
            InitializeComponent();
            update = new List<BackgroundWorker>();
            claves = new List<int>();
            banner = new List<string>();
            Princ = p;
               

        }

        private void Nombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            banner.Clear();
            claves.Clear();

            if (update.Count != 0)
            {
                update[update.Count - 1].CancelAsync();
            }
            BackgroundWorker nuevo = new BackgroundWorker();
            nuevo.WorkerSupportsCancellation = true;
            nuevo.DoWork += new DoWorkEventHandler(Update);
            update.Add(nuevo);
            nuevo.RunWorkerAsync(Nombre.Text);
        
                    

        }

        private void Update(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker aux = (BackgroundWorker)sender;
            string text = (string)e.Argument;
            if (aux.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            List<string> texto = new List<string>();
            if (text != "")
            {
                XmlDocument x = new XmlDocument();
                x.Load(@"http://thetvdb.com/api/GetSeries.php?seriesname=" + text);
                XmlNodeList results = x.SelectNodes("/Data/Series");
                foreach (XmlNode r in results)
                {
                    if (aux.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
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
                    Opciones.ItemsSource = texto;
                }
                ));


            }
            else
            {
                Opciones.Dispatcher.Invoke(new Action(() =>
                {
                    Opciones.ItemsSource = null;
                }
                ));
            }







            

        }

        private void Opciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Opciones.SelectedIndex != -1)
            {
                DoubleAnimation baja = new DoubleAnimation(343, new TimeSpan(0, 0, 0, 0, 250));
                DoubleAnimation anima = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250));

                anima.Completed += (sender3, e3) =>
                {
                    BackgroundWorker b = new BackgroundWorker();
                    b.DoWork += (sender2, e2) =>
                    {
                        Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        (ThreadStart)delegate
                        {
                            s = new Serie(claves[Opciones.SelectedIndex], Princ.Series.Count);
                            List<int> numeros = new List<int>();
                            for (int i = 1; i <= s.Episodios.Count; i++)
                            {
                                if (s.Episodios[i - 1][0].Fecha > DateTime.Now)
                                {
                                    break;
                                }
                                numeros.Add(i);
                            }
                            Temp.ItemsSource = numeros;
                            Temp.Visibility = System.Windows.Visibility.Visible;
                            Temp.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));

                        });

                    };
                    b.RunWorkerAsync();
                };

                baja.Completed += (sender2, e2) =>
                    {
                        Banner.Visibility = System.Windows.Visibility.Visible;
                        Temporada.Visibility = System.Windows.Visibility.Visible;
                        Ultepv.Visibility = System.Windows.Visibility.Visible;
                        Selecciona.Visibility = System.Windows.Visibility.Visible;
                        Ninguno.Visibility = System.Windows.Visibility.Visible;
                        if (banner[Opciones.SelectedIndex] != "")
                            Banner.Source = new BitmapImage(new Uri
                               (@"http://thetvdb.com/banners/" + banner[Opciones.SelectedIndex]));
                        RenderOptions.SetBitmapScalingMode(Banner, BitmapScalingMode.Fant);
                        Ultepv.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                        Selecciona.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                        Ninguno.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));
                        Temporada.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250)));                        
                        Banner.BeginAnimation(OpacityProperty, anima);
                    };

                
                
                
                

                
                Lona.BeginAnimation(HeightProperty, baja);

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
                foreach (Episodio ep in s.Episodios[Temp.SelectedIndex])
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
                    Princ.tapa.BeginAnimation(OpacityProperty, desap2);
                    Princ.Agregar.BeginAnimation(OpacityProperty, new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 125)));
                };

            desap2.Completed += (sender5, e5) =>
                {
                    Princ.tapa.Visibility = System.Windows.Visibility.Hidden;
                    Princ.Agregar.Visibility = Visibility.Visible;
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

                if (cap + 1 > s.Episodios[temp - 1].Count)
                {
                    if (temp + 1 > s.Episodios.Count)
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


                Princ.tapa.BeginAnimation(OpacityProperty, desap2);
            };

            desap2.Completed += (sender5, e5) =>
            {
                Princ.tapa.Visibility = System.Windows.Visibility.Hidden;
                Princ.AgregarNSerie(s, temp, cap);
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

        public void AnimaInicio()
        {
            
            DoubleAnimation ap = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 250));
            DoubleAnimation baja1 = new DoubleAnimation(216, new TimeSpan(0, 0, 0, 0, 250));
            ThicknessAnimation baja2 = new ThicknessAnimation(new Thickness(0), new TimeSpan(0, 0, 0, 0, 125));
            Lona.Visibility = System.Windows.Visibility.Visible;
            baja1.Completed += (sender3, e3) =>
                {
                    Nombrel.Visibility = System.Windows.Visibility.Visible;
                    Nombre.Visibility = System.Windows.Visibility.Visible;
                    Seleccional.Visibility = System.Windows.Visibility.Visible;
                    Opciones.Visibility = System.Windows.Visibility.Visible;
                    Nombrel.BeginAnimation(OpacityProperty, ap);
                    Nombre.BeginAnimation(OpacityProperty, ap);
                    Seleccional.BeginAnimation(OpacityProperty, ap);
                    Opciones.BeginAnimation(OpacityProperty, ap);
                    FocusManager.SetFocusedElement(this, Nombre); 

                };
            baja2.Completed += (sender2, e2) =>
                {
                    Lona.BeginAnimation(HeightProperty, baja1);
                };

            RectAdd.BeginAnimation(MarginProperty, baja2);
            LabAdd.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0), new TimeSpan(0,0,0,0,125)));
            Tacha.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(Tacha.Margin.Left, 6, 0,0), new TimeSpan(0, 0, 0, 0, 125)));


           


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
