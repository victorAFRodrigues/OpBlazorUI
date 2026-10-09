// Infra de overlays do OpBlazorUI.
//
// Replica o que o PrimeNG/Optimus faz em runtime e que não está no CSS dos temas:
//  - ZIndexUtils: z-index crescente por camada (modal, overlay, menu, tooltip);
//  - posicionamento conectado: o painel é posicionado (position: fixed) a partir do
//    elemento âncora, inverte o lado quando falta espaço e acompanha scroll/resize.
//
// O DOM não é movido (o Blazor continua dono dos nós); `position: fixed` já escapa do
// corte por `overflow` dos containers. Os estilos inline aplicados aqui só persistem
// porque os componentes não renderizam o atributo `style` nesses elementos.

const LAYERS = { modal: 1100, overlay: 1000, menu: 1000, tooltip: 1100 };
const VIEWPORT_MARGIN = 8;

// ---------------------------------------------------------------- z-index

const zStack = [];

function zIndexSet(el, key, baseZIndex) {
    zIndexClear(el);
    const base = (baseZIndex || 0) + (LAYERS[key] ?? LAYERS.overlay);
    const last = zStack.length > 0 ? zStack[zStack.length - 1] : { key, value: base };
    // Mesma regra do ZIndexUtils: um overlay aberto sobre outra camada (ex.: select dentro
    // de um dialog) fica acima dela.
    const value = last.value + (last.key === key ? 0 : base) + 2;
    zStack.push({ el, key, value });
    el.style.zIndex = String(value);
    return value;
}

function zIndexClear(el) {
    const i = zStack.findIndex(z => z.el === el);
    if (i >= 0) zStack.splice(i, 1);
}

export function setZIndex(el, key = 'overlay', baseZIndex = 0) {
    if (!el) return 0;
    return zIndexSet(el, key, baseZIndex);
}

export function clearZIndex(el) {
    if (!el) return;
    zIndexClear(el);
}

// ---------------------------------------------------------------- posicionamento

// Ancestral que vira containing block de elementos fixed (transform, filter, etc.).
// Nesse caso as coordenadas do viewport precisam ser compensadas.
function fixedContainingBlock(el) {
    for (let p = el.parentElement; p && p !== document.body; p = p.parentElement) {
        const s = getComputedStyle(p);
        if (s.transform !== 'none' || s.perspective !== 'none' || s.filter !== 'none' ||
            (s.backdropFilter && s.backdropFilter !== 'none') ||
            /paint|layout|strict|content/.test(s.contain) ||
            /transform|perspective|filter/.test(s.willChange)) {
            return p;
        }
    }
    return null;
}

function clamp(v, min, max) {
    return Math.max(min, Math.min(v, max));
}

function position(state) {
    const { el, anchor, opts } = state;
    if (!el.isConnected || !anchor.isConnected) return;

    const a = anchor.getBoundingClientRect();
    const vw = document.documentElement.clientWidth;
    const vh = document.documentElement.clientHeight;

    if (opts.matchWidth) el.style.minWidth = a.width + 'px';

    const w = el.offsetWidth;
    const h = el.offsetHeight;
    const gap = opts.gap || 0;
    const side = opts.placement || 'bottom';
    const vertical = side === 'bottom' || side === 'top';
    let placed = side;
    let top;
    let left;

    if (vertical) {
        const below = a.bottom + gap;
        const above = a.top - gap - h;
        const fitsBelow = below + h <= vh - VIEWPORT_MARGIN;
        const fitsAbove = above >= VIEWPORT_MARGIN;
        if (side === 'bottom') {
            placed = !fitsBelow && fitsAbove && opts.flip !== false ? 'top' : 'bottom';
        } else {
            placed = !fitsAbove && fitsBelow && opts.flip !== false ? 'bottom' : 'top';
        }
        top = placed === 'bottom' ? below : above;
        left = opts.align === 'center' ? a.left + (a.width - w) / 2 : a.left;
        left = clamp(left, VIEWPORT_MARGIN, Math.max(VIEWPORT_MARGIN, vw - w - VIEWPORT_MARGIN));
    } else {
        const right = a.right + gap;
        const leftSide = a.left - gap - w;
        const fitsRight = right + w <= vw - VIEWPORT_MARGIN;
        const fitsLeft = leftSide >= VIEWPORT_MARGIN;
        if (side === 'right') {
            placed = !fitsRight && fitsLeft && opts.flip !== false ? 'left' : 'right';
        } else {
            placed = !fitsLeft && fitsRight && opts.flip !== false ? 'right' : 'left';
        }
        left = placed === 'right' ? right : leftSide;
        top = a.top + (a.height - h) / 2;
        top = clamp(top, VIEWPORT_MARGIN, Math.max(VIEWPORT_MARGIN, vh - h - VIEWPORT_MARGIN));
    }

    const cb = state.containingBlock;
    if (cb) {
        const r = cb.getBoundingClientRect();
        top -= r.top;
        left -= r.left;
    }

    el.style.top = top + 'px';
    el.style.left = left + 'px';
    el.style.transformOrigin = placed === 'top' ? 'bottom' : placed === 'bottom' ? 'top' : 'center';
    el.setAttribute('data-op-placement', placed);

    if (opts.arrow === 'popover') alignPopoverArrow(el, a, left + (cb ? cb.getBoundingClientRect().left : 0), placed);
    else if (opts.arrow === 'tooltip') alignTooltipArrow(state, a, top + (cb ? cb.getBoundingClientRect().top : 0), left + (cb ? cb.getBoundingClientRect().left : 0), placed);
}

function cssLengthToPx(value, el) {
    const v = (value || '').trim();
    const n = parseFloat(v);
    if (Number.isNaN(n)) return 0;
    if (v.endsWith('rem')) return n * parseFloat(getComputedStyle(document.documentElement).fontSize);
    if (v.endsWith('em')) return n * parseFloat(getComputedStyle(el).fontSize);
    return n;
}

// Popover: o tema desenha a seta com left: calc(--p-popover-arrow-offset + --p-popover-arrow-left)
// e usa .p-popover-flipped quando o painel abre acima do alvo. A seta aponta para o centro do
// alvo (o oficial aponta para a borda esquerda + offset, o que parece descentralizado em alvos largos).
function alignPopoverArrow(el, a, left, placed) {
    const cs = getComputedStyle(el);
    const offset = cssLengthToPx(cs.getPropertyValue('--p-popover-arrow-offset'), el);
    const gutter = cssLengthToPx(cs.getPropertyValue("--p-popover-gutter"), el);
    const radius = parseFloat(cs.borderTopLeftRadius) || 0;
    // Mantém a seta dentro da parte reta da borda.
    const min = radius + gutter - offset;
    const max = el.offsetWidth - radius - gutter - offset;
    const arrowLeft = clamp(a.left + a.width / 2 - left - offset, min, Math.max(min, max));
    el.style.setProperty('--p-popover-arrow-left', arrowLeft + 'px');
    el.classList.toggle('p-popover-flipped', placed === 'top');
}

// Tooltip: a classe de posição (p-tooltip-top/right/...) define o lado da seta; a seta é
// centralizada no alvo, mesmo quando o tooltip foi deslocado para caber no viewport.
function alignTooltipArrow(state, a, top, left, placed) {
    const el = state.el;
    for (const s of ['top', 'bottom', 'left', 'right']) el.classList.toggle('p-tooltip-' + s, s === placed);
    const arrow = el.querySelector('.p-tooltip-arrow');
    if (!arrow) return;
    if (placed === 'top' || placed === 'bottom') {
        arrow.style.left = (a.left + a.width / 2 - left) + 'px';
        arrow.style.top = placed === 'bottom' ? '0' : '';
        arrow.style.bottom = placed === 'top' ? '0' : '';
        arrow.style.right = '';
    } else {
        arrow.style.top = (a.top + a.height / 2 - top) + 'px';
        arrow.style.left = placed === 'right' ? '0' : '';
        arrow.style.right = placed === 'left' ? '0' : '';
        arrow.style.bottom = '';
    }
}

const attached = new Map();

/**
 * Conecta `el` (o painel) ao `anchor`.
 * opts: { layer, baseZIndex, positioned, placement, align, matchWidth, gap, flip, arrow }
 */
export function attach(el, anchor, opts = {}) {
    if (!el) return;
    detach(el);

    const state = { el, anchor, opts, raf: 0 };
    attached.set(el, state);
    if (opts.autoZIndex !== false) {
        const z = setZIndex(el, opts.layer || 'overlay', opts.baseZIndex || 0);
        // Máscara irmã (Drawer): logo abaixo do painel, como o enableModality do upstream.
        if (opts.maskPrevious && el.previousElementSibling) el.previousElementSibling.style.zIndex = String(z - 1);
    }

    if (opts.positioned === false || !anchor) return;

    el.style.position = 'fixed';
    el.style.margin = opts.keepMargin ? '' : '0';
    el.style.transform = opts.keepTransform ? '' : 'none';
    state.containingBlock = fixedContainingBlock(el);

    const schedule = () => {
        if (state.raf) return;
        state.raf = requestAnimationFrame(() => {
            state.raf = 0;
            position(state);
        });
    };
    state.onScroll = schedule;
    window.addEventListener('scroll', schedule, { capture: true, passive: true });
    window.addEventListener('resize', schedule, { passive: true });
    if (typeof ResizeObserver !== 'undefined') {
        state.ro = new ResizeObserver(schedule);
        state.ro.observe(el);
        state.ro.observe(anchor);
    }

    position(state);
}

export function update(el) {
    const state = attached.get(el);
    if (state && state.opts.positioned !== false && state.anchor) position(state);
}

export function detach(el) {
    const state = attached.get(el);
    if (!state) return;
    attached.delete(el);
    clearZIndex(el);
    if (state.raf) cancelAnimationFrame(state.raf);
    if (state.onScroll) {
        window.removeEventListener('scroll', state.onScroll, { capture: true });
        window.removeEventListener('resize', state.onScroll);
    }
    if (state.ro) state.ro.disconnect();
}

// ---------------------------------------------------------------- dismiss (clique fora / Escape)
//
// Um único par de listeners (window, fase de captura) atende todos os overlays abertos, na
// ordem em que foram abertos (o último é o do topo):
//  - Escape vai só para o overlay do topo que aceita Escape e não propaga: um Escape num
//    Select dentro de um Dialog fecha só o Select.
//  - pointerdown fora do painel, da âncora e de qualquer overlay aberto acima dele fecha o
//    overlay. Os painéis continuam dentro do DOM do componente (position: fixed), então um
//    overlay aninhado é descendente do que o contém.
// Substitui o fechamento por focusout, que falhava quando o foco nunca entrava no painel.

const dismissStack = [];

function dismissIndex(el) {
    return dismissStack.findIndex(d => d.el === el);
}

function containsTarget(d, target) {
    return d.el.contains(target) || (d.anchor && d.anchor.contains && d.anchor.contains(target));
}

function notifyDismiss(d, method) {
    try {
        d.dotNet.invokeMethodAsync(method).catch(() => { /* componente descartado */ });
    } catch (_) {
        // referência já descartada
    }
}

function onDismissKeydown(e) {
    if (e.key !== 'Escape' || e.defaultPrevented) return;
    for (let i = dismissStack.length - 1; i >= 0; i--) {
        const d = dismissStack[i];
        if (!d.escape || !d.el.isConnected) continue;
        e.preventDefault();
        e.stopImmediatePropagation();
        notifyDismiss(d, 'OnOverlayEscape');
        return;
    }
}

function onDismissPointerdown(e) {
    const target = e.target;
    if (!(target instanceof Node)) return;
    const snapshot = dismissStack.slice();
    for (let i = snapshot.length - 1; i >= 0; i--) {
        const d = snapshot[i];
        if (!d.outside || !d.el.isConnected) continue;
        if (containsTarget(d, target)) continue;
        // Clique dentro de um overlay aberto por cima deste (ex.: painel de um Select aberto
        // a partir de um Popover) não fecha este.
        let insideAbove = false;
        for (let j = i + 1; j < snapshot.length; j++) {
            if (snapshot[j].el.isConnected && containsTarget(snapshot[j], target)) {
                insideAbove = true;
                break;
            }
        }
        if (!insideAbove) notifyDismiss(d, 'OnOverlayOutsideClick');
    }
}

function ensureDismissListeners() {
    if (ensureDismissListeners.done) return;
    ensureDismissListeners.done = true;
    window.addEventListener('keydown', onDismissKeydown, true);
    window.addEventListener('pointerdown', onDismissPointerdown, true);
}

/**
 * Registra `el` para fechar com clique fora e/ou Escape.
 * opts: { outside: bool, escape: bool }
 */
export function registerDismiss(el, anchor, dotNet, opts = {}) {
    if (!el || !dotNet) return;
    unregisterDismiss(el);
    dismissStack.push({ el, anchor: anchor || null, dotNet, outside: !!opts.outside, escape: !!opts.escape });
    ensureDismissListeners();
}

export function unregisterDismiss(el) {
    const i = dismissIndex(el);
    if (i >= 0) dismissStack.splice(i, 1);
}

// Usado pelo OpOverlayAttach: o painel é o pai do marcador renderizado pelo componente.
export function attachParent(marker, anchor, opts, dotNet) {
    const el = marker && marker.parentElement;
    if (!el) return;
    marker.__opOverlayEl = el;
    if (!anchor && opts && opts.anchorPrevious) anchor = el.previousElementSibling;
    if (dotNet && opts && (opts.dismissOutside || opts.dismissEscape)) {
        registerDismiss(el, anchor, dotNet, { outside: opts.dismissOutside, escape: opts.dismissEscape });
    }
    if (!anchor && opts) opts.positioned = false;
    attach(el, anchor, opts);
}

export function detachParent(marker) {
    const el = marker && marker.__opOverlayEl;
    if (!el) return;
    marker.__opOverlayEl = null;
    unregisterDismiss(el);
    detach(el);
}

/** Atualiza as opções de dismiss de um overlay já anexado (ex.: Dismissable mudou). */
export function updateDismissParent(marker, anchor, opts, dotNet) {
    const el = marker && marker.__opOverlayEl;
    if (!el) return;
    if (!anchor && opts && opts.anchorPrevious) anchor = el.previousElementSibling;
    const i = dismissIndex(el);
    const outside = !!(opts && opts.dismissOutside);
    const escape = !!(opts && opts.dismissEscape);
    if (!dotNet || (!outside && !escape)) {
        if (i >= 0) dismissStack.splice(i, 1);
        return;
    }
    if (i >= 0) {
        Object.assign(dismissStack[i], { anchor: anchor || null, dotNet, outside, escape });
    } else {
        registerDismiss(el, anchor, dotNet, { outside, escape });
    }
}

export function updateParent(marker) {
    const el = marker && marker.__opOverlayEl;
    if (el) update(el);
}
