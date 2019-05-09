using System;
namespace UiserClient
{
	public class EmptyKeyException : Exception {
		public EmptyKeyException() {
		
		}
	}
	public class BadInputException : Exception
	{
		public string message;

		public BadInputException(string message) {
			this.message = message;
		}

		public override string ToString() {
			return string.Format("Bad Input Exception: {0}", message);
		}
	}
}
