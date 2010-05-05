using System;
using Mono.Unix;
using Gtk;
using Tomboy;

namespace Tomboy.TaskManager {
	
	class TaskManagerApplicationAddin : ApplicationAddin {
		
		private bool initialized;
		public override bool Initialized {
			get { return initialized; }
		}

		
		/// <summary>
		/// Sets up the TaskManager Addin.
		/// </summary>
		public override void Initialize()
		{
			initialized = true;
			
			// Register additional Tags
			// Edit: Deleted. No need anymore
		}
		
		
		/// <summary>
		/// Cleanup when TaskManager is disabled or Tomboy is closed.
		/// </summary>
		public override void Shutdown ()
		{
		}
	}
}