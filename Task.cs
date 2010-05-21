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
		public TaskTag TaskTag {
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
			Buffer.InsertAtCursor ("  ");
			
			AddWidgets ();
			TagUpdate ();

			iter = Start;
			iter.ForwardChars (3);
			Buffer.PlaceCursor (iter);
		}
		
		/// <summary>
		/// Adds all widgets (besides calendar) to this task
		/// </summary>
		public void AddWidgets ()
		{
			var end = Start;
			end.ForwardChar ();
			InsertCheckButton (end);
			
			UpdateWidgetTags ();
		}

		public void UpdateWidgetTags ()
		{
			var start = Start;
			var end = start;
			end.ForwardChars (1);
			Buffer.ApplyTag ("priority", start, end);
		
			start = Start;
			end = start;
			start.BackwardChar ();
			end.ForwardChars (2);
			Buffer.ApplyTag ("locked", start, end);
			
			start = Start;
			start.ForwardChar ();
			end = start;
			end.ForwardChars (1);
			Buffer.ApplyTag ("checkbox", start, end);
			
			start = Start;
			start.ForwardChars (2);
			end = start;
			end.ForwardChars (1);
			Buffer.ApplyTag ("checkbox-active", start, end);
		}
		
		public Task (TaskList containingList, Gtk.TextIter location, TaskTag tag)
		{
			InitializeTask(containingList, location, tag);
		}
		
		public bool TaskTagValid(Gtk.TextIter iter)
		{
			bool tag_existing = false;
			foreach (Gtk.TextTag tag in iter.Tags) {
				if (tag is TaskTag) {
					TaskTag tasktag = (TaskTag) tag;
					if (tasktag.Task != this)
						return false;
					else
						tag_existing = true;
				}
			}
			
			if (!tag_existing)
				return false;
			
			return true;
		}
		
		public bool WasDeleted ()
		{
			if (TaskTagValid (Start))
				return false;
			else
				return true;
		}
		
		public bool IsValid ()
		{
			//if (!TaskTagValid (DescriptionStart))
			//	return false;
			
			var iter = Start;
			iter.ForwardChars (2);

			if (!TaskTagValid (iter))
				return false;
			
			bool found = false;
			foreach (Gtk.TextTag tag in iter.Tags)
			{
				if (tag.Name == "checkbox-active")
				{
					found = true;
					break;
				}
			}
			if (!found)
				return false;
			
			// Checkbox deleted
/*			if (boxanchor.Deleted)
				return false;
			
			// Enter inserted too early
			if (DescriptionStart.Line != Start.Line)
				return false;*/
			
			return true;
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
			Children = new List<AttributedTask> ();
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

		/// <summary>
		/// Makes the priority widget visible
		/// </summary>
		public void AddPriority ()
		{
			Priority = Priorities.LOW;

			Gtk.TextIter start = Start;

			Gtk.TextIter end = Start;
			end.ForwardChar ();

			Buffer.Delete (ref start, ref end);
			
			Gtk.TextIter iter = Start;
			Buffer.Insert (ref iter, ((int)Priority).ToString ());
			
			SetPriority ();
			UpdateWidgetTags ();
		}
		
		public void SetPriority ()
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
				return start;
			}
		}
		
		protected override Gtk.TextIter DescriptionEnd {
			get {
				var start = Start;
				start.ForwardToLineEnd ();
				return start;
			}
		}
		
		protected override Gtk.TextIter DescriptionStart {
			get {
				var start = Start;
				start.ForwardChars (3);
				return start;
			}
		}
		
		public void Toggle ()
		{
			CheckBox.Active = !CheckBox.Active;
			TagUpdate ();
		}

		/// <summary>
		/// Updates the strikethrough tag of the task description. If the checkbox is
		/// active or removes it if it's not. Also applies the tasklist tag.
		/// </summary>
		public void TagUpdate ()
		{
			//TaskTag.Indent = 0;
			//TaskTag.Priority = Notes.;
			ApplyTag (TaskTag);
			
			if (CheckBox != null && CheckBox.Active) {
				Buffer.ApplyTag ("strikethrough", DescriptionStart, DescriptionEnd);
			} 
			else {
				Buffer.RemoveTag ("strikethrough", DescriptionStart, DescriptionEnd);
			}
		}

		
		public void RemoveTag (Gtk.TextTag tag)
		{
			var end = End;
		
			Buffer.RemoveTag (tag, Start, end);
		}
		
		public void ApplyTag (Gtk.TextTag tag)
		{
			var end = End;
		
			Buffer.ApplyTag (tag, Start, end);
		}
		
		public bool LineIsEmpty ()
		{
			return Buffer.GetText (DescriptionStart, DescriptionEnd, true).Trim ().Length == 0;
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
			var list = ContainingTaskList.Children;
			
			foreach (Task task in list)
			{
				if (task.Start.Line > Start.Line)
				{
					return false;
				}
			}
			return true;
		}
		
		public List<Task> TasksFollowing ()
		{
			List<Task> tasks_following = new List<Task>();
			foreach (Task task in ContainingTaskList.Children)
			{
				if (task.Start.Line > Start.Line)
				{
					tasks_following.Add (task);
				}
			}
			return tasks_following;
		}

		/// <summary>
		/// Delete this task, it's widgets and corresponding tag representation
		/// </summary>
		public TaskList Delete ()
		{
			var start = Start;
			var end = DescriptionEnd;
			Buffer.Delete (ref start, ref end);
			
			start = Start;
			end = start;
			end.ForwardLines (2);
			
			Buffer.RemoveAllTags (start, end);
			ContainingTaskList.Children.Remove (this);
			
			start = Start;
			start.ForwardLine ();
			//end = start;
			//end.ForwardLine ();
			
			var tasks_following = TasksFollowing ();
			
			if (!IsLastTask ()) {
				Logger.Debug("is not last task");
				foreach (Task task in tasks_following)
				{
					ContainingTaskList.Children.Remove (task);
				}
				
				TaskList new_list = new TaskList (ContainingTaskList.ContainingNote, tasks_following, ContainingTaskList.Name + " 2", start);
				
				return new_list;
			}
			
			return null;
			
			//FIXME also for other containers?
		}
		
		public void DebugPrint ()
		{
			if (!Tomboy.Debugging)
				return;
			
		    Console.WriteLine ("Task: " + Description ());
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
