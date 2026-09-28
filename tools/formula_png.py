"""Рендер формул в PNG средствами LibreOffice.

Формула в Writer всегда растягивается под размер draw:frame, поэтому натуральный
размер там измерить нельзя. Здесь каждая формула рендерится отдельным
документом Math и обрезается по границам чёрных пикселей.

Структура страницы Math (все три блока одинаковы для всех формул):
    полоса 1  — заголовок документа,
    полоса 2  — сама формула (натурального размера),
    полоса 3  — исходный текст StarMath.
Полосы разделены линиями во всю ширину текста, поэтому формула измеряется
только внутри средней полосы.
"""

import contextlib
import hashlib
import json
import os
import subprocess
import sys
import time

MATHML_NS = 'http://www.w3.org/1998/Math/MathML'
DPI = 300
INK_LIMIT = 200
INK = bytes(1 if value < INK_LIMIT else 0 for value in range(256))

_TEX2MATHML = None


def _converter():
    global _TEX2MATHML
    if _TEX2MATHML is None:
        sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
        import tex2mathml
        _TEX2MATHML = tex2mathml
    return _TEX2MATHML


def mathml(latex):
    """MathML формулы.

    Содержимое обязательно обёрнуто в mrow: LibreOffice считает каждого прямого
    потомка math отдельной строкой и вставляет между ними newline, из-за чего
    формула распадается на несколько строк.
    """
    inner = _converter().latex_to_mathml(latex)
    return (f'<math xmlns="{MATHML_NS}" xmlns:mml="{MATHML_NS}" display="inline">'
            f'<mrow>{inner}</mrow></math>')


def cache_name(latex):
    return hashlib.sha1(latex.encode('utf8')).hexdigest()[:16]


def read_pgm(path):
    """Ширина, высота и байты пикселей серого PGM."""
    with open(path, 'rb') as file:
        data = file.read()
    fields, index = [], 2
    while len(fields) < 3:
        while index < len(data) and data[index:index + 1].isspace():
            index += 1
        if data[index:index + 1] == b'#':
            while data[index:index + 1] not in (b'\n', b''):
                index += 1
            continue
        start = index
        while index < len(data) and not data[index:index + 1].isspace():
            index += 1
        fields.append(int(data[start:index]))
    width, height, _ = fields
    return width, height, data[index + 1:index + 1 + width * height]


def ink_box(dpi=DPI):
    """Растр в виде булева байта: 1 — чёрный пиксель."""
    return dpi


def formula_box(width, height, pixels):
    """Границы формулы в пикселях или None."""
    ink = pixels.translate(INK)
    rows = [r for r in range(height) if ink[r * width:(r + 1) * width].count(1)]
    if not rows:
        return None
    cols = [c for c in range(width) if any(ink[r * width + c] for r in rows)]
    full_width = cols[-1] - cols[0] + 1
    full = sorted(r for r in rows
                  if ink[r * width:(r + 1) * width].count(1) > 0.9 * full_width)
    if len(full) < 4:
        return None
    top, bottom = full[2], full[3]
    span = bottom - top + 1
    box_rows = [r for r in range(top, bottom + 1)
                if 3 <= ink[r * width:(r + 1) * width].count(1) < 0.9 * full_width]
    if not box_rows:
        return None
    box_cols = [c for c in range(width)
                if 3 <= sum(ink[r * width + c] for r in range(top, bottom + 1)) < 0.5 * span]
    if not box_cols:
        return None
    return min(box_cols), min(box_rows), max(box_cols), max(box_rows)


@contextlib.contextmanager
def soffice(port=2030, profile='/tmp/opencode/lo_profile_formula'):
    """Запущенный LibreOffice и его desktop."""
    import uno
    from com.sun.star.beans import PropertyValue

    def prop(name, value):
        item = PropertyValue()
        item.Name, item.Value = name, value
        return item

    server = subprocess.Popen(
        ['soffice', '--headless', '--norestore', '--invisible',
         f'-env:UserInstallation=file://{profile}',
         f'--accept=socket,host=127.0.0.1,port={port};urp;'],
        stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    local = uno.getComponentContext()
    resolver = local.ServiceManager.createInstanceWithContext(
        'com.sun.star.bridge.UnoUrlResolver', local)
    context = None
    for _ in range(90):
        try:
            context = resolver.resolve(
                f'uno:socket,host=127.0.0.1,port={port};urp;StarOffice.ComponentContext')
            break
        except Exception:
            time.sleep(1)
    if context is None:
        server.terminate()
        raise RuntimeError('LibreOffice не запустился')
    desktop = context.ServiceManager.createInstanceWithContext(
        'com.sun.star.frame.Desktop', context)
    try:
        yield desktop, prop
    finally:
        with contextlib.suppress(Exception):
            desktop.terminate()
        with contextlib.suppress(Exception):
            server.wait(timeout=30)


def _measure(pdf, prefix, dpi=DPI):
    """Границы формулы в пикселях растра."""
    subprocess.run(['pdftoppm', '-gray', '-r', str(dpi), '-f', '1', '-l', '1',
                    pdf, prefix], check=True, capture_output=True)
    width, height, pixels = read_pgm(f'{prefix}-1.pgm')
    box = formula_box(width, height, pixels)
    return box, prefix


def _crop_png(pdf, prefix, box, target, dpi=DPI):
    """Обрезанный по границам формулы PNG."""
    left, top, right, bottom = box
    subprocess.run(['pdftoppm', '-png', '-gray', '-r', str(dpi), '-f', '1', '-l', '1',
                    '-x', str(left), '-y', str(top),
                    '-W', str(right - left + 1), '-H', str(bottom - top + 1),
                    pdf, target[:-4]], check=True, capture_output=True)
    for name in sorted(os.listdir(os.path.dirname(target) or '.')):
        if name.startswith(os.path.basename(target)[:-4]) and name.endswith('.png'):
            path = os.path.join(os.path.dirname(target) or '.', name)
            os.replace(path, target)
            return target
    raise RuntimeError(f'PNG не создан: {target}')


def render(formulas, work_dir, dpi=DPI, port=2030, log=True):
    """PNG и натуральный размер каждой формулы.

    Возвращает {ключ формулы: (png, ширина_пт, высота_пт)}.
    """
    cache = os.path.join(work_dir, 'formula_png')
    os.makedirs(cache, exist_ok=True)
    result, missing = {}, []
    for latex in formulas:
        target = os.path.join(cache, cache_name(latex) + '.png')
        if os.path.exists(target):
            with open(target, 'rb') as file:
                data = file.read()
            width = int.from_bytes(data[16:20], 'big')
            height = int.from_bytes(data[20:24], 'big')
            result[latex] = (data, round(width * 72.0 / dpi, 2),
                             round(height * 72.0 / dpi, 2))
        else:
            missing.append(latex)
    if not missing:
        return result
    if log:
        print(f'  рендер формул в PNG: {len(missing)} из {len(formulas)}', file=sys.stderr)
    work = os.path.join(work_dir, 'formula_png_work')
    os.makedirs(work, exist_ok=True)
    broken = []
    with soffice(port=port) as (desktop, prop):
        source_prop = prop('FilterName', 'MathML XML (Math)')
        pdf_prop = prop('FilterName', 'math_pdf_Export')
        for index, latex in enumerate(missing):
            name = cache_name(latex)
            mml_path = os.path.join(work, name + '.mml')
            with open(mml_path, 'w', encoding='utf8') as file:
                file.write('<?xml version="1.0" encoding="UTF-8"?>\n' + mathml(latex))
            try:
                imported = desktop.loadComponentFromURL(
                    'file://' + mml_path, '_blank', 0, (source_prop,))
                starmath = imported.Formula
                imported.close(False)
                document = desktop.loadComponentFromURL(
                    'private:factory/smath', '_blank', 0, ())
                document.Formula = starmath
                pdf_path = os.path.join(work, name + '.pdf')
                document.storeToURL('file://' + pdf_path, (pdf_prop,))
                document.close(False)
                box, prefix = _measure(pdf_path, os.path.join(work, name), dpi=dpi)
                if box is None:
                    broken.append(latex)
                    continue
                target = os.path.join(cache, name + '.png')
                _crop_png(pdf_path, prefix, box, target, dpi=dpi)
                with open(target, 'rb') as file:
                    data = file.read()
                width = box[2] - box[0] + 1
                height = box[3] - box[1] + 1
                result[latex] = (data, round(width * 72.0 / dpi, 2),
                                 round(height * 72.0 / dpi, 2))
            except Exception as error:                      # noqa: BLE001
                print(f'  ! {latex[:48]}: {error}', file=sys.stderr)
                broken.append(latex)
            if log and (index + 1) % 20 == 0:
                print(f'    {index + 1}/{len(missing)}', file=sys.stderr)
    with open(os.path.join(cache, 'broken.json'), 'w', encoding='utf8') as file:
        json.dump(broken, file, ensure_ascii=False, indent=1)
    return result
