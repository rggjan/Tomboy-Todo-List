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
	/// Tests the ContainingNote methods for our 2 Special Notebook classes
	/// OpenTasksNotebook and OverdueTasksNotebook.
	/// </summary>
	[TestFixture()]
	public class TestSpecialNotebooks : GtkTest
	{
		// disabled not used warnings for notes
		#pragma warning disable 0414
		private Note singleTaskListNote;
		private TaskManagerNoteAddin singleManager;
		
		private Note doubleTaskListNote;
		private TaskManagerNoteAddin doubleManager;
		
		private Note noTaskListNote;
		private TaskManagerNoteAddin noManager;
		
		private Note allDoneTaskListNote;
		private TaskManagerNoteAddin allDoneManager;
		#pragma warning restore 0414
		
		private OpenTasksNotebook openTasksNotebook = new OpenTasksNotebook();
		private OverdueTasksNotebook overdueTasksNotebook = new OverdueTasksNotebook();
		
		
		[SetUp()]
		public override void Initialize ()
		{
			base.Initialize (); // set up gtk
			
			NotesCreationManager.CreateNote ("AllDoneTaskListNote", out allDoneTaskListNote, out allDoneManager);
			NotesCreationManager.CreateNote ("NoTaskListNote", out noTaskListNote, out noManager);
			NotesCreationManager.CreateNote ("SingleTaskListNote", out singleTaskListNote, out singleManager);
			NotesCreationManager.CreateNote ("DoubleTaskListNote", out doubleTaskListNote, out doubleManager);
		}

		
		/// <summary>
		/// Checks the OpenTasksNotebook to ensure it correctly matches the notes it will contain.
		/// </summary>
		[Test()]
		public void CheckOpenTasks ()
		{
			Assert.That(openTasksNotebook.ContainsNote(singleTaskListNote));
			Assert.That(openTasksNotebook.ContainsNote(doubleTaskListNote));
			Assert.That(!openTasksNotebook.ContainsNote(noTaskListNote));
			Assert.That(!openTasksNotebook.ContainsNote(allDoneTaskListNote));
		}
		
		/// <summary>
		/// Checks the OverdueTasksNotebook to ensure it correctly matches the notes it will contain.
		/// </summary>
		[Test()]
		public void CheckOverdueTasks ()
		{
			throw new NotImplementedException();
		}
		
		
		[TearDown()]
		public void Cleanup ()
		{
			NotesCreationManager.DeleteNoteFiles ();
		}
		
	}
}
