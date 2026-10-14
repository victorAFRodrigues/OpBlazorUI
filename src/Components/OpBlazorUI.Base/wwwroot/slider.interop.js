const states = new WeakMap();

function percentFromEvent(root, orientation, e) {
    const rect = root.getBoundingClientRect();
    let percent;

    if (orientation === 'vertical') {
        percent = rect.height ? (rect.bottom - e.clientY) / rect.height : 0;
    } else {
        percent = rect.width ? (e.clientX - rect.left) / rect.width : 0;
    }

    if (!isFinite(percent)) {
        percent = 0;
    }

    return Math.min(1, Math.max(0, percent));
}

function handlePercent(root, orientation, handle) {
    const rootRect = root.getBoundingClientRect();
    const rect = handle.getBoundingClientRect();

    if (orientation === 'vertical') {
        return rootRect.height ? (rootRect.bottom - (rect.top + rect.height / 2)) / rootRect.height : 0;
    }

    return rootRect.width ? (rect.left + rect.width / 2 - rootRect.left) / rootRect.width : 0;
}

function nearestHandle(root, orientation, percent) {
    const handles = root.querySelectorAll('[data-op-slider-handle]');
    if (handles.length <= 1) {
        return 0;
    }

    let best = 0;
    let bestDistance = Infinity;

    handles.forEach(function (handle, index) {
        const distance = Math.abs(handlePercent(root, orientation, handle) - percent);
        if (distance < bestDistance) {
            bestDistance = distance;
            best = index;
        }
    });

    return best;
}

function emit(state, root, e) {
    const percent = percentFromEvent(root, state.orientation, e);
    state.dotNet.invokeMethodAsync('OnSlide', percent, state.index).catch(function () {
        /* disposed */
    });
}

export function init(dotNet, root, orientation) {
    if (!root) {
        return;
    }

    dispose(root);

    const state = {
        dotNet: dotNet,
        orientation: orientation,
        index: 0,
        dragging: false,
        root: root
    };

    state.down = function (e) {
        const handle = e.target.closest('[data-op-slider-handle]');
        if (handle) {
            state.index = parseInt(handle.getAttribute('data-op-slider-handle'), 10) || 0;
            state.dragging = true;
            if (handle.setPointerCapture) {
                try {
                    handle.setPointerCapture(e.pointerId);
                } catch (_) {
                    /* ignore */
                }
            }
        } else {
            state.index = nearestHandle(root, orientation, percentFromEvent(root, orientation, e));
            state.dragging = false;
        }

        emit(state, root, e);
        e.preventDefault();
    };

    state.move = function (e) {
        if (!state.dragging) {
            return;
        }
        emit(state, root, e);
        e.preventDefault();
    };

    state.up = function () {
        if (!state.dragging) {
            return;
        }
        state.dragging = false;
        state.dotNet.invokeMethodAsync('OnSlideEnd').catch(function () {
            /* disposed */
        });
    };

    // Setas/PgUp/Home não devem rolar a página ao ajustar o slider.
    const scrollKeys = ['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'PageUp', 'PageDown', 'Home', 'End'];
    state.keydown = function (e) {
        if (scrollKeys.indexOf(e.key) >= 0) {
            e.preventDefault();
        }
    };

    root.addEventListener('pointerdown', state.down);
    root.addEventListener('pointermove', state.move);
    root.addEventListener('pointerup', state.up);
    root.addEventListener('pointercancel', state.up);
    root.addEventListener('keydown', state.keydown);
    states.set(root, state);
}

export function dispose(root) {
    const state = states.get(root);
    if (!state) {
        return;
    }

    root.removeEventListener('pointerdown', state.down);
    root.removeEventListener('pointermove', state.move);
    root.removeEventListener('pointerup', state.up);
    root.removeEventListener('pointercancel', state.up);
    root.removeEventListener('keydown', state.keydown);
    states.delete(root);
}
