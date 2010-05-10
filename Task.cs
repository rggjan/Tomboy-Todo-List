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
using System.Xml;

namespace Tomboy.TaskManager {

	/// <summary>
	/// A Task is a piece of text representing a “todo” item,
	/// accompanied with a checkbox.
	/// It may have a due date and a priority and can be
	/// marked as done by crossing out the checkbox.
	/// </summary>
	public class Task : AttributedTask {
		
		
		/// <summary>
		/// Description of the Task the user wrote in the Buffer
		/// </summary>
		public string Description {
			get; set;
		}
		
		/// <summary>
		/// Tells us where in the NoteBuffer this Task is located.
		/// </summary>
		private Gtk.TextMark Position { 
			get; set; 
		}
		
		/// <summary>
		/// Is this task completed?
		/// </summary>
		public bool Done {
			get {
				if (CheckBox != null)
					return CheckBox.Active;
				else
					return false;
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
		
		/// <summary>
		/// Date until the task should be completed.
		/// </summary>
		// NOTE: deleted DueDate as already defined in AttributedTask
		private Gtk.Calendar DueDateWidget {
			get; set; 
		}
		
		
		/// <summary>
		/// TaskList containing this task.
		/// </summary>
		private TaskList containing_task_list;
		
		private List<AttributedTask> containers;
		public override List<AttributedTask> Containers {
			get {
				if (containers == null){
					List<AttributedTask> result = new List<AttributedTask> ();
					result.Add (containing_task_list);
				}
				return containers;
			}
			set { 
				containers = value; 
			}
		}
		
		//TODO
		public override List<AttributedTask> Children {
			get; set;
		}
		
		/// <summary>
		/// Just a shortcut for accessing the Notes Buffer
		/// </summary>
		private NoteBuffer Buffer {
			get {
				return ContainingTaskList.ContainingNote.Buffer;
			}
		}
		
		public TaskList ContainingTaskList {
			get {
				return containing_task_list;
			}
			set {
				containing_task_list = value;
			}
		}
		
		private TaskTag Tag 
		{ get; set; }
		
		
		public Task (TaskList containingList, Gtk.TextMark location) 
			: this (containingList, location, (TaskTag) containingList.ContainingNote.TagTable.CreateDynamicTag ("task"))
		{
		}
		
		public Task (TaskList containingList, Gtk.TextMark location, TaskTag tag)
		{
			ContainingTaskList = containingList;
			Position = location;
			Buffer.UserActionEnded += BufferChanged;
			
			Tag = tag;
			Tag.bind (this);
			
			InsertCheckButton ();
			
			//Buffer.InsertWithTags (GetDescriptionStart (), "Testtask", new TextTag[] {tt});
			
			//Structure
			//Containers = new List<AttributedTask> ();
			//Containers.Add (ContainingTaskList);
		}
	
		/// <summary>
		/// Inserts a CheckButton in the TextBuffer.
		/// </summary>
		/// <param name="insertIter">
		/// <see cref="Gtk.TextIter"/> Where to insert (exactly).
		/// </param>
		private void InsertCheckButton ()
		{
			Gtk.TextIter insertIter = Buffer.GetIterAtMark (Position);
			insertIter.LineOffset = 0;
			
			CheckBox = new Gtk.CheckButton ();
			CheckBox.Name = "tomboy-inline-checkbox";
			CheckBox.Toggled += ToggleCheckBox;
			
			Gtk.TextChildAnchor anchor = Buffer.CreateChildAnchor (ref insertIter);
			ContainingTaskList.ContainingNote.Window.Editor.AddChildAtAnchor (CheckBox, anchor);
			CheckBox.Show ();
		
			var start = insertIter;
			start.BackwardChars (2);
			var end = insertIter;
			Buffer.ApplyTag ("locked", start, end);

			Buffer.InsertWithTagsByName (ref insertIter, "X", "invisible");
			
			start = insertIter;
			end = insertIter;
			start.BackwardChars (2);
			
			Buffer.RemoveTag ("locked", start, end);
			
			TagUpdate ();
		}
		
		/// <summary>
		/// Inserts the pr or;ty ComboBox in the TextBuffer.
		/// </summary>
		/// <param name="at"> Where to insert (exactly). </param>
		/// <returns>
		/// A TextIter
		/// </returns>
		public TextIter InsertPriorityBox (TextIter insertIter)
		{
			string[] priorities = { "1", "2", "3", "4", "5" };
			var box = new Gtk.ComboBox (priorities);
			box.Name = "tomboy-inline-combobox";

			Gtk.TextChildAnchor anchor = Buffer.CreateChildAnchor (ref insertIter);
			ContainingTaskList.ContainingNote.Window.Editor.AddChildAtAnchor (box, anchor);
			box.Show ();
			return insertIter;
		}
		
		/// <returns>
		/// A <see cref="Gtk.TextIter"/> marking the beginning of the textual description
		/// of this task in the NoteBuffer.
		/// </returns>
		public Gtk.TextIter GetTaskStart ()
		{
			var start = Buffer.GetIterAtMark (Position);
			start.LineOffset = 0;
			//while ((start.LineIndex < start.BytesInLine) && !start.InsideWord ()) {
			//	start.ForwardCursorPosition ();
			//}
						
			return start;
		}
		
		/// <returns>
		/// A <see cref="Gtk.TextIter"/> marking the end of the textual description of this
		/// task in the NoteBuffer.
		/// </returns>
		public Gtk.TextIter GetTaskEnd ()
		{
			var start = GetTaskStart ();

			var endIter = Buffer.GetIterAtLine (start.Line);
			endIter.ForwardToLineEnd ();
			
			return endIter;
		}
		
		/// <summary>
		/// Updates the strikethrough tag of the task description. If the checkbox is
		/// active or removes it if it's not. Also applies the tasklist tag.
		/// </summary>
		private void TagUpdate ()
		{
			var start = GetTaskStart ();
			var end = GetTaskEnd ();

			//Logger.Debug ("line " + start.Line + " start index: " + start.LineIndex + " end index: " + end.LineIndex);
			Buffer.ApplyTag (Tag, GetTaskStart (), GetTaskEnd ());
			
			Tag.bind(this);
			
			if (CheckBox != null && CheckBox.Active) {
				Buffer.ApplyTag ("strikethrough", start, end);
			} 
			else {
				Buffer.RemoveTag ("strikethrough", start, end);
			}
		}
		
		/// <summary>
		/// Called when the buffer is changed. Currently this watches for changes in the Task
		/// description and updates the strikethrough task.
		/// </summary>
		private void BufferChanged (object sender, EventArgs e)
		{
			Debug.Assert (Buffer == sender); // no other buffer should be registred here	
			int line = Buffer.GetIterAtMark (Buffer.InsertMark).Line;

			if (line == GetTaskStart ().Line) {
				TagUpdate ();
			}
			
		}

		/// <summary>
		/// Signal when checkbutton for the task was clicked.
		/// This function is responsible for updating strikethrough functionality.
		/// </summary>
		private void ToggleCheckBox (object sender, EventArgs e)
		{
			Debug.Assert (CheckBox == sender); // no other checkbox should be registred here
			TagUpdate ();
			
			// TODO some signalling here?
		}
		
	}
}
