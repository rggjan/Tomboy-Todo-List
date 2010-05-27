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
	/// Visitor that checks upwards whether the changes in the level below (task -> subtask or tasklist -> task)
	/// Made the corresponding task complete (in terms of every child is done) or incomplete
	/// The correspondonding policies can be adjusted via the Task class
	/// </summary>
	public class CheckDoneVisitor : Visitor
	{
		private bool done;
	
		/// <summary>
		/// Expects whether we are checking for 'new' tasks/tasklists to mark as done or not done
		/// </summary>
		/// <param name="done">
		/// A <see cref="System.Boolean"/>
		/// </param>
		public CheckDoneVisitor (bool done)
		{
			visited = new List<AttributedTask> ();
			this.done = done;
		}
		
		public override void visit (Note n)
		{
			//Not needed here	
		}
		
		public override void visit (TaskList taskList)
		{
			visited.Add (taskList);
			
			//Can't check here because do not know about other subtasks of the supertasks
			foreach (Task task in taskList.SuperTasks)
				if (!visited.Contains (task))
					this.visit (task);
		}
		
		public override void visit (Task task)
		{
			visited.Add (task);
			/*If checking upward for freshly completed tasks, check whether all subtasks have been done
			 *Assume all values have been propagated - change only if this task matters
			 * x && true = x
			 */
			if (done && !task.Done){
				if (task.Subtasks.FindAll (c => c.Done==true).Count == task.Subtasks.Count)
					if (task.Done == false) task.Toggle ();
			
				if (!visited.Contains (task.ContainingTaskList))
					this.visit (task.ContainingTaskList);
			}
			
			/* If checking upward for freshly 'not completed anymore tasks', need no check.
			 * x && false = false
			 */
			if (!done && task.Done){
				task.Toggle ();
				if (!visited.Contains (task.ContainingTaskList))
					this.visit (task.ContainingTaskList);
			}
		}
	}
}
