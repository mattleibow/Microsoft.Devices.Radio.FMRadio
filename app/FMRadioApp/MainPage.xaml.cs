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
using System.Threading;
using Windows.UI.Core;

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

		private readonly FMRadio radio;
		private double frequency;
		private double minFrequency;
		private double maxFrequency;

		private Task radioUpdateTimer;

		public MainPage()
		{
			this.InitializeComponent();

			// regions
			Regions = new ObservableCollection<RadioRegion>(Enum.GetValues(typeof(RadioRegion)).Cast<RadioRegion>());

			// the radio
			try
			{
				radio = FMRadio.Instance;
				Region = radio.CurrentRegion;
				Frequency = radio.Frequency;
				RadioOn = radio.PowerMode == RadioPowerMode.On;
			}
			catch (RadioDisabledException)
			{
				radio = null;
			}
			OnPropertyChanged();
			OnPropertyChanged(nameof(RadioSupported));

			// start binding
			DataContext = this;

			// start the timer
			radioUpdateTimer = Task.Run(OnRefreshRadio);
		}

		private async Task OnRefreshRadio()
		{
			while (true)
			{
				// we don't want to stop the UI at all
				if (RadioOn && Math.Abs(radio.Frequency - Frequency) > 0.1)
				{
					radio.Frequency = Frequency;
				}

				// update the signal UI
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged(nameof(SignalStrength)));

				// restart the timer
				await Task.Delay(500);
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

		public ObservableCollection<RadioRegion> Regions { get; }

		public RadioRegion Region
		{
			get { return radio?.CurrentRegion ?? RadioRegion.UnitedStates; }
			set
			{
				radio.CurrentRegion = value;

				MaxFrequency = frequencyLimits[Region].Item2;
				MinFrequency = frequencyLimits[Region].Item1;

				OnPropertyChanged();
			}
		}

		public double SignalStrength
		{
			get { return RadioOn ? radio.SignalStrength : 0.0; }
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
			set { frequency = CorrectFrequency(value); OnPropertyChanged(); }
		}

		private double CorrectFrequency(double value)
		{
			// round
			value = Math.Round(value, 2);

			if (Region == RadioRegion.UnitedStates)
			{
				// if the decimal is even, then move it one up
				if ((value * 10) % 2 == 0)
				{
					value += 0.1;
				}
			}

			return Math.Round(value, 2);
		}

		public bool RadioOn
		{
			get { return RadioSupported && radio.PowerMode == RadioPowerMode.On; }
			set
			{
				if (RadioSupported)
				{
					radio.PowerMode = value ? RadioPowerMode.On : RadioPowerMode.Off;
					OnPropertyChanged();
				}
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
