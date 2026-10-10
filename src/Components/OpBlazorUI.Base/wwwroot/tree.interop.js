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

// ---------------------------------------------------------------- TreeTable
//
// As linhas do TreeTable são planas na tabela; usamos data-op-level para achar o pai.

function tableRows(root) {
    return Array.from(root.querySelectorAll('tr.p-treetable-row'));
}

function focusRow(root, tr) {
    if (!tr) return;
    tableRows(root).forEach((r) => r.setAttribute('tabindex', r === tr ? '0' : '-1'));
    try {
        tr.focus();
    } catch (_) {
        // ignore
    }
}

function rowToggle(tr) {
    return tr.querySelector('.p-treetable-node-toggle-button');
}

export function initTreeTableKeyboard(root) {
    if (!root || root.__opTreeTbKb) return;

    const ensureTabstop = () => {
        const rows = tableRows(root);
        if (rows.length && !rows.some((r) => r.getAttribute('tabindex') === '0')) {
            rows[0].setAttribute('tabindex', '0');
        }
    };
    ensureTabstop();

    const onKey = (e) => {
        const current = document.activeElement;
        if (!current || !current.matches('tr.p-treetable-row') || !root.contains(current)) return;

        const rows = tableRows(root);
        const idx = rows.indexOf(current);
        if (idx < 0) return;

        const level = parseInt(current.getAttribute('data-op-level') || '0', 10);
        const expanded = current.getAttribute('data-op-expanded') === 'true';
        const leaf = current.getAttribute('data-op-leaf') === 'true';

        switch (e.key) {
            case 'ArrowDown':
                focusRow(root, rows[Math.min(idx + 1, rows.length - 1)]);
                e.preventDefault();
                break;
            case 'ArrowUp':
                focusRow(root, rows[Math.max(idx - 1, 0)]);
                e.preventDefault();
                break;
            case 'Home':
                focusRow(root, rows[0]);
                e.preventDefault();
                break;
            case 'End':
                focusRow(root, rows[rows.length - 1]);
                e.preventDefault();
                break;
            case 'ArrowRight':
                if (leaf) break;
                if (expanded) {
                    const next = rows[idx + 1];
                    if (next && parseInt(next.getAttribute('data-op-level') || '0', 10) > level) {
                        focusRow(root, next);
                    }
                } else {
                    const button = rowToggle(current);
                    if (button) button.click();
                    let tries = 0;
                    const attempt = () => {
                        const next = tableRows(root)[idx + 1];
                        if (next && parseInt(next.getAttribute('data-op-level') || '0', 10) > level) {
                            focusRow(root, next);
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
                    const button = rowToggle(current);
                    if (button) button.click();
                } else {
                    for (let i = idx - 1; i >= 0; i--) {
                        if (parseInt(tableRows(root)[i].getAttribute('data-op-level') || '0', 10) < level) {
                            focusRow(root, tableRows(root)[i]);
                            break;
                        }
                    }
                }
                e.preventDefault();
                break;
            case 'Enter':
            case ' ':
                current.click();
                e.preventDefault();
                break;
        }
    };

    root.addEventListener('keydown', onKey);
    root.__opTreeTbKb = { onKey, ensureTabstop };
}

export function refreshTreeTableTabstop(root) {
    const kb = root && root.__opTreeTbKb;
    if (kb) kb.ensureTabstop();
}

