using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using Tomboy;

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
				Note.TagTable.Add (new DateTag ("duedate"));

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
			
//			//Now that Buffer and Note exists and does not give bugs, assign it to prio tag
//			PriorityTag tag = (PriorityTag) Buffer.TagTable.Lookup ("priority");
//			tag.Buffer = Buffer;
			
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
		
		void DeleteRange (object o, Gtk.DeleteRangeArgs args)
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
			foreach (Gtk.TextTag t in args.Pos.Tags)
			{
				if (t.Name != null && t.Name.Equals("duedate"))
				{
					var iter = args.Pos;
					iter.BackwardChar();
					foreach (Gtk.TextTag t2 in iter.Tags)
					{
						if (t2.Name != null && t2.Name.Equals("duedate") && args.Text.Contains (Environment.NewLine))
						{
							fix_list.Add (new FixUndoAction (this));
							return;
						}
					}
				}
				if (t.Name != null && t.Name.Equals("checkbox-active"))
				{
					fix_list.Add (new FixUndoAction (this));
					return;
				}
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
				TaskList tasklist = utils.GetTaskList (end);
				if (tasklist != null) {
					fix_list.Add(new NewTaskAction(this, tasklist));
					return;
				}
				
				end = args.Pos;
				Gtk.TextIter start = args.Pos;
				start.BackwardLine ();
				
				if (utils.IsTextTodoItem (Buffer.GetText (start, end, false))) {
					fix_list.Add(new NewTaskAction(this));
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
