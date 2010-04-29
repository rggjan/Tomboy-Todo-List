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
			Logger.Debug ("Initializing TaskManager");
			item = new Gtk.MenuItem (Catalog.GetString ("Add TaskList"));
			item.Activated += OnMenuItemActivated;
			item.Show ();
			AddPluginMenuItem (item);

			// Register additional Tags
			TaskListTag tlt = new TaskListTag ();
			Note.TagTable.Add (tlt);
			
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
			new TaskList(Note);
		}

	}
}
