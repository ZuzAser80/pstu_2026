#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""График к лабораторной работе №5 «Методы решения дифференциальных уравнений».

Вариант 4: 3*y*y' = 2*x, y(-2) = -3. Скрипт не требует сторонних библиотек
Python: он строит SVG, затем преобразует его в PDF и PNG доступным
конвертером (rsvg-convert, Inkscape, LibreOffice или ImageMagick).

Запуск:
    python3 graphics/main.py
"""

import math
import os
import shutil
import subprocess

# --- данные варианта 4 ------------------------------------------------------
X0, X1, Y0 = -2.0, -1.0, -3.0
H_COARSE, H_FINE = 0.1, 0.05

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "img")


def rhs(x, y):
    return 2.0 * x / (3.0 * y)


def exact(x):
    return -math.sqrt((2.0 * x * x + 19.0) / 3.0)


def rk4(x0, y0, h, count):
    """Метод Рунге-Кутта 4-го порядка: список точек (x, y)."""
    points = [(x0, y0)]
    x, y = x0, y0
    for index in range(count):
        k0 = h * rhs(x, y)
        k1 = h * rhs(x + h / 2.0, y + k0 / 2.0)
        k2 = h * rhs(x + h / 2.0, y + k1 / 2.0)
        k3 = h * rhs(x + h, y + k2)
        y += (k0 + 2.0 * k1 + 2.0 * k2 + k3) / 6.0
        x = x0 + (index + 1) * h
        points.append((x, y))
    return points


def map_x(value, left, right, xmin, xmax):
    return left + (value - xmin) / (xmax - xmin) * (right - left)


def map_y(value, top, bottom, ymin, ymax):
    return bottom - (value - ymin) / (ymax - ymin) * (bottom - top)


def make_svg(coarse, fine):
    width, height = 1000, 620
    left, right = 95, 45
    top, bottom = 55, 80
    xmin, xmax = -2.02, -0.98
    ymin, ymax = -3.02, -2.60

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
    for value in [-3.0, -2.9, -2.8, -2.7, -2.6]:
        y = py(value)
        parts.append(
            '<line x1="%d" y1="%.1f" x2="%d" y2="%.1f" stroke="#d0d0d0" '
            'stroke-dasharray="4,4"/>\n' % (left, y, width - right, y)
        )
        parts.append(
            '<text x="%d" y="%.1f" text-anchor="end" font-size="15" '
            'fill="#555">%.1f</text>\n' % (left - 12, y + 5, value)
        )

    for value in [-2.0, -1.9, -1.8, -1.7, -1.6, -1.5, -1.4, -1.3, -1.2, -1.1, -1.0]:
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

    # Точное решение.
    samples = 400
    curve = []
    for index in range(samples + 1):
        xv = xmin + (xmax - xmin) * index / samples
        curve.append("%.2f,%.2f" % (px(xv), py(exact(xv))))
    parts.append(
        '<polyline points="%s" fill="none" stroke="#1e7b34" stroke-width="5" '
        'stroke-linecap="round"/>\n' % " ".join(curve)
    )

    # Численные решения.
    for xv, yv in fine:
        parts.append(
            '<circle cx="%.2f" cy="%.2f" r="3.6" fill="#2c78bf" opacity="0.85"/>\n'
            % (px(xv), py(yv))
        )
    for xv, yv in coarse:
        parts.append(
            '<rect x="%.2f" y="%.2f" width="8" height="8" fill="#FF6B00" '
            'stroke="white" stroke-width="1"/>\n'
            % (px(xv) - 4, py(yv) - 4)
        )

    # Конечная точка x = -1.
    tx, ty = px(X1), py(exact(X1))
    parts.append(
        '<circle cx="%.2f" cy="%.2f" r="9" fill="#c0392b" stroke="white" '
        'stroke-width="2.5"/>\n' % (tx, ty)
    )
    parts.append(
        '<text x="%.2f" y="%.2f" font-size="17" fill="#c0392b" '
        'text-anchor="end">x = -1, y = -2.645751311</text>\n'
        % (tx - 14, ty + 30)
    )

    # Легенда.
    lx, ly = width - right - 330, top + 15
    parts.append(
        '<rect x="%d" y="%d" width="315" height="120" rx="8" fill="white" '
        'fill-opacity="0.92" stroke="#bbbbbb"/>\n' % (lx, ly)
    )
    parts.append(
        '<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#1e7b34" '
        'stroke-width="5"/>\n' % (lx + 15, ly + 25, lx + 55, ly + 25)
    )
    parts.append(
        '<text x="%d" y="%d" font-size="16">точное решение</text>\n'
        % (lx + 65, ly + 31)
    )
    parts.append(
        '<circle cx="%d" cy="%d" r="5" fill="#2c78bf"/>\n' % (lx + 35, ly + 57)
    )
    parts.append(
        '<text x="%d" y="%d" font-size="16">РК4, h = 0.05</text>\n'
        % (lx + 65, ly + 63)
    )
    parts.append(
        '<rect x="%d" y="%d" width="9" height="9" fill="#FF6B00"/>\n'
        % (lx + 31, ly + 87)
    )
    parts.append(
        '<text x="%d" y="%d" font-size="16">РК4, h = 0.1</text>\n'
        % (lx + 65, ly + 96)
    )

    # Заголовок и подписи осей.
    parts.append(
        '<text x="%d" y="32" text-anchor="middle" font-size="22" '
        'font-weight="bold">Решение задачи Коши, вариант 4</text>\n' % (width // 2)
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

    coarse_count = int(round((X1 - X0) / H_COARSE))
    fine_count = int(round((X1 - X0) / H_FINE))
    coarse = rk4(X0, Y0, H_COARSE, coarse_count)
    fine = rk4(X0, Y0, H_FINE, fine_count)

    svg_path = os.path.join(OUT_DIR, "fig_ode.svg")
    with open(svg_path, "w", encoding="utf-8") as stream:
        stream.write(make_svg(coarse, fine))
    print("  " + svg_path)

    pdf_path = os.path.join(OUT_DIR, "fig_ode.pdf")
    png_path = os.path.join(OUT_DIR, "fig_ode.png")
    if not render_figure(svg_path, pdf_path, png_path):
        print("  Конвертер SVG не найден: PDF/PNG не построены.")
        return

    print("  " + pdf_path)
    print("  " + png_path)
    print("  y(-1) = %.10f (точное %.10f)" % (fine[-1][1], exact(X1)))


if __name__ == "__main__":
    main()
