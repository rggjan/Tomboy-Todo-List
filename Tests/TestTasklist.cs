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
using NUnit.Framework;

namespace Tomboy.TaskManager.Tests
{

	
	/// <summary>
	/// Tests for the Tasklist methods & behaviour.
	/// </summary>
	[TestFixture()]
	public class TestTasklist : GtkTest
	{

		// disabled not used warnings for notes
		#pragma warning disable 0414		
		private Note noTaskListNote;
		private TaskManagerNoteAddin noManager;

		private Note changedNote;
		private TaskManagerNoteAddin changedManager;
		#pragma warning restore 0414

		
		[SetUp()]
		public override void Initialize ()
		{
			base.Initialize (); // set up gtk
			
			NotesCreationManager.CreateNote ("NoTaskListNote", out noTaskListNote, out noManager);
		}
		
		
		/// <summary>
		/// Insert a Tasklist and checks that its really there by reloading it
		/// in a different note object afterwards.
		/// </summary>
		[Test()]
		public void InsertTasklist ()
		{
			// go to the end of the note
			var insertIter = noTaskListNote.Buffer.GetIterAtMark (noTaskListNote.Buffer.InsertMark);
			insertIter.ForwardLines (10);
			
			noTaskListNote.Buffer.BeginUserAction ();
			TaskList tl = new TaskList (noTaskListNote);
			noTaskListNote.Buffer.EndUserAction ();
			
			noTaskListNote.Save ();
			
			Assert.That(noManager.TaskLists.Count == 1);

			// reload as another note object
			NotesCreationManager.LoadNote(noTaskListNote.FilePath, out changedNote, out changedManager, false);
			changedManager.DeserializeTasklists();
			
			Assert.That(changedManager.TaskLists.Count == 1); // make sure its available even after reload
		}		

		
		[TearDown()]
		public void Cleanup ()
		{
			NotesCreationManager.DeleteNoteFiles ();
		}
		
	}
}
