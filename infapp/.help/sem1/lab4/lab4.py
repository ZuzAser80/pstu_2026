import math


class BooleanFunction:
    """
    Класс, представляющий булеву функцию,
    заданную вектором значений.
    """

    def __init__(self, vector: str):
        self.Vector = vector
        self.VariablesCount = int(math.log2(len(vector)))

    # Проверка сохранения нуля (T0)
    def PreservesZero(self) -> bool:
        return self.Vector[0] == '0'

    # Проверка сохранения единицы (T1)
    def PreservesOne(self) -> bool:
        return self.Vector[-1] == '1'

    # Проверка самодвойственности (S)
    def IsSelfDual(self) -> bool:
        n = len(self.Vector)
        for i in range(n // 2):
            if self.Vector[i] == self.Vector[n - 1 - i]:
                return False
        return True

    # Проверка монотонности (M)
    def IsMonotonic(self) -> bool:
        n = self.VariablesCount
        vector_length = 1 << n

        for i in range(vector_length):
            for j in range(vector_length):
                if (i & j) == i:
                    if self.Vector[i] == '1' and self.Vector[j] == '0':
                        return False
        return True

    # Проверка линейности (L)
    def IsLinear(self) -> bool:
        n = self.VariablesCount
        vector_length = 1 << n
        values = [1 if self.Vector[i] == '1' else 0 for i in range(vector_length)]
        coeffs = values[:]

        # Преобразование Мёбиуса
        for i in range(n):
            step = 1 << i
            for j in range(0, vector_length, step * 2):
                for k in range(j, j + step):
                    coeffs[k + step] ^= coeffs[k]

        # Проверка коэффициентов на нелинейность
        for i in range(vector_length):
            if coeffs[i] == 1 and self._CountOnes(i) > 1:
                return False
        return True

    def _CountOnes(self, x: int) -> int:
        count = 0
        while x > 0:
            count += x & 1
            x >>= 1
        return count


class FunctionSystemAnalyzer:
    """
    Анализатор системы булевых функций
    на полноту и базис по теореме Поста.
    """

    def __init__(self):
        self.Functions = []

    def AddFunction(self, vector: str):
        length = len(vector)
        if length not in (1, 2, 4, 8):
            raise ValueError("Длина вектора должна быть 1, 2, 4 или 8")
        for c in vector:
            if c not in ('0', '1'):
                raise ValueError("Вектор должен содержать только '0' и '1'")
        self.Functions.append(BooleanFunction(vector))

    def PrintMembershipMatrix(self):
        print("\nМатрица принадлежности функций к замкнутым классам:")
        print("T0\tT1\tS\tM\tL")

        for func in self.Functions:
            print(
                f"{int(func.PreservesZero())}\t"
                f"{int(func.PreservesOne())}\t"
                f"{int(func.IsSelfDual())}\t"
                f"{int(func.IsMonotonic())}\t"
                f"{int(func.IsLinear())}"
            )

    def AnalyzeSystem(self):
        hasT0 = hasT1 = hasS = hasM = hasL = False

        for func in self.Functions:
            if not func.PreservesZero():
                hasT0 = True
            if not func.PreservesOne():
                hasT1 = True
            if not func.IsSelfDual():
                hasS = True
            if not func.IsMonotonic():
                hasM = True
            if not func.IsLinear():
                hasL = True

        isComplete = hasT0 and hasT1 and hasS and hasM and hasL
        isBasis = isComplete and self._IsMinimalSystem()

        print("\nРезультаты:")
        print("Система является полной:", "ДА" if isComplete else "НЕТ")
        print("Система является базисом:", "ДА" if isBasis else "НЕТ")

        if not isComplete:
            print("\nСистема не полна, так как отсутствуют функции из классов:")
            if not hasT0:
                print("- Не сохраняет 0")
            if not hasT1:
                print("- Не сохраняет 1")
            if not hasS:
                print("- Не самодвойственная")
            if not hasM:
                print("- Не монотонная")
            if not hasL:
                print("- Не линейная")

    def _IsMinimalSystem(self) -> bool:
        for i in range(len(self.Functions)):
            temp = [self.Functions[j] for j in range(len(self.Functions)) if j != i]
            if self._IsSystemComplete(temp):
                return False
        return True

    def _IsSystemComplete(self, functions) -> bool:
        if not functions:
            return False
        hasT0 = hasT1 = hasS = hasM = hasL = False
        for func in functions:
            if not func.PreservesZero():
                hasT0 = True
            if not func.PreservesOne():
                hasT1 = True
            if not func.IsSelfDual():
                hasS = True
            if not func.IsMonotonic():
                hasM = True
            if not func.IsLinear():
                hasL = True
        return hasT0 and hasT1 and hasS and hasM and hasL


def main():
    analyzer = FunctionSystemAnalyzer()

    print("Проверка полноты системы функций\n")

    try:
        print("Введите функции (для завершения введите 'end'):")

        while True:
            vector = input("Вектор функции (длина 1, 2, 4 или 8): ")
            if vector == "end":
                break
            analyzer.AddFunction(vector)
            print(f"Функция добавлена.\n")

        if not analyzer.Functions:
            print("\nИспользуется демонстрационный пример:")
            analyzer.AddFunction("0111")
            analyzer.AddFunction("1110")
            print("Добавлены функции: 0111, 1110")

        analyzer.PrintMembershipMatrix()
        analyzer.AnalyzeSystem()

        print("\nПроверка линейности:")
        a = 0
        for func in analyzer.Functions:
            a+=1
            print(a, end=". ")
            print(
                f"{'Линейна' if func.IsLinear() else 'Нелинейна'}"
            )

    except Exception as ex:
        print("Ошибка:", ex)

    input("\nНажмите Enter для выхода...")


if __name__ == "__main__":
    main()
