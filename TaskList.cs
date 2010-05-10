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

namespace Tomboy.TaskManager {

	/// <summary>
	/// A task list is a collection of tasks grouped together.
	/// It may have a title, a priority and a due date.
	/// </summary>
	public class TaskList : AttributedTask {
	

		/// <summary>
		/// Name of this task list
		/// </summary>
		public string Name {
			get;
			set;
		}
		
		/// <summary>
		/// Note containing the TaskList.
		/// </summary>
		internal Note ContainingNote {
			get; set;
		}
		
		private NoteBuffer Buffer {
			get { return ContainingNote.Buffer; }
		}
		
		/// <summary>
		/// Children for ITask interface
		/// </summary>
		public override List<AttributedTask> Children {
			get; set;
		}
		
		/// <summary>
		/// Containers for ITask interface
		/// </summary>
		public override List<AttributedTask> Containers { 
			get; set;
		}
		
		//TODO
		public bool Done { 
			get; set; 
		}
		
		private TaskListTag Tag {
			get; set;
		}
		
		private Gtk.TextMark Start {
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
			ContainingNote = note;
			Name = ("New TaskList!");
			
			Tag = (TaskListTag)ContainingNote.TagTable.CreateDynamicTag ("tasklist");
			Tag.bind (this);
			
			var insertAt = Buffer.GetIterAtMark (Buffer.InsertMark);
			
			insertAt.BackwardChar ();
			if (insertAt.Char != System.Environment.NewLine)
				Buffer.InsertAtCursor (System.Environment.NewLine);
			
			Start = Buffer.CreateMark (null, insertAt, true);
			Buffer.InsertAtCursor ("New TaskList!\n");
			
			Logger.Debug ("TaskList created");
			
			// registring buffer event handlers
			
			Children = new List<AttributedTask> ();

			addTask (ContainingNote.Buffer.InsertMark);
		}
		
		/// <summary>
		/// Creates a new Task and add it to the `Tasks` list.
		/// </summary>
		/// <param name="at">
		/// <see cref="Gtk.TextMark"/> Where to add the task in the Buffer.
		/// </param>
		public void addTask (Gtk.TextMark at)
		{
			var insertIter = Buffer.GetIterAtMark (at);
			// go to beginning of the line
			insertIter.LineOffset = 0;

			Children.Add (new Task (this, Buffer.CreateMark (null, insertIter, true)));
			
			var startList = Buffer.GetIterAtMark(Start);
			var endList = Buffer.GetIterAtMark(Buffer.InsertMark);
			ContainingNote.Buffer.ApplyTag(Tag, startList, endList);
		}

		

	}
}
