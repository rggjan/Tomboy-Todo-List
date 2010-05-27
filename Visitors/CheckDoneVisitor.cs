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
	public class CheckDoneVisitor : Visitor
	{
		private List<AttributedTask> visited;
	
		public CheckDoneVisitor ()
		{
			visited = new List<AttributedTask> ();
		}
		
		public void visit (Note n)
		{
			//Not needed here	
		}
		
		public void visit (TaskList tl)
		{
			visited.Add (tl);
			
			//Can't check here because do not know about other subtasks of the supertasks
			foreach (Task t in tl.SuperTasks)
				if (!visited.Contains (t))
					visit (t);
		}
		
		public void visit (Task t)
		{
			visited.Add (t);
			//Assume that subtasks have propagated their value before
			if (t.Subtasks.FindAll (c => c.Done==true).Count == t.Subtasks.Count)
				if (t.Done == false) t.Toggle ();
			
			if (!visited.Contains (t.ContainingTaskList))
				visit (t.ContainingTaskList);
		}
	}
}
