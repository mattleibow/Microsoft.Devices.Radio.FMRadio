using System;

namespace Microsoft.Devices.Radio
{
	public class RadioDisabledException : Exception
	{
		public RadioDisabledException()
			: base("The radio is not enabled for your system")
		{
		}
	}
}
