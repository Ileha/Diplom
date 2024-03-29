﻿using System;
namespace UiserClient.Commands
{
    public class KeyNotFoundException : Exception
    {
        public KeyNotFoundException(char key) : base(String.Format("not found {0}", key))
        {}
    }
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
