all: requirements.pdf

requirements.pdf: *.tex graphics/Screenshot_cropped_edited.png
	pdflatex requirements-document.tex
	pdflatex requirements-document.tex 

clean: 
	rm -f *.toc *.log *.aux *~ *.pdf
