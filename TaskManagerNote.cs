using System;
using Mono.Unix;
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager
{
	public class TaskManagerNoteAddin : NoteAddin
	{
		Gtk.MenuItem item;

		public override void Initialize ()
		{
			item = new Gtk.MenuItem (Catalog.GetString ("Add TaskList"));
			item.Activated += OnMenuItemActivated;
			item.Show ();
			AddPluginMenuItem (item);
		}

		public override void Shutdown ()
		{
			item.Activated -= OnMenuItemActivated;
		}

		public override void OnNoteOpened ()
		{
		}

		void OnMenuItemActivated (object sender, EventArgs args)
		{

			InsertTaskList ();
		}
		
		void InsertTaskList ()
		{
			TaskList taskList = new TaskList(Note);
		}
	}
}
