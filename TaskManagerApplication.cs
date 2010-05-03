using System;
using Mono.Unix;
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager {
	
	class TaskManagerApplicationAddin : ApplicationAddin {
		
		private bool initialized;
		
		public override bool Initialized {
			get{return initialized;}	
		}
		
		public override void Shutdown ()
		{	
		}
		
		public override void Initialize()
		{
			Tomboy.ActionManager.UI.AddUiFromString(@"
			<ui>
			<menubar name='MainWindowMenubar'>
	    	<placeholder name='MainWindowMenuPlaceholder'>
	      	<menu name='ToolsMenu' action='ToolsMenuAction'>
	        <menuitem name='TaskManager' action='TaskManagerAction'/>
	      	</menu>
	    	</placeholder>
	  		</menubar>
			</ui> 
			");
			
			ActionGroup group = new ActionGroup ("TaskManager");
			group.Add (new Gtk.ActionEntry [] {new Gtk.ActionEntry ("TaskManagerAction", null,"Test", null, null, null)});
			
			Tomboy.ActionManager.UI.InsertActionGroup (group,0);
			initialized = true;
			
			// Register additional Tags
			NoteTagTable tagtable = NoteTagTable.Instance;
			
			if(tagtable.Lookup("task") == null)
				tagtable.Add (new TaskTag ());
		}
	}
}