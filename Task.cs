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
		public override bool Done {
			get {
				return (CheckBox != null && CheckBox.Active);
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
		
		//TODO: Subtasks
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
		
		/// <summary>
		/// The containing task list
		/// </summary>
		public TaskList ContainingTaskList {
			get {
				return containing_task_list;
			}
			set {
				containing_task_list = value;
			}
		}
		
		/// <summary>
		/// Gets the tag attached to this task. Shortcut.
		/// </summary>
		private TaskTag TaskTag {
			get{ return (TaskTag) Tag; }
			set{ Tag = value; }
		}
		
		
		public Task (TaskList containingList, Gtk.TextIter location)
		{
			//TODO: rewrite tag part (it's ugly)
			initialize (containingList, location, (TaskTag)containingList.ContainingNote.TagTable.CreateDynamicTag ("task"));

			TaskTag.bind (this);
			
			AddWidgets ();
			TagUpdate ();
			
			var end = GetTaskStart ();
			end.ForwardChars (2);
			Buffer.PlaceCursor (end);
		}
		
		/// <summary>
		/// Adds all widgets (besides calendar) to this task
		/// </summary>
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
		
		/// <summary>
		/// Initialize fields, tag, etc
		/// </summary>
		/// <param name="containingList">
		/// A <see cref="TaskList"/>
		/// </param>
		/// <param name="location">
		/// A <see cref="Gtk.TextIter"/>
		/// </param>
		/// <param name="tag">
		/// A <see cref="TaskTag"/>
		/// </param>
		private void initialize (TaskList containingList, Gtk.TextIter location, TaskTag tag)
		{
			Containers = new List<AttributedTask> ();
			Containers.Add (ContainingTaskList);
			
			ContainingTaskList = containingList;
			location.LineOffset = 0;
			Position = Buffer.CreateMark (null, location, true);
			
			Buffer.UserActionEnded += BufferChanged;
			
			TaskTag = tag;
			TaskTag.Task = this;
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

		//TODO: do we need this?
		public void test (object o, System.EventArgs args)
		{
			Logger.Debug ("destroyed");
		}
		
		/// <summary>
		/// Resets the priority of this task
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="System.EventArgs"/>
		/// </param>
		private void setpriority (object o, System.EventArgs args)
		{
			Priority = (Priorities)priority_box.Active;
			TagUpdate ();
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
			priority_box.Active = (int)Priority;
			priority_box.Name = "tomboy-inline-combobox";

			priority_box.Changed += setpriority;

			Gtk.TextChildAnchor anchor = Buffer.CreateChildAnchor (ref insertIter);
			ContainingTaskList.ContainingNote.Window.Editor.AddChildAtAnchor (priority_box, anchor);
				
		/*		var end = insertIter;
				var start = insertIter;
				start.BackwardChar ();
				
				Buffer.ApplyTag ("invisible", start, end);*/
				//priority_box.Show ();
		}
		
		/// <summary>
		/// Makes the priority widget visible
		/// </summary>
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
			return start;
		}
		
		/// <returns>
		/// A <see cref="Gtk.TextIter"/> marking the end of the textual description of this
		/// task in the NoteBuffer.
		/// </returns>
		public Gtk.TextIter GetTaskEnd ()
		{
			var start = GetTaskStart ();
			start.ForwardLine ();
			//TODO: backwardchar here?
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
		
			//Tag.bind (this);
			start.ForwardChar ();
			Buffer.ApplyTag (TaskTag, start, end);
			
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
				// Delete (); //FIXME
			}
			/*Debug.Assert (Buffer == sender); // no other buffer should be registred here	
			int line = Buffer.GetIterAtMark (Buffer.InsertMark).Line;

			if (line == GetTaskStart ().Line) {
				TagUpdate ();
			}*/
		}
		
		/// <summary>
		/// Returns true iff this task is the last (in means of buffer offset) one in containingtasklist
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool IsLastTask ()
		{
			foreach (Task task in ContainingTaskList.Children)
			{
				if (Buffer.GetIterAtMark (task.Position).Line > Buffer.GetIterAtMark (Position).Line)
				{
					Logger.Debug ("is not last!");
					return false;
				}
			}
			Logger.Debug ("is last!");
			return true;
		}

		/// <summary>
		/// Delete this task, it's widgets and corresponding tag representation
		/// </summary>
		public void Delete ()
		{
			if (!Position.Deleted)
			{
				var start = Buffer.GetIterAtMark (Position);
				var end = start;
				end.ForwardToLineEnd ();
				
				Buffer.Delete (ref start, ref end);
				
				start = Buffer.GetIterAtMark (Position);
				end = start;
				//start.ForwardLine ();
				end.ForwardLines (2);
				
				Buffer.RemoveAllTags (start, end);
				Buffer.PlaceCursor (Buffer.GetIterAtMark (Buffer.InsertMark));
				
				ContainingTaskList.Children.Remove (this); //FIXME also for other containers?
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
