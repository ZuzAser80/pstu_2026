#!/usr/bin/env bash
# Сборка лабораторной работы №3: график, программа, отчёт.
set -e
cd "$(dirname "$0")"

echo "== 1. Построение графика =="
if [ -x graphics/.venv/bin/python ]; then
    graphics/.venv/bin/python graphics/main.py
else
    python3 graphics/main.py
fi

echo
echo "== 2. Запуск программы (C#) =="
dotnet run

echo
echo "== 3. Сборка отчёта LaTeX =="
engine=""
if command -v xelatex >/dev/null 2>&1; then
    engine="xelatex"
elif command -v lualatex >/dev/null 2>&1; then
    engine="lualatex"
elif command -v pdflatex >/dev/null 2>&1; then
    engine="pdflatex"
fi

if [ -z "$engine" ]; then
    echo "LaTeX-движок не найден (xelatex/lualatex/pdflatex)."
    echo "Отчёт не собран; исходник BastrakovReport.tex готов к сборке."
    exit 0
fi

echo "Используется движок: $engine"
"$engine" -interaction=nonstopmode BastrakovReport.tex
"$engine" -interaction=nonstopmode BastrakovReport.tex

echo
echo "Готово: BastrakovReport.pdf"
