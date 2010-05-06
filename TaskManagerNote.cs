using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager {
	
	public class TaskManagerNoteAddin : NoteAddin, ITask {
		
		Gtk.MenuItem tasklist = new Gtk.MenuItem (Catalog.GetString ("TaskList"));
		Gtk.Menu task_menu = new Gtk.Menu();
		Gtk.MenuItem add_list = new Gtk.MenuItem (Catalog.GetString ("Add TaskList"));
		Gtk.MenuItem add_priority = new Gtk.MenuItem (Catalog.GetString ("Add Priority"));
		

		bool initialized = false;
		bool new_task_needed = false;
		bool deletion_needed = false;
		
		public override void Initialize ()
		{
			Logger.Debug ("Initializing TaskManager");
				
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
			
			tasklist.Submenu = task_menu;
			add_list.Activated += OnAddListActivated;
			add_priority.Activated += OnAddPriorityActivated;
			
			task_menu.Add(add_list);
			task_menu.Add(add_priority);
			tasklist.ShowAll();
			
			AddPluginMenuItem (tasklist);

			Buffer.UserActionEnded += CheckIfNewTaskNeeded;
			Buffer.InsertText += BufferInsertText;
			
			//Initialise tasklists list
			//TODO: get from previous sessions?
			Children = new List<AttributedTask> ();
		}

		public override void Shutdown ()
		{
			add_list.Activated -= OnAddListActivated;
			add_priority.Activated -= OnAddPriorityActivated;
		}

		public override void OnNoteOpened ()
		{}

		void CheckIfNewTaskNeeded (object sender, System.EventArgs args)
		{
			if (new_task_needed)
			{
				Logger.Debug ("Adding a new Task");
				
				if (deletion_needed)
				{
					//Logger.Debug ("Deleting stuff");
					Gtk.TextIter start = Buffer.GetIterAtMark (Buffer.InsertMark);
					start.BackwardLine();
					
					Gtk.TextIter end = start;
					end.ForwardChars (2);
					
					//TODO: Use the rest of this line as the title of the new task list
						
//					Logger.Debug(Buffer.GetText(start, end, false));
					Buffer.Delete (ref start, ref end);
				}
				
				TaskList tl = new TaskList (Note);
				Children.Add (tl);
				new_task_needed = false;
			}
		}
	
		void BufferInsertText (object o, Gtk.InsertTextArgs args)
		{
			if (args.Text == System.Environment.NewLine) 
			{
				Gtk.TextIter end = args.Pos;
				end.BackwardChars (2);
				
				foreach (Gtk.TextTag tag in end.Tags)
				{
					//Edit: Wow. Now this looks pretty!
					if (tag is TaskTag)
					{
						Logger.Debug ("TaskTag found!");
						deletion_needed = false;
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
					deletion_needed = true;
					new_task_needed = true;
				}
			}
			
			//TODO: also check for tasklist name change
		}
		
		bool IsTextTodoItem (String text)
		{
			//Logger.Debug(text.Trim());
			return text.Trim().Equals("[]");
		}

		void OnAddListActivated (object sender, EventArgs args)
		{
			Gtk.TextIter cursor = Buffer.GetIterAtMark (Buffer.InsertMark);
			cursor.BackwardChar ();
			if (cursor.Char != System.Environment.NewLine)
				Buffer.InsertAtCursor (System.Environment.NewLine);	
			Buffer.InsertAtCursor ("New TaskList!\n");
			
			TaskList tl = new TaskList (Note);
			tl.Name = "New TaskList!";
			Children.Add (tl);
		}

		void OnAddPriorityActivated (object sender, EventArgs args)
		{
			Gtk.TextIter cursor = Buffer.GetIterAtMark (Buffer.InsertMark);
			cursor.BackwardChar ();
			
			foreach (Gtk.TextTag tag in cursor.Tags)
			{
				//Edit: Wow. Now this looks pretty!
				if (tag is TaskTag)
				{
					Logger.Debug ("TaskTag found!");
					TaskTag tasktag = (TaskTag)tag;
					cursor.LineOffset = 0;
					tasktag.Task.InsertPriorityBox(cursor);
				}
			}
			
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
	}
}
