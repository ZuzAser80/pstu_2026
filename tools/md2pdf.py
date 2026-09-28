#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Сборка отчёта и листинга программы в PDF через LibreOffice.

Отчёт: Markdown -> FODT -> PDF. Формулы LaTeX переводятся в MathML
модулем tex2mathml и вставляются объектами math. Размер объекта
подбирается измерением натурального размера формулы: сначала
собирается вспомогательный документ, где каждая формула стоит в
рамке большого размера между горизонтальными разделителями, затем
страницы растрируются в PGM и по тёмным пикселям вычисляются
натуральные размеры.

Запуск:
    python3 tools/md2pdf.py
    python3 tools/md2pdf.py --no-listing --out-dir /tmp/report
"""
import argparse
import os
import re
import shutil
import subprocess
import sys
import tempfile

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import formula_png
import tex2mathml

MATHML_NS = 'http://www.w3.org/1998/Math/MathML'

# --- оформление -----------------------------------------------------------

FONT_TEXT = 'Liberation Serif'      # метрически совместим с Times New Roman
FONT_MONO = 'Liberation Mono'       # метрически совместим с Courier New
FONT_SIZE = 14.0
LINE_HEIGHT = '150%'

PAGE_WIDTH = 21.0
PAGE_HEIGHT = 29.7
MARGIN_LEFT = 3.0
MARGIN_RIGHT = 1.5
MARGIN_TOP = 2.0
MARGIN_BOTTOM = 2.0
TEXT_WIDTH = PAGE_WIDTH - MARGIN_LEFT - MARGIN_RIGHT

FORMULA_PAD_X = 3.0                 # запас рамки по ширине, пт
FORMULA_PAD_Y = 1.5                 # запас рамки по высоте, пт
FALLBACK_SIZE = (170.0, 24.0)       # размер, если измерение не удалось


def cm(value):
    return f'{value:.2f}cm'


def pt_to_cm(value):
    return value / 72.0 * 2.54


def escape(text):
    return (text.replace('&', '&amp;').replace('<', '&lt;')
            .replace('>', '&gt;').replace('"', '&quot;'))


def keep_spaces(text):
    """Выводит текст с сохранением пробелов и переводов строк."""
    out = ''
    for index, line in enumerate(text.split('\n')):
        if index:
            out += '<text:line-break/>'
        while '  ' in line:
            head, line = line.split('  ', 1)
            out += escape(head) + '<text:s/>'
        out += escape(line)
    return out


# --- внешние программы ----------------------------------------------------

def run_soffice(source, out_dir, target='pdf'):
    """Конвертирует документ в PDF и возвращает путь к результату."""
    profile = tempfile.mkdtemp(prefix='loprofile')
    try:
        subprocess.run(
            ['soffice', '--headless', '--norestore', '--invisible',
             f'-env:UserInstallation=file://{profile}',
             '--convert-to', target, '--outdir', out_dir, source],
            check=True, capture_output=True, timeout=900)
    finally:
        shutil.rmtree(profile, ignore_errors=True)
    result = os.path.join(out_dir,
                          os.path.splitext(os.path.basename(source))[0] + '.' + target)
    if not os.path.exists(result):
        raise RuntimeError(f'LibreOffice не создал {result}')
    return result


INLINE_RE = re.compile(
    r'(?P<math>\$[^$]+\$)'
    r'|(?P<code>`[^`]+`)'
    r'|(?P<bold>\*\*(?P<bold_text>.+?)\*\*)'
    r'|(?P<italic>(?<![\w*])\*(?P<italic_text>[^*]+?)\*(?!\*))'
    r'|(?P<link>\[(?P<label>[^\]]+)\]\((?P<url>[^)]+)\))', re.S)


def parse_inline(text):
    """Фрагменты текста: ('text'|'bold'|'italic'|'code'|'math'|'link', ...)."""
    parts = []
    pos = 0
    for match in INLINE_RE.finditer(text):
        if match.start() > pos:
            parts.append(('text', text[pos:match.start()]))
        if match.group('math'):
            parts.append(('math', match.group('math')[1:-1]))
        elif match.group('code'):
            parts.append(('code', match.group('code')[1:-1]))
        elif match.group('bold'):
            parts.append(('bold', match.group('bold_text')))
        elif match.group('italic'):
            parts.append(('italic', match.group('italic_text')))
        elif match.group('link'):
            parts.append(('link', match.group('label'), match.group('url')))
        pos = match.end()
    if pos < len(text):
        parts.append(('text', text[pos:]))
    return parts


def inline_plain(parts):
    """Текст без разметки (для измерения ширины столбцов таблицы)."""
    out = []
    for part in parts:
        if part[0] == 'link':
            out.append(part[1])
        elif part[0] in ('text', 'code', 'math', 'bold', 'italic'):
            out.append(part[1])
    return ''.join(out)


def split_table_row(line):
    """Делит строку таблицы по вертикальной черте, игнорируя формулы."""
    cells = []
    current = ''
    in_math = False
    for char in line:
        if char == '$':
            in_math = not in_math
        if char == '|' and not in_math:
            cells.append(current)
            current = ''
            continue
        current += char
    cells.append(current)
    if cells and not cells[0].strip():
        cells.pop(0)
    if cells and not cells[-1].strip():
        cells.pop()
    return [cell.strip() for cell in cells]


def is_table_separator(line):
    return bool(re.match(r'^\|?[\s:\-|]+\|[\s:\-|]*$', line)) and '-' in line


def parse_markdown(source):
    """Разбирает Markdown на блоки."""
    lines = source.replace('\r\n', '\n').split('\n')
    blocks = []
    index = 0
    while index < len(lines):
        line = lines[index]
        stripped = line.strip()
        if not stripped:
            index += 1
            continue
        if stripped.startswith('```'):
            index += 1
            body = []
            while index < len(lines) and not lines[index].strip().startswith('```'):
                body.append(lines[index])
                index += 1
            index += 1
            blocks.append(('code', '\n'.join(body)))
            continue
        if stripped == '$$':
            index += 1
            body = []
            while index < len(lines) and lines[index].strip() != '$$':
                body.append(lines[index])
                index += 1
            index += 1
            blocks.append(('math', ' '.join(' '.join(body).split())))
            continue
        heading = re.match(r'^(#{1,6})\s+(.*)$', stripped)
        if heading:
            blocks.append(('h', len(heading.group(1)), heading.group(2).strip()))
            index += 1
            continue
        if re.match(r'^([-*_]\s*){3,}$', stripped):
            blocks.append(('hr',))
            index += 1
            continue
        if stripped.startswith('|') and index + 1 < len(lines) and \
                is_table_separator(lines[index + 1]):
            header = split_table_row(stripped)
            aligns = []
            for cell in split_table_row(lines[index + 1]):
                left, right = cell.startswith(':'), cell.endswith(':')
                if left and right:
                    aligns.append('center')
                elif right:
                    aligns.append('right')
                else:
                    aligns.append('left')
            index += 2
            rows = []
            while index < len(lines) and lines[index].strip().startswith('|'):
                rows.append(split_table_row(lines[index].strip()))
                index += 1
            blocks.append(('table', header, aligns, rows))
            continue
        if re.match(r'^\s*([-*]|\d+\.)\s+', line):
            ordered = bool(re.match(r'^\s*\d+\.\s+', line))
            items = []
            while index < len(lines):
                match = re.match(r'^\s*([-*]|\d+\.)\s+(.*)$', lines[index])
                if not match or bool(re.match(r'^\s*\d+\.\s+', lines[index])) != ordered:
                    break
                items.append(match.group(2).strip())
                index += 1
            blocks.append(('list', ordered, items))
            continue
        paragraph = []
        while index < len(lines) and lines[index].strip() and \
                not re.match(r'^\s*(#{1,6}\s|```|\||\$\$)', lines[index]) and \
                not re.match(r'^([-*_]\s*){3,}$', lines[index].strip()) and \
                not re.match(r'^\s*([-*]|\d+\.)\s+', lines[index]):
            paragraph.append(lines[index].strip())
            index += 1
        if paragraph:
            blocks.append(('p', ' '.join(paragraph)))
        else:
            index += 1
    return blocks


# --- стили ODF ------------------------------------------------------------

def paragraph_style(name, parent, text='', paragraph=''):
    return (f'<style:style style:name="{name}" style:family="paragraph"'
            f' style:parent-style-name="{parent}">'
            f'<style:paragraph-properties {paragraph}/>'
            f'<style:text-properties {text}/></style:style>')


def character_styles():
    return [
        '<style:style style:name="Bold" style:family="text">'
        '<style:text-properties fo:font-weight="bold"/></style:style>',
        '<style:style style:name="Italic" style:family="text">'
        '<style:text-properties fo:font-style="italic"/></style:style>',
        '<style:style style:name="Mono" style:family="text">'
        f'<style:text-properties style:font-name="{FONT_MONO}" fo:font-size="0.9em"/>'
        '</style:style>',
        '<style:style style:name="Link" style:family="text">'
        '<style:text-properties fo:color="#0B5FA5" fo:font-style="italic"/>'
        '</style:style>',
    ]


def page_layout(left=MARGIN_LEFT, right=MARGIN_RIGHT):
    return ('<style:page-layout style:name="pm1"><style:page-layout-properties'
            f' fo:page-width="{cm(PAGE_WIDTH)}" fo:page-height="{cm(PAGE_HEIGHT)}"'
            f' fo:margin-top="{cm(MARGIN_TOP)}" fo:margin-bottom="{cm(MARGIN_BOTTOM)}"'
            f' fo:margin-left="{cm(left)}" fo:margin-right="{cm(right)}"'
            ' style:print-orientation="portrait"/></style:page-layout>')


def standard_style(font=FONT_TEXT, size=FONT_SIZE, align='justify',
                   line_height=LINE_HEIGHT):
    return ('<style:style style:name="Standard" style:family="paragraph"'
            ' style:default-outline-level="0">'
            f'<style:paragraph-properties fo:text-align="{align}"'
            f' fo:line-height="{line_height}"/>'
            f'<style:text-properties style:font-name="{font}" fo:font-size="{size}pt"'
            ' fo:language="ru" fo:country="RU"/></style:style>')


def report_styles():
    """Стили отчёта."""
    indent = 'fo:text-indent="1.25cm" fo:margin-top="0cm" fo:margin-bottom="0cm"'
    parts = [page_layout(), standard_style()]
    parts.append(paragraph_style('Body', 'Standard', f'fo:font-size="{FONT_SIZE}pt"',
                                 indent))
    parts.append(paragraph_style(
        'BodyFirst', 'Standard', f'fo:font-size="{FONT_SIZE}pt"',
        'fo:text-indent="0cm" fo:margin-top="0cm" fo:margin-bottom="0cm"'))
    parts.append(paragraph_style(
        'Title', 'Standard', 'fo:font-weight="bold" fo:font-size="16pt"',
        'fo:text-align="center" fo:line-height="120%" fo:margin-bottom="0.15cm"'))
    parts.append(paragraph_style(
        'Subtitle', 'Standard', 'fo:font-size="14pt"',
        'fo:text-align="center" fo:line-height="120%" fo:margin-bottom="0.1cm"'))
    parts.append(paragraph_style(
        'Section', 'Standard', 'fo:font-weight="bold" fo:font-size="15pt"',
        'fo:text-align="left" fo:line-height="120%" fo:margin-top="0.5cm"'
        ' fo:margin-bottom="0.25cm" fo:keep-with-next="always"'))
    parts.append(paragraph_style(
        'Subsection', 'Standard', 'fo:font-weight="bold" fo:font-size="14pt"',
        'fo:text-align="left" fo:margin-top="0.35cm" fo:margin-bottom="0.2cm"'
        ' fo:keep-with-next="always"'))
    parts.append(paragraph_style(
        'Rule', 'Standard', 'fo:font-size="6pt"',
        'fo:border-bottom="0.02cm solid #A8A8A8" fo:margin-top="0.3cm"'
        ' fo:margin-bottom="0.3cm" fo:line-height="100%"'))
    formula_common = ('fo:text-align="center" fo:line-height="100%"'
                      ' fo:margin-top="0.25cm" fo:margin-bottom="0.25cm"'
                      ' fo:keep-together="always"')
    parts.append(paragraph_style('Formula', 'Standard', 'fo:font-size="12pt"',
                                 formula_common))
    parts.append(paragraph_style(
        'Boxed', 'Standard', 'fo:font-size="12pt"',
        formula_common + ' fo:border="0.5pt solid #000000" fo:padding="0.12cm"'))
    parts.append(paragraph_style(
        'FormulaIn', 'Standard', 'fo:font-size="12pt"',
        'fo:text-align="left" fo:line-height="100%" fo:keep-together="always"'))
    parts.append(paragraph_style(
        'Code', 'Standard', f'style:font-name="{FONT_MONO}" fo:font-size="11pt"',
        'fo:text-align="left" fo:line-height="110%" fo:padding="0.12cm"'
        ' fo:margin-top="0.2cm" fo:margin-bottom="0.2cm" fo:text-indent="0cm"'
        ' fo:background-color="#F7F7F7" fo:border="0.5pt solid #C8C8C8"'))
    cell_common = ('fo:line-height="100%" fo:margin-top="0.05cm"'
                   ' fo:margin-bottom="0.05cm" fo:text-indent="0cm"')
    parts.append(paragraph_style('TableHead', 'Standard',
                                 'fo:font-weight="bold" fo:font-size="12pt"',
                                 'fo:text-align="center" ' + cell_common))
    for name, align in (('TableLeft', 'left'), ('TableRight', 'right'),
                        ('TableCenter', 'center')):
        parts.append(paragraph_style(name, 'Standard', 'fo:font-size="12pt"',
                                     f'fo:text-align="{align}" ' + cell_common))
    parts.append(paragraph_style(
        'ListItem', 'Standard', f'fo:font-size="{FONT_SIZE}pt"',
        'fo:margin-top="0cm" fo:margin-bottom="0.1cm" fo:text-indent="0cm"'))
    parts.append(paragraph_style(
        'Spacer', 'Standard', 'fo:font-size="8pt"',
        'fo:line-height="100%" fo:margin-top="0cm" fo:margin-bottom="0cm"'))
    parts.append(paragraph_style(
        'Footer', 'Standard', 'fo:font-size="12pt"',
        'fo:text-align="center" fo:line-height="100%" fo:margin-top="0cm"'))
    parts.append(
        '<style:style style:name="Frame" style:family="graphic">'
        '<style:graphic-properties style:vertical-pos="from-top"'
        ' style:vertical-rel="baseline" fo:border="none" style:wrap="none"'
        ' style:run-through="all" draw:stroke="none" draw:fill="none"'
        ' fo:padding="0cm"/></style:style>')
    parts.append(
        '<style:style style:name="FormulaImg" style:family="graphic">'
        '<style:graphic-properties style:vertical-pos="bottom"'
        ' style:vertical-rel="baseline" fo:border="none" style:wrap="none"'
        ' style:run-through="all" style:protect="none" draw:stroke="none"'
        ' draw:fill="none" fo:padding="0cm"/></style:style>')
    parts.append(
        '<style:style style:name="Tbl" style:family="table">'
        '<style:table-column-properties table:display="true"/>'
        '<style:table-cell-properties fo:border="0.5pt solid #000000"'
        ' fo:padding="0.08cm"/></style:style>')
    parts.append(
        '<style:style style:name="TblHeadCell" style:family="table-cell">'
        '<style:table-cell-properties fo:background-color="#EDEDED"/></style:style>')
    return parts


def listing_styles():
    """Стили листинга."""
    parts = [page_layout(left=2.0), standard_style(FONT_MONO, 9.0, 'left')]
    parts.append(paragraph_style(
        'LNumber', 'Standard', 'fo:font-size="8pt" fo:color="#808080"',
        'fo:text-align="end" fo:line-height="100%" fo:text-indent="0cm"'
        ' fo:margin-top="0cm" fo:margin-bottom="0cm"'))
    parts.append(paragraph_style(
        'LCode', 'Standard', 'fo:font-size="9pt"',
        'fo:text-align="left" fo:line-height="100%" fo:text-indent="0cm"'
        ' fo:margin-top="0cm" fo:margin-bottom="0cm"'))
    parts.append(paragraph_style(
        'LTitle', 'Standard',
        f'style:font-name="{FONT_TEXT}" fo:font-weight="bold" fo:font-size="13pt"',
        'fo:margin-top="0cm" fo:margin-bottom="0.2cm" fo:line-height="120%"'))
    parts.append(paragraph_style(
        'LFile', 'Standard',
        f'style:font-name="{FONT_TEXT}" fo:font-weight="bold" fo:font-size="10.5pt"',
        'fo:margin-top="0.3cm" fo:margin-bottom="0.1cm" fo:line-height="110%"'
        ' fo:background-color="#E4E4E4" fo:padding="0.06cm"'
        ' fo:keep-with-next="always"'))
    parts.append(paragraph_style(
        'LInfo', 'Standard',
        f'style:font-name="{FONT_TEXT}" fo:font-size="11pt" fo:color="#404040"',
        'fo:line-height="120%" fo:margin-bottom="0.1cm"'))
    parts.append(paragraph_style(
        'Footer', 'Standard', 'fo:font-size="12pt"',
        'fo:text-align="center" fo:line-height="100%"'))
    parts.append(
        '<style:style style:name="LsTbl" style:family="table">'
        '<style:table-column-properties table:display="true"/>'
        '<style:table-cell-properties fo:border="none" fo:padding="0cm"'
        ' fo:background-color="transparent"/></style:style>')
    parts.append(
        '<style:style style:name="ColNum" style:family="table-column">'
        '<style:table-column-properties style:column-width="1.1cm"/></style:style>')
    parts.append(
        '<style:style style:name="ColCode" style:family="table-column">'
        f'<style:table-column-properties style:column-width="'
        f'{cm(PAGE_WIDTH - 2.0 - 1.5 - 1.1)}"/></style:style>')
    for name, color, weight in (('CsComment', '#3C7A3C', 'normal'),
                                ('CsString', '#A33A2B', 'normal'),
                                ('CsKeyword', '#00338C', 'bold'),
                                ('CsType', '#0B6E4F', 'normal'),
                                ('CsNumber', '#7A3E9D', 'normal')):
        parts.append(f'<style:style style:name="{name}" style:family="text">'
                     f'<style:text-properties fo:color="{color}"'
                     f' fo:font-weight="{weight}"/></style:style>')
    return parts


def wrap_document(automatic_styles, body, footer=True):
    """Готовый FODT по стилям и разметке тела."""
    page_footer = ''
    if footer:
        page_footer = ('<style:footer><text:p text:style-name="Footer">'
                       '<text:page-number text:select-page="current">1'
                       '</text:page-number></text:p></style:footer>')
    return (
        '<?xml version="1.0" encoding="UTF-8"?>\n'
        '<office:document\n'
        ' xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"\n'
        ' xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0"\n'
        ' xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0"\n'
        ' xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0"\n'
        ' xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0"\n'
        ' xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0"\n'
        ' xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0"\n'
        ' xmlns:xlink="http://www.w3.org/1999/xlink"\n'
        ' office:version="1.3"'
        ' office:mimetype="application/vnd.oasis.opendocument.text">\n'
        ' <office:automatic-styles>\n'
        + '\n'.join(automatic_styles)
        + '\n </office:automatic-styles>\n'
        ' <office:master-styles>\n'
        '  <style:master-page style:name="Standard" style:page-layout-name="pm1">'
        + page_footer + '</style:master-page>\n'
        ' </office:master-styles>\n'
        ' <office:body><office:text>\n'
        + body
        + '\n </office:text></office:body>\n'
        '</office:document>\n')


# --- формулы --------------------------------------------------------------

BOXED_RE = re.compile(r'^\s*\\boxed\{(.*)\}\s*$', re.S)


def formula_key(latex):
    return ' '.join(latex.split())


def formula_object(latex):
    """MathML-разметка формулы с объявленными пространствами имён.

    Содержимое обёрнуто в mrow: иначе LibreOffice считает каждого прямого
    потомка math отдельной строкой и разбивает формулу переносами.
    """
    inner = tex2mathml.latex_to_mathml(latex)
    return (f'<math xmlns="{MATHML_NS}" xmlns:mml="{MATHML_NS}" display="inline">'
            f'<mrow>{inner}</mrow></math>')


def formula_frame(latex, images):
    """Формула-картинка натурального размера."""
    key = formula_key(latex)
    entry = images.get(key)
    if entry is None:
        width, height = FALLBACK_SIZE
        return (f'<draw:frame draw:style-name="Frame"'
                f' svg:width="{pt_to_cm(width + 2 * FORMULA_PAD_X):.3f}cm"'
                f' svg:height="{pt_to_cm(height + 2 * FORMULA_PAD_Y):.3f}cm"'
                f' text:anchor-type="as-char"><draw:object>'
                f'{formula_object(latex)}</draw:object></draw:frame>')
    png, width, height = entry
    href = f'formula_png/{formula_png.cache_name(key)}.png'
    return (f'<draw:frame draw:style-name="FormulaImg"'
            f' svg:width="{pt_to_cm(width):.3f}cm"'
            f' svg:height="{pt_to_cm(height):.3f}cm"'
            f' text:anchor-type="as-char">'
            f'<draw:image xlink:href="{href}"/>'
            f'</draw:frame>')


def inline_markup(parts, sizes):
    """Разметка фрагментов абзаца."""
    out = []
    for part in parts:
        kind = part[0]
        if kind == 'text':
            out.append(escape(part[1]))
        elif kind == 'math':
            out.append(formula_frame(part[1], sizes))
        elif kind == 'code':
            out.append(f'<text:span text:style-name="Mono">{escape(part[1])}'
                       '</text:span>')
        elif kind == 'link':
            out.append(f'<text:span text:style-name="Link">{escape(part[1])}'
                       '</text:span>')
        else:
            out.append(f'<text:span text:style-name="{"Bold" if kind == "bold" else "Italic"}">'
                       f'{inline_markup(parse_inline(part[1]), sizes)}</text:span>')
    return ''.join(out)


# --- измерение формул -----------------------------------------------------

def measure_formulas(formulas, work_dir, dpi=120):
    """PNG и натуральный размер каждой формулы.

    Формула в Writer растягивается под размер рамки, поэтому натуральный размер
    берётся из отдельного документа Math: формула рендерится в PNG и
    обрезается по границам чёрных пикселей.
    """
    if not formulas:
        return {}
    return formula_png.render(formulas, work_dir, dpi=formula_png.DPI)


# --- таблицы --------------------------------------------------------------

def column_widths(header, rows):
    """Ширины столбцов пропорционально содержимому."""
    count = len(header)
    weights = []
    for index in range(count):
        cells = [header[index] if index < len(header) else '']
        cells += [row[index] if index < len(row) else '' for row in rows]
        longest = max((len(inline_plain(parse_inline(cell))) for cell in cells),
                      default=1)
        weights.append(float(max(6, min(45, longest))))
    total = sum(weights)
    widths = [TEXT_WIDTH * weight / total for weight in weights]
    minimum = TEXT_WIDTH / (count * 2.5)
    widths = [max(value, minimum) for value in widths]
    scale = TEXT_WIDTH / sum(widths)
    return [value * scale for value in widths]


def table_markup(index, header, aligns, rows, sizes):
    """Разметка таблицы и её столбцов."""
    widths = column_widths(header, rows)
    columns = []
    for position, width in enumerate(widths):
        columns.append(
            f'<style:style style:name="Co{index}_{position}"'
            ' style:family="table-column"><style:table-column-properties'
            f' style:column-width="{cm(width)}"/></style:style>')
    tags = ''.join(f'<table:table-column table:style-name="Co{index}_{position}"/>'
                   for position in range(len(widths)))
    head = ''
    cells = ''
    for cell in header:
        cells += ('<table:table-cell table:style-name="TblHeadCell"'
                  ' office:value-type="string"><text:p text:style-name="TableHead">'
                  f'{inline_markup(parse_inline(cell), sizes)}</text:p>'
                  '</table:table-cell>')
    head = f'<table:table-header-rows><table:table-row>{cells}</table:table-row>' \
           '</table:table-header-rows>'
    body = ''
    for row in rows:
        cells = ''
        for position, align in enumerate(aligns):
            value = row[position] if position < len(row) else ''
            name = {'left': 'TableLeft', 'right': 'TableRight'}.get(align, 'TableCenter')
            cells += (f'<table:table-cell table:style-name="Tbl"'
                      f' office:value-type="string">'
                      f'<text:p text:style-name="{name}">'
                      f'{inline_markup(parse_inline(value), sizes)}</text:p>'
                      '</table:table-cell>')
        body += f'<table:table-row>{cells}</table:table-row>'
    markup = (f'<table:table table:name="Table{index}" table:style-name="Tbl">'
              f'{head}{tags}{body}</table:table>'
              f'<text:p text:style-name="Spacer"/>')
    return markup, columns


# --- отчёт ----------------------------------------------------------------

def collect_formulas(blocks):
    """Уникальные формулы в порядке появления."""
    formulas = []

    def add(latex):
        latex = formula_key(latex)
        boxed = BOXED_RE.match(latex)
        if boxed:
            latex = formula_key(boxed.group(1))
        if latex and latex not in formulas:
            formulas.append(latex)

    def walk(parts):
        for part in parts:
            if part[0] == 'math':
                add(part[1])
            elif part[0] in ('bold', 'italic'):
                walk(parse_inline(part[1]))

    for block in blocks:
        kind = block[0]
        if kind == 'math':
            add(block[1])
        elif kind == 'p':
            walk(parse_inline(block[1]))
        elif kind == 'table':
            for cell in block[1]:
                walk(parse_inline(cell))
            for row in block[3]:
                for cell in row:
                    walk(parse_inline(cell))
        elif kind == 'list':
            for item in block[2]:
                walk(parse_inline(item))
    return formulas


def report_body(blocks, sizes):
    """Разметка тела отчёта и дополнительные стили."""
    body = ''
    extra = []
    previous = None
    seen_rule = False
    subtitle_done = False
    table_index = 0
    list_index = 0
    for block in blocks:
        kind = block[0]
        if kind == 'h':
            level, text = block[1], block[2]
            if level == 1:
                style = 'Title'
            elif level == 2 and not re.match(r'^\d', text) and not seen_rule \
                    and not subtitle_done:
                style, subtitle_done = 'Subtitle', True
            elif level == 2:
                style = 'Section'
            else:
                style = 'Subsection'
            body += (f'<text:p text:style-name="{style}">'
                     f'{inline_markup(parse_inline(text), sizes)}</text:p>')
        elif kind == 'p':
            parts = parse_inline(block[1])
            style = 'BodyFirst' if previous == 'h' else 'Body'
            if previous in ('h', 'hr') and parts and \
                    all(part[0] in ('bold', 'text') for part in parts) and \
                    any(part[0] == 'bold' for part in parts):
                style = 'Subtitle'
            body += (f'<text:p text:style-name="{style}">'
                     f'{inline_markup(parts, sizes)}</text:p>')
        elif kind == 'math':
            boxed = BOXED_RE.match(formula_key(block[1]))
            latex = boxed.group(1) if boxed else block[1]
            style = 'Boxed' if boxed else 'Formula'
            body += (f'<text:p text:style-name="{style}">'
                     f'{formula_frame(latex, sizes)}</text:p>')
        elif kind == 'code':
            body += (f'<text:p text:style-name="Code">'
                     f'{keep_spaces(block[1])}</text:p>')
        elif kind == 'table':
            _, header, aligns, rows = block
            markup, columns = table_markup(table_index, header, aligns, rows, sizes)
            body += markup
            extra += columns
            table_index += 1
        elif kind == 'list':
            body += list_markup(list_index, block[1], block[2], sizes)
            extra += list_styles(list_index, block[1])
            list_index += 1
        elif kind == 'hr':
            body += '<text:p text:style-name="Rule"/>'
            seen_rule = True
        previous = {'h': 'h', 'hr': 'hr'}.get(kind, 'p')
    return body, extra


def list_styles(index, ordered):
    name = f'List{index}'
    if ordered:
        return [f'<text:list-style style:name="{name}">'
                f'<text:list-level-style-number text:style-name="{name}.1"'
                ' style:num-format="1" style:num-suffix="." text:min-label-width="0.5cm"'
                ' text:label-followed-by="listtab" text:list-tab-stop-position="0.8cm"/>'
                '</text:list-style>']
    return [f'<text:list-style style:name="{name}">'
            f'<text:list-level-style-bullet text:style-name="{name}.1"'
            ' text:bullet-char="•" text:bullet-relative-size="100%"'
            ' text:min-label-width="0.4cm" text:label-followed-by="listtab"'
            ' text:list-tab-stop-position="0.8cm"/></text:list-style>']


def list_markup(index, ordered, items, sizes):
    name = f'List{index}'
    markup = ''
    for item in items:
        markup += ('<text:list-item><text:list-item-content>'
                   '<text:p text:style-name="ListItem">'
                   f'{inline_markup(parse_inline(item), sizes)}</text:p>'
                   '</text:list-item-content></text:list-item>')
    tag = 'text:ordered-list' if ordered else 'text:unordered-list'
    return (f'<{tag} text:style-name="{name}">{markup}</{tag}>'
            '<text:p text:style-name="Spacer"/>')


def build_report(source, work_dir, dpi=120):
    """Готовит FODT отчёта, возвращает путь к нему."""
    with open(source, encoding='utf8') as file:
        blocks = parse_markdown(file.read())
    formulas = collect_formulas(blocks)
    print(f'  блоков: {len(blocks)}, формул: {len(formulas)}')
    print('  измерение формул...')
    sizes = measure_formulas(formulas, work_dir, dpi=dpi)
    print(f'  измерено размеров: {len(sizes)}')
    body, extra = report_body(blocks, sizes)
    styles = report_styles() + character_styles() + extra
    fodt = os.path.join(work_dir, 'report.fodt')
    with open(fodt, 'w', encoding='utf8') as file:
        file.write(wrap_document(styles, body))
    return fodt


# --- листинг --------------------------------------------------------------

CS_KEYWORDS = set(
    'abstract as base bool break byte case catch char checked class const continue '
    'decimal default delegate do double else enum event explicit extern false finally '
    'fixed float for foreach goto if implicit in int interface internal is lock long '
    'namespace new null object operator out override params private protected public '
    'readonly record ref return sbyte sealed short sizeof stackalloc static string '
    'struct switch this throw true try typeof uint ulong unchecked unsafe ushort using '
    'var virtual void volatile while yield'.split())

CS_TYPES = set(
    'double int string bool char decimal float long Math List Func Action '
    'ArgumentException InvalidOperationException Exception IReadOnlyList IEnumerable '
    'Array Console'.split())


def highlight_cs(line):
    """Подсветка строки C#: комментарии, строки, ключевые слова, числа."""
    if not line.strip():
        return '<text:s/>'
    out = ''
    index = 0
    length = len(line)
    while index < length:
        if line.startswith('//', index):
            out += (f'<text:span text:style-name="CsComment">'
                    f'{escape(line[index:])}</text:span>')
            break
        char = line[index]
        if char == '"':
            stop = index + 1
            while stop < length:
                if line[stop] == '\\':
                    stop += 2
                    continue
                if line[stop] == '"':
                    stop += 1
                    break
                stop += 1
            out += (f'<text:span text:style-name="CsString">'
                    f'{escape(line[index:stop])}</text:span>')
            index = stop
            continue
        match = re.match(r'[A-Za-z_][A-Za-z_0-9]*', line[index:])
        if match:
            word = match.group(0)
            if word in CS_KEYWORDS:
                out += (f'<text:span text:style-name="CsKeyword">{escape(word)}'
                        '</text:span>')
            elif word in CS_TYPES:
                out += (f'<text:span text:style-name="CsType">{escape(word)}'
                        '</text:span>')
            else:
                out += escape(word)
            index += len(word)
            continue
        match = re.match(r'\d+(\.\d+)?([eE][+-]?\d+)?', line[index:])
        if match:
            out += (f'<text:span text:style-name="CsNumber">{escape(match.group(0))}'
                    '</text:span>')
            index += len(match.group(0))
            continue
        out += escape(char)
        index += 1
    return out


def listing_table(rows):
    """Строки кода таблицей: номер — код."""
    markup = ''
    for number, code in rows:
        markup += ('<table:table-row>'
                   '<table:table-cell table:style-name="LsTbl"'
                   ' office:value-type="string"><text:p text:style-name="LNumber">'
                   f'{number}</text:p></table:table-cell>'
                   '<table:table-cell table:style-name="LsTbl"'
                   ' office:value-type="string"><text:p text:style-name="LCode">'
                   f'{code}</text:p></table:table-cell></table:table-row>')
    return markup


def build_listing(folder, files, work_dir, title='Листинг программы'):
    """Готовит FODT листинга, возвращает путь к нему."""
    body = (f'<text:p text:style-name="LTitle">{escape(title)}</text:p>'
            '<text:p text:style-name="LInfo">Вариант 4. Решение систем линейных '
            'алгебраических уравнений. Листинг исходных файлов проекта в порядке '
            'сборки.</text:p>')
    total = 0
    for name in files:
        with open(os.path.join(folder, name), encoding='utf8') as file:
            lines = file.read().replace('\r\n', '\n').rstrip('\n').split('\n')
        total += len(lines)
        body += f'<text:p text:style-name="LFile">{escape(name)}</text:p>'
        rows = [(index + 1, highlight_cs(line))
                for index, line in enumerate(lines)]
        body += ('<table:table table:style-name="LsTbl">'
                 '<table:table-column table:style-name="ColNum"/>'
                 '<table:table-column table:style-name="ColCode"/>'
                 f'{listing_table(rows)}</table:table>'
                 '<text:p text:style-name="LInfo">'
                 f'{escape(name)} — строк: {len(lines)}</text:p>')
    body += (f'<text:p text:style-name="LInfo">Всего строк: {total}.</text:p>')
    fodt = os.path.join(work_dir, 'listing.fodt')
    with open(fodt, 'w', encoding='utf8') as file:
        file.write(wrap_document(listing_styles(), body))
    return fodt


# --- основной сценарий ---------------------------------------------------

def main():
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    lab = os.path.join(root, 'infapp', 'Lab1')
    parser = argparse.ArgumentParser(description='Сборка отчёта и листинга в PDF')
    parser.add_argument('--report', default=os.path.join(lab, 'Отчет.md'))
    parser.add_argument('--out-dir', default=lab)
    parser.add_argument('--work-dir', default=None)
    parser.add_argument('--dpi', type=int, default=120)
    parser.add_argument('--no-listing', action='store_true')
    options = parser.parse_args()

    work_dir = options.work_dir or tempfile.mkdtemp(prefix='md2pdf-')
    os.makedirs(work_dir, exist_ok=True)
    os.makedirs(options.out_dir, exist_ok=True)
    try:
        print('отчёт:')
        fodt = build_report(options.report, work_dir, dpi=options.dpi)
        report_pdf = run_soffice(fodt, work_dir)
        target = os.path.join(options.out_dir, 'Отчет.pdf')
        shutil.copyfile(report_pdf, target)
        print(f'  {target}')

        if not options.no_listing:
            print('листинг:')
            folder = os.path.dirname(os.path.abspath(options.report))
            found = sorted(name for name in os.listdir(folder)
                           if name.endswith('.cs'))
            order = ['Program.cs', 'LinearSystem.cs', 'GaussSolver.cs',
                     'IterationForm.cs', 'ZeidelSolver.cs', 'NumberFormat.cs']
            files = [name for name in order if name in found]
            files += [name for name in found if name not in order]
            fodt = build_listing(folder, files, work_dir)
            listing_pdf = run_soffice(fodt, work_dir)
            target = os.path.join(options.out_dir, 'Отчет_листинг.pdf')
            shutil.copyfile(listing_pdf, target)
            print(f'  {target}')
    finally:
        if options.work_dir is None:
            shutil.rmtree(work_dir, ignore_errors=True)
    return 0


if __name__ == '__main__':
    sys.exit(main())
