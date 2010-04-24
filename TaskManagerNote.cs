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
			item = new Gtk.MenuItem (Catalog.GetString ("Insert date and time"));
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
			NoteBuffer buffer = Note.Buffer;
//	buffer.InsertAtCursor (Tomboy.ActionManager.UI.Ui);
			buffer.InsertAtCursor ("This is a test...");
			
		}
	}
}
