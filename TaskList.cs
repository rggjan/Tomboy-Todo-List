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
		
		public override void TagUpdate ()
		{
			//TODO
		}

		
		/// <summary>
		/// Note containing the TaskList.
		/// </summary>
		internal Note ContainingNote {
			get; set;
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
		
		/// <summary>
		/// Describes what to do when tasklist is marked as done
		/// </summary>
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
		
		/// <summary>
		/// Shortcut for attached tag
		/// </summary>
		private TaskListTag TaskListTag {
			get{ return (TaskListTag) Tag; }
			set{ Tag = value; }
		}
		
		//TODO
		protected override TextIter End {
			get {return Start;}
		}

		/// <summary>
		/// Sets up the TaskList at cursor position.
		/// </summary>
		/// <param name="note">
		/// <see cref="Note"/> where the TaskLists is located.
		/// </param>
		public TaskList (Note note)
		{
			//TODO: rewrite. looks a bit ugly everything...
			ContainingNote = note;
			Name = ("New TaskList!");		
			
			TaskListTag tag = (TaskListTag)ContainingNote.TagTable.CreateDynamicTag ("tasklist");
			TextIter iter;
			NoteBuffer buffer = note.Buffer;
			
			int line = buffer.GetIterAtMark (buffer.InsertMark).Line;
			var linestart = buffer.GetIterAtLine (line);
			var lineend = linestart;
			lineend.ForwardToLineEnd ();
			
			if (buffer.GetText (linestart, lineend, false).Trim ().Length == 0)
				iter = linestart;
			else
			{
				buffer.Insert (ref lineend, System.Environment.NewLine);
				iter = lineend;
			}
			
			Initialize (buffer, iter, tag);
			
			var end = Start;
			buffer.Insert (ref end, "New Tasklist!\n\n");
			var start = Start;
			
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
			
			Initialize (note.Buffer, start, tag);
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
			Children.Add (new Task (this, position));
		}

		public void addTask (Gtk.TextIter position, TaskTag tag)
		{
			Children.Add (new Task (this, position, tag));
		}
	}
}
