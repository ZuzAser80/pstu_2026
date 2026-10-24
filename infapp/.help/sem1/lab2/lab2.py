'''
Определение свойств отношений, заданных на множестве из 6 элементов в матричном виде
Будут выданы тестовые матрицы
Программа должна получить на вход тестовую матрицу размером 6х6 из нулей и единиц. На выходе должен выводиться перечень свойств, которыми обладает данное отношение
Можно выводить все свойства и писать, обладает оно ими или нет
Можно выводить только те свойства, которыми оно обладает
Свойства: рефлексивность, антирефлексивность, симметричность, ассиметричность, антисимметричность, транзитивность, связность

Если из комбинации получается отношение эквивалентности, выводить фразу "отношение эквивалентности". Аналогичным образом выводить отношение порядка.

Приносим аналогичным образом к следующей паре частично готовую работу, через одну пару - сделанную целиком

'''

import os

# загрузка матрицы из файла
def load_matrix_from_file(filename):
    matrix = []
    try:
        with open(filename, 'r', encoding='utf-8') as file:
            for line in file:
                # убираем пробелы в начале и конце и разбиваем по пробелам
                row = list(map(int, line.strip().split()))
                matrix.append(row)
        return matrix
    except Exception as e:
        print(f"Ошибка при загрузке файла {filename}: {e}")
        return None

#Получение списка txt файлов 
def get_txt_files():
    txt_files = []

    root = 'DM/lab2/tasks' # путь к папе с матрицами

    for file in os.listdir(root):
        if file.endswith('.txt') and file.startswith('m'):
            txt_files.append(file)
    return sorted(txt_files)  # сортировка

# отображение матрицы
def display_matrix(matrix):
    print("Матрица:")
    for row in matrix:
        print(' '.join(map(str, row)))
    print()

# функция проверки свойств 
def check_relation_properties(matrix):
    n = len(matrix)
    properties = {
        'рефлексивность (все диагональные элементы = 1)': True,
        'антирефлексивность (все диагональные элементы = 0)': True,
        'симметричность (матрица симметрична)': True,
        'асимметричность (если aRb, то не bRa для a≠b)': True,
        'антисимметричность (если aRb и bRa, то a=b)': True,
        'транзитивность (если aRb и bRc, то aRc)': True,
        'связность (для любых a≠b выполняется aRb или bRa)': True
    }
    
    # проверка рефлексивности и антирефлексивности
    for i in range(n):
        if matrix[i][i] != 1:
            properties['рефлексивность (все диагональные элементы = 1)'] = False
        if matrix[i][i] != 0:
            properties['антирефлексивность (все диагональные элементы = 0)'] = False
    
    # проверка симметричности
    for i in range(n):
        for j in range(n):
            if matrix[i][j] != matrix[j][i]:
                properties['симметричность (матрица симметрична)'] = False
                break
        if not properties['симметричность (матрица симметрична)']:
            break
    
    # проверка асимметричности
    asymmetric = True
    if not properties['антирефлексивность (все диагональные элементы = 0)']:
        asymmetric = False
    else:
        for i in range(n):
            for j in range(n):
                if matrix[i][j] == 1 and matrix[j][i] == 1:
                    asymmetric = False
                    break
            if not asymmetric:
                break
    properties['асимметричность (если aRb, то не bRa для a≠b)'] = asymmetric
    
    # проверка антисимметричности
    antisymmetric = True
    for i in range(n):
        for j in range(n):
            if i != j and matrix[i][j] == 1 and matrix[j][i] == 1:
                antisymmetric = False
                break
        if not antisymmetric:
            break
    properties['антисимметричность (если aRb и bRa, то a=b)'] = antisymmetric
    
    # проверка транзитивности
    transitive = True
    for i in range(n):
        for j in range(n):
            if matrix[i][j] == 1:
                for k in range(n):
                    if matrix[j][k] == 1 and matrix[i][k] != 1:
                        transitive = False
                        break
                if not transitive:
                    break
        if not transitive:
            break
    properties['транзитивность (если aRb и bRc, то aRc)'] = transitive
    
    # проверка связности
    connected = True
    for i in range(n):
        for j in range(n):
            if i != j and matrix[i][j] == 0 and matrix[j][i] == 0:
                connected = False
                break
        if not connected:
            break
    properties['связность (для любых a≠b выполняется aRb или bRa)'] = connected
    
    return properties

# проверка эквивалентности
def is_equivalence(properties, matrix):
    return (properties['рефлексивность (все диагональные элементы = 1)'] and 
            properties['симметричность (матрица симметрична)'] and 
            properties['транзитивность (если aRb и bRc, то aRc)'])

# проверка отношения порядка
def is_order_relation(properties, matrix):
    return (properties['рефлексивность (все диагональные элементы = 1)'] and 
            properties['антисимметричность (если aRb и bRa, то a=b)'] and 
            properties['транзитивность (если aRb и bRc, то aRc)'])

# проверка матрицы и вывод результатов
def analyze_matrix(matrix, filename):
    print("\n" + "="*50)
    print(f"Анализ отношения из файла: {filename}")
    print("="*50)
    
    # проверка размер матрицы
    if len(matrix) != 6 or any(len(row) != 6 for row in matrix):
        print("Ошибка: матрица должна быть размером 6x6")
        return

    # вывод матрицы    
    display_matrix(matrix)
    
    # проверка свойств свойства
    properties = check_relation_properties(matrix)
    
    # вывод свойств
    print("Свойства отношения:")
    found_properties = False
    for prop, value in properties.items():
        if value:
            print(f"✓ {prop}")
            found_properties = True
    
    if not found_properties:
        print("Отношение не обладает ни одним из проверяемых свойств")
    
    print("\nСвойства, которыми отношение НЕ обладает:")
    not_found_properties = False
    for prop, value in properties.items():
        if not value:
            print(f"✗ {prop}")
            not_found_properties = True
    
    # проверка типов отношений
    print("\n" + "-"*50)
    if is_equivalence(properties, matrix):
        print("✓ ОТНОШЕНИЕ ЭКВИВАЛЕНТНОСТИ")
        print("  (рефлексивное, симметричное, транзитивное)")
    
    if is_order_relation(properties, matrix):
        print("✓ ОТНОШЕНИЕ ПОРЯДКА")
        print("  (рефлексивное, антисимметричное, транзитивное)")
    
    if not is_equivalence(properties, matrix) and not is_order_relation(properties, matrix):
        print("Отношение не является ни отношением эквивалентности, ни отношением порядка")

# отображение интерфейса
def main_menu():
    while True:
        print("\n" + "="*50)
        print("АНАЛИЗ СВОЙСТВ ОТНОШЕНИЙ")
        print("="*50)
        print("1. Выбрать матрицу из файла")
        print("2. Выйти из программы")
        
        choice = input("\nВыберите действие (1-2): ").strip()
        
        if choice == '1':
            select_matrix_menu()
        elif choice == '2':
            print("Выход из программы...")
            break
        else:
            print("Неверный выбор. Попробуйте снова.")

# обработка нажатий
def select_matrix_menu():
    txt_files = get_txt_files()
    
    if not txt_files:
        print("\nВ текущей папке не найдено файлов m1.txt, m2.txt, ...")
        print("Убедитесь, что файлы находятся в той же папке, что и программа")
        return
    
    print("\nДоступные матрицы:")
    for i, filename in enumerate(txt_files, 1):
        print(f"{i}. {filename}")

    print(f"{len(txt_files) + 1}. Вернуться в главное меню")
    
    try:
        choice = int(input(f"\nВыберите матрицу (1-{len(txt_files) + 1}): "))
        
        if 1 <= choice <= len(txt_files):
            filename = f'DM/lab2/tasks/{txt_files[choice - 1]}'
            matrix = load_matrix_from_file(filename)
            
            if matrix is not None:
                analyze_matrix(matrix, filename)
            else:
                print("Ошибка при загрузке матрицы")
        
        elif choice == len(txt_files) + 1:
            return  # Возврат в главное меню
        
        else:
            print("Неверный выбор")
    
    except ValueError:
        print("Пожалуйста, введите число")


main_menu() 

'''
def manual_input():
    print("Введите матрицу 6x6 (построчно, элементы через пробел):")
    matrix = []
    
    for i in range(6):
        while True:
            try:
                row = list(map(int, input(f"Строка {i+1}: ").split()))
                if len(row) != 6:
                    print("Ошибка: нужно ввести 6 чисел")
                    continue
                if any(x not in [0, 1] for x in row):
                    print("Ошибка: матрица должна содержать только 0 и 1")
                    continue
                matrix.append(row)
                break
            except ValueError:
                print("Ошибка: введите целые числа 0 или 1")
    
    return matrix'''
