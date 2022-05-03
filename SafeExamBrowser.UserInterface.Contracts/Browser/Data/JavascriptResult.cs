using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeExamBrowser.UserInterface.Contracts.Browser.Data
{
	/// <summary>
	/// The data resulting from a Javascript expression evaluation.
	/// </summary>
	public class JavascriptResult
	{
		/// <summary>
		/// Indicates if the Javascript was evaluated successfully or not.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// The error message, in case of an unsuccessful evaluation of the Javascript expression.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The data item returned by the Javascript expression.
		/// </summary>
		public dynamic Result { get; set; }
	}
}
