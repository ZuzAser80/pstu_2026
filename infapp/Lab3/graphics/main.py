#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""График к лабораторной работе №3 «Сплайн-интерполяция».

Вариант 4, таблица 4. Скрипт не требует сторонних библиотек Python:
он строит SVG-файл, а затем преобразует его в PDF и PNG для отчёта
доступным конвертером (rsvg-convert, Inkscape или LibreOffice).
ImageMagick используется только как последний резерв: его встроенный
рендерер SVG рисует линии серым и теряет цвет кривой.

Запуск:
    python3 graphics/main.py
"""

import math
import os
import shutil
import subprocess

# --- данные варианта 4 (таблица 4) -----------------------------------------
X_NODES = [0.180, 0.185, 0.190, 0.195, 0.200, 0.205]
Y_NODES = [5.61543, 5.46693, 5.32634, 5.19304, 5.06649, 4.94619]
TARGET_X = 0.1838

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "img")


def build_spline(x, y):
    """Естественный кубический сплайн: вторые производные и коэффициенты."""
    n = len(x)
    h = [x[i + 1] - x[i] for i in range(n - 1)]

    # Трёхдиагональная система для m_i, m_1 = m_n = 0.
    lower = [0.0] * n
    diagonal = [0.0] * n
    upper = [0.0] * n
    rhs = [0.0] * n

    diagonal[0] = 1.0
    for i in range(1, n - 1):
        lower[i] = h[i - 1]
        diagonal[i] = 2.0 * (h[i - 1] + h[i])
        upper[i] = h[i]
        rhs[i] = 6.0 * ((y[i + 1] - y[i]) / h[i] - (y[i] - y[i - 1]) / h[i - 1])
    diagonal[n - 1] = 1.0

    # Метод прогонки.
    p = [0.0] * n
    q = [0.0] * n
    p[0] = -upper[0] / diagonal[0]
    q[0] = rhs[0] / diagonal[0]
    for i in range(1, n):
        denominator = diagonal[i] + lower[i] * p[i - 1]
        p[i] = -upper[i] / denominator
        q[i] = (rhs[i] - lower[i] * q[i - 1]) / denominator

    m = [0.0] * n
    m[n - 1] = q[n - 1]
    for i in range(n - 2, -1, -1):
        m[i] = p[i] * m[i + 1] + q[i]

    a = list(y[:-1])
    b = []
    c = [m[i] / 2.0 for i in range(n - 1)]
    d = []
    for i in range(n - 1):
        b.append((y[i + 1] - y[i]) / h[i] - h[i] * (2.0 * m[i] + m[i + 1]) / 6.0)
        d.append((m[i + 1] - m[i]) / (6.0 * h[i]))

    return h, m, a, b, c, d


def evaluate(x, x_nodes, a, b, c, d):
    if x <= x_nodes[0]:
        i = 0
    elif x >= x_nodes[-1]:
        i = len(x_nodes) - 2
    else:
        i = 0
        while i < len(x_nodes) - 2 and x > x_nodes[i + 1]:
            i += 1
    dx = x - x_nodes[i]
    return a[i] + dx * (b[i] + dx * (c[i] + dx * d[i]))


def map_x(value, left, right, xmin, xmax):
    return left + (value - xmin) / (xmax - xmin) * (right - left)


def map_y(value, top, bottom, ymin, ymax):
    return bottom - (value - ymin) / (ymax - ymin) * (bottom - top)


def make_svg(h, m, a, b, c, d):
    width, height = 1000, 620
    left, right = 95, 45
    top, bottom = 55, 80
    xmin, xmax = 0.1780, 0.2070
    ymin, ymax = 4.90, 5.70

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
    y_ticks = [4.9, 5.0, 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7]
    for value in y_ticks:
        y = py(value)
        parts.append(
            '<line x1="%d" y1="%.1f" x2="%d" y2="%.1f" stroke="#d0d0d0" '
            'stroke-dasharray="4,4"/>\n' % (left, y, width - right, y)
        )
        parts.append(
            '<text x="%d" y="%.1f" text-anchor="end" font-size="15" '
            'fill="#555">%.1f</text>\n' % (left - 12, y + 5, value)
        )

    x_ticks = [0.180, 0.185, 0.190, 0.195, 0.200, 0.205]
    for value in x_ticks:
        x = px(value)
        parts.append(
            '<line x1="%.1f" y1="%d" x2="%.1f" y2="%d" stroke="#e4e4e4"/>\n'
            % (x, top, x, height - bottom)
        )
        parts.append(
            '<text x="%.1f" y="%d" text-anchor="middle" font-size="15" '
            'fill="#555">%.3f</text>\n' % (x, height - bottom + 26, value)
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

    # Кривая сплайна.
    sample_count = 500
    points = []
    for k in range(sample_count + 1):
        xv = xmin + (xmax - xmin) * k / sample_count
        yv = evaluate(xv, X_NODES, a, b, c, d)
        points.append("%.2f,%.2f" % (px(xv), py(yv)))
    parts.append(
        '<polyline points="%s" fill="none" stroke="#FF6B00" stroke-width="6" '
        'stroke-linecap="round" stroke-linejoin="round"/>\n'
        % " ".join(points)
    )

    # Табличные точки.
    for xv, yv in zip(X_NODES, Y_NODES):
        parts.append(
            '<circle cx="%.2f" cy="%.2f" r="7" fill="#c0392b" stroke="white" '
            'stroke-width="2"/>\n' % (px(xv), py(yv))
        )

    # Точка X = 0.1838.
    target_y = evaluate(TARGET_X, X_NODES, a, b, c, d)
    tx, ty = px(TARGET_X), py(target_y)
    parts.append(
        '<line x1="%.2f" y1="%.2f" x2="%.2f" y2="%d" stroke="#1e7b34" '
        'stroke-width="1.6" stroke-dasharray="6,4"/>\n'
        % (tx, ty, tx, height - bottom)
    )
    parts.append(
        '<line x1="%d" y1="%.2f" x2="%.2f" y2="%.2f" stroke="#1e7b34" '
        'stroke-width="1.6" stroke-dasharray="6,4"/>\n'
        % (left, ty, tx, ty)
    )
    parts.append(
        '<circle cx="%.2f" cy="%.2f" r="9" fill="#1e7b34" stroke="white" '
        'stroke-width="2.5"/>\n' % (tx, ty)
    )
    parts.append(
        '<text x="%.2f" y="%.2f" font-size="17" fill="#1e7b34" '
        'text-anchor="start">X = %.4f, S = %.6f</text>\n'
        % (tx + 14, ty - 14, TARGET_X, target_y)
    )

    # Легенда.
    lx, ly = width - right - 260, top + 15
    parts.append(
        '<rect x="%d" y="%d" width="245" height="82" rx="8" fill="white" '
        'fill-opacity="0.92" stroke="#bbbbbb"/>\n' % (lx, ly)
    )
    parts.append(
        '<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="#FF6B00" stroke-width="6" '
        'stroke-linecap="round"/>\n'
        % (lx + 15, ly + 24, lx + 55, ly + 24)
    )
    parts.append(
        '<text x="%d" y="%d" font-size="16">кубический сплайн</text>\n'
        % (lx + 65, ly + 30)
    )
    parts.append(
        '<circle cx="%d" cy="%d" r="6" fill="#c0392b"/>\n' % (lx + 35, ly + 58)
    )
    parts.append(
        '<text x="%d" y="%d" font-size="16">узлы таблицы</text>\n'
        % (lx + 65, ly + 64)
    )

    # Заголовок и подписи осей.
    parts.append(
        '<text x="%d" y="32" text-anchor="middle" font-size="22" '
        'font-weight="bold">Кубический сплайн, вариант 4</text>\n' % (width // 2)
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

    h, m, a, b, c, d = build_spline(X_NODES, Y_NODES)
    target_y = evaluate(TARGET_X, X_NODES, a, b, c, d)

    svg_path = os.path.join(OUT_DIR, "fig_spline.svg")
    with open(svg_path, "w", encoding="utf-8") as stream:
        stream.write(make_svg(h, m, a, b, c, d))
    print("  " + svg_path)

    pdf_path = os.path.join(OUT_DIR, "fig_spline.pdf")
    png_path = os.path.join(OUT_DIR, "fig_spline.png")
    if not render_figure(svg_path, pdf_path, png_path):
        print("  Конвертер SVG не найден: PDF/PNG не построены.")
        return

    print("  " + pdf_path)
    print("  " + png_path)
    print("  S(%.4f) = %.10f" % (TARGET_X, target_y))


if __name__ == "__main__":
    main()
