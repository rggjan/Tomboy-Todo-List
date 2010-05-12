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
		
		public int Priority {
			get {
				if (priority_box != null)
				{
					Logger.Debug ("reading priority");
					Logger.Debug (priority_box.ActiveText);
					return priority_box.Active;
				}
				else
					return 0;
			}
			set {
				priority_box.Active = value;
			}
		}
		
		/// <summary>
		/// Corresponding Widget for Completed Tasks.
		/// </summary>
		private Gtk.CheckButton CheckBox {
			get; set;
		}
		Gtk.TextChildAnchor boxanchor;
		
		private Gtk.ComboBox priority_box;

		
		/// <summary>
		/// Date until the task should be completed.
		/// </summary>
		// NOTE: deleted DueDate as already defined in AttributedTask
		private Gtk.Calendar DateWidget {
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
		
		
		public Task (TaskList containingList, Gtk.TextIter location)
		{
			initialize (containingList, location, (TaskTag)containingList.ContainingNote.TagTable.CreateDynamicTag ("task"));

			Tag.bind (this);
			
			AddWidgets ();
			TagUpdate ();
			
			var end = GetTaskStart ();
			end.ForwardChars (2);
			Buffer.PlaceCursor (end);
		}
		
		public void AddWidgets ()
		{
			InsertCheckButton (GetTaskStart ());
			InsertPriorityBox (GetTaskStart ());
			
			var end = GetTaskStart ();
			var start = end;
			end.ForwardChar ();
			start.BackwardChar ();
			Buffer.ApplyTag ("locked", start, end);
		}
		
		public Task (TaskList containingList, Gtk.TextIter location, TaskTag tag)
		{
			initialize(containingList, location, tag);
		}
		
		private void initialize (TaskList containingList, Gtk.TextIter location, TaskTag tag)
		{
			Containers = new List<AttributedTask> ();
			Containers.Add (ContainingTaskList);
			
			ContainingTaskList = containingList;
			location.LineOffset = 0;
			Position = Buffer.CreateMark (null, location, true);
			
			Buffer.UserActionEnded += BufferChanged;
			
			Tag = tag;
			Tag.Task = this;
		}
	
		/// <summary>
		/// Inserts a CheckButton at cursor position.
		/// </summary>
		private void InsertCheckButton (Gtk.TextIter insertIter)
		{
			//Gtk.TextIter insertIter = Buffer.GetIterAtMark (Buffer.InsertMark);
			
			CheckBox = new Gtk.CheckButton ();
			CheckBox.Name = "tomboy-inline-checkbox";
			CheckBox.Toggled += ToggleCheckBox;
			
			boxanchor = Buffer.CreateChildAnchor (ref insertIter);
			ContainingTaskList.ContainingNote.Window.Editor.AddChildAtAnchor (CheckBox, boxanchor);
			CheckBox.Show ();
		}

		public void test (object o, System.EventArgs args)
		{
			Logger.Debug ("destroyed");
		}
		
		/// <summary>
		/// Inserts the pr or;ty ComboBox in the TextBuffer.
		/// </summary>
		/// <param name="at"> Where to insert (exactly). </param>
		/// <returns>
		/// A TextIter
		/// </returns>
		private void InsertPriorityBox (Gtk.TextIter insertIter)
		{
			string[] priorities = { "1", "2", "3", "4", "5" };
			priority_box = new Gtk.ComboBox (priorities);
			priority_box.Active = Priority;
			priority_box.Name = "tomboy-inline-combobox";

			//priority_box.Changed += setpriority();

			Gtk.TextChildAnchor anchor = Buffer.CreateChildAnchor (ref insertIter);
			ContainingTaskList.ContainingNote.Window.Editor.AddChildAtAnchor (priority_box, anchor);
				
		/*		var end = insertIter;
				var start = insertIter;
				start.BackwardChar ();
				
				Buffer.ApplyTag ("invisible", start, end);*/
				//priority_box.Show ();
		}
		
		public void ShowPriority ()
		{
			priority_box.Show ();
		}
		
		/// <returns>
		/// A <see cref="Gtk.TextIter"/> marking the beginning of the textual description
		/// of this task in the NoteBuffer.
		/// </returns>
		public Gtk.TextIter GetTaskStart ()
		{
			var start = Buffer.GetIterAtMark (Position);
			//start.LineOffset = 0;
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
			//var end = Buffer.GetIterAtLine (start.Line);
			//end.ForwardToLineEnd ();

			//var endIter = Buffer.GetIterAtLine (start.Line);
			//endIter.ForwardToLineEnd ();
			start.ForwardLine ();
			//start.ForwardLine ();
			return start;
		}
		
		/// <summary>
		/// Updates the strikethrough tag of the task description. If the checkbox is
		/// active or removes it if it's not. Also applies the tasklist tag.
		/// </summary>
		private void TagUpdate ()
		{
			var start = GetTaskStart ();
			var end = GetTaskEnd ();
			//start.ForwardChar ();

			
			Tag.bind (this);
			start.ForwardChar ();
			Buffer.ApplyTag (Tag, start, end);
			
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
			if (boxanchor.Deleted)
			{
				Logger.Debug("destroying");
				CheckBox.Destroy();
			}
			/*Debug.Assert (Buffer == sender); // no other buffer should be registred here	
			int line = Buffer.GetIterAtMark (Buffer.InsertMark).Line;

			if (line == GetTaskStart ().Line) {
				TagUpdate ();
			}*/
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
