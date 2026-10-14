// Utilidades para inputs controlados pelo Blazor.

// Força o valor do elemento no DOM. Necessário quando o valor do componente não muda (ex.: um
// caractere rejeitado pelo KeyFilter) e o diff do Blazor não atualizaria o input.
export function setValue(element, value) {
    if (!element) return;
    const next = value == null ? '' : String(value);
    if (element.value !== next) element.value = next;
}

// Impede que Enter no input submeta o formulário (o chip é adicionado pelo componente).
export function preventEnterSubmit(element) {
    if (!element || element.__opPreventEnter) return;
    const handler = (e) => {
        if (e.key === 'Enter') e.preventDefault();
    };
    element.__opPreventEnter = handler;
    element.addEventListener('keydown', handler, true);
}

export function removePreventEnterSubmit(element) {
    if (!element || !element.__opPreventEnter) return;
    element.removeEventListener('keydown', element.__opPreventEnter, true);
    element.__opPreventEnter = null;
}
