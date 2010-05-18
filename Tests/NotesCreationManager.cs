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
using System.Collections.Generic;

namespace Tomboy.TaskManager.Tests
{

	/// <summary>
	/// This class simplifies the creation of notes for the TaskManager addin unit tests.
	/// If you want to use it simply add your own note XML in the test folder. In your tests you can then
	/// instantiate copies of this XML file represented by <see cref="Note"/> objects.
	/// </summary>
	internal class NotesCreationManager
	{
		private static List<Note> createdNotes = new List<Note>();
		private static string BASE_NAMESPACE = "Tomboy.TaskManager.Tests";
		private static string NOTE_EXTENSION = "xml";

		/// <summary>
		/// Creates Tomboy Notes from a given Template File (embedded as a ressource in the assembly)
		/// in a temporary directory.
		/// </summary>
		/// <param name="fromTemplate">
		/// A <see cref="System.String"/> identifying the template we'd like to use.
		/// </param>
		/// <param name="note">
		/// note <see cref="Note"/> will be set to a new Note instance with the contents of the template note.
		/// </param>
		/// <param name="addin">
		/// The <see cref="TaskManagerNoteAddin"/> is wired together with the note to ensure that
		/// the note has the TaskManager functionality.
		/// </param>
		public static void CreateNote(string fromTemplate, out Note note, out TaskManagerNoteAddin addin)
		{
			var noteFile = TemporaryFileManager.CreateFile (CreateResourceName(fromTemplate));
			
			note = Note.Load (noteFile.FullName, null);
			addin = new TaskManagerNoteAddin(note);
			
			createdNotes.Add(note);
		}
		
		/// <summary>
		/// Deletes all corresponding note XML files which where created with 
		/// CreateNote before.
		/// </summary>
		public static void DeleteNoteFiles()
		{
			createdNotes.ForEach(n => TemporaryFileManager.DeleteFile(new FileInfo(n.FilePath)));
			createdNotes.Clear();
		}
		
		/// <summary>
		/// Helper function which returns the correct assembly uri of note xml test files
		/// for a given filename.
		/// </summary>
		/// <param name="templateName">
		/// A <see cref="System.String"/> containing the file name of the Note XML
		/// file (without extension).
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> containing the correct uri for the notes in the assembly.
		/// </returns>
		private static string CreateResourceName(string templateName)
		{
			return BASE_NAMESPACE + "." + templateName + "." + NOTE_EXTENSION;
		}
		
	}
}
