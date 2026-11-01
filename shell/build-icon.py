"""Render the gold reversed-R vector master into Windows/DPI icon sizes.

Requires Pillow. Run from any directory: python shell\\build-icon.py
"""
import io
import pathlib
import re
import struct
import xml.etree.ElementTree as ET

from PIL import Image, ImageDraw

ROOT = pathlib.Path(__file__).resolve().parent
SIZES = (16, 20, 24, 32, 40, 48, 64, 96, 128, 256)
SCALE = 8
master = ET.parse(ROOT / "app-icon.svg").getroot()
namespace = {"svg": "http://www.w3.org/2000/svg"}
stops = master.findall(".//svg:stop", namespace)
colors = [tuple(bytes.fromhex(stop.attrib["stop-color"][1:])) for stop in stops]
mask = Image.new("L", (256 * SCALE, 256 * SCALE))
draw = ImageDraw.Draw(mask)
for index, rect in enumerate(master.findall("svg:rect", namespace)):
    x, y = float(rect.get("x", 0)), float(rect.get("y", 0))
    w, h, radius = (float(rect.get(name)) for name in ("width", "height", "rx"))
    draw.rounded_rectangle((x * SCALE, y * SCALE, (x + w) * SCALE - 1,
                            (y + h) * SCALE - 1), radius * SCALE, fill=255 if index == 0 else 0)
tokens = re.findall(r"[MLCZ]|-?\d+(?:\.\d+)?", master.find("svg:path", namespace).get("d"))
position = 0
contours = []
points = []
while position < len(tokens):
    command = tokens[position]
    position += 1
    if command == "Z":
        contours.append(points)
        points = []
        continue
    count = 6 if command == "C" else 2
    values = [float(value) for value in tokens[position:position + count]]
    position += count
    if command in ("M", "L"):
        points.append(tuple(values))
    elif command == "C":
        start = points[-1]
        for step in range(1, 65):
            t = step / 64
            points.append(tuple((1 - t) ** 3 * start[axis] +
                                3 * (1 - t) ** 2 * t * values[axis] +
                                3 * (1 - t) * t ** 2 * values[axis + 2] +
                                t ** 3 * values[axis + 4] for axis in (0, 1)))
    else:
        raise ValueError(f"Unsupported vector command: {command}")
for index, contour in enumerate(contours):
    draw.polygon([(x * SCALE, y * SCALE) for x, y in contour], fill=255 if index == 0 else 0)
art = Image.new("RGB", mask.size, "#0c0b10")
gradient = Image.new("RGB", mask.size)
gradient_draw = ImageDraw.Draw(gradient)
for y in range(mask.height):
    t = y / (mask.height - 1)
    color = tuple(round(a * (1 - t) + b * t) for a, b in zip(*colors))
    gradient_draw.line((0, y, mask.width, y), fill=color)
art.paste(gradient, mask=mask)
alpha = Image.new("L", mask.size)
ImageDraw.Draw(alpha).rounded_rectangle((0, 0, mask.width - 1, mask.height - 1),
                                       30 * SCALE, fill=255)
art.putalpha(alpha)
art.resize((1024, 1024), Image.Resampling.LANCZOS).save(ROOT / "logo.png")
entries = []
for size in SIZES:
    buffer = io.BytesIO()
    art.resize((size, size), Image.Resampling.LANCZOS).save(buffer, format="PNG")
    entries.append((size, buffer.getvalue()))
offset = 6 + 16 * len(entries)
header = bytearray(struct.pack("<HHH", 0, 1, len(entries)))
for size, data in entries:
    header.extend(struct.pack("<BBBBHHII", size % 256, size % 256, 0, 0, 1, 32, len(data), offset))
    offset += len(data)
(ROOT / "app.ico").write_bytes(header + b"".join(data for _, data in entries))
print(f"Rendered {len(SIZES)} icon sizes and 1024px logo from app-icon.svg.")
