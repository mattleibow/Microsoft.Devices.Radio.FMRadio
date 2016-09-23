using System.Runtime.InteropServices;

namespace Microsoft.Devices.Radio
{
	internal static class RadioApiNativeMethods
	{
		private const string RadioApiFileName = "radioapi.dll";

		[DllImport(RadioApiFileName, CharSet = CharSet.Unicode, ExactSpelling = false, PreserveSig = false)]
		public static extern void MediaApi_EnableRadio(uint powerMode);

		[DllImport(RadioApiFileName, CharSet = CharSet.Unicode, ExactSpelling = false, PreserveSig = false)]
		public static extern void MediaApi_GetRadioEnabled(out uint fEnabled);

		[DllImport(RadioApiFileName, CharSet = CharSet.Unicode, ExactSpelling = false, PreserveSig = false)]
		public static extern void MediaApi_GetRadioFrequency(out uint KHz);

		[DllImport(RadioApiFileName, CharSet = CharSet.Unicode, ExactSpelling = false, PreserveSig = false)]
		public static extern void MediaApi_GetRadioPlaying(out uint fPlaying);

		[DllImport(RadioApiFileName, CharSet = CharSet.Unicode, ExactSpelling = false, PreserveSig = false)]
		public static extern void MediaApi_GetRadioQuality(out uint currentRssi, out uint maxRssi);

		[DllImport(RadioApiFileName, CharSet = CharSet.Unicode, ExactSpelling = false, PreserveSig = false)]
		public static extern void MediaApi_TuneRadio(uint region, uint KHz);
	}
}
