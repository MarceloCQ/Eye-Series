using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace EyeSeries
{
    public class NumberToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int numero = (int)value;
            switch (numero)
            {
                case 0:
                    return new BitmapImage(new Uri
                                   (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Clockb2.png"));
                case 1:
                    return new BitmapImage(new Uri
                                   (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\Descb.png"));
                case 2:
                    return new BitmapImage(new Uri
                                    (@"C:\Users\Marcelo\Documents\Eye-Series\EyeSeries\EyeSeries\Interfaz\play.png"));
                default:
                    return null;

            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
