using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Devices.Radio;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Foundation;
using System.Windows.Input;

namespace FMRadioApp
{
	public sealed partial class MainPage : Page, INotifyPropertyChanged
	{
		private const int FrequencyScaling = 70;
		private const int HalfFrequencyScaling = 35;

		private const int ThickMarkWidth = 3;
		private const double ThickMarkLength = 0.5;
		private const int ThinMarkWidth = 1;
		private const double ThinMarkLength = 0.25;

		private double frequency;
		private bool isScrolling;

		private Task radioUpdateTimer;

		public MainPage()
		{
			this.InitializeComponent();

			SelectRegionCommand = new Command(p => Region = (RadioRegion)Enum.Parse(typeof(RadioRegion), p.ToString()));

			// regions
			Regions = new ObservableCollection<RadioRegion>(Enum.GetValues(typeof(RadioRegion)).Cast<RadioRegion>());

			// the radio
			if (!FMRadioController.RadioSupported)
			{
				// TODO: let the user know
			}
			Frequency = FMRadioController.Frequency;
			RadioOn = FMRadioController.PoweredOn;

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
				if (RadioOn && Math.Abs(FMRadioController.Frequency - Frequency) > 0.1)
				{
					FMRadioController.Frequency = Frequency;
				}

				// update the signal UI
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					OnPropertyChanged(nameof(SignalStrength));
					OnPropertyChanged(nameof(SignalBars));
				});

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

		public ICommand SelectRegionCommand { get; }

		public bool RadioSupported => FMRadioController.RadioSupported;

		public ObservableCollection<RadioRegion> Regions { get; }

		public RadioRegion Region
		{
			get { return FMRadioController.Region; }
			set
			{
				FMRadioController.Region = value;

				OnPropertyChanged(nameof(MinFrequency));
				OnPropertyChanged(nameof(MaxFrequency));
				RefreshScroller();
				Frequency = Frequency;

				OnPropertyChanged();
			}
		}

		public Symbol SignalBars
		{
			get
			{
				var sig = SignalStrength * 2;
				if (sig >= 4)
					return Symbol.FourBars;
				if (sig >= 3)
					return Symbol.ThreeBars;
				if (sig >= 2)
					return Symbol.TwoBars;
				if (sig >= 1)
					return Symbol.OneBar;
				return Symbol.ZeroBars;
			}
		}

		public double SignalStrength => FMRadioController.SignalStrength;

		public double MinFrequency => FMRadioController.MinimumFrequency;

		public double MaxFrequency => FMRadioController.MaximumFrequency;

		public string FrequencyName => "NAME";

		public double Frequency
		{
			get { return frequency; }
			set
			{
				frequency = FMRadioController.CorrectFrequency(value);
				if (!isScrolling)
				{
					scrollview.ChangeView(GetOffset(Frequency), null, null, false);
				}
				OnPropertyChanged();
			}
		}

		public bool RadioOn
		{
			get { return FMRadioController.PoweredOn; }
			set { FMRadioController.PoweredOn = value; OnPropertyChanged(); }
		}

		private double GetFrequency(double offset) => (offset / FrequencyScaling) + MinFrequency;

		private double GetOffset(double frequency) => (frequency - MinFrequency) * FrequencyScaling;

		// INotifyPropertyChanged

		private void OnPropertyChanged([CallerMemberName]string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public event PropertyChangedEventHandler PropertyChanged;


		private void OnScrollerScrolled(object sender, ScrollViewerViewChangedEventArgs e)
		{
			isScrolling = true;
			Frequency = GetFrequency(scrollview.HorizontalOffset);
			isScrolling = false;
		}

		private void RefreshScroller()
		{
			var size = (int)((MaxFrequency - MinFrequency) * FrequencyScaling);

			scrollerCanvas.Children.Clear();
			scrollerCanvas.Width = size;

			var back = ((SolidColorBrush)scrollerCanvas.Background).Color;

			// the start gradient
			var rect = new Rectangle
			{
				Fill = new LinearGradientBrush
				{
					StartPoint = new Point(0, 0),
					EndPoint = new Point(1, 0),
					GradientStops = new GradientStopCollection
					{
						new GradientStop { Color = Colors.Transparent, Offset = 0 },
						new GradientStop { Color = Colors.Transparent, Offset = 0.5 },
						new GradientStop { Color = back, Offset = 1 }
					},
				}
			};
			rect.SetValue(Canvas.TopProperty, 0);
			rect.SetValue(Canvas.LeftProperty, scrollerCanvas.Margin.Right * -2);
			rect.SetValue(Canvas.WidthProperty, scrollerCanvas.Margin.Right * 2);
			rect.SetValue(Canvas.HeightProperty, scrollerCanvas.ActualHeight);
			scrollerCanvas.Children.Add(rect);

			// the end gradient
			rect = new Rectangle
			{
				Fill = new LinearGradientBrush
				{
					StartPoint = new Point(0, 0),
					EndPoint = new Point(1, 0),
					GradientStops = new GradientStopCollection
					{
						new GradientStop { Color = back, Offset = 0 },
						new GradientStop { Color = Colors.Transparent, Offset = 0.5 },
						new GradientStop { Color = Colors.Transparent, Offset = 1 }
					},
				}
			};
			rect.SetValue(Canvas.TopProperty, 0);
			rect.SetValue(Canvas.LeftProperty, size);
			rect.SetValue(Canvas.WidthProperty, scrollerCanvas.Margin.Right * 2);
			rect.SetValue(Canvas.HeightProperty, scrollerCanvas.ActualHeight);
			scrollerCanvas.Children.Add(rect);

			// start, and end, with a few lines
			int x = (int)(((int)MinFrequency - MinFrequency) * FrequencyScaling) - FrequencyScaling;
			while (x < size + FrequencyScaling + FrequencyScaling)
			{
				// the text is only in the valid range
				if (x >= 0 && x <= size)
				{
					var freq = new Border
					{
						BorderBrush = null,
						Child = new TextBlock
						{
							Text = GetFrequency(x).ToString("0"),
							VerticalAlignment = VerticalAlignment.Center,
							HorizontalAlignment = HorizontalAlignment.Center
						}
					};
					freq.SetValue(Canvas.TopProperty, 0);
					freq.SetValue(Canvas.LeftProperty, x - (FrequencyScaling / 2));
					freq.SetValue(Canvas.WidthProperty, FrequencyScaling);
					freq.SetValue(Canvas.HeightProperty, scrollerCanvas.ActualHeight * ThickMarkLength);
					scrollerCanvas.Children.Add(freq);
				}

				// the main line
				scrollerCanvas.Children.Add(new Line
				{
					X1 = x,
					X2 = x,
					Y1 = scrollerCanvas.ActualHeight * (1 - ThickMarkLength),
					Y2 = scrollerCanvas.ActualHeight,
					Stroke = scrollview.Foreground,
					StrokeThickness = ThickMarkWidth
				});

				// the half line
				scrollerCanvas.Children.Add(new Line
				{
					X1 = x - HalfFrequencyScaling,
					X2 = x - HalfFrequencyScaling,
					Y1 = scrollerCanvas.ActualHeight * (1 - ThinMarkLength),
					Y2 = scrollerCanvas.ActualHeight,
					Stroke = scrollview.Foreground,
					StrokeThickness = ThinMarkWidth
				});

				x += FrequencyScaling;
			}

			// the last half line
			scrollerCanvas.Children.Add(new Line
			{
				X1 = x - HalfFrequencyScaling,
				X2 = x - HalfFrequencyScaling,
				Y1 = scrollerCanvas.ActualHeight * (1 - ThinMarkLength),
				Y2 = scrollerCanvas.ActualHeight,
				Stroke = scrollview.Foreground,
				StrokeThickness = ThinMarkWidth
			});
		}

		private void OnScrollviewLayout(object sender, object e)
		{
			var margin = scrollview.ActualWidth / 2;
			scrollerCanvas.Margin = new Thickness(margin, 0, margin, 0);
		}

		private void OnScrollerLoaded(object sender, RoutedEventArgs e)
		{
			RefreshScroller();
			Frequency = Frequency;
		}
	}
}
