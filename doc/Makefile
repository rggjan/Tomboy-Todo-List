pdfcommand = pdflatex -output-directory=tmp requirements-document.tex

all: tmp/requirements-document.pdf

tmp/requirements-document.pdf: *.tex graphics/*.png
	$(pdfcommand) -draftmode
	makeindex tmp/requirements-document.idx
	$(pdfcommand)

draft: *.tex graphics/*.png
	$(pdfcommand)

clean: 
	rm -r tmp/*
