/*
 * OpBlazorUI — aplica o tema antes do Blazor iniciar (evita flash de tema/cor/modo escuro).
 * Inclua no <head>, depois do <base href>:
 *
 *   <script src="_content/OpBlazorUI/op-theme.js"
 *           data-preset="aura" data-primary="noir" data-surface="slate"
 *           data-dark="light" data-persist="true"></script>
 *
 * data-dark: "light" (padrão), "dark" ou "system".
 * data-persist="false" ignora o estado salvo no localStorage; use junto com PersistTheme = false.
 * Os data-* são os padrões usados quando não há nada salvo e devem refletir o AddOpBlazorUI(...).
 */
(function () {
    var script = document.currentScript;
    var defaults = (script && script.dataset) || {};
    var persist = defaults.persist !== 'false';

    var state = null;
    if (persist) {
        try {
            state = JSON.parse(localStorage.getItem('op-theme') || 'null');
        } catch (e) {
            state = null;
        }
    }
    state = state && typeof state === 'object' ? state : {};

    var root = document.documentElement;
    var preset = state.preset || defaults.preset || 'aura';
    var primary = 'primary' in state ? state.primary : (defaults.primary || null);
    var surface = 'surface' in state ? state.surface : (defaults.surface || null);
    var mode = resolveDarkMode(state.darkMode, defaults.dark);
    var dark = mode === 'system' ? prefersDark() : mode === 'dark';

    if (primary) root.setAttribute('data-op-primary', primary);
    if (surface) root.setAttribute('data-op-surface', surface);
    if (dark) root.classList.add('app-dark');
    if (state.rtl) root.setAttribute('dir', 'rtl');

    // O OpBlazorUiSetup lê este valor para saber se o modo atual é uma preferência explícita
    // (claro/escuro) ou se deve continuar seguindo o sistema operacional.
    window.__opThemeMode = mode;

    if (mode === 'system' && window.matchMedia) {
        var media = window.matchMedia('(prefers-color-scheme: dark)');
        var onSystemChange = function (event) {
            if (window.__opThemeMode === 'system') root.classList.toggle('app-dark', event.matches);
        };
        if (typeof media.addEventListener === 'function') media.addEventListener('change', onSystemChange);
        else if (typeof media.addListener === 'function') media.addListener(onSystemChange);
    }

    // CSS do preset e das paletas antes da primeira pintura. Os mesmos <link> são atualizados
    // depois pelo OpBlazorUiSetup (optimus.interop.js#applyTheme) ao trocar o tema.
    var base = '_content/OpBlazorUI/themes/';
    var link = document.createElement('link');
    link.id = 'optimus-theme';
    link.rel = 'stylesheet';
    link.href = base + preset + '.css';
    document.head.appendChild(link);
    var palettes = document.createElement('link');
    palettes.id = 'optimus-palettes';
    palettes.rel = 'stylesheet';
    palettes.href = base + 'palettes.css';
    document.head.appendChild(palettes);

    function resolveDarkMode(stored, configured) {
        if (stored === true) return 'dark';
        if (stored === false) return 'light';
        if (stored === 'dark' || stored === 'light' || stored === 'system') return stored;
        var value = String(configured || 'light').toLowerCase();
        return value === 'dark' || value === 'system' ? value : 'light';
    }

    function prefersDark() {
        return !!(window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches);
    }
})();
