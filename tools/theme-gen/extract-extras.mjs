// Extrai o CSS extra que os componentes Angular do Optimus UI injetam em runtime
// (packages/optimus-ui/src/<nome>/style/<nome>style.ts). Esse CSS não está em
// @openng/optimus-ui-styles: são regras como o zebrado da Table, o loader do Scroller ou a
// máscara do Drawer. O resultado (component-extras.json) é versionado e o generate.mjs o anexa
// a cada tema, resolvendo os dt() com o preset.
//
//   node extract-extras.mjs <caminho do clone de openng-org/optimus-ui>
//   (ou OPTIMUS_SRC=<caminho> npm run extract)

import { readdirSync, readFileSync, writeFileSync, existsSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { execSync } from 'node:child_process';

const here = dirname(fileURLToPath(import.meta.url));
const repo = process.argv[2] || process.env.OPTIMUS_SRC;
if (!repo) {
    console.error('Uso: node extract-extras.mjs <clone do optimus-ui>');
    process.exit(1);
}

const srcDir = join(repo, 'packages/optimus-ui/src');
// "base" é o BaseStyle do Angular (utilitários como .p-hidden-accessible); o CSS do tema já vem
// do @openng/optimus-ui-styles, então as interpolações ${xxx_style} são removidas.
const DECL = /const (?:style|css|theme) = (?:\/\*css\*\/ )?`([\s\S]*?)`;/;

const styles = {};
for (const name of readdirSync(srcDir).sort()) {
    const file = join(srcDir, name, 'style', `${name}style.ts`);
    if (!existsSync(file)) continue;
    const match = readFileSync(file, 'utf8').match(DECL);
    if (!match) continue;
    const css = match[1].replace(/\$\{\s*\w+_style\s*\}/g, '').trim();
    if (css) styles[name] = css;
}

let commit = null;
try {
    commit = execSync('git rev-parse --short HEAD', { cwd: repo }).toString().trim();
} catch {
    // sem git: segue sem o commit
}

writeFileSync(join(here, 'component-extras.json'), JSON.stringify({ source: 'openng-org/optimus-ui', commit, styles }, null, 2) + '\n');
console.log(`component-extras.json: ${Object.keys(styles).length} componentes (${commit ?? 'sem commit'})`);
