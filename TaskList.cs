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
		
		public override bool Done { 
			get {
				if (Children != null)
					return Children.FindAll(c => c.Done == true).Count == Children.Count;
				else
					return true;
			}
			set {
				Children.ForEach(c => c.Done = true);
			}
		}
		
		private TaskListTag TaskListTag {
			get{ return (TaskListTag) Tag; }
			set{ Tag = value; }
		}
		
		
		private Gtk.TextMark Start {
			get; set;
		}

		/// <summary>
		/// Sets up the TaskList at cursor position.
		/// </summary>
		/// <param name="note">
		/// <see cref="Note"/> where the TaskLists is located.
		/// </param>
		public TaskList (Note note)
		{
			ContainingNote = note;
			Name = ("New TaskList!");
			
			TaskListTag = (TaskListTag)ContainingNote.TagTable.CreateDynamicTag ("tasklist");
			TaskListTag.bind (this);
			
			int line = Buffer.GetIterAtMark (Buffer.InsertMark).Line;
			var linestart = Buffer.GetIterAtLine (line);
			var lineend = linestart;
			lineend.ForwardToLineEnd ();
			
			if (Buffer.GetText (linestart, lineend, false).Trim ().Length == 0)
				Start = Buffer.CreateMark (null, linestart, true);
			else
			{
				Buffer.Insert (ref lineend, System.Environment.NewLine);
				Start = Buffer.CreateMark (null, lineend, true);
			}
			
			var end = Buffer.GetIterAtMark (Start);
			Buffer.Insert (ref end, "New Tasklist!\n\n");
			var start = Buffer.GetIterAtMark (Start);
			
			Buffer.ApplyTag (TaskListTag, start, end);
			start = end; // FIXME do this when loading
			start.BackwardChar ();
			Buffer.ApplyTag ("locked", start, end);
			
			Logger.Debug ("TaskList created");
			//Logger.Debug (iter.Char.ToString());
			
			Children = new List<AttributedTask> ();
			end.BackwardChar ();
			addTask (end);
		}
		
		public TaskList (Note note, Gtk.TextIter start, TaskListTag tag)
		{
			ContainingNote = note;
			Name = ("New TaskList!");//FIXME
			
			TaskListTag = tag;
			TaskListTag.TaskList = this;
			Start = Buffer.CreateMark (null, start, true);
			Logger.Debug ("TaskList created");
			
			Children = new List<AttributedTask> ();
		}
		
		/// <summary>
		/// Creates a new Task and add it to the `Tasks` list.
		/// </summary>
		/// <param name="at">
		/// <see cref="Gtk.TextMark"/> Where to add the task in the Buffer.
		/// </param>
		public void addTask (Gtk.TextIter position)
		{
			//Buffer.PlaceCursor (position);
			Children.Add (new Task (this, position));
			//var insertIter = Buffer.GetIterAtMark (at);
			// go to beginning of the line
			/*insertIter.LineOffset = 0;
		

			var startList = Buffer.GetIterAtMark (Start);
			var endList = Buffer.GetIterAtMark (Buffer.InsertMark);
			ContainingNote.Buffer.ApplyTag (Tag, startList, endList);
			Logger.Debug(Buffer.GetText(startList, endList, false));
			 */
			//Children.Add (new Task (this, Buffer.CreateMark (null, insertIter, true)));
		}

		public void addTask (Gtk.TextIter position, TaskTag tag)
		{
			//Buffer.PlaceCursor (position);
			Children.Add (new Task (this, position, tag));
		}
	}
}
