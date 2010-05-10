using System;
using Mono.Unix;
using Gtk;
using Tomboy;
using Tomboy.Notebooks;

namespace Tomboy.TaskManager {
	
	class TaskManagerApplicationAddin : ApplicationAddin {
		
		private bool initialized;
		public override bool Initialized {
			get { return initialized; }
		}

		
		/// <summary>
		/// Sets up the TaskManager Addin.
		/// </summary>
		public override void Initialize()
		{
			initialized = true;
			
			
			//NoteRecentChanges search = NoteRecentChanges.GetInstance (Tomboy.DefaultNoteManager);
			
			//OpenTasksNotebook openTasksNotebook = new OpenTasksNotebook ();
			//Gtk.TreeIter iter = NotebookManager.ListNotebooks.Append ();
			//NotebookManager.ListNotebooks.SetValue (iter, 0, openTasksNotebook);
		}
		
		
		/// <summary>
		/// Cleanup when TaskManager is disabled or Tomboy is closed.
		/// </summary>
		public override void Shutdown ()
		{
		}
	}
}