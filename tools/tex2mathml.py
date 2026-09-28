"""Преобразование формул LaTeX в MathML для вставки в документ OpenDocument.

Поддерживается подмножество, используемое в отчётах лабораторных работ:
дроби, суммы с пределами, матрицы (array, pmatrix, cases, aligned), разделители
\\left...\\right, модули, векторные и скалярные произведения, функции (max, det),
греческие буквы, boxed, substack, checkmark, vdots.

Неизвестная команда вызывает TexSyntaxError, чтобы ошибка не прошла молча.
"""

import re

# Символы, которые не требуют экранирования в XML
_LITERALS = {
    'ne': '≠',
    'neq': '≠',
    'le': '≤',
    'leq': '≤',
    'ge': '≥',
    'geq': '≥',
    'll': '≪',
    'gg': '≫',
    'approx': '≈',
    'equiv': '≡',
    'sim': '∼',
    'propto': '∝',
    'in': '∈',
    'notin': '∉',
    'subset': '⊂',
    'subseteq': '⊆',
    'cup': '∪',
    'cap': '∩',
    'forall': '∀',
    'exists': '∃',
    'pm': '±',
    'times': '×',
    'div': '÷',
    'cdot': '·',
    'cdots': '⋯',
    'ldots': '…',
    'dots': '…',
    'vdots': '⋮',
    'ddots': '⋱',
    'infty': '∞',
    'partial': '∂',
    'nabla': '∇',
    'varepsilon': 'ε',
    'epsilon': 'ε',
    'theta': 'θ',
    'alpha': 'α',
    'beta': 'β',
    'gamma': 'γ',
    'lambda': 'λ',
    'mu': 'μ',
    'sigma': 'σ',
    'tau': 'τ',
    'phi': 'φ',
    'omega': 'ω',
    'Delta': 'Δ',
    'Sigma': 'Σ',
    'Omega': 'Ω',
    'Phi': 'Φ',
    'leftarrow': '←',
    'rightarrow': '→',
    'Rightarrow': '⇒',
    'to': '→',
    'gets': '←',
    'mapsto': '↦',
    'ldots_': '…',
    'checkmark': '✓',
    'prime': '′',
    'circ': '∘',
    'ast': '∗',
    'star': '⋆',
    'angle': '∠',
    'perp': '⊥',
    'parallel': '∥',
    'lVert': '‖',
    'rVert': '‖',
    'lvert': '|',
    'rvert': '|',
    'Vert': '‖',
    'langle': '⟨',
    'rangle': '⟩',
    'lfloor': '⌊',
    'rfloor': '⌋',
    'lceil': '⌈',
    'rceil': '⌉',
    'backslash': '\\',
    'lbrace': '{',
    'rbrace': '}',
    'emptyset': '∅',
}

# Размеры пробелов в единицах em
_SPACES = {
    ',': 0.167,
    ':': 0.222,
    ';': 0.278,
    '!': -0.167,
    ' ': 0.25,
    'enspace': 0.5,
    'quad': 1.0,
    'qquad': 2.0,
    'thinspace': 0.167,
}

# Функции: печатаются прямым шрифтом
_FUNCTIONS = {'max', 'min', 'det', 'lim', 'sup', 'inf', 'sin', 'cos', 'tan',
              'exp', 'ln', 'lg', 'log', 'arg', 'deg', 'dim', 'ker'}

# Акценты: \overline{...} и подобные
_ACCENTS = {'overline': '\u203e', 'widehat': '\u005e', 'hat': '\u005e',
            'tilde': '~', 'widetilde': '~', 'bar': '\u00af',
            'vec': '\u2192', 'dot': '\u02d9', 'ddot': '\u00a8'}

_LIMITS = {'sum': '∑', 'prod': '∏', 'int': '∫', 'bigcup': '⋃',
           'bigcap': '⋂', 'lim': 'lim', 'max': 'max', 'min': 'min',
           'det': 'det', 'sup': 'sup', 'inf': 'inf'}


class TexSyntaxError(Exception):
    pass


def escape(text):
    return (text.replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;'))


def _is_letter(char):
    return char.isalpha()


def _is_digit(char):
    return char.isdigit() or char == '.'


class Node:
    """Узел дерева формулы. kind определяет тип узла."""

    __slots__ = ('kind', 'value', 'children', 'attrs')

    def __init__(self, kind, value=None, children=None, attrs=None):
        self.kind = kind
        self.value = value
        self.children = children or []
        self.attrs = attrs or {}


class Parser:
    def __init__(self, source):
        self.text = source
        self.pos = 0

    # --- вспомогательные функции доступа к исходному тексту ---

    def peek(self):
        return self.text[self.pos] if self.pos < len(self.text) else ''

    def eof(self):
        return self.pos >= len(self.text)

    def skip_spaces(self):
        while not self.eof() and self.text[self.pos] in ' \t\n\r':
            self.pos += 1

    def read_command(self):
        """Читает имя команды без обратного слэша."""
        start = self.pos
        while not self.eof() and (self.text[self.pos].isalpha() or
                                  (start + 1 < len(self.text) and
                                   self.text[self.pos] == self.text[start] and
                                   self.text[start].isalpha())):
            self.pos += 1
        return self.text[start:self.pos]

    # --- грамматика ---

    def parse(self):
        nodes = self.parse_sequence(())
        return nodes[0] if len(nodes) == 1 else Node('row', children=nodes)

    def parse_sequence(self, stop):
        nodes = []
        while not self.eof():
            char = self.peek()
            if char in stop:
                break
            if char == '}':
                break
            if char == '\\' and self.text[self.pos:self.pos + 4] in ('\\end', '\\rig'):
                break
            if char in ' \t\n\r':
                self.pos += 1
                continue
            node = self.parse_item(stop)
            if node is not None:
                nodes.append(node)
        return nodes

    def parse_item(self, stop):
        char = self.peek()

        # escape-последовательности
        if char == '\\':
            return self.parse_command()

        # группы
        if char == '{':
            self.pos += 1
            children = self.parse_sequence('}')
            if self.peek() == '}':
                self.pos += 1
            return Node('row', children=build_tree(children))

        # верхние и нижние индексы
        if char in '_^':
            self.pos += 1
            self.skip_spaces()
            return Node('script', value=char)

        # разделители строк и ячеек
        if char == '&':
            self.pos += 1
            return Node('amp')
        if char == '~':
            self.pos += 1
            return Node('mspace', value=0.333)

        # числа
        if char.isdigit():
            return self.parse_number()

        # буквы (латиница и кириллица)
        if _is_letter(char):
            self.pos += 1
            return Node('mi', value=char)

        # пробелы в математическом режиме игнорируются
        if char in ' \t\n\r':
            self.pos += 1
            return None

        # операторы и разделители
        self.pos += 1
        return Node('mo', value=char)

    def parse_number(self):
        start = self.pos
        while not self.eof() and (self.text[self.pos].isdigit() or
                                  (self.text[self.pos] == '.' and
                                   self.pos + 1 < len(self.text) and
                                   self.text[self.pos + 1].isdigit())):
            self.pos += 1
        return Node('mn', value=self.text[start:self.pos])

    def read_argument(self):
        """Читает один аргумент команды: группу {...} или одиночный символ."""
        self.skip_spaces()
        if self.eof():
            return Node('row')
        if self.peek() == '{':
            self.pos += 1
            children = self.parse_sequence('}')
            if self.peek() == '}':
                self.pos += 1
            return Node('row', children=build_tree(children))
        node = self.parse_item(())
        return node if node is not None else Node('row')

    def read_optional_argument(self):
        self.skip_spaces()
        if not self.eof() and self.peek() == '[':
            self.pos += 1
            start = self.pos
            while not self.eof() and self.peek() != ']':
                self.pos += 1
            value = self.text[start:self.pos]
            if not self.eof():
                self.pos += 1
            return value
        return None

    def parse_command(self):
        self.pos += 1  # пропускаем обратный слэш
        if self.eof():
            return Node('mo', value='\\')
        char = self.peek()
        if not char.isalpha():
            # небуквенная команда: \\, \cdot-like символы
            self.pos += 1
            if char in _SPACES:
                return Node('mspace', value=_SPACES[char])
            if char == '{':
                return Node('mo', value='{', attrs={'fence': 'false'})
            if char == '}':
                return Node('mo', value='}', attrs={'fence': 'false'})
            if char == '|':
                return Node('mo', value='\u2016', attrs={'fence': 'true',
                                                        'stretchy': 'false'})
            if char in ('(', ')', '[', ']'):
                return Node('mo', value=char,
                            attrs={'stretchy': 'false'})
            if char == ',':
                return Node('mspace', value=0.167)
            if char == ';':
                return Node('mspace', value=0.278)
            if char == '!':
                return Node('mspace', value=-0.167)
            if char == '%':
                return Node('comment')
            return Node('mo', value=char, attrs={'stretchy': 'false'})

        name = self.read_command()

        # разделитель строк \\
        if name == '\\':
            extra = self.read_optional_argument()
            return Node('newline', value=extra)

        # функции и пределы
        if name in _FUNCTIONS:
            return Node('opname', value=name)

        if name == 'frac' or name == 'dfrac' or name == 'tfrac':
            num = self.read_argument()
            den = self.read_argument()
            return Node('frac', children=[num, den])

        if name == 'binom':
            num = self.read_argument()
            den = self.read_argument()
            return Node('fenced', value='(', children=[num, den],
                        attrs={'open': '(', 'close': ')'})

        if name == 'sqrt':
            return Node('root', children=[self.read_argument()])

        if name in _LIMITS:
            return Node('bigop', value=_LIMITS[name])

        if name == 'left':
            opening = self.read_delimiter()
            body = Node('row', children=self.parse_sequence(()))
            closing = None
            self.skip_spaces()
            if self.text[self.pos:self.pos + 6] == '\\right':
                self.pos += 6
                closing = self.read_delimiter()
            return Node('fenced', value=opening, children=[body],
                        attrs={'open': opening or '',
                               'close': closing or ''})

        if name == 'right':
            self.read_delimiter()
            return Node('row')

        if name == 'big' or name == 'Big' or name == 'bigg' or name == 'Bigg':
            size = {'big': 1, 'Big': 1.2, 'bigg': 1.4, 'Bigg': 1.6}[name]
            symbol = self.read_delimiter()
            return Node('mo', value=symbol or '',
                        attrs={'stretchy': 'false', 'size': str(size)})

        if name in ('begin', 'end'):
            environment = self.read_argument()
            text = ''.join(child.value for child in environment.children
                           if child.kind in ('mi', 'mn', 'mo'))
            if name == 'end':
                return Node('endenv', value=text)
            return self.parse_environment(text)

        if name == 'boxed':
            return Node('boxed', children=[self.read_argument()])

        if name in _ACCENTS:
            return Node('accent', value=_ACCENTS[name],
                        children=[self.read_argument()])

        if name == 'underline':
            return Node('underaccent', value='_',
                        children=[self.read_argument()])

        if name == 'substack':
            return self.parse_substack()

        if name == 'text' or name == 'textrm' or name == 'mathrm' or \
                name == 'operatorname':
            return self.parse_text_argument()

        if name in _SPACES and _SPACES[name] > 0:
            return Node('mspace', value=_SPACES[name])

        if name == 'displaystyle' or name == 'textstyle' or name == 'limits' \
                or name == 'nolimits' or name == '!' or name == ',' \
                or name == 'nonumber':
            return Node('row')

        if name in _LITERALS:
            return Node('mo', value=_LITERALS[name],
                        attrs={'fence': 'false'})

        raise TexSyntaxError(f'неизвестная команда \\{name}')

    def read_delimiter(self):
        self.skip_spaces()
        if self.eof():
            return None
        char = self.peek()
        if char == '\\':
            self.pos += 1
            name = self.read_command()
            if name == 'lbrace' or name == '{':
                return '{'
            if name == 'rbrace' or name == '}':
                return '}'
            if name in ('|', 'Vert', 'lVert', 'rVert'):
                return '‖'
            if name == 'backslash':
                return '\\'
            if name == 'langle':
                return '⟨'
            if name == 'rangle':
                return '⟩'
            if name in ('lfloor',):
                return '⌊'
            if name in ('rfloor',):
                return '⌋'
            if name in ('lceil',):
                return '⌈'
            if name in ('rceil',):
                return '⌉'
            if name in _LITERALS:
                return _LITERALS[name]
            raise TexSyntaxError(f'неизвестный разделитель \\{name}')
        self.pos += 1
        return char

    def parse_environment(self, name):
        if name == 'array':
            spec = self.read_argument()
            column = ''.join(child.value for child in spec.children
                             if child.kind in ('mi', 'mo'))
            rows = self.parse_rows()
            align = []
            rules = []
            for char in column:
                if char == 'l':
                    align.append('left')
                elif char == 'r':
                    align.append('right')
                elif char == 'c':
                    align.append('center')
                elif char == '|':
                    rules.append(len(align) - 1)
            return Node('table', children=rows,
                        attrs={'align': align, 'rules': rules})

        if name in ('pmatrix', 'bmatrix', 'Bmatrix', 'vmatrix', 'matrix'):
            rows = self.parse_rows()
            fences = {'pmatrix': ('(', ')'), 'bmatrix': ('[', ']'),
                      'Bmatrix': ('{', '}'), 'vmatrix': ('|', '|'),
                      'matrix': ('', '')}[name]
            align = ['center'] * (max(len(row) for row in rows) if rows else 0)
            return Node('fenced', value=fences[0], children=[rows],
                        attrs={'open': fences[0], 'close': fences[1],
                               'align': align})

        if name == 'cases':
            rows = self.parse_rows()
            width = max(len(row) for row in rows) if rows else 0
            return Node('fenced', value='{', children=[rows],
                        attrs={'open': '{', 'close': '',
                               'align': ['left'] * width})

        if name == 'aligned' or name == 'align' or name == 'alignedat':
            rows = self.parse_rows()
            return Node('table', children=rows,
                        attrs={'align': ['right', 'left']})

        if name in ('gathered', 'smallmatrix'):
            rows = self.parse_rows()
            return Node('table', children=rows, attrs={'align': ['center']})

        raise TexSyntaxError(f'неизвестное окружение {name}')

    def parse_rows(self):
        """Разбирает тело окружения: список строк, каждая — список ячеек."""
        rows = []
        cells = []
        cell = []
        while not self.eof():
            if self.text[self.pos:self.pos + 4] == '\\end':
                self.pos += 4
                self.read_argument()
                if self.peek() == '}':
                    self.pos += 1
                break
            char = self.peek()
            if char == '}':
                self.pos += 1
                break
            if self.text[self.pos:self.pos + 2] == '\\\\':
                self.pos += 2
                self.read_optional_argument()
                cells.append(cell)
                rows.append(cells)
                cells = []
                cell = []
                continue
            if char == '&':
                self.pos += 1
                cells.append(cell)
                cell = []
                continue
            before = self.pos
            node = self.parse_item(())
            if node is not None and node.kind != 'newline':
                cell.append(node)
            if self.pos == before:
                self.pos += 1
        cells.append(cell)
        rows.append(cells)
        return [[build_tree(item) for item in row] for row in rows]

    def parse_substack(self):
        self.skip_spaces()
        self.pos += 1  # {
        rows = []
        current = []
        while not self.eof():
            char = self.peek()
            if char == '}':
                self.pos += 1
                break
            if self.text[self.pos:self.pos + 2] == '\\\\':
                self.pos += 2
                rows.append(current)
                current = []
                continue
            node = self.parse_item(())
            if node is not None and node.kind != 'newline':
                current.append(node)
        rows.append(current)
        table = Node('table', children=[[cell] for cell in rows],
                     attrs={'align': ['center'], 'scriptsize': 'true'})
        return table

    def parse_text_argument(self):
        self.skip_spaces()
        if self.eof() or self.peek() != '{':
            return Node('mtext', value='')
        self.pos += 1
        start = self.pos
        depth = 1
        while not self.eof():
            char = self.text[self.pos]
            if char == '\\':
                self.pos += 2
                continue
            if char == '{':
                depth += 1
            elif char == '}':
                depth -= 1
                if depth == 0:
                    break
            self.pos += 1
        value = self.text[start:self.pos]
        if not self.eof():
            self.pos += 1
        return Node('mtext', value=value)


def build_tree(nodes):
    """Превращает плоский список узлов в дерево, присоединяя индексы."""
    result = []
    index = 0
    while index < len(nodes):
        node = nodes[index]
        if node.kind == 'script' and result:
            base = result.pop()
            prime = None
            if base.kind == 'mo' and base.value in ("'", '\u2032'):
                base = Node('mo', value='\u2032')
                prime = base
                if not result:
                    result.append(prime)
                    index += 1
                    continue
                base = result.pop()
            sub = sup = None
            if node.value == '_':
                index += 1
                if index < len(nodes):
                    sub = nodes[index]
            else:
                index += 1
                if index < len(nodes):
                    sup = nodes[index]
            if prime is not None:
                sup = prime if sup is None else Node('row', children=[prime, sup])
            result.append(Node('subsup', value=base, children=[sub, sup]))
            index += 1
            continue
        if node.kind == 'row':
            node = Node('row', children=build_tree(node.children))
        result.append(node)
        index += 1
    return result


def _is_scriptable(node):
    return node is not None and node.kind not in ('amp', 'newline', 'mspace')


class MathMLWriter:
    """Сериализатор дерева формул в MathML."""

    def __init__(self):
        self.parts = []
        self.buffer = ''

    def write(self, nodes):
        self.parts = []
        for node in nodes:
            self.emit(node)
        return ''.join(self.parts)

    # --- вспомогательные ---

    def add(self, text):
        self.parts.append(text)

    @staticmethod
    def wrap(tag, content, attrs=''):
        return f'<mml:{tag}{attrs}>{content}</mml:{tag}>'

    def emit(self, node):
        if node is None:
            return
        handler = getattr(self, 'emit_' + node.kind, None)
        if handler is None:
            raise TexSyntaxError(f'неизвестный узел {node.kind}')
        handler(node)

    def emit_children(self, nodes, wrap_tag='mrow'):
        content = ''
        for node in nodes:
            content += self.render(node)
        return self.wrap(wrap_tag, content) if wrap_tag else content

    def render(self, node):
        saved = self.parts
        self.parts = []
        self.emit(node)
        result = ''.join(self.parts)
        self.parts = saved
        return result

    # --- узлы ---

    def emit_row(self, node):
        if not node.children:
            return
        self.add(self.emit_children(node.children))

    def emit_mi(self, node):
        value = node.value
        if len(value) > 1 and value.isalpha():
            parts = ''
            for char in value:
                parts += self.wrap('mi', escape(char))
            self.add(parts)
            return
        self.add(self.wrap('mi', escape(value)))

    def emit_mn(self, node):
        self.add(self.wrap('mn', escape(node.value)))

    def emit_mo(self, node):
        attrs = ''
        if node.attrs.get('stretchy') == 'false':
            attrs += ' stretchy="false"'
        if node.attrs.get('fence') == 'false':
            attrs += ' fence="false"'
        if 'size' in node.attrs:
            attrs += ' minsize="' + node.attrs['size'] + 'em"'
        self.add(self.wrap('mo', escape(node.value), attrs))

    def emit_mtext(self, node):
        self.add(self.wrap('mtext', escape(node.value)))

    def emit_opname(self, node):
        self.add(self.wrap('mi', escape(node.value), ' mathvariant="normal"'))

    def emit_mspace(self, node):
        self.add(self.wrap('mspace', '', ' width="%sem"' % node.value))

    def emit_frac(self, node):
        content = self.render(node.children[0]) + self.render(node.children[1])
        self.add(self.wrap('mfrac', content))

    def emit_root(self, node):
        content = self.render(node.children[0])
        self.add(self.wrap('msqrt', content))

    def emit_subsup(self, node):
        base = self.render(node.value)
        sub, sup = node.children
        if sub is not None and sup is not None:
            self.add(self.wrap('msubsup', base + self.render(sub) +
                               self.render(sup)))
        elif sub is not None:
            self.add(self.wrap('msub', base + self.render(sub)))
        elif sup is not None:
            self.add(self.wrap('msup', base + self.render(sup)))
        else:
            self.add(base)

    def emit_fenced(self, node):
        opening = node.attrs.get('open', '')
        closing = node.attrs.get('close', '')
        content = ''
        align = node.attrs.get('align')
        for child in node.children:
            content += self.render_body(child, align)
        if opening:
            content = (self.wrap('mo', escape(opening),
                                 ' stretchy="true" fence="true"') +
                       content)
        if closing:
            content += self.wrap('mo', escape(closing),
                                 ' stretchy="true" fence="true"')
        self.add(self.wrap('mrow', content))

    def render_body(self, child, align=None):
        """Отрисовывает содержимое: узел или таблицу (список строк)."""
        if isinstance(child, list):
            return self.render_rows(child, align)
        return self.render(child)

    def emit_boxed(self, node):
        content = self.emit_children(node.children)
        self.add(self.wrap('menclose', content, ' notation="box"'))

    def emit_accent(self, node):
        base = self.render(node.children[0])
        mark = self.wrap('mo', escape(node.value),
                         ' stretchy="false" fence="false"')
        self.add(self.wrap('mover', base + mark, ' accent="true"'))

    def emit_underaccent(self, node):
        base = self.render(node.children[0])
        mark = self.wrap('mo', escape(node.value),
                         ' stretchy="false" fence="false"')
        self.add(self.wrap('munder', base + mark, ' accentunder="true"'))

    def emit_bigop(self, node):
        self.add(self.wrap('mo', escape(node.value), ' stretchy="false"'))

    def emit_table(self, node):
        align = node.attrs.get('align') or []
        if node.attrs.get('scriptsize') == 'true':
            rows_xml = ''
            for row in node.children:
                cells = ''.join(self.emit_children(cell)
                                for cell in row if cell)
                rows_xml += self.wrap('mtr', cells)
            self.add(self.wrap('mtable', rows_xml, ' rowspacing="0.1em"'))
            return
        self.add(self.render_rows(node.children, align,
                                  node.attrs.get('rules') or []))

    def render_rows(self, rows, align, rules=()):
        """Отрисовывает строки ячеек в mtable, добавляя вертикальные линии."""
        column_align = ''
        if align:
            column_align = ' columnalign="' + ' '.join(align) + '"'
        rows_xml = ''
        for row in rows:
            cells = ''
            for index in range(len(align)):
                cell = row[index] if index < len(row) else []
                cells += self.wrap('mtd', self.emit_children(cell) if cell
                                   else '')
                if index in rules:
                    cells += self.wrap(
                        'mtd', self.wrap('mo', '|', ' stretchy="true"'))
            for cell in row[len(align):]:
                cells += self.wrap('mtd', self.emit_children(cell) if cell
                                   else '')
            rows_xml += self.wrap('mtr', cells)
        return self.wrap('mtable', rows_xml,
                         ' rowspacing="0.12em"' + column_align)


def latex_to_mathml(latex):
    """Возвращает MathML формулы без внешнего <mml:math>."""
    parser = Parser(latex)
    raw = parser.parse_sequence(())
    nodes = build_tree(raw)
    writer = MathMLWriter()
    body = ''.join(writer.render(node) for node in nodes)
    return body


def latex_to_math( latex, display=False):
    """Возвращает полный элемент <math> (в namespace MathML) для ODF."""
    body = latex_to_mathml(latex)
    return ('<math xmlns="http://www.w3.org/1998/Math/MathML" '
            f'display="{"block" if display else "inline"}">'
            f'<semantics>{body}</semantics></math>')
