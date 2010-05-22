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
		private bool new_task_needed = false;
		private TaskList current_task_list = null;
		
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

			tag = new NoteTag ("priority");
			tag.CanActivate = true;
			tag.CanSerialize = false;
			tag.Foreground = "blue";
			tag.Family = "monospace";
			tag.Activated += OnPriorityClicked;

			if (Note.TagTable.Lookup ("priority") == null)
				Note.TagTable.Add (tag);

			tag = new NoteTag ("checkbox-active");
			tag.CanActivate = true;
			tag.CanSerialize = false;
			tag.Family = "monospace";
			tag.Activated += ToggleCheckbox;

			if (Note.TagTable.Lookup ("checkbox-active") == null)
				Note.TagTable.Add (tag);
			
			tag = new NoteTag ("checkbox");
			tag.CanActivate = true;
			tag.CanSerialize = false;
			tag.Family = "monospace";

			if (Note.TagTable.Lookup ("checkbox") == null)
				Note.TagTable.Add (tag);
			
			tag = new NoteTag ("invisible");
			tag.Invisible = true;
			
			if (Note.TagTable.Lookup ("invisible") == null)
				Note.TagTable.Add (tag);
			
			if (Note.TagTable.Lookup ("duedate") == null)
				Note.TagTable.Add (new DateTag ("duedate"));

			//TaskTag
			if (!Note.TagTable.IsDynamicTagRegistered ("task"))
				Note.TagTable.RegisterDynamicTag ("task", typeof(TaskTag));
		
			//TaskListTag
			if (!Note.TagTable.IsDynamicTagRegistered ("tasklist"))
				Note.TagTable.RegisterDynamicTag ("tasklist", typeof(TaskListTag));
			
			Buffer.InsertText += BufferInsertText;
			Buffer.UserActionEnded += CheckIfNewTaskNeeded;
			Buffer.DeleteRange += DeleteRange;
		}

		public override void Shutdown ()
		{
			Buffer.InsertText -= BufferInsertText;
			Buffer.UserActionEnded -= CheckIfNewTaskNeeded;
			
			gui.StopListeners ();
		}

		/// <summary>

		/// Loads the content of the buffer, sets up GUI
		/// </summary>
		public override void OnNoteOpened ()
		{
			gui = new TaskManagerGui (this);
			gui.StartListeners ();
			
			//Initialise tasklists list
			//TODO: get from previous sessions?		
			DeserializeTasklists ();
		}
		
		private bool OnPriorityClicked (NoteTag tag, NoteEditor editor, Gtk.TextIter start, Gtk.TextIter end)
		{
			Logger.Debug ("clicked!");
			utils.GetTask (start).Priority = Priorities.VERY_HIGH;
			//utils.GetTask (start).TagUpdate ();
			//Logger.Debug(utils.GetTask (start).Priority.ToString());
			
			return true;
		}
		
		bool ToggleCheckbox (NoteTag tag, NoteEditor editor, Gtk.TextIter begin, Gtk.TextIter end)
		{
			Task task = utils.GetTask (begin);
			task.Toggle ();
			return true;
		}
		
		void DeleteRange (object o, Gtk.DeleteRangeArgs args)
		{
			// Recursion problem without this:
			Buffer.DeleteRange -= DeleteRange;
			
			Buffer.Undoer.ClearUndoHistory ();
			//TODO apply this everywhere!

			TaskList tasklist1 = utils.GetTaskList (args.Start);
			if (tasklist1 != null)
				Logger.Debug ("Tasklist start deleted!");
			
			var iter = args.Start;
			iter.BackwardChar ();

			TaskList tasklist2 = utils.GetTaskList (iter);
			if (tasklist2 != null)
			{
				tasklist2.FixEnd ();
				Logger.Debug ("Tasklist end deleted!");			
			}
			Buffer.DeleteRange += DeleteRange;
		}
		
		/*Task GetTaskAtCursor ()
		{
			here.LineOffset = 0;
		}*/
		
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
			if (args.Text == System.Environment.NewLine) {
				Gtk.TextIter end = args.Pos;
				end.BackwardChar ();
				
				Task task = utils.GetTask ();
				
				// Behaviour: onTask\n\n should delete empty checkbox
				if (task != null && task.LineIsEmpty ()) {
					// Recursion problem without this:
					Buffer.DeleteRange -= DeleteRange;
					
					TaskList list = task.Delete ();
					if (list != null)
						tasklists.Add (list);
					
					Buffer.PlaceCursor (Buffer.GetIterAtMark (Buffer.InsertMark));
					
					Buffer.DeleteRange += DeleteRange;
					return;
				}
				
				// Insert new checkbox if was onTask
				TaskList tasklist = utils.GetTaskList (end);
				if (tasklist != null) {
					current_task_list = tasklist;
					new_task_needed = true;
					return;
				}
				
				end = args.Pos;
				Gtk.TextIter start = args.Pos;
				
				start.BackwardLine ();
				
				if (IsTextTodoItem (Buffer.GetText (start, end, false))) {
					current_task_list = null;
					new_task_needed = true;
				}
			}
		}
			
		/// <summary>
		/// Checks whether new task is needed
		/// </summary>
		/// <param name="sender">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="System.EventArgs"/>
		/// </param>
		void CheckIfNewTaskNeeded (object sender, System.EventArgs args)
		{
			if (new_task_needed) {
				Logger.Debug ("Adding a new Task");
				
				if (current_task_list == null) {
					Gtk.TextIter start = Buffer.GetIterAtMark (Buffer.InsertMark);
					Gtk.TextIter end = start;
					
					start.BackwardLine ();
					//end.ForwardChars (2);
					
					//TODO: Use the rest of this line as the title of the new task list
					
					// Logger.Debug(Buffer.GetText(start, end, false));
					Buffer.Delete (ref start, ref end);
					
					TaskLists.Add (new TaskList (Note));
				} else {
					var iter = Buffer.GetIterAtMark (Buffer.InsertMark);
					var end = iter;
					end.ForwardToLineEnd ();
					
					TaskTag tt = utils.GetTaskTag (iter);
					if(tt!=null){
						Logger.Debug ("removing old tasktag");
						Buffer.RemoveTag (tt, iter, end);
					}
//					Buffer.RemoveTag ("locked", iter, end);
					
					current_task_list.addTask (iter);
				}
				new_task_needed = false;
			}
			//TODO: also check for tasklist name change
		}
		
		private bool IsTextTodoItem (String text)
		{
			//Logger.Debug(text.Trim());
			return text.Trim().Equals("[]");
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
				foreach (Task t in tl.Children)
				{
					t.AddWidgets ();
				}
			}
			
			// TODO load this from the configuration?
			gui.PriorityShown = true;
		}
	}
}
