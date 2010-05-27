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
		
		public bool CheckUpComplete {
			get; set;	
		}
		
		public bool CheckUpIncomplete {
			get; set;	
		}
		
		public bool SetDownComplete {
			get; set;	
		}
		
		public bool SetDownIncomplete {
			get; set;	
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
		/// Is this task completed?
		/// Caution: the setter does not update the buffer (strikethrough), just the attribute of the tag and the checkbox
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
		/// The priority that is assigned to this task.
		/// Note that default must be set to 3, not 0
		/// </summary>
		public Priority Priority {
			get { return TaskTag.TaskPriority; }
			set { TaskTag.TaskPriority = value; }
		}
		
				
		/// <summary>
		/// The line on which this Task is located
		/// </summary>
		public int Line {
			get {
				return Start.Line;
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
		
		//TODO: Subtasks
		/// <summary>
		/// List of Subtasks of this task
		/// </summary>
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
		
		/// <summary>
		/// Gets the tag attached to this task. Shortcut.
		/// </summary>
		public TaskTag TaskTag {
			get { return (TaskTag)Tag; }
			set { Tag = value; }
		}
		
		
		/// <summary>
		/// Returns true if the priority of this task is yet unset.
		/// </summary>
		public bool PriorityUnset {
			get {
				return (Priority == Priority.UNSET);
			}
		}
		
				/// <summary>
		/// Returns true if the Task was deleted in the Buffer
		/// </summary>
		public bool WasDeleted
		{
			get {
				if (TaskTagValid (Start) && Start.LineOffset == 0)
					return false;
				else
					return true;
			}
		}
		
		/// <summary>
		/// Returns true if the Task is valid (including the tags etc. at the beginning)
		/// </summary>
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
		
				return true;
			}
		}	
				
		/// <summary>
		/// Returns true iff this task is the last (in means of buffer offset) one in containingtasklist
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool IsLastTask {
				get {
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
		}
		
		/// <summary>
		/// Returns true if the description line of the Task is empty
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool LineIsEmpty {
			get {
				return Buffer.GetText (DescriptionStart, DescriptionEnd, true).Trim ().Length == 0;
			}
		}

		/// <summary>
		/// The Buffer containing this Task
		/// </summary>
		protected override NoteBuffer Buffer {
			get {
				return ContainingTaskList.ContainingNote.Buffer;
			}
		}
		
				
		/// <summary>
		/// End of the Description of this Task.
		/// </summary>		
		protected override Gtk.TextIter DescriptionEnd {
			get {
				var start = Start;
				start.ForwardToLineEnd ();
				return start;
			}
		}
		
		/// <summary>
		/// Start of the Description of this Task. Just after the checkbox etc.
		/// </summary>
		protected override Gtk.TextIter DescriptionStart {
			get {
				var start = Start;
				start.ForwardChars (3);
				return start;
			}
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

		/// <summary>
		/// Initialize a new task within a list, at a certain location and with a specific tag
		/// </summary>
		public Task (TaskList containingList, Gtk.TextIter location, TaskTag tag)
		{
			InitializeTask(containingList, location, tag);
		}
		
		/// <summary>
		/// Initialize a new task within a task list.
		/// </summary>
		/// <param name="containingList">
		/// A <see cref="TaskList"/>, where the task should go.
		/// </param>
		/// <param name="location">
		/// A <see cref="Gtk.TextIter"/>, the place in the Buffer where to add the task
		/// </param>
		public Task (TaskList containingList, Gtk.TextIter location)
		{
			TaskTag tt = (TaskTag) containingList.ContainingNote.TagTable.CreateDynamicTag ("task");
			tt.Bind (this);
			InitializeTask (containingList, location, tt);
			
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
			
			CheckUpComplete = true;
			CheckUpIncomplete = false;
			SetDownComplete = true;
			SetDownIncomplete = false;
			//Buffer.UserActionEnded += BufferChanged;
		}
		
		public void SetAllVisitorPolicies (bool upcom, bool upin, bool downcom, bool downin)
		{
			CheckUpComplete = upcom;
			CheckUpIncomplete = upin;
			SetDownComplete = downcom;
			SetDownIncomplete = downin;
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
		/// Replace the first Char of the Task with the given String, keeping all the Tags etc.
		/// Used for the Priority stuff.
		/// </summary>
		private void Replace (String after)
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
		
		/// <summary>
		/// Signal when checkbutton for the task was clicked.
		/// This function is responsible for updating strikethrough functionality.
		/// </summary>
		private void ToggleCheckBox (object sender, EventArgs e)
		{
			Debug.Assert (check_box == sender); // no other checkbox should be registred here
			
			Done = check_box.Active;
			
			//Downward setting
			if (Done){
				
				if (SetDownComplete){
					SetDoneVisitor dvisitor = new SetDoneVisitor (true, this);
					dvisitor.visit (this);
				}
				if (CheckUpComplete){
					CheckDoneVisitor uvisitor = new CheckDoneVisitor (true);
					uvisitor.visit (ContainingTaskList);
				}
			} else {
				if (SetDownIncomplete){
					SetDoneVisitor dvisitor = new SetDoneVisitor (false, this);
					dvisitor.visit (this);
				}
				if (CheckUpIncomplete){
					CheckDoneVisitor uvisitor = new CheckDoneVisitor (false);
					uvisitor.visit (ContainingTaskList);
				}
			}
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

		/// <summary>
		/// Update the tags at the beginning of the task.
		/// </summary>
		public void UpdateWidgetTags ()
		{
			var start = Start;
			var end = start;
			end.ForwardChars (1);
			Buffer.ApplyTag ("priority", start, end);
			ColorTag tag = (ColorTag)Buffer.GetDynamicTag ("color", start);
			if (tag == null)
				tag = (ColorTag)ContainingTaskList.ContainingNote.TagTable.CreateDynamicTag ("color");
			tag.setColor (Priority);
			Buffer.ApplyTag (tag, start, end);
		
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
		
		public void ShowPriority ()
		{
			if (PriorityUnset)
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

		/// <summary>
		/// Removes the given Tag from this Task
		/// </summary>
		/// <param name="tag">
		/// A <see cref="Gtk.TextTag"/>
		/// </param>
		public void RemoveTag (Gtk.TextTag tag)
		{
			var end = End;
		
			Buffer.RemoveTag (tag, Start, end);
		}
		
		/// <summary>
		/// Applies the given Tag to this Task
		/// </summary>
		/// <param name="tag">
		/// A <see cref="Gtk.TextTag"/>
		/// </param>
		public void ApplyTag (Gtk.TextTag tag)
		{
			var end = End;
		
			Buffer.ApplyTag (tag, Start, end);
		}
		
		/// <summary>
		/// Returns a list of the Tasks that follow this task in the Buffer
		/// </summary>
		/// <returns>
		/// A <see cref="List<Task>"/>
		/// </returns>
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
		
		/// <summary>
		/// Delete the Tag of this Task
		/// </summary>
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
			
			if (!IsLastTask || (name != null && name.Length > 0)) {
				Logger.Debug ("is not last task");
				
				if (name == null)
					return Split ();
				else
					return Split (name);
			}
			
			return null;
		}
		
		/// <summary>
		/// Fixes this task, for example after some deletion operations.
		/// </summary>
		/// <returns>
		/// A <see cref="TaskList"/>, the result of splitting of the TaskList that might occur.
		/// </returns>
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
				
		/// <summary>
		/// Returns true if the tasktag is valid at the given iter.
		/// </summary>
		/// <param name="iter">
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

		/// <summary>
		/// Splits this TaskList, creating two TaskLists.
		/// </summary>
		/// <param name="name">
		/// A <see cref="String"/>, the name of the new TaskList
		/// </param>
		/// <returns>
		/// A <see cref="TaskList"/>, the second (the new) TaskList
		/// </returns>
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
		
		/// <summary>
		/// Splits with a default name for the new TaskList
		/// </summary>
		/// <returns>
		/// A <see cref="TaskList"/>
		/// </returns>
		public TaskList Split ()
		{
			return Split (ContainingTaskList.Name + "(2)");
		}
		
		/// <summary>
		/// Prints the Task to the Console.
		/// </summary>
		public void DebugPrint ()
		{
			if (!Tomboy.Debugging)
				return;
			
		    Console.WriteLine ("Task: " + Description ());
		}
	}
}
