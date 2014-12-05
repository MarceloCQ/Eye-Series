using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using UTorrentAPI;

namespace EyeSeries
{
    /*Clase episodio que representa un episodio de la serie
     */
    public class Episodio
    {
        //Atributos
        public Serie Serie { get; set; } //Serie a la que pertenece
        private string nombreep; //Nombre del episodio
        public int Temporada { get; set; } //Numero de Temporada
        public int Capitulo { get; set; } //Numero de Capitulo
        private int estado; //0-> No ha salido, 1-> Descargando, 2-> Descargado, 3-> Visto
        private DateTime fecha; //Fecha de cuando salio el episodio
        public string Hash { get; set; } //Hash del torrent
        public string Calidad { get; set; } //Calidad 720p/1080p
        private UTorrentClient uClient;

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

        public string NombreEp
        {
            get { return nombreep; }
            set
            {
                if (value != nombreep)
                {
                    nombreep = value;
                    OnPropertyChanged("NombreEp");
                }
            }
        }

        public DateTime Fecha
        {
            get { return fecha; }
            set
            {
                if (value != fecha)
                {
                    fecha = value;
                    OnPropertyChanged("Fecha");
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
            uClient = new UTorrentClient(new Uri("http://127.0.0.1:8080/gui/"), "admin", "admin", 1000000);
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
            bool lastresort = false;
            string pagina = "";
            do
            {
                
                if (!lastresort)
                    pagina = "http://thepiratebay.se/search/" + Serie.Nombre.Replace(' ', '+') + "+S" + temp + "E" + cap + "+" + Calidad + "/0/7/0";
                HttpDownloader fuente = new HttpDownloader(pagina, "thepiratebay.se", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:29.0) Gecko/20100101 Firefox/29.0");
                codigo = fuente.GetPage();
                primero = codigo.IndexOf("href=\"magnet");
                if (primero == -1)
                {
                    if (Calidad == "")
                    {
                        if (cap == "01" || cap == "02"  || cap == "21" || cap == "22" && !lastresort )
                        {
                            lastresort = true;
                            if (cap == "01" || cap == "02") pagina = "http://thepiratebay.se/search/" + Serie.Nombre.Replace(' ', '+') + "+S" + temp + "E01E02+720P/0/7/0";
                            else if (cap == "21" || cap == "22") pagina = pagina = "http://thepiratebay.se/search/" + Serie.Nombre.Replace(' ', '+') + "+S" + temp + "E21E22+720P/0/7/0";
                        }
                        else
                        {
                            link = "-1";
                            break;
                        }
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

        private void getSubtitulo(string nombrearch, bool movido, string patha)
        {
            FileStream f = null;
            if (movido)
            {
                f = System.IO.File.OpenRead(@"C:\Users\Marcelo\Videos\Series\" + Serie.Nombre + @"\Temporada " + Temporada + @"\Episodio " + Capitulo + ".mkv");
            }
            else
            {
                f = System.IO.File.OpenRead(patha);
            }

            byte[] inicio = new byte[64 * 1024];
            f.Read(inicio, 0, 64 * 1024);
            long p = f.Length - 64 * 1024;
            byte[] final = new byte[64 * 1024];
            f.Position = p;
            f.Read(final, 0, 64 * 1024);
            byte[] total = Combine(inicio, final);
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(total);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            string codigo = sb.ToString();
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("user-agent", "SubDB/1.0 (Pyrrot/0.1; http://github.com/jrhames/pyrrot-cli)");
                wc.DownloadFile("http://api.thesubdb.com/?action=download&hash=" + codigo + "&language=en", @"C:\Users\Marcelo\Videos\Series\" + Serie.Nombre + @"\Temporada " + Temporada + @"\Episodio " + Capitulo + ".srt");
            }
            catch (Exception e)
            {
                getSubtitulo2(nombrearch);
            }


           
        }

        private void getSubtitulo2(string name)
        {
            string nombreserie = Serie.Nombre.Replace(' ', '+');
            string vers, desc = null, descalt = null;
            bool flag = false;
            Regex vert = new Regex(@"\.720p\.(.+?)\.");
            name = name.Replace(' ', '.');
            if (vert.Match(name).Groups[1].Value == "WEB-DL")
            {
                vers = vert.Match(name).Groups[1].Value;
            }
            else
            {
                vert = new Regex(@".+?-(\w+)");
                vers = vert.Match(name).Groups[1].Value;
            }
            WebClient getid = new WebClient();
            string pags = getid.DownloadString("http://www.addic7ed.com/ajax_loadShow.php?show=" + Serie.Subid + @"&season=" + Temporada + "&langs=|1|&hd=1&hi=0");
            Regex subs = new Regex(@"<tr class=""epeven completed""><td>" + Temporada + @"</td><td>" + Capitulo + @"</td><td><a href="".+"">.+English</td><td class=""c"">(.+)</td>\n+.+<td class=""c""><a href=""(.+)"">Download</a></td>");
            MatchCollection m2 = subs.Matches(pags);
            foreach (Match m in m2)
            {
                
                if (m.Groups[1].Value != "WEB-DL")
                {
                    descalt = m.Groups[2].Value;
                }
                if (m.Groups[1].Value == vers)
                {
                    desc = m.Groups[2].Value;
                    flag = true;
                    break;
                }
                
            }
            if (!flag)
                desc = descalt;

            WebClient p = new WebClient();
            p.Headers.Add("referer", "http://www.addic7ed.com/" + "http://www.addic7ed.com/ajax_loadShow.php?show=" + Serie.Subid + @"&season=" + Temporada + "&langs=|1|&hd=1&hi=2");
            p.DownloadFile(@"http://www.addic7ed.com" + desc, @"C:\Users\Marcelo\Videos\Series\" + Serie.Nombre + @"\Temporada " + Temporada + @"\Episodio " + Capitulo + "ad.srt");
        }

        public void Mover(UTorrentClient uClient, bool primera)
        {
            string path = uClient.Torrents[Hash].SavePath;
            string nombrea = uClient.Torrents[Hash].Name;
            foreach (UTorrentAPI.File f in uClient.Torrents[Hash].Files)
            {
                if (f.Path.EndsWith(".mkv"))
                {
                    if (!f.Path.Contains(@"Sample\"))
                    {
                        path += @"\" + f.Path;
                        break;
                    }
                }
            }
            string dest = @"C:\Users\Marcelo\Videos\Series\" + Serie.Nombre + @"\Temporada " + Temporada;
            if (!System.IO.Directory.Exists(dest))
            {
                System.IO.Directory.CreateDirectory(dest);
            }
            uClient.Torrents.Remove(Hash, TorrentRemovalOptions.TorrentFile);
            Hash = "-1";
            bool corr = true;
            try
            {
                if (!System.IO.File.Exists(dest + @"\Episodio " + Capitulo + ".mkv"))
                {
                    System.IO.File.Move(path, dest + @"\Episodio " + Capitulo + ".mkv");
                }
            }
            catch (Exception e)
            {
               //Implementar una estructura de datos temporal con un path y el episodio para cuando 
              //el usuario le de salir se mueva
            }
            getSubtitulo(nombrea, corr, path);
            Estado = 2;

            if (!primera)
            {
                Serie.PorVer++;
                Serie.Descargando--;
                Serie.CrearArchivo();
            }

       /*     if (!corr)
            {
                try
                {
                    if (!System.IO.File.Exists(dest + @"\Episodio " + Capitulo + ".mkv"))
                    {
                        System.IO.File.Move(path, dest + @"\Episodio " + Capitulo + ".mkv");
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + " " + Serie.Nombre + " S" + Temporada + "E" + Capitulo);
                }
            } */

        }

        public string Imprimir()
        {
            return Temporada + "*" + Capitulo + "*" + NombreEp + "*" + Hash + "*" + Fecha.Year + "/" + Fecha.Month + "/" + Fecha.Day + "/" + Fecha.Hour + "/" + Fecha.Minute + "*" + Estado;
        }


        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
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
