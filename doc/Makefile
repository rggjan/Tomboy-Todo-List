pdfcommand = pdflatex -output-directory=tmp requirements-document.tex && mv tmp/requirements-document.pdf ./

all: requirements-document.pdf

requirements-document.pdf: *.tex graphics/*.png
	$(pdfcommand)
	makeindex tmp/requirements-document.idx
	$(pdfcommand)

draft: *.tex graphics/*.png
	$(pdfcommand)

clean: 
	rm -r tmp
