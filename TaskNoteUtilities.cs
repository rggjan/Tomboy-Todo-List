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
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager
{

	/// <summary>
	/// Class that encapsulates often used accesses to unknown tasks or tasklists
	/// (or their corresponding tag) via some position in the buffer
	/// </summary>
	public class TaskNoteUtilities
	{

		private NoteBuffer Buffer {
			get; set;	
		}
		
		public TaskNoteUtilities (NoteBuffer n)
		{
			this.Buffer = n;	
		}
		
		public bool InTaskList ()
		{
			Gtk.TextIter here = Buffer.GetIterAtMark (Buffer.InsertMark);
			return InTaskList (here);
		}
		
		public bool InTaskList (TextIter cursor)
		{
			TaskListTag tlt = (TaskListTag) Buffer.GetDynamicTag ("tasklist", cursor);
			return (tlt!=null);
		}
		
		public Task GetTask ()
		{
			TaskTag tag = GetTaskTag ();
			if(tag != null)
				return tag.Task; 
			return null;
		}
		
		public Task GetTask (TextIter iter)
		{
			TaskTag tag = GetTaskTag (iter);
			if (tag != null)
				return tag.Task;
			return null;
		}
		
		public TaskTag GetTaskTag ()
		{
			return GetTaskTag (Buffer.GetIterAtMark (Buffer.InsertMark));
		}
				
		public bool IsTextTodoItem (String text)
		{
			return text.Trim().Equals("[]");
		}
		
		public TaskTag GetTaskTag (TextIter iter)
		{
			return (TaskTag)Buffer.GetDynamicTag ("task", iter);
		}
		
		public void ResetCursor ()
		{
			Buffer.PlaceCursor (Buffer.GetIterAtMark(Buffer.InsertMark));
		}
		
		public TaskList GetTaskList ()
		{
			TaskListTag tag = GetTaskListTag ();
			if (tag != null)
				return tag.TaskList;
			return null;
		}
		
		public void RemoveTaskTags (Gtk.TextIter start, Gtk.TextIter end)
		{
			var iter = start;

			Buffer.RemoveTag ("locked", start, end);
			Buffer.RemoveTag ("checkbox", start, end);
			Buffer.RemoveTag ("checkbox-active", start, end);
			Buffer.RemoveTag ("priority", start, end);
			//Buffer.RemoveTag ("duedate", start, end);
			
			while (!iter.Equal (end))
			{
				bool found = true;
				while (found) {
					found = false;
					Gtk.TextTag tag = Buffer.GetDynamicTag ("task", iter);
					if (tag != null) {
						Buffer.RemoveTag (tag, start, end);
						found = true;
					}
					tag = Buffer.GetDynamicTag ("tasklist", iter);
					if (tag != null) {
						Buffer.RemoveTag (tag, start, end);
						found = true;
					}
				}
				iter.ForwardChar ();
			}
		}		
		public TaskList GetTaskList (TextIter iter)
		{
			TaskListTag tag = GetTaskListTag (iter);
			if (tag != null)
				return tag.TaskList;
			return null;
		}
		
		public TaskListTag GetTaskListTag ()
		{
			return GetTaskListTag (Buffer.GetIterAtMark (Buffer.InsertMark));
		}
		
		public TaskListTag GetTaskListTag (TextIter iter)
		{
			return (TaskListTag) Buffer.GetDynamicTag ("tasklist", iter);
		}
	}
}
