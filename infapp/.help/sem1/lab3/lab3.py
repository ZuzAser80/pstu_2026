import itertools

def find_glued_implicants(implicants_list):
    # Функция поиска склеенных импликант
    # Сравнивает каждую пару импликант и находит те, которые отличаются
    # Только одним литералом (X/x, Y/y, Z/z, W/w)
    # Если такие находятся - создает новую импликанту без этого литерала и добавляет ее в результат
    glued_result = []
    
    total_implicants = len(implicants_list)
    for first_idx in range(total_implicants):
        for second_idx in range(first_idx + 1, total_implicants):
            current_implicant = implicants_list[first_idx]
            comparison_implicant = implicants_list[second_idx]
            
            difference_position = 0
            differences_count = 0
            
            # Поиск позиции различия между двумя импликантами
            min_length = min(len(current_implicant), len(comparison_implicant))
            for position in range(min_length):
                if comparison_implicant.find(current_implicant[position]) == -1:
                    difference_position = position
                    differences_count += 1
            
            # Если найдено ровно одно различие - возможно склеивание
            if differences_count == 1:
                differing_char = current_implicant[difference_position]
                
                # Проверяем пары противоположных литералов
                if (differing_char == 'X' and 'x' in comparison_implicant or
                    differing_char == 'x' and 'X' in comparison_implicant or
                    differing_char == 'Y' and 'y' in comparison_implicant or
                    differing_char == 'y' and 'Y' in comparison_implicant or
                    differing_char == 'Z' and 'z' in comparison_implicant or
                    differing_char == 'z' and 'Z' in comparison_implicant or
                    differing_char == 'W' and 'w' in comparison_implicant or
                    differing_char == 'w' and 'W' in comparison_implicant):
                    
                    # Удаляем различающийся литерал и добавляем результат
                    new_implicant = (current_implicant[:difference_position] + 
                                   current_implicant[difference_position + 1:])
                    glued_result.append(new_implicant)
    
    return glued_result


def check_mdnf_coverage(coverage_table, candidate_mdnf, coverage_mask, full_coverage_mask):
    # Функция проверки полноты покрытия в МДНФ
    # Проверяет, покрывает ли предложенная МДНФ всё множество 
    
    current_coverage = list(coverage_mask)
    
    # Проходим по всем импликантам в таблице покрытия
    for row_idx in range(len(coverage_table)):
        # Если импликант присутствует в МДНФ
        if candidate_mdnf.find(coverage_table[row_idx][0]) != -1:
            # Отмечаем покрытые
            for col_idx in range(1, len(coverage_table[0])):
                if coverage_table[row_idx][col_idx] == "1   ":
                    current_coverage[col_idx - 1] = '+'
    
    # Сравниваем с идеальным покрытием
    return ''.join(current_coverage) == full_coverage_mask


def minimize_boolean_function():
    # Основная функция минимизации функции
    
    # Инициализация таблицы истинности
    truth_values = [[False] * 5 for _ in range(16)]
    row_counter = 0
    
    # Генерация всех комбинаций переменных
    for x_val in [0, 1]:
        for y_val in [0, 1]:
            for z_val in [0, 1]:
                for w_val in [0, 1]:
                    truth_values[row_counter][0] = bool(x_val)
                    truth_values[row_counter][1] = bool(y_val)
                    truth_values[row_counter][2] = bool(z_val)
                    truth_values[row_counter][3] = bool(w_val)
                    row_counter += 1

    # Ввод вектора значений функции
    function_vector = input("Вектор значений функции = ").replace(" ", "")

    # Заполнение столбца значений функции
    for row_idx in range(16):
        if function_vector[row_idx] == '0':
            truth_values[row_idx][4] = False
        else:
            truth_values[row_idx][4] = True

    # Вывод таблицы истинности
    print("\nТаблица истинности:")
    print("X  Y  Z  W  f")
    for row_idx in range(16):
        for col_idx in range(5):
            print(int(truth_values[row_idx][col_idx]), end="  ")
        print()

    # Построение СДНФ
    perfect_dnf = []
    for row_idx in range(16):
        if truth_values[row_idx][4]:  # Если функция равна 1
            term_literals = ""
            
            # Формирование с учетом отрицания
            # Строчные буквы - истинные переменные, заглавные - отрицания
            term_literals += "x" if truth_values[row_idx][0] else "X"
            term_literals += "y" if truth_values[row_idx][1] else "Y"
            term_literals += "z" if truth_values[row_idx][2] else "Z"
            term_literals += "w" if truth_values[row_idx][3] else "W"
            
            perfect_dnf.append(term_literals)

    # Вывод СДНФ
    print("\nСовершенная ДНФ:")
    print(" ∨ ".join(perfect_dnf))
    print()

    # Метод Квайна для нахождения простых импликант
    current_implicants = perfect_dnf.copy()
    all_found_implicants = perfect_dnf.copy()
    
    # Многократное склеивание пока это возможно
    while len(current_implicants) > 1:
        current_implicants = find_glued_implicants(current_implicants)
        all_found_implicants.extend(current_implicants)

    # Отбор ядерных импликант
    core_implicants = []
    core_implicants.append(all_found_implicants[-1])
    
    for impl_idx in range(len(all_found_implicants) - 2, -1, -1):
        is_redundant = False
        
        for core_impl in core_implicants:
            matching_literals = 0
            min_len = min(len(all_found_implicants[impl_idx]), len(core_impl))
            
            for pos in range(min_len):
                if all_found_implicants[impl_idx].find(core_impl[pos]) != -1:
                    matching_literals += 1
            
            if matching_literals == len(core_impl):
                is_redundant = True
                break
        
        if not is_redundant:
            core_implicants.append(all_found_implicants[impl_idx])

    # Удаление избыточных импликант 
    while len(all_found_implicants[-1]) < 4:
        all_found_implicants.pop()

    # Построение таблицы покрытия
    coverage_table = [["" for _ in range(len(all_found_implicants) + 1)] 
                     for _ in range(len(core_implicants) + 1)]

    # Заполнение заголовков таблицы
    for col in range(1, len(all_found_implicants) + 1):
        coverage_table[0][col] = all_found_implicants[col - 1]
    
    for row in range(1, len(core_implicants) + 1):
        coverage_table[row][0] = core_implicants[row - 1] # импликанта = конституенте

    # Заполнение ячеек таблицы покрытия
    # Если импликанта покрывает конституенте - ставится 1
    # Иначе остается пусто
    # Проход по всем конституентам и импликантам
    for row in range(1, len(coverage_table)):
        for col in range(1, len(coverage_table[0])):
            covered_literals = 0
            
            # Подсчет покрытых литералов
            for literal in coverage_table[row][0]:
                if coverage_table[0][col].find(literal) != -1:
                    covered_literals += 1
            
            # Все литералы импликанты * содержатся в конституенте?     
            # Если да - ставится 1     
            if covered_literals == len(coverage_table[row][0]):
                coverage_table[row][col] = "1   "
            

    # Вывод таблицы покрытия
    print("Таблица покрытия (импликантная матрица):")
    for row in coverage_table:
        for cell in row:
            padding = " " * (4 - len(cell))
            print(padding + cell + " ", end="")
        print()

    # Подготовка к поиску минимального покрытия
    candidate_expression = ""
    coverage_status = ""
    target_coverage = ""

    # Инициализация строк покрытия
    for _ in range(len(coverage_table[0]) - 1):
        coverage_status += "-"  # Непокрытые 
        target_coverage += "+"  # Полное покрытие

    # Генерация всех комбинаций импликант
    total_implicant_sets = len(coverage_table) - 1
    combination_matrix = [[0] * (1 << total_implicant_sets) 
                         for _ in range(total_implicant_sets)] # Матрица комбинаций 

    # Заполнение матрицы комбинаций
    pattern_size = 1 << (total_implicant_sets - 1) # Размер шаблона покрытия 
    
    # Для каждого столбца (импликанты) генерируем шаблон покрытия 
    for combination_col in range(total_implicant_sets):
        current_row = pattern_size
        
        while current_row < (1 << total_implicant_sets):
            for fill_count in range(pattern_size):
                if current_row + fill_count < (1 << total_implicant_sets):
                    combination_matrix[combination_col][current_row + fill_count] = 1
            
            current_row += pattern_size * 2
        
        pattern_size >>= 1

    # Поиск минимальной ДНФ
    min_length = 1000
    minimal_dnf = ""
    
    # Перебор всех комбинаций импликант
    # Каждая комбинация представлена числом от 0 до 2^n - 1
    # n - количество импликант
    # Формирование текущей комбинации и проверка покрытия
    for combination_code in range(1 << total_implicant_sets):
        for impl_index in range(total_implicant_sets):
            if combination_matrix[impl_index][combination_code]:
                candidate_expression += coverage_table[impl_index + 1][0] + " ∨ " # Формирование текущей комбинации 
        
        # Проверяем покрытие текущей комбинацией
        # Если покрытие полное - сравниваем длины ДНФ 
        # и сохраняем минимальную
        if check_mdnf_coverage(coverage_table, candidate_expression, coverage_status, target_coverage):
            if candidate_expression:
                candidate_expression = candidate_expression[:-3]  # Удаляем " ∨ " в конце 
            
            # Выбираем самую короткую ДНФ
            # Сравнение по длине строки
            # Если текущая короче минимальной найденной - обновляем
            
            if len(candidate_expression) < min_length:
                min_length = len(candidate_expression)
                minimal_dnf = candidate_expression
         
        candidate_expression = "" # Сброс для следующей комбинации
 
    # Вывод результата
    print(f"\nМинимальная ДНФ: {minimal_dnf}")


minimize_boolean_function()