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
using System.Collections.Generic;
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager {

	/// <summary>
	/// A task list is a collection of tasks grouped together.
	/// It may have a title, a priority and a due date.
	/// </summary>
	public class TaskList : AttributedTask {
	
		/// <summary>
		/// Name of this task list
		/// </summary>
		public string Name {
			get;
			set;
		}
		
		/// <summary>
		/// Note containing the TaskList.
		/// </summary>
		internal Note ContainingNote {
			get; set;
		}
		
		//TODO: Changed Type. Rething this ^^
		internal List<AttributedTask> Tasks {
			get; set;
		}
		
		/// <summary>
		/// Children for ITask interface
		/// </summary>
		public override List<AttributedTask> Children {
			get{return Tasks;}	
		}
		
		/// <summary>
		/// Containers for ITask interface
		/// </summary>
		public override List<AttributedTask> Containers { 
			get{return null;}
		}
		
		//TODO
		public bool Done { 
			get; set; 
		}
		
		/// <summary>
		/// Sets up the TaskList.
		/// </summary>
		/// <param name="note">
		/// <see cref="Note"/> where the TaskLists is located.
		/// </param>
		public TaskList (Note note)
		{
			Logger.Debug ("TaskList created");
			ContainingNote = note;
			
			Tasks = new List<AttributedTask> ();
					
			addTask (ContainingNote.Buffer.InsertMark);
			
			//Containers = new List<AttributedTask> ();
			//TODO: add correct TaskNote
		}
		
		/// <summary>
		/// Creates a new Task and add it to the `Tasks` list.
		/// </summary>
		/// <param name="at">
		/// <see cref="Gtk.TextMark"/> Where to add the task in the Buffer.
		/// </param>
		public void addTask (Gtk.TextMark at)
		{
			var insertIter = ContainingNote.Buffer.GetIterAtMark (at);
			insertIter.LineOffset = 0;
			// go to beginning of the line

			Tasks.Add (new Task (this, ContainingNote.Buffer.CreateMark (null, insertIter, true)));
		}

		
	}
	
		/// <summary>
	/// Marks a Task in a NoteBuffer. Currently this does nothing (used to restore notes)
	/// </summary>
	public class TaskListTag : DynamicNoteTag
	{
		
		public TaskList TaskList {
			get;
			set;
		}

		public override void Initialize (string element_name)
		{
			base.Initialize (element_name);

			Background = "red";
			LeftMargin = 3;
			LeftMarginSet = true;
			CanSpellCheck = true;
		}
		
		public void bind (TaskList taskList) {
			TaskList = taskList;
			
			if (!Attributes.ContainsKey("Done"))
				Attributes.Add ("Done", TaskList.Done.ToString ());
			else
				Attributes["Done"] = TaskList.Done.ToString ();
			
			if (!Attributes.ContainsKey("Duedate"))
				Attributes.Add ("Duedate", TaskList.DueDate.ToString ());
			else
				Attributes["Duedate"] = TaskList.DueDate.ToString();
			
			if (!Attributes.ContainsKey("Priority"))
				Attributes.Add ("Priority", TaskList.Priority.ToString ());
			else
				Attributes["Priority"] = TaskList.Priority.ToString ();
		}
	}
}
