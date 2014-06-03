using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        public char Estado { get; set; } //Estado de la serie
        public List<List<Episodio>> Episodios { get; set; } //Matriz de episodios de la serie
        private int temporada;  //Temporada de siguiente episodio a ver / descargar
        private int capitulo;   //Capitulo de siguiente episodio a ver / descargar
        private int porVer { get; set; } //Cantidad de episodios por ver
        private int descargando;          //Cantidad de episodios por descargar
        private XmlDocument doc;        //Documento de donde se extrae la informacion de la serie
        private string Hora;
        private UTorrentClient uClient = new UTorrentClient(new Uri("http://127.0.0.1:8080/gui/"), "admin", "admin", 1000000);

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
                if (value != capitulo)
                {
                    capitulo = value;
                    OnPropertyChanged("Capitulo");
                }
            }
        }



        /// <summary>
        /// Metodo constructor de la serie
        /// </summary>
        /// <param name="id">Id de la serie</param>

        public Serie(int id)
        {
            Id = id;

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
            AddEpisodes();
        }
        /// <summary>
        /// Constructor de la serie
        /// </summary>
        /// <param name="i">Id de la serie</param>
        /// <param name="n">Nombre de la serie</param>
        /// <param name="ns">Numero de la serie</param>
        /// <param name="e">Estado de la serie</param>
        public Serie(int i, string n, int ns, char e, string h)
        {
            Id = i;
            Nombre = n;
            Numserie = ns;
            Estado = e;
            Descargando = 0;
            PorVer = 0;
            Hora = h;
            Episodios = new List<List<Episodio>>();
        }

        /// <summary>
        /// Metodo utilizado para agregar episodios nuevos
        /// </summary>
        private void AddEpisodes()
        {
            //Se declaran las variables
            XmlNodeList episodios = doc.SelectNodes("/Data/Episode");
            string nombreSerie, nombreEp, calidad;
            int tempo, capi, estado;
            DateTime Fecha;

            //Se ponen los parametros
            nombreSerie = Nombre;
            calidad = "720p";
            List<Episodio> aux = new List<Episodio>();
            int tempAct = 1;

            //Se ingresan los episodios
            foreach (XmlNode e in episodios)
            {
                if (e.SelectSingleNode("SeasonNumber").InnerText != "" && e.SelectSingleNode("SeasonNumber").InnerText != "0")
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
                    int hora = Convert.ToInt32(Hora.Split(':')[0]);
                    int minutos = Convert.ToInt32(Hora.Split(':')[1].Split(' ')[0]);
                    hora = Hora.Split(':')[1].Split(' ')[1] == "PM" ? hora += 12 : hora;
                    Fecha = new DateTime(Convert.ToInt32(fecha2.Substring(0, 4)), Convert.ToInt32(fecha2.Substring(5, 2)), Convert.ToInt32(fecha2.Substring(8, 2)), hora, minutos, 0);
                    Fecha = Fecha.AddDays(0.5);
                    estado = 3;
                    aux.Add(new Episodio(Nombre, tempo, capi, nombreEp, "-1", Fecha, estado, calidad));

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

            //Se descarga la imagen de la serie
            WebClient wc = new WebClient();
            wc.DownloadFile("http://thetvdb.com/banners/" + doc.SelectSingleNode("/Data/Series/fanart").InnerText, @"C:\Users\Marcelo\Documents\Project Eye\Project-Eye\Interfaz\Fanart\" + Nombre + ".jpg");

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
                    }
                    j++;

                }
            }
            if (!System.IO.Directory.Exists(@"C:\Users\Marcelo\Videos\Series\" + Nombre))
            {
                System.IO.Directory.CreateDirectory(@"C:\Users\Marcelo\Videos\Series\" + Nombre);
            }
            CrearArchivo();


        }

        /// <summary>
        /// Alimenta los episodios de la base de datos a la serie
        /// </summary>
        public void AlimentarEps(int t, int c)
        {
            StreamReader leer = new StreamReader(@"C:\Users\Marcelo\Documents\Project Eye\Project-Eye\Base de Datos\Series\" + Nombre + ".txt");
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
                    DateTime fecha = new DateTime(Convert.ToInt32(linea.Split('*')[5].Split('/')[0]), Convert.ToInt32(linea.Split('*')[5].Split('/')[1]), Convert.ToInt32(linea.Split('*')[5].Split('/')[2]), Convert.ToInt32(linea.Split('*')[5].Split('/')[3]), Convert.ToInt32(linea.Split('*')[5].Split('/')[4]), 0);
                    Episodio ep = new Episodio(linea.Split('*')[0], Convert.ToInt32(linea.Split('*')[1]), Convert.ToInt32(linea.Split('*')[2]), linea.Split('*')[3], linea.Split('*')[4], fecha, Convert.ToInt32(linea.Split('*')[6]), "720p");
                    ep.RevisarEp();
                    //Hacer el conteo de PorVer/Descargando
                    if (ep.Estado == 1)
                    {
                        Descargando++;
                    }
                    else
                    {
                        if (ep.Estado == 2)
                        {
                            PorVer++;
                        }
                    }

                    aux.Add(ep);
                }

            }

            Temporada = t;
            Capitulo = c;

            leer.Close();

            CrearArchivo();

        }

        /// <summary>
        /// Metodo que sirve para crear la base de datos de cada serie
        /// </summary>

        public void CrearArchivo()
        {
            string se = "";
            StreamWriter escribe = new StreamWriter(@"C:\Users\Marcelo\Documents\Project Eye\Project-Eye\Base de Datos\Series\" + Nombre + ".txt");
            foreach (List<Episodio> lista in Episodios)
            {
                foreach (Episodio ep in lista)
                {
                    se += ep.NombreSerie + "*" + ep.Temporada + "*" + ep.Capitulo + "*" + ep.NombreEp + "*" + ep.Hash + "*" + ep.Fecha.Year + "/" + ep.Fecha.Month + "/" + ep.Fecha.Day + "/" + ep.Fecha.Hour + "/" + ep.Fecha.Minute + "*" + ep.Estado + "\r\n";
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
            System.IO.File.Delete(@"C:\Users\Marcelo\Documents\Project Eye\Project-Eye\Base de Datos\Series\" + Nombre + ".txt");
        }


        /// <summary>
        /// Metodo que sirve para imprimir la informacion general de la serie
        /// </summary>
        /// <returns>El string con toda la informacion de la serie</returns>
        public string Imprimir()
        {
            return Nombre + "\r\n" + Id + "\r\n" + Temporada + " " + Capitulo + "\r\n" + Estado + "\r\n" + Hora + "\r\n-";

        }

        /// <summary>
        /// Metodo utilizado para pasar al siguiente episodio de la serie
        /// </summary>
        public void SiguienteEp()
        {
            Capitulo++;
            if (Episodios[Temporada - 1].Count < Capitulo)
            {
                Temporada++;
                Capitulo = 1;
                if (Temporada > Episodios.Count)
                {
                    Temporada = 0;
                    Capitulo = 0;
                }

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


        public void ActualizarBD()
        {
            //Se carga el documento de donde se saca la informacion
            doc = new XmlDocument();
            doc.Load(@"http://thetvdb.com/api/97AAE7796E3F60D2/series/" + Id + "/all/en.xml");
            Estado = doc.SelectSingleNode("/Data/Series/Status").InnerText == "Continuing" ? 'c' : 'e';
            XmlNodeList episodios = doc.SelectNodes("/Data/Episode");
            DateTime fecha;
            int tempo, epi, estado;
            string nombreEp;
            foreach (XmlNode e in episodios)
            {
                if (e.SelectSingleNode("SeasonNumber").InnerText != "" && e.SelectSingleNode("SeasonNumber").InnerText != "0")
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
                            if (tempo > Episodios.Count) Episodios.Add(new List<Episodio>());
                            if (fecha < DateTime.Now) estado = 1;
                            else estado = 0;
                            Episodio aux = new Episodio(Nombre, tempo, epi, nombreEp, "-1", fecha, estado, "720p");
                            if (aux.Estado == 1)
                            {
                                aux.getMagnet();
                                Descargando++;
                            }
                            Episodios[tempo - 1].Add(aux);
                        }
                    }
                }


            }

            MessageBox.Show("ListoBDE");


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
