using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FMRadioApp
{
	public class FrequencyFormatter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null || parameter == null)
				return value;

			return string.Format(parameter.ToString(), value.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
				return default(double);

			return double.Parse(value.ToString());
		}
	}
}
