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

namespace Tomboy.TaskManager
{

	/// <summary>
	/// A Task is a piece of text representing a “todo” item,
	/// accompanied with a checkbox.
	/// It may have a due date and a priority and can be
	/// marked as done by crossing out the checkbox.
	/// </summary>
	public class Task
	{
		/// <summary>
		/// Description of the Task the user wrote in the Buffer
		/// </summary>
		public string Description
		{ get; set; }
		
		/// <summary>
		/// Is this task completed?
		/// </summary>
		public bool Completed { 
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
		public DateTime DueDate 
		{ get; set; }
		private Gtk.Calendar DueDateWidget
		{ get; set; }
				
		/// <summary>
		/// Priority for this Task
		/// </summary>
		public int Priority
		{ get; set; }
		// TODO find corresponding widget here
		
		
		/// <summary>
		/// TaskList containing this task.
		/// </summary>
		private TaskList TaskList 
		{ get; set; }
		
		public Task (TaskList containingList)
		{
			TaskList = containingList;
		}
		
	}
}
