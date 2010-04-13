all: requirements.pdf

requirements.pdf: *.tex graphics/*.png
	pdflatex requirements-document.tex
	makeindex requirements-document.idx
	pdflatex requirements-document.tex 

draft: *.tex graphics/*.png
	pdflatex requirements-document.tex

clean: 
	rm -f *.toc *.log *.aux *~ *.pdf *.idx *.ilg *.ind *.out
