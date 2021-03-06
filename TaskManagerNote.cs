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
using Mono.Unix;
using Gtk;
using Tomboy;

/*! \mainpage Developer API of TaskManager for Tomboy
 *
 * \section intro This is the developer documentation for the TaskManager for Tomboy project.
 * This section is only interesting for you if you want to do changes to the code of this project.
 * 
 * To just install and use it, and for any other information, just have a look at the main project page:
 * http://wiki.github.com/rggjan/Tomboy-Todo-List/
 *
 * \subsection developers Developer Information
 * More information for developers is also available on this page.
 * 
 * To just get started hacking, however, have a look at this:
 * http://github.com/rggjan/Tomboy-Todo-List/blob/master/doc/README.developer
 * 
 * \subsection License
 * TaskManager is licensed under the LGPL:
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *   
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *   
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *  
 * \subsection authors Authors
 *       Jan Rüegg <rggjan@gmail.com>
 *       Gabriel Walch <walchg@student.ethz.ch>
 *       Gerd Zellweger <mail@gerdzellweger.com>
 * 
 */

namespace Tomboy.TaskManager {
	
	/// <summary>
	/// Class that describes behaviour of Notes (in terms of TaskManager features)
	/// </summary>
	public class TaskManagerNoteAddin : NoteAddin {

		private TaskManagerGui gui;
		private TaskNoteUtilities utils;
		private TaskList lock_end_needed = null;
		private List<FixAction> fix_list = new List<FixAction>();

		private List<TaskList> tasklists;
		
		public List<TaskList> TaskLists {
			get
			{
				if (tasklists == null){
					TaskListParser parser = new TaskListParser (Note);
					tasklists = parser.Parse ();
				}
				return tasklists;
			}
		}
		
		/// <summary>
		/// Returns true if the TaskList has open Tasks left
		/// </summary>
		public bool HasOpenTasks {
			get {
				return TaskLists.FindAll(c => c.Done == true).Count == TaskLists.Count;
			}
		}
		
		public TaskNoteUtilities Utils {
			get {
				return utils;
			}
		}
		
		/// <summary>
		/// This constructor is used in Unit tests to initialize the addin 
		/// for notes created in the tests on-the-fly.
		/// It calls the underlying Initialize function in <see cref="NoteAddin"/> which takes
		/// care of the rest.
		/// </summary>
		/// <param name="note">
		/// A <see cref="Note"/> for which we want the TaskManager Addin functionality.
		/// </param>
		public TaskManagerNoteAddin (Note note)
		{
			Initialize (note);
		}
		
		public TaskManagerNoteAddin ()
		{
		}
		
		/// <summary>
		/// Initialize the Note (but no loading/buffer yet)
		/// </summary>
		public override void Initialize ()
		{
			NoteTag tag = new NoteTag ("locked");
			tag.Editable = false;
			tag.CanSerialize = false;

			if (Note.TagTable.Lookup ("locked") == null)
				Note.TagTable.Add (tag);

//			tag = new NoteTag ("priority");
//			tag.CanActivate = true;
//			tag.CanSerialize = false;
//			tag.Foreground = "blue";
//			tag.Family = "monospace";
//			tag.Activated += OnPriorityClicked;
			
			tag = new PriorityTag ("priority", this);

			if (Note.TagTable.Lookup ("priority") == null)
				Note.TagTable.Add (tag);

			tag = new NoteTag ("checkbox-active");
			//tag.CanActivate = true;
			tag.CanSerialize = false;
			tag.Family = "monospace";
			//tag.Activated += ToggleCheckbox;

			if (Note.TagTable.Lookup ("checkbox-active") == null)
				Note.TagTable.Add (tag);
			
			tag = new NoteTag ("checkbox");
			tag.CanActivate = true;
			tag.CanSerialize = false;
			tag.Family = "monospace";

			if (Note.TagTable.Lookup ("checkbox") == null)
				Note.TagTable.Add (tag);
			
			if (Note.TagTable.Lookup ("duedate") == null)
				Note.TagTable.Add (new DateTag ("duedate", this));

			//TaskTag
			if (!Note.TagTable.IsDynamicTagRegistered ("task"))
				Note.TagTable.RegisterDynamicTag ("task", typeof(TaskTag));
		
			//TaskListTag
			if (!Note.TagTable.IsDynamicTagRegistered ("tasklist"))
				Note.TagTable.RegisterDynamicTag ("tasklist", typeof(TaskListTag));
			
			if (!Note.TagTable.IsDynamicTagRegistered ("color"))
				Note.TagTable.RegisterDynamicTag ("color", typeof(ColorTag));
			
			//StartListeners ();
		}
		
		/// <summary>
		/// Start all the TextBuffer listeners, + the keyrelease repair listener.
		/// </summary>
		public void StartListeners ()
		{
			StopListeners();
			Buffer.InsertText += BufferInsertText;
			Buffer.UserActionEnded += Repair;
			Buffer.DeleteRange += DeleteRange;
			
			if (HasWindow)
				Window.Editor.KeyReleaseEvent += Repair;
			//this.Note.Window.Editor.
		}
		
		/// <summary>
		/// Stop all the TextBuffer listeners, + the keyrelease repair listener
		/// </summary>
		public void StopListeners ()
		{
			if (HasWindow)
				Window.Editor.KeyReleaseEvent -= Repair;
			
			Buffer.InsertText -= BufferInsertText;
			Buffer.UserActionEnded -= Repair;
			Buffer.DeleteRange -= DeleteRange;
		}
		
		/// <summary>
		/// Fixes all the items in the fix_list
		/// </summary>
		public void FixList()
		{
			StopListeners ();
			
			foreach (FixAction action in fix_list)
			{
				if (action.Priority)
					action.fix ();
			}
			
			foreach (FixAction action in fix_list)
			{
				if (!action.Priority)
					action.fix ();
			}
			fix_list.Clear ();

			if (lock_end_needed != null)
			{
				if (lock_end_needed.LockEnd ())
					lock_end_needed.PlaceCursorAtEnd ();
				
				lock_end_needed = null;
			}
			
			StartListeners ();
		}
		
		/// <summary>
		/// Do all the Buffer modifications that can not be done during the actual events
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="Gtk.KeyReleaseEventArgs"/>
		/// </param>
		public void Repair (object o, EventArgs args)
		{
			FixList();
		}

		public override void Shutdown ()
		{
			StopListeners ();
			gui.StopListeners ();
		}


		/// <summary>
		/// Loads the content of the buffer, sets up GUI
		/// </summary>
		public override void OnNoteOpened ()
		{
			//Makes note window wider so it shows by default the tasklist menu
			Window.DefaultWidth += 200;
			
			gui = new TaskManagerGui (this);
			gui.StartListeners ();
			StartListeners ();
			
			//Initialise tasklists list
			DeserializeTasklists ();
			gui.PriorityShown = true;
		}
		
		public bool OnPriorityClicked (Task t)
		{
			Logger.Debug ("clicked!");
			
			StopListeners ();
			gui.PriorityShown = true;
			t.ShowPriority ();
			StartListeners ();
			//utils.GetTask (start).TagUpdate ();
			//Logger.Debug(utils.GetTask (start).Priority.ToString());
			
			return true;
		}
		
//		bool ToggleCheckbox (NoteTag tag, NoteEditor editor, Gtk.TextIter begin, Gtk.TextIter end)
//		{
//			Task task = utils.GetTask (begin);
//			task.Toggle ();
//			return true;
//		}
		
		/// <summary>
		/// Look for deleted tasks and delete them from the internal structures, if necessary
		/// </summary>
		public void ValidateTaskLists ()
		{
			List<TaskList> to_delete = new List<TaskList> ();
			
			foreach (TaskList tasklist in tasklists)
			{
				if (tasklist.WasDeleted ())
				{
					Logger.Debug ("Deleting removed tasklist");
					to_delete.Add (tasklist);
				}
			}
			
			foreach (TaskList tasklist in to_delete) {
				tasklist.Delete ();
				tasklists.Remove (tasklist);
			}
		}
		
				
		/// <summary>
		/// Listener for delete events, repairs tasks if necessary...
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="Gtk.DeleteRangeArgs"/>
		/// </param>
		private void DeleteRange (object o, Gtk.DeleteRangeArgs args)
		{
				if (fix_list.Count == 0)
				{
					TaskList tasklist1 = utils.GetTaskList (args.Start);
					if (tasklist1 != null)
						Logger.Debug ("Tasklist start deleted!");
				
				var iter = args.Start;
					iter.BackwardChar ();
					TaskList tasklist2 = utils.GetTaskList (iter);
				
				if (tasklist2 != null)
						Logger.Debug ("Tasklist end deleted!");
				
				FixDeleteAction action = new FixDeleteAction (this, tasklist1, tasklist2, args.Start.Line);

				fix_list.Add (action);
			}
		}
		
		//TODO beginning of task lists need fix
		
		/// <summary>
		/// Check for changes (related to taskmanager) when text is written into the buffer
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="Gtk.InsertTextArgs"/>
		/// </param>
		void BufferInsertText (object o, Gtk.InsertTextArgs args)
		{			
			// Forbid insertion of text at certain places...
			List<TextTag> tags = new List<TextTag> (args.Pos.Tags);
			if (tags.Exists (c => c.Name == "checkbox-active")){
				fix_list.Add (new FixUndoAction (this));
				return;
			}
			if (tags.Exists (c => c.Name == "duedate") && args.Text.Contains (Environment.NewLine)){
				fix_list.Add (new FixUndoAction (this));
				return;
			}
			
			if (args.Text == System.Environment.NewLine) {
				Gtk.TextIter end = args.Pos;
				end.BackwardChar ();
				
				Task task = utils.GetTask ();
				
				// Behaviour: onTask\n\n should delete empty checkbox
				if (task != null && task.LineIsEmpty) {
					fix_list.Add(new FixDeleteEmptyCheckBoxAction(this, task));
					return;
				}
				
				// Insert new checkbox if was onTask
				TaskList tasklist = utils.GetTaskList (args.Pos);
				if (tasklist != null) {
					if (tasklist.Description().Length == 0) {
						fix_list.Add(new FixUndoAction(this));
						return;
					}
					fix_list.Add(new NewTaskAction(this, tasklist));
					return;
				}
				
				end = args.Pos;
				Gtk.TextIter start = args.Pos;
				start.BackwardLine ();
				
				if (utils.IsTextTodoItem (Buffer.GetText (start, end, false))) {
					fix_list.Add(new NewTaskAction(this));
				}
			} else {
				var iter = args.Pos;
				iter.BackwardChars(args.Length);
				if (iter.StartsLine()) {
					TaskList tasklist = utils.GetTaskList();
					if (tasklist != null)
						fix_list.Add(new FixTitleAction(this, tasklist));
				}
			}
		}

		
		/// <summary>
		/// Loads (in terms of Tasks and Tasklists) the contents of a note
		/// </summary>
		public void DeserializeTasklists ()
		{
			Logger.Debug ("Loading...");
			utils = new TaskNoteUtilities (Buffer);
			
			//var parser = new TaskListParser(Note);
			//TaskLists = parser.Parse();
						/* Not necessary anymore...
			 * I put the loader into the getter
			 * That way, it's dynamic
			 */
		
			foreach (TaskList tl in TaskLists)
			{
				foreach (Task t in tl.Tasks)
				{
					t.AddWidgets ();
				}
				tl.LockEnd ();
			}
			
			// TODO load "PriorityShown" from the configuration?
		}
	}

	
}
