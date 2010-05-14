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
using Tomboy;
using Gtk;

namespace Tomboy.TaskManager
{


	public class TaskNoteParser
	{

		public TaskNoteParser ()
		{
		}
		
		public static List<TaskList> ParseTasks(Note note)
		{
			List<TaskList> tls = new List<TaskList>();
			
			TextIter iter = note.Buffer.StartIter;
			do {
				TaskListTag taskliststart = (TaskListTag)note.Buffer.GetDynamicTag ("tasklist", iter);
				if (taskliststart != null)
				{
					//Logger.Debug ("=> found Tasklist!");

					TaskList tl = new TaskList (note, iter, taskliststart);
					tls.Add (tl);
					
					TaskTag start;
					do
					{
						start = (TaskTag)note.Buffer.GetDynamicTag ("task", iter);
						iter.ForwardChar ();
					} while (start == null);
					
					//Logger.Debug ("=> found Tasktag!");
					tl.addTask (iter, start);
					
					TaskTag end = start;
					while (end == start) {
						iter.ForwardChar ();
						end = (TaskTag)note.Buffer.GetDynamicTag ("task", iter);
					}
				}
			} while (iter.ForwardChar ());
			
			return tls;
		}
	}
}
