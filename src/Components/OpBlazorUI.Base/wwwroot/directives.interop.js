// Interop das diretivas portadas do PrimeNG/Optimus UI v21: Ripple, AnimateOnScroll,
// StyleClass e AutoFocus. Cada função mantém o estado por elemento em WeakMap e expõe
// um par *Init/*Dispose (ou *Toggle, no caso do StyleClass).

// ---------------------------------------------------------------- Ripple
const ripples = new WeakMap();

export function rippleInit(element, options) {
    if (!element || ripples.has(element)) return;
    const config = options || {};
    const onPointerDown = (event) => createInk(element, event, config);
    element.addEventListener('pointerdown', onPointerDown);
    ripples.set(element, { onPointerDown });
}

export function rippleDispose(element) {
    const entry = ripples.get(element);
    if (!entry) return;
    element.removeEventListener('pointerdown', entry.onPointerDown);
    element.querySelectorAll('.p-ink').forEach((ink) => ink.remove());
    ripples.delete(element);
}

function createInk(element, event, config) {
    if (element.classList.contains('p-ripple-disabled')) return;

    const rect = element.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height) * 2;
    let x = event.clientX - rect.left - size / 2;
    let y = event.clientY - rect.top - size / 2;
    if (config.center) {
        x = rect.width / 2 - size / 2;
        y = rect.height / 2 - size / 2;
    }

    const ink = document.createElement('span');
    ink.className = 'p-ink';
    ink.setAttribute('aria-hidden', 'true');
    ink.style.width = `${size}px`;
    ink.style.height = `${size}px`;
    ink.style.left = `${x}px`;
    ink.style.top = `${y}px`;
    element.appendChild(ink);

    // Força o reflow para que a transição de escala dispare.
    void ink.offsetWidth;
    ink.classList.add('p-ink-active');
    ink.addEventListener('animationend', () => ink.remove(), { once: true });
}

// ---------------------------------------------------------------- AnimateOnScroll
const animates = new WeakMap();

export function animateOnScrollInit(element, options) {
    if (!element || animates.has(element)) return;
    const config = options || {};
    const enterClass = config.enterClass || '';
    const leaveClass = config.leaveClass || '';
    const threshold = typeof config.threshold === 'number' ? config.threshold : 0;

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
            if (entry.isIntersecting) {
                applyClasses(element, enterClass, leaveClass);
            } else {
                applyClasses(element, leaveClass, enterClass);
            }
        });
    }, { threshold });

    observer.observe(element);
    animates.set(element, { observer });
}

export function animateOnScrollDispose(element) {
    const entry = animates.get(element);
    if (!entry) return;
    entry.observer.disconnect();
    animates.delete(element);
}

// ---------------------------------------------------------------- StyleClass
const styleClasses = new WeakMap();

export function styleClassInit(element, options) {
    if (!element) return;
    const entry = {
        options: options || {},
        target: null,
        clickHandler: null,
        outsideHandler: null,
        escapeHandler: null,
        resizeHandler: null,
        hidden: null
    };
    entry.clickHandler = () => styleClassToggle(element);
    element.addEventListener('click', entry.clickHandler);
    styleClasses.set(element, entry);

    if (entry.options.hideOnResize) {
        const resizeTarget = resolveResizeTarget(entry.options.resizeSelector);
        if (resizeTarget) {
            entry.resizeHandler = () => {
                if (!element.isConnected) {
                    resizeTarget.removeEventListener('resize', entry.resizeHandler);
                    return;
                }
                hideOnResize(element, entry);
            };
            resizeTarget.addEventListener('resize', entry.resizeHandler);
        }
    }
}

function hideOnResize(element, entry) {
    const config = entry.options;
    const target = resolveTarget(element, config.target);
    if (!target) return;
    const leaveTo = toTokens(config.leaveToClass);
    const isHidden = entry.hidden === true || (leaveTo.length > 0 && leaveTo.some((token) => target.classList.contains(token)));
    if (isHidden) return;
    leave(target, config);
    entry.hidden = true;
    detachOverlay(element, entry);
}

export function styleClassDispose(element) {
    const entry = styleClasses.get(element);
    if (!entry) return;
    if (entry.clickHandler) element.removeEventListener('click', entry.clickHandler);
    detachOverlay(element, entry);
    if (entry.resizeHandler) {
        const resizeTarget = resolveResizeTarget(entry.options.resizeSelector);
        if (resizeTarget) resizeTarget.removeEventListener('resize', entry.resizeHandler);
    }
    styleClasses.delete(element);
}

export function styleClassToggle(element) {
    const entry = styleClasses.get(element);
    if (!entry) return;
    const config = entry.options;
    const target = resolveTarget(element, config.target);
    if (!target) return;

    const toggleTokens = toTokens(config.toggleClass);
    if (toggleTokens.length > 0) {
        toggleTokens.forEach((token) => target.classList.toggle(token));
        entry.hidden = toggleTokens.every((token) => target.classList.contains(token));
        if (entry.hidden) detachOverlay(element, entry);
        else attachOverlay(element, entry, target);
        return;
    }

    const hasEnter = !!(config.enterFromClass || config.enterActiveClass || config.enterToClass);
    const hasLeave = !!(config.leaveFromClass || config.leaveActiveClass || config.leaveToClass);

    if (hasEnter && !hasLeave) {
        enter(target, config);
        entry.hidden = false;
        attachOverlay(element, entry, target);
        return;
    }

    if (hasLeave && !hasEnter) {
        leave(target, config);
        entry.hidden = true;
        detachOverlay(element, entry);
        return;
    }

    const leaveTo = toTokens(config.leaveToClass);
    const hidden = leaveTo.length > 0
        ? leaveTo.some((token) => target.classList.contains(token))
        : entry.hidden !== false;

    if (hidden) {
        enter(target, config);
        entry.hidden = false;
        attachOverlay(element, entry, target);
    } else {
        leave(target, config);
        entry.hidden = true;
        detachOverlay(element, entry);
    }
}

function enter(target, config) {
    const from = toTokens(config.enterFromClass);
    const to = toTokens(config.enterToClass);

    removeAll(target, config.leaveFromClass, config.leaveActiveClass, config.leaveToClass);
    addAll(target, from);
    void target.offsetWidth;
    removeAll(target, config.enterFromClass);
    runAnimation(target, toTokens(config.enterActiveClass), () => {
        removeAll(target, config.enterActiveClass);
        addAll(target, to);
    });
}

function leave(target, config) {
    const from = toTokens(config.leaveFromClass);
    const to = toTokens(config.leaveToClass);

    removeAll(target, config.enterFromClass, config.enterActiveClass, config.enterToClass);
    addAll(target, from);
    void target.offsetWidth;
    removeAll(target, config.leaveFromClass);
    runAnimation(target, toTokens(config.leaveActiveClass), () => {
        removeAll(target, config.leaveActiveClass);
        addAll(target, to);
    });
}

function runAnimation(target, activeClasses, done) {
    // Sem classes ativas não há animação a esperar: aplica o estado final direto.
    if (activeClasses.length === 0) {
        done();
        return;
    }

    const finish = () => {
        target.removeEventListener('animationend', finish);
        target.removeEventListener('transitionend', finish);
        done();
    };
    target.addEventListener('animationend', finish, { once: true });
    target.addEventListener('transitionend', finish, { once: true });
    addAll(target, activeClasses);
}

function attachOverlay(element, entry, target) {
    const config = entry.options;
    if (config.hideOnOutsideClick && !entry.outsideHandler) {
        entry.outsideHandler = (event) => {
            if (!element.isConnected) {
                detachOverlay(element, entry);
                return;
            }
            if (!target.contains(event.target) && !element.contains(event.target)) {
                styleClassToggle(element);
            }
        };
        document.addEventListener('click', entry.outsideHandler, true);
    }
    if (config.hideOnEscape && !entry.escapeHandler) {
        entry.escapeHandler = (event) => {
            if (!element.isConnected) {
                detachOverlay(element, entry);
                return;
            }
            if (event.key === 'Escape') styleClassToggle(element);
        };
        document.addEventListener('keydown', entry.escapeHandler);
    }
}

function detachOverlay(element, entry) {
    if (entry.outsideHandler) {
        document.removeEventListener('click', entry.outsideHandler, true);
        entry.outsideHandler = null;
    }
    if (entry.escapeHandler) {
        document.removeEventListener('keydown', entry.escapeHandler);
        entry.escapeHandler = null;
    }
}

function resolveTarget(host, selector) {
    if (!selector) return host;
    switch (selector) {
        case '@next':
        case 'next':
            return host.nextElementSibling;
        case '@prev':
        case 'prev':
            return host.previousElementSibling;
        case '@parent':
        case 'parent':
            return host.parentElement;
        case '@grandparent':
        case 'grandparent':
            return host.parentElement ? host.parentElement.parentElement : null;
        default:
            return document.querySelector(selector);
    }
}

function resolveResizeTarget(selector) {
    if (!selector || selector === 'window') return window;
    if (selector === 'document') return document;
    return document.querySelector(selector);
}

function toTokens(value) {
    return (value || '').split(' ').filter((token) => token.length > 0);
}

function addAll(target, value) {
    toTokens(value).forEach((token) => target.classList.add(token));
}

function removeAll(target, ...values) {
    values.forEach((value) => toTokens(value).forEach((token) => target.classList.remove(token)));
}

function applyClasses(target, add, remove) {
    removeAll(target, remove);
    addAll(target, add);
}

// ---------------------------------------------------------------- AutoFocus
const autofocuses = new WeakMap();

const FOCUSABLE = 'input:not([type=hidden]):not([disabled]), select:not([disabled]), textarea:not([disabled]), button:not([disabled]), a[href], [tabindex]:not([tabindex="-1"])';

export function autoFocusInit(element) {
    if (!element) return;
    const previous = document.activeElement;
    const target = element.matches(FOCUSABLE) ? element : element.querySelector(FOCUSABLE);
    if (target) {
        try {
            target.focus();
        } catch {
            // foco pode falhar se o elemento não for focável
        }
    }
    autofocuses.set(element, { previous });
}

export function autoFocusDispose(element) {
    const entry = autofocuses.get(element);
    if (!entry) return;
    autofocuses.delete(element);
    if (entry.previous && typeof entry.previous.focus === 'function') {
        try {
            entry.previous.focus();
        } catch {
            // ignore
        }
    }
}
