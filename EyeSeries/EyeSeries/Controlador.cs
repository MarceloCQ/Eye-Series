using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml;
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

            //Revisa si la ultima actualizacion fue hace mas de 7 dias
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
                string capUV = leer.ReadLine();

                int tempUV = Convert.ToInt32(capUV.Split(' ')[0]);
                int capiUV = Convert.ToInt32(capUV.Split(' ')[1]);

                Serie s = new Serie(id, nombre, temp, capi, i, estado, hora, subid, tempUV, capiUV, this);
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
                escribe.WriteLine(s.imprimir());
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
                Interfaz.agregarSerie(s, cont);
             
                cont++;
            }

            //Se agrega todo lo relacionado a añadir una nueva serie
            Interfaz.agregarInterfazAñadir();

            //Se añade el siguiente y anterior
            Interfaz.agregarSigAnt();

            //Se inicia el chequeo a ver si algo ya se termino de descargar
            checarTerminodeDescarga.Start();

            Interfaz.bajarInformacion();

            DateTime final = DateTime.Now;
            TimeSpan diferencia = final - inicio;
         //   MessageBox.Show(diferencia.TotalSeconds.ToString());

            if (Actualizar)
            {
                BackgroundWorker b = new BackgroundWorker();
                b.DoWork += (sender, e) => actualizarBasedeDatos();
                b.RunWorkerCompleted += (sender, e) =>
                    {
                        UltimaActualizacion = DateTime.Now;
                        guardarBasedeDatos();
                       // MessageBox.Show("LISTO");
                    };
                b.RunWorkerAsync();
            }


        }

        public void agregarNuevaSerie(Serie s, int temp, int cap)
        {
            s.Temporada = temp;
            s.Capitulo = cap;


            if (!System.IO.Directory.Exists(@"C:\Users\Marcelo\Videos\Series\" + s.Nombre))
            {
                System.IO.Directory.CreateDirectory(@"C:\Users\Marcelo\Videos\Series\" + s.Nombre);
            }

            if (temp != 0 && cap != 0)
            {
                s.moverEpisodiosVistosaNoVistos();
                Episodio aux = s.EpisodiosNoVistos[0];
                if (aux.Fecha > DateTime.Now)
                {
                    s.AlDia = true;
                }
            }
            else
            {  
                s.EpisodiosNoVistos = new List<Episodio>();
                s.guardarEpisodiosVistos();
                s.guardarEpisodiosNoVistos();
                s.AlDia = true;
            }

            Interfaz.agregarSerie(s, Series.Count);

            Interfaz.agregarDesgloseEpisodios(Interfaz.getInterfazSerie(Series.Count), true);

            Interfaz.reacomodarInterfazAñadir(Series.Count + 1, true);

            Series.Add(s);

            guardarBasedeDatos();
            
        }

        public double getProgresoTorrent(string hash)
        {
            if (hash != "-1")
            {
                double prog = (double)uClient.Torrents[hash].ProgressInMils / 10.0;
                return Math.Round(prog, 1);
            }
            else
            {
                return 0;
            }
        }

        public bool torrentTerminado(string hash)
        {
            return uClient.Torrents[hash].RemainingBytes == 0 && uClient.Torrents[hash].DownloadedBytes > 0 || uClient.Torrents[hash].ProgressInMils == 1000;
        }

        public void descargarEpisodio(Episodio ep)
        {

            string magnet = "";

            magnet = descargarEpisodioKickass(ep, false);

            if (magnet == "")
            {
                magnet = descargarEpisodioTPB(ep, false);
            }

            if (magnet == "")
            {
                //Checa a ver si el episodio es doble y ya se descargo
                if (descargarEpisodioKickass(ep, true) != "")
                {
                    Episodio epAnterior = ep.Serie.buscarEpEnNoVistos(ep.Temporada, ep.Capitulo - 1);
                    ep.Hash = epAnterior.Hash;
                    epAnterior.Doble = true;
                }
                else
                {
                    magnet = descargarEpisodioTPB(ep, true);
                    if (magnet != "")
                    {
                        ep.Doble = true;
                        uClient.Torrents.AddUrl(magnet);
                        Regex r2 = new Regex(".+xt=urn:btih:(.+?)&dn=.+");
                        ep.Hash = r2.Match(magnet).Groups[1].Value.ToUpper();
                        
                    }
                    else
                    {
                        if (ep.Serie.Capitulo > 1 && ep.Serie.buscarEpEnNoVistos(ep.Temporada, ep.Capitulo - 1).Doble)
                        {
                            Episodio epAnterior = ep.Serie.buscarEpEnNoVistos(ep.Temporada, ep.Capitulo - 1);
                            ep.Hash = epAnterior.Hash;
                        }
                        else
                        {
                            MessageBox.Show("No se encontro el torrent");
                        }
                    }
                }
            }
            else
            {
                uClient.Torrents.AddUrl(magnet);

                Regex r2 = new Regex(".+xt=urn:btih:(.+?)&dn=.+");
                ep.Hash = r2.Match(magnet).Groups[1].Value.ToUpper();
            }

            

            //    Regex r2 = new Regex(".+xt=urn:btih:(.+?)&dn=.+");
            //    ep.Hash = r2.Match(magnet).Groups[1].Value.ToUpper();
       
        }

        private string descargarEpisodioTPB(Episodio ep, bool doble)
        {
            string magnet = "";
            string temp = (ep.Temporada < 10 ? "0" : "") + ep.Temporada;
            string cap = (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo;
            string nombreSerie = ep.Serie.Nombre;
            string calidad = "720p";

            for (int i = 0; i < 2; i++)
            {
                string pagina;
                if (!doble)
                {
                    pagina = @"https://thepiratebay.se/search/" + nombreSerie + " S" + temp + "E" + cap + " " + calidad + "/0/7/0";
                }
                else
                {
                    pagina = @"https://thepiratebay.se/search/" + nombreSerie + " S" + temp + "E" + cap + "E" + (ep.Capitulo + 1 < 10 ? "0" : "") + (ep.Capitulo + 1) +" " + calidad + "/0/7/0";
                }

                WebClient wc = new WebClient();
                string codigoFuente = wc.DownloadString(pagina);

                string patron = "<a href=\"(magnet.+?)\"";
                Regex enlaceMagent = new Regex(patron);
                Match m = enlaceMagent.Match(codigoFuente);
                if (m.Success)
                {
                    magnet = m.Groups[1].Value;
                    break;
                }
                else
                {
                    calidad = "";
                }
            }

            return magnet;
        }

        private string descargarEpisodioKickass(Episodio ep, bool doble)
        {
            string magnet = "";

            //Se construye la pagina con los datos del capitulo
            string temp = (ep.Temporada < 10 ? "0" : "") + ep.Temporada;
            string cap = (ep.Capitulo < 10 ? "0" : "") + ep.Capitulo;
            string calidad = "720p";
            string nombreSerie = ep.Serie.Nombre;

            for (int i = 0; i < 2; i++)
            {
                string pagina;
                if (!doble)
                {
                    pagina = @"http://kickass.to/usearch/" + nombreSerie + " S" + temp + "E" + cap + " " + calidad + @"/?field=seeders&sorder=desc";
                }
                else
                {
                    pagina = @"http://kickass.to/usearch/" + nombreSerie + " S" + temp + "E" + (ep.Capitulo - 1 < 10 ? "0" : "") + (ep.Capitulo - 1)+ "E" + cap + " " + calidad + @"/?field=seeders&sorder=desc";
                }
                try
                {
                    //Se saca el codigo fuente de la pagina
                    HttpDownloader fuente = new HttpDownloader(pagina, "kickass.to", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.89 Safari/537.36");
                    string codigoFuente = fuente.GetPage();

                    string patron = "did not match any documents";
                    Regex resultados = new Regex(patron);
                    if (!resultados.Match(codigoFuente).Success)
                    {
                        string patronMagnet = "<a title=\"Torrent magnet link\" href=\"(magnet.+?)\"";
                        Regex encontrarMagent = new Regex(patronMagnet);
                        Match magnetMatch = encontrarMagent.Match(codigoFuente);

                        magnet = magnetMatch.Groups[1].Value;

                        break;

                    }
                    else
                    {
                        calidad = "";
                    }
                }
                catch
                {
                    return magnet;
                }
            }

            return magnet;
        }

        public void moverEpisodioTerminado(Episodio ep)
        {
            string pathOrigen = uClient.Torrents[ep.Hash].SavePath;
            string nombreArchivo = uClient.Torrents[ep.Hash].Name;

            //Saca el path del archivo .mkv
            foreach (UTorrentAPI.File f in uClient.Torrents[ep.Hash].Files)
            {
                if (f.Path.EndsWith(".mkv") || f.Path.EndsWith(".mp4"))
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


            //Se añade al path destino el capitulo
            pathDestino += @"\Episodio " + ep.Capitulo + ".mkv";

            //Se elimina el torrent de la lista de torrents
            uClient.Torrents.Remove(ep.Hash, TorrentRemovalOptions.TorrentFile);

            //Se pone su hash en -1
            ep.Hash = "-1";

            Timer moveryDescargar = new Timer()
            {
                Interval = 1,
            };

            moveryDescargar.Elapsed += (sender, e) =>
                {
                    try
                    {

                        moveryDescargar.Interval = 400;

                        if (!System.IO.File.Exists(pathDestino))
                        {
                            //Se copia al destino
                            if (!uClient.Torrents.Contains(ep.Hash))
                            {
                                System.IO.File.Move(pathOrigen, pathDestino);
                            }
                            else
                            {
                                throw new Exception("El torrent aun no se elimina");
                            }
                        }

                        ((Timer)sender).Stop();

                        //Se descarga el subtitulo
                        descargarSubtituloSUBDB(nombreArchivo, pathDestino, ep);



                        

                        



                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                };

            moveryDescargar.Start();
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
                            if (ep.Hash != "-1" && torrentTerminado(ep.Hash))
                            {
                                if (ep.Doble)
                                {
                                    Episodio epPosterior = ep.Serie.buscarEpEnNoVistos(ep.Temporada, ep.Capitulo + 1);
                                    epPosterior.Hash = "-1";
                                    epPosterior.Estado = 2;
                                    s.Descargando--;
                                    s.PorVer++;
                                }

                                BackgroundWorker b = new BackgroundWorker();
                                b.DoWork += (sender2, e2) =>
                                    {
                                        

                                        moverEpisodioTerminado(ep);
                                    };
                                b.RunWorkerCompleted += (sender3, e3) =>
                                    {
                                        s.Descargando--;
                                        s.PorVer++;
                                        ep.Estado = 2;
                                        s.guardarEpisodiosNoVistos();   
                                    };
                                b.RunWorkerAsync();
                                                             
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
                        Interfaz.agregarDesgloseEpisodios(s, false);
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
                if (System.IO.File.Exists(@"C:\Users\Marcelo\Videos\Series\" + ep.Serie.Nombre + @"\Temporada " + ep.Temporada + @"\Episodio " + ep.Capitulo + ".mkv"))
                {
                    reproducirEpisodio(ep);
                }
                else
                {
                    if (ep.Serie.buscarEpEnNoVistos(ep.Temporada, ep.Capitulo - 1).Doble)
                    {
                        reproducirEpisodio(ep.Serie.buscarEpEnNoVistos(ep.Temporada, ep.Capitulo - 1));
                    }
                }

                while (Se.EpisodiosNoVistos.Count != 0 && Se.EpisodiosNoVistos[0] != ep)
                {
                    Se.siguienteEpisodio();
                }

                Se.siguienteEpisodio();

                if (ep.Doble)
                {
                    Se.siguienteEpisodio();
                }

            }
            else
            {
                if (ep.Estado == 3)
                {
                    List<Episodio> aDescargar = new List<Episodio>();
                    Se.AlDia = false;

                    for (int t = Se.EpisodiosVistos.Count - 1; t >= ep.Temporada - 1; t--)
                    {
                        int c = Se.EpisodiosVistos[t].Count - 1;
                        while (t == ep.Temporada - 1? c >= ep.Capitulo - 1 : c >= 0)
                        {

                            
                            Episodio epReferenciado = Se.EpisodiosVistos[t][c];
                            if (System.IO.File.Exists(@"C:\Users\Marcelo\Videos\Series\" + Se.Nombre + @"\Temporada " + epReferenciado.Temporada + @"\Episodio " + epReferenciado.Capitulo + ".mkv") || (c > 0 && Se.EpisodiosVistos[t][c-1].Doble))
                            {
                                epReferenciado.Estado = 2;
                                Se.PorVer++;
                            }
                            else
                            {
                                aDescargar.Add(epReferenciado);
                                epReferenciado.Estado = 1;                               
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

                    if (ep.Capitulo == 1)
                    {
                        if (ep.Temporada == 1)
                        {
                            Se.TemporadaUltEpisodioVisto = 0;
                            Se.CapituloUltEpisodioVisto = 0;
                        }
                        else
                        {
                            Se.TemporadaUltEpisodioVisto = ep.Temporada - 1;
                            Se.CapituloUltEpisodioVisto = Se.EpisodiosVistos[Se.TemporadaUltEpisodioVisto - 1].Count;
                        }
                    }
                    else
                    {
                        Se.TemporadaUltEpisodioVisto = ep.Temporada;
                        Se.CapituloUltEpisodioVisto = ep.Capitulo - 1;
                    }




                    Se.Temporada = ep.Temporada;
                    Se.Capitulo = ep.Capitulo;

                    BackgroundWorker b = new BackgroundWorker();
                    b.DoWork += (sender, e) =>
                        {
                            foreach (Episodio epi in aDescargar)
                            {
                                descargarEpisodio(epi);
                                Se.Descargando++;
                            }
                            Se.guardarEpisodiosNoVistos();
                            Se.guardarEpisodiosVistos();
                            guardarBasedeDatos();
                        };
                    b.RunWorkerAsync();
                    


                }
            }
        }

        public void quitarTorrent(string hash)
        {
            uClient.Torrents.Remove(hash, TorrentRemovalOptions.TorrentFileAndData);
        }

        public void eliminarSerie(int num)
        {
            Serie eliminaS = Series[num];
            checarTerminodeDescarga.Stop();

            foreach (Episodio ep in eliminaS.EpisodiosNoVistos)
            {
                if (ep.Estado == 1)
                {
                    quitarTorrent(ep.Hash);
                }
                else
                {
                    if (ep.Estado == 0)
                        break;
                }
            }

            Interfaz.eliminarSeriedeInterfaz(num);

            Series.RemoveAt(num);

            guardarBasedeDatos();

            

            System.IO.File.Delete(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\EpisodiosNoVistos\" + eliminaS.Nombre + ".txt");
            System.IO.File.Delete(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\Series\EpisodiosVistos\" + eliminaS.Nombre + ".txt");
            System.IO.File.Delete(@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\FanArt\" + eliminaS.Nombre + ".jpg");

            checarTerminodeDescarga.Start();





        }

        public void actualizarBasedeDatos()
        {
            List<Serie> seriesaActualizar = new List<Serie>(Series);

            foreach (Serie s in seriesaActualizar)
            {
                //Se carga el documento de la info de las series
                XmlDocument doc = new XmlDocument();
                doc.Load(@"http://thetvdb.com/api/97AAE7796E3F60D2/series/" + s.Id + "/all/en.xml");

                //Se pone el estado de la serie (si cambio o no)
                s.Estado = doc.SelectSingleNode("/Data/Series/Status").InnerText == "Continuing" ? 'c' : 'e';

                //Se sacan los nodos de todos los episodios
                XmlNodeList episodios = doc.SelectNodes("/Data/Episode");

                
                bool encontrado = false;  //Variable que sirve para saber si el episodio con el que se empezará la actualización ya se encontro
                int numEp = 0;           //Variable que sirve para saber el numero de episodio que se está actualizando
                //Se crea una lista de los episodios que se van a descargar; si es que hay
                List<Episodio> aDescargar = new List<Episodio>();
                //Se recorren todos los nodos de los episodios
                foreach (XmlNode e in episodios)
                {
                   
                    //Ignora todos los episodios extras
                    if (e.SelectSingleNode("SeasonNumber").InnerText != "" && e.SelectSingleNode("SeasonNumber").InnerText != "0" && e.SelectSingleNode("FirstAired").InnerText != "")
                    {
                        int temporada = int.Parse(e.SelectSingleNode("SeasonNumber").InnerText);   //Se saca la temporada del episodio
                        int capitulo = int.Parse(e.SelectSingleNode("EpisodeNumber").InnerText);  //Se saca el capitulo del episodio
                        
                        //Si no se ha encontrado el capitulo, lo busca
                        if (!encontrado)
                        {
                            if (temporada == s.TemporadaUltEpisodioVisto && capitulo == s.CapituloUltEpisodioVisto)
                            {
                                encontrado = true;
                            }
                        }
                        //Si ya se encontró
                        else
                        {
                            //Se saca su nombre
                            string nombreEp = e.SelectSingleNode("EpisodeName").InnerText;
                            //Se saca su fecha
                            DateTime fecha = convertirAFecha(e.SelectSingleNode("FirstAired").InnerText, s.Hora);
                            fecha = fecha.AddDays(0.5);

                            //Si el episodio coincide con alguno de los que ya se encuentra
                            if (numEp + 1 <= s.EpisodiosNoVistos.Count)
                            {
                                //Se saca dicho episodio
                                Episodio epAActualizar = s.EpisodiosNoVistos[numEp];

                                //Se determina si en realidad es el episodio buscado
                                if (epAActualizar.Temporada == temporada && epAActualizar.Capitulo == capitulo)
                                {
                                    epAActualizar.Fecha = fecha;
                                    epAActualizar.NombreEp = nombreEp;
                                }
                                //Si no es, se maneja una excepción
                                else
                                {
                                    MessageBox.Show("Esto no deberia de pasar\r\nEpisodioQuePareció S" + temporada + "E" + capitulo + " Episodio Real: S" + epAActualizar.Temporada + "E" + epAActualizar.Capitulo);
                                }
                            }
                            //Si el episodio no coincide con ninguno de los que ya están (es decir el numero de episodio es mayor que el total de episodios
                            else
                            {


                                //Se crea el nuevo episodio con los parametros sacados
                                Episodio epAAgregar = new Episodio(s, temporada, capitulo, nombreEp, "-1", fecha, 0, "720p", false);
                                //Se agrega a la lista de episodios
                                s.EpisodiosNoVistos.Add(epAAgregar);

                                //Si no hay ningun episodio actual
                                if (s.Temporada == 0 && s.Capitulo == 0)
                                {
                                    //Se le agrega el handler al episodio recien agregado
                                    Interfaz.getInterfazSerie(s.Numserie).agregarHandlerAEpisodioPrincipal();
                                    //Se cambia la temporada y capitulo
                                    s.Temporada = epAAgregar.Temporada;
                                    s.Capitulo = epAAgregar.Capitulo;
                                }

                                //Si ya estan cargados los episodios
                                if (s.episodiosCargados)
                                {
                                    if (epAAgregar.Capitulo == 1)
                                    {
                                        Interfaz.getInterfazSerie(s.Numserie).agregarTemporadaAlDesglose();
                                        Interfaz.getInterfazSerie(s.Numserie).Sig.Visibility = Visibility.Visible;
                                    }
                                    //Se agrega al desglose 
                                    Interfaz.getInterfazSerie(s.Numserie).agregarEpisodioAlDesglose(epAAgregar);
                                }
                                

                                //Se checa si el episodio que se va a agregar ya salió para descargarlo
                                if (DateTime.Now > epAAgregar.Fecha)
                                {
                                    s.AlDia = false;
                                    aDescargar.Add(epAAgregar);
                                }




                            }

                            numEp++;
                        }
                    }
                }

                //Se descargan todos los episodios que ya salieron
                BackgroundWorker b = new BackgroundWorker();
                b.DoWork += (sender, e2) =>
                {
                    foreach (Episodio ep in aDescargar)
                    {
                        descargarEpisodio(ep);
                        ep.Estado = 1;
                        s.Descargando++;
                    }

                    if (aDescargar.Count != 0)
                    {
                        guardarBasedeDatos();
                        s.guardarEpisodiosNoVistos();
                    }

                };
                b.RunWorkerAsync();

                if (aDescargar.Count == 0)
                {
                    guardarBasedeDatos();
                    s.guardarEpisodiosNoVistos();
                }

            } 


        }

        public void prueba()
        {
            
        }

        private DateTime convertirAFecha(string fecha, string horas)
        {
            int hora = Convert.ToInt32(horas.Split(':')[0]);
            int minutos = Convert.ToInt32(horas.Split(':')[1].Split(' ')[0]);
            hora = horas.Split(':')[1].Split(' ')[1] == "PM" ? hora += 12 : hora;
            return new DateTime(Convert.ToInt32(fecha.Substring(0, 4)), Convert.ToInt32(fecha.Substring(5, 2)), Convert.ToInt32(fecha.Substring(8, 2)), hora, minutos, 0);
        }

    }



}
