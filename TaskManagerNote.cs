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
			
/*			NoteTag tag;
			tag = new NoteTag ("abctag");
//tag.Justification = Gtk.Justification.Center;
			tag.Scale = 10;
			tag.CanUndo = true;
			tag.CanGrow = true;
			tag.CanSpellCheck = true;
			Buffer.TagTable.Add (tag);*/
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
		
		void ToggleCheckBox (object sender, EventArgs e)
		{
			NoteBuffer buffer = Note.Buffer;
			buffer.ApplyTag ("strikethrough", buffer.StartIter, buffer.EndIter);
		}
		
		void InsertTaskList ()
		{
			var checkbox = new Gtk.CheckButton ();
			checkbox.Name = "tomboy-inline-checkbox";
			
			checkbox.Toggled += ToggleCheckBox;
			
			NoteBuffer buffer = Note.Buffer;
			
			Gtk.TextMark cursor = buffer.GetMark ("insert");
			TextIter iter = buffer.GetIterAtMark (cursor);
			Gtk.TextChildAnchor anchor = Buffer.CreateChildAnchor (ref iter);
			Note.Window.Editor.AddChildAtAnchor (checkbox, anchor);
			checkbox.Show ();
			
			//buffer.ApplyTag ("abctag", buffer.StartIter, buffer.EndIter);
		}
	}
}
