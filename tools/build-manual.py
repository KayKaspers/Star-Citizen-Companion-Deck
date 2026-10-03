"""Erzeugt das deutsche Handbuch aus der versionierten Markdown-Quelle."""
from pathlib import Path
import re
from xml.sax.saxutils import escape
from reportlab.pdfgen import canvas
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, PageBreak, Image
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.colors import HexColor
from reportlab.lib.enums import TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'docs' / 'Benutzerhandbuch.pdf'
# Windows' Arial is embedded; avoids variable-font limitations in ReportLab.
pdfmetrics.registerFont(TTFont('Handbuch', 'C:/Windows/Fonts/arial.ttf'))
pdfmetrics.registerFont(TTFont('HandbuchBold', 'C:/Windows/Fonts/arialbd.ttf'))
styles = {
    'body': ParagraphStyle('body', fontName='Handbuch', fontSize=10.5, leading=15, textColor=HexColor('#243746'), spaceAfter=10),
    'title': ParagraphStyle('title', fontName='HandbuchBold', fontSize=29, leading=34, textColor=HexColor('#102b3b'), spaceAfter=18),
    'h1': ParagraphStyle('h1', fontName='HandbuchBold', fontSize=22, leading=27, textColor=HexColor('#102b3b'), spaceAfter=20),
    'h2': ParagraphStyle('h2', fontName='HandbuchBold', fontSize=12.5, leading=17, textColor=HexColor('#147b86'), spaceBefore=5, spaceAfter=7),
}

def linked(text):
    text = escape(text)
    return re.sub(r'https://[^\s<]+', lambda m: f'<link href="{m[0]}" color="#147b86">{m[0]}</link>', text)

def page_frame(c, doc):
    w, h = A4
    c.setFillColor(HexColor('#102b3b')); c.rect(0, h-14, w, 14, fill=1, stroke=0)
    c.setFillColor(HexColor('#147b86')); c.setFont('HandbuchBold', 8)
    c.drawString(46, h-38, 'BLACKHOLE DYNAMICS / STAR CITIZEN BEGLEITER-DECK')
    c.setStrokeColor(HexColor('#d7e5e9')); c.line(46, 43, w-46, 43)
    c.setFillColor(HexColor('#546b78')); c.setFont('Handbuch', 8)
    c.drawString(46, 29, 'Benutzerhandbuch | Version 0.1.0 | Deutsch')
    c.drawRightString(w-46, 29, f'{doc.page} / 7')

source = (ROOT / 'docs' / 'BENUTZERHANDBUCH.md').read_text(encoding='utf-8')
source = re.sub(r'\n(?=#{1,3} )', '\n\n', source)
story = []
def body(text):
    lines = text.splitlines() if re.match(r'\d\. ', text) or text.startswith('Repository:') else [text.replace('\n', ' ')]
    for line in lines:
        story.append(Paragraph(linked(line), styles['body']))

for block in re.split(r'\n\s*\n', source.strip()):
    if block.startswith('# '):
        story.append(Spacer(1, 30))
        story.append(Image(str(ROOT / 'branding/assets/banner-v2.png'), width=503, height=126))
        story.append(Spacer(1, 36))
        lines = block.splitlines()
        story.append(Paragraph(linked(lines[0][2:]), styles['title']))
        if len(lines) > 1: story.append(Paragraph(linked(' '.join(lines[1:])), styles['h2']))
    elif block.startswith('## '):
        story.append(PageBreak()); story.append(Paragraph(linked(block[3:]), styles['h1']))
    elif block.startswith('### '):
        lines = block.splitlines()
        story.append(Paragraph(linked(lines[0][4:]), styles['h2']))
        if len(lines)>1: body('\n'.join(lines[1:]))
    else:
        body(block)
doc = SimpleDocTemplate(str(OUT), pagesize=A4, leftMargin=46, rightMargin=46, topMargin=65, bottomMargin=58,
                        title='Star Citizen Begleiter-Deck - Benutzerhandbuch', author='Blackhole Dynamics / KayKaspers')
doc.build(story, onFirstPage=page_frame, onLaterPages=page_frame)
print(OUT)
