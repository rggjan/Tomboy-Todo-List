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

	//TODO test delete file
	
	/// <summary>
	/// Makes sure Notes get initialized correctly by the TaskManager Addin.
	/// </summary>
	[TestFixture()]
	public class TestNoteInitialization : GtkTest
	{

		// disabled not used warnings for notes
		#pragma warning disable 0414
		private Note noTaskListNote;
		private TaskManagerNoteAddin noManager;
		
		private Note singleTaskListNote;
		private TaskManagerNoteAddin singleManager;
		#pragma warning restore 0414
		
		
		[SetUp()]
		public override void Initialize ()
		{
			base.Initialize(); // set up gtk
			
			NotesCreationManager.CreateNote ("NoTaskListNote", out noTaskListNote, out noManager);
			NotesCreationManager.CreateNote ("SingleTaskListNote", out singleTaskListNote, out singleManager);
		}
		
		
		/// <summary>
		/// Ensures that necessary tags are registred for notes with TaskManager addin.
		/// </summary>
		[Test()]
		public void TagsRegistred () 
		{
			// for notes containing no tasklists
			Assert.That (noTaskListNote.TagTable.IsDynamicTagRegistered("tasklist"));
			Assert.That (noTaskListNote.TagTable.IsDynamicTagRegistered("task"));
			Assert.That (noTaskListNote.TagTable.Lookup("locked") != null);
			Assert.That (noTaskListNote.TagTable.Lookup("duedate") != null);
			
			// and for notes containing a tasklist
			Assert.That (singleTaskListNote.TagTable.IsDynamicTagRegistered("tasklist"));
			Assert.That (singleTaskListNote.TagTable.IsDynamicTagRegistered("task"));
			Assert.That (singleTaskListNote.TagTable.Lookup("locked") != null);
			Assert.That (singleTaskListNote.TagTable.Lookup("duedate") != null);
		}
		
		
		[TearDown()]
		public void Cleanup ()
		{
			NotesCreationManager.DeleteNoteFiles ();
		}
	}
}
