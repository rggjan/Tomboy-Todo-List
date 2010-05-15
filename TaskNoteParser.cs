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


	public class TaskNoteParser
	{
		
		private Note note;
		private TaskNoteUtilities utils;
		
		private NoteBuffer buffer{
			get {return note.Buffer;}
		}
		private TaskNoteParser(Note note)
		{
			this.note = note;
			utils = new TaskNoteUtilities (buffer);
		}
		
		public static List<TaskList> ParseNote(Note note)
		{
			List<TaskList> result = new List<TaskList> ();
			TaskNoteParser parser = new TaskNoteParser (note);
			var tasklists = parser.PrepareTaskListTags ();
			var tasks = parser.PrepareTaskTags ();
			
			foreach (KeyValuePair<TaskListTag, TextRange> kvp in tasklists){
				result.Add (new TaskList (note, kvp.Value.Start, kvp.Key));
				//Logger.Debug ("Tasklist found:\nStart: {0}\nEnd: {1}\nText: {2}", 
				//            new object[]{kvp.Value.Start.Offset, kvp.Value.End.Offset, kvp.Value.Text});
			}
			
//			TextIter iter = note.Buffer.StartIter;
//			do{
//				TaskListTag tlt = parser.utils.GetTaskListTag (iter);
//				if (tlt != null && tlt.TaskList == null)
//					Logger.Debug ("THIS SHOULD NOT HAPPEN");
//			} while (iter.ForwardChar ());
			
			foreach (KeyValuePair<TaskTag, TextRange> kvp in tasks){
				//Logger.Debug ("Task found:\nStart: {0}\nEnd: {1}\nText: {2}", 
				//             new object[]{kvp.Value.Start.Offset, kvp.Value.End.Offset, kvp.Value.Text});
				
				TaskList tasklist = parser.utils.GetTaskList (kvp.Value.Start);
				tasklist.addTask (kvp.Value.Start, kvp.Key);
			}
			
			return result;
		}
		
		//Ugly, but working
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
