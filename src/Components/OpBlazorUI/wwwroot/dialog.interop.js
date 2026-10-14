// Drag e redimensionamento do OpDialog. O estado por elemento é guardado em um WeakMap e
// expresso como deslocamento (transform) + largura/altura, preservando o layout do upstream.

const _states = new WeakMap();

function getState(element) {
    let state = _states.get(element);
    if (!state) {
        state = { dx: 0, dy: 0, drag: null, resize: null };
        _states.set(element, state);
    }
    return state;
}

function applyTransform(element, state) {
    element.style.transform = `translate(${state.dx}px, ${state.dy}px)`;
}

const clamp = (value, min, max) => Math.min(Math.max(value, min), max);

export function initDraggable(element, handle, keepInViewport) {
    if (!element || !handle) return;

    const state = getState(element);

    const onPointerDown = (event) => {
        if (event.button !== 0) return;
        if (event.target.closest('button, a, input, textarea, select, .p-dialog-header-icon')) return;

        const rect = element.getBoundingClientRect();
        state.drag = {
            handle,
            pointerId: event.pointerId,
            startX: event.clientX,
            startY: event.clientY,
            baseDx: state.dx,
            baseDy: state.dy,
            minDx: keepInViewport ? -rect.left : Number.NEGATIVE_INFINITY,
            maxDx: keepInViewport ? window.innerWidth - rect.right : Number.POSITIVE_INFINITY,
            minDy: keepInViewport ? -rect.top : Number.NEGATIVE_INFINITY,
            maxDy: keepInViewport ? window.innerHeight - rect.bottom : Number.POSITIVE_INFINITY
        };

        handle.setPointerCapture?.(event.pointerId);
        event.preventDefault();
    };

    const onPointerMove = (event) => {
        const drag = state.drag;
        if (!drag || event.pointerId !== drag.pointerId) return;

        const rawDx = event.clientX - drag.startX;
        const rawDy = event.clientY - drag.startY;
        state.dx = drag.baseDx + clamp(rawDx, drag.minDx, drag.maxDx);
        state.dy = drag.baseDy + clamp(rawDy, drag.minDy, drag.maxDy);
        applyTransform(element, state);
    };

    const onPointerUp = (event) => {
        if (state.drag && event.pointerId === state.drag.pointerId) {
            handle.releasePointerCapture?.(event.pointerId);
            state.drag = null;
        }
    };

    handle.addEventListener('pointerdown', onPointerDown);
    handle.addEventListener('pointermove', onPointerMove);
    handle.addEventListener('pointerup', onPointerUp);
    handle.addEventListener('pointercancel', onPointerUp);

    state.dragHandlers = { handle, onPointerDown, onPointerMove, onPointerUp };
}

export function initResizable(element, minWidth, minHeight, keepInViewport) {
    if (!element) return;

    const state = getState(element);
    const handles = Array.from(element.querySelectorAll('[data-op-resize]'));

    handles.forEach((handle) => {
        const direction = handle.getAttribute('data-op-resize') || '';

        const onPointerDown = (event) => {
            if (event.button !== 0) return;

            const rect = element.getBoundingClientRect();
            state.resize = {
                handle,
                pointerId: event.pointerId,
                direction,
                startX: event.clientX,
                startY: event.clientY,
                startWidth: rect.width,
                startHeight: rect.height,
                baseDx: state.dx,
                baseDy: state.dy,
                minDx: keepInViewport ? -rect.left : Number.NEGATIVE_INFINITY,
                maxDx: keepInViewport ? window.innerWidth - rect.right : Number.POSITIVE_INFINITY,
                minDy: keepInViewport ? -rect.top : Number.NEGATIVE_INFINITY,
                maxDy: keepInViewport ? window.innerHeight - rect.bottom : Number.POSITIVE_INFINITY
            };

            handle.setPointerCapture?.(event.pointerId);
            event.preventDefault();
            event.stopPropagation();
        };

        const onPointerMove = (event) => {
            const resize = state.resize;
            if (!resize || event.pointerId !== resize.pointerId) return;

            const movingWest = resize.direction.includes('w');
            const movingEast = resize.direction.includes('e');
            const movingNorth = resize.direction.includes('n');
            const movingSouth = resize.direction.includes('s');

            const deltaX = event.clientX - resize.startX;
            const deltaY = event.clientY - resize.startY;

            if (movingEast) {
                element.style.width = Math.max(minWidth, resize.startWidth + deltaX) + 'px';
            }
            if (movingSouth) {
                element.style.height = Math.max(minHeight, resize.startHeight + deltaY) + 'px';
            }
            if (movingWest) {
                const width = Math.max(minWidth, resize.startWidth - deltaX);
                state.dx = clamp(resize.baseDx + (resize.startWidth - width), resize.minDx, resize.maxDx);
            }
            if (movingNorth) {
                const height = Math.max(minHeight, resize.startHeight - deltaY);
                state.dy = clamp(resize.baseDy + (resize.startHeight - height), resize.minDy, resize.maxDy);
            }

            if (movingWest || movingNorth) {
                applyTransform(element, state);
            }
        };

        const onPointerUp = (event) => {
            if (state.resize && event.pointerId === state.resize.pointerId) {
                handle.releasePointerCapture?.(event.pointerId);
                state.resize = null;
            }
        };

        handle.addEventListener('pointerdown', onPointerDown);
        handle.addEventListener('pointermove', onPointerMove);
        handle.addEventListener('pointerup', onPointerUp);
        handle.addEventListener('pointercancel', onPointerUp);

        state.resizeHandlers ??= [];
        state.resizeHandlers.push({ handle, onPointerDown, onPointerMove, onPointerUp });
    });
}

export function destroyDialog(element) {
    const state = _states.get(element);
    if (!state) return;

    if (state.dragHandlers) {
        const { handle, onPointerDown, onPointerMove, onPointerUp } = state.dragHandlers;
        handle.removeEventListener('pointerdown', onPointerDown);
        handle.removeEventListener('pointermove', onPointerMove);
        handle.removeEventListener('pointerup', onPointerUp);
        handle.removeEventListener('pointercancel', onPointerUp);
    }

    if (state.resizeHandlers) {
        state.resizeHandlers.forEach(({ handle, onPointerDown, onPointerMove, onPointerUp }) => {
            handle.removeEventListener('pointerdown', onPointerDown);
            handle.removeEventListener('pointermove', onPointerMove);
            handle.removeEventListener('pointerup', onPointerUp);
            handle.removeEventListener('pointercancel', onPointerUp);
        });
    }

    _states.delete(element);
}
