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
		
		Gtk.MenuItem tasklist = new Gtk.MenuItem (Catalog.GetString ("TaskList"));
		Gtk.Menu task_menu = new Gtk.Menu();
		Gtk.MenuItem add_list = new Gtk.MenuItem (Catalog.GetString ("Add TaskList"));
		Gtk.MenuItem add_priority = new Gtk.MenuItem (Catalog.GetString ("Add Priority"));
		
		bool new_task_needed = false;
		TaskList current_task_list = null;
		TaskNoteUtilities utils = null;
		
		/// <summary>
		/// Initialize the Note (but no loading/buffer yet)
		/// </summary>
		public override void Initialize ()
		{
			Logger.Debug ("Initializing TaskManager");
			// FIXME this is executed 20 times
				
			string checkButtonStyleMod = @"style ""mystyle"" {
												GtkCheckButton::indicator-spacing = 0
												GtkCheckButton::focus-padding = 0
												GtkCheckButton::focus-line-width = 0
												GtkCheckButton::indicator-size = 13
											}
											widget ""*.tomboy-inline-checkbox"" style ""mystyle""
										 ";
			
			Gtk.Rc.ParseString (checkButtonStyleMod);
			
			
			string comboStyleMod = @"style ""combobox-style"" {
											GtkComboBox::appears-as-list = 0
											GtkComboBox::arrow-size = 0
											GtkComboBox::width-request = 10
										}
										widget ""*.tomboy-inline-combobox"" style ""combobox-style""
									";
			
			Gtk.Rc.ParseString (comboStyleMod);
		
			NoteTag tag = new NoteTag ("locked");
			tag.Editable = false;
			tag.CanSerialize = false;

			if (Note.TagTable.Lookup ("locked") == null)
				Note.TagTable.Add (tag);

			//tag = new NoteTag ("invisible");
			//tag.Invisible = true;
			
			//if (Note.TagTable.Lookup ("invisible") == null)
			//	Note.TagTable.Add (tag);
			
			
			//TaskTag
			if(!Note.TagTable.IsDynamicTagRegistered ("task"))
				Note.TagTable.RegisterDynamicTag ("task", typeof (TaskTag));

			//TaskListTag
			if(!Note.TagTable.IsDynamicTagRegistered ("tasklist"))
				Note.TagTable.RegisterDynamicTag ("tasklist", typeof (TaskListTag));
		}

		public override void Shutdown ()
		{
			add_list.Activated -= OnAddListActivated;
			add_priority.Activated -= OnAddPriorityActivated;
		}

		/// <summary>
		/// Loads the content of the buffer, sets up GUI
		/// </summary>
		public override void OnNoteOpened ()
		{
			tasklist.Submenu = task_menu;
			add_list.Activated += OnAddListActivated;
			add_priority.Activated += OnAddPriorityActivated;
			add_priority.Sensitive = false;
			
			Buffer.InsertText += BufferInsertText;
			Buffer.UserActionEnded += CheckIfNewTaskNeeded;
			
			Buffer.MarkSet += UpdateMenuSensitivity; //FIXME too often called?

			task_menu.Add(add_list);
			task_menu.Add(add_priority);
			tasklist.ShowAll();
			
			AddPluginMenuItem (tasklist);
			
			//Initialise tasklists list
			//TODO: get from previous sessions?
			TaskLists = new List<TaskList> ();
			utils = new TaskNoteUtilities (Buffer);
			
			Load ();
		}
		
		/// <summary>
		/// Makes sure that add_list and add_priority menu items Sensitive property
		/// is set correctly according to where we currently are in the NoteBuffer
		/// </summary>
		void UpdateMenuSensitivity (object sender, EventArgs args) {
			
			//Logger.Debug("UpdateMenuSensitivity");
			Gtk.TextIter cursor = Buffer.GetIterAtMark (Buffer.InsertMark);
			cursor.LineOffset = 0;
			
			// toggle sensitivity
			if(utils.InTaskList (cursor)) {
				add_priority.Sensitive = true;
				add_list.Sensitive = false;
			}
			else {
				add_priority.Sensitive = false;
				add_list.Sensitive = true;
			}
		}

		/// <summary>
		/// Add a new tasklist into the buffer
		/// </summary>
		/// <param name="sender">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="EventArgs"/>
		/// </param>
		void OnAddListActivated (object sender, EventArgs args)
		{
			TaskList tl = new TaskList (Note);
			
			add_list.Sensitive = false;
			add_priority.Sensitive = true;
			
			//tl.Name = "New TaskList!";
			TaskLists.Add (tl);
		}
		
		/// <summary>
		/// Add priority widget to some task
		/// </summary>
		/// <param name="sender">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="EventArgs"/>
		/// </param>
		void OnAddPriorityActivated (object sender, EventArgs args)
		{
			Gtk.TextIter cursor = Buffer.GetIterAtMark (Buffer.InsertMark);
			cursor.BackwardChar ();
			
			Task task = utils.GetTask (cursor);
			if(task!=null){
				task.ShowPriority ();
				add_priority.Sensitive = false;
			} else {
				Logger.Debug ("Tried to insert Priority outside of a task");	
			}
		}
		
		/*Task GetTaskAtCursor ()
		{
			Gtk.TextIter here = Buffer.GetIterAtMark (Buffer.InsertMark);
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
				
				var begin = end;
				begin.LineOffset = 0;
				
				// Behaviour: onTask\n\n should delete empty checkbox
				if (Buffer.GetText (begin, end, true).Trim ().Length == 0 && utils.InTaskList (end)) {
					Task task = utils.GetTask ();
					if (task != null && task.IsLastTask ()) {
						task.Delete ();
						return;
					}
				}
				
				// Insert new checkbox if was onTask
				TaskList tasklist = utils.GetTaskList (end);
				if (tasklist != null){
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
			

		public List<TaskList> TaskLists {
			get; private set;
		}
		
		public bool HasOpenTasks {
			get {
				return TaskLists.FindAll(c => c.Done == true).Count == TaskLists.Count;
			}
		}
		
		/// <summary>
		/// Loads (in terms of Tasks and Tasklists) the contents of a note
		/// </summary>
		public void Load ()
		{
			Logger.Debug ("Loading...");
			TaskLists = TaskNoteParser.ParseNote(Note);
			
			Logger.Debug ("There have been {0} tasklists", new object[]{TaskLists.Count});
			
			foreach (TaskList tl in TaskLists)
			{
				foreach (Task t in tl.Children)
				{
					t.AddWidgets ();
				}
			}
		}
	}
}
