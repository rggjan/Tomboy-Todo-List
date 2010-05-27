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
	/// It may have a title and a due date.
	/// </summary>
	public class TaskList : AttributedTask
	{

		/// <summary>
		/// Beginning of the description, for TaskLists the same as Start
		/// </summary>
		protected override TextIter DescriptionStart {
			get { return Start; }
		}

		/// <summary>
		/// End of the description, basically end of first line
		/// </summary>
		protected override TextIter DescriptionEnd {
			get {
				var start = Start;
				start.ForwardToLineEnd ();
				return start;
			}
		}

		protected override NoteBuffer Buffer {
			get { return ContainingNote.Buffer; }
		}


		/// <summary>
		/// Name of this task list
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// Note containing the TaskList.
		/// </summary>
		internal Note ContainingNote { get; set; }

		/// <summary>
		/// Tasks for ITask interface
		/// </summary>
		public List<Task> Tasks { get; set; }

		//Dropped for now
//		/// <summary>
//		/// LinkingTasks for ITask interface
//		/// </summary>
//		public List<Task> LinkinTasks { 
//			get; set;
//		}

		/// <summary>
		/// Describes what to do when tasklist is marked as done
		/// </summary>
		public override bool Done {
			get {
				if (Tasks != null) {
					return Tasks.FindAll (c => c.Done == true).Count == Tasks.Count;
				} else
					return true;
			}
			set { Tasks.ForEach (c => c.Done = value); }
		}

		/// <summary>
		/// Shortcut for attached tag
		/// </summary>
		private TaskListTag TaskListTag {
			get { return (TaskListTag)Tag; }
			set { Tag = value; }
		}

		/// <summary>
		/// End of the Tasklist, including all tasks
		/// </summary>
		protected override TextIter End {
			get {
				Gtk.TextIter iter = Buffer.GetIterAtLine (LastTaskLine);
				iter.ForwardToLineEnd ();
				return iter;
			}
		}

		/// <summary>
		/// Returns the last line where a Task is on
		/// </summary>
		public int LastTaskLine {
			get {
				int line = -1;
				foreach (Task task in Tasks) {
					if (task.Line > line)
						line = task.Line;
				}
				return line;
			}
		}

		/// <summary>
		/// Creates a new tasklists including all the given tasks.
		/// </summary>
		/// <param name="tasks">
		/// A <see cref="List<Task>"/>
		/// </param>
		public TaskList (Note note, List<Task> tasks, String name, Gtk.TextIter start)
		{
			ContainingNote = note;
			//TODO possible merge this with things below?
			Name = name;
			TaskListTag tag = (TaskListTag)ContainingNote.TagTable.CreateDynamicTag ("tasklist");
			
			NoteBuffer buffer = note.Buffer;
			
			Initialize (start, tag);
			
			var end = Start;
			if (tasks.Count == 0)
				name = name + "\n";
			
			buffer.Insert (ref end, name);
			start = Start;
			
			if (!end.EndsLine ())
				end.ForwardToLineEnd ();
			
			end.ForwardChar ();
			Buffer.ApplyTag (TaskListTag, start, end);
			
			Tasks = new List<Task> ();
			
			if (tasks.Count > 0)
				foreach (Task task in tasks) {
					Tasks.Add (task);
					task.RemoveTag (task.ContainingTaskList.Tag);
					task.ContainingTaskList = this;
					
					// This is required for intendation
					this.Tag.Priority = 0;
					task.ApplyTag (this.Tag);
				}

			else {
				end.BackwardChar ();
				AddTask (end);
			}
		}

		/// <summary>
		/// Transfer all the tasks to another tasklist, used for merging
		/// </summary>
		/// <param name="tasklist">
		/// The other <see cref="TaskList"/> tasklist to send the tasks to
		/// </param>
		public void TransferTasksTo (TaskList tasklist)
		{
			List<Task> to_transfer = new List<Task> ();
			
			foreach (Task task in Tasks) {
				if (!task.WasDeleted) {
					Logger.Debug ("adding task " + task.Description ());
					to_transfer.Add (task);
				}
			}
			
			foreach (Task task in to_transfer) {
				tasklist.AddFinishedTask (task);
				task.RemoveTag (Tag);
				Tasks.Remove (task);
			}
			
			Delete ();
			tasklist.LockEnd ();
		}

		/// <summary>
		/// Fix the TaskList, for example after heavy deletion operations.
		/// </summary>
		/// <param name="line">
		/// A <see cref="System.Int32"/>, the line on which fixing is needed.
		/// </param>
		/// <returns>
		/// Returns the new <see cref="TaskList"/> if splitting was required.
		/// </returns>
		public TaskList FixWithin (int line)
		{
			Logger.Debug ("FixWithin");
			var invalid_list = RemoveDeletedTasks ();
			if (invalid_list.Count >= 1) {
				foreach (Task inv_task in invalid_list) {
					if (inv_task.Line != line) {
						Logger.Fatal ("Got wrong line!");
						return null;
					}
				}
			}
			
			TaskTag tasktag = (TaskTag)Buffer.GetDynamicTag ("task", Buffer.GetIterAtLine (line));
			// Not in title
			if (tasktag != null) {
				Task task = tasktag.Task;
				return task.Fix ();
			} else {
				// In title
				Logger.Debug ("Fixing title");
				FixTitle ();
				return null;
			}
		}

		/// <summary>
		/// Fix the title after merging or deleting operations.
		/// </summary>
		public void FixTitle ()
		{
			utils.RemoveTaskTags (DescriptionStart, DescriptionEnd);
			Buffer.ApplyTag (Tag, DescriptionStart, End);
		}

		/// <summary>
		/// Remove deleted Tasks.
		/// </summary>
		/// <returns>
		/// Returns the invalid tasks as <see cref="List<Task>"/>
		/// </returns>
		public List<Task> RemoveDeletedTasks ()
		{
			List<Task> remove_list = new List<Task> ();
			List<Task> invalid_list = new List<Task> ();
			
			foreach (Task task in Tasks) {
				if (task.WasDeleted) {
					Logger.Debug ("Found Deleted Task");
					remove_list.Add (task);
				} else {
					if (task.IsValid) {
						Logger.Debug (task.Description () + " is valid!");
					} else {
						Logger.Debug ("Found invalid Task");
						invalid_list.Add (task);
					}
				}
			}
			
			foreach (Task task in remove_list) {
				task.Delete ();
			}
			
			return invalid_list;
		}

		/// <summary>
		/// Locks the end of a tasklist.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>, true if a newline was inserted
		/// </returns>
		public bool LockEnd ()
		{
			bool result = false;
						
			Logger.Debug ("locking end");
			var end = End;
			if (Buffer.GetDynamicTag ("tasklist", end) != Tag) {
				result = true;
				Logger.Debug ("inserting \\n");
				Buffer.Insert (ref end, System.Environment.NewLine);
				if (Buffer.GetIterAtMark (Buffer.InsertMark).Equal (end)) {
					end.BackwardChar ();
					Buffer.PlaceCursor (end);
				}
			}
			
			end = End;
			var start = end;
			end.ForwardChar ();
			Buffer.ApplyTag ("locked", start, end);
			
			return result;
		}

		/// <summary>
		/// Returns true if the start of the TaskList was deleted.
		/// </summary>
		public bool StartDeleted ()
		{
			TaskListTag tag = (TaskListTag)Buffer.GetDynamicTag ("tasklist", Start);
			if (tag != this.Tag)
				return true;
			
			return false;
		}

		/// <summary>
		/// Returns true if the TaskList was deleted.
		/// </summary>
		public bool WasDeleted ()
		{
			TaskListTag tag = (TaskListTag)Buffer.GetDynamicTag ("tasklist", Start);
			if (tag != this.Tag)
				return true;
			//FIXME what if only start deleted?
			
			return false;
		}

		/// <summary>
		/// Delete all the metadata of the Tasklist.
		/// </summary>
		public void Delete ()
		{
			ContainingNote.TagTable.Remove (this.TaskListTag);
			foreach (Task task in Tasks)
				task.DeleteTag ();
		}

		/// <summary>
		/// Print the structure of the TaskList to the console, only if debugging!
		/// </summary>
		public void DebugPrint ()
		{
			if (!Tomboy.Debugging)
				return;
			
			Console.WriteLine ("Tasklist '" + Description () + "':");
			foreach (Task task in Tasks)
				task.DebugPrint ();
			
			Console.WriteLine ();
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
			else {
				buffer.Insert (ref lineend, System.Environment.NewLine);
				iter = lineend;
			}
			
			Initialize (iter, tag);
			
			var end = Start;
			buffer.Insert (ref end, "New Tasklist!\n\n");
			var start = Start;
			Buffer.ApplyTag (TaskListTag, start, end);
			
			/*start = end;
			start.BackwardChar ();
			Buffer.ApplyTag ("locked", start, end);*/			//TODO lock start!

			Logger.Debug ("TaskList created");
			//Logger.Debug (iter.Char.ToString());
			
			Tasks = new List<Task> ();
			end.BackwardChar ();
			AddTask (end);
			
			LockEnd ();
		}

		/// <summary>
		/// Places the cursor at the end of the TaskList.
		/// </summary>
		public void PlaceCursorAtEnd ()
		{
			Buffer.PlaceCursor (End);
		}

		/// <summary>
		/// Creates a new task list with existing tags.
		/// </summary>
		public TaskList (Note note, Gtk.TextIter start, TaskListTag tag)
		{
			ContainingNote = note;
			Name = ("New TaskList!");
			//FIXME
			Initialize (start, tag);
			Logger.Debug ("TaskList created");
			
			Tasks = new List<Task> ();
		}

		/// <summary>
		/// Creates a new Task and add it to the `Tasks` list.
		/// </summary>
		/// <param name="at">
		/// <see cref="Gtk.TextMark"/> Where to add the task in the Buffer.
		/// </param>
		public void AddTask (Gtk.TextIter position)
		{
			Tasks.Add (new Task (this, position));
		}

		/// <summary>
		/// Add an existing task to this list.
		/// </summary>
		/// <param name="task">
		/// A <see cref="Task"/> to add.
		/// </param>
		public void AddFinishedTask (Task task)
		{
			Tasks.Add (task);
			task.ApplyTag (Tag);
		}

		/// <summary>
		/// Add a new task to this tasklist
		/// </summary>
		/// <param name="position">
		/// A <see cref="Gtk.TextIter"/>, where to insert the task
		/// </param>
		/// <param name="tag">
		/// A <see cref="TaskTag"/>, what tag to give the new task
		/// </param>
		public void AddTask (Gtk.TextIter position, TaskTag tag)
		{
			Tasks.Add (new Task (this, position, tag));
		}
	}
}
