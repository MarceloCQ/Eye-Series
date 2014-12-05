using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EyeSeries
{
    class Controlador
    {
        public List<Serie> Series;

        private DateTime UltimaActualizacion;
        private bool Actualizar;
        private MainWindow Interfaz;

        public Controlador(MainWindow i)
        {
            Series = new List<Serie>();
            Interfaz = i;
        }

        public void AlimentarBasedeDatos()
        {

            //Se abre el archivo de donde se van a leer los datos
            string archivoAbrir = @"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Bases de Datos\General.txt";
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
                Serie s = new Serie(id, nombre, i, estado, hora, subid);
                Series.Add(s);
                leer.ReadLine();
                i++;
            }


        }

        public void AlimentarBasedeDatosCapitulosNoVistos()
        {
            foreach (Serie s in Series)
            {
                
                s.AlimentarEpisodiosNoVistos();
                
            }
                    
        }

    }
}
