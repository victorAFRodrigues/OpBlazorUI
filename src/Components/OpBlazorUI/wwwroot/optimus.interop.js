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
    // preventScroll evita a corrida com o posicionamento do OpOverlayAttach: o painel ainda é
    // position:absolute/top:0 no instante do foco e rolar a página para "alcançá-lo" levava o
    // documento inteiro para o topo (desvio consciente; o upstream posiciona antes de focar).
    if (initialFocusSelector) {
        requestAnimationFrame(() => {
            const target = element.querySelector(initialFocusSelector) || getFocusableElements(element)[0];
            if (target) {
                try {
                    target.focus({ preventScroll: true });
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

const THEMES_BASE = "_content/OpBlazorUI/themes/";

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

let _systemThemeDotNet = null;
let _systemThemeMedia = null;

function onSystemThemeChange(event) {
    if (window.__opThemeMode !== "system") return;
    document.documentElement.classList.toggle("app-dark", event.matches);
    if (_systemThemeDotNet) {
        _systemThemeDotNet.invokeMethodAsync("OnSystemDarkChanged", event.matches).catch(() => {
            // circuito encerrado
        });
    }
}

// Registra o callback .NET para o modo System. O op-theme.js já aplica a classe antes do boot.
export function observeSystemTheme(dotnetRef) {
    _systemThemeDotNet = dotnetRef;
    if (!window.matchMedia || _systemThemeMedia) return;
    _systemThemeMedia = window.matchMedia("(prefers-color-scheme: dark)");
    if (typeof _systemThemeMedia.addEventListener === "function") {
        _systemThemeMedia.addEventListener("change", onSystemThemeChange);
    } else if (typeof _systemThemeMedia.addListener === "function") {
        _systemThemeMedia.addListener(onSystemThemeChange);
    }
}

export function unobserveSystemTheme() {
    _systemThemeDotNet = null;
    if (!_systemThemeMedia) return;
    if (typeof _systemThemeMedia.removeEventListener === "function") {
        _systemThemeMedia.removeEventListener("change", onSystemThemeChange);
    } else if (typeof _systemThemeMedia.removeListener === "function") {
        _systemThemeMedia.removeListener(onSystemThemeChange);
    }
    _systemThemeMedia = null;
}

// Normaliza o formato atual e o antigo (boolean) para "light" | "dark" | "system".
function normalizeDarkMode(value) {
    if (value === true || value === "dark") return "dark";
    if (value === false || value === "light") return "light";
    if (value === "system") return "system";
    return null;
}

// Aplica o estado do OpThemeService no documento.
export function applyTheme(state) {
    ensureLink("optimus-theme", THEMES_BASE + state.preset + ".css");
    ensureLink("optimus-palettes", THEMES_BASE + "palettes.css");
    setPalette(state.primary, state.surface);
    setDarkMode(state.darkMode);
    window.__opThemeMode = normalizeDarkMode(state.darkModePreference)
        || (state.darkMode ? "dark" : "light");
    setRtl(state.rtl);
}

// Estado atual: o salvo (se houver), o que já está no documento (aplicado pelo op-theme.js) e a
// preferência do sistema. O modo salvo é normalizado para o formato atual.
export function readTheme() {
    let stored = null;
    try {
        stored = JSON.parse(localStorage.getItem(THEME_STORAGE_KEY) || 'null');
    } catch (_) {
        stored = null;
    }

    if (stored && typeof stored === "object") {
        stored = { ...stored, darkMode: normalizeDarkMode(stored.darkMode) };
    } else {
        stored = null;
    }

    return {
        stored,
        darkMode: getDarkMode(),
        prefersDark: !!(window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches)
    };
}

export function saveTheme(state) {
    try {
        const value = {
            ...state,
            darkMode: normalizeDarkMode(state.darkMode) || "light"
        };
        localStorage.setItem(THEME_STORAGE_KEY, JSON.stringify(value));
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
