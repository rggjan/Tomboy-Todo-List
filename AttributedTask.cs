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
using Gtk;

namespace Tomboy.TaskManager
{
	
	/// <summary>
	/// Class that represents the common functionality of TaskList and Task
	/// </summary>
	public abstract class AttributedTask {
	
		protected TaskNoteUtilities utils;
		
		public void Initialize (TextIter iter, AttributedTaskTag tag)
		{
			//this.Buffer = buffer;
			this.Position = Buffer.CreateMark (null, iter, true);
			this.Tag = tag;
			Tag.AttributedTask = this;
			
			utils = new TaskNoteUtilities (Buffer);
		}
		
		/// <summary>
		/// The duedate of the corresponding task or tasklist
		/// if DateTime.MinValue
		/// </summary>
		public Nullable<DateTime> DueDate {
			
			get { 
				TextTagEnumerator dates = new TextTagEnumerator (Buffer, "duedate");
				
				foreach (TextRange r in dates) {
					if (r.Start.Compare (DescriptionStart) >= 0 && r.End.Compare (DescriptionEnd) <=0){
						try{
						 	return DateTime.Parse (r.Text);
						} catch (FormatException ex) {
							return null;	
						}
					}
				}
				
				return null;
			}
		}
		
		/// <summary>
		/// True if this task's duedate lies in the past, false if the duedates time is in the future or not set.
		/// </summary>
		/// <returns>
		/// A <see cref="boolean"/>; 
		/// </returns>
		public bool IsOverdue ()
		{
			DateTime? date = DueDate;
			if (DueDate != null)
				return ((DateTime)date).CompareTo(DateTime.Now) <= 0;
			return false;
		}
		
		/// <summary>
		/// Whether or not this task has been completed
		/// </summary>
		public abstract bool Done {
			get; set;
		}
		
		/// <summary>
		/// Tag that is attached to this attributedtask (in the buffer)
		/// </summary>
		public AttributedTaskTag Tag {
			get; set;	
		}
		
		/// <summary>
		/// Describes the shortcut to the buffer
		/// </summary>
		protected abstract NoteBuffer Buffer {
			get;
		}
		
		protected TextMark Position;
		
		public String Description ()
		{
			return Buffer.GetText (DescriptionStart, DescriptionEnd, false);
		}
				
		public TextIter Start {
			get { return Buffer.GetIterAtMark (Position); }	
		}

		protected abstract TextIter DescriptionStart {
			get;
		}
		
		protected abstract TextIter DescriptionEnd {
			get;
		}
		
		protected abstract TextIter End {
			get;	
		}
		
		
		public bool DueDateSet {
			get 
			{
				TextTagEnumerator dates = new TextTagEnumerator (Buffer, "duedate");
				
				foreach (TextRange r in dates)
				{
					if (r.Start.Compare (DescriptionStart) >= 0 && r.End.Compare (DescriptionEnd) <=0)
						return true;
				}
				
				return false;
			}
		}
		
		private DateTag datetag;
		private DateTag DateTag{
			get {
				if (datetag ==null)
					datetag = (DateTag) Buffer.TagTable.Lookup ("duedate");
			
				return datetag;
			}	
		}
		
		public void AddDueDate (DateTime date){
			TextIter pos = Buffer.GetIterAtMark (Buffer.InsertMark);
			Buffer.InsertWithTags (
			                       ref pos, 
			                       date.ToShortDateString (), 
			                       new TextTag[]{DateTag}
			);
		}
	}
}
