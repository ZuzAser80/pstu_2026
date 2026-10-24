#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Построение графиков к лабораторной работе №2
«Методы решения нелинейных уравнений».

Уравнение варианта 4: f(x) = x^3 - 0.1 x^2 + 0.4 x - 1.5 = 0.

Скрипт сохраняет векторные рисунки (PDF) в каталог img/ и используется
при сборке отчёта BastrakovReport.tex.

Запуск:
    graphics/.venv/bin/python graphics/main.py
"""
import os

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np

# --- уравнение варианта 4 ---------------------------------------------------

def f(x):
    return x ** 3 - 0.1 * x ** 2 + 0.4 * x - 1.5


def df(x):
    return 3 * x ** 2 - 0.2 * x + 0.4


def ddf(x):
    return 6 * x - 0.2


# Отрезок, на котором отделён единственный корень
A, B = 1.0, 1.1
ROOT = 1.05925320

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "img")

BLUE = "#1f4e9c"
RED = "#c0392b"
GREEN = "#1e7b34"
ORANGE = "#e67e22"


def save(fig, name):
    """Сохраняет рисунок в PDF и PNG рядом со скриптом."""
    os.makedirs(OUT_DIR, exist_ok=True)
    pdf = os.path.join(OUT_DIR, name + ".pdf")
    fig.savefig(pdf, bbox_inches="tight")
    fig.savefig(os.path.join(OUT_DIR, name + ".png"), dpi=150, bbox_inches="tight")
    plt.close(fig)
    print("  " + pdf)


def new_axes(xlim, ylim, title):
    fig, ax = plt.subplots(figsize=(7.2, 4.4))
    ax.set_xlim(*xlim)
    ax.set_ylim(*ylim)
    ax.axhline(0, color="black", lw=0.8)
    ax.axvline(0, color="black", lw=0.8)
    ax.grid(True, which="both", ls=":", color="#bbbbbb")
    ax.set_xlabel("x")
    ax.set_ylabel("y")
    ax.set_title(title)
    return fig, ax


# --- 1. График функции и отделение корня ------------------------------------

def fig_function():
    fig, ax = new_axes((-1.2, 2.2), (-6, 8),
                       "График функции $f(x)=x^3-0.1x^2+0.4x-1.5$")
    x = np.linspace(-1.2, 2.2, 600)
    ax.plot(x, f(x), color=BLUE, lw=2, label="$f(x)$")
    ax.plot([ROOT], [0], "o", color=RED, zorder=5)
    ax.annotate("корень $\\xi\\approx1.0593$", xy=(ROOT, 0), xytext=(ROOT + 0.15, -3.0),
                arrowprops=dict(arrowstyle="->", color=RED), color=RED)
    ax.legend(loc="upper left")
    save(fig, "fig_function")


def fig_separation():
    fig, ax = new_axes((0.9, 1.2), (-0.25, 0.2),
                       "Отделение корня на отрезке $[1;\\,1.1]$")
    x = np.linspace(0.9, 1.2, 400)
    ax.plot(x, f(x), color=BLUE, lw=2, label="$f(x)$")
    ax.axvspan(A, B, color=ORANGE, alpha=0.18)
    ax.plot([A, B], [f(A), f(B)], "o", color=RED, zorder=5)
    ax.annotate("$f(1)=-0.2<0$", xy=(A, f(A)), xytext=(A - 0.02, f(A) - 0.06),
                color=RED)
    ax.annotate("$f(1.1)=0.15>0$", xy=(B, f(B)), xytext=(B - 0.13, f(B) + 0.03),
                color=RED)
    ax.legend(loc="lower right")
    save(fig, "fig_separation")


# --- последовательности приближений -----------------------------------------

def bisection_seq(a=A, b=B, eps=1e-6):
    seq = []
    while b - a >= eps:
        c = 0.5 * (a + b)
        seq.append((a, b, c))
        if f(a) * f(c) <= 0:
            b = c
        else:
            a = c
    return seq


def chord_seq(eps=1e-6):
    # неподвижен конец b (f(b)*f''(b) > 0), x0 = a
    seq = [A]
    fixed = B
    x = A
    while True:
        fx = f(x)
        nxt = x - fx / (fx - f(fixed)) * (x - fixed)
        seq.append(nxt)
        if abs(nxt - x) < eps or abs(f(nxt)) < eps:
            break
        x = nxt
    return seq


def newton_seq(eps=1e-6):
    seq = [B]
    x = B
    while True:
        fx = f(x)
        nxt = x - fx / df(x)
        seq.append(nxt)
        if abs(nxt - x) < eps or abs(f(nxt)) < eps:
            break
        x = nxt
    return seq


def combined_seq(eps=1e-6):
    # по недостатку — хорды от a, по избытку — касательные от b
    low, high = A, B
    lowers, uppers = [low], [high]
    while high - low >= eps:
        fx, fy = f(low), f(high)
        low_next = low - fx / (fx - fy) * (low - high)
        high_next = high - fy / df(high)
        low, high = low_next, high_next
        lowers.append(low)
        uppers.append(high)
    return lowers, uppers


def iteration_seq(eps=1e-6):
    q0 = max(abs(df(A)), abs(df(B)))
    k = df(0.5 * (A + B))
    if abs(k) <= 0.5 * q0:
        k = np.sign(df(A)) * q0
    seq = [0.5 * (A + B)]
    x = 0.5 * (A + B)
    while True:
        nxt = x - f(x) / k
        seq.append(nxt)
        if abs(nxt - x) < eps:
            break
        x = nxt
    return seq, k


# --- 2. Метод половинного деления -------------------------------------------

def fig_bisection():
    fig, ax = new_axes((0.995, 1.105), (-0.25, 0.2),
                       "Метод половинного деления")
    x = np.linspace(0.995, 1.105, 400)
    ax.plot(x, f(x), color=BLUE, lw=2, label="$f(x)$")
    seq = bisection_seq()
    for i, (a, b, c) in enumerate(seq[:5]):
        y = -0.22 + i * 0.02
        ax.plot([a, b], [y, y], color=GREEN, lw=1.5)
        ax.plot([c], [y], "|", color=RED, ms=10)
        ax.plot([c, c], [y, f(c)], color=RED, lw=0.6, ls="--")
    ax.plot([ROOT], [0], "o", color=RED, zorder=5)
    ax.annotate("$c_1$", xy=(seq[0][2], 0), xytext=(seq[0][2], -0.19),
                ha="center", color=RED)
    ax.legend(loc="upper left")
    save(fig, "fig_bisection")


# --- 3. Метод хорд ----------------------------------------------------------

def fig_chord():
    fig, ax = new_axes((0.995, 1.105), (-0.25, 0.2), "Метод хорд")
    x = np.linspace(0.995, 1.105, 400)
    ax.plot(x, f(x), color=BLUE, lw=2, label="$f(x)$")
    seq = chord_seq()
    for xn, xn1 in zip(seq[:-2], seq[1:-1]):
        ax.plot([xn, B], [f(xn), f(B)], color=ORANGE, lw=1.3)
        ax.plot([xn1], [0], "o", color=RED, ms=4)
    ax.plot([ROOT], [0], "o", color=RED, zorder=5)
    ax.annotate("неподвижный конец $b$", xy=(B, f(B)), xytext=(B - 0.09, f(B) + 0.02),
                color=ORANGE)
    ax.legend(loc="upper left")
    save(fig, "fig_chord")


# --- 4. Метод Ньютона -------------------------------------------------------

def fig_newton():
    fig, ax = new_axes((0.995, 1.105), (-0.25, 0.2), "Метод Ньютона (касательных)")
    x = np.linspace(0.995, 1.105, 400)
    ax.plot(x, f(x), color=BLUE, lw=2, label="$f(x)$")
    seq = newton_seq()
    for xn, xn1 in zip(seq[:-1], seq[1:]):
        slope = df(xn)
        xs = np.array([xn1 - 0.005, xn + 0.005])
        ax.plot(xs, f(xn) + slope * (xs - xn), color=GREEN, lw=1.3)
        ax.plot([xn1], [0], "o", color=RED, ms=4)
        ax.plot([xn, xn], [0, f(xn)], color=GREEN, lw=0.6, ls="--")
    ax.plot([ROOT], [0], "o", color=RED, zorder=5)
    ax.legend(loc="upper left")
    save(fig, "fig_newton")


# --- 5. Комбинированный метод -----------------------------------------------

def fig_combined():
    fig, ax = new_axes((0.995, 1.105), (-0.25, 0.2),
                       "Комбинированный метод хорд и касательных")
    x = np.linspace(0.995, 1.105, 400)
    ax.plot(x, f(x), color=BLUE, lw=2, label="$f(x)$")
    lowers, uppers = combined_seq()
    ax.plot(lowers, f(np.array(lowers)), "o", color=ORANGE, ms=4,
            label="по недостатку (хорды)")
    ax.plot(uppers, f(np.array(uppers)), "s", color=GREEN, ms=4,
            label="по избытку (касательные)")
    for lo, hi in zip(lowers[:-1], uppers[:-1]):
        ax.plot([lo, hi], [f(lo), f(hi)], color=ORANGE, lw=1.0, ls="--")
    ax.plot([ROOT], [0], "o", color=RED, zorder=5)
    ax.legend(loc="upper left", fontsize=8)
    save(fig, "fig_combined")


# --- 6. Метод итераций (паутинная диаграмма) --------------------------------

def fig_iteration():
    seq, k = iteration_seq()
    lo, hi = 1.045, 1.075
    fig, ax = plt.subplots(figsize=(6.4, 6.0))
    x = np.linspace(lo, hi, 400)
    ax.plot(x, x - f(x) / k, color=BLUE, lw=2, label="$y=\\varphi(x)$")
    ax.plot(x, x, color="#888888", lw=1.2, ls="--", label="$y=x$")
    # паутинная диаграмма (лестница)
    px, py = seq[0], 0.0
    for xn, xn1 in zip(seq[:-1], seq[1:]):
        ax.plot([xn, xn], [xn - f(xn) / k, xn], color=RED, lw=0.8)
        ax.plot([xn, xn1], [xn1 - f(xn1) / k, xn1 - f(xn1) / k], color=RED, lw=0.8)
        px, py = xn1, xn
    ax.plot([ROOT], [ROOT], "o", color=GREEN, zorder=5)
    ax.set_xlim(lo, hi)
    ax.set_ylim(lo, hi)
    ax.grid(True, ls=":", color="#bbbbbb")
    ax.set_xlabel("$x_n$")
    ax.set_ylabel("$x_{n+1}$")
    ax.set_title("Метод итераций: $x_{n+1}=\\varphi(x_n)$")
    ax.legend(loc="upper left")
    save(fig, "fig_iteration")


def main():
    print("Построение графиков:")
    fig_function()
    fig_separation()
    fig_bisection()
    fig_chord()
    fig_newton()
    fig_combined()
    fig_iteration()
    print("Готово. Рисунки в", OUT_DIR)


if __name__ == "__main__":
    main()
