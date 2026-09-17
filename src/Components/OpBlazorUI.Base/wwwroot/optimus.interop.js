export function setDarkMode(enabled) {
    document.documentElement.classList.toggle('app-dark', !!enabled);
    try {
        localStorage.setItem('op-dark-mode', enabled ? '1' : '0');
    } catch (e) {
        // ignore
    }
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
