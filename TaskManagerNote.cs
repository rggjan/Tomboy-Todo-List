using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager {
	
	public class TaskManagerNoteAddin : NoteAddin {
		
		Gtk.MenuItem tasklist = new Gtk.MenuItem (Catalog.GetString ("TaskList"));
		Gtk.Menu task_menu = new Gtk.Menu();
		Gtk.MenuItem add_list = new Gtk.MenuItem (Catalog.GetString ("Add TaskList"));
		Gtk.MenuItem add_priority = new Gtk.MenuItem (Catalog.GetString ("Add Priority"));
		
		bool new_task_needed = false;
		TaskList current_task = null;
		
		public override void Initialize ()
		{
			Logger.Debug ("Initializing TaskManager"); // FIXME this is executed 20 times
				
			/*string styleMod =
				@"style ""mystyle"" {
				#GtkCheckButton::indicator-spacing = 0
				#GtkCheckButton::focus-padding = 0
				#GtkCheckButton::focus-line-width = 2
				#GtkCheckButton::indicator-size = 100
				}
				widget ""*.tomboy-inline-checkbox"" style ""mystyle""";
			Gtk.Rc.ParseString (styleMod);*/
			
			
			/*string styleMod =
				@"style ""combobox-style"" {
				#GtkComboBox::appears-as-list = 1
				GtkComboBox::arrow-size = 0
				}
				widget ""*.tomboy-inline-combobox"" style ""combobox-style""";
			
			Gtk.Rc.ParseString (styleMod);*/
			
			NoteTag tag = new NoteTag ("locked");
			tag.Editable = false;

			if (Note.TagTable.Lookup ("locked") == null)
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
			add_priority.Activated -= OnAddPriorityActivated;
		}

		public override void OnNoteOpened ()
		{
			tasklist.Submenu = task_menu;
			add_list.Activated += OnAddListActivated;
			add_priority.Activated += OnAddPriorityActivated;
			
			Buffer.InsertText += BufferInsertText;
			Buffer.MarkSet += BufferMarkSet;
			Buffer.UserActionEnded += CheckIfNewTaskNeeded;
			
			task_menu.Add(add_list);
			task_menu.Add(add_priority);
			tasklist.ShowAll();
			
			AddPluginMenuItem (tasklist);
			
			//Initialise tasklists list
			//TODO: get from previous sessions?
			Children = new List<AttributedTask> ();
			
			Load ();
		}


		void OnAddListActivated (object sender, EventArgs args)
		{
			TaskList tl = new TaskList (Note);
			
			//tl.Name = "New TaskList!";
			Children.Add (tl);
		}
		
		void BufferMarkSet (object o, EventArgs args)
		{
		}

		void OnAddPriorityActivated (object sender, EventArgs args)
		{
			Gtk.TextIter cursor = Buffer.GetIterAtMark (Buffer.InsertMark);
			cursor.BackwardChar ();
			
			foreach (Gtk.TextTag tag in cursor.Tags) {
				if (tag is TaskTag) {
					Logger.Debug ("TaskTag found!");
					TaskTag tasktag = (TaskTag)tag;
					cursor.LineOffset = 0;
					tasktag.Task.InsertPriorityBox (cursor);
				}
			}
		
		}
		
		void BufferInsertText (object o, Gtk.InsertTextArgs args)
		{
			// TODO check that we're really in this tasklist
			
			if (args.Text == System.Environment.NewLine) {
				Gtk.TextIter end = args.Pos;
				end.BackwardChar ();
				
				var begin = end;
				begin.LineOffset = 0;
				
				if (Buffer.GetText (begin, end, false).Trim ().Length == 0) {
					//FIXME delete task!
				}
				
				end.BackwardChar ();
				
				// Go back to last char on line that is not a newline
				foreach (Gtk.TextTag tag in end.Tags) {
					//Edit: Wow. Now this looks pretty!
					if (tag is TaskTag) {
						Logger.Debug ("TaskTag found!");
						
						TaskTag tasktag = (TaskTag)tag;
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
				
				if (IsTextTodoItem (Buffer.GetText (start, end, false))) {
					current_task = null;
					new_task_needed = true;
				}
			
			
			



}
		}
			
		void CheckIfNewTaskNeeded (object sender, System.EventArgs args)
		{
			// TODO check that we're really in this tasklist
			
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
			//TODO: also check for tasklist name change
		}



		
		private bool IsTextTodoItem (String text)
		{
			//Logger.Debug(text.Trim());
			return text.Trim().Equals("[]");
		}
			

		public List<AttributedTask> Children {
			get; private set;
		}
		
		public List<AttributedTask> Containers {
			get; private set;
		}
		
		//TODO
		public bool Done {
			get; set;
		}
		
		public void Load ()
		{
		/*	Logger.Debug ("Loading...");
			TextIter iter = Buffer.StartIter;
			do {
				TaskListTag taskliststart = (TaskListTag)Buffer.GetDynamicTag ("tasklist", iter)				
				if (taskliststart != null)
				{
					TaskList tl = new TaskList (Note, iter);
					Children.Add (tl);
				}
				
				
				TaskTag start = (TaskTag)Buffer.GetDynamicTag ("task", iter);
				if (start != null) {
					Logger.Debug ("Tasktag found!");
					TaskTag end = start;

					while (end==start){
						iter.ForwardChar ();
						end = (TaskTag) Buffer.GetDynamicTag ("task", iter);
					}
				}
			} while(iter.ForwardChar ());*/
		}
		
	}
}
