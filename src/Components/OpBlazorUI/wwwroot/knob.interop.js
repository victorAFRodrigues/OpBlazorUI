const states = new WeakMap();

function emit(state, e) {
    const rect = state.el.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    state.dotNet.invokeMethodAsync('OnPointer', x, y).catch(function () {
        /* disposed */
    });
}

export function init(dotNet, el) {
    if (!el) {
        return;
    }

    dispose(el);

    const state = { dotNet: dotNet, el: el, dragging: false };

    state.down = function (e) {
        state.dragging = true;
        try {
            el.setPointerCapture(e.pointerId);
        } catch (_) {
            /* ignore */
        }
        emit(state, e);
        e.preventDefault();
    };

    state.move = function (e) {
        if (!state.dragging) {
            return;
        }
        emit(state, e);
        e.preventDefault();
    };

    state.up = function () {
        state.dragging = false;
    };

    el.addEventListener('pointerdown', state.down);
    el.addEventListener('pointermove', state.move);
    el.addEventListener('pointerup', state.up);
    el.addEventListener('pointercancel', state.up);
    states.set(el, state);
}

export function dispose(el) {
    const state = states.get(el);
    if (!state) {
        return;
    }

    el.removeEventListener('pointerdown', state.down);
    el.removeEventListener('pointermove', state.move);
    el.removeEventListener('pointerup', state.up);
    el.removeEventListener('pointercancel', state.up);
    states.delete(el);
}
