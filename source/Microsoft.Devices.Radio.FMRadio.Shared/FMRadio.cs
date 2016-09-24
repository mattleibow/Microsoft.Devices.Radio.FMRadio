using System;

namespace Microsoft.Devices.Radio
{
	public sealed class FMRadio
	{
		private static FMRadio instance;

		private RadioPowerMode powerMode;

		static FMRadio()
		{
			uint enabled;
			try
			{
				RadioApiNativeMethods.MediaApi_GetRadioEnabled(out enabled);
			}
			catch (DllNotFoundException)
			{
				// radioapi.dll may not exist, such as on desktop
				enabled = 0;
			}

			// create an instance if we can
			instance = enabled == 0 ? null : new FMRadio();
		}

		private FMRadio()
		{
			uint playing;
			CurrentRegion = RadioRegion.UnitedStates;
			RadioApiNativeMethods.MediaApi_GetRadioPlaying(out playing);
			powerMode = (playing == 0 ? RadioPowerMode.Off : RadioPowerMode.On);
		}

		public static FMRadio Instance
		{
			get
			{
				if (instance == null)
				{
					throw new RadioDisabledException();
				}
				return instance;
			}
		}

		public RadioRegion CurrentRegion { get; set; }

		public double Frequency
		{
			get
			{
				uint khz;
				RadioApiNativeMethods.MediaApi_GetRadioFrequency(out khz);
				return khz / 1000.0;
			}
			set
			{
				if (value < 0 || value > 1000)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}
				uint khz = (uint)(value * 1000);
				RadioApiNativeMethods.MediaApi_TuneRadio((uint)CurrentRegion, khz);
			}
		}

		public RadioPowerMode PowerMode
		{
			get
			{
				return powerMode;
			}
			set
			{
				powerMode = value;
				RadioApiNativeMethods.MediaApi_EnableRadio((uint)powerMode);
			}
		}

		public double SignalStrength
		{
			get
			{
				uint maxRssi;
				uint rssi;
				RadioApiNativeMethods.MediaApi_GetRadioQuality(out rssi, out maxRssi);
				if (rssi == 0 || maxRssi == 0)
				{
					return 0;
				}
				return (double)rssi / maxRssi;
			}
		}
	}
}
