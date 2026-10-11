const editors = new Map();
// Ids descartados enquanto o init ainda aguardava o Quill: o init é abortado ao retomar.
const cancelled = new Set();

function notify(dotNet, method, arg) {
    if (!dotNet) return;
    const p = arg === undefined
        ? dotNet.invokeMethodAsync(method)
        : dotNet.invokeMethodAsync(method, arg);
    p.catch(function () {
        // ignore (component may be disposed)
    });
}

function loadScript(src) {
    return new Promise(function (resolve, reject) {
        const existing = document.querySelector('script[data-op-quill]');
        if (existing) {
            if (window.Quill) {
                resolve();
                return;
            }
            existing.addEventListener('load', function () { resolve(); });
            existing.addEventListener('error', function () { reject(new Error('Falha ao carregar o Quill.')); });
            return;
        }

        const script = document.createElement('script');
        script.src = src;
        script.setAttribute('data-op-quill', '');
        script.onload = function () { resolve(); };
        script.onerror = function () { reject(new Error('Falha ao carregar o Quill em ' + src + '.')); };
        document.head.appendChild(script);
    });
}

// O script do Quill é grande (~200 KB) e foi removido do setup global: carrega sob demanda
// na primeira inicialização de um Editor e fica em cache para os próximos.
let quillLoadPromise = null;

function ensureQuill() {
    if (window.Quill) {
        return Promise.resolve();
    }

    if (!quillLoadPromise) {
        quillLoadPromise = loadScript('_content/OpBlazorUI.Base/quill.min.js')
            .then(function () {
                if (!window.Quill) {
                    throw new Error('O Quill foi carregado, mas window.Quill não está disponível.');
                }
            })
            .catch(function (error) {
                // Permite nova tentativa numa próxima inicialização.
                quillLoadPromise = null;
                throw error;
            });
    }

    return quillLoadPromise;
}

export async function init(dotNet, id, contentEl, toolbarEl, config) {
    await ensureQuill(10000);
    if (cancelled.delete(id) || !contentEl || !contentEl.isConnected) return false;

    const options = Object.assign({}, config || {}, {
        theme: 'snow',
        modules: {
            toolbar: toolbarEl ? { container: toolbarEl } : false
        }
    });

    const quill = new window.Quill(contentEl, options);
    const root = contentEl.parentElement;

    quill.on('text-change', function (delta, oldDelta, source) {
        if (source === 'user') {
            notify(dotNet, 'NotifyTextChange', quill.root.innerHTML);
        }
    });

    let focusIn = null;
    let focusOut = null;
    if (root) {
        focusIn = function () { notify(dotNet, 'NotifyFocus'); };
        focusOut = function () { notify(dotNet, 'NotifyBlur'); };
        root.addEventListener('focusin', focusIn);
        root.addEventListener('focusout', focusOut);
    }

    editors.set(id, { quill, root, focusIn, focusOut });
    return true;
}

export function getHtml(id) {
    const entry = editors.get(id);
    return entry ? entry.quill.root.innerHTML : '';
}

export function setHtml(id, html) {
    const entry = editors.get(id);
    if (!entry) return;
    const next = html == null ? '' : html;
    if (entry.quill.root.innerHTML === next) return;
    if (next === '') {
        entry.quill.setText('', 'api');
    } else {
        entry.quill.clipboard.dangerouslyPasteHTML(next, 'api');
    }
}

export function setReadOnly(id, value) {
    const entry = editors.get(id);
    if (entry) entry.quill.enable(!value);
}

export function destroy(id) {
    const entry = editors.get(id);
    if (!entry) {
        cancelled.add(id);
        return;
    }
    if (entry.root) {
        if (entry.focusIn) entry.root.removeEventListener('focusin', entry.focusIn);
        if (entry.focusOut) entry.root.removeEventListener('focusout', entry.focusOut);
    }
    editors.delete(id);
}
