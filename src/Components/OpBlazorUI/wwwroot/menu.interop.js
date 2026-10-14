// Posicionamento dos submenus verticais (TieredMenu/ContextMenu/Menubar).
//
// Os submenus são renderizados no fluxo do Blazor com posição absoluta. Para abrir ao lado do
// item e inverter perto da borda, medimos o item pai e posicionamos o <ul data-op-submenu>.
// `position: fixed` é usado porque o painel já vive no viewport; recalculamos a cada render,
// então as mudanças de posição/tamanho do conteúdo são absorvidas.

const MARGIN = 8;

function place(el, level, orientation) {
    const li = el.parentElement; // <li> do item
    if (!li || !el.isConnected) return;

    const vw = document.documentElement.clientWidth;
    const vh = document.documentElement.clientHeight;
    const r = li.getBoundingClientRect();

    // Posicionamento absoluto relativo ao <li> (o tema usa min-width:100% do item).
    el.style.position = 'absolute';
    el.style.right = '';
    el.style.left = '';
    el.style.top = '';
    el.style.bottom = '';
    const w = el.offsetWidth;
    const h = el.offsetHeight;

    const horizontal = orientation === 'horizontal';
    let placed = 'right';
    let left;
    let top;

    if (level <= 1 && horizontal) {
        // Menubar: primeiro nível abre abaixo do item.
        placed = 'bottom';
        left = 0;
        top = li.offsetHeight;
        if (r.left + w > vw - MARGIN) left = li.offsetWidth - w;
    } else {
        // Demais níveis (e menus verticais): ao lado, invertendo se não couber.
        left = li.offsetWidth;
        top = 0;
        if (r.right + w > vw - MARGIN) {
            placed = 'left';
            left = -w;
        }
    }

    // Ajusta verticalmente dentro do viewport (top relativo ao <li>).
    let viewportTop = r.top + top;
    if (viewportTop + h > vh - MARGIN) viewportTop = Math.max(MARGIN, vh - h - MARGIN);
    if (viewportTop < MARGIN) viewportTop = MARGIN;
    top = viewportTop - r.top;

    el.style.left = left + 'px';
    el.style.top = top + 'px';
    el.style.transformOrigin = placed === 'left' ? 'top right' : 'top left';
    el.setAttribute('data-op-submenu-placement', placed);
}

export function positionSubmenus(root) {
    if (!root || !root.querySelectorAll) return;
    root.querySelectorAll('[data-op-submenu]').forEach((el) => {
        const level = parseInt(el.getAttribute('data-op-submenu'), 10) || 1;
        const orientation = el.getAttribute('data-op-orientation') || 'vertical';
        place(el, level, orientation);
    });
}

// ---------------------------------------------------------------- Teclado
//
// Navegação por teclado (roving tabindex) delegada no menu raiz. O foco real move-se entre os
// itens; Enter/Space disparam o `click` do próprio item (o Blazor decide abrir o submenu ou
// selecionar). Depois de abrir um submenu, o foco vai para o primeiro filho.

function menuItems(ul) {
    if (!ul) return [];
    return Array.from(ul.children)
        .filter((li) => li.tagName === 'LI')
        .map((li) => li.querySelector(':scope > div > a[role="menuitem"]'))
        .filter((a) => a && a.getAttribute('aria-disabled') !== 'true');
}

function focusItem(anchor) {
    if (!anchor) return;
    const ul = anchor.closest('ul');
    menuItems(ul).forEach((a) => a.setAttribute('tabindex', a === anchor ? '0' : '-1'));
    try {
        anchor.focus();
    } catch (_) {
        // elemento não focável
    }
}

function parentAnchor(anchor) {
    const li = anchor.closest('li');
    const ul = li && li.parentElement;
    if (!ul || !ul.hasAttribute('data-op-submenu')) return null;
    const parentLi = ul.parentElement;
    return parentLi ? parentLi.querySelector(':scope > div > a[role="menuitem"]') : null;
}

function hasSubmenu(anchor) {
    const li = anchor.closest('li');
    return li ? li.querySelector(':scope > ul[data-op-submenu]') : null;
}

function focusFirstChild(anchor) {
    let tries = 0;
    const attempt = () => {
        const items = menuItems(hasSubmenu(anchor));
        if (items.length) {
            focusItem(items[0]);
            return;
        }
        if (tries++ < 30) requestAnimationFrame(attempt);
    };
    attempt();
}

export function initMenuKeyboard(root, orientation) {
    if (!root || root.__opMenuKb) return;
    const vertical = orientation !== 'horizontal';

    const onKey = (e) => {
        const current = document.activeElement;
        if (!current || !current.matches('a[role="menuitem"]') || !root.contains(current)) return;

        const ul = current.closest('ul');
        const items = menuItems(ul);
        const idx = items.indexOf(current);
        if (idx < 0 || items.length === 0) return;

        const open = current.getAttribute('aria-expanded') === 'true';

        switch (e.key) {
            case 'ArrowDown':
                if (vertical) {
                    focusItem(items[(idx + 1) % items.length]);
                    e.preventDefault();
                }
                break;
            case 'ArrowUp':
                if (vertical) {
                    focusItem(items[(idx - 1 + items.length) % items.length]);
                    e.preventDefault();
                }
                break;
            case 'ArrowRight':
                if (!vertical) {
                    focusItem(items[(idx + 1) % items.length]);
                    e.preventDefault();
                } else if (current.getAttribute('aria-haspopup') === 'true') {
                    if (!open) current.click();
                    focusFirstChild(current);
                    e.preventDefault();
                }
                break;
            case 'ArrowLeft':
                if (!vertical) {
                    focusItem(items[(idx - 1 + items.length) % items.length]);
                    e.preventDefault();
                } else {
                    const parent = parentAnchor(current);
                    if (parent) {
                        // Fecha o submenu atual alternando o item pai (não seleciona o filho).
                        parent.click();
                        focusItem(parent);
                        e.preventDefault();
                    }
                }
                break;
            case 'Home':
                focusItem(items[0]);
                e.preventDefault();
                break;
            case 'End':
                focusItem(items[items.length - 1]);
                e.preventDefault();
                break;
            case 'Enter':
            case ' ':
                if (current.getAttribute('aria-haspopup') === 'true' && !open) {
                    current.click();
                    focusFirstChild(current);
                } else {
                    current.click();
                }
                e.preventDefault();
                break;
        }
    };

    root.addEventListener('keydown', onKey);
    root.__opMenuKb = { onKey };
}

// ---------------------------------------------------------------- Megamenu/PanelMenu
//
// Foco roving sobre âncoras (cabeçalho/item) que não são alcançáveis por Tab. Setas movem,
// Enter/Space disparam o click (o Blazor abre/seleciona), ←/→ expandem/recolhem.

function focusAnchors(root, selector) {
    return Array.from(root.querySelectorAll(selector));
}

export function initMenuFocusKeyboard(root, selector) {
    if (!root || root.__opFocusKb) return;

    const items = () => focusAnchors(root, selector);
    const focusAt = (el) => {
        if (!el) return;
        items().forEach((a) => a.setAttribute('tabindex', a === el ? '0' : '-1'));
        try {
            el.focus();
        } catch (_) {
            // ignore
        }
    };
    const ensure = () => {
        const all = items();
        if (all.length && !all.some((a) => a.getAttribute('tabindex') === '0')) {
            all[0].setAttribute('tabindex', '0');
        }
    };
    ensure();

    const onKey = (e) => {
        const current = document.activeElement;
        if (!current || !current.matches(selector) || !root.contains(current)) return;

        const all = items();
        const idx = all.indexOf(current);
        if (idx < 0) return;

        switch (e.key) {
            case 'ArrowDown':
                focusAt(all[Math.min(idx + 1, all.length - 1)]);
                e.preventDefault();
                break;
            case 'ArrowUp':
                focusAt(all[Math.max(idx - 1, 0)]);
                e.preventDefault();
                break;
            case 'Home':
                focusAt(all[0]);
                e.preventDefault();
                break;
            case 'End':
                focusAt(all[all.length - 1]);
                e.preventDefault();
                break;
            case 'ArrowRight':
                if (current.getAttribute('aria-haspopup') === 'true' && current.getAttribute('aria-expanded') === 'false') {
                    current.click();
                    // Foca o primeiro filho quando o submenu renderizar.
                    const li = current.closest('li');
                    let tries = 0;
                    const attempt = () => {
                        const all = li ? Array.from(li.querySelectorAll(selector)) : [];
                        const child = all.find((a) => a !== current);
                        if (child) {
                            focusAt(child);
                        } else if (tries++ < 30) {
                            requestAnimationFrame(attempt);
                        }
                    };
                    attempt();
                    e.preventDefault();
                }
                break;
            case 'ArrowLeft':
                if (current.getAttribute('aria-haspopup') === 'true' && current.getAttribute('aria-expanded') === 'true') {
                    current.click();
                    e.preventDefault();
                }
                break;
            case 'Enter':
            case ' ':
                current.click();
                e.preventDefault();
                break;
        }
    };

    root.addEventListener('keydown', onKey);
    root.__opFocusKb = { onKey, ensure };
}


// ContextMenu global/por seletor: escuta o contextmenu no documento e abre o menu nas
// coordenadas do cursor. `selector` nulo = qualquer ponto (global). Chaveado pelo host.
const _ctxMenus = new WeakMap();

export function initContextMenu(root, selector, dotnetRef) {
    if (!root) return;
    disposeContextMenu(root);

    const handler = (event) => {
        const target = selector ? event.target.closest(selector) : document.body;
        if (!target) return;
        event.preventDefault();
        dotnetRef.invokeMethodAsync('OnContextMenuAt', event.clientX, event.clientY);
    };

    document.addEventListener('contextmenu', handler, true);
    _ctxMenus.set(root, handler);
}

export function disposeContextMenu(root) {
    const handler = _ctxMenus.get(root);
    if (handler) {
        document.removeEventListener('contextmenu', handler, true);
        _ctxMenus.delete(root);
    }
}
