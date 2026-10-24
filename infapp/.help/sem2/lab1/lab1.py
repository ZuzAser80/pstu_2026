#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Задаётся матрица смежности A размера N×N с элементами 0 и 1:
  A[i][j] = 1  — есть ребро (дуга) из вершины i в вершину j,
  A[i][j] = 0  — ребра нет.
Вершины нумеруются индексами 0..N-1 в коде и 1..N в выводе пользователю.

Дальше по условию лабораторной:
  • матрица при необходимости симметризуется (ориентированный граф заменяется
    неориентированным «объединением» парных дуг);
  • на неориентированном графе ищутся компоненты связности (обход графа);
  • строится матрица достижимости (транзитивное замыкание отношения смежности).
"""

import os

# Число вершин графа (размер матрицы смежности).
N = 10

MATRICES_SUBDIR = "test"


def matrix_filenames(matrices_dir: str) -> list[str]:
    """
    Служебная функция: собирает имена .txt файлов в папке с тестовыми матрицами.

    К дискретной математике не относится — только удобство выбора файла по имени.
    """
    if not os.path.isdir(matrices_dir):
        return []
    return sorted(f for f in os.listdir(matrices_dir) if f.lower().endswith(".txt"))


def path_for_matrix_name(matrices_dir: str, name: str) -> str:
    """
    Собирает полный путь к файлу матрицы только из имени файла.

    Берётся os.path.basename(name), чтобы пользователь не мог ввести путь вида
    «../другая_папка/файл» — чтение только из фиксированной папки matrices_dir.
    """
    safe = os.path.basename(name.strip())
    return os.path.join(matrices_dir, safe)


def print_matrix(matrix: list[list[int]], title: str | None = None) -> None:
    """Выводит матрицу построчно (элементы через пробел), опционально с заголовком."""
    if title:
        print(title)
    for row in matrix:
        print(" ".join(str(x) for x in row))
    print()


def parse_tokens_to_matrix(tokens: list[str]) -> list[list[int]]:
    """
    Превращает ровно N×N токенов «0» или «1» в список из N строк матрицы.

    Матрица смежности кодирует граф: строка i — исходящие рёбра из вершины i.
    Диагональ обычно 0 (петель нет), но условие задаёт только 0/1 — петли
    допускаются, если так записано во входе.
    """
    if len(tokens) != N * N:
        raise ValueError(
            f"Ожидается {N * N} чисел (0 или 1), получено {len(tokens)}."
        )
    nums = []
    for t in tokens:
        if t not in ("0", "1"):
            raise ValueError(f"Недопустимый символ в матрице: {t!r} (нужны 0 и 1).")
        nums.append(int(t))
    # Разбиваем линейный список из N*N чисел на N строк по N элементов.
    return [nums[i * N : (i + 1) * N] for i in range(N)]


def read_matrix_from_file(path: str) -> list[list[int]]:
    """Читает весь файл, режет по пробельным символам, парсит как матрицу N×N."""
    path = os.path.expanduser(path)
    if not os.path.isfile(path):
        raise FileNotFoundError(f"Файл не найден: {path}")
    with open(path, "r", encoding="utf-8") as f:
        text = f.read()
    tokens = text.split()
    return parse_tokens_to_matrix(tokens)


def read_matrix_manual() -> list[list[int]]:
    """
    Пошаговый ввод матрицы с клавиатуры: N строк, в каждой ровно N чисел 0/1.

    Каждая строка ввода проверяется через parse_tokens_to_matrix отдельно —
    так проще сообщать об ошибке только для текущей строки.
    """
    print(f"Введите матрицу {N}×{N} построчно: в каждой строке {N} чисел 0 или 1 через пробел.")
    rows: list[list[int]] = []
    for i in range(N):
        while True:
            line = input(f"Строка {i + 1}/{N}: ").strip()
            parts = line.split()
            try:
                row = parse_tokens_to_matrix(parts)[0]
            except ValueError as e:
                print(e, "Повторите ввод.")
                continue
            rows.append(row)
            break
    return rows


def is_symmetric(matrix: list[list[int]]) -> bool:
    """
    Проверка симметрии матрицы смежности относительно главной диагонали.

    В теории графов: если A[i][j] = A[j][i] для всех i, j, то матрица смежности
    задаёт неориентированный граф (ребро {i,j} кодируется одинаково в обе стороны).

    Достаточно сравнить пары (i, j) при j > i — остальное следует из уже
    проверенных пар и диагонали (на диагонали i=j сравнение с самой собой тривиально).
    """
    for i in range(N):
        for j in range(i + 1, N):
            if matrix[i][j] != matrix[j][i]:
                return False
    return True


def symmetrize(matrix: list[list[int]]) -> list[list[int]]:
    """
    Переход от возможного ориентированного графа к неориентированному по матрице смежности.

    Дискретная математика:
      Если дуга есть хотя бы в одном направлении (i→j или j→i), в неориентированном
      графе считаем, что есть неориентированное ребро между i и j.

    Формула для новой матрицы A':
      A'[i][j] = A'[j][i] = 1, если A[i][j] = 1 или A[j][i] = 1, иначе 0.
    Это то же самое, что взять покомпонентное «ИЛИ» для пары (A[i][j], A[j][i])
    и записать результат симметрично — получается симметричная матрица смежности.

    Результат всегда симметричен, по нему удобно считать компоненты связности
    как в неориентированном графе.
    """
    m = [[0] * N for _ in range(N)]
    for i in range(N):
        for j in range(N):
            # Ребро неориентированного графа: было ли хоть одно направление дуги.
            v = 1 if (matrix[i][j] or matrix[j][i]) else 0
            m[i][j] = v
            m[j][i] = v
    return m


def connected_components(adj: list[list[int]]) -> list[list[int]]:
    """
    Компоненты связности неориентированного графа, заданного симметричной матрицей adj.

    Дискретная математика:
      Компонента связности — максимальное по включению множество вершин, в котором
      любые две вершины соединены цепью рёбер (в неориентированном графе).

    Алгоритм: обход в глубину (DFS) из каждой ещё не посещённой вершины.
      • Внешний цикл for start перебирает стартовые вершины — так мы находим все
        компоненты: каждая новая «незакрашенная» вершина начинает новую компоненту.
      • Стек реализует DFS: из текущей вершины уходим в первого непосещённого соседа
        по матрице (adj[u][v] == 1).

    Возвращается список компонент; вершины внутри компоненты сортируются по номеру
    для аккуратного вывода.
    """
    visited = [False] * N
    components: list[list[int]] = []

    for start in range(N):
        if visited[start]:
            continue
        stack = [start]
        visited[start] = True
        comp: list[int] = []
        while stack:
            u = stack.pop()
            comp.append(u)
            # Соседи в неориентированном графе: все v с ребром (u,v).
            for v in range(N):
                if adj[u][v] and not visited[v]:
                    visited[v] = True
                    stack.append(v)
        components.append(sorted(comp))

    return components


def reachability_matrix(adj: list[list[int]]) -> list[list[int]]:
    """
    Матрица достижимости R (транзитивное замыкание бинарного отношения «есть ребро»).

    Дискретная математика:
      R[i][j] = 1, если существует путь из i в j (длина пути ≥ 0).
      Путь нулевой длины: вершина достижима сама из себя — поэтому на диагонали
      всегда 1 (рефлексивное замыкание).

      Для первого шага берём не только рёбра графа, но и «стояние на месте»:
      начальная матрица: R₀[i][j] = 1, если i = j или adj[i][j] = 1.

    Дальше — алгоритм Уоршелла (булевый вариант алгоритма Флойда — Уоршелла):
      для каждой k от 0 до N-1 разрешаем в качестве промежуточных вершин только
      номера ≤ k. Инвариант после шага k:
        R[i][j] = 1  ⇔  существует путь из i в j, все внутренние вершины которого
        имеют номера не больше k.

      Рекуррентное соотношение (логическое ИЛИ):
        R_new[i][j] = R[i][j] ∨ (R[i][k] ∧ R[k][j])
      То есть путь либо уже был без k, либо идёт через k: i → … → k → … → j.

    После N итераций учтены все вершины как промежуточные — получена полная
    достижимость. Для неориентированного связного куска матрица на этом куске
    будет «все единицы»: из любой вершины достижима любая другая.
    """
    r = [[0] * N for _ in range(N)]
    # Рефлексивно-замкнутый старт: петли и ребра длины 1.
    for i in range(N):
        for j in range(N):
            r[i][j] = 1 if (i == j or adj[i][j]) else 0

    # k — максимальный номер промежуточной вершины на текущем шаге (Уоршелл).
    for k in range(N):
        for i in range(N):
            for j in range(N):
                if r[i][k] and r[k][j]:
                    r[i][j] = 1

    return r


def main() -> None:
    """Интерактивный цикл: выбор ввода → расчёты → вывод → снова меню."""
    base_dir = os.path.dirname(os.path.abspath(__file__))
    matrices_dir = os.path.join(base_dir, MATRICES_SUBDIR)
    default_name = "g11.txt"

    print("Матрица смежности графа (", N, "×", N, ")", sep="")

    while True:
        # --- Меню источника данных ---
        print()
        print("Источник данных:")
        print("  1 — файл")
        print("  2 — ручной ввод с клавиатуры")
        print("  0 — выход")
        choice = input("Выбор (0/1/2) [1]: ").strip() or "1"

        if choice == "0":
            print("Выход.")
            break
        if choice not in ("1", "2"):
            print("Неверный выбор. Повторите.")
            continue

        try:
            if choice == "1":
                if not os.path.isdir(matrices_dir):
                    print(f"Ошибка: папка с матрицами не найдена: {matrices_dir}")
                    continue
                names = matrix_filenames(matrices_dir)
                print(f"Папка с матрицами: {matrices_dir}")
                if names:
                    print("Файлы: " + ", ".join(names))
                else:
                    print("В папке нет .txt файлов.")
                prompt = f"Имя файла [{default_name}]: "
                name = input(prompt).strip() or default_name
                path = path_for_matrix_name(matrices_dir, name)
                matrix = read_matrix_from_file(path)
                print(f"\nПрочитано: {os.path.basename(path)}")
            else:
                matrix = read_matrix_manual()
        except (ValueError, FileNotFoundError) as e:
            print(f"Ошибка: {e}")
            continue

        # --- Шаг 1: исходная матрица и вывод о типе графа (ориент. / неориент.) ---
        print("\n--- Исходная матрица смежности ---")
        print_matrix(matrix)

        if is_symmetric(matrix):
            print(
                "Матрица смежности симметрична относительно главной диагонали.\n"
                "Следовательно, граф является неориентированным."
            )
        else:
            print(
                "Матрица смежности не симметрична относительно главной диагонали.\n"
                "Следовательно, граф ориентированный; для компонент связности и "
                "матрицы достижимости он приводится к неориентированному (симметризация)."
            )
        print()

        # --- Шаг 2: симметризация; дальше все теоретические шаги — на неориентированном графе ---
        sym = symmetrize(matrix)
        print("--- После приведения к неориентированному графу (симметризация) ---")
        if matrix == sym:
            print("(Матрица уже была симметричной — изменений нет.)\n")
        print_matrix(sym)

        # --- Шаг 3: компоненты связности по симметризованной матрице ---
        comps = connected_components(sym)
        print("--- Компоненты связности ---")
        print(f"Количество компонент связности: {len(comps)}")
        for idx, comp in enumerate(comps, start=1):
            verts = ", ".join(str(v + 1) for v in comp)
            print(f"  Компонента {idx}: вершины {{{verts}}} (нумерация с 1)")

        # --- Шаг 4: матрица достижимости для того же неориентированного графа ---
        reach = reachability_matrix(sym)
        print("--- Матрица достижимости (после симметризации) ---")
        print_matrix(reach)


if __name__ == "__main__":
    main()
