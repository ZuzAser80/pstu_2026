#!/usr/bin/env bash
# Сборка лабораторной работы №2: графики, программа, отчёт.
set -e
cd "$(dirname "$0")"

echo "== 1. Построение графиков =="
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
elif command -v xetex >/dev/null 2>&1; then
    # xelatex-обёртки нет, но есть xetex: формат xelatex собирается в
    # пользовательское дерево TeX (без прав root)
    echo "  xelatex не найден, готовится формат xelatex через xetex..."
    fmtutil-user --byfmt xelatex >/dev/null 2>&1 || true
    if [ -f "$HOME/.texlive2025/texmf-var/web2c/xetex/xelatex.fmt" ] || kpsewhich xelatex.fmt >/dev/null 2>&1; then
        engine="xetex-fmt"
    else
        echo "Ошибка: формат xelatex.fmt не найден." >&2
        exit 1
    fi
elif command -v lualatex >/dev/null 2>&1; then
    engine="lualatex"
elif command -v pdflatex >/dev/null 2>&1; then
    engine="pdflatex"
fi
if [ -z "$engine" ]; then
    echo "Ошибка: не найден LaTeX (xelatex/lualatex/xetex/pdflatex)." >&2
    exit 1
fi
echo "Используется движок: $engine"

run_tex() {
    case "$engine" in
        xetex-fmt) xetex -fmt=xelatex -output-driver=xdvipdfmx "$@" ;;
        *)         "$engine" "$@" ;;
    esac
}

run_tex -interaction=nonstopmode BastrakovReport.tex
run_tex -interaction=nonstopmode BastrakovReport.tex

echo
echo "Готово: BastrakovReport.pdf"