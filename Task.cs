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
using System.Diagnostics;
using System.Collections.Generic;
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager
{

	/// <summary>
	/// A Task is a piece of text representing a “todo” item,
	/// accompanied with a checkbox.
	/// It may have a due date and a priority and can be
	/// marked as done by crossing out the checkbox.
	/// </summary>
	public class Task : AttributedTask,ITask
	{
		/// <summary>
		/// Description of the Task the user wrote in the Buffer
		/// </summary>
		public string Description
		{ get; set; }
		
		/// <summary>
		/// Tells us where in the NoteBuffer this Task is located.
		/// </summary>
		private Gtk.TextMark Position 
		{ get; set; }
		
		//EDIT: renaming
		/// <summary>
		/// Is this task completed?
		/// </summary>
		public bool Done { 
			get {
				return CheckBox.Active;
			}
			set {
				CheckBox.Active = value;
			}
		}
		
		/// <summary>
		/// Corresponding Widget for Completed Tasks.
		/// </summary>
		private Gtk.CheckButton CheckBox {
			get; set;
		}
		
		/* TODO the getter / setter here have to be hardwired to the corresponding widgets */
		/// <summary>
		/// Date until the task should be completed.
		/// </summary>
		// NOTE: deleted DueDate as already defined in AttributedTask
		private Gtk.Calendar DueDateWidget
		{ get; set; }
		
		
		/// <summary>
		/// TaskList containing this task.
		/// </summary>
		private TaskList ContainingTaskList;
		
		private List<AttributedTask> containers;
		public List<AttributedTask> Containers{
			get{
				return containers;
			}
		}
		
		//TODO
		private List<AttributedTask> SubTasks;
		public List<AttributedTask> Children{
			get{return SubTasks;}
		}
		
		/// <summary>
		/// Just a shortcut for accessing the Notes Buffer
		/// </summary>
		private NoteBuffer Buffer {
			get {
				return ContainingTaskList.ContainingNote.Buffer;
			}
		}
		
		public Task (TaskList containingList, Gtk.TextMark location)
		{
			ContainingTaskList = containingList;
			Position = location;
			Buffer.UserActionEnded += BufferChanged;
			InsertCheckButton (Position);
			
			//Structure
			containers = new List<AttributedTask>();
			containers.Add(ContainingTaskList);
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
			
			CheckBox = new Gtk.CheckButton();
			CheckBox.Name = "tomboy-inline-checkbox";
			CheckBox.Toggled += ToggleCheckBox;
			
			Gtk.TextChildAnchor anchor = Buffer.CreateChildAnchor (ref insertIter);
			ContainingTaskList.ContainingNote.Window.Editor.AddChildAtAnchor (CheckBox, anchor);
			CheckBox.Show ();
			
			Logger.Debug ("Checkbox inserted.");
		}
		
		/// <returns>
		/// A <see cref="Gtk.TextIter"/> marking the beginning of the textual description
		/// of this task in the NoteBuffer.
		/// </returns>
		Gtk.TextIter GetDescriptionStart()
		{
			var start = Buffer.GetIterAtMark (Position);
			int line = start.Line;
			while (start.Line == line && !start.InsideWord ()) {
				Logger.Debug("forwardchar: lineindex" + start.LineIndex + " bytes in line:" + start.BytesInLine );
				start.ForwardChar ();
			}
			if (start.Line != line)
				Debug.Assert(false); // TODO: What to really do here?
			
			return start;
		}
		
		/// <returns>
		/// A <see cref="Gtk.TextIter"/> marking the end of the textual description of this
		/// task in the NoteBuffer.
		/// </returns>
		Gtk.TextIter GetDescriptionEnd () {
			var start = GetDescriptionStart ();
			
			var endIter = Buffer.GetIterAtLineIndex (start.Line, start.BytesInLine-1);
			if(endIter.Char != System.Environment.NewLine) {
				// we do this because if we we construct a TextIter at a newline
				// it will fail because GetIterAtLineIndex is not recognizing this
				// as the same line
				endIter.ForwardChar ();
			}
			
			return endIter;
		}
		
		/// <summary>
		/// Updates the strikethrough tag of the task description. If the checkbox is
		/// active or removes it if it's not. Also applies the tasklist tag.
		/// </summary>
		void TagUpdate ()
		{
			Logger.Debug("Strikethrough Update");
			var start = GetDescriptionStart ();
			Logger.Debug("start.LineIndex:" + start.LineIndex);
			var end = GetDescriptionEnd ();
			Debug.Assert(start.LineIndex < end.LineIndex);
			
			//Logger.Debug ("line " + start.Line + " start index: " + start.LineIndex + " end index: " + end.LineIndex);
		
			if(start.Char != "\n") // Check if a new Task is being created!
			{		
				Buffer.ApplyTag ("tasklist", start, end);
			
				if (CheckBox != null && CheckBox.Active) {
					Buffer.ApplyTag ("strikethrough", start, end);
				} 
				else {
					Buffer.RemoveTag ("strikethrough", start, end);
				}
			}
		}
		
		/// <summary>
		/// Called when the buffer is changed. Currently this watches for changes in the Task
		/// description and updates the strikethrough task.
		/// </summary>
		void BufferChanged(object sender, EventArgs e) 
		{
			Debug.Assert(Buffer == sender); // no other buffer should be registred here
			
			Logger.Debug("buffer has changed");
			int line = Buffer.GetIterAtMark(Buffer.InsertMark).Line;
			
			// update strikethrough
			if(line == GetDescriptionStart().Line) {
				TagUpdate ();
			}

		}

		void ToggleCheckBox (object sender, EventArgs e)
		{
			Logger.Debug ("Toggled CheckBox");
			Debug.Assert (CheckBox == sender); // no other checkbox should be registred here
			TagUpdate ();
			
			// TODO some signalling here?
		}
		
	}
}
