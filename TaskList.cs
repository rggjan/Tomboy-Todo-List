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
	
		bool new_task_needed = false;
		TaskList current_task = null;
		
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
		
		/// <summary>
		/// Sets up the TaskList.
		/// </summary>
		/// <param name="note">
		/// <see cref="Note"/> where the TaskLists is located.
		/// </param>
		public TaskList (Note note, Gtk.TextIter insertAt)
		{
			ContainingNote = note;

			insertAt.BackwardChar ();
			if (insertAt.Char != System.Environment.NewLine)
				Buffer.InsertAtCursor (System.Environment.NewLine);	
			Buffer.InsertAtCursor ("New TaskList!\n");
			
			Logger.Debug ("TaskList created");
			
			// registring buffer event handlers
			Buffer.UserActionEnded += CheckIfNewTaskNeeded;
			Buffer.InsertText += BufferInsertText;
			Buffer.MarkSet += BufferMarkSet;
			
			Children = new List<AttributedTask> ();	
			
			Tag = (TaskListTag) ContainingNote.TagTable.CreateDynamicTag ("tasklist");
			Tag.bind(this);
			
			//var insertIter = ContainingNote.Buffer.GetIterAtMark(ContainingNote.Buffer.InsertMark);
			//var endIter = ContainingNote.Buffer.EndIter;
			//Logger.Debug("applying tag...");
			//ContainingNote.Buffer.ApplyTag(Tag, insertIter, endIter);
			
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
			var insertIter = ContainingNote.Buffer.GetIterAtMark (at);
			// go to beginning of the line
			insertIter.LineOffset = 0;

			Children.Add (new Task (this, ContainingNote.Buffer.CreateMark (null, insertIter, true)));
		}

		
		void CheckIfNewTaskNeeded (object sender, System.EventArgs args)
		{
			if (new_task_needed) {
				Logger.Debug ("Adding a new Task");
				
				if (current_task == null) {
					//Logger.Debug ("Deleting stuff");
					Gtk.TextIter start = Buffer.GetIterAtMark (Buffer.InsertMark);
					start.BackwardLine ();
					
					Gtk.TextIter end = start;
					end.ForwardChars (2);
					
					//TODO: Use the rest of this line as the title of the new task list
					
					// Logger.Debug(Buffer.GetText(start, end, false));
					Buffer.Delete (ref start, ref end);
					
					//Children.Add (new TaskList (Note));
				} else {
					current_task.addTask (Buffer.InsertMark);
				}
				new_task_needed = false;
			}
		}
	
		void BufferMarkSet (object o, EventArgs args)
		{
		}
		
		void BufferInsertText (object o, Gtk.InsertTextArgs args)
		{
			if (args.Text == System.Environment.NewLine)
			{
				Gtk.TextIter end = args.Pos;
				end.BackwardChar ();

				var begin = end;
				begin.LineOffset = 0;
				
				if (Buffer.GetText (begin, end, false).Trim ().Length == 0)
				{
					//FIXME delete task!
				}
				
				end.BackwardChar();
				
				// Go back to last char on line that is not a newline
				foreach (Gtk.TextTag tag in end.Tags)
				{
					//Edit: Wow. Now this looks pretty!
					if (tag is TaskTag)
					{
						Logger.Debug ("TaskTag found!");
						
						TaskTag tasktag = (TaskTag) tag;
						current_task = tasktag.Task.ContainingTaskList;

						new_task_needed = true;
						return;
					}
				}
				
				end = args.Pos;
				end.ForwardChars (5);
				
				Gtk.TextIter start = args.Pos;
				start.BackwardLine ();
				
				end = start;
				end.ForwardChars (2);
				
				//Logger.Debug ("Before new Line: "+Buffer.GetText(start, end, false));
				
				if (IsTextTodoItem (Buffer.GetText (start, end, false)))
				{
					current_task = null;
					new_task_needed = true;
				}
			}
			
			//TODO: also check for tasklist name change
		}
		
		private bool IsTextTodoItem (String text)
		{
			//Logger.Debug(text.Trim());
			return text.Trim().Equals("[]");
		}
	}
}
