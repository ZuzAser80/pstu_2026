"""
U = [-30, 30]

Реализовать:
1. Задание и вывод на экран минимум трёх множеств
    1.1. Проверка корректности количества элементов, отсутствия повторяющихся элементов

2. Возможность исправления ошибок без перезапуска программы 

3. Сохранение заданных множеств после выполнения над ними операций 

4. Осуществление над множествами операций объединения, пересечения, разности, симметричной разности, дополнения
    4.1. Нормальное обозначение пустого множества 

5. Возможность добавления впоследствии функции задания и расчёта формул

Отчёт по лабе:
1. Постановка задачи

2. Краткое описание решения (структуры данных, методы итд)

3. Результаты тестирования, отражающие весь функционал

4. Либо ссылка на гитхаб, либо приложение основной рукописной части кода 

rustamkhanova@ya.ru
"""

# -------------------------------------------------------------------------------------

# библиотеки
import random
import os
import sys

# 5 операций
'''
+ объединение
* пересечение
- разность
^ симметрическая разность
! дополнение

'''

# функция расчета уравнения
def equation(sets):
    if not sets:
        print("Ошибка! Множества еще не созданы!")
        return
    
    # словарь для быстрого доступа к множествам по ключу
    set_dict = {
        'A': sets[0], 
        'B': sets[1], 
        'C': sets[2]
    }
    
    print("\nДоступные множества:")
    print(f"A = {sets[0]}")
    print(f"B = {sets[1]}")
    print(f"C = {sets[2]}")
    
    print("\nОперации:")
    print("+ - объединение")
    print("* - пересечение")
    print("- - разность")
    print("^ - симметрическая разность")
    print("! - дополнение")
    print("\nПримеры: A+B, A*B, A-B, A^B, !A, (A+B)*C, !(A+B)")
    
    eq = input("\nВведите выражение: ").replace(" ", "")
    
    # проход по выражению и выделение всех операций
    def parse_expression(expr):
        if not expr:
            return set()
        
        # обработка дополнения
        if expr.startswith('!'):
            operand = parse_expression(expr[1:])
            result, _, _ = addition(operand)
            return result
        
        # обработка выражений в скобках
        if expr.startswith('('):
            balance = 1
            for i in range(1, len(expr)):
                if expr[i] == '(':
                    balance += 1
                elif expr[i] == ')':
                    balance -= 1
                    if balance == 0:
                        if i == len(expr) - 1:
                            # все выражение в скобках
                            return parse_expression(expr[1:-1])
                        else:
                            left = parse_expression(expr[1:i])
                            op = expr[i+1]
                            right = parse_expression(expr[i+2:])
                            return perform_operation(left, right, op)
        
        for i in range(len(expr)-1, -1, -1):
            if expr[i] in ['+', '*', '-', '^']:
                balance = 0
                for j in range(i):
                    if expr[j] == '(':
                        balance += 1
                    elif expr[j] == ')':
                        balance -= 1

                # поиск операций
                if balance == 0:
                    left = parse_expression(expr[:i])
                    right = parse_expression(expr[i+1:])
                    return perform_operation(left, right, expr[i])
        
        if expr in set_dict:
            return set_dict[expr]
        else:
            print(f"Ошибка: неизвестный идентификатор '{expr}'")
            return set()

    # функция выполнения операции над двумя множествами
    def perform_operation(left, right, op):
        if op == '+':
            return unificate(left, right)
        elif op == '*':
            return intersection(left, right)
        elif op == '-':
            return difference(left, right)
        elif op == '^':
            return sym_difference(left, right)
        else:
            return set()
    
    try:
        result = parse_expression(eq)
        if not result:
            print(f"\nРезультат: ∅")
        else:
            print(f"\nРезультат: {sorted(result)}")
    except Exception as e:
        print(f"Ошибка при вычислении выражения: {e}")

'''
блок базовых операций над множествами
'''
# объединение 2 множеств
def unificate(set1, set2):
    result = set()
    for a in set1:
        result.add(a)
    
    for c in set2:
        result.add(c)
    
    return sorted(result)

# пересечение 2 множеств
def intersection(set1, set2):
    result = set()

    for a in set1:
        for b in set2:
            if a == b:
                result.add(a)

    return sorted(result)

# разность 2 множеств
def difference(set1, set2):
    result = set()

    for i in set1:
        if i not in set2:
            result.add(i)
    
    return sorted(result)

# симметрическая разность 2 множеств
def sym_difference(set1, set2):
    result = set()
    for i in set1:
        if i not in set2:
            result.add(i)
    for j in set2:
        if j not in set1:
            result.add(j)
    
    return sorted(result)

# дополнение 1 множества до универсума
def addition(set1):
    low, high = -30, 30

    universum = set()
    for i in range(low, high + 1):
        universum.add(i)

    result = set()
    for num in universum:
        if num not in set1:
            result.add(num)

    return sorted(result), low, high

# функция изменения введенных множеств
def change_sets(sets):
    if sets == None:
        print("Ошибка! Множества еще не созданы!")
        return None
    
    print("\n--- ИЗМЕНЕНИЕ СУЩЕСТВУЮЩИХ МНОЖЕСТВ ---")
    print_sets(sets, [0, 1, 2])
    a = input("Введите порядковый номер множества для изменения (1-3): ")

    print(f"\n--- МНОЖЕСТВО {a} ---")
    print("Выберите способ создания:")
    print("1 - Ручной ввод")
    print("2 - Случайная генерация")
    print("3 - Генерация с параметрами")
            
    choice = input("Выберите способ (1-3): ")
            
    mode_map = {
        "1": "manual",
        "2": "random",
        "3": "param"
        }
    
    if choice in mode_map:
        mode = mode_map[choice]
        sets[int(a)-1] = create_set(int(a)-1, mode)
    else:
        print("Неверный выбор! Попробуйте снова.")

# отладочная функция для вывода выбранных множеств
def print_sets(sets, indexes):
    if sets:
        print("\nЗадействованные множества:")
        for i in indexes:
            print(f"Множество {i+1} ({'ABC'[i]}): {sets[i]}")
        return
    else:
        print("Множества еще не созданы!")

# очистка консоли при возврате в меню
def clear_console():
    os.system('clear')

# создание 1 множества
def create_set(set_number, mode):
    if mode == "manual":
        print(f"\n--- РУЧНОЙ ВВОД МНОЖЕСТВА {set_number+1} ({'ABC'[set_number]}) ---")
        a = []
        k = int(input(f"Введите длину множества {set_number+1}: "))
        for j in range(k):
            element = int(input(f"Введите {j+1}й элемент множества {set_number+1}: "))
            a.append(element)
        result = sorted(set(a))
        print(f"Множество {set_number+1} ({'ABC'[set_number]}) создано: {result}")
        return result
        
    elif mode == "random":
        print(f"\n--- СЛУЧАЙНАЯ ГЕНЕРАЦИЯ МНОЖЕСТВА {set_number+1} ({'ABC'[set_number]}) ---")
        a = []
        rand_len = random.randint(2, 10)
        print(f"Длина множества {set_number+1}: {rand_len}")
        for l in range(rand_len):
            a.append(random.randint(-30, 30))
        result = sorted(set(a))
        print(f"Множество {set_number+1} ({'ABC'[set_number]}): {result}")
        return result
        
    elif mode == "param":
        print(f"\n--- СОЗДАНИЕ МНОЖЕСТВА {set_number+1} ({'ABC'[set_number]}) С ПАРАМЕТРАМИ ---")

        '''
        - по знаку 
        - четность
        - кратность n
        '''
        a = []

        # ввод параметров
        math_sign = input("Введите параметр знака множества (только + или - ): ")
        parity = int(input("Введите параметр чётности (1 - только нечётные, 2 - только чётные): "))
        multiplicity = int(input("Введите значение числа, кратному которым будет множество: (0 - пропуск параметра): "))

        length = int(input(f"Введите длину множества {set_number+1} (из-за наличия параметров множество может оказаться меньше необходимой длины): "))

        for i in range(-30, 31):
            # проверка знака
            if math_sign == "+" and i <= 0: continue
            if math_sign == "-" and i >= 0: continue
            # проверка чётности
                # нечётные не должны быть чётными
            if parity == 1 and i % 2 == 0: continue
                # чётные не должны быть нечётными
            if parity == 2 and i % 2 != 0: continue
            # проверка кратности + если multiplicity != 0
            if multiplicity != 0 and i % multiplicity != 0: continue
            
            a.append(i)
            if len(a) >= length:
                break
        result = sorted(set(a))
        print(f"Множество {set_number+1} ({'ABC'[set_number]}): {result}")
        return result

# создание всех множеств
def create_all_sets():
    all_tuples = []
    for i in range(3):
        while True:
            print(f"\n--- МНОЖЕСТВО {i+1} ({'ABC'[i]}) ---")
            print("Выберите способ создания:")
            print("1 - Ручной ввод")
            print("2 - Случайная генерация")
            print("3 - Генерация с параметрами")
            
            choice = input("Выберите способ (1-3): ")
            if choice not in ['1', '2', '3']:
                print("Ошибка выбора!")
                continue

            mode_map = {
                "1": "manual",
                "2": "random",
                "3": "param"
            }
            
            if choice in mode_map:
                mode = mode_map[choice]
                current_set = create_set(i, mode)
                if current_set is not None:
                    all_tuples.append(current_set)
                    break
                else:
                    print("Ошибка при создании множества. Попробуйте снова.")
            else:
                print("Неверный выбор! Попробуйте снова.")
    
    print(f"\nВсе множества созданы:")
    for i, s in enumerate(all_tuples, 1):
        print(f"Множество {i} ({'ABC'[i-1]}): {s}")
    
    return all_tuples


def perform_operation(sets):
    """Выполняет операции над множествами"""
    if not sets:
        print("Сначала создайте множества!")
        return
    
    print("\n--- ВЫБОР ОПЕРАЦИИ ---")
    print("1 - Объединение множеств")
    print("2 - Пересечение множеств")
    print("3 - Разность множеств")
    print("4 - Симметрическая разность")
    print("5 - Дополнение")
    print("6 - Вычисление выражения")
    
    choice = input("Выберите операцию (1-6): ")
    
    if len(sets) < 3:
        print("Введено недостаточное количество множеств для выполнения операций!")
        return
    
    # Объединение двух множеств
    if choice == "1":
        print('\n\n~~~ Объединение двух множеств ~~~')
        print_sets(sets, [0, 1, 2])
        
        # Выбор двух множеств
        a = input("\nВыберите первое множество (1-3): ")
        b = input("Выберите второе множество (1-3): ")

        # обработка ошибки ввода
        if (int(a) < 1 or int(a) > 3 or int(b) < 1 or int(b) > 3 or int(a) == int(b)):  
            print("Некорректный ввод!")
            sys.exit()

        set1 = sets[int(a)-1]
        set2 = sets[int(b)-1]
        
        result = unificate(set1, set2)
        if not result:
            print(f"\nОбъединение множеств {a} и {b}: ∅")
        else:
            print(f"\nОбъединение множеств {a} и {b}: {sorted(result)}")

    # пересечение
    elif choice == "2":
        print('\n\n~~~ Пересечение двух множеств ~~~')
        print_sets(sets, [0, 1, 2])
        
        # Выбор двух множеств
        a = input("\nВыберите первое множество (1-3): ")
        b = input("Выберите второе множество (1-3): ")
        
        if (int(a) < 1 or int(a) > 3 or int(b) < 1 or int(b) > 3 or int(a) == int(b)):  
            print("Некорректный ввод!")
            sys.exit()
            
        set1 = sets[int(a)-1]
        set2 = sets[int(b)-1]
        
        result = intersection(set1, set2)
        if not result:
            print(f"\nПересечение множеств {a} и {b}: ∅")
        else:
            print(f"\nПересечение множеств {a} и {b}: {sorted(result)}")
    
    # разность
    elif choice == "3":
        print('\n\n~~~ Разность двух множеств ~~~')
        print_sets(sets, [0, 1, 2])
        
        # Выбор двух множеств с указанием порядка
        a = input("\nВыберите основное множество (уменьшаемое) (1-3): ")
        b = input("Выберите вычитаемое множество (1-3): ")
        
        if (int(a) < 1 or int(a) > 3 or int(b) < 1 or int(b) > 3 or int(a) == int(b)):  
            print("Некорректный ввод!")
            sys.exit()
            
        set1 = sets[int(a)-1]
        set2 = sets[int(b)-1]
        
        result = difference(set1, set2)
        if not result:
            print(f"\nРазность множеств {a} - {b}: ∅")
        else:
            print(f"\nРазность множеств {a} - {b}: {sorted(result)}")

    # симметрическая разность
    elif choice == "4":
        print('\n\n~~~ Симметрическая разность двух множеств ~~~')
        print_sets(sets, [0, 1, 2])
        
        # Выбор двух множеств
        a = input("\nВыберите первое множество (1-3): ")
        b = input("Выберите второе множество (1-3): ")
        
        if (int(a) < 1 or int(a) > 3 or int(b) < 1 or int(b) > 3 or int(a) == int(b)):  
            print("Некорректный ввод!")
            sys.exit()
            
        set1 = sets[int(a)-1]
        set2 = sets[int(b)-1]
        
        result = sym_difference(set1, set2)
        if not result:
            print(f"\nСимметрическая разность множеств {a} и {b}: ∅")
        else:
            print(f"\nСимметрическая разность множеств {a} и {b}: {sorted(result)}")

    # дополнение
    elif choice == "5":
        print('\n\n~~~ Дополнение множества ~~~')
        print_sets(sets, [0, 1, 2])
        
        # Выбор одного множества
        a = input("\nВыберите множество для дополнения (1-3): ")
        
        if (int(a) < 1 or int(a) > 3 ):  
            print("Некорректный ввод!")
            sys.exit()

        set1 = sets[int(a)-1]
        result, low, high = addition(set1)

        if not result:
            print(f"\nДополнение множества {a} до границ [{low}, {high}]: ∅")
        else:
            print(f"\nДополнение множества {a} до границ [{low}, {high}]: {sorted(result)}")
    
    
    # вычисление выражения
    elif choice == "6":
        equation(sets)

    else:
        print("Неверный выбор!")

# отображение главного меню
def menu():
    print("\n" + "="*50)
    print('Лабораторная работа 1 "Операции над множествами"')
    print("="*50)
    print("ГЛАВНОЕ МЕНЮ")
    print("1 - Создание множеств")
    print("2 - Изменение множеств")
    print("3 - Просмотр текущих множеств")
    print("4 - Выбор операции")
    print("5 - Сброс прогресса")
    print("6 - Выход")
    print("="*50)

# основная функция: интерфейс и запуск остальных функций
def main():
    current_sets = None
    
    while True:
        clear_console()
        menu()
        choice = input("Выберите пункт меню (1-6): ")
        
        if choice == "1":
            clear_console()
            current_sets = create_all_sets()
        
        elif choice == "2":
            clear_console()
            if current_sets:
                change_sets(current_sets)
            else:
                print("Ошибка! Множества еще не созданы!")

        elif choice == "3":
            clear_console()
            if current_sets:
                print_sets(current_sets, [0,1,2])
            else:
                print("Ошибка! Множества еще не созданы!")
        
        elif choice == "4":
            clear_console()
            if current_sets:
                perform_operation(current_sets)
            else:
                print("Ошибка! Множества еще не созданы!")
        
        elif choice == "5":
            clear_console()
            current_sets = None
            print("Прогресс сброшен! Все множества удалены.")
        
        elif choice == "6":
            print("Выход из программы...")
            break
        
        else:
            print("Неверный выбор! Попробуйте снова.")
        
        # Пауза перед следующим шагом
        if choice != "5":
            input("\nНажмите Enter для возврата в главное меню...")
    
    # перезапуск
    f = True
    while f:
        restart = input("\nХотите перезапустить? (y/n): ")
        if restart.lower() == 'y':
            clear_console()
            main()
            break
        elif restart.lower() == 'n':
            print("До свидания!")
            break
        else:
            print("Повторите ввод...")

# запуск
if __name__ == "__main__":
    main()