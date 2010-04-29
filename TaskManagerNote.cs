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
			
			string styleMod =
				@"style ""mystyle"" {
				#GtkCheckButton::indicator-spacing = 0
				#GtkCheckButton::focus-padding = 0
				#GtkCheckButton::focus-line-width = 2
				#GtkCheckButton::indicator-size = 100
				}
				widget ""*.tomboy-inline-checkbox"" style ""mystyle""";
			Gtk.Rc.ParseString (styleMod);
		
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
