/*
 * OpBlazorUI — aplica o tema salvo antes do Blazor iniciar (evita flash de tema/cor/modo escuro).
 * Inclua no <head>, depois do <base href>:
 *
 *   <script src="_content/OpBlazorUI.Base/op-theme.js"
 *           data-preset="aura" data-primary="noir" data-surface=""></script>
 *
 * Os data-* são os padrões usados quando não há nada salvo; devem refletir o AddOpBlazorUI(...).
 * O estado é salvo pelo OpBlazorUiSetup quando OpThemeOptions.PersistTheme = true.
 */
(function () {
    var script = document.currentScript;
    var defaults = (script && script.dataset) || {};
    var state = null;
    try {
        state = JSON.parse(localStorage.getItem('op-theme') || 'null');
    } catch (e) {
        state = null;
    }
    state = state || {};

    var root = document.documentElement;
    var preset = state.preset || defaults.preset || 'aura';
    var primary = 'primary' in state ? state.primary : (defaults.primary || null);
    var surface = 'surface' in state ? state.surface : (defaults.surface || null);
    var dark = typeof state.darkMode === 'boolean'
        ? state.darkMode
        : window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;

    if (primary) root.setAttribute('data-op-primary', primary);
    if (surface) root.setAttribute('data-op-surface', surface);
    if (dark) root.classList.add('app-dark');
    if (state.rtl) root.setAttribute('dir', 'rtl');

    // CSS do preset e das paletas antes da primeira pintura; o OpBlazorUiSetup assume depois.
    var base = '_content/OpBlazorUI.Base/themes/';
    var link = document.createElement('link');
    link.id = 'op-theme-boot';
    link.rel = 'stylesheet';
    link.href = base + preset + '.css';
    document.head.appendChild(link);
    var palettes = document.createElement('link');
    palettes.id = 'op-theme-boot-palettes';
    palettes.rel = 'stylesheet';
    palettes.href = base + 'palettes.css';
    document.head.appendChild(palettes);
})();
