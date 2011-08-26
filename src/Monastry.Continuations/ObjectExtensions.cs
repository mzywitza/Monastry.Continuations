using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monastry.Continuations
{
	public static class ObjectExtensions
	{
		public static T When<T>(this T subject, Func<T, bool> tester) where T : class
		{
			return (subject != null && tester(subject)) ? subject : null;
		}

		public static T Do<T>(this T subject, Action<T> worker) where T : class
		{
			if (subject == null) return null;
			worker(subject);
			return subject;
		}
	}
}
