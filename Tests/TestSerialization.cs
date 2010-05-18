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
	/// Tests that check tasklist serialization (writing tasklists to xml) works correctly.
	/// We usually test this by changing a note, saving it to disk and reload it to see if
	/// the changes have been successful.
	/// </summary>
	[TestFixture()]
	public class TestSerialization : GtkTest
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
		
		[TearDown()]
		public void Cleanup ()
		{
			NotesCreationManager.DeleteNoteFiles ();
		}
		
	}
}
