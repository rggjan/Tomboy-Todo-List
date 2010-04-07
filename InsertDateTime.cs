using System;
using Mono.Unix;
using Gtk;
using Tomboy;

namespace Tomboy.InsertDateTime
{
	public class InsertDateTimeAddin : NoteAddin
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
			string format = Catalog.GetString ("dddd, MMMM d, h:mm tt");
			string text = DateTime.Now.ToString (format);
			
			NoteBuffer buffer = Note.Buffer;
			Gtk.TextIter cursor = buffer.GetIterAtMark (buffer.InsertMark);
			buffer.InsertWithTagsByName (ref cursor, text, "datetime");
			
		}
	}
}
