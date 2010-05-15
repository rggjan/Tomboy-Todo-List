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

	public class OpenTasksNotebook : SpecialNotebook
	{
		public OpenTasksNotebook () : base ()
		{
		}
		
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
		
		public override bool ContainsNote(Note n) 
		{
			Logger.Debug("ContainsNote");
			var tls = TaskNoteParser.ParseNote(n);
			Logger.Debug("Found tasklist#:" + tls.Count);
			return tls.Count > 0;
		}
	}
}
