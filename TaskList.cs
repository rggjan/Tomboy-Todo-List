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
using Tomboy;

namespace Tomboy.TaskManager
{

	/// <summary>
	/// This class represents the TaskLists in Notes. It handles all the communication with the
	/// NoteBuffer.
	/// </summary>
	public class TaskList
	{
		
		/// <summary>
		/// Marks the Start of the TaskList in containingNote Buffer.
		/// </summary>	
		public Gtk.TextMark Start {
			get; set;
		}
		
		/// <summary>
		/// Note containing the TaskList.
		/// </summary>
		private Note ContainingNote {
			get; set;
		}
		
		private NoteBuffer Buffer {
			get {
				return ContainingNote.Buffer;
			}
		}
		
		/// <summary>
		/// Sets up the TaskList.
		/// </summary>
		/// <param name="note">
		/// <see cref="Note"/> where the TaskLists is located.
		/// </param>
		public TaskList (Note note)
		{
			Logger.Debug("TaskList created");
			ContainingNote = note;
			
			Start = Buffer.InsertMark;
			
			// TODO set and EndIter correctly
			/*Buffer.ApplyTag (TaskListTag.NAME, 
			                 Buffer.GetIterAtMark(Start), 
							 Buffer.EndIter);*/
			
			string styleMod =
				@"style ""mystyle"" {
				#GtkCheckButton::indicator-spacing = 0
				#GtkCheckButton::focus-padding = 0
				#GtkCheckButton::focus-line-width = 2
				#GtkCheckButton::indicator-size = 100
				}
				widget ""*.tomboy-inline-checkbox"" style ""mystyle""";
			Gtk.Rc.ParseString (styleMod);
			// First we need a checkbox
			InsertCheckButton(Start);
		}
		
		/// <summary>
		/// Inserts a CheckButton in the TextBuffer.
		/// </summary>
		/// <param name="at">
		/// <see cref="Gtk.TextMark"/> Where to insert.
		/// </param>
		void InsertCheckButton (Gtk.TextMark at)
		{
			TextIter insertIter = Buffer.GetIterAtMark(at);
			insertIter.BackwardChars (insertIter.LineOffset); // go to beginning of the line
			
			var checkbox = new Gtk.CheckButton ();
			checkbox.Name = "tomboy-inline-checkbox";
			checkbox.Toggled += ToggleCheckBox;
			
			Gtk.TextChildAnchor anchor = Buffer.CreateChildAnchor (ref insertIter);
			ContainingNote.Window.Editor.AddChildAtAnchor (checkbox, anchor);
			checkbox.Show();
			
			Logger.Debug("Checkbox inserted.");
		}

		void ToggleCheckBox (object sender, EventArgs e)
		{
			Logger.Debug("Toggled");
			Buffer.ApplyTag ("strikethrough", Buffer.StartIter, Buffer.EndIter);
		}
		
	}

	/// <summary>
	/// Marks a TaskList in a NoteBuffer. Currently this only sets the background to green for
	/// better debugging.
	/// </summary>
	public class TaskListTag : NoteTag
	{
		public static String NAME = "tasklist";
		
		public TaskListTag () : base(TaskListTag.NAME)
		{
			Background = "green";
			LeftMargin = 3;
			LeftMarginSet = true;
			CanSerialize = false;
			CanSpellCheck = true;
		}
	}


}
