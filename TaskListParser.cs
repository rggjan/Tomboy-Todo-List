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
using System.Collections.Generic;
using Tomboy;
using Gtk;

namespace Tomboy.TaskManager
{

	/// <summary>
	/// Class for parsing notes for tasklists and tasks
	/// </summary>
	public class TaskListParser
	{
		
		private Note note;
		private TaskNoteUtilities utils;
		
		private NoteBuffer buffer {
			get {return note.Buffer;}
		}
		
		public TaskListParser(Note note)
		{
			this.note = note;
			utils = new TaskNoteUtilities (buffer);
		}
		
		/// <summary>
		/// Scans note for existing tags, sets up concrete Task and TaskList objects and returns list of
		/// initialized tasklists (whose tasks are also initialized)
		/// </summary>
		/// <param name="note">
		/// A <see cref="Note"/>
		/// </param>
		/// <returns>
		/// A <see cref="List<TaskList>"/>
		/// </returns>
		public List<TaskList> Parse ()
		{
			List<TaskList> result = TryGetExisting ();
			if (result != null)
				return result;
			
			result = new List<TaskList> ();
			
			var tasklists = PrepareTaskListTags ();
			var tasks = PrepareTaskTags ();
			
			foreach (KeyValuePair<TaskListTag, TextRange> kvp in tasklists){
				result.Add (new TaskList (note, kvp.Value.Start, kvp.Key));
			}
			
			foreach (KeyValuePair<TaskTag, TextRange> kvp in tasks){
				TaskList tasklist = utils.GetTaskList (kvp.Value.Start);
				tasklist.AddTask (kvp.Value.Start, kvp.Key);
			}
			
			return result;
		}
		
		
		private List<TaskList> TryGetExisting ()
		{
			TextIter iter = buffer.StartIter;
			List<TaskList> result = new List<TaskList> ();
			do {
				TaskListTag tlt = utils.GetTaskListTag (iter);
				TaskList tl = utils.GetTaskList (iter);
				
				if (tlt != null && tl == null)
					return null;
				
				if (tl != null && !result.Contains (tl))
					result.Add (tl);
				
			} while (iter.ForwardChar ());
			
			return result;
		}
		
		//Ugly, but working
		/// <summary>
		/// Reads all existing TaskListTags and collects them together with their ranges
		/// </summary>
		/// <returns>
		/// A <see cref="Dictionary<TaskListTag, TextRange>"/>
		/// </returns>
		private Dictionary<TaskListTag, TextRange> PrepareTaskListTags ()
		{
			Dictionary<TaskListTag, TextRange> result = new Dictionary<TaskListTag, TextRange> ();
			
			TextIter iter = buffer.StartIter;
			do{
				TaskListTag tlt = utils.GetTaskListTag (iter);
				if (tlt != null){
					//Insert if not yet exiting
					if (!result.ContainsKey (tlt))
						result.Add (tlt, new TextRange (iter, iter));
					else{
						TextRange tr = result[tlt];
						result[tlt] = new TextRange (tr.Start, iter);
					}
				}
			} while (iter.ForwardChar ());
				
			return result;
		}
		
		/// <summary>
		/// Reads all existing TaskTags and collects them together with their ranges
		/// </summary>
		/// <returns>
		/// A <see cref="Dictionary<TaskTag, TextRange>"/>
		/// </returns>
		private Dictionary<TaskTag, TextRange> PrepareTaskTags ()
		{
			Dictionary<TaskTag, TextRange> result = new Dictionary<TaskTag, TextRange> ();
			
			TextIter iter = buffer.StartIter;
			do{
				TaskTag tt = utils.GetTaskTag (iter);
				if (tt != null){
					//Insert if not yet exiting
					if (!result.ContainsKey (tt))
						result.Add (tt, new TextRange (iter, iter));
					else{
						TextRange tr = result[tt];
						result[tt] = new TextRange (tr.Start, iter);
					}
				}
			} while (iter.ForwardChar ());
				
			return result;
		}
	}
}
