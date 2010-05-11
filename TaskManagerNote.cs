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
			tag.Family = "Arial";

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

		public override void OnNoteOpened ()
		{
			tasklist.Submenu = task_menu;
			add_list.Activated += OnAddListActivated;
			add_priority.Activated += OnAddPriorityActivated;
			add_priority.Sensitive = false;
			
			Buffer.InsertText += BufferInsertText;
			Buffer.MarkSet += BufferMarkSet;
			Buffer.UserActionEnded += CheckIfNewTaskNeeded;
			Buffer.UserActionEnded += UpdateMenuSensitivity;

			task_menu.Add(add_list);
			task_menu.Add(add_priority);
			tasklist.ShowAll();
			
			AddPluginMenuItem (tasklist);
			
			//Initialise tasklists list
			//TODO: get from previous sessions?
			Children = new List<AttributedTask> ();
			
			Load ();
		}
		
		/// <summary>
		/// Makes sure that add_list and add_priority menu items Sensitive property
		/// is set correctly according to where we currently are in the NoteBuffer
		/// </summary>
		void UpdateMenuSensitivity (object sender, EventArgs args) {
			
			Logger.Debug("UpdateMenuSensitivity");
			Gtk.TextIter cursor = Buffer.GetIterAtMark (Buffer.InsertMark);
			cursor.LineOffset = 0;
			
			bool inTaskList = false;
			foreach (Gtk.TextTag tag in cursor.Tags) {
				if(tag is TaskListTag) {
					Logger.Debug("found tasklisttag");
					inTaskList = true;
					break;
				}
			}
			
			// toggle sensitivity
			if(inTaskList) {
				add_priority.Sensitive = true;
				add_list.Sensitive = false;
			}
			else {
				add_priority.Sensitive = false;
				add_list.Sensitive = true;
			}

			
		}

		void OnAddListActivated (object sender, EventArgs args)
		{
			TaskList tl = new TaskList (Note);
			
			add_list.Sensitive = false;
			add_priority.Sensitive = true;
			
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
					tasktag.Task.ShowPriority ();
					add_priority.Sensitive = false;
				}
			}
		
		}
		
		/*Task GetTaskAtCursor ()
		{
			Gtk.TextIter here = Buffer.GetIterAtMark (Buffer.InsertMark);
			here.LineOffset = 0;
		}*/
		
		void BufferInsertText (object o, Gtk.InsertTextArgs args)
		{
			if (args.Text == System.Environment.NewLine) {
				Gtk.TextIter end = args.Pos;
				end.BackwardChar ();
				
				var begin = end;
				begin.LineOffset = 0;
				
				if (Buffer.GetText (begin, end, true).Trim () == "X") {
					//FIXME better way to do this
					Buffer.Delete (ref begin, ref end);
					return;
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
				//end.ForwardChars (5);
				
				Gtk.TextIter start = args.Pos;
				start.BackwardLine ();
				
				//end = start;
				//end.ForwardChars (2);
				
				//Logger.Debug ("Before new Line: "+Buffer.GetText(start, end, false));
				
				if (IsTextTodoItem (Buffer.GetText (start, end, false))) {
					current_task = null;
					new_task_needed = true;
				}
			}
		}
			
		void CheckIfNewTaskNeeded (object sender, System.EventArgs args)
		{
			if (new_task_needed) {
				Logger.Debug ("Adding a new Task");
				
				if (current_task == null) {
					//Logger.Debug ("Deleting stuff");
					Gtk.TextIter start = Buffer.GetIterAtMark (Buffer.InsertMark);
					Gtk.TextIter end = start;
					
					start.BackwardLine ();
					//end.ForwardChars (2);
					
					//TODO: Use the rest of this line as the title of the new task list
					
					// Logger.Debug(Buffer.GetText(start, end, false));
					Buffer.Delete (ref start, ref end);
					
					Children.Add (new TaskList (Note));
					
				} else {
					var iter = Buffer.GetIterAtMark (Buffer.InsertMark);
					var end = iter;
					end.ForwardToLineEnd ();
					
					foreach (Gtk.TextTag tag in iter.Tags) {
						if (tag is TaskTag) {
							Logger.Debug ("removing old tasktag");
							Buffer.RemoveTag (tag, iter, end);
						}
					}
					
				//	current_task.addTask (Buffer.InsertMark);
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
			Logger.Debug ("Loading...");
			TextIter iter = Buffer.StartIter;
			do {
				TaskListTag taskliststart = (TaskListTag)Buffer.GetDynamicTag ("tasklist", iter);
				if (taskliststart != null)
				{
					Logger.Debug ("=> found Tasklist!");

					TaskList tl = new TaskList (Note, iter, taskliststart);
					Children.Add (tl);
					
					TaskTag start;
					do
					{
						start = (TaskTag)Buffer.GetDynamicTag ("task", iter);
						iter.ForwardChar ();
					} while (start == null);
					
					Logger.Debug ("=> found Tasktag!");
					tl.addTask (iter, start);
					
					TaskTag end = start;
					while (end == start) {
						iter.ForwardChar ();
						end = (TaskTag)Buffer.GetDynamicTag ("task", iter);
					}
				}
			} while (iter.ForwardChar ());
			
			foreach (TaskList tl in Children)
			{
				foreach (Task t in tl.Children)
				{
					t.AddWidgets ();
				}
			}
		}
		
	}
}
