// Gera os temas do OpBlazorUI a partir dos presets do Optimus UI (fork MIT do PrimeNG v21).
//
//   npm install && npm run build      → escreve wwwroot/themes/{aura,lara,nora}.css e palettes.css
//   npm run compare                   → gera o Aura "noir" em out/ para comparar com um tema antigo
//
// Cada preset vira um CSS estático com: tokens primitivos → semânticos (claro e .app-dark) →
// globais → estilo base → estilos comuns do preset → por componente (tokens + estilo).
// As paletas (primária e surface) ficam em palettes.css e são aplicadas por atributo no <html>,
// então trocar a cor não exige outro arquivo de tema.

import { readdirSync, writeFileSync, mkdirSync, readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Theme, definePreset, evaluateDtExpressions, dt } from '@openng/optimus-ui-styled';

const here = dirname(fileURLToPath(import.meta.url));
const THEMES_DIR = join(here, '../../src/Components/OpBlazorUI.Base/wwwroot/themes');
const PRESETS = ['aura', 'lara', 'nora'];
const OPTIONS = { prefix: 'p', darkModeSelector: '.app-dark', cssLayer: false };

const pkgDir = name => join(here, 'node_modules', name);
const pkgVersion = name => JSON.parse(readFileSync(join(pkgDir(name), 'package.json'), 'utf8')).version;
const THEMES_VERSION = pkgVersion('@openng/optimus-ui-themes');
const STYLES_VERSION = pkgVersion('@openng/optimus-ui-styles');

// ---------------------------------------------------------------- formatação

// O motor emite blocos minificados (":root,:host{--a:1;--b:2}"); formata no estilo dos
// arquivos de tema (seletores separados por ", " e uma declaração por linha).
function formatBlocks(css) {
    if (!css) return '';
    let out = '';
    const re = /([^{}]+)\{([^{}]*)\}/g;
    let m;
    while ((m = re.exec(css))) {
        const selector = m[1].trim().split(',').map(s => s.trim()).join(', ');
        const decls = splitDeclarations(m[2]);
        out += `${selector} {\n${decls.map(d => `    ${d};`).join('\n')}\n}\n\n`;
    }
    return out;
}

// Separa por ";" fora de parênteses (valores como color-mix(...) não contêm ";", mas url() pode).
function splitDeclarations(body) {
    const parts = [];
    let depth = 0;
    let cur = '';
    for (const ch of body) {
        if (ch === '(') depth++;
        else if (ch === ')') depth--;
        if (ch === ';' && depth === 0) {
            if (cur.trim()) parts.push(cur.trim());
            cur = '';
        } else cur += ch;
    }
    if (cur.trim()) parts.push(cur.trim());
    return parts;
}

// Estilos dos componentes vêm como template literals indentados com 4 espaços.
function dedent(css) {
    const lines = css.replace(/^\n+|\s+$/g, '').split('\n');
    const indent = Math.min(...lines.filter(l => l.trim()).map(l => l.match(/^ */)[0].length));
    return lines.map(l => l.slice(indent)).join('\n') + '\n\n';
}

const resolveDt = css => (css ? evaluateDtExpressions(css, dt) : '');

// ---------------------------------------------------------------- geração

function componentNames() {
    return readdirSync(join(pkgDir('@openng/optimus-ui-styles'), 'dist'), { withFileTypes: true })
        .filter(d => d.isDirectory() && d.name !== 'base')
        .map(d => d.name)
        .sort();
}

async function componentStyle(name) {
    try {
        const mod = await import(`@openng/optimus-ui-styles/${name}`);
        return mod.style || '';
    } catch {
        return '';
    }
}

async function buildPreset(preset, title) {
    Theme.setTheme({ preset, options: OPTIONS });
    const common = Theme.getCommon('common', {});
    const base = await componentStyle('base');

    let css = `/*\n * ${title} — gerado por tools/theme-gen a partir de @openng/optimus-ui-themes ${THEMES_VERSION}\n` +
        ` * e @openng/optimus-ui-styles ${STYLES_VERSION} (MIT). Não edite à mão: ajustes do OpBlazorUI\n` +
        ` * ficam em optimus-base.css; as paletas de cor em palettes.css.\n */\n\n`;
    css += formatBlocks(common.primitive?.css);
    css += formatBlocks(common.semantic?.css);
    css += formatBlocks(common.global?.css);
    if (base) css += dedent(resolveDt(base));
    if (common.style) css += dedent(resolveDt(common.style));

    for (const name of componentNames()) {
        const vars = Theme.getComponent(name, {});
        const style = await componentStyle(name);
        if (!vars.css && !style) continue;
        css += formatBlocks(vars.css);
        if (style) css += dedent(resolveDt(style));
        if (vars.style) css += dedent(resolveDt(vars.style));
    }
    return css.replace(/\n{3,}/g, '\n\n');
}

// ---------------------------------------------------------------- paletas

const PRIMARIES = ['emerald', 'green', 'lime', 'orange', 'amber', 'yellow', 'teal', 'cyan', 'sky', 'blue',
    'indigo', 'violet', 'purple', 'fuchsia', 'pink', 'rose'];
const SURFACES = {
    slate: 'slate', gray: 'gray', zinc: 'zinc', neutral: 'neutral', stone: 'stone',
    // Paletas extras do configurador do PrimeNG (não existem como primitivos).
    soho: { 50: '#f4f4f4', 100: '#e8e9e9', 200: '#d2d2d4', 300: '#bbbcbe', 400: '#a5a5a9', 500: '#8e8f93', 600: '#77787d', 700: '#616268', 800: '#4a4b52', 900: '#34343d', 950: '#1d1e27' },
    viva: { 50: '#f3f3f3', 100: '#e7e7e8', 200: '#cfd0d0', 300: '#b7b8b9', 400: '#9fa1a1', 500: '#87898a', 600: '#6e7173', 700: '#565a5b', 800: '#3e4244', 900: '#262b2c', 950: '#0e1315' },
    ocean: { 50: '#fbfcfc', 100: '#f7f9f8', 200: '#eff3f2', 300: '#dadedd', 400: '#b1b7b6', 500: '#828787', 600: '#5f7274', 700: '#415b61', 800: '#29444e', 900: '#183240', 950: '#0c1920' }
};
const SHADES = [50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950];

function paletteVars(prefix, source) {
    return SHADES.map(s => `    --p-${prefix}-${s}: ${typeof source === 'string' ? `var(--p-${source}-${s})` : source[s]};`).join('\n');
}

function buildPalettes() {
    let css = `/*\n * Paletas do OpBlazorUI — gerado por tools/theme-gen. Funciona com qualquer preset\n` +
        ` * (aura.css, lara.css, nora.css). Aplique no <html>:\n` +
        ` *   data-op-primary="noir|emerald|blue|..."   data-op-surface="slate|zinc|..."\n` +
        ` * Sem atributo, vale a paleta padrão do preset.\n */\n\n`;

    for (const name of PRIMARIES) {
        css += `:root[data-op-primary="${name}"] {\n${paletteVars('primary', name)}\n}\n\n`;
    }

    // Noir: a primária segue a surface (preto no claro, branco no escuro), como no configurador
    // do PrimeNG. Desvio consciente: o highlight (seleção em listas/tabelas) é suave, em vez de
    // primary-950 sólido, para a seleção não ficar preta.
    css += `:root[data-op-primary="noir"] {\n${paletteVars('primary', 'surface')}\n` +
        `    --p-primary-color: var(--p-primary-950);\n` +
        `    --p-primary-contrast-color: #ffffff;\n` +
        `    --p-primary-hover-color: var(--p-primary-800);\n` +
        `    --p-primary-active-color: var(--p-primary-700);\n` +
        `    --p-highlight-background: var(--p-surface-100);\n` +
        `    --p-highlight-focus-background: var(--p-surface-200);\n` +
        `    --p-highlight-color: var(--p-surface-900);\n` +
        `    --p-highlight-focus-color: var(--p-surface-950);\n}\n\n`;
    css += `.app-dark[data-op-primary="noir"] {\n` +
        `    --p-primary-color: var(--p-primary-50);\n` +
        `    --p-primary-contrast-color: var(--p-primary-950);\n` +
        `    --p-primary-hover-color: var(--p-primary-200);\n` +
        `    --p-primary-active-color: var(--p-primary-300);\n` +
        `    --p-highlight-background: color-mix(in srgb, var(--p-surface-0), transparent 84%);\n` +
        `    --p-highlight-focus-background: color-mix(in srgb, var(--p-surface-0), transparent 76%);\n` +
        `    --p-highlight-color: rgba(255, 255, 255, 0.87);\n` +
        `    --p-highlight-focus-color: rgba(255, 255, 255, 0.87);\n}\n\n`;

    // Surface: vale para claro e escuro (o preset usa slate no claro e zinc no escuro).
    for (const [name, source] of Object.entries(SURFACES)) {
        const vars = `    --p-surface-0: #ffffff;\n${paletteVars('surface', source)}`;
        css += `:root[data-op-surface="${name}"],\n.app-dark[data-op-surface="${name}"] {\n${vars}\n}\n\n`;
    }
    return css.replace(/\n{3,}/g, '\n\n');
}

// ---------------------------------------------------------------- main

async function loadPreset(name) {
    return (await import(`@openng/optimus-ui-themes/${name}`)).default;
}

if (process.argv.includes('--compare')) {
    // Reproduz o antigo aura.css (Aura com primária noir e highlight sólido) para diff.
    const aura = await loadPreset('aura');
    const surface = Object.fromEntries(SHADES.map(s => [s, `{surface.${s}}`]));
    const noir = definePreset(aura, {
        semantic: {
            primary: surface,
            colorScheme: {
                light: {
                    primary: { color: '{primary.950}', contrastColor: '#ffffff', hoverColor: '{primary.800}', activeColor: '{primary.700}' },
                    highlight: { background: '{primary.950}', focusBackground: '{primary.700}', color: '#ffffff', focusColor: '#ffffff' }
                },
                dark: {
                    primary: { color: '{primary.50}', contrastColor: '{primary.950}', hoverColor: '{primary.200}', activeColor: '{primary.300}' },
                    highlight: { background: '{primary.50}', focusBackground: '{primary.300}', color: '{primary.950}', focusColor: '{primary.950}' }
                }
            }
        }
    });
    mkdirSync(join(here, 'out'), { recursive: true });
    writeFileSync(join(here, 'out/aura-noir.css'), await buildPreset(noir, 'Aura (noir)'));
    console.log('out/aura-noir.css gerado');
} else {
    const titles = { aura: 'Aura', lara: 'Lara', nora: 'Nora' };
    for (const name of PRESETS) {
        const css = await buildPreset(await loadPreset(name), titles[name]);
        writeFileSync(join(THEMES_DIR, `${name}.css`), css);
        console.log(`${name}.css: ${css.split('\n').length} linhas`);
    }
    writeFileSync(join(THEMES_DIR, 'palettes.css'), buildPalettes());
    console.log('palettes.css gerado');
}
