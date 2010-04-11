all: requirements.pdf

requirements.pdf: *.tex graphics/*.png
	pdflatex requirements-document.tex
	pdflatex requirements-document.tex 

clean: 
	rm -f *.toc *.log *.aux *~ *.pdf
