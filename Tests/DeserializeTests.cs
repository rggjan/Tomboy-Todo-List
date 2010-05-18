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
	/// Tests that check if loading tasklists from notes xml files and setting up
	/// the data structures is done correctly.
	/// </summary>
	[TestFixture()]
	public class DeserializeTests : GtkTest
	{
		
		private Note singleTaskListNote;
		private TaskManagerNoteAddin singleManager;
		
		private Note doubleTaskListNote;
		private TaskManagerNoteAddin doubleManager;
		
		
		[SetUp()]
		public override void Initialize ()
		{
			base.Initialize();
			
			TestNotesManager.CreateNote("SingleTaskListNote", out singleTaskListNote, out singleManager);
			TestNotesManager.CreateNote("DoubleTaskListNote", out doubleTaskListNote, out doubleManager);
		}

		
		/// <summary>
		/// Checks if the parser correctly finds a tasklist and all its tasks contained.
		/// </summary>
		[Test()]
		public void FindTaskList ()
		{
			var tasklists = TaskNoteParser.ParseNote(singleTaskListNote);
			Assert.That(tasklists.Count == 1); // one task list is found
			Assert.That(tasklists[0].Children.Count == 3); // with 3 tasks
		}

		
		/// <summary>
		/// Checks if the parser correctly finds 2 tasklists and all its tasks contained.
		/// </summary>
		[Test()]
		public void FindMultipleTaskLists ()
		{
			var tasklists = TaskNoteParser.ParseNote(doubleTaskListNote);
			Assert.That(tasklists.Count == 2); // two task lists are found
			Assert.That(tasklists[0].Children.Count == 3); // 1st tasklist has 3 tasks
			Assert.That(tasklists[1].Children.Count == 2); // 2nd tasklist has 2 tasks
		}
		
		
		/// <summary>
		/// This Test ensures that priorities are read correctly from the XML file.
		/// </summary>
		[Test()]
		public void LoadingPriorities ()
		{
			
		}
		
		
		[TearDown()]
		public void Cleanup ()
		{
			TestNotesManager.DeleteNoteFiles ();
		}
		
	}
}
