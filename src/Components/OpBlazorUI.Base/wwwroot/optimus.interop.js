export function setDarkMode(enabled) {
    document.documentElement.classList.toggle('app-dark', !!enabled);
}

// Paletas de themes/palettes.css: null remove o atributo (vale a paleta do preset).
export function setPalette(primary, surface) {
    const root = document.documentElement;
    if (primary) root.setAttribute('data-op-primary', primary); else root.removeAttribute('data-op-primary');
    if (surface) root.setAttribute('data-op-surface', surface); else root.removeAttribute('data-op-surface');
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

// Gatilho (quem tinha o foco antes de abrir) por elemento trap, para restaurar ao fechar.
const FOCUS_TRAP_RESTORE = new WeakMap();

export function focusTrapInit(element, initialFocusSelector, restoreFocus) {
    if (!element) return;

    // Captura o gatilho antes de qualquer foco inicial do próprio trap/modal.
    if (restoreFocus && !FOCUS_TRAP_RESTORE.has(element)) {
        const trigger = document.activeElement;
        FOCUS_TRAP_RESTORE.set(element, trigger && trigger !== document.body ? trigger : null);
    }

    if (!element.__opFocusTrapHandler) {
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

    // Foco inicial só quando há um seletor; sem ele, quem manda é o componente (FocusOnShow).
    if (initialFocusSelector) {
        requestAnimationFrame(() => {
            const target = element.querySelector(initialFocusSelector) || getFocusableElements(element)[0];
            if (target) {
                try {
                    target.focus();
                } catch (_) {
                    // ignore
                }
            }
        });
    }
}

export function focusTrapDispose(element) {
    if (!element) return;
    if (element.__opFocusTrapHandler) {
        element.removeEventListener('keydown', element.__opFocusTrapHandler);
        element.__opFocusTrapHandler = null;
    }

    const previous = FOCUS_TRAP_RESTORE.get(element);
    FOCUS_TRAP_RESTORE.delete(element);
    if (previous && typeof previous.focus === 'function' && document.contains(previous)) {
        try {
            previous.focus();
        } catch (_) {
            // ignore
        }
    }
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

// ---------------------------------------------------------------- tema (OpThemeService)

const THEME_STORAGE_KEY = 'op-theme';

const THEMES_BASE = "_content/OpBlazorUI.Base/themes/";

// Os <link> do tema são controlados aqui (e criados pelo op-theme.js antes do boot), não por
// <HeadContent>: numa Blazor Web App com interatividade por página o <HeadOutlet> é estático e
// não recebe o HeadContent de componentes interativos. Ficam antes do optimus-base.css, que
// sobrescreve regras do tema.
function ensureLink(id, href) {
    let link = document.getElementById(id);
    if (link && link.getAttribute("href") === href) return;
    const next = document.createElement("link");
    next.id = id;
    next.rel = "stylesheet";
    next.href = href;
    const base = document.querySelector("link[href*=\"optimus-base.css\"]");
    if (link) {
        // Troca sem flash: o antigo só sai quando o novo carregou.
        link.id = id + "-old";
        link.after(next);
        const old = link;
        const drop = () => old.remove();
        next.addEventListener("load", drop, { once: true });
        next.addEventListener("error", drop, { once: true });
    } else if (base) {
        base.before(next);
    } else {
        document.head.appendChild(next);
    }
}

// Aplica o estado do OpThemeService no documento.
export function applyTheme(state) {
    ensureLink("optimus-theme", THEMES_BASE + state.preset + ".css");
    ensureLink("optimus-palettes", THEMES_BASE + "palettes.css");
    setPalette(state.primary, state.surface);
    setDarkMode(state.darkMode);
    setRtl(state.rtl);
}

// Estado atual: o salvo (se houver) e o que já está no documento (aplicado pelo op-theme.js ou
// pelo prefers-color-scheme).
export function readTheme() {
    let stored = null;
    try {
        stored = JSON.parse(localStorage.getItem(THEME_STORAGE_KEY) || 'null');
    } catch (_) {
        stored = null;
    }
    return { stored, darkMode: getDarkMode() };
}

export function saveTheme(state) {
    try {
        localStorage.setItem(THEME_STORAGE_KEY, JSON.stringify(state));
    } catch (_) {
        // localStorage indisponível
    }
}

// SpeedDial (circle/semi-circle/quarter-circle): centraliza os itens no botão compensando a
// diferença de tamanho entre o botão e a ação (--item-diff-x/y, como no upstream).
export function speedDialItemDiff(root) {
    if (!root) return;
    const button = root.querySelector('.p-speeddial-button');
    const list = root.querySelector('.p-speeddial-list');
    const item = list && list.querySelector('.p-speeddial-item');
    if (!button || !item) return;
    list.style.setProperty('--item-diff-x', `${Math.abs(button.offsetWidth - item.offsetWidth) / 2}px`);
    list.style.setProperty('--item-diff-y', `${Math.abs(button.offsetHeight - item.offsetHeight) / 2}px`);
}

// Foca o primeiro elemento que casa com o seletor dentro de `root` (usado pelo teclado do DatePicker).
export function focusSelector(root, selector) {
    if (!root || !selector) return;
    const el = root.querySelector(selector);
    if (!el) return;
    try {
        el.focus();
    } catch (_) {
        // não focável
    }
}

// Rola o elemento do seletor para dentro do container (opção focada em listas longas).
export function scrollSelectorIntoView(root, selector) {
    if (!root || !selector) return;
    const el = root.querySelector(selector);
    if (el && typeof el.scrollIntoView === 'function') {
        try {
            el.scrollIntoView({ block: 'nearest' });
        } catch (_) {
            // ignore
        }
    }
}

// Move o foco para o próximo/anterior elemento focável dentro de `root` (SpeedDial).
export function focusNextWithin(root, selector, delta) {
    if (!root || !selector) return;
    const items = Array.from(root.querySelectorAll(selector)).filter((el) => !el.disabled);
    if (!items.length) return;
    const current = items.indexOf(document.activeElement);
    const next = current < 0
        ? (delta >= 0 ? 0 : items.length - 1)
        : (current + delta + items.length) % items.length;
    try {
        items[next].focus();
    } catch (_) {
        // ignore
    }
}

// Trava a rolagem do body enquanto há um overlay modal aberto (com contador para overlays
// empilhados) compensando a largura da barra de rolagem para não deslocar o layout.
let _opScrollLocks = 0;
let _opSavedOverflow = '';
let _opSavedPaddingRight = '';

export function blockScroll() {
    if (typeof document === 'undefined') return;
    if (_opScrollLocks === 0) {
        const body = document.body;
        const scrollbar = window.innerWidth - document.documentElement.clientWidth;
        _opSavedOverflow = body.style.overflow;
        _opSavedPaddingRight = body.style.paddingRight;
        body.style.overflow = 'hidden';
        if (scrollbar > 0) {
            const current = parseFloat(getComputedStyle(body).paddingRight) || 0;
            body.style.paddingRight = (current + scrollbar) + 'px';
        }
    }
    _opScrollLocks++;
}

export function unblockScroll() {
    if (typeof document === 'undefined') return;
    if (_opScrollLocks > 0) _opScrollLocks--;
    if (_opScrollLocks === 0) {
        const body = document.body;
        body.style.overflow = _opSavedOverflow;
        body.style.paddingRight = _opSavedPaddingRight;
    }
}
