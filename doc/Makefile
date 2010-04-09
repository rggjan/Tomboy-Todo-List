all: requirements.pdf

requirements.pdf: *.tex 
	pdflatex requirements-document.tex
	pdflatex requirements-document.tex 

clean: 
	rm -f *.toc *.log *.aux *~ *.pdf
