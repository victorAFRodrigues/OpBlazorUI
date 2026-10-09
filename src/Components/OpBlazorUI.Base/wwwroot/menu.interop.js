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
