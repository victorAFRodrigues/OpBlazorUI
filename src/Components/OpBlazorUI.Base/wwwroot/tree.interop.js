// Teclado do Tree (p-tree / p-treetable). Roving tabindex sobre os treeitems visíveis; o DOM já só
// contém os nós expandidos, então a ordem é o pré-ordem visível.

function treeItems(root) {
    return Array.from(root.querySelectorAll('li[role="treeitem"]'));
}

function focusNode(root, li) {
    if (!li) return;
    treeItems(root).forEach((n) => n.setAttribute('tabindex', n === li ? '0' : '-1'));
    try {
        li.focus();
    } catch (_) {
        // ignore
    }
}

function firstChild(li) {
    return li.querySelector(':scope > ul[role="group"] > li[role="treeitem"]');
}

function parentNode(li) {
    const ul = li.parentElement;
    if (!ul || ul.getAttribute('role') !== 'group') return null;
    const parent = ul.parentElement;
    return parent && parent.getAttribute('role') === 'treeitem' ? parent : null;
}

function toggleButton(li) {
    return li.querySelector(':scope > div > button.p-tree-node-toggle-button');
}

export function initTreeKeyboard(root) {
    if (!root || root.__opTreeKb) return;

    // Garante um ponto de entrada por Tab enquanto não houver foco.
    const ensureTabstop = () => {
        const items = treeItems(root);
        if (items.length && !items.some((n) => n.getAttribute('tabindex') === '0')) {
            items[0].setAttribute('tabindex', '0');
        }
    };
    ensureTabstop();

    const onKey = (e) => {
        const current = document.activeElement;
        if (!current || current.getAttribute('role') !== 'treeitem' || !root.contains(current)) return;

        const items = treeItems(root);
        const idx = items.indexOf(current);
        if (idx < 0) return;

        const leaf = current.getAttribute('data-op-leaf') === 'true';
        const expanded = current.getAttribute('data-op-expanded') === 'true';

        switch (e.key) {
            case 'ArrowDown':
                focusNode(root, items[Math.min(idx + 1, items.length - 1)]);
                e.preventDefault();
                break;
            case 'ArrowUp':
                focusNode(root, items[Math.max(idx - 1, 0)]);
                e.preventDefault();
                break;
            case 'Home':
                focusNode(root, items[0]);
                e.preventDefault();
                break;
            case 'End':
                focusNode(root, items[items.length - 1]);
                e.preventDefault();
                break;
            case 'ArrowRight':
                if (leaf) break;
                if (expanded) {
                    focusNode(root, firstChild(current));
                } else {
                    const button = toggleButton(current);
                    if (button) button.click();
                    let tries = 0;
                    const attempt = () => {
                        const child = firstChild(current);
                        if (child) {
                            focusNode(root, child);
                        } else if (tries++ < 30) {
                            requestAnimationFrame(attempt);
                        }
                    };
                    attempt();
                }
                e.preventDefault();
                break;
            case 'ArrowLeft':
                if (!leaf && expanded) {
                    const button = toggleButton(current);
                    if (button) button.click();
                } else {
                    focusNode(root, parentNode(current));
                }
                e.preventDefault();
                break;
            case 'Enter':
            case ' ':
                {
                    const content = current.querySelector(':scope > div');
                    if (content) content.click();
                    e.preventDefault();
                }
                break;
        }
    };

    root.addEventListener('keydown', onKey);
    // Mantém o tabstop válido quando os nós mudam (filtro, expandir).
    root.__opTreeKb = { onKey, ensureTabstop };
}

// Chamado após cada render para revalidar o tabstop (nós podem ter mudado).
export function refreshTreeTabstop(root) {
    const kb = root && root.__opTreeKb;
    if (kb) kb.ensureTabstop();
}
