using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Devices.Radio;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;

namespace FMRadioApp
{
	public sealed partial class MainPage : Page, INotifyPropertyChanged
	{
		private static readonly Dictionary<RadioRegion, Tuple<double, double>> frequencyLimits =
			new Dictionary<RadioRegion, Tuple<double, double>>
			{
				{ RadioRegion.Europe, new Tuple<double, double>(87.5, 108.0) },
				{ RadioRegion.UnitedStates, new Tuple<double, double>(87.9, 107.9) },
				{ RadioRegion.Japan, new Tuple<double, double>(76.0, 95.0) },
			};

		private FMRadio radio;
		private RadioRegion? region;
		private double frequency;
		private double minFrequency;
		private double maxFrequency;

		public MainPage()
		{
			this.InitializeComponent();

			Regions = new ObservableCollection<RadioRegion>(Enum.GetValues(typeof(RadioRegion)).Cast<RadioRegion>());

			DataContext = this;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			// set up the radio
			if (radio == null)
			{
				GetRadio();
			}
		}

		private void GetRadio()
		{
			try
			{
				Radio = FMRadio.Instance;
			}
			catch (RadioDisabledException)
			{
				Radio = null;
			}
		}

		//private async Task FetchRegionAsync()
		//{
		//	var access = await Geolocator.RequestAccessAsync();
		//	if (access == GeolocationAccessStatus.Allowed)
		//	{
		//		var locator = new Geolocator
		//		{
		//			DesiredAccuracy = PositionAccuracy.Default
		//		};
		//		try
		//		{
		//			var position = await locator.GetGeopositionAsync(TimeSpan.FromHours(6), TimeSpan.FromSeconds(10));
		//			var locations = await MapLocationFinder.FindLocationsAtAsync(position.Coordinate.Point);
		//			if (locations.Status == MapLocationFinderStatus.Success)
		//			{
		//				var location = locations.Locations.FirstOrDefault();
		//				switch (location.Address.Country)
		//				{
		//					case "United States":
		//						Region = RadioRegion.UnitedStates;
		//						break;
		//					case "Japan":
		//						Region = RadioRegion.Japan;
		//						break;
		//					default:
		//						Region = RadioRegion.Europe;
		//						break;
		//				}
		//			}
		//		}
		//		catch
		//		{
		//			// we just fall back to the default
		//		}
		//	}
		//}

		// properties

		public bool RadioSupported
		{
			get { return radio != null; }
		}

		public FMRadio Radio
		{
			get { return radio; }
			set
			{
				radio = value;
				Region = radio.CurrentRegion;
				OnPropertyChanged();
				OnPropertyChanged(nameof(RadioSupported));
			}
		}

		public ObservableCollection<RadioRegion> Regions { get; }

		public RadioRegion Region
		{
			get { return region ?? RadioRegion.UnitedStates; }
			set
			{
				region = value;

				MaxFrequency = frequencyLimits[Region].Item2;
				MinFrequency = frequencyLimits[Region].Item1;
				OnPropertyChanged(nameof(Frequency));

				OnPropertyChanged();
			}
		}

		public double SignalStrength
		{
			get { return radio?.SignalStrength ?? 0.0; }
		}

		public double MinFrequency
		{
			get { return minFrequency; }
			set { minFrequency = value; OnPropertyChanged(); }
		}

		public double MaxFrequency
		{
			get { return maxFrequency; }
			set { maxFrequency = value; OnPropertyChanged(); }
		}

		public double Frequency
		{
			get { return frequency; }
			set
			{
				frequency = value;
				if (RadioOn)
				{
					radio.Frequency = frequency;
				}
				OnPropertyChanged();
				OnPropertyChanged(nameof(SignalStrength));
			}
		}

		public bool RadioOn
		{
			get { return RadioSupported && radio.PowerMode == RadioPowerMode.On; }
			set
			{
				if (RadioSupported)
				{
					radio.PowerMode = value ? RadioPowerMode.On : RadioPowerMode.Off;
					if (value)
					{
						radio.Frequency = frequency;
					}
				}
				OnPropertyChanged();
				OnPropertyChanged(nameof(SignalStrength));
			}
		}

		// INotifyPropertyChanged

		private void OnPropertyChanged([CallerMemberName]string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
