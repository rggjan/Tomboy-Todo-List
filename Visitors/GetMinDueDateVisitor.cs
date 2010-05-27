// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//   
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//   
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  
// Authors:
//       Jan Rüegg <rggjan@gmail.com>
//       Gabriel Walch <walchg@student.ethz.ch>
//       Gerd Zellweger <mail@gerdzellweger.com>
// 

using System;
using System.Collections.Generic;

namespace Tomboy.TaskManager
{

	/// <summary>
	/// Visitor that traverses the task structure for the minimum duedate of all lower level attributedtasks.
	/// If no duedate is set, the default return is the current day
	/// </summary>
	public class GetMinDueDateVisitor : Visitor
	{

		/// <summary>
		/// The private result
		/// May be DateTime.MaxValue, meaning unset (no other duedate found with lower value)
		/// </summary>
		private DateTime result;
		
		/// <summary>
		/// Public getter for the result
		/// Sets the default value
		/// </summary>
		public DateTime Result {
			get {
				if (((DateTime)result) == DateTime.MaxValue)
					return DateTime.Today;
				return ((DateTime)result);
			}
		}
		
		public GetMinDueDateVisitor ()
		{
			result = DateTime.MaxValue;
			visited = new List<AttributedTask>();
		}
		
		public override void visit (Note n)
		{
			//Again, nothing here	
		}
		
		public override void visit (TaskList tl)
		{
			visited.Add (tl);
			
			DateTime? date = tl.DueDate;
			if (date != null){
				result = ((DateTime) date) < result? (DateTime)date : result;
			}
			
			foreach (Task t in tl.Tasks)
				if (!visited.Contains (t))
					this.visit (t);
		}
		
		public override void visit (Task t)
		{
			visited.Add (t);
			
			DateTime? date = t.DueDate;
			if (date != null){
				result = ((DateTime) date) < result? (DateTime)date : result;
			}
			
			foreach (TaskList tl in t.Subtasks)
				if (!visited.Contains (tl))
					this.visit (tl);
		}
	}
}
