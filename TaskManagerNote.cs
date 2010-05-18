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
		
		Gtk.Menu task_menu = new Gtk.Menu();
		Gtk.MenuItem add_list = new Gtk.MenuItem (Catalog.GetString ("Add TaskList"));
		Gtk.MenuItem add_duedate = new Gtk.MenuItem (Catalog.GetString ("Add Duedate"));
		Gtk.CheckMenuItem show_priority = new Gtk.CheckMenuItem (Catalog.GetString ("Show Priorities"));
		
		bool new_task_needed = false;
		TaskList current_task_list = null;
		TaskNoteUtilities utils = null;
		
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
			Logger.Debug ("Initializing TaskManager");
			// FIXME this is executed 20 times (is this really a bug? i mean if u have 20 notes then
			// u probably want this executed 20 times (at least the tags should be registred for every note)
			// im not sure if this holds for the gtk styles too, maybe they should be factored
			// out to TaskManagerApplication.cs?
				
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
											GtkComboBox::width-request = 0
										}
										widget ""*.tomboy-inline-combobox"" style ""combobox-style""
									";
			
			Gtk.Rc.ParseString (comboStyleMod);
			
			RegisterTags();
		}
		
		public void RegisterTags ()
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
			tag.Activated += OnPriorityClicked;

			if (Note.TagTable.Lookup ("priority") == null)
				Note.TagTable.Add (tag);
			
			if (Note.TagTable.Lookup ("duedate") == null)
				Note.TagTable.Add (new DateTag ("duedate"));

			tag = new NoteTag ("invisible");
			tag.Invisible = true;
			
			if (Note.TagTable.Lookup ("invisible") == null)
				Note.TagTable.Add (tag);
			
			
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
			add_duedate.Activated -= OnAddDuedateActivated; //FIXME some are missing...

			Buffer.InsertText -= BufferInsertText;
			Buffer.UserActionEnded -= CheckIfNewTaskNeeded;
		}

		/// <summary>
		/// Loads the content of the buffer, sets up GUI
		/// </summary>
		public override void OnNoteOpened ()
		{
			Buffer.InsertText += BufferInsertText;
			Buffer.UserActionEnded += CheckIfNewTaskNeeded;
			
			//Initialise tasklists list
			//TODO: get from previous sessions?		
			DeserializeTasklists ();
			InitializeGui ();
		}
		
		private bool OnPriorityClicked (NoteTag tag, NoteEditor editor, Gtk.TextIter start, Gtk.TextIter end)
		{
			Logger.Debug ("clicked!");
			utils.GetTask (start).Priority = Priorities.VERY_HIGH;
			utils.GetTask (start).TagUpdate ();
			Logger.Debug(utils.GetTask (start).Priority.ToString());
			
			return true;
		}
		
		private void InitializeGui ()
		{
			task_menu.Add (add_duedate);
			add_duedate.Activated += OnAddDuedateActivated;
			
			task_menu.Add (show_priority);
			show_priority.Toggled += OnShowPriorityActivated;
			
			Gtk.MenuToolButton menu_tool_button = new Gtk.MenuToolButton (Gtk.Stock.Strikethrough);
			
			menu_tool_button.IconName = "ghi"; //Not working!
			
			menu_tool_button.TooltipText = Catalog.GetString("Add a new TaskList");
			menu_tool_button.ArrowTooltipText = Catalog.GetString("Set TaskList properties");
			menu_tool_button.Menu = task_menu;
			task_menu.ShowAll ();
		
			menu_tool_button.Clicked += OnAddListActivated;
			menu_tool_button.ShowMenu += UpdateMenuSensitivity;
			menu_tool_button.Show ();
			
			AddToolItem (menu_tool_button, -1);
		}
		
		/// <summary>
		/// Makes sure that add_list and add_priority menu items Sensitive property
		/// is set correctly according to where we currently are in the NoteBuffer
		/// </summary>
		void UpdateMenuSensitivity (object sender, EventArgs args)
		{
			Logger.Debug ("UpdateMenuSensitivity");
			
			// toggle sensitivity
			Task task = utils.GetTask ();
			if (task != null && task.PriorityUnset () && show_priority.Active) {
				//add_priority.Sensitive = true;
				//add_list.Sensitive = false;
			}
			else {
				//add_priority.Sensitive = false;
				//add_list.Sensitive = true;
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
			if (utils.InTaskList ())
				return;
			
			TaskList tl = new TaskList (Note);
			
			//tl.Name = "New TaskList!";
			TaskLists.Add (tl);
		}
		
		void OnShowPriorityActivated (object sender, EventArgs args)
		{
			TogglePriorityVisibility ();
		}
		
		private void TogglePriorityVisibility ()
		{
						if (show_priority.Active) {
				foreach (TaskList list in TaskLists)
					foreach (Task task in list.Children)
						task.ShowPriority ();
			} else {
				foreach (TaskList list in TaskLists)
					foreach (Task task in list.Children)
						task.HidePriority ();
			}

		}
		
		void OnAddDuedateActivated (object sender, EventArgs args)
		{
			Dialog dialog = new Dialog
                ("Sample", Window, Gtk.DialogFlags.DestroyWithParent);
            dialog.Modal = true;
			dialog.VBox.Add (new Calendar ());
			dialog.VBox.ShowAll ();
			dialog.AddButton ("OK", ResponseType.Ok);
            dialog.AddButton ("Cancel", ResponseType.Cancel);
			
			dialog.Response += new ResponseHandler (on_dialog_response);
            dialog.Run ();
            dialog.Destroy ();
		}
		
		void on_dialog_response (object obj, ResponseArgs args)
		{
			if (args.ResponseId != ResponseType.Ok)
				return;
			
			var iter = Buffer.GetIterAtMark (Buffer.InsertMark);
			
			Buffer.InsertWithTags (ref iter, "test", 
			    new TextTag[]{Note.TagTable.Lookup ("duedate")});
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
			
			Task task = utils.GetTask ();
			if(task != null){
				task.AddPriority ();
			} else {
				Logger.Debug ("Tried to insert Priority outside of a task");	
			}
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
				
				var begin = end;
				begin.LineOffset = 0;
				
				// Behaviour: onTask\n\n should delete empty checkbox
				if (Buffer.GetText (begin, end, true).Trim ().Length == 1 && utils.InTaskList (end)) {
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
		public void DeserializeTasklists ()
		{
			Logger.Debug ("Loading...");
			TaskLists = new List<TaskList> ();
			utils = new TaskNoteUtilities (Buffer);
			
			var parser = new TaskListParser(Note);
			TaskLists = parser.Parse();
			
			Logger.Debug ("There have been {0} tasklists", new object[] { TaskLists.Count });
			
			foreach (TaskList tl in TaskLists)
			{
				foreach (Task t in tl.Children)
				{
					t.AddWidgets ();
				}
			}
			
			// TODO load this from the configuration?
			show_priority.Active = true;
			TogglePriorityVisibility ();
		}
	}
}
