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

namespace Tomboy.TaskManager
{
	
	/// <summary>
	/// Enum used for priorities
	/// </summary>
	public enum Priorities {
		VERY_LOW = 1,
		LOW,
		NORMAL,
		HIGH,
		VERY_HIGH
	}
	
	/// <summary>
	/// Class that represents the common functionality of TaskList and Task
	/// </summary>
	public abstract class AttributedTask {
	
		/// <summary>
		/// Date when this task is overdue
		/// </summary>
		public DateTime DueDate {
			get;
			set;
		}
		
		/// <summary>
		/// True iff (if and only if) this task's duedate lies in the past
		/// </summary>
		/// <returns>
		/// A <see cref="boolean"/>; 
		/// </returns>
		public bool isOverdue ()
		{
			return DueDate.CompareTo(DateTime.Now) <= 0;
		}
		
		
		/// <summary>
		/// The priority that is assigned to this task.
		/// Note that default must be set to 3, not 0
		/// </summary>
		public Priorities Priority {
			get {return Tag.TaskPriority;}
			set {Tag.TaskPriority = value;}
		}	
		
		/// <summary>
		/// Returns all Abstract Tasks that are children of this Abstracttask.
		/// TaskList -> Tasks
		/// Tasks -> Subtasks (linked via TaskNotes to TaskLists)
		/// </summary>
		public abstract List<AttributedTask> Children {
			get; set;
		}
		
		/// <summary>
		/// As implicitly described by Children (other way arround)
		/// </summary>
		public abstract List<AttributedTask> Containers {
			get; set;
		}
		
		/// <summary>
		/// Whether or not this task has been completed
		/// </summary>
		public abstract bool Done {
			get; set;
		}
		
		/// <summary>
		/// Tag that is attached to this attributedtask (in the buffer)
		/// </summary>
		protected AttributedTaskTag Tag {
			get; set;	
		}
		
		/// <summary>
		/// Describes the shortcut to the buffer
		/// </summary>
		protected NoteBuffer Buffer {
			get;
			set;
		}
		
		protected TextMark Position;
		protected TextIter Start {
			get { return Buffer.GetIterAtMark (Position); }	
		}
		
		protected abstract TextIter End {
			get;	
		}
	}
}
