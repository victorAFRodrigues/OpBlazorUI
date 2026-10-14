const states = new WeakMap();

function updatePosition(state) {
    const content = state.content;
    const maxX = content.scrollWidth - content.clientWidth;
    const maxY = content.scrollHeight - content.clientHeight;

    if (maxX > 0) {
        const left = (content.scrollLeft / maxX) * Math.max(0, content.clientWidth - state.xBar.offsetWidth);
        state.xBar.style.left = left + 'px';
        state.xBar.setAttribute('aria-valuenow', String(Math.round(content.scrollLeft)));
    }

    if (maxY > 0) {
        const top = (content.scrollTop / maxY) * Math.max(0, content.clientHeight - state.yBar.offsetHeight);
        state.yBar.style.top = top + 'px';
        state.yBar.setAttribute('aria-valuenow', String(Math.round(content.scrollTop)));
    }
}

function moveBar(state) {
    const content = state.content;
    const cw = content.clientWidth;
    const sw = content.scrollWidth;
    const ch = content.clientHeight;
    const sh = content.scrollHeight;

    const xRatio = sw ? cw / sw : 1;
    const yRatio = sh ? ch / sh : 1;
    state.xRatio = xRatio;
    state.yRatio = yRatio;

    if (xRatio >= 1) {
        state.xBar.classList.add('p-scrollpanel-hidden');
    } else {
        state.xBar.classList.remove('p-scrollpanel-hidden');
        state.xBar.style.width = Math.max(cw * xRatio, 10) + 'px';
    }

    if (yRatio >= 1) {
        state.yBar.classList.add('p-scrollpanel-hidden');
    } else {
        state.yBar.classList.remove('p-scrollpanel-hidden');
        state.yBar.style.height = Math.max(ch * yRatio, 10) + 'px';
    }

    updatePosition(state);
}

export function init(root, step) {
    if (!root) {
        return;
    }

    dispose(root);

    const content = root.querySelector('[data-op-scroll-content]');
    const xBar = root.querySelector('.p-scrollpanel-bar-x');
    const yBar = root.querySelector('.p-scrollpanel-bar-y');
    if (!content || !xBar || !yBar) {
        return;
    }

    const state = { root: root, content: content, xBar: xBar, yBar: yBar, step: step || 5, xRatio: 1, yRatio: 1 };

    state.onScroll = function () { updatePosition(state); };
    state.onResize = function () { moveBar(state); };

    state.move = function (e) {
        if (!state.dragging) {
            return;
        }
        const delta = state.vertical ? e.clientY - state.startPos : e.clientX - state.startPos;
        const ratio = state.vertical ? state.yRatio : state.xRatio;
        if (ratio > 0) {
            if (state.vertical) {
                content.scrollTop = state.startScroll + delta / ratio;
            } else {
                content.scrollLeft = state.startScroll + delta / ratio;
            }
        }
        e.preventDefault();
    };

    state.up = function () { state.dragging = false; };

    state.down = function (e) {
        const bar = e.target.closest('.p-scrollpanel-bar');
        if (!bar) {
            return;
        }
        state.vertical = bar.classList.contains('p-scrollpanel-bar-y');
        state.dragging = true;
        state.startPos = state.vertical ? e.clientY : e.clientX;
        state.startScroll = state.vertical ? content.scrollTop : content.scrollLeft;
        try {
            bar.setPointerCapture(e.pointerId);
        } catch (_) { }
        e.preventDefault();
    };

    state.key = function (e) {
        const bar = e.target.closest('.p-scrollpanel-bar');
        if (!bar) {
            return;
        }
        const vertical = bar.classList.contains('p-scrollpanel-bar-y');
        let handled = true;

        if (vertical && e.code === 'ArrowDown') content.scrollTop += state.step;
        else if (vertical && e.code === 'ArrowUp') content.scrollTop -= state.step;
        else if (!vertical && e.code === 'ArrowRight') content.scrollLeft += state.step;
        else if (!vertical && e.code === 'ArrowLeft') content.scrollLeft -= state.step;
        else handled = false;

        if (handled) {
            e.preventDefault();
        }
    };

    content.addEventListener('scroll', state.onScroll);
    window.addEventListener('resize', state.onResize);
    root.addEventListener('pointerdown', state.down);
    root.addEventListener('pointermove', state.move);
    root.addEventListener('pointerup', state.up);
    root.addEventListener('pointercancel', state.up);
    root.addEventListener('keydown', state.key);

    states.set(root, state);
    moveBar(state);
}

export function refresh(root) {
    const state = states.get(root);
    if (state) {
        moveBar(state);
    }
}

export function scrollTop(root, value) {
    const state = states.get(root);
    if (state) {
        state.content.scrollTop = value;
    }
}

export function dispose(root) {
    const state = states.get(root);
    if (!state) {
        return;
    }

    state.content.removeEventListener('scroll', state.onScroll);
    window.removeEventListener('resize', state.onResize);
    root.removeEventListener('pointerdown', state.down);
    root.removeEventListener('pointermove', state.move);
    root.removeEventListener('pointerup', state.up);
    root.removeEventListener('pointercancel', state.up);
    root.removeEventListener('keydown', state.key);
    states.delete(root);
}
