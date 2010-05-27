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
using Gtk;

namespace Tomboy.TaskManager
{


	public class PriorityTag : NoteTag
	{
		private ComboBox box;
		private Task task;
		private TaskManagerNoteAddin addin;
		
		public PriorityTag (string name, TaskManagerNoteAddin addin) : base (name)
		{
			this.addin = addin;
		}
		
		public override void Initialize (string element_name)
		{
			base.Initialize (element_name);

			CanActivate = true;
			Foreground = "blue";
			Family = "monospace";
			CanSerialize = false;
		}
		
		protected override bool OnActivate (NoteEditor editor, Gtk.TextIter start, Gtk.TextIter end)
		{
			string[] prios = new string[6];
			for (int i=0;i<6;i++)
				prios[i] = i.ToString ()+" ("+PriorityUtils.PrettyString ((Priority)i)+")";
			box = new ComboBox (prios);
			
			//Member priority is shadowing the enum, therefore explicit here)
			box.Active = (int)TaskManager.Priority.NORMAL;
			
			task = addin.Utils.GetTask (start);
			if (task.Priority == TaskManager.Priority.UNSET){
				GetAvPriorityVisitor visitor = new GetAvPriorityVisitor ();
				
				visitor.visit (task);
				box.Active = visitor.IntResult;
			}
			
			Dialog dialog = new Dialog ();
			dialog.Modal = true;
			dialog.VBox.Add (box);
			dialog.VBox.ShowAll ();
			dialog.AddButton ("OK", ResponseType.Ok);
			dialog.AddButton ("Cancel", ResponseType.Cancel);
			
			dialog.Response += new ResponseHandler (on_dialog_response);
			dialog.Run ();
			dialog.Destroy ();
			
			return true;
		}
		
		void on_dialog_response (object obj, ResponseArgs args)
		{	
			if (args.ResponseId != ResponseType.Ok)
				return;
			
			task.Priority = (Priority) box.Active;
			addin.OnPriorityClicked (task);
		}
	}
}
