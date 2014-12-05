using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using UTorrentAPI;

namespace EyeSeries
{

    public class Serie
    {
        //Atributos

        public string Nombre { get; set; } ///Nombre de la serie
        public int Id { get; set; }       //Id de la serie
        public int Numserie;             //Numero de la serie
        public char estado; //Estado de la serie
        public List<List<Episodio>> Episodios { get; set; } //Matriz de episodios de la serie
        public List<Episodio> EpisodiosNoVistos { get; set; }
        public int Subid { get; set; }
        public string NombreCap { get; set; }
        public int EstadoCap { get; set; }
        private int temporada;  //Temporada de siguiente episodio a ver / descargar
        private int capitulo;   //Capitulo de siguiente episodio a ver / descargar
        private int porVer { get; set; } //Cantidad de episodios por ver
        private int descargando;          //Cantidad de episodios por descargar
        private XmlDocument doc;        //Documento de donde se extrae la informacion de la serie
        private string Hora;
        private bool aldia;
        UTorrentClient uClient;

        //Eventos
        public event PropertyChangedEventHandler PropertyChanged;

        //GetSet
        public int Descargando
        {
            get { return descargando; }
            set
            {
                if (value != descargando)
                {
                    descargando = value;
                    OnPropertyChanged("Descargando");
                }
            }
        }

        public int PorVer
        {
            get { return porVer; }
            set
            {
                if (value != porVer)
                {
                    porVer = value;
                    OnPropertyChanged("PorVer");
                }
            }
        }

        public int Temporada
        {
            get { return temporada; }
            set
            {
                if (value != temporada)
                {
                    temporada = value;
                    OnPropertyChanged("Temporada");
                }
            }
        }

        public int Capitulo
        {
            get { return capitulo; }
            set
            {

                    capitulo = value;
                    OnPropertyChanged("Capitulo");
         
            }
        }

        public bool AlDia
        {
            get { return aldia; }
            set
            {
                if (value != aldia)
                {
                    aldia = value;
                    OnPropertyChanged("AlDia");
                }
            }
        }

        public char Estado
        {
            get { return estado; }
            set
            {
                if (value != estado)
                {
                    estado = value;
                    OnPropertyChanged("Estado");
                }
            }
        }

        /// <summary>
        /// Metodo constructor de la serie
        /// </summary>
        /// <param name="id">Id de la serie</param>

        public Serie(int id, int nums)
        {
            Numserie = nums;
            Id = id;
            uClient = new UTorrentClient(new Uri("http://127.0.0.1:8080/gui/"), "admin", "admin", 1000000);
            //Se carga el documento de donde se saca la informacion
            doc = new XmlDocument();
            doc.Load(@"http://thetvdb.com/api/97AAE7796E3F60D2/series/" + Id + "/all/en.xml");

            //Se pone su nombre y estado
            Nombre = doc.SelectSingleNode("/Data/Series/SeriesName").InnerText;
            Estado = doc.SelectSingleNode("/Data/Series/Status").InnerText == "Continuing" ? 'c' : 'e';
            Hora = doc.SelectSingleNode("/Data/Series/Airs_Time").InnerText;
            PorVer = 0;
            Descargando = 0;
            Episodios = new List<List<Episodio>>();
            BackgroundWorker b = new BackgroundWorker();
            BackgroundWorker c = new BackgroundWorker();
            b.DoWork += (sender, e) =>
                {
                    WebClient wc = new WebClient();
                    wc.DownloadFile("http://thetvdb.com/banners/" + doc.SelectSingleNode("/Data/Series/fanart").InnerText, @"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\FanArt\" + Nombre + ".jpg");
                };
            c.DoWork += (sender, e) =>
                {
                    WebClient getid = new WebClient();
                    string id2 = getid.DownloadString(@"http://www.addic7ed.com/search.php?search="+ Nombre +"&Submit=Search");
                    Regex pag = new Regex(@"Are you looking for  <a href=""(.+)""");
                    Match m1 = pag.Match(id2);
                    Regex num = new Regex(@"/show/(.+)");
                    Subid = Convert.ToInt32(num.Match(m1.Groups[1].Value).Groups[1].Value);
                };
            b.RunWorkerAsync();
            c.RunWorkerAsync();
            AddEpisodes();
        }
        /// <summary>
        /// Constructor de la serie
        /// </summary>
        /// <param name="i">Id de la serie</param>
        /// <param name="n">Nombre de la serie</param>
        /// <param name="ns">Numero de la serie</param>
        /// <param name="e">Estado de la serie</param>
        public Serie(int i, string n,/* int t, int c,*/ int ns, char e, string h, int s)
        {
            Id = i;
            Nombre = n;
            Numserie = ns;
            Estado = e;
            Descargando = 0;
            PorVer = 0;
            //Temporada = t;
            //Capitulo = c;
            Hora = h;
            Subid = s;
            Episodios = new List<List<Episodio>>();
            EpisodiosNoVistos = new List<Episodio>();
            AlDia = false;
            uClient = new UTorrentClient(new Uri("http://127.0.0.1:8080/gui/"), "admin", "admin", 1000000);
        }

        /// <summary>
        /// Metodo utilizado para agregar episodios nuevos
        /// </summary>
        private void AddEpisodes()
        {
            //Se declaran las variables
            XmlNodeList episodios = doc.SelectNodes("/Data/Episode");
            string nombreEp, calidad;
            int tempo, capi, estado;
            DateTime Fecha;

            //Se ponen los parametros
            calidad = "720p";
            List<Episodio> aux = new List<Episodio>();
            int tempAct = 1;

            //Se ingresan los episodios
            foreach (XmlNode e in episodios)
            {
                if (e.SelectSingleNode("SeasonNumber").InnerText != "" && e.SelectSingleNode("SeasonNumber").InnerText != "0" && e.SelectSingleNode("FirstAired").InnerText != "")
                {
                    if (tempAct < Convert.ToInt32(e.SelectSingleNode("SeasonNumber").InnerText))
                    {
                        Episodios.Add(aux);
                        aux = new List<Episodio>();
                        tempAct++;
                    }

                    nombreEp = e.SelectSingleNode("EpisodeName").InnerText;
                    tempo = Convert.ToInt32(e.SelectSingleNode("SeasonNumber").InnerText);
                    capi = Convert.ToInt32(e.SelectSingleNode("EpisodeNumber").InnerText);
                    string fecha2 = e.SelectSingleNode("FirstAired").InnerText;

                    int hora = 0, minutos = 0;
                    if (Hora == "")
                    {
                        hora = 12;
                        minutos = 0;
                    }
                    else
                        if (Hora.Length == 5)
                        {
                            hora = Convert.ToInt32(Hora.Split(':')[0]);
                            minutos = Convert.ToInt32(Hora.Split(':')[1]);
                        }
                        else
                        {
                            hora = Convert.ToInt32(Hora.Split(':')[0]);
                            minutos = Convert.ToInt32(Hora.Split(':')[1].Split(' ')[0]);
                            hora = Hora.Split(':')[1].Split(' ')[1] == "PM" ? hora += 12 : hora;
                        }
                    Fecha = new DateTime(Convert.ToInt32(fecha2.Substring(0, 4)), Convert.ToInt32(fecha2.Substring(5, 2)), Convert.ToInt32(fecha2.Substring(8, 2)), hora, minutos, 0);
                    Fecha = Fecha.AddDays(0.5);
                    estado = 3;
                    aux.Add(new Episodio(this, tempo, capi, nombreEp, "-1", Fecha, estado, calidad));

                }


            }

            Episodios.Add(aux);

        }

        /// <summary>
        /// Metodo que sirve para agregar una serie una vez seleccionada
        /// </summary>
        /// <param name="t">Temporada del siguiente capitulo a descargar</param>
        /// <param name="c">Capitulo del siguiente capitulo a descargar</param>
        public void addSerie(int t, int c)
        {
            Temporada = t;
            Capitulo = c;
            if ((Temporada == 0 && Capitulo == 0) || Episodios[Temporada - 1][Capitulo - 1].Estado == 0)
            {
                AlDia = true;
            }
            else
            {
                AlDia = false;
            }

        }

        public void Descarga()
        {
            if (!AlDia)
            {
                for (int i = Temporada - 1; i < Episodios.Count; i++)
                {
                    int j = (i == Temporada - 1 ? capitulo - 1 : 0);
                    while (j < Episodios[i].Count)
                    {

                        if (Episodios[i][j].Fecha < DateTime.Now)
                        {
                            Episodios[i][j].getMagnet();
                            Episodios[i][j].Estado = 1;
                            Descargando++;
                        }
                        else
                        {
                            Episodios[i][j].Estado = 0;
                            if ((Episodios[i][j].Fecha - DateTime.Now).TotalDays < 24)
                            {
                                System.Timers.Timer act = new System.Timers.Timer()
                                {
                                    Interval = (Episodios[i][j].Fecha - DateTime.Now).TotalMilliseconds,


                                };
                                act.Elapsed += (sender, e) => EpAlAire(sender, e, Episodios[i][j]);
                                act.Start();

                            }
                        }
                        j++;

                    }
                }
                if (!System.IO.Directory.Exists(@"C:\Users\Marcelo\Videos\Series\" + Nombre))
                {
                    System.IO.Directory.CreateDirectory(@"C:\Users\Marcelo\Videos\Series\" + Nombre);
                }
            }
            CrearArchivo();
        }
        

        /// <summary>
        /// Alimenta los episodios de la base de datos a la serie
        /// </summary>
        public void AlimentarEps(int t, int c)
        {
            StreamReader leer = new StreamReader(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\" + Nombre + ".txt");
            List<Episodio> aux = new List<Episodio>();
            string linea;
            while (!leer.EndOfStream)
            {
                linea = leer.ReadLine();
                if (linea == "-")
                {
                    Episodios.Add(aux);
                    aux = new List<Episodio>();
                }
                else
                {
                    DateTime fecha = new DateTime(Convert.ToInt32(linea.Split('*')[4].Split('/')[0]), Convert.ToInt32(linea.Split('*')[4].Split('/')[1]), Convert.ToInt32(linea.Split('*')[4].Split('/')[2]), Convert.ToInt32(linea.Split('*')[4].Split('/')[3]), Convert.ToInt32(linea.Split('*')[4].Split('/')[4]), 0);
                    Episodio ep = new Episodio(this, Convert.ToInt32(linea.Split('*')[0]), Convert.ToInt32(linea.Split('*')[1]), linea.Split('*')[2], linea.Split('*')[3], fecha, Convert.ToInt32(linea.Split('*')[5]), "720p");

                    if (ep.Estado == 0)
                    {
                        if (ep.Fecha < DateTime.Now)
                        {
                            ep.Estado = 1;
                            ep.getMagnet();
                            Descargando++;
                            //CrearArchivo();
                        }
                        else
                        {
                            if ((ep.Fecha - DateTime.Now).TotalDays < 24)
                            {
                                System.Timers.Timer act = new System.Timers.Timer()
                                {
                                    Interval = (ep.Fecha - DateTime.Now).TotalMilliseconds,
                                   
                                    
                                };
                                act.Elapsed += (sender, e) => EpAlAire(sender, e, ep);
                                act.Start();
                                
                            }
                           
                        }
                    }
                    else
                    {
                        if (ep.Estado == 1)
                        {

                            if (uClient.Torrents[ep.Hash].RemainingBytes == 0 && uClient.Torrents[ep.Hash].DownloadedBytes > 0 || uClient.Torrents[ep.Hash].ProgressInMils == 1000)
                            {
                                ep.Mover(uClient, true);
                                PorVer++;
                            }
                            else
                            {
                                Descargando++;
                            }
                        }
                        else
                        {
                            if (ep.Estado == 2) PorVer++;
                        }
                    }
                    aux.Add(ep);
                }

            }

            Temporada = t;
            Capitulo = c;

            if ((Temporada == 0 && Capitulo == 0) || Episodios[Temporada - 1][Capitulo - 1].Estado == 0)
            {
                AlDia = true;
            }
            else
            {
                AlDia = false;
            }


            leer.Close();

            CrearArchivo();

        }

        public void AlimentarEpisodiosNoVistos()
        {
            string PathArchivoLeer = @"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\EpisodiosNoVistos\" + Nombre + ".txt";
            StreamReader leer = new StreamReader(PathArchivoLeer);
            string linea;
            while (!leer.EndOfStream)
            {
                linea = leer.ReadLine();
                DateTime fecha = new DateTime(Convert.ToInt32(linea.Split('*')[4].Split('/')[0]), Convert.ToInt32(linea.Split('*')[4].Split('/')[1]), Convert.ToInt32(linea.Split('*')[4].Split('/')[2]), Convert.ToInt32(linea.Split('*')[4].Split('/')[3]), Convert.ToInt32(linea.Split('*')[4].Split('/')[4]), 0);
                Episodio ep = new Episodio(this, Convert.ToInt32(linea.Split('*')[0]), Convert.ToInt32(linea.Split('*')[1]), linea.Split('*')[2], linea.Split('*')[3], fecha, Convert.ToInt32(linea.Split('*')[5]), "720p");
                EpisodiosNoVistos.Add(ep);
               
            }

            leer.Close();

        }

        /// <summary>
        /// Metodo que sirve para crear la base de datos de cada serie
        /// </summary>

        public void CrearArchivo()
        {
            string se = "";
            StreamWriter escribe = new StreamWriter(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\" + Nombre + ".txt");
            foreach (List<Episodio> lista in Episodios)
            {
                foreach (Episodio ep in lista)
                {
                    se += ep.Imprimir() + "\r\n";
                }
                se += "-\r\n";
                escribe.Write(se);
                se = "";
            }

            escribe.Close();
           
        }


        /// <summary>
        /// Metodo que sirve para borrar un archivo de la serie
        /// </summary>
        public void BorrarArchivo()
        {
            System.IO.File.Delete(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\" + Nombre + ".txt");
        }


        /// <summary>
        /// Metodo que sirve para imprimir la informacion general de la serie
        /// </summary>
        /// <returns>El string con toda la informacion de la serie</returns>
        public string Imprimir()
        {
            return Nombre + "\r\n" + Id + "\r\n" + Subid + "\r\n" + Temporada + " " + Capitulo + "\r\n" + Estado + "\r\n" + Hora + "\r\n-";

        }

        /// <summary>
        /// Metodo utilizado para pasar al siguiente episodio de la serie
        /// </summary>
        public void SiguienteEp()
        {

            if (Capitulo + 1 > Episodios[Temporada - 1].Count)
            {
                if (Temporada + 1 > Episodios.Count)
                {
                    Temporada = 0;
                    Capitulo = 0;
                }
                else
                {
                    Temporada++;
                    Capitulo = 1;
                }
            }
            else
            {
                Capitulo++;
            }

        }

        /// <summary>
        /// Revisa si el capitulo existe  
        /// </summary>
        /// <param name="temp">Temporada que se quiere revisar</param>
        /// <param name="cap">Capitulo que se quiere revisar</param>
        /// <returns></returns>
        public bool Existe(int temp, int cap)
        {
            if (Episodios.Count > temp)
                return true;
            else
                if (Episodios.Count == temp)
                    if (Episodios[temp - 1].Count >= cap)
                        return true;
                    else
                        return false;
                else
                    return false;
        }

        public void ActualizarBD(IntSerie interf)
        {
            //Se carga el documento de donde se saca la informacion
            doc = new XmlDocument();
            doc.Load(@"http://thetvdb.com/api/97AAE7796E3F60D2/series/" + Id + "/all/en.xml");
            Estado = doc.SelectSingleNode("/Data/Series/Status").InnerText == "Continuing" ? 'c' : 'e';
            XmlNodeList episodios = doc.SelectNodes("/Data/Episode");
            DateTime fecha;
            int tempo, epi, estado;
            string nombreEp;
            bool agregado = Temporada == 0 && Capitulo == 0? true : false;
            foreach (XmlNode e in episodios)
            {
                if (e.SelectSingleNode("SeasonNumber").InnerText != "" && e.SelectSingleNode("SeasonNumber").InnerText != "0" && e.SelectSingleNode("FirstAired").InnerText != "")
                {
                    string fecha2 = e.SelectSingleNode("FirstAired").InnerText;
                    int hora = Convert.ToInt32(Hora.Split(':')[0]);
                    int minutos = Convert.ToInt32(Hora.Split(':')[1].Split(' ')[0]);
                    hora = Hora.Split(':')[1].Split(' ')[1] == "PM" ? hora += 12 : hora;
                    fecha = new DateTime(Convert.ToInt32(fecha2.Substring(0, 4)), Convert.ToInt32(fecha2.Substring(5, 2)), Convert.ToInt32(fecha2.Substring(8, 2)), hora, minutos, 0);
                    fecha = fecha.AddDays(0.5);
                    nombreEp = e.SelectSingleNode("EpisodeName").InnerText;
                    tempo = Convert.ToInt32(e.SelectSingleNode("SeasonNumber").InnerText);
                    epi = Convert.ToInt32(e.SelectSingleNode("EpisodeNumber").InnerText);
                    if (fecha > DateTime.Now || !Existe(tempo, epi))
                    {
                        if (Existe(tempo, epi))
                        {
                            Episodios[tempo - 1][epi - 1].NombreEp = nombreEp;
                            Episodios[tempo - 1][epi - 1].Fecha = fecha;

                        }
                        else
                        {
                            bool nuevatemp = false;
                            if (tempo > Episodios.Count)
                            {
                                Episodios.Add(new List<Episodio>());
                                nuevatemp = true;
                            }
                            if (fecha < DateTime.Now) estado = 1;
                            else estado = 0;
                            Episodio aux = new Episodio(this, tempo, epi, nombreEp, "-1", fecha, estado, "720p");
                            if (aux.Estado == 1)
                            {
                                aux.getMagnet();
                                Descargando++;
                                AlDia = false;
                            }
                            Episodios[tempo - 1].Add(aux);
                            if (agregado)
                            {
                                Temporada = aux.Temporada;
                                Capitulo = aux.Capitulo;
                                agregado = false;
                            }
                            Application.Current.Dispatcher.Invoke(
                            DispatcherPriority.Normal,
                            (ThreadStart)delegate
                            {
                                interf.AgregarEp(aux, nuevatemp);
                            });
                            

                        }
                    }
                }


            }

            


            CrearArchivo();


        }

        private void EpAlAire(object sender, EventArgs e, Episodio ep)
        {
            System.Timers.Timer d = (System.Timers.Timer)sender;
            Application.Current.Dispatcher.Invoke(
               DispatcherPriority.Normal,
               (ThreadStart)delegate
               {
                   
                    ep.getMagnet();
                    ep.Estado = 1;
                    Descargando++;
                    CrearArchivo();
               });
            
            d.Stop();

        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


    }
}
