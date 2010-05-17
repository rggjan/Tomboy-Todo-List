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
using System.Reflection;
using System.IO;
using Tomboy;

namespace Tomboy.TaskManager.Tests
{

	/// <summary>
	/// This is a helper class for the unit tests. It creates Temporary Files from the XML
	/// sample notes file which are embedded as ressources because tomboy
	/// cannot instantiate notes with ressources directly.
	/// </summary>
	internal static class TemporaryFileManager
	{
		
		/// <summary>
		/// Creates a file from an embedded ressource in the assembly.
		/// </summary>
		/// <param name="resourceName">
		/// A <see cref="System.String"/> descring the ressource name.
		/// </param>
		/// <returns>
		/// A <see cref="FileInfo"/> linking to a temporary file with the contents of the ressource.
		/// </returns>
		internal static FileInfo CreateFile (string resourceName)
		{
			FileInfo file = new FileInfo (Path.GetTempFileName ());
			
			Assembly assembly = Assembly.GetCallingAssembly ();
			Stream istream = assembly.GetManifestResourceStream (resourceName);
			using (FileStream ostream = file.Open (FileMode.Create, FileAccess.Write)) {
				
				byte[] buffer = new byte[0x2000];
				int bytes;
				
				while ((bytes = istream.Read (buffer, 0, buffer.Length)) > 0) {
					ostream.Write (buffer, 0, bytes);
				}
				
			}
			
			return file;
		}

		
		/// <summary>
		/// Deletes a file, used to delete temporary files after they've been created.
		/// </summary>
		/// <param name="file">
		/// A <see cref="FileInfo"/>
		/// </param>
		internal static void DeleteFile (FileInfo file)
		{
			if (file != null) {
				string fullName = file.FullName;
				try {
					file.Delete ();
				} catch (IOException ex) {
					Logger.Debug ("Cannot delete \"{0}\": {1}", fullName, ex);
				}
			}
		}

	}	
}
