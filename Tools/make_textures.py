#!/usr/bin/env python3
"""Generates the extra surface textures used by the scripture-accuracy pass.

Run from the repository root:  python3 Tools/make_textures.py
Needs numpy, scipy and Pillow. Output goes to Assets/Resources/Art/.

All tiling textures are seamless by construction (periodic noise, integer thread counts).
They are albedo-only, like the existing maps: the Tabernacle/Physical shader derives its relief from them.
Curtain.png is built from your existing InnerVeil.png, so the cherubim keep exactly the same look.
"""
import os
import sys

import numpy as np
from PIL import Image
from scipy import ndimage

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Assets", "Resources", "Art")
N = 1024


def rng(seed):
    return np.random.default_rng(seed)


def periodic_noise(h, w, seed, beta=1.6, stretch=(1.0, 1.0)):
    """Seamless 1/f^beta noise in 0..1. stretch=(sy, sx) > 1 elongates features along that axis."""
    r = rng(seed)
    white = r.standard_normal((h, w))
    fy = np.fft.fftfreq(h)[:, None] * 1.0
    fx = np.fft.fftfreq(w)[None, :] * 1.0
    f = np.sqrt((fy * stretch[0]) ** 2 + (fx * stretch[1]) ** 2)
    f[0, 0] = 1.0
    spec = np.fft.fft2(white) / (f ** (beta / 1.0))
    spec[0, 0] = 0
    out = np.real(np.fft.ifft2(spec))
    out -= out.min()
    out /= max(out.max(), 1e-9)
    return out


def worley(n, cells, seed):
    """Seamless cellular noise. Returns F1 and F2 distances (in cell units)."""
    r = rng(seed)
    pts = r.random((cells, cells, 2))
    ys, xs = np.mgrid[0:n, 0:n].astype(np.float64)
    gy, gx = ys / n * cells, xs / n * cells
    cy, cx = np.floor(gy).astype(int), np.floor(gx).astype(int)
    f1 = np.full((n, n), 9.0)
    f2 = np.full((n, n), 9.0)
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            ny, nx = (cy + dy) % cells, (cx + dx) % cells
            py = (cy + dy) + pts[ny, nx, 0]
            px = (cx + dx) + pts[ny, nx, 1]
            d = np.sqrt((gy - py) ** 2 + (gx - px) ** 2)
            closer = d < f1
            f2 = np.where(closer, f1, np.minimum(f2, d))
            f1 = np.where(closer, d, f1)
    return f1, f2


def ramp(t, stops):
    """Piecewise-linear colour ramp. stops: [(pos, (r,g,b)), ...] with 0..1 colour values."""
    t = np.clip(t, 0, 1)
    pos = [s[0] for s in stops]
    out = np.zeros(t.shape + (3,))
    for c in range(3):
        out[..., c] = np.interp(t, pos, [s[1][c] for s in stops])
    return out


def to_img(a):
    return Image.fromarray((np.clip(a, 0, 1) * 255 + 0.5).astype(np.uint8), "RGB")


def save(img, name):
    path = os.path.join(OUT, name)
    img.save(path, optimize=True)
    print("wrote", os.path.relpath(path, ROOT), img.size)


def weave(h, w, threads_x, threads_y, seed, fibre_seed):
    """Plain weave. Returns (height 0..1, per-thread brightness, fibre noise)."""
    cx, cy = w / threads_x, h / threads_y
    ys, xs = np.mgrid[0:h, 0:w].astype(np.float64)
    ix, iy = np.floor(xs / cx).astype(int), np.floor(ys / cy).astype(int)
    fx, fy = (xs / cx) % 1.0, (ys / cy) % 1.0
    warp_on_top = ((ix + iy) % 2) == 0
    prof_x = np.sin(np.pi * fx) ** 0.75 * (0.72 + 0.28 * np.sin(np.pi * fy))
    prof_y = np.sin(np.pi * fy) ** 0.75 * (0.72 + 0.28 * np.sin(np.pi * fx))
    height = np.where(warp_on_top, prof_x, prof_y)
    r = rng(seed)
    var_x = 0.80 + 0.40 * r.random(threads_x)
    var_y = 0.80 + 0.40 * r.random(threads_y)
    thread = np.where(warp_on_top, var_x[ix % threads_x], var_y[iy % threads_y])
    fib_v = periodic_noise(h, w, fibre_seed, 1.2, (0.12, 1.0))
    fib_h = periodic_noise(h, w, fibre_seed + 1, 1.2, (1.0, 0.12))
    fibre = np.where(warp_on_top, fib_v, fib_h)
    return height, thread, fibre


# ---------------------------------------------------------------- acacia wood
def acacia():
    n = N
    ys, xs = np.mgrid[0:n, 0:n].astype(np.float64) / n
    warp = periodic_noise(n, n, 11, 2.8, (1.0, 0.5))
    # Growth rings as a sawtooth: pale early wood fading to a sharp dark line, with a second, finer set on top. Lines run vertically (along a post).
    ring1 = ((7 * xs + warp * 0.55) % 1.0) ** 2.2
    ring2 = ((23 * xs + warp * 0.9 + 0.3) % 1.0) ** 3.0
    grain_lines = (ring1 - 0.5) * 1.2 + (ring2 - 0.5) * 0.5
    fine = periodic_noise(n, n, 12, 1.0, (0.10, 1.0))               # long fibres along y
    pores = periodic_noise(n, n, 13, 0.6, (0.12, 1.0))
    pore_mask = np.clip((pores - 0.74) * 9, 0, 1)
    mottling = periodic_noise(n, n, 14, 2.6, (1.0, 1.0))
    t = 0.46 + 0.30 * grain_lines + 0.32 * (fine - 0.5) + 0.22 * (mottling - 0.5)
    col = ramp(t, [(0.0, (0.22, 0.13, 0.07)), (0.45, (0.44, 0.29, 0.16)), (1.0, (0.66, 0.47, 0.28))])
    col *= (1 - 0.35 * pore_mask)[..., None]
    save(to_img(col), "Acacia.png")


# ---------------------------------------------------------------- goat hair cloth
def goat_hair():
    n = N
    height, thread, fibre = weave(n, n, 56, 56, 21, 22)
    shade = (0.35 + 0.75 * height ** 0.8) * thread * (0.75 + 0.5 * fibre)
    mott = periodic_noise(n, n, 23, 2.4)
    base = ramp(0.25 + 0.5 * (mott - 0.5) + 0.5 * (fibre - 0.5), [(0.0, (0.07, 0.05, 0.04)), (0.5, (0.19, 0.14, 0.10)), (1.0, (0.33, 0.25, 0.18))])
    col = base * shade[..., None] * 1.15
    # stray hairs: thin bright streaks
    hairs = periodic_noise(n, n, 24, 0.5, (0.04, 1.0))
    col += (np.clip((hairs - 0.80) * 6, 0, 1) * 0.18)[..., None] * np.array([0.8, 0.65, 0.5])
    save(to_img(col), "GoatHair.png")


# ---------------------------------------------------------------- leather
def leather(name, cells, palette, seed, crease_strength, pore_strength):
    n = N
    f1, f2 = worley(n, cells, seed)
    edge = ndimage.gaussian_filter(np.clip(1 - (f2 - f1) * 4.0, 0, 1), 1.6)   # soft creases along cell borders
    puffy = np.clip(f1 * 1.6, 0, 1)
    mott = periodic_noise(n, n, seed + 1, 2.6)
    fine = periodic_noise(n, n, seed + 2, 0.9)
    t = 0.50 + 0.55 * (mott - 0.5) + 0.18 * (fine - 0.5) - 0.20 * edge * crease_strength + 0.10 * (0.5 - puffy)
    col = ramp(t, palette)
    pores = worley(n, cells * 4, seed + 3)[0]
    pore = np.clip((0.16 - pores) * 7, 0, 1) * pore_strength
    col *= (1 - 0.45 * pore)[..., None]
    col *= (1 - 0.35 * edge * crease_strength)[..., None]
    save(to_img(col), name)


def ram_skin():
    leather("RamSkin.png", 40,
            [(0.0, (0.25, 0.04, 0.03)), (0.5, (0.50, 0.11, 0.08)), (1.0, (0.70, 0.22, 0.15))],
            31, 0.40, 0.8)


def tahash():
    # What "tahash" means is uncertain (sea-cow, a fine leather, ...). This is a tough, dull grey-brown hide, kept deliberately generic.
    leather("Tahash.png", 22,
            [(0.0, (0.17, 0.15, 0.13)), (0.5, (0.34, 0.30, 0.26)), (1.0, (0.50, 0.45, 0.39))],
            41, 0.45, 0.6)


# ---------------------------------------------------------------- blue, purple, scarlet gate / door screen
def gate_screen():
    n = N
    period = n // 2                                   # pattern repeats twice across the tile (seamless horizontally)
    bands = [  # (width px of 512, base colour, thread colour for embroidery)
        (150, (0.06, 0.15, 0.34), (0.38, 0.52, 0.80)),
        (12, (0.84, 0.79, 0.66), None),
        (150, (0.27, 0.09, 0.31), (0.66, 0.44, 0.70)),
        (12, (0.84, 0.79, 0.66), None),
        (150, (0.55, 0.07, 0.06), (0.90, 0.38, 0.28)),
        (12, (0.84, 0.79, 0.66), None),
    ]
    used = sum(b[0] for b in bands)
    bands.append((period - used, (0.84, 0.79, 0.66), None))    # remaining width is plain linen
    base = np.zeros((n, n, 3))
    accent = np.zeros((n, n, 3))
    has_accent = np.zeros((n, n), bool)
    local_x = np.zeros((n, n))
    band_w = np.ones((n, n))
    for rep in range(2):
        x0 = rep * period
        for width, col, acc in bands:
            base[:, x0:x0 + width, :] = col
            if acc is not None:
                accent[:, x0:x0 + width, :] = acc
                has_accent[:, x0:x0 + width] = True
                local_x[:, x0:x0 + width] = (np.arange(width) - (width - 1) / 2)[None, :]
                band_w[:, x0:x0 + width] = width
            x0 += width
    ys = np.arange(n)[:, None].astype(np.float64)
    # Diamond lattice embroidered down the middle of each coloured band.
    p = 128.0
    dx = np.abs(local_x) / (p / 2)
    dy = np.abs((ys % p) - p / 2) / (p / 2)
    d = dx + dy
    outline = (np.abs(d - 0.80) < 0.07) & (np.abs(local_x) < band_w / 2 - 14)
    dot = (d < 0.22) & (np.abs(local_x) < band_w / 2 - 14)
    stitch = (outline | dot) & has_accent
    # Narrow edge lines in the band's accent colour.
    edge = (np.abs(np.abs(local_x) - (band_w / 2 - 6)) < 2) & has_accent
    col = np.where((stitch | edge)[..., None], accent, base)
    height, thread, fibre = weave(n, n, 192, 192, 51, 52)
    shade = (0.50 + 0.62 * height ** 0.8) * thread * (0.88 + 0.24 * fibre)
    col = col * shade[..., None]
    save(to_img(col), "GateScreen.png")


# ---------------------------------------------------------------- ceiling curtain (cherubim), built from the existing veil art
def curtain():
    veil_path = os.path.join(OUT, "InnerVeil.png")
    if not os.path.exists(veil_path):
        print("InnerVeil.png not found; skipping Curtain.png")
        return
    veil = np.asarray(Image.open(veil_path).convert("RGB")).astype(np.float64) / 255.0
    W, H = 1280, 512
    # Indigo linen ground, matched to the veil's own background colour.
    bg_patch = veil[420:1100, 575:690]
    bg_mean = np.median(bg_patch.reshape(-1, 3), axis=0)
    height, thread, fibre = weave(H, W, 320, 128, 61, 62)
    ground = bg_mean[None, None, :] * (0.62 + 0.55 * height ** 0.8)[..., None] * thread[..., None] * (0.90 + 0.2 * fibre)[..., None]
    # Cut the embroidery out of the veil by colour distance from the ground.
    box = (100, 100, 1158, 1168)
    crop = veil[box[1]:box[3], box[0]:box[2]]
    dist = np.sqrt(((crop - bg_mean[None, None, :]) ** 2).sum(axis=2))
    local_bg = ndimage.gaussian_filter(dist, 1.2)
    mask = np.clip((local_bg - 0.10) / 0.12, 0, 1)
    mask = ndimage.grey_closing(mask, size=(5, 5))               # fill small gaps inside the figures
    mask = ndimage.gaussian_filter(mask, 0.8)
    target_h = 456
    scale = target_h / crop.shape[0]
    new_w = int(round(crop.shape[1] * scale))
    rgb = np.asarray(Image.fromarray((crop * 255).astype(np.uint8)).resize((new_w, target_h), Image.LANCZOS)).astype(np.float64) / 255.0
    msk = np.asarray(Image.fromarray((mask * 255).astype(np.uint8)).resize((new_w, target_h), Image.LANCZOS)).astype(np.float64) / 255.0
    canvas = ground.copy()
    for centre in (W * 0.25, W * 0.75):
        x0 = int(centre - new_w / 2)
        y0 = (H - target_h) // 2
        a = msk[..., None]
        canvas[y0:y0 + target_h, x0:x0 + new_w] = canvas[y0:y0 + target_h, x0:x0 + new_w] * (1 - a) + rgb * a
    # Blue, purple and scarlet edge bands along the long sides, in the same order the Bible lists the yarns (blue, purple, scarlet).
    for i, c in enumerate([(0.07, 0.17, 0.36), (0.30, 0.10, 0.34), (0.58, 0.08, 0.07)]):
        for y in range(i * 5, i * 5 + 5):
            canvas[y, :, :] = np.asarray(c)[None, :] * (0.7 + 0.5 * height[y, :] ** 0.8)[:, None]
            canvas[H - 1 - y, :, :] = np.asarray(c)[None, :] * (0.7 + 0.5 * height[H - 1 - y, :] ** 0.8)[:, None]
    img = to_img(canvas).resize((1024, 512), Image.LANCZOS)    # squeeze 2.5:1 art into 2:1; the 10x4-cubit panel stretches it back
    save(img, "Curtain.png")


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    which = sys.argv[1:] or ["acacia", "goat", "ram", "tahash", "gate", "curtain"]
    table = {"acacia": acacia, "goat": goat_hair, "ram": ram_skin, "tahash": tahash, "gate": gate_screen, "curtain": curtain}
    for key in which:
        table[key]()
