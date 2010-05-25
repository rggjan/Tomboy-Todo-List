// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//   
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//   
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  
// Authors:
//       Jan Rüegg <rggjan@gmail.com>
//       Gabriel Walch <walchg@student.ethz.ch>
//       Gerd Zellweger <mail@gerdzellweger.com>
// 

using System;
using Mono.Unix;
using Gtk;
using Tomboy;
using Tomboy.Notebooks;

namespace Tomboy.TaskManager {
	
	/// <summary>
	/// Class that describes what to do (in terms of taskmanager addin) at tomboy startup
	/// </summary>
	class TaskManagerApplicationAddin : ApplicationAddin {
		
		/// <summary>
		/// Returns true iff TaskManager addin has been initialized
		/// </summary>
		bool initialized;
		public override bool Initialized {
			get {return initialized;}
		}
		
		/// <summary>
		/// Sets up the TaskManager Addin.
		/// </summary>
		public override void Initialize()
		{
			if (!Initialized)
			{
				Logger.Debug("TaskManagerApplicationAddin.Initialize");
				//NoteRecentChanges search = NoteRecentChanges.GetInstance (Tomboy.DefaultNoteManager);
				
				OpenTasksNotebook openTasksNotebook = new OpenTasksNotebook ();
				NotebookManager.AddSpecialNotebook(openTasksNotebook);
				
				OverdueTasksNotebook overdueTasksNotebook = new OverdueTasksNotebook ();
				NotebookManager.AddSpecialNotebook(overdueTasksNotebook);
				
				
				
				string checkButtonStyleMod = @"style ""mystyle"" {
												GtkCheckButton::indicator-spacing = 0
												GtkCheckButton::focus-padding = 0
												GtkCheckButton::focus-line-width = 0
												GtkCheckButton::indicator-size = 13
											}
											widget ""*.tomboy-inline-checkbox"" style ""mystyle""
										 ";
			
				Gtk.Rc.ParseString (checkButtonStyleMod);
				initialized = true;
			}
			
		}
		
		
		/// <summary>
		/// Cleanup when TaskManager is disabled or Tomboy is closed.
		/// </summary>
		public override void Shutdown ()
		{
			// TODO unregister notebooks
		}
	}
}