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
				return Boolean.Parse(TaskTag.Attributes["Done"]);
			}
			set {
				TaskTag.Attributes["Done"] = value.ToString();
				check_box.Active = value;
			}
		}
		
		
		/// <summary>
		/// Returns true if the priority of this task is yet unset.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool PriorityUnset ()
		{
			return (Priority == Priority.UNSET);
		}
		/// <summary>
		/// The priority that is assigned to this task.
		/// Note that default must be set to 3, not 0
		/// </summary>
		public Priority Priority {
			get { return TaskTag.TaskPriority; }
			set { TaskTag.TaskPriority = value; }
		}
		
		protected override NoteBuffer Buffer {
			get {
				return ContainingTaskList.ContainingNote.Buffer;
			}
		}
		
		/// <summary>
		/// Corresponding Widget for Completed Tasks.
		/// </summary>
		private Gtk.CheckButton check_box = new Gtk.CheckButton ();
		
		/// <summary>
		/// TaskList containing this task.
		/// </summary>
		private TaskList containing_task_list;
		
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
			get { return (TaskTag)Tag; }
			set { Tag = value; }
		}
		
		//TODO: Subtasks
		public List<TaskList> Subtasks {
			get 
			{
				List<TaskList> result = new List<TaskList> ();
				TextTagEnumerator links = new TextTagEnumerator (Buffer, "link:internal");
				
				foreach (TextRange r in links) {
					if (r.Start.Compare (Start)>=0 && r.End.Compare (End)<=0) {
						
						Note linkedNote = Tomboy.DefaultNoteManager.Find (r.Text);
					
						if (!linkedNote.IsLoaded)
							Note.Load (linkedNote.FilePath, Tomboy.DefaultNoteManager);
					
						TaskListParser subparser = new TaskListParser (linkedNote);
						List<TaskList> sublists = subparser.Parse ();
					
						result.AddRange (sublists);
					}
				}
				
				return result;
			}
		}
		
		public Task (TaskList containingList, Gtk.TextIter location)
		{
			//TODO: rewrite tag part (it's ugly)
			InitializeTask (containingList, location, (TaskTag)containingList.ContainingNote.TagTable.CreateDynamicTag ("task"));
			TaskTag.Bind (this);
			
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
		
		public bool WasDeleted
		{
			get {
				if (TaskTagValid (Start) && Start.LineOffset == 0)
					return false;
				else
					return true;
			}
		}
		
		public bool IsValid
		{
			get {
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
			
			Initialize (location, tag);
			
			if(tag.Attributes["Done"] == true.ToString())
				check_box.Active = true;
			
			
			//Buffer.UserActionEnded += BufferChanged;
		}
		
	
		/// <summary>
		/// Inserts a CheckButton at cursor position.
		/// </summary>
		private void InsertCheckButton (Gtk.TextIter insertIter)
		{
			//Gtk.TextIter insertIter = Buffer.GetIterAtMark (Buffer.InsertMark);
			
			check_box.Name = "tomboy-inline-checkbox";
			check_box.Toggled += ToggleCheckBox;
			
			var check_box_anchor = Buffer.CreateChildAnchor (ref insertIter);
			ContainingTaskList.ContainingNote.Window.Editor.AddChildAtAnchor (check_box, check_box_anchor);
			check_box.Show ();
		}

		/// <summary>
		/// Makes the priority widget visible
		/// </summary>
		public void AddPriority ()
		{
			GetAvPriorityVisitor visitor = new GetAvPriorityVisitor ();
			visitor.visit (this);
			Priority = visitor.Result;
			ShowPriority ();
		}
		
		
		public void Replace (String after)
		{
			var iter = Buffer.GetIterAtMark (Buffer.InsertMark);
			int offset = iter.Offset;
			
			var start = Start;
			start.ForwardChar ();
			
			Buffer.PlaceCursor (start);
			Buffer.InsertAtCursor (after);
			
			start = Start;
			Gtk.TextIter end = Start;
			end.ForwardChar ();
			Buffer.Delete (ref start, ref end);
			
			Buffer.PlaceCursor (Buffer.GetIterAtOffset(offset));
		}
		
		public void ShowPriority ()
		{
			if (PriorityUnset ())
				Replace (" ");
			else
				Replace (((int)Priority).ToString ());
			
			UpdateWidgetTags ();
		}
		
		public void HidePriority ()
		{
			Replace (" ");
			UpdateWidgetTags ();
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
		
		public int Line {
			get {
				return Start.Line;
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
			check_box.Active = !check_box.Active;
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
			
			if (check_box != null && check_box.Active) {
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
		/// Returns true iff this task is the last (in means of buffer offset) one in containingtasklist
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool IsLastTask ()
		{
			var list = ContainingTaskList.Tasks;
			
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
			foreach (Task task in ContainingTaskList.Tasks)
			{
				if (task.Start.Line > Start.Line)
				{
					tasks_following.Add (task);
				}
			}
			return tasks_following;
		}

		/// <summary>
		/// Should be called when the task is removed.
		/// </summary>
		public void Delete ()
		{
			ContainingTaskList.Tasks.Remove (this);
			DeleteTag ();
		}
		
		public void DeleteTag ()
		{
			Buffer.TagTable.Remove (TaskTag);
		}
		
		/// <summary>
		/// Deletes this task or splits this tasklist, depending on whether we are on the last line or not.
		/// When splitting, returns newly created tasklist.
		/// </summary>
		public TaskList DeleteEmptyCheckBox (String name)
		{
			var start = Start;			
			var end = DescriptionEnd;
			
			Buffer.Delete (ref start, ref end);

			start = Start;
			end = start;
			end.ForwardLines (2);
			
			Buffer.RemoveAllTags (start, end);

			Delete ();
			
			if (!IsLastTask () || (name != null && name.Length > 0)) {
				Logger.Debug ("is not last task");
				
				if (name == null)
					return Split ();
				else
					return Split (name);
			}
			
			return null;
		}
		
		
		public TaskList Fix ()
		{
			Logger.Debug ("Fixing");
			if (IsValid)
			{
				Logger.Debug ("removing all tags in ");
				Logger.Debug (Buffer.GetText (DescriptionStart, DescriptionEnd, false));
				utils.RemoveTaskTags (DescriptionStart, DescriptionEnd);
				TagUpdate ();
				ApplyTag (ContainingTaskList.Tag);
				return null;
			}
			else { 
				return DeleteAndSplit ();
			}
		}
		
		/// <summary>
		/// Deletes this task and Splits / Merges the two remaining task list parts.
		/// </summary>
		/// <returns>
		/// A <see cref="TaskList"/>
		/// </returns>
		public TaskList DeleteAndSplit ()
		{
			Logger.Debug ("Deleting and Splitting");
			if (Buffer.GetText (Start, End, false).Trim ().Length == 0)
			{
				Logger.Debug ("Merging");
				var start = Start;
				var end = End;
				Buffer.Delete (ref start, ref end);
				Delete ();
				return null;
			} else {
				var end = DescriptionEnd;
				Buffer.Insert (ref end, "\n");
				return DeleteEmptyCheckBox (Buffer.GetText (Start, DescriptionEnd, false).TrimStart ());
			}
		}


		public bool HasSpecialTag (Gtk.TextIter iter)
		{
			foreach (Gtk.TextTag tag in iter.Tags)
			{
				if (tag.Name == "priority" || tag.Name == "checkbox" || tag.Name == "checkbox-active" || tag.Name == "locked")
					return true;
			}
			return false;
		}
		
		public TaskList Split (String name)
		{
			var tasks_following = TasksFollowing ();

			foreach (Task task in tasks_following)
			{
				ContainingTaskList.Tasks.Remove (task);
			}
			
			var start = Start;
			start.ForwardLine ();
			TaskList new_list = new TaskList (ContainingTaskList.ContainingNote, tasks_following, name, start);
			return new_list;
		}
		
		public TaskList Split ()
		{
			return Split (ContainingTaskList.Name + "(2)");
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
			Debug.Assert (check_box == sender); // no other checkbox should be registred here
			
			Done = check_box.Active;
			SetDoneVisitor visitor = new SetDoneVisitor (check_box.Active, this);
			visitor.visit (this);
		}
	}
}
