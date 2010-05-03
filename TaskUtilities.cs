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
using Tomboy;
using System.Collections.Generic;

namespace Tomboy.TaskManager
{
	public enum Priority {
		VERY_LOW = 1,
		LOW,
		NORMAL,
		HIGH,
		VERY_HIGH
	}
	
	public interface ITask {
		
		/// <summary>
		/// Returns all Abstract Tasks that are children of this Abstracttask.
		/// TaskNote -> TaskLists (TODO: not sure this is necessary)
		/// TaskList -> Tasks
		/// Tasks -> Subtasks (linked via TaskNotes to TaskLists)
		/// </summary>
		List<AttributedTask> Children {
			get;
		}
		
		/// <summary>
		/// As implicitly described by Children (other way arround)
		/// </summary>
		List<AttributedTask> Containers {
			get;
		}
		
		/// <summary>
		/// Whether or not this task has been completed
		/// </summary>
		bool Done {
			get;
			set;
		}
	}
	
	public abstract class AttributedTask {
	
		/// <summary>
		/// Date when this task is overdue
		/// </summary>
		public DateTime DueDate {
			get; set;
		}
		
		/// <summary>
		/// True if this task's duedate lies in the past
		/// </summary>
		/// <returns>
		/// A <see cref="boolean"/>; 
		/// </returns>
		public bool isOverdue () {
			return DueDate.CompareTo(DateTime.Now) <= 0;	
		}
		
		/// <summary>
		/// The priority that is assigned to this task.
		/// Note that default must be set to 3, not 0
		/// </summary>
		public Priority Prio {
			get; set;
		}	
	}
}
