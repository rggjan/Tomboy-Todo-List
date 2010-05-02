using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager
{
	public class TaskManagerNoteAddin : NoteAddin, ITask
	{
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
			
				item = new Gtk.MenuItem (Catalog.GetString ("Add TaskList"));
				item.Activated += OnMenuItemActivated;
				item.Show ();
				AddPluginMenuItem (item);
	
				// Register additional Tags
				TaskListTag tlt = new TaskListTag ();
				Note.TagTable.Add (tlt);

				Buffer.UserActionEnded += CheckIfNewTaskNeeded;
				Buffer.InsertText += BufferInsertText;
				
				//Initialise tasklists list
				//TODO: get from previous sessions?
				TaskLists = new List<AttributedTask>();
				
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
					Gtk.TextIter end = Buffer.GetIterAtMark (Buffer.InsertMark);
					Gtk.TextIter start = end;
					start.BackwardLine();
						
//					Logger.Debug(Buffer.GetText(start, end, false));
					Buffer.Delete (ref start, ref end);
				}
				
				TaskList tl = new TaskList (Note);
				TaskLists.Add(tl);
				new_task_needed = false;
			}
		}
	
		void BufferInsertText (object o, Gtk.InsertTextArgs args)
		{
			if (args.Text == "\n") 
			{
				Gtk.TextIter end = args.Pos;
				end.BackwardChars(2);
				
				foreach (Gtk.TextTag tag in end.Tags)
				{
					if (tag.Name == "tasklist")
					{
						Logger.Debug("tasklist Tag found!");
						deletion_needed = false;
						new_task_needed = true;
						return;
					}
				}
				
				end.ForwardChar();
				Gtk.TextIter start = end;
				start.LineOffset = 0;
				
				//Logger.Debug(Buffer.GetText(start, end, false));
				
				if(IsTextTodoItem(Buffer.GetText(start, end, false)))
				{
					deletion_needed = true;
					new_task_needed = true;
				}
			}
		}
		
		bool IsTextTodoItem (String text)
		{
			//Logger.Debug(text.Trim());
			return text.Trim().Equals("[]");
		}

		void OnMenuItemActivated (object sender, EventArgs args)
		{
			TaskList tl = new TaskList(Note);
			TaskLists.Add(tl);
		}

		private List<AttributedTask> TaskLists;
		public List<AttributedTask> Children{
			get{return TaskLists;}	
		}
		
		//TODO
		private List<AttributedTask> LinkingTasks;
		public List<AttributedTask> Containers{
			get{return LinkingTasks;}	
		}
		
		//TODO
		public bool Done{
			get;
			set;
		}
	}
}
