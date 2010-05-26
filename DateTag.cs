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


	public class DateTag : NoteTag
	{
		
		private Calendar calendar;
		private TextRange range;
		private NoteEditor editor;
		
		public DateTag (string name) : base (name)
		{
		}
		
		public override void Initialize (string element_name)
		{
			base.Initialize (element_name);

			//Underline = Pango.Underline.Single;
			Foreground = "purple";
			CanActivate = true;
			CanGrow = true;
		}
		
		protected override bool OnActivate (NoteEditor editor, Gtk.TextIter start, Gtk.TextIter end)
		{
			calendar = new Calendar ();
			range = new TextRange (start, end);
			this.editor = editor;
			
			Dialog dialog = new Dialog ();
			dialog.Modal = true;
			dialog.VBox.Add (calendar);
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
			
			TextIter start = range.Start;
			TextIter end = range.End;
			
			string newdate = calendar.GetDate ().ToShortDateString ();
			editor.Buffer.Delete (ref start, ref end);
			editor.Buffer.InsertWithTags (ref start, newdate, new TextTag[]{this});
		}
	}
}
