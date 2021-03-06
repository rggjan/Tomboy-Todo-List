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
	/// Traverses the tasks structure recursively to either set the tasks to done or not done
	/// </summary>
	public class SetDoneVisitor : Visitor
	{
		
		private bool done;
		private AttributedTask sender;
		public SetDoneVisitor (bool done, Task sender)
		{
			this.done = done;
			this.sender = sender;
			visited = new List<AttributedTask> ();
		}
		
		public override void visit (Note note)
		{
			//Should not be used
			//Go directly from task to subtasks (which is list of tasklist)
		}
		
		public override void visit (TaskList taskList)
		{
			if (visited.Contains (taskList))
				return;
			
			visited.Add (taskList);
			foreach (Task task in taskList.Tasks)
				if (!visited.Contains (task))
					this.visit (task);
		}
		
		public override void visit (Task task)
		{	
			if(task == sender)
				task.TagUpdate ();
			else if (!task.Done == done)
				task.Toggle ();
			
			visited.Add (task);
			
			foreach (TaskList taskList in task.Subtasks)
				if (!visited.Contains (taskList))
					this.visit (taskList);
		}
	}
}
