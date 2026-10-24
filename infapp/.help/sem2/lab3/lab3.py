"""
Алгоритм Шимбелла для поиска путей минимальной или максимальной длины
с заданным числом переходов.
Вход: весовая матрица 6x6 (0 - отсутствие пути)
Выход: матрица 6x6 с весами наилучших путей длины k.
"""

import math

def read_matrix_from_file(filename):
    """Чтение матрицы 6x6 из файла."""
    try:
        with open(filename, 'r') as f:
            matrix = []
            for line_num, line in enumerate(f, 1):
                line = line.strip()
                if not line:
                    continue
                parts = line.split()
                if len(parts) != 6:
                    raise ValueError(f"Строка {line_num}: ожидается 6 чисел, получено {len(parts)}")
                row = [int(x) for x in parts]
                if any(x < 0 for x in row):
                    raise ValueError(f"Строка {line_num}: отрицательные числа не допускаются")
                matrix.append(row)
            if len(matrix) != 6:
                raise ValueError(f"Должно быть 6 строк, получено {len(matrix)}")
            return matrix
    except Exception as e:
        print(f"Ошибка при чтении файла: {e}")
        return None

def print_matrix(matrix, title="Матрица"):
    """Вывод матрицы."""
    print(f"\n{title}:")
    print("-" * 40)
    for row in matrix:
        print(" ".join(f"{x:5}" if x != float('inf') and x != -float('inf') else f"{'inf':5}" for x in row))
    print("-" * 40)

def shimbell_multiply(A, B, mode='min', inf_val=float('inf')):
    """
    Умножение матриц по операциям Шимбелла.
    A, B - матрицы (списки списков) размера n x n.
    mode: 'min' для min-plus, 'max' для max-plus.
    inf_val: значение, обозначающее отсутствие пути.
    Возвращает новую матрицу C, где C[i][j] = (по mode) над t (A[i][t] + B[t][j]).
    """
    n = len(A)
    C = [[inf_val] * n for _ in range(n)]
    if mode == 'min':
        for i in range(n):
            for j in range(n):
                best = inf_val
                for t in range(n):
                    if A[i][t] != inf_val and B[t][j] != inf_val:
                        s = A[i][t] + B[t][j]
                        if s < best:
                            best = s
                if best != inf_val:
                    C[i][j] = best
    elif mode == 'max':
        neg_inf = -float('inf')
        for i in range(n):
            for j in range(n):
                best = neg_inf
                for t in range(n):
                    if A[i][t] != neg_inf and B[t][j] != neg_inf:
                        s = A[i][t] + B[t][j]
                        if s > best:
                            best = s
                if best != neg_inf:
                    C[i][j] = best
    else:
        raise ValueError("mode должен быть 'min' или 'max'")
    return C

def matrix_power(matrix, power, mode='min'):
    """
    Возведение матрицы в степень power по операциям Шимбелла.
    matrix - исходная весовая матрица (0 означает отсутствие ребра).
    power - целое неотрицательное число (количество переходов).
    Возвращает матрицу путей длины power.
    """
    n = len(matrix)
    # Преобразуем 0 в специальное значение для отсутствия ребра
    if mode == 'min':
        inf_val = float('inf')
        # для минимума: отсутствие пути - бесконечность
        mat = [[inf_val] * n for _ in range(n)]
        for i in range(n):
            for j in range(n):
                if matrix[i][j] != 0:
                    mat[i][j] = matrix[i][j]
    else:  # max
        inf_val = -float('inf')
        mat = [[inf_val] * n for _ in range(n)]
        for i in range(n):
            for j in range(n):
                if matrix[i][j] != 0:
                    mat[i][j] = matrix[i][j]

    if power == 0:
        # Единичная матрица: пути длины 0 (только из вершины в себя)
        result = [[inf_val] * n for _ in range(n)]
        for i in range(n):
            if mode == 'min':
                result[i][i] = 0   # для min-пути длина 0
            else:
                result[i][i] = 0   # для max-пути тоже 0 (путь нулевой длины)
        return result

    # Начальная степень 1
    result = mat
    for _ in range(1, power):
        result = shimbell_multiply(result, mat, mode, inf_val)
    return result

def main():
    print("=" * 60)
    print("ПОИСК ПУТЕЙ МИНИМАЛЬНОЙ/МАКСИМАЛЬНОЙ ДЛИНЫ (АЛГОРИТМ ШИМБЕЛЛА)")
    print("=" * 60)

    # Ввод матрицы
    while True:
        print("\nВыберите способ ввода матрицы 6x6:")
        print("1. Ввести с клавиатуры")
        print("2. Загрузить из файла")
        print("3. Использовать встроенный пример")
        choice = input("Ваш выбор (1/2/3): ").strip()
        if choice == '1':
            print("Введите 6 строк по 6 целых неотрицательных чисел, разделённых пробелами:")
            matrix = []
            for i in range(6):
                while True:
                    line = input(f"Строка {i+1}: ").strip()
                    parts = line.split()
                    if len(parts) != 6:
                        print("Ошибка: должно быть 6 чисел. Попробуйте снова.")
                        continue
                    try:
                        row = [int(x) for x in parts]
                        if any(x < 0 for x in row):
                            print("Ошибка: числа должны быть неотрицательными.")
                            continue
                        matrix.append(row)
                        break
                    except ValueError:
                        print("Ошибка: введите целые числа.")
            break
        elif choice == '2':
            filename = input("Введите имя файла: ").strip()
            matrix = read_matrix_from_file(filename)
            if matrix is not None:
                break
        elif choice == '3':
            # Пример матрицы 6x6
            matrix = [
                [0, 2, 0, 1, 0, 0],
                [2, 0, 3, 0, 0, 0],
                [0, 3, 0, 0, 0, 4],
                [1, 0, 0, 0, 5, 0],
                [0, 0, 0, 5, 0, 6],
                [0, 0, 4, 0, 6, 0]
            ]
            print("Используется встроенная матрица:")
            print_matrix(matrix, "Исходная матрица")
            break
        else:
            print("Неверный выбор. Попробуйте снова.")

    # Вывод исходной матрицы
    print_matrix(matrix, "Исходная матрица весов (0 - нет ребра)")

    # Выбор направления
    while True:
        direction = input("\nНаправление поиска (min - минимум, max - максимум): ").strip().lower()
        if direction in ['min', 'max']:
            break
        else:
            print("Неверный ввод. Введите 'min' или 'max'.")

    # Ввод количества переходов
    while True:
        try:
            k = int(input("Введите количество переходов (положительное целое): "))
            if k <= 0:
                print("Число должно быть положительным.")
                continue
            break
        except ValueError:
            print("Введите целое число.")

    # Построение матрицы путей длины k
    result = matrix_power(matrix, k, direction)

    # Преобразование бесконечностей в 0 для вывода
    output_matrix = [[0] * 6 for _ in range(6)]
    for i in range(6):
        for j in range(6):
            val = result[i][j]
            if direction == 'min':
                if val != float('inf'):
                    output_matrix[i][j] = val
            else:
                if val != -float('inf'):
                    output_matrix[i][j] = val

    # Вывод результата
    print_matrix(output_matrix, f"Матрица {'минимальных' if direction == 'min' else 'максимальных'} путей длины {k}")

    print("\n" + "=" * 60)
    print("готово")
    print("=" * 60)

if __name__ == "__main__":
    main()
