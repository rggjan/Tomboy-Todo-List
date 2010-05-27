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
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager
{
	/// <summary>
	/// Marks a Task in a NoteBuffer. Currently this does nothing (used to restore notes)
	/// </summary>
	public class TaskListTag : AttributedTaskTag
	{
		
		/// <summary>
		/// Shortcut for the attached tasklist
		/// </summary>
		public TaskList TaskList {
			get {return (TaskList) AttributedTask;}
			set {AttributedTask = value;}
		}

		/// <summary>
		/// Initializes the fields to custom (not available/different in tasktag) values.
		/// </summary>
		/// <param name="element_name">
		/// A <see cref="System.String"/>
		/// </param>
		public override void Initialize (string element_name)
		{
			base.Initialize (element_name);
			
			ParagraphBackground = "lightgray";
			LeftMargin = 8;
		}
	}
}
