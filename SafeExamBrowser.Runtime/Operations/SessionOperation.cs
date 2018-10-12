using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;

namespace SafeExamBrowser.Runtime.Operations
{
	/// <summary>
	/// The base implementation to be used for all operations in the session operation sequence.
	/// </summary>
	internal abstract class SessionOperation : IRepeatableOperation
	{
		protected SessionContext Context { get; private set; }

		public abstract event ActionRequiredEventHandler ActionRequired;
		public abstract event StatusChangedEventHandler StatusChanged;

		public SessionOperation(SessionContext sessionContext)
		{
			Context = sessionContext;
		}

		public abstract OperationResult Perform();
		public abstract OperationResult Repeat();
		public abstract OperationResult Revert();
	}
}
