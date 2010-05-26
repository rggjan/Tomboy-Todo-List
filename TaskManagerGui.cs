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
using System.Reflection;
using Mono.Unix;
using Gtk;
using Gdk;
using Tomboy;

namespace Tomboy.TaskManager
{	
	
	/// <summary>
	/// The class that manages the Menu and GUI stuff of our Addin.
	/// </summary>
	public class TaskManagerGui
	{
		private Gtk.Menu task_menu = new Gtk.Menu ();
		private Gtk.ImageMenuItem add_priority = new Gtk.ImageMenuItem (Catalog.GetString ("Add Priority"));
		private Gtk.ImageMenuItem add_duedate = new Gtk.ImageMenuItem (Catalog.GetString ("Add Duedate"));
		private Gtk.CheckMenuItem show_priority = new Gtk.CheckMenuItem (Catalog.GetString ("Show Priorities"));
		private Gtk.MenuToolButton menu_tool_button;
			
		private TaskManagerNoteAddin addin;
		
		public NoteBuffer Buffer
		{
			get {
				return addin.Buffer;
			}
		}

		
		public Note Note {
			
			get {
				return addin.Note;
			}
		}
		
		public bool PriorityShown
		{
			get {
				return show_priority.Active;
			}
			set {
				if (show_priority.Active != value)
					{
						show_priority.Active = value;
						TogglePriorityVisibility ();
					}
			}
		}

		TaskNoteUtilities utils = null;
		
		public TaskManagerGui (TaskManagerNoteAddin addin)
		{
			this.addin = addin;
			utils = new TaskNoteUtilities (addin.Buffer);
			
			var duedateImage = new Gtk.Image(null, "Tomboy.TaskManager.Icons.duedate-icon22.png");
			add_duedate.Image = duedateImage;
			
			var priorityImage = new Gtk.Image(null, "Tomboy.TaskManager.Icons.priority-icon22.png");
			add_priority.Image = priorityImage;
			
			task_menu.Add (add_duedate);
			task_menu.Add (add_priority);
			task_menu.Add (show_priority);

			if (Tomboy.Debugging) {
				Gtk.MenuItem print_structure = new Gtk.MenuItem (Catalog.GetString ("Print Structure"));
				print_structure.Activated += OnPrintStructureActivated;
				task_menu.Add (print_structure);
			}
			
			var todoImage = new Gtk.Image(null, "Tomboy.TaskManager.Icons.todo-icon24.png");
			menu_tool_button = new Gtk.MenuToolButton (todoImage, null);
			
			menu_tool_button.TooltipText = Catalog.GetString ("Add a new TaskList");
			menu_tool_button.ArrowTooltipText = Catalog.GetString ("Set TaskList properties");
			menu_tool_button.Menu = task_menu;
			task_menu.ShowAll ();
		
			menu_tool_button.Show ();

			addin.AddToolItem (menu_tool_button, -1);
		}
		
		public void StartListeners ()
		{
			add_duedate.Activated += OnAddDuedateActivated;
			add_priority.Activated += OnAddPriorityActivated;
			show_priority.Toggled += OnShowPriorityActivated;
			
			menu_tool_button.Clicked += OnAddListActivated;
			menu_tool_button.ShowMenu += UpdateMenuSensitivity;
		}
		
		public void StopListeners ()
		{
			add_duedate.Activated -= OnAddDuedateActivated;
			add_priority.Activated -= OnAddPriorityActivated;
			show_priority.Toggled -= OnShowPriorityActivated;
			
			menu_tool_button.Clicked -= OnAddListActivated;
			menu_tool_button.ShowMenu -= UpdateMenuSensitivity;
		}
		
		void OnAddDuedateActivated (object sender, EventArgs args)
		{
			Task t = utils.GetTask (Buffer.GetIterAtMark (Buffer.InsertMark));
			
			if (t != null && !t.DueDateSet){
				GetMinDueDateVisitor visitor = new GetMinDueDateVisitor ();
				visitor.visit (t);
				
				t.AddDueDate (visitor.Result);
			}
		}
		
		/// <summary>
		/// Add a new tasklist into the buffer
		/// </summary>
		/// <param name="sender">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="EventArgs"/>
		/// </param>
		void OnAddListActivated (object sender, EventArgs args)
		{
			if (utils.InTaskList ())
				return;
			
			TaskList tl = new TaskList (Note);
			//tl.Name = "New TaskList!";
			
			addin.TaskLists.Add (tl);
		}
		
		/// <summary>
		/// Add priority widget to some task
		/// </summary>
		/// <param name="sender">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="EventArgs"/>
		/// </param>
		void OnAddPriorityActivated (object sender, EventArgs args)
		{
			addin.StopListeners ();
			Gtk.TextIter cursor = Buffer.GetIterAtMark (Buffer.InsertMark);
			cursor.BackwardChar ();
			
			Task task = utils.GetTask ();
			if (task != null) {
				PriorityShown = true;
				task.AddPriority ();
			} else {
				Logger.Debug ("Tried to insert Priority outside of a task");	
			}
			addin.StartListeners ();
		}
		
		private void OnShowPriorityActivated (object sender, EventArgs args)
		{
			TogglePriorityVisibility ();
		}

		private void OnPrintStructureActivated (object sender, EventArgs args)
		{
			Logger.Debug ("----------------Printing structure!------------------");
			foreach (TaskList list in addin.TaskLists)
				list.DebugPrint();
			Logger.Debug ("-----------------------------------------------------");
		}
		
		private void TogglePriorityVisibility ()
		{
			addin.StopListeners ();
			if (PriorityShown) {
				foreach (TaskList list in addin.TaskLists)
					foreach (Task task in list.Tasks)
						task.ShowPriority ();
			} else {
				foreach (TaskList list in addin.TaskLists)
					foreach (Task task in list.Tasks)
						task.HidePriority ();
			}
			addin.StartListeners ();
		}
		
		/// <summary>
		/// Makes sure that add_priority menu items Sensitive property
		/// is set correctly according to where we currently are in the NoteBuffer
		/// </summary>
		void UpdateMenuSensitivity (object sender, EventArgs args)
		{
			// toggle sensitivity
			Task task = utils.GetTask ();
			if (task != null && task.PriorityUnset () && show_priority.Active) {
				//add_priority.Sensitive = true;
			}
			else {
				//add_priority.Sensitive = false;
			}
		}
	}
}
