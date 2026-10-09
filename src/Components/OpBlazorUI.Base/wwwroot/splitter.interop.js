const states = new WeakMap();

function isVertical(state) {
    return state.options.orientation === 'vertical';
}

function axisPos(state, e) {
    return isVertical(state) ? e.clientY : e.clientX;
}

function panels(root) {
    return Array.from(root.querySelectorAll('[data-op-splitter-panel]'));
}

function elSize(state, el) {
    return isVertical(state) ? el.offsetHeight : el.offsetWidth;
}

function containerSize(state) {
    return isVertical(state) ? state.root.clientHeight : state.root.clientWidth;
}

function applyPair(state, prev, next, prevSize, nextSize) {
    const total = state.total || 1;
    prev.style.flexBasis = ((prevSize / total) * 100) + '%';
    next.style.flexBasis = ((nextSize / total) * 100) + '%';
}

function resize(state, delta) {
    const ps = panels(state.root);
    const prev = ps[state.gutterIndex];
    const next = ps[state.gutterIndex + 1];
    if (!prev || !next) {
        return;
    }

    const mins = state.options.minSizes || [];
    const total = state.total || 1;
    let prevSize = state.prevSize + delta;
    let nextSize = state.nextSize - delta;

    const minPrev = ((mins[state.gutterIndex] || 0) / 100) * total;
    const minNext = ((mins[state.gutterIndex + 1] || 0) / 100) * total;

    if (prevSize < minPrev) {
        nextSize -= (minPrev - prevSize);
        prevSize = minPrev;
    }
    if (nextSize < minNext) {
        prevSize -= (minNext - nextSize);
        nextSize = minNext;
    }

    applyPair(state, prev, next, prevSize, nextSize);
}

function sizes(state) {
    const ps = panels(state.root);
    const total = containerSize(state) || 1;
    return ps.map(function (p) { return (elSize(state, p) / total) * 100; });
}

function begin(state, gutterIndex, e) {
    const ps = panels(state.root);
    const prev = ps[gutterIndex];
    const next = ps[gutterIndex + 1];
    if (!prev || !next) {
        return;
    }

    state.gutterIndex = gutterIndex;
    state.dragging = true;
    state.startPos = axisPos(state, e);
    state.prevSize = elSize(state, prev);
    state.nextSize = elSize(state, next);
    state.total = containerSize(state);

    state.root.classList.add('p-splitter-resizing');
    state.gutterEl.classList.add('p-splitter-gutter-resizing');

    state.dotNet.invokeMethodAsync('OnResizeStart', sizes(state)).catch(function () { });
}

function end(state) {
    if (!state.dragging) {
        return;
    }

    state.dragging = false;
    state.root.classList.remove('p-splitter-resizing');
    if (state.gutterEl) {
        state.gutterEl.classList.remove('p-splitter-gutter-resizing');
    }

    state.dotNet.invokeMethodAsync('OnResizeEnd', sizes(state)).catch(function () { });
}

export function init(dotNet, root, options) {
    if (!root) {
        return;
    }

    dispose(root);

    const state = { dotNet: dotNet, root: root, options: options, dragging: false, gutterIndex: -1, gutterEl: null };

    state.down = function (e) {
        const gutter = e.target.closest('[data-op-splitter-gutter]');
        if (!gutter) {
            return;
        }
        state.gutterEl = gutter;
        begin(state, parseInt(gutter.getAttribute('data-op-splitter-gutter'), 10) || 0, e);
        try {
            gutter.setPointerCapture(e.pointerId);
        } catch (_) { }
        e.preventDefault();
    };

    state.move = function (e) {
        if (!state.dragging) {
            return;
        }
        resize(state, axisPos(state, e) - state.startPos);
        e.preventDefault();
    };

    state.up = function () {
        end(state);
    };

    state.key = function (e) {
        const gutter = e.target.closest('[data-op-splitter-gutter]');
        if (!gutter) {
            return;
        }

        const index = parseInt(gutter.getAttribute('data-op-splitter-gutter'), 10) || 0;
        const step = state.options.step || 5;
        const vertical = isVertical(state);
        let delta = 0;

        if (vertical && e.code === 'ArrowDown') delta = step;
        else if (vertical && e.code === 'ArrowUp') delta = -step;
        else if (!vertical && e.code === 'ArrowRight') delta = step;
        else if (!vertical && e.code === 'ArrowLeft') delta = -step;
        else return;

        state.gutterEl = gutter;
        begin(state, index, { clientX: 0, clientY: 0 });
        state.startPos = 0;
        resize(state, delta);
        end(state);
        e.preventDefault();
    };

    root.addEventListener('pointerdown', state.down);
    root.addEventListener('pointermove', state.move);
    root.addEventListener('pointerup', state.up);
    root.addEventListener('pointercancel', state.up);
    root.addEventListener('keydown', state.key);
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
    root.removeEventListener('keydown', state.key);
    states.delete(root);
}
