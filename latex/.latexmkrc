# Общий конфиг latexmk для всех отчётов в /latex
# ВАЖНО: latexmk ищет rc-файл только в ТЕКУЩЕЙ папке, не в родительских.
#   из /latex:            latexmk main.tex
#   из подпапки проекта:  latexmk -r ../.latexmkrc main.tex
# (можно также симлинк:  ln -s ../.latexmkrc .latexmkrc)

# --- Движок: XeLaTeX (как в Makefile шаблона latex-g7-32) ---
$pdf_mode = 5;              # 5 = xelatex
$xelatex = 'xelatex -interaction=nonstopmode -halt-on-error -file-line-error -shell-escape %O %S';
$pdf_previewer = 'start xdg-open %S';

# --- BibTeX для .bib ---
$bibtex_use = 2;            # библиография через bibtex, чистить .bbl при -c

# --- Выходные файлы: всё во вложенную папку build/ ---
$out_dir = 'build';

# --- Стили latex-g7-32 без копирования в каждый проект ---
# Ищем клон шаблона, поднимаясь от текущей папки вверх до /latex
use Cwd qw(abs_path);
use File::Spec;
use File::Basename qw(dirname);
my $root = abs_path('.');
while ($root ne File::Spec->rootdir() && !-d "$root/latex-g7-32") {
    my $parent = dirname($root);
    last if $parent eq $root;
    $root = $parent;
}
if (-d "$root/latex-g7-32") {
    $ENV{'TEXINPUTS'} = "$root/latex-g7-32/G7-32/tex//:$root/latex-g7-32/tex//:" . ($ENV{'TEXINPUTS'} // '');
}
