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
using System.IO;
using NUnit.Framework;
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager.Tests
{

	/// <summary>
	/// Test that check the general workings of the TaskList parser (i.e. correct number of tasks
	/// and tasklists found in notes).
	/// </summary>
	[TestFixture()]
	public class TestDeserialization : GtkTest
	{
		// disabled not used warnings for notes
		#pragma warning disable 0414
		private Note noTaskListNote;
		private TaskManagerNoteAddin noManager;
		
		private Note singleTaskListNote;
		private TaskManagerNoteAddin singleManager;
		
		private Note doubleTaskListNote;
		private TaskManagerNoteAddin doubleManager;
		#pragma warning restore 0414
		
		
		[SetUp()]
		public override void Initialize ()
		{
			base.Initialize(); // set up gtk
			
			NotesCreationManager.CreateNote ("SingleTaskListNote", out singleTaskListNote, out singleManager);
			NotesCreationManager.CreateNote ("DoubleTaskListNote", out doubleTaskListNote, out doubleManager);
			NotesCreationManager.CreateNote ("NoTaskListNote", out noTaskListNote, out noManager);
		}


		/// <summary>
		/// Deserialize a note containing no tasklists will still work.
		/// </summary>
		[Test()]
		public void FindNoTaskList () 
		{
			noManager.DeserializeTasklists ();
			Assert.That (noManager.TaskLists.Count == 0);
		}
		
		
		/// <summary>
		/// Checks if the parser correctly finds a tasklist and all its tasks contained.
		/// </summary>
		[Test()]
		public void FindTaskList ()
		{
			singleManager.DeserializeTasklists ();
			Assert.That (singleManager.TaskLists.Count == 1); // one task list is found
			Assert.That (singleManager.TaskLists[0].Tasks.Count == 3); // with 3 tasks
		}

		
		/// <summary>
		/// Checks if the parser correctly finds 2 tasklists and all its tasks contained.
		/// </summary>
		[Test()]
		public void FindMultipleTaskLists ()
		{
			doubleManager.DeserializeTasklists ();
			Assert.That (doubleManager.TaskLists.Count == 2); // two task lists are found
			Assert.That (doubleManager.TaskLists[0].Tasks.Count == 3); // 1st tasklist has 3 tasks
			Assert.That (doubleManager.TaskLists[1].Tasks.Count == 2); // 2nd tasklist has 2 tasks
		}
		
		
		[TearDown()]
		public void Cleanup ()
		{
			NotesCreationManager.DeleteNoteFiles ();
		}
		
	}
}
