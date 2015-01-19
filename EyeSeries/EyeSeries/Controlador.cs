using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using UTorrentAPI;

namespace EyeSeries
{
    public class Controlador
    {
        public List<Serie> Series;

        private DateTime UltimaActualizacion;
        private bool Actualizar;
        private MainWindow Interfaz;
        UTorrentClient uClient;
        public DispatcherTimer checarTerminodeDescarga;

        public Controlador(MainWindow i)
        {
            Series = new List<Serie>();
            uClient = new UTorrentClient(new Uri("http://127.0.0.1:8080/gui/"), "admin", "admin", 1000000);
            Interfaz = i;

            checarTerminodeDescarga = new DispatcherTimer();
            checarTerminodeDescarga.Interval = new TimeSpan(0,0,5);
            checarTerminodeDescarga.Tick += chequeoTerminoDescarga;
        }

        /// <summary>
        /// Metodo utilizado para rellenar la lista de series con la 
        /// información de la base de datos.
        /// </summary>
        private void alimentarBasedeDatos()
        {

            //Se abre el archivo de donde se van a leer los datos
            string archivoAbrir = @"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\General2.txt";
            StreamReader leer = new StreamReader(archivoAbrir);

            //Revisa si la ultima actualizacion fue hace mas de 14 dias
            string ultact = leer.ReadLine();
            UltimaActualizacion = new DateTime(Convert.ToInt32(ultact.Split('/')[2]), Convert.ToInt32(ultact.Split('/')[1]), Convert.ToInt32(ultact.Split('/')[0]));
            Actualizar = (DateTime.Now - UltimaActualizacion).TotalDays > 7;


            //Se recorren todas las series
            int i = 0;
            while (!leer.EndOfStream)
            {
                string nombre = leer.ReadLine();
                int id = Convert.ToInt32(leer.ReadLine());
                int subid = Convert.ToInt32(leer.ReadLine());
                string cap = leer.ReadLine();
                int temp = Convert.ToInt32(cap.Split(' ')[0]);
                int capi = Convert.ToInt32(cap.Split(' ')[1]);
                char estado = Convert.ToChar(leer.ReadLine());
                string hora = leer.ReadLine();
                Serie s = new Serie(id, nombre, temp, capi, i, estado, hora, subid, this);
                Series.Add(s);
                leer.ReadLine();
                i++;
            }


        }

        /// <summary>
        /// Metodo utilizado para rellenar los episodios no vistos de
        /// cada serie
        /// </summary>
        private void alimentarBasedeDatosCapitulosNoVistos()
        {
            foreach (Serie s in Series)
            {                
                s.alimentarEpisodiosNoVistos();                
            }
                    
        }

        public void guardarBasedeDatos()
        {
            StreamWriter escribe = new StreamWriter(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\General2.txt");
            escribe.WriteLine(UltimaActualizacion.Day.ToString() + "/" + UltimaActualizacion.Month.ToString() + "/" + UltimaActualizacion.Year.ToString());
            foreach (Serie s in Series)
            {
                escribe.WriteLine(s.Imprimir());
            }

            escribe.Close();
        }

        public void iniciarAplicacion()
        {
            DateTime inicio = DateTime.Now;
            //Se alimenta la base de datos de series
            alimentarBasedeDatos();
            //Se alimentan la informacion de los episodios por ver
            alimentarBasedeDatosCapitulosNoVistos();


            //Se crea la primer pagina
            Interfaz.CrearPag(0);

            //Contador de series agregadas
            int cont = 0; 

            //Se recorren todas las series
           
            foreach (Serie s in Series)
            {
                //Se calcula la página de la serie a agregar
                int pag = cont / 9; 

                //Si la serie va en una página nueva esta se agrega
                if (cont % 9 == 0 && cont != 0) 
                {
                    Interfaz.CrearPag(pag);
                }

                //Se agrega la serie a la interfaz
                Interfaz.AgregarSerie2(s, cont);
             
                cont++;
            }

            //Se agrega todo lo relacionado a añadir una nueva serie
            Interfaz.agregarInterfazAñadir();

            //Se añade el siguiente y anterior
            Interfaz.agregarSigAnt();

            //Se inicia el chequeo a ver si algo ya se termino de descargar
            checarTerminodeDescarga.Start();



            DateTime final = DateTime.Now;
            TimeSpan diferencia = final - inicio;
            MessageBox.Show(diferencia.TotalSeconds.ToString());

        }

        public void agregarNuevaSerie(Serie s, int temp, int cap)
        {
            s.Temporada = temp;
            s.Capitulo = cap;

            if (!System.IO.Directory.Exists(@"C:\Users\Marcelo\Videos\Series\" + s.Nombre))
            {
                System.IO.Directory.CreateDirectory(@"C:\Users\Marcelo\Videos\Series\" + s.Nombre);
            }

            s.moverEpisodiosVistosaNoVistos();

            Interfaz.AgregarSerie2(s, Series.Count);

            Series.Add(s);
            
        }

        public double getProgresoTorrent(string hash)
        {
            double prog = (double)uClient.Torrents[hash].ProgressInMils / 10.0;
            return Math.Round(prog, 1);
        }

        public bool torrentTerminado(string hash)
        {
            return uClient.Torrents[hash].RemainingBytes == 0 && uClient.Torrents[hash].DownloadedBytes > 0 || uClient.Torrents[hash].ProgressInMils == 1000;
        }

        public void descargarEpisodio(Episodio ep)
        {
            Ping checarConexion = new Ping();

            /*try
            {
                var result = checarConexion.Send("http://thepiratebay.se");
                descargarEpisodioTPB(ep);
            }
            catch
            {*/
            descargarEpisodioKickass(ep);
            //}
       
        }

        private void descargarEpisodioTPB(Episodio ep)
        {
            string link;
            string temp = (ep.Temporada < 10 ? "0" : "") + ep.Temporada;
            string cap = (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo;
            string codigoFuentePagina;
            int primero, segundo;
            bool lastresort = false;
            string pagina = "";
            do
            {

                if (!lastresort)
                    pagina = "http://thepiratebay.se/search/" + ep.Serie.Nombre.Replace(' ', '+') + "+S" + temp + "E" + cap + "+" + ep.Calidad + "/0/7/0";
                HttpDownloader fuente = new HttpDownloader(pagina, "thepiratebay.se", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:29.0) Gecko/20100101 Firefox/29.0");
                codigoFuentePagina = fuente.GetPage();

                primero = codigoFuentePagina.IndexOf("href=\"magnet");
                if (primero == -1)
                {
                    if (ep.Calidad == "")
                    {
                        if (cap == "01" || cap == "02" || cap == "21" || cap == "22" && !lastresort)
                        {
                            lastresort = true;
                            if (cap == "01" || cap == "02") pagina = "http://thepiratebay.se/search/" + ep.Serie.Nombre.Replace(' ', '+') + "+S" + temp + "E01E02+720P/0/7/0";
                            else if (cap == "21" || cap == "22") pagina = pagina = "http://thepiratebay.se/search/" + ep.Serie.Nombre.Replace(' ', '+') + "+S" + temp + "E21E22+720P/0/7/0";
                        }
                        else
                        {
                            link = "-1";
                            break;
                        }
                    }
                    ep.Calidad = (ep.Calidad == "1080p" ? "720p" : "");
                }

            }
            while (primero == -1);




            if (primero != -1)
            {
                //Saca el link de la pagina
                segundo = codigoFuentePagina.IndexOf("\" title", primero);
                link = codigoFuentePagina.Substring(primero + 6, segundo - primero - 6);
                uClient.Torrents.AddUrl(link);
                //Saca el HASH del link
                Regex r2 = new Regex(".+xt=urn:btih:(.+?)&dn=.+");
                ep.Hash = r2.Match(link).Groups[1].Value.ToUpper();

            }
            else
            {
                throw new Exception("No se encontro el torrent");
            }
        }

        private void descargarEpisodioKickass(Episodio ep)
        {
            //Se construye la pagina con los datos del capitulo
            string temp = (ep.Temporada < 10 ? "0" : "") + ep.Temporada;
            string cap = (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo;
            string calidad = "720p";
            string nombreSerie = ep.Serie.Nombre;

            bool torrentEncontrado = false;
            string codigoFuentePagina = "";
            while (!torrentEncontrado)
            {
                string pagina = @"http://kickass.so/usearch/"+ nombreSerie + " S" + temp +"E" + cap + " " + calidad + @"/?field=seeders&sorder=desc";

                //Se saca el codigo fuente de la pagina
                HttpDownloader fuente = new HttpDownloader(pagina, "kickass.so", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:29.0) Gecko/20100101 Firefox/29.0");
                codigoFuentePagina = fuente.GetPage();

                //Se checa si la búsqueda encontró resultados
                Regex resultados = new Regex("did not match any documents");
                if (!resultados.Match(codigoFuentePagina).Success)
                {
                    torrentEncontrado = true;
                }
                else
                {
                    if (calidad != "")
                    {
                        calidad = "";
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (torrentEncontrado)
            {

                Regex encontrarMagnet = new Regex("Torrent magnet link\" href=\"(.*?)\"");
                string magnet = encontrarMagnet.Match(codigoFuentePagina).Groups[1].Value;

                //Saca el HASH del link
                Regex r2 = new Regex(".+xt=urn:btih:(.+?)&dn=.+");
                ep.Hash = r2.Match(magnet).Groups[1].Value.ToUpper();

                uClient.Torrents.AddUrl(magnet);
            }

            else
            {
                MessageBox.Show("TORRRENT NO ENCONTRADO");
            }


        }

        private void descargarEpisodioTOPB(Episodio ep)
        {
            //Se construye la pagina con los datos del capitulo
            string temp = (ep.Temporada < 10 ? "0" : "") + ep.Temporada;
            string cap = (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo;
            string calidad = "720p";
            string nombreSerie = ep.Serie.Nombre;

            bool torrentEncontrado = false;
            string codigoFuentePagina = "";
            while (!torrentEncontrado)
            {
                string pagina = @"https://oldpiratebay.org/search.php?q=" + "\"" + nombreSerie+ "\"" + " \"S" + temp + "E" + cap + "\" " + calidad + @"&Torrent_sort=seeders.desc";

                //Se saca el codigo fuente de la pagina
                HttpDownloader fuente = new HttpDownloader(pagina, "oldpiratebay.org", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:29.0) Gecko/20100101 Firefox/29.0");
                codigoFuentePagina = fuente.GetPage();

                //Se checa si la búsqueda encontró resultados
                Regex resultados = new Regex("No results found.");
                if (!resultados.Match(codigoFuentePagina).Success)
                {
                    torrentEncontrado = true;
                }
                else
                {
                    if (calidad != "")
                    {
                        calidad = "";
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (torrentEncontrado)
            {
                int primero = codigoFuentePagina.IndexOf("href='magnet");
                int segundo = codigoFuentePagina.IndexOf("' title", primero);
                string magnet = codigoFuentePagina.Substring(primero + 6, segundo - primero - 6);
                //Saca el HASH del link
                Regex r2 = new Regex(".+xt=urn:btih:(.+?)&dn=.+");

                if (r2.Match(magnet).Success)
                {
                    ep.Hash = r2.Match(magnet).Groups[1].Value.ToUpper();
                }
                else
                {
                    Regex r3 = new Regex(".+xt=urn:btih:(.+?)&amp");
                    ep.Hash = r3.Match(magnet).Groups[1].Value.ToUpper();
                }

                uClient.Torrents.AddUrl(magnet);
            }

            else
            {
                MessageBox.Show("TORRRENT NO ENCONTRADO");
            }
        }

        public void moverEpisodioTerminado(Episodio ep)
        {
            string pathOrigen = uClient.Torrents[ep.Hash].SavePath;
            string nombreArchivo = uClient.Torrents[ep.Hash].Name;

            //Saca el path del archivo .mkv
            foreach (UTorrentAPI.File f in uClient.Torrents[ep.Hash].Files)
            {
                if (f.Path.EndsWith(".mkv"))
                {
                    if (!f.Path.Contains(@"Sample\"))
                    {
                        pathOrigen += @"\" + f.Path;
                        break;
                    }
                }
            }

            //El path destino es en donde está la temporada de la serie
            string pathDestino = @"C:\Users\Marcelo\Videos\Series\" + ep.Serie.Nombre + @"\Temporada " + ep.Temporada;

            //Si no existe el path destino, se crea
            if (!System.IO.Directory.Exists(pathDestino))
            {
                System.IO.Directory.CreateDirectory(pathDestino);
            }

            //Se elimina el torrent de la lista de torrents
            uClient.Torrents.Remove(ep.Hash, TorrentRemovalOptions.TorrentFile);
            //Se pone su hash en -1
            ep.Hash = "-1";

            //Se añade al path destino el capitulo
            pathDestino += @"\Episodio " + ep.Capitulo + ".mkv";

            try
            {
                if (!System.IO.File.Exists(pathDestino))
                {
                    //Se copia al destino
                    System.IO.File.Move(pathOrigen, pathDestino);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("No jalo! " + e.Message);
            }

            //Se descarga el subtitulo
            descargarSubtituloSUBDB(nombreArchivo, pathDestino, ep);

        }

        private void descargarSubtituloSUBDB(string nombreArchivo, string pathDestino, Episodio ep)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("user-agent", "SubDB/1.0 (Pyrrot/0.1; http://github.com/jrhames/pyrrot-cli)");
                wc.DownloadFile("http://api.thesubdb.com/?action=download&hash=" + getCodigoParaSubtitulo(pathDestino) + "&language=en", @"C:\Users\Marcelo\Videos\Series\" + ep.Serie.Nombre + @"\Temporada " + ep.Temporada + @"\Episodio " + ep.Capitulo + ".srt");
            }
            catch
            {
                descargarSubtituloAddicted(ep, nombreArchivo);
            }
        }

        private void descargarSubtituloAddicted(Episodio ep, string name)
        {
            string nombreserie = ep.Serie.Nombre.Replace(' ', '+');
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
            string pags = getid.DownloadString("http://www.addic7ed.com/ajax_loadShow.php?show=" + ep.Serie.Subid + @"&season=" + ep.Temporada + "&langs=|1|&hd=1&hi=0");
            Regex subs = new Regex(@"<tr class=""epeven completed""><td>" + ep.Temporada + @"</td><td>" + ep.Capitulo + @"</td><td><a href="".+"">.+English</td><td class=""c"">(.+)</td>\n+.+<td class=""c""><a href=""(.+)"">Download</a></td>");
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
            p.Headers.Add("referer", "http://www.addic7ed.com/" + "http://www.addic7ed.com/ajax_loadShow.php?show=" + ep.Serie.Subid + @"&season=" + ep.Temporada + "&langs=|1|&hd=1&hi=2");
            p.DownloadFile(@"http://www.addic7ed.com" + desc, @"C:\Users\Marcelo\Videos\Series\" + ep.Serie.Nombre + @"\Temporada " + ep.Temporada + @"\Episodio " + ep.Capitulo + "ad.srt");
        }

        private static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        private string getCodigoParaSubtitulo(string pathDestino)
        {
            FileStream f = null;
            f = System.IO.File.OpenRead(pathDestino);
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
            return sb.ToString();
        }

        private void chequeoTerminoDescarga(object sender, EventArgs e)
        {
            foreach (Serie s in Series)
            {
                if (!s.AlDia)
                {
                    foreach (Episodio ep in s.EpisodiosNoVistos)
                    {
                        if (ep.Estado == 1)
                        {
                            if (ep.Hash != "-1")
                            {
                                if (torrentTerminado(ep.Hash))
                                {
                                    BackgroundWorker b = new BackgroundWorker();
                                    b.DoWork += (sender2, e2) => moverEpisodioTerminado(ep);
                                    b.RunWorkerAsync();
                                    s.Descargando--;
                                    s.PorVer++;
                                    ep.Estado = 2;
                                    s.guardarEpisodiosNoVistos();
                                }
                            }

                        }
                        else
                            if (ep.Estado == 0)
                            {
                                break;
                            }
                    }
                }
            }
        }

        public void desplegarEpisodios(IntSerie s)
        {

            Serie serieaDesplegar = s.Se;
            bool primeraVez = serieaDesplegar.EpisodiosVistos.Count == 0;
            Interfaz.subirLona(s, primeraVez);

            if (!serieaDesplegar.episodiosCargados)
            {
                BackgroundWorker b = new BackgroundWorker();
                b.DoWork += (sender, e) =>
                    {
                        serieaDesplegar.alimentarEpisodiosVistos();
                    };
                b.RunWorkerCompleted += (sender, e) =>
                    {
                        Interfaz.agregarDesgloseEpisodios(s);
                        serieaDesplegar.episodiosCargados = true;

                    };
                b.RunWorkerAsync();
            }

           
        }

        private void reproducirEpisodio(Episodio ep)
        {
            string dir = @"C:\Users\Marcelo\Videos\Series\" + ep.Serie.Nombre + @"\Temporada " + ep.Temporada + @"\Episodio " + ep.Capitulo + ".mkv";
            System.Diagnostics.Process.Start(dir);
        }

        public void cambiarEpPrincipal(Episodio ep)
        {
            Serie Se = ep.Serie;

            if (ep.Estado == 2)
            {
                reproducirEpisodio(ep);

                while (Se.EpisodiosNoVistos.Count != 0 && Se.EpisodiosNoVistos[0] != ep)
                {
                    Se.siguienteEpisodio();
                }

                Se.siguienteEpisodio();

            }
            else
            {
                if (ep.Estado == 3)
                {

                    for (int t = Se.EpisodiosVistos.Count - 1; t >= Se.Temporada - 1; t--)
                    {
                        int c = Se.EpisodiosVistos[t].Count - 1;
                        while (t == ep.Temporada - 1? c >= ep.Capitulo - 1 : c >= 0)
                        {
                           
                            Episodio epReferenciado = Se.EpisodiosVistos[t][c];
                            if (System.IO.File.Exists(@"C:\Users\Marcelo\Videos\Series\" + Se.Nombre + @"\Temporada " + epReferenciado.Temporada + @"\Episodio " + epReferenciado.Capitulo + ".mkv"))
                            {
                                epReferenciado.Estado = 2;
                                Se.PorVer++;
                            }
                            else
                            {
                                epReferenciado.Estado = 1;
                                BackgroundWorker b = new BackgroundWorker();
                                b.DoWork += (sender, e) => descargarEpisodio(ep);
                                b.RunWorkerCompleted += (sender, e) => Se.Descargando++;
                                
                            }
                            c--;

                            Se.EpisodiosNoVistos.Insert(0, epReferenciado);
                            Se.EpisodiosVistos[t].Remove(epReferenciado);

                            if (Se.EpisodiosVistos[t].Count == 0)
                            {
                                Se.EpisodiosVistos.RemoveAt(t);
                            }
                            

                        }
                    }

                    Se.Temporada = ep.Temporada;
                    Se.Capitulo = ep.Capitulo;

                    Se.guardarEpisodiosNoVistos();
                    Se.guardarEpisodiosVistos();
                    guardarBasedeDatos();
                }
            }
        }


    }
}
