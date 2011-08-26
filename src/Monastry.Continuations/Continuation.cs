using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monastry.Continuations
{
	public class Continuation<T> where T : class
	{
		protected Func<T, T> Operation;

		public Continuation(Func<T,T> operation)
		{
			Operation = operation;
		}

		public virtual T Execute(T subject)
		{
			if (subject == null) return null;
			return Operation(subject);
		}
	}
}
