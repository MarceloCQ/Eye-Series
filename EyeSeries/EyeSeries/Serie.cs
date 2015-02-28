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

    public class Serie : INotifyPropertyChanged
    {
        //Atributos

        public string Nombre { get; set; } ///Nombre de la serie
        public int Id { get; set; }       //Id de la serie
        public int Numserie;             //Numero de la serie
        private char estado; //Estado de la serie
        public int Subid { get; set; }
        public bool episodiosCargados { get; set; }

        public int TemporadaUltEpisodioVisto { get; set; }
        public int CapituloUltEpisodioVisto { get; set; }


        private int temporada;  //Temporada de siguiente episodio a ver / descargar
        private int capitulo;   //Capitulo de siguiente episodio a ver / descargar
        private int porVer; //Cantidad de episodios por ver
        private int descargando;          //Cantidad de episodios por descargar
        public string Hora;
        private bool aldia;

        public List<List<Episodio>> EpisodiosVistos { get; set; } //Matriz de episodios de la serie
        public List<Episodio> EpisodiosNoVistos { get; set; }

        private Controlador controlador;

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

        public Serie(int id, int nums, Controlador contr)
        {
            controlador = contr;
            Numserie = nums;
            Id = id;           

            //Se carga el documento de donde se saca la informacion
            XmlDocument doc = new XmlDocument();
            doc.Load(@"http://thetvdb.com/api/97AAE7796E3F60D2/series/" + Id + "/all/en.xml");

            //Se pone su nombre y estado
            Nombre = doc.SelectSingleNode("/Data/Series/SeriesName").InnerText;
            Estado = doc.SelectSingleNode("/Data/Series/Status").InnerText == "Continuing" ? 'c' : 'e';
            Hora = doc.SelectSingleNode("/Data/Series/Airs_Time").InnerText;
            PorVer = 0;
            Descargando = 0;
            EpisodiosVistos = new List<List<Episodio>>();

            BackgroundWorker c = new BackgroundWorker();

            WebClient wc = new WebClient();
            wc.DownloadFileAsync(new Uri("http://thetvdb.com/banners/" + doc.SelectSingleNode("/Data/Series/fanart").InnerText), @"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\FanArt\" + Nombre + ".jpg");
            
            c.DoWork += (sender, e) =>
                {
                    WebClient getid = new WebClient();
                    string id2 = getid.DownloadString(@"http://www.addic7ed.com/search.php?search="+ Nombre +"&Submit=Search");
                    Regex pag = new Regex(@"Are you looking for  <a href=""(.+)""");
                    Match m1 = pag.Match(id2);
                    Regex num = new Regex(@"/show/(.+)");
                    Subid = Convert.ToInt32(num.Match(m1.Groups[1].Value).Groups[1].Value);
                };

            c.RunWorkerAsync();
            añadirEpisodios(doc);
        }
        /// <summary>
        /// Constructor de la serie
        /// </summary>
        /// <param name="i">Id de la serie</param>
        /// <param name="n">Nombre de la serie</param>
        /// <param name="ns">Numero de la serie</param>
        /// <param name="e">Estado de la serie</param>
        public Serie(int i, string n, int t, int c, int ns, char e, string h, int s, int tempUV, int capUV, Controlador contr)
        {
            Id = i;
            Nombre = n;
            Numserie = ns;
            Estado = e;
            Descargando = 0;
            PorVer = 0;
            Temporada = t;
            Capitulo = c;
            Hora = h;
            Subid = s;
            TemporadaUltEpisodioVisto = tempUV;
            CapituloUltEpisodioVisto = capUV;

            controlador = contr;

            EpisodiosVistos = new List<List<Episodio>>();
            EpisodiosNoVistos = new List<Episodio>();

            AlDia = false;
            //uClient = new UTorrentClient(new Uri("http://127.0.0.1:8080/gui/"), "admin", "admin", 1000000);
        }


        /// <summary>
        /// Metodo utilizado para agregar episodios nuevos
        /// </summary>
        public void añadirEpisodios(XmlDocument doc)
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
                        EpisodiosVistos.Add(aux);
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
                            if (Hora[2] == ':')
                            {
                                hora = Convert.ToInt32(Hora.Split(':')[0]);
                                minutos = Convert.ToInt32(Hora.Split(':')[1]);
                            }
                            else
                            {
                                hora = Convert.ToInt32(Hora.Split('.')[0]);
                                minutos = Convert.ToInt32(Hora.Split('.')[1]);
                            }
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

            EpisodiosVistos.Add(aux);

        }

        /// <summary>
        /// Alimenta los episodios de la base de datos a la serie
        /// </summary>
       
        public void alimentarEpisodiosNoVistos()
        {
            //Path del archivo de donde se van a leer los episodios
            string PathArchivoLeer = @"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\EpisodiosNoVistos\" + Nombre + ".txt";
            
            StreamReader leer = new StreamReader(PathArchivoLeer);

            List<Episodio> aDescargar = new List<Episodio>();
            List<Episodio> aMover = new List<Episodio>();

            string linea;
            //Se recorre todo el archivo
            while (!leer.EndOfStream)
            {
                linea = leer.ReadLine();
                //Se saca la fecha 
                DateTime fecha = new DateTime(Convert.ToInt32(linea.Split('*')[4].Split('/')[0]), Convert.ToInt32(linea.Split('*')[4].Split('/')[1]), Convert.ToInt32(linea.Split('*')[4].Split('/')[2]), Convert.ToInt32(linea.Split('*')[4].Split('/')[3]), Convert.ToInt32(linea.Split('*')[4].Split('/')[4]), 0);
                //Se crea el episodio
                Episodio ep = new Episodio(this, Convert.ToInt32(linea.Split('*')[0]), Convert.ToInt32(linea.Split('*')[1]), linea.Split('*')[2], linea.Split('*')[3], fecha, Convert.ToInt32(linea.Split('*')[5]), "720p");

                switch (ep.Estado)
                {
                    case 0:
                        if (ep.Fecha < DateTime.Now)
                        {

                            aDescargar.Add(ep);

                            
                        }
                        else
                        {
                            //Se revisa si el episodio sale en menos de 24 dias
                            if ((ep.Fecha - DateTime.Now).TotalDays < 24)
                            {
                                //Se inicia un timer para empezar a descargar el episodio
                                System.Timers.Timer act = new System.Timers.Timer()
                                {
                                    Interval = (ep.Fecha - DateTime.Now).TotalMilliseconds,
                                };

                                //Se pone el evento que se va a realizar
                                act.Elapsed += (sender, e) => episodioAlAire(sender, e, ep);
                                
                                //Se hecha a andar el timer
                                act.Start();

                            }
                        }
                    break;

                    case 1:
                    if (controlador.torrentTerminado(ep.Hash))
                    {
                        aMover.Add(ep);
                        
                        
                    }
                    else
                    {
                        Descargando++;
                    }

                    break;

                    case 2:
                    PorVer++;
                    break;
                }

                
                //Se añade a la lista de Episodios no vistos
                EpisodiosNoVistos.Add(ep);
               
            }

            if (EpisodiosNoVistos.Count == 0)
            {
                AlDia = true;                 
            }
            else
            {
                Episodio aux = EpisodiosNoVistos[0];
                if (aux.Fecha > DateTime.Now)
                {
                    AlDia = true;
                }
            }

            leer.Close();


            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (sender, e) =>
                {
                    foreach (Episodio ep in aMover)
                    {
                        controlador.moverEpisodioTerminado(ep);
                        ep.Estado = 2;
                        PorVer++;
                    }

                    foreach (Episodio ep in aDescargar)
                    {
                        controlador.descargarEpisodio(ep);
                        ep.Estado = 1;
                        Descargando++;
                    }
                };

            b.RunWorkerCompleted += (sender, e) =>
                {
                    guardarEpisodiosNoVistos();
                };

            b.RunWorkerAsync();


        }

        public void alimentarEpisodiosVistos()
        {
            StreamReader leer = new StreamReader(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\EpisodiosVistos\" + Nombre + ".txt");
            List<Episodio> aux = new List<Episodio>();
            string linea;
            while (!leer.EndOfStream)
            {
                linea = leer.ReadLine();
                if (linea == "-")
                {
                    EpisodiosVistos.Add(aux);
                    aux = new List<Episodio>();
                }
                else
                {
                    DateTime fecha = new DateTime(Convert.ToInt32(linea.Split('*')[4].Split('/')[0]), Convert.ToInt32(linea.Split('*')[4].Split('/')[1]), Convert.ToInt32(linea.Split('*')[4].Split('/')[2]), Convert.ToInt32(linea.Split('*')[4].Split('/')[3]), Convert.ToInt32(linea.Split('*')[4].Split('/')[4]), 0);
                    Episodio ep = new Episodio(this, Convert.ToInt32(linea.Split('*')[0]), Convert.ToInt32(linea.Split('*')[1]), linea.Split('*')[2], linea.Split('*')[3], fecha, Convert.ToInt32(linea.Split('*')[5]), "720p");                      
                    aux.Add(ep);
                }

            }

            EpisodiosVistos.Add(aux);

            leer.Close();

        }

        /// <summary>
        /// Metodo que sirve para crear la base de datos de cada serie
        /// </summary>

        public void guardarEpisodiosVistos()
        {
            StreamWriter escribe = new StreamWriter(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\EpisodiosVistos\" + Nombre + ".txt");

            int cont = 1;
            int ultTemp = EpisodiosVistos.Count;
            foreach (List<Episodio> lista in EpisodiosVistos)
            {
                foreach (Episodio ep in lista)
                {
                    escribe.WriteLine(ep.Imprimir());
                }

                if (cont < ultTemp)
                {
                   escribe.WriteLine("-");
                }

                cont++;

            }
            

            escribe.Close();
           
        }

        public void guardarEpisodiosNoVistos()
        {
            StreamWriter escribe = new StreamWriter(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\EpisodiosNoVistos\" + Nombre + ".txt");

            foreach (Episodio ep in EpisodiosNoVistos)
            {
                escribe.WriteLine(ep.Imprimir());
            }

            escribe.Close();

        }

        public void moverEpisodiosVistosaNoVistos()
        {
            EpisodiosNoVistos = new List<Episodio>();
            List<Episodio> aDescargar = new List<Episodio>();

            for (int t = EpisodiosVistos.Count - 1; t >= Temporada - 1; t--)
            {
                int c = EpisodiosVistos[t].Count - 1;
                while (t == Temporada - 1 ? c >= Capitulo - 1 : c >= 0)
                {
                    Episodio epReferenciado = EpisodiosVistos[t][c];
                    if (DateTime.Now > epReferenciado.Fecha)
                    {
                        if (System.IO.File.Exists(@"C:\Users\Marcelo\Videos\Series\" + Nombre + @"\Temporada " + epReferenciado.Temporada + @"\Episodio " + epReferenciado.Capitulo + ".mkv"))
                        {
                            epReferenciado.Estado = 2;
                            PorVer++;
                        }
                        else
                        {

                            aDescargar.Add(epReferenciado);

                        }
                    }
                    else
                    {
                        epReferenciado.Estado = 0;
                        if ((epReferenciado.Fecha - DateTime.Now).TotalDays < 24)
                        {
                            //Se inicia un timer para empezar a descargar el episodio
                            System.Timers.Timer act = new System.Timers.Timer()
                            {
                                Interval = (epReferenciado.Fecha - DateTime.Now).TotalMilliseconds,
                            };

                            //Se pone el evento que se va a realizar
                            act.Elapsed += (sender, e) => episodioAlAire(sender, e, epReferenciado);

                            //Se hecha a andar el timer
                            act.Start();
                        }

                    }

                    c--;

                    EpisodiosNoVistos.Insert(0, epReferenciado);
                    EpisodiosVistos[t].Remove(epReferenciado);

                    if (EpisodiosVistos[t].Count == 0)
                    {
                        EpisodiosVistos.RemoveAt(t);
                    }
                }
            }

            BackgroundWorker b = new BackgroundWorker();
            b.DoWork += (sender, e) =>
                {
                    foreach (Episodio ep in aDescargar)
                    {
                        controlador.descargarEpisodio(ep);
                        ep.Estado = 1;
                        Descargando++;
                    }

                    guardarEpisodiosNoVistos();
                    guardarEpisodiosVistos();

                };
            b.RunWorkerAsync();

            

        }

        public void actualiza()
        {

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
            return Nombre + "\r\n" + Id + "\r\n" + Subid + "\r\n" + Temporada + " " + Capitulo + "\r\n" + Estado + "\r\n" + Hora + "\r\n" + TemporadaUltEpisodioVisto + " " + CapituloUltEpisodioVisto + "\r\n-";

        }

        /// <summary>
        /// Metodo utilizado para pasar al siguiente episodio de la serie
        /// </summary>
        public void siguienteEpisodio()
        {

            StreamWriter agregarEpisodioVisto = new StreamWriter(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\EpisodiosVistos\" + Nombre + ".txt", true);
            Episodio epActual = EpisodiosNoVistos[0];

            TemporadaUltEpisodioVisto = epActual.Temporada;
            CapituloUltEpisodioVisto = epActual.Capitulo;

            if (epActual.Estado == 1 && epActual.Hash != "-1")
            {
                controlador.quitarTorrent(epActual.Hash);
            }

            epActual.Estado = 3;

            if (epActual.Capitulo == 1) agregarEpisodioVisto.WriteLine("-");
            agregarEpisodioVisto.WriteLine(epActual.Imprimir());
            agregarEpisodioVisto.Close();

            if (episodiosCargados)
            {
                if (epActual.Capitulo == 1) EpisodiosVistos.Add(new List<Episodio>());
                EpisodiosVistos[epActual.Temporada - 1].Add(epActual);
            }

            EpisodiosNoVistos.RemoveAt(0);
            guardarEpisodiosNoVistos();

            if (EpisodiosNoVistos.Count == 0)
            {
                Temporada = 0;
                Capitulo = 0;
                AlDia = true;
            }
            else
            {
                Temporada = EpisodiosNoVistos[0].Temporada;
                Capitulo = EpisodiosNoVistos[0].Capitulo;
                if (EpisodiosNoVistos[0].Estado == 0)
                {
                    AlDia = true;
                }
            }


            PorVer--;
                
            controlador.guardarBasedeDatos();

        }

        /// <summary>
        /// Revisa si el capitulo existe  
        /// </summary>
        /// <param name="temp">Temporada que se quiere revisar</param>
        /// <param name="cap">Capitulo que se quiere revisar</param>
        /// <returns></returns>
        public bool Existe(int temp, int cap)
        {
            if (EpisodiosVistos.Count > temp)
                return true;
            else
                if (EpisodiosVistos.Count == temp)
                    if (EpisodiosVistos[temp - 1].Count >= cap)
                        return true;
                    else
                        return false;
                else
                    return false;
        }

        public void ActualizarBD(IntSerie interf)
        {
            //Se carga el documento de donde se saca la informacion
            XmlDocument doc = new XmlDocument();
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
                            EpisodiosVistos[tempo - 1][epi - 1].NombreEp = nombreEp;
                            EpisodiosVistos[tempo - 1][epi - 1].Fecha = fecha;

                        }
                        else
                        {
                            bool nuevatemp = false;
                            if (tempo > EpisodiosVistos.Count)
                            {
                                EpisodiosVistos.Add(new List<Episodio>());
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
                            EpisodiosVistos[tempo - 1].Add(aux);
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
                                interf.agregarEpisodioAlDesglose(aux);
                            });
                            

                        }
                    }
                }


            }

            


            guardarEpisodiosVistos();


        }

        public void actualizarBasedeDatos()
        {

            XmlDocument doc = new XmlDocument();
            
            doc.Load(@"http://thetvdb.com/api/97AAE7796E3F60D2/series/" + Id + "/all/en.xml");

            //Se actualiza el estado de la serie
            Estado = doc.SelectSingleNode("/Data/Series/Status").InnerText == "Continuing" ? 'c' : 'e';
        }

        private void episodioAlAire(object sender, EventArgs e, Episodio ep)
        {
            
            System.Timers.Timer d = (System.Timers.Timer)sender;

            Application.Current.Dispatcher.Invoke(
               DispatcherPriority.Normal,
               (ThreadStart)delegate
               {
                   controlador.descargarEpisodio(ep);
                   ep.Estado = 1;
                   Descargando++;
                   AlDia = false;
                   guardarEpisodiosNoVistos();
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
