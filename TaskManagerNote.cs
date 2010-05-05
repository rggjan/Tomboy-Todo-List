using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager {
	
	public class TaskManagerNoteAddin : NoteAddin, ITask {
		
		Gtk.MenuItem item;

		bool initialized = false;
		bool new_task_needed = false;
		bool deletion_needed = false;
		
		public override void Initialize ()
		{
		}

		public override void Shutdown ()
		{
			item.Activated -= OnMenuItemActivated;
		}

		public override void OnNoteOpened ()
		{
		
			if (!initialized)
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
				
				item = new Gtk.MenuItem (Catalog.GetString ("Add TaskList"));
				item.Activated += OnMenuItemActivated;
				item.Show ();
				AddPluginMenuItem (item);

				Buffer.UserActionEnded += CheckIfNewTaskNeeded;
				Buffer.InsertText += BufferInsertText;
				
				//Initialise tasklists list
				//TODO: get from previous sessions?
				Children = new List<AttributedTask> ();
				
				initialized = true;
			}
		}

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

		void OnMenuItemActivated (object sender, EventArgs args)
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
