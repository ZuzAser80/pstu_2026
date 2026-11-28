#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""График к лабораторной работе №6 «Краевая задача, метод прогонки».

Вариант 4: y'' - x*y' + 2*y = x + 1,
           y(0.9) - 0.5*y'(0.9) = 2, y(1.2) = 1.
Скрипт не требует сторонних библиотек Python: строит SVG и преобразует
его в PDF/PNG доступным конвертером (rsvg-convert, Inkscape, LibreOffice
или ImageMagick).

Запуск:
    python3 graphics/main.py
"""

import os
import shutil
import subprocess

# --- данные варианта 4 ------------------------------------------------------
LEFT, RIGHT = 0.9, 1.2
STEP = 0.05
ALPHA0, ALPHA1, LEFT_VALUE = 1.0, -0.5, 2.0
BETA0, BETA1, RIGHT_VALUE = 1.0, 0.0, 1.0

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "img")


def coefficients(x, h):
    p, q, f = -x, 2.0, x + 1.0
    denominator = 1.0 + 0.5 * p * h
    return ((1.0 - 0.5 * p * h) / denominator,
            (2.0 - q * h * h) / denominator,
            h * h * f / denominator)


def solve(h):
    """Прогонка для системы (2.11); возвращает список (x_i, y_i)."""
    n = int(round((RIGHT - LEFT) / h))
    x = [LEFT + i * h for i in range(n + 1)]
    p = [0.0] * (n + 1)
    q = [0.0] * (n + 1)
    y = [0.0] * (n + 1)

    left_denominator = ALPHA0 * h - ALPHA1
    p[0] = -ALPHA1 / left_denominator
    q[0] = LEFT_VALUE * h / left_denominator
    for i in range(1, n):
        a, b, d = coefficients(x[i], h)
        denominator = a * p[i - 1] - b
        p[i] = -1.0 / denominator
        q[i] = (d - a * q[i - 1]) / denominator

    right_denominator = BETA0 * h + BETA1 * (1.0 - p[n - 1])
    y[n] = (RIGHT_VALUE * h + BETA1 * q[n - 1]) / right_denominator
    for i in range(n - 1, -1, -1):
        y[i] = p[i] * y[i + 1] + q[i]
    return list(zip(x, y))


def map_x(value, left, right, xmin, xmax):
    return left + (value - xmin) / (xmax - xmin) * (right - left)


def map_y(value, top, bottom, ymin, ymax):
    return bottom - (value - ymin) / (ymax - ymin) * (bottom - top)


def make_svg(coarse, fine):
    width, height = 1000, 620
    left, right = 95, 45
    top, bottom = 55, 80
    xmin, xmax = 0.88, 1.22
    ymin, ymax = 0.96, 1.45

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
    for value in [1.0, 1.1, 1.2, 1.3, 1.4]:
        y = py(value)
        parts.append(
            '<line x1="%d" y1="%.1f" x2="%d" y2="%.1f" stroke="#d0d0d0" '
            'stroke-dasharray="4,4"/>\n' % (left, y, width - right, y)
        )
        parts.append(
            '<text x="%d" y="%.1f" text-anchor="end" font-size="15" '
            'fill="#555">%.1f</text>\n' % (left - 12, y + 5, value)
        )

    for value in [0.90, 0.95, 1.00, 1.05, 1.10, 1.15, 1.20]:
        x = px(value)
        parts.append(
            '<line x1="%.1f" y1="%d" x2="%.1f" y2="%d" stroke="#e4e4e4"/>\n'
            % (x, top, x, height - bottom)
        )
        parts.append(
            '<text x="%.1f" y="%d" text-anchor="middle" font-size="15" '
            'fill="#555">%.2f</text>\n' % (x, height - bottom + 26, value)
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

    # Решение на мелкой сетке h/2 (тонкая линия) и на сетке h (ломаная).
    fine_points = " ".join("%.2f,%.2f" % (px(xv), py(yv)) for xv, yv in fine)
    parts.append(
        '<polyline points="%s" fill="none" stroke="#2c78bf" stroke-width="3" '
        'opacity="0.75"/>\n' % fine_points
    )
    coarse_points = " ".join("%.2f,%.2f" % (px(xv), py(yv)) for xv, yv in coarse)
    parts.append(
        '<polyline points="%s" fill="none" stroke="#FF6B00" stroke-width="5" '
        'stroke-linecap="round" stroke-linejoin="round"/>\n' % coarse_points
    )

    # Узлы грубой сетки.
    for xv, yv in coarse:
        parts.append(
            '<circle cx="%.2f" cy="%.2f" r="7" fill="#c0392b" stroke="white" '
            'stroke-width="2"/>\n' % (px(xv), py(yv))
        )

    # Краевые точки.
    for index, label in [(0, "y(0.9) = 1.419394"), (len(coarse) - 1, "y(1.2) = 1")]:
        xv, yv = coarse[index]
        tx, ty = px(xv), py(yv)
        parts.append(
            '<circle cx="%.2f" cy="%.2f" r="9" fill="none" stroke="#1e7b34" '
            'stroke-width="3"/>\n' % (tx, ty)
        )
        anchor = "start" if index == 0 else "end"
        offset = 16 if index == 0 else -16
        parts.append(
            '<text x="%.2f" y="%.2f" font-size="16" fill="#1e7b34" '
            'text-anchor="%s">%s</text>\n'
            % (tx + offset, ty - 14, anchor, label)
        )

    # Легенда.
    lx, ly = width - right - 320, top + 15
    parts.append(
        '<rect x="%d" y="%d" width="300" height="82" rx="8" fill="white" '
        'fill-opacity="0.92" stroke="#bbbbbb"/>\n' % (lx, ly)
    )
    parts.append(
        '<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#FF6B00" '
        'stroke-width="5"/>\n' % (lx + 15, ly + 25, lx + 55, ly + 25)
    )
    parts.append(
        '<text x="%d" y="%d" font-size="16">решение при h = 0.05</text>\n'
        % (lx + 65, ly + 31)
    )
    parts.append(
        '<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#2c78bf" '
        'stroke-width="3"/>\n' % (lx + 15, ly + 57, lx + 55, ly + 57)
    )
    parts.append(
        '<text x="%d" y="%d" font-size="16">решение при h = 0.025</text>\n'
        % (lx + 65, ly + 63)
    )

    # Заголовок и подписи осей.
    parts.append(
        '<text x="%d" y="32" text-anchor="middle" font-size="22" '
        'font-weight="bold">Краевая задача, метод прогонки, вариант 4</text>\n'
        % (width // 2)
    )
    parts.append(
        '<text x="%d" y="%d" text-anchor="middle" font-size="18">x</text>\n'
        % (width // 2, height - 20)
    )
    parts.append(
        '<text x="28" y="%d" text-anchor="middle" font-size="18" '
        'transform="rotate(-90 28 %d)">y</text>\n'
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
            run([soffice, "--headless", "--convert-to", "pdf",
                 "--outdir", out_dir, svg_path])
            run([soffice, "--headless", "--convert-to", "png",
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

    coarse = solve(STEP)
    fine = solve(STEP / 2.0)

    svg_path = os.path.join(OUT_DIR, "fig_bvp.svg")
    with open(svg_path, "w", encoding="utf-8") as stream:
        stream.write(make_svg(coarse, fine))
    print("  " + svg_path)

    pdf_path = os.path.join(OUT_DIR, "fig_bvp.pdf")
    png_path = os.path.join(OUT_DIR, "fig_bvp.png")
    if not render_figure(svg_path, pdf_path, png_path):
        print("  Конвертер SVG не найден: PDF/PNG не построены.")
        return

    print("  " + pdf_path)
    print("  " + png_path)
    print("  y(0.9) = %.10f, y(1.2) = %.10f" % (coarse[0][1], coarse[-1][1]))


if __name__ == "__main__":
    main()
