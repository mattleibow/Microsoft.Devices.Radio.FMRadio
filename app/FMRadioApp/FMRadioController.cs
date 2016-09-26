using System;
using System.Collections.Generic;
using Microsoft.Devices.Radio;

namespace FMRadioApp
{
	public static class FMRadioController
	{
		private static readonly Dictionary<RadioRegion, Tuple<double, double>> frequencyLimits =
			new Dictionary<RadioRegion, Tuple<double, double>>
			{
				{ RadioRegion.Europe, new Tuple<double, double>(87.5, 108.0) },
				{ RadioRegion.UnitedStates, new Tuple<double, double>(87.9, 107.9) },
				{ RadioRegion.Japan, new Tuple<double, double>(76.0, 95.0) },
			};

		private static readonly FMRadio radio;
		private static double frequency;

		static FMRadioController()
		{
			try
			{
				radio = FMRadio.Instance;
				frequency = radio.Frequency;
			}
			catch (RadioDisabledException)
			{
				radio = null;
			}
		}

		public static bool RadioSupported => radio != null;

		public static RadioRegion Region
		{
			get { return radio?.CurrentRegion ?? RadioRegion.UnitedStates; }
			set
			{
				if (RadioSupported)
				{
					radio.CurrentRegion = value;
				}
			}
		}

		public static bool PoweredOn
		{
			get { return radio?.PowerMode == RadioPowerMode.On; }
			set
			{
				if (RadioSupported)
				{
					radio.PowerMode = value ? RadioPowerMode.On : RadioPowerMode.Off;
					if (PoweredOn)
					{
						radio.Frequency = Frequency;
					}
				}
			}
		}

		public static double Frequency
		{
			get { return PoweredOn ? radio.Frequency : frequency; }
			set
			{
				value = CorrectFrequency(value);

				if (frequency != value)
				{
					frequency = value;

					if (PoweredOn)
					{
						radio.Frequency = value;
					}
				}
			}
		}

		public static double MinimumFrequency => frequencyLimits[Region].Item1;

		public static double MaximumFrequency => frequencyLimits[Region].Item2;

		public static double SignalStrength => radio?.SignalStrength ?? 0.0;

		public static double CorrectFrequency(double frequency)
		{
			// clamp between min and max
			frequency = Math.Min(Math.Max(MinimumFrequency, frequency), MaximumFrequency);

			// round
			frequency = Math.Round(frequency, 1);

			if (Region == RadioRegion.UnitedStates)
			{
				// if the decimal is even, then move it one up
				if ((frequency * 10) % 2 == 0)
				{
					frequency += 0.1;
				}
			}

			return Math.Round(frequency, 1);
		}
	}
}
