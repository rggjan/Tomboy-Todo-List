run: ~/.config/tomboy/addins/InsertDateTime.dll
	tomboy

~/.config/tomboy/addins/InsertDateTime.dll: InsertDateTime.dll
	cp ./InsertDateTime.dll ~/.config/tomboy/addins/

InsertDateTime.dll: InsertDateTime.cs InsertDateTime.addin.xml
	gmcs -debug -out:InsertDateTime.dll -target:library -pkg:tomboy-addins -r:Mono.Posix InsertDateTime.cs -resource:InsertDateTime.addin.xml
