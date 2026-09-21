export function setDarkMode(enabled) {
    document.documentElement.classList.toggle('app-dark', !!enabled);
    try {
        localStorage.setItem('op-dark-mode', enabled ? '1' : '0');
    } catch (e) {
        // ignore
    }
}

export function getDarkMode() {
    return document.documentElement.classList.contains('app-dark');
}

export function setRtl(enabled) {
    document.documentElement.dir = enabled ? 'rtl' : 'ltr';
    document.documentElement.setAttribute('dir', enabled ? 'rtl' : 'ltr');
}

export function getRtl() {
    return document.documentElement.dir === 'rtl';
}

export function copyText(text) {
    if (navigator.clipboard && window.isSecureContext) {
        return navigator.clipboard.writeText(text);
    }
    return Promise.reject(new Error('clipboard unavailable'));
}

const FOCUSABLE_SELECTOR = 'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])';

function getFocusableElements(container) {
    return Array.from(container.querySelectorAll(FOCUSABLE_SELECTOR)).filter(
        (el) => !el.disabled && el.getAttribute('aria-hidden') !== 'true' && el.offsetParent !== null
    );
}

export function focusTrapInit(element) {
    if (!element || element.__opFocusTrapHandler) return;
    const handler = (e) => {
        if (e.key !== 'Tab' || element.getAttribute('data-focustrap-disabled') === 'true') return;
        const focusables = getFocusableElements(element);
        if (focusables.length === 0) return;
        const first = focusables[0];
        const last = focusables[focusables.length - 1];
        const active = document.activeElement;
        if (e.shiftKey) {
            if (active === first || !element.contains(active)) {
                e.preventDefault();
                last.focus();
            }
        } else if (active === last || !element.contains(active)) {
            e.preventDefault();
            first.focus();
        }
    };
    element.__opFocusTrapHandler = handler;
    element.addEventListener('keydown', handler);
}

export function focusTrapDispose(element) {
    if (!element) return;
    if (element.__opFocusTrapHandler) {
        element.removeEventListener('keydown', element.__opFocusTrapHandler);
        element.__opFocusTrapHandler = null;
    }
}

export function alignOverlay(element, target, position = 'bottom') {
    if (!element || !target) return { top: 0, left: 0, flipped: false };
    element.style.position = 'fixed';
    element.style.top = '0px';
    element.style.left = '0px';
    element.style.margin = '0';
    element.style.transform = 'none';
    element.style.visibility = 'hidden';

    const targetRect = target.getBoundingClientRect();
    const elRect = element.getBoundingClientRect();
    const gutter = 8;
    const viewportMargin = 8;

    const centeredLeft = targetRect.left + (targetRect.width - elRect.width) / 2;
    let top;
    let left = centeredLeft;
    let flipped = false;

    if (position === 'top') {
        top = targetRect.top - elRect.height - gutter;
    } else {
        top = targetRect.bottom + gutter;
    }

    if (top < viewportMargin && position !== 'top') {
        top = targetRect.top - elRect.height - gutter;
        flipped = true;
    } else if (top + elRect.height > window.innerHeight - viewportMargin && position === 'top') {
        top = targetRect.bottom + gutter;
        flipped = true;
    }

    left = Math.max(viewportMargin, Math.min(left, window.innerWidth - elRect.width - viewportMargin));

    element.style.top = top + 'px';
    element.style.left = left + 'px';
    element.classList.toggle('p-popover-flipped', flipped);
    element.style.visibility = 'visible';

    return { top, left, flipped };
}

export function addOutsideClickListener(element, target, dotnetRef) {
    if (!element || element.__opOutsideHandler) return;
    const handler = (e) => {
        if (element.contains(e.target) || (target && target.contains && target.contains(e.target))) return;
        try {
            dotnetRef.invokeMethodAsync('OnOutsideClick');
        } catch (_) {
            // ignore
        }
    };
    element.__opOutsideHandler = handler;
    document.addEventListener('pointerdown', handler, true);
}

export function removeOutsideClickListener(element) {
    if (!element) return;
    if (element.__opOutsideHandler) {
        document.removeEventListener('pointerdown', element.__opOutsideHandler, true);
        element.__opOutsideHandler = null;
    }
}

export function scrollTopInit(element, dotnetRef, target) {
    if (!element || element.__opScrollTop) return;
    const isParent = target === 'parent';
    const scrollEl = isParent ? element.parentElement : window;
    const getTop = () => {
        if (isParent) return scrollEl ? scrollEl.scrollTop : 0;
        return window.scrollY || document.documentElement.scrollTop || document.body.scrollTop || 0;
    };
    let ticking = false;
    const handler = () => {
        if (ticking) return;
        ticking = true;
        requestAnimationFrame(() => {
            ticking = false;
            try {
                dotnetRef.invokeMethodAsync('NotifyScrollTop', getTop());
            } catch (_) {
                // ignore
            }
        });
    };
    element.__opScrollTop = { scrollEl, handler };
    scrollEl.addEventListener('scroll', handler, { passive: true });
    handler();
}

export function scrollTopDispose(element) {
    if (!element || !element.__opScrollTop) return;
    element.__opScrollTop.scrollEl.removeEventListener('scroll', element.__opScrollTop.handler);
    element.__opScrollTop = null;
}

export function scrollTopTo(element, target, behavior) {
    const isParent = target === 'parent';
    const scrollEl = isParent ? element.parentElement : (document.scrollingElement || document.documentElement);
    if (!scrollEl) return;
    scrollEl.scrollTo({ top: 0, behavior });
}
