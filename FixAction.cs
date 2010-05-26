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

namespace Tomboy.TaskManager
{

	/// <summary>
	/// This class can be used to create Actions that should be carried out on the Buffer, but have
	/// to be deferred because it would render still used Gtk.TextIters useless, crashing Tomboy...
	/// </summary>
	public abstract class FixAction
	{
		protected TaskManagerNoteAddin addin;

		/// <summary>
		/// If set to true, this will be handled before other FixActions
		/// </summary>
		public bool Priority;

		public FixAction (TaskManagerNoteAddin addin)
		{
			this.addin = addin;
		}

		/// <summary>
		/// The actual fix to perform
		/// </summary>
		public abstract void fix ();
	}

	/// <summary>
	/// Does a simple undo.
	/// </summary>
	public class FixUndoAction : FixAction
	{
		public FixUndoAction (TaskManagerNoteAddin addin) : base(addin)
		{
			Priority = false;
		}

		public override void fix ()
		{
			addin.Buffer.Undoer.Undo ();
		}
		
	}

	/// <summary>
	/// Does a simple undo.
	/// </summary>
	public class FixDeleteEmptyCheckBoxAction : FixAction
	{
		Task task;

		public FixDeleteEmptyCheckBoxAction (TaskManagerNoteAddin addin, Task task) : base(addin)
		{
			this.task = task;
			Priority = false;
		}

		public override void fix ()
		{
			var iter = addin.Buffer.GetIterAtLine(task.Start.Line+1);
			var start = iter;
			var end = iter;
			
			if (!end.EndsLine())
				end.ForwardToLineEnd();
			
			string text = addin.Buffer.GetText(start, end, false);
			
			TaskList list;
			
			if (text.Trim().Length == 0)
				list = task.DeleteEmptyCheckBox (null);
			else
				list = task.DeleteEmptyCheckBox (string.Empty);
				
			
			if (list != null)
				addin.TaskLists.Add (list);
			
			addin.Utils.ResetCursor ();
			addin.Buffer.Undoer.ClearUndoHistory ();
		}
		
	}

	/// <summary>
	/// Action that Deletes (cleanly) a Task
	/// </summary>
	public class FixDeleteAction : FixAction
	{
		TaskList tasklist1;
		TaskList tasklist2;
		int line;

		public FixDeleteAction (TaskManagerNoteAddin addin, TaskList tasklist1, TaskList tasklist2, int line) : base(addin)
		{
			Priority = false;
			this.tasklist1 = tasklist1;
			this.tasklist2 = tasklist2;
			this.line = line;
		}

		public override void fix ()
		{
			if (tasklist2 == null && tasklist1 == null) {
				Logger.Debug ("Checking for Deleted Tasks");
				addin.ValidateTaskLists ();
			} else if (tasklist1 != null && tasklist2 != null) {
				if (tasklist1 == tasklist2) {
					Logger.Debug ("Have to repair within TaskList");
					TaskList new_list = tasklist1.FixWithin (line);
					if (new_list != null)
						addin.TaskLists.Add (new_list);
				} else {
					Logger.Debug ("Oh No, have to merge two TaskLists!");
					tasklist2.FixWithin (line);
					tasklist1.TransferTasksTo (tasklist2);
					addin.TaskLists.Remove (tasklist1);
				}
			} else if (tasklist1 != null) {
				Logger.Debug ("Fixing Start");
				tasklist1.FixTitle ();
				tasklist1.RemoveDeletedTasks ();
			} else {
				Logger.Debug ("Fixing End");
				tasklist2.FixWithin (line);
				tasklist2.LockEnd ();
			}
			
			addin.Utils.ResetCursor ();
			addin.Buffer.Undoer.ClearUndoHistory ();
		}
	}

	/// <summary>
	/// Action that adds a Task
	/// </summary>
	public class NewTaskAction : FixAction
	{
		TaskList tasklist;

		public NewTaskAction (TaskManagerNoteAddin addin, TaskList tasklist) : base(addin)
		{
			Priority = false;
			this.tasklist = tasklist;
		}

		public NewTaskAction (TaskManagerNoteAddin addin) : base(addin)
		{
			Priority = false;
		}

		public override void fix ()
		{
			Logger.Debug ("Adding a new Task");
			
			if (tasklist == null) {
				Gtk.TextIter start = addin.Buffer.GetIterAtMark (addin.Buffer.InsertMark);
				Gtk.TextIter end = start;
				
				start.BackwardLine ();
				addin.Buffer.Delete (ref start, ref end);
				addin.TaskLists.Add (new TaskList (addin.Note));
			} else {
				var iter = addin.Buffer.GetIterAtMark (addin.Buffer.InsertMark);
				var end = iter;
				end.ForwardToLineEnd ();
				
				TaskTag tt = addin.Utils.GetTaskTag (iter);
				if (tt != null) {
					//Remove Old TaskTag
					addin.Buffer.RemoveTag (tt, iter, end);
				}
				
				tasklist.AddTask (iter);
			}
			
			addin.Utils.ResetCursor();
			addin.Buffer.Undoer.ClearUndoHistory();
		}
	}
}
