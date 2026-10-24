ALPHABET = {'a', 'b', 'c', 'd', 'e'}

# Таблица переходов: delta[state][symbol] -> new_state
# dead — явная ловушка (b встречено до первого a)
delta = {
    'q0':   {'a': 'q4',   'b': 'dead', 'c': 'q0',   'd': 'q1',   'e': 'q0'  },
    'q1':   {'a': 'q5',   'b': 'dead', 'c': 'q1',   'd': 'q2',   'e': 'q1'  },
    'q2':   {'a': 'q6',   'b': 'dead', 'c': 'q2',   'd': 'q3',   'e': 'q2'  },
    'q3':   {'a': 'q7',   'b': 'dead', 'c': 'q3',   'd': 'q3',   'e': 'q3'  },
    'q4':   {'a': 'q4',   'b': 'q4',   'c': 'q4',   'd': 'q5',   'e': 'q4'  },
    'q5':   {'a': 'q5',   'b': 'q5',   'c': 'q5',   'd': 'q6',   'e': 'q5'  },
    'q6':   {'a': 'q6',   'b': 'q6',   'c': 'q6',   'd': 'q7',   'e': 'q6'  },
    'q7':   {'a': 'q7',   'b': 'q7',   'c': 'q7',   'd': 'q7',   'e': 'q7'  },
    'dead': {'a': 'dead', 'b': 'dead', 'c': 'dead', 'd': 'dead', 'e': 'dead'},
}

ACCEPTING = {'q3', 'q7'}  # q3: b не было, d>=3; q7: a было, d>=3
START = 'q0'


def analyze(word):
    state = START
    count_d = 0
    for ch in word:
        if ch not in ALPHABET:
            return False, f"недопустимый символ '{ch}'", count_d
        if ch == 'd':
            count_d += 1
        state = delta[state][ch]
    return state in ACCEPTING, state, count_d


def main():
    print("Вариант 15: слова, в которых 'b' встречается не раньше первого 'a'")
    print("              и количество вхождений 'd' больше 2")
    print("Алфавит: {a, b, c, d, e}")
    print("Для выхода введите пустую строку.\n")

    while True:
        word = input("Слово: ").strip()
        if not word:
            break

        result, state, count_d = analyze(word)

        if result:
            print(f"  ПРИНАДЛЕЖИТ языку  |  d встретилось: {count_d}  |  состояние: {state}\n")
        else:
            if state == 'dead':
                print(f"  НЕ ПРИНАДЛЕЖИТ  |  встречено 'b' до первого 'a'  |  d встретилось: {count_d}\n")
            elif isinstance(state, str) and state.startswith("недопустимый"):
                print(f"  ОШИБКА: {state}\n")
            else:
                print(f"  НЕ ПРИНАДЛЕЖИТ  |  d встретилось: {count_d} (нужно >2)  |  состояние: {state}\n")


if __name__ == "__main__":
    main()