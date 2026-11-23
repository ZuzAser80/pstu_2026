#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""График к лабораторной работе №4 «Численное интегрирование».

Вариант 4: f(x) = x^2/(x^2+1), отрезок интегрирования задаётся в input.txt.
Скрипт не требует сторонних библиотек Python: он строит SVG-файл, а затем
преобразует его в PDF и PNG для отчёта доступным конвертером (rsvg-convert,
Inkscape или LibreOffice). ImageMagick используется только как последний
резерв: его встроенный рендерер SVG рисует линии серым и теряет цвет кривой.

Запуск:
    python3 graphics/main.py
"""

import math
import os
import shutil
import subprocess

INPUT_FILE = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "input.txt"
)
OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "img")


def read_input(path):
    """Прочитать a, b, epsilon и n0 из input.txt (тот же формат, что и в C#)."""
    tokens = []
    with open(path, encoding="utf-8") as stream:
        for raw_line in stream:
            line = raw_line.split("#", 1)[0]
            tokens.extend(line.split())

    if len(tokens) != 4:
        raise ValueError("ожидалось 4 числа (a, b, epsilon, n0) в " + path)

    a = float(tokens[0].replace(",", "."))
    b = float(tokens[1].replace(",", "."))
    epsilon = float(tokens[2].replace(",", "."))
    n0 = int(tokens[3])

    if not a < b or epsilon <= 0 or n0 < 2:
        raise ValueError("проверьте a < b, epsilon > 0 и n0 >= 2")
    return a, b, epsilon, n0


def function(x):
    return x * x / (x * x + 1.0)


def trapezoid(a, b, n):
    step = (b - a) / n
    total = 0.5 * (function(a) + function(b))
    for i in range(1, n):
        total += function(a + i * step)
    return step * total


def final_parts(a, b, epsilon, n0):
    """Число разбиений, на котором трапеции достигают заданной точности."""
    parts = n0
    coarse = trapezoid(a, b, parts)
    while True:
        fine_parts = parts * 2
        fine = trapezoid(a, b, fine_parts)
        if abs(fine - coarse) < epsilon:
            return fine_parts
        parts, coarse = fine_parts, fine


def map_x(value, left, right, xmin, xmax):
    return left + (value - xmin) / (xmax - xmin) * (right - left)


def map_y(value, top, bottom, ymin, ymax):
    return bottom - (value - ymin) / (ymax - ymin) * (bottom - top)


def make_svg(a, b, epsilon, n0, final_n):
    width, height = 1000, 620
    left, right = 95, 45
    top, bottom = 60, 85

    span = b - a
    xmin, xmax = a - 0.06 * span, b + 0.06 * span
    ymin, ymax = 0.0, 1.0

    def px(value):
        return map_x(value, left, width - right, xmin, xmax)

    def py(value):
        return map_y(value, top, height - bottom, ymin, ymax)

    parts = []
    parts.append(
        '<?xml version="1.0" encoding="UTF-8"?>\n'
        '<svg xmlns="http://www.w3.org/2000/svg" width="%d" height="%d" '
        'viewBox="0 0 %d %d">\n' % (width, height, width, height)
    )
    parts.append('<rect width="100%" height="100%" fill="white"/>\n')
    parts.append(
        '<g font-family="DejaVu Sans, Liberation Sans, sans-serif" '
        'font-size="18" fill="#222">\n'
    )

    # Сетка и подписи по осям.
    for k in range(11):
        value = k / 10.0
        y = py(value)
        parts.append(
            '<line x1="%d" y1="%.1f" x2="%d" y2="%.1f" stroke="#d0d0d0" '
            'stroke-dasharray="4,4"/>\n' % (left, y, width - right, y)
        )
        parts.append(
            '<text x="%d" y="%.1f" text-anchor="end" font-size="15" '
            'fill="#555">%.1f</text>\n' % (left - 12, y + 5, value)
        )

    for k in range(7):
        value = a + span * k / 6.0
        x = px(value)
        parts.append(
            '<line x1="%.1f" y1="%d" x2="%.1f" y2="%d" stroke="#e4e4e4"/>\n'
            % (x, top, x, height - bottom)
        )
        parts.append(
            '<text x="%.1f" y="%d" text-anchor="middle" font-size="15" '
            'fill="#555">%.1f</text>\n' % (x, height - bottom + 26, value)
        )

    # Оси.
    parts.append(
        '<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#222" stroke-width="2"/>\n'
        % (left, height - bottom, width - right, height - bottom)
    )
    parts.append(
        '<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#222" stroke-width="2"/>\n'
        % (left, top, left, height - bottom)
    )

    # Площадь под кривой на отрезке [a, b].
    sample_count = 500
    curve = []
    for k in range(sample_count + 1):
        xv = a + span * k / sample_count
        curve.append((px(xv), py(function(xv))))
    area = [(px(a), py(0.0))] + curve + [(px(b), py(0.0))]
    parts.append(
        '<polygon points="%s" fill="#FF6B00" fill-opacity="0.20"/>\n'
        % " ".join("%.2f,%.2f" % point for point in area)
    )

    # Границы области и кривую f(x) рисуем поверх заливки.
    for xv in (a, b):
        parts.append(
            '<line x1="%.2f" y1="%d" x2="%.2f" y2="%.2f" stroke="#1e7b34" '
            'stroke-width="2"/>\n' % (px(xv), height - bottom, px(xv), py(function(xv)))
        )

    parts.append(
        '<polyline points="%s" fill="none" stroke="#FF6B00" stroke-width="6" '
        'stroke-linecap="round" stroke-linejoin="round"/>\n'
        % " ".join("%.2f,%.2f" % point for point in curve)
    )

    # Начальное разбиение n0 — вертикальные пунктирные линии.
    for k in range(1, n0):
        xv = a + span * k / n0
        parts.append(
            '<line x1="%.2f" y1="%d" x2="%.2f" y2="%.2f" stroke="#1e7b34" '
            'stroke-width="1.3" stroke-dasharray="5,4" stroke-opacity="0.75"/>\n'
            % (px(xv), height - bottom, px(xv), py(function(xv)))
        )
    for k in range(n0 + 1):
        xv = a + span * k / n0
        parts.append(
            '<circle cx="%.2f" cy="%.2f" r="4" fill="#1e7b34" stroke="white" '
            'stroke-width="1.5"/>\n' % (px(xv), py(function(xv)))
        )

    # Легенда.
    lx, ly = width - right - 320, top + 15
    parts.append(
        '<rect x="%d" y="%d" width="305" height="124" rx="8" fill="white" '
        'fill-opacity="0.94" stroke="#bbbbbb"/>\n' % (lx, ly)
    )
    parts.append(
        '<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#FF6B00" stroke-width="6" '
        'stroke-linecap="round"/>\n' % (lx + 15, ly + 24, lx + 55, ly + 24)
    )
    parts.append(
        '<text x="%d" y="%d" font-size="16">f(x) = x^2/(x^2+1)</text>\n'
        % (lx + 65, ly + 30)
    )
    parts.append(
        '<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#1e7b34" '
        'stroke-width="1.6" stroke-dasharray="5,4"/>\n' % (lx + 15, ly + 56, lx + 55, ly + 56)
    )
    parts.append(
        '<text x="%d" y="%d" font-size="16">разбиение n0 = %d</text>\n'
        % (lx + 65, ly + 62, n0)
    )
    parts.append(
        '<text x="%d" y="%d" font-size="16">epsilon = %g, n = %d</text>\n'
        % (lx + 15, ly + 96, epsilon, final_n)
    )

    # Заголовок и подписи осей.
    parts.append(
        '<text x="%d" y="32" text-anchor="middle" font-size="22" '
        'font-weight="bold">Численное интегрирование, вариант 4</text>\n' % (width // 2)
    )
    parts.append(
        '<text x="%d" y="%d" text-anchor="middle" font-size="18">x</text>\n'
        % (width // 2, height - 20)
    )
    parts.append(
        '<text x="28" y="%d" text-anchor="middle" font-size="18" '
        'transform="rotate(-90 28 %d)">f(x)</text>\n'
        % (height // 2, height // 2)
    )
    parts.append("</g>\n</svg>\n")
    return "".join(parts)


def render_figure(svg_path, pdf_path, png_path):
    """Преобразовать SVG в PDF и PNG, сохраняя цвета линий."""
    def run(command):
        subprocess.run(
            command,
            check=True,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )

    rsvg = shutil.which("rsvg-convert")
    if rsvg:
        try:
            run([rsvg, "-f", "pdf", "-o", pdf_path, svg_path])
            run([rsvg, "-f", "png", "-d", "150", "-p", "150",
                 "-o", png_path, svg_path])
            return True
        except subprocess.CalledProcessError:
            pass

    inkscape = shutil.which("inkscape")
    if inkscape:
        try:
            run([inkscape, svg_path, "--export-type=pdf",
                 "--export-filename=" + pdf_path])
            run([inkscape, svg_path, "--export-type=png",
                 "--export-dpi=150", "--export-filename=" + png_path])
            return True
        except subprocess.CalledProcessError:
            pass

    soffice = shutil.which("soffice") or shutil.which("libreoffice")
    if soffice:
        out_dir = os.path.dirname(pdf_path)
        try:
            for fmt in ("pdf", "png"):
                run([soffice, "--headless", "--convert-to", fmt,
                     "--outdir", out_dir, svg_path])
            if os.path.exists(pdf_path) and os.path.exists(png_path):
                return True
        except subprocess.CalledProcessError:
            pass

    converter = shutil.which("convert")
    if converter:
        print("  Внимание: ImageMagick искажает цвет линий SVG.")
        run([converter, "-density", "150", svg_path, pdf_path])
        run([converter, "-density", "150", svg_path, png_path])
        return True

    return False


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    a, b, epsilon, n0 = read_input(INPUT_FILE)
    final_n = final_parts(a, b, epsilon, n0)

    svg_path = os.path.join(OUT_DIR, "fig_integral.svg")
    with open(svg_path, "w", encoding="utf-8") as stream:
        stream.write(make_svg(a, b, epsilon, n0, final_n))
    print("  " + svg_path)

    pdf_path = os.path.join(OUT_DIR, "fig_integral.pdf")
    png_path = os.path.join(OUT_DIR, "fig_integral.png")
    if not render_figure(svg_path, pdf_path, png_path):
        print("  Конвертер SVG не найден: PDF/PNG не построены.")
        return

    print("  " + pdf_path)
    print("  " + png_path)
    print("  a = %.1f, b = %.1f, epsilon = %g, n0 = %d, n = %d"
          % (a, b, epsilon, n0, final_n))


if __name__ == "__main__":
    main()
