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

namespace Tomboy.TaskManager
{
	
	/// <summary>
	/// Class for the tag representation of a task or tasklist in the buffer
	/// Used by AttributedTask - Sums up the commonalities between tasktag and tasklisttag
	/// </summary>
	public class AttributedTaskTag : DynamicNoteTag
	{
		
		/// <summary>
		/// Set common (TaskList, Task) properties
		/// </summary>
		/// <param name="element_name">
		/// A <see cref="System.String"/>
		/// </param>
		public override void Initialize (string element_name)
		{
			base.Initialize (element_name);
			CanGrow = true;
			LeftMarginSet = true;
		}
		
		
		/// <summary>
		/// Task this tag is (currently, but ought to be fixed) attached to
		/// </summary>
		public AttributedTask AttributedTask {
			get;
			set;
		}
		
		/// <summary>
		/// Attach a task or tasklist to this tag
		/// </summary>
		/// <param name="atask">
		/// A <see cref="AttributedTask"/>
		/// </param>
		public void Bind (AttributedTask atask)
		{
			AttributedTask = atask;
		}
	}
}
