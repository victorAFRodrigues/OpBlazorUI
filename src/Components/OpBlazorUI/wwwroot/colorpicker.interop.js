const states = new WeakMap();
const outsideWatchers = new WeakMap();

function clamp01(value) {
    return Math.min(1, Math.max(0, value));
}

function pickColor(state, e) {
    const rect = state.selector.getBoundingClientRect();
    const saturation = rect.width ? clamp01((e.clientX - rect.left) / rect.width) * 100 : 0;
    const brightness = rect.height ? clamp01((rect.bottom - e.clientY) / rect.height) * 100 : 0;
    state.dotNet.invokeMethodAsync('OnPickColor', saturation, brightness).catch(function () {
        /* disposed */
    });
}

function pickHue(state, e) {
    const rect = state.hue.getBoundingClientRect();
    const ratio = rect.height ? clamp01((rect.bottom - e.clientY) / rect.height) : 0;
    state.dotNet.invokeMethodAsync('OnPickHue', ratio * 360).catch(function () {
        /* disposed */
    });
}

export function init(dotNet, root, selector, hue, outside) {
    if (!root) {
        return;
    }

    dispose(root);

    const state = {
        dotNet: dotNet,
        root: root,
        selector: selector,
        hue: hue,
        dragging: null
    };

    state.down = function (e) {
        if (selector && selector.contains(e.target)) {
            state.dragging = 'color';
            pickColor(state, e);
        } else if (hue && hue.contains(e.target)) {
            state.dragging = 'hue';
            pickHue(state, e);
        } else {
            return;
        }
        e.preventDefault();
    };

    state.move = function (e) {
        if (!state.dragging) {
            return;
        }
        if (state.dragging === 'color') {
            pickColor(state, e);
        } else {
            pickHue(state, e);
        }
        e.preventDefault();
    };

    state.up = function () {
        state.dragging = null;
    };

    root.addEventListener('pointerdown', state.down);
    document.addEventListener('pointermove', state.move);
    document.addEventListener('pointerup', state.up);
    if (outside) {
        watchOutside(dotNet, root);
    }

    states.set(root, state);
}

// O painel só é renderizado depois que estes listeners estão ativos (TogglePanel aguarda
// watchOutside), então um clique fora logo após abrir não se perde enquanto o módulo carrega.
export function watchOutside(dotNet, root) {
    if (!root) {
        return;
    }

    unwatchOutside(root);

    const handler = function (e) {
        if (root.contains(e.target)) {
            return;
        }

        try {
            dotNet.invokeMethodAsync('OnOutsideClick').catch(function () {
                /* disposed */
            });
        } catch (_) {
            // referência já descartada
        }
    };

    outsideWatchers.set(root, handler);
    document.addEventListener('pointerdown', handler, true);
}

export function unwatchOutside(root) {
    const handler = root && outsideWatchers.get(root);
    if (!handler) {
        return;
    }

    document.removeEventListener('pointerdown', handler, true);
    outsideWatchers.delete(root);
}

export function dispose(root) {
    unwatchOutside(root);

    const state = states.get(root);
    if (!state) {
        return;
    }

    root.removeEventListener('pointerdown', state.down);
    document.removeEventListener('pointermove', state.move);
    document.removeEventListener('pointerup', state.up);
    states.delete(root);
}
