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
using Tomboy.Notebooks;
using Mono.Unix;

namespace Tomboy.TaskManager {

	/// <summary>
	/// This represents a Dialog in the Tomboy Search Notes Dialog
	/// and is responsible for showing all notes who have tasklists
	/// containing tasks that are not done.
	/// </summary>
	public class OpenTasksNotebook : SpecialNotebook
	{
		
		public OpenTasksNotebook () : base ()
		{}
		
		
		public override string Name
		{
			get { return Catalog.GetString ("Open Tasks"); }
		}
		
		
		public override string NormalizedName
		{
			get { return "___NotebookManager___OpenTasksNotes__Notebook___"; }
		}
		
		
		public override Tag Tag
		{
			get { return null; }
		}
		
		
		public override Note GetTemplateNote ()
		{
			return Tomboy.DefaultNoteManager.GetOrCreateTemplateNote ();
		}
		
		/// <summary>
		/// Checks wheter a Note is in this Notebook or not.
		/// </summary>
		/// <param name="n">
		/// A <see cref="Note"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>, false if all tasks in the note are
		/// marked as done, true otherwise.
		/// </returns>
		public override bool ContainsNote (Note n)
		{
			TaskListParser parser = new TaskListParser (n);
			var tls = parser.Parse ();
			
			bool allTaskListsDone = true;
			
			foreach (TaskList tl in tls) { // TODO is called very often... (once for each notebook?), do some kind of caching?
				allTaskListsDone &= tl.Done;
			}
			
			return !allTaskListsDone;
		}
	}
}
