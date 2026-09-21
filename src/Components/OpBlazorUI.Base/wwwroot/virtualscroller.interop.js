export function init(dotNet, element) {
    if (!element || element.__opVs) return;

    let ticking = false;
    const onScroll = () => {
        if (ticking) return;
        ticking = true;
        requestAnimationFrame(() => {
            ticking = false;
            try {
                dotNet.invokeMethodAsync('NotifyScroll', element.scrollTop, element.scrollLeft);
            } catch (_) {
                // ignore
            }
        });
    };

    element.addEventListener('scroll', onScroll, { passive: true });

    let ro = null;
    if (typeof ResizeObserver !== 'undefined') {
        ro = new ResizeObserver(() => {
            try {
                dotNet.invokeMethodAsync('NotifyResize', element.clientWidth, element.clientHeight);
            } catch (_) {
                // ignore
            }
        });
        ro.observe(element);
    }

    element.__opVs = { onScroll, ro };

    try {
        dotNet.invokeMethodAsync('NotifyResize', element.clientWidth, element.clientHeight);
        dotNet.invokeMethodAsync('NotifyScroll', element.scrollTop, element.scrollLeft);
    } catch (_) {
        // ignore
    }
}

export function getScrollPosition(element) {
    if (!element) return { top: 0, left: 0 };
    return { top: element.scrollTop, left: element.scrollLeft };
}

export function scrollTo(element, top, left, behavior) {
    if (!element) return;
    element.scrollTo({ top, left, behavior: behavior || 'auto' });
}

export function dispose(element) {
    if (!element || !element.__opVs) return;
    element.removeEventListener('scroll', element.__opVs.onScroll);
    if (element.__opVs.ro) element.__opVs.ro.disconnect();
    element.__opVs = null;
}
