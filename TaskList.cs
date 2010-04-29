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
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager
{

	/// <summary>
	/// A task list is a collection of tasks grouped together.
	/// It may have a title, a priority and a due date.
	/// </summary>
	public class TaskList
	{
		
		/// <summary>
		/// Note containing the TaskList.
		/// </summary>
		internal Note Note {
			get; set;
		}
		
		internal List<Task> Tasks {
			get; set;
		}
	
		/// <summary>
		/// Sets up the TaskList.
		/// </summary>
		/// <param name="note">
		/// <see cref="Note"/> where the TaskLists is located.
		/// </param>
		public TaskList (Note note)
		{
			Logger.Debug("TaskList created");
			Note = note;
			
			Tasks = new List<Task>();
			addTask(Note.Buffer.InsertMark);
		}
		
		/// <summary>
		/// Creates a new Task and add it to the `Tasks` list.
		/// </summary>
		/// <param name="at">
		/// <see cref="Gtk.TextMark"/> Where to add the task in the Buffer.
		/// </param>
		public void addTask(Gtk.TextMark at) {
			var insertIter = Note.Buffer.GetIterAtMark(at);
			insertIter.BackwardChars (insertIter.LineOffset); // go to beginning of the line

			Tasks.Add(new Task(this, Note.Buffer.CreateMark (null, insertIter, true)));
		}

		
	}

	/// <summary>
	/// Marks a TaskList in a NoteBuffer. Currently this only sets the background to green for
	/// better debugging.
	/// </summary>
	public class TaskListTag : NoteTag
	{
		public static String NAME = "tasklist";
		
		public TaskListTag () : base(TaskListTag.NAME)
		{
			Background = "green";
			LeftMargin = 3;
			LeftMarginSet = true;
			CanSerialize = false;
			CanSpellCheck = true;
		}
	}


}
