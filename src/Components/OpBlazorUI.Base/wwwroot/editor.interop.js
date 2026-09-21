const editors = new Map();

function notify(dotNet, method, arg) {
    if (!dotNet) return;
    const p = arg === undefined
        ? dotNet.invokeMethodAsync(method)
        : dotNet.invokeMethodAsync(method, arg);
    p.catch(function () {
        // ignore (component may be disposed)
    });
}

function ensureQuill(timeoutMs) {
    return new Promise(function (resolve, reject) {
        if (window.Quill) {
            resolve();
            return;
        }
        const started = Date.now();
        const timer = setInterval(function () {
            if (window.Quill) {
                clearInterval(timer);
                resolve();
            } else if (Date.now() - started > timeoutMs) {
                clearInterval(timer);
                reject(new Error('Quill is not loaded. Add <script src="_content/OpBlazorUI.Base/quill.min.js"></script> to your page.'));
            }
        }, 50);
    });
}

export async function init(dotNet, id, contentEl, toolbarEl, config) {
    await ensureQuill(10000);

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
    if (!entry) return;
    if (entry.root) {
        if (entry.focusIn) entry.root.removeEventListener('focusin', entry.focusIn);
        if (entry.focusOut) entry.root.removeEventListener('focusout', entry.focusOut);
    }
    editors.delete(id);
}
