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

		void OnAddPriorityActivated (object sender, EventArgs args)
		{
			Gtk.TextIter cursor = Buffer.GetIterAtMark (Buffer.InsertMark);
			cursor.BackwardChar ();
			
			foreach (Gtk.TextTag tag in cursor.Tags) {
				if (tag is TaskTag) {
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
