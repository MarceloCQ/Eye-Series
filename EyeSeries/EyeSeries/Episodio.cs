using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UTorrentAPI;

namespace EyeSeries
{
    /*Clase episodio que representa un episodio de la serie
     */
    public class Episodio
    {
        //Atributos
        public Serie Serie { get; set; } //Serie a la que pertenece
        public string NombreEp { get; set; } //Nombre del episodio
        public int Temporada { get; set; } //Numero de Temporada
        public int Capitulo { get; set; } //Numero de Capitulo
        private int estado; //0-> No ha salido, 1-> Descargando, 2-> Descargado, 3-> Visto
        public DateTime Fecha { get; set; } //Fecha de cuando salio el episodio
        public string Hash { get; set; } //Hash del torrent
        public string Calidad { get; set; } //Calidad 720p/1080p
        private UTorrentClient uClient = new UTorrentClient(new Uri("http://127.0.0.1:8080/gui/"), "admin", "admin", 1000000);

        //Eventos
        public event PropertyChangedEventHandler PropertyChanged;

        public int Estado
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
        /// Constructor de episodio
        /// </summary>
        /// <param name="nombrese">Nombre de la serie</param>
        /// <param name="tempo">Temporada</param>
        /// <param name="capi">Capitulo</param>
        /// <param name="nombrep">Nombre del episodio</param>
        /// <param name="hash">Hash del torrent del episodio</param>
        /// <param name="fecha">Fecha en que se transmite el episodio</param>
        /// <param name="estado">Estadp del episodio</param>
        /// <param name="calidad">Calidad del episodio</param>
        public Episodio(Serie s, int tempo, int capi, string nombrep, string hash, DateTime fecha, int estado, string calidad)
        {
            NombreEp = nombrep;
            Serie = s;
            Temporada = tempo;
            Capitulo = capi;
            Estado = estado;
            Fecha = fecha;
            Hash = hash;
            Calidad = calidad;
            Hash = hash;
        }

        public void getMagnet()
        {
            string link;
            string temp = (Temporada < 10 ? "0" : "") + Temporada;
            string cap = (Capitulo < 10 ? "0" : "") + Capitulo;
            string codigo;
            int primero, segundo;

            do
            {
                string pagina = "http://thepiratebay.se/search/" + Serie.Nombre.Replace(' ', '+') + "+S" + temp + "E" + cap + "+" + Calidad + "/0/7/0";
                HttpDownloader fuente = new HttpDownloader(pagina, "thepiratebay.se", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:29.0) Gecko/20100101 Firefox/29.0");
                codigo = fuente.GetPage();
                primero = codigo.IndexOf("href=\"magnet");
                if (primero == -1)
                {
                    if (Calidad == "")
                    {
                        link = "-1";
                        break;
                    }
                    Calidad = (Calidad == "1080p" ? "720p" : "");
                }

            }
            while (primero == -1);

            if (primero != -1)
            {
                //Saca el link de la pagina
                segundo = codigo.IndexOf("\" title", primero);
                link = codigo.Substring(primero + 6, segundo - primero - 6);
               uClient.Torrents.AddUrl(link);
             //   Saca el HASH del link
                Regex r2 = new Regex(".+xt=urn:btih:(.+?)&dn=.+");
                Hash = r2.Match(link).Groups[1].Value.ToUpper();

            }
            else
            {
                throw new Exception("No se encontro el torrent");
            }

        }
        /// <summary>
        /// Lo que hace es revisar si el episodio ya salio, si es así, lo descarga
        /// Tambien revisa si es que esta completo el torrent y lo mueve y renombra
        /// </summary>
        public void RevisarEp(Boolean primera)
        {
            if (Estado == 0)
            {
                if (Fecha < DateTime.Now)
                {
                    Estado = 1;
                    getMagnet();
                    Serie.CrearArchivo();
                    if (!primera)
                    {
                        Serie.Descargando++;
                    }
                    
                }
            }
            else
            {
                if (Estado == 1)
                {
                    if (uClient.Torrents[Hash].RemainingBytes == 0 && uClient.Torrents[Hash].DownloadedBytes > 0)
                    {
                        string path = uClient.Torrents[Hash].SavePath;
                        foreach (UTorrentAPI.File f in uClient.Torrents[Hash].Files)
                        {
                            if (f.Path.EndsWith(".mkv"))
                            {
                                path += @"\" + f.Path;
                                break;
                            }
                        }
                        string dest = @"C:\Users\Marcelo\Videos\Series\" + Serie.Nombre + @"\Temporada " + Temporada;
                        if (!System.IO.Directory.Exists(dest))
                        {
                            System.IO.Directory.CreateDirectory(dest);
                        }
                        uClient.Torrents.Remove(Hash, TorrentRemovalOptions.TorrentFile);
                        Hash = "-1";
                        System.IO.File.Move(path, dest + @"\Episodio " + Capitulo + ".mkv");
                        
                        Estado = 2;
                        Serie.CrearArchivo();
                        if (!primera)
                        {
                            Serie.PorVer++;
                            Serie.Descargando--;
                        }

                    }
                }
            }
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
