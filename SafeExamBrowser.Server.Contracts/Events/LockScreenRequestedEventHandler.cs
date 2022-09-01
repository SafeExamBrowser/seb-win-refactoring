using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeExamBrowser.Server.Contracts.Events
{
	/// <summary>
	/// Event handler used to handle a requested lock screen from SEB Server
	/// </summary>
	public delegate void LockScreenRequestedEventHandler(String message);
	
}
