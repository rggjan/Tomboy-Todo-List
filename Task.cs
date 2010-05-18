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
			Buffer = containingList.ContainingNote.Buffer;
			//TODO: rewrite tag part (it's ugly)
			InitializeTask (containingList, location, (TaskTag)containingList.ContainingNote.TagTable.CreateDynamicTag ("task"));
			TaskTag.bind (this);
			
			var iter = Start;
			Buffer.PlaceCursor (iter);
			Buffer.InsertAtCursor (" 3 ");
			
			AddWidgets ();
			TagUpdate ();

			iter = Start;
			iter.ForwardChars (4);
			Buffer.PlaceCursor (iter);
		}
		
		/// <summary>
		/// Adds all widgets (besides calendar) to this task
		/// </summary>
		public void AddWidgets ()
		{
			var start = Start;
			start.ForwardChar ();
			var end = start;
			end.ForwardChar ();
			Buffer.ApplyTag ("priority", start, end);
		
			InsertCheckButton (end);
			
			start = Start;
			end = start;
			start.BackwardChar ();
			end.ForwardChars (3);
			Buffer.ApplyTag ("locked", start, end);
			
			//end.BackwardChar ();
			//Buffer.PlaceCursor (End);
		}
		
		public Task (TaskList containingList, Gtk.TextIter location, TaskTag tag)
		{
			InitializeTask(containingList, location, tag);
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
		private void InitializeTask (TaskList containingList, Gtk.TextIter location, TaskTag tag)
		{			
			ContainingTaskList = containingList;
			location.LineOffset = 0;
			
			Initialize (ContainingTaskList.ContainingNote.Buffer, location, tag);
			
			Buffer.UserActionEnded += BufferChanged;
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
		/// Inserts the pr or;ty ComboBox in the TextBuffer.
		/// </summary>
		/// <param name="at"> Where to insert (exactly). </param>
		/// <returns>
		/// A TextIter
		/// </returns>
		private void InsertPriorityBox (Gtk.TextIter insertIter)
		{
			//priority_box.Changed += setpriority;
			//Buffer.insert
			//Buffer.InsertWithTagsByName (ref insertIter, "3", "priority");
		}
		
		/// <summary>
		/// Makes the priority widget visible
		/// </summary>
		public void AddPriority ()
		{
			Priority = Priorities.LOW;

			var insertIter = Buffer.GetIterAtMark (Position);

			
			ShowPriority ();
		}
		
		public void ShowPriority ()
		{
			//if (!PriorityUnset())
			//	priority_box.Show (); //FIXME
		}
		
		public void HidePriority ()
		{
			// priority_box.Hide (); //FIXME
		}
		
		/// <returns>
		/// A <see cref="Gtk.TextIter"/> marking the end of the textual description of this
		/// task in the NoteBuffer.
		/// </returns>
		protected override Gtk.TextIter End {
			get {
				var start = Start;
				start.ForwardLine ();
				//TODO: backwardchar here?
				return start;
			}
		}

		/// <summary>
		/// Updates the strikethrough tag of the task description. If the checkbox is
		/// active or removes it if it's not. Also applies the tasklist tag.
		/// </summary>
		public override void TagUpdate ()
		{
			var start = Start;
			var end = End;
			end.ForwardChar ();
		
			Buffer.ApplyTag (TaskTag, start, end);
			
			if (CheckBox != null && CheckBox.Active) {
				start.ForwardChars (4);
				Buffer.ApplyTag ("strikethrough", start, end);
			} 
			else {
				start.ForwardChars (4);
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
				if (task.Start.Line > Start.Line)
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
