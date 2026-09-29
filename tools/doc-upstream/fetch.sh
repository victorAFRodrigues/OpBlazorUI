#!/usr/bin/env bash
# Baixa as seções de documentação de um componente do showcase do PrimeNG (base do Optimus UI)
# e imprime um resumo (ordem, id, label, texto e template de cada seção).
#
# Uso:   bash tools/doc-upstream/fetch.sh <componente> [ref]
# Ex.:   bash tools/doc-upstream/fetch.sh inputnumber
#        bash tools/doc-upstream/fetch.sh button master
#
# Saída: tools/doc-upstream/out/<componente>/ (index.ts + *doc.ts) e resumo no terminal.
set -euo pipefail

comp="${1:?informe o componente (nome da pasta no PrimeNG, ex.: button, inputnumber)}"
ref="${2:-21.0.0}"   # tag da v21 (a doc de referência é v21.primeng.org)
base="https://raw.githubusercontent.com/primefaces/primeng/$ref/apps/showcase"
out="$(dirname "$0")/out/$comp"
mkdir -p "$out"

curl -sf "$base/pages/$comp/index.ts" -o "$out/index.ts" || { echo "página '$comp' não encontrada em $ref"; exit 1; }

files=$(curl -sf "https://api.github.com/repos/primefaces/primeng/contents/apps/showcase/doc/$comp?ref=$ref" \
    | grep -oE '"name": *"[^"]*doc\.ts"' | sed 's/.*"\([^"]*\)"$/\1/')   # v21: basicdoc.ts / master: basic-doc.ts
for f in $files; do
    curl -sf "$base/doc/$comp/$f" -o "$out/$f"
done

echo "# $comp ($ref) — $(echo "$files" | wc -w) seções baixadas em $out"
echo
# Ordem oficial: pares id/label do array `docs` do index.ts
grep -oE "(id|label): *'[^']*'" "$out/index.ts" | sed -E "s/(id|label): *'([^']*)'/\2/" | paste - - | nl -w2 -s'. '
echo
for f in $files; do
    echo "===== $f"
    # Bloco template (texto explicativo + markup da demo), sem o <app-code>.
    sed -n '/template: `/,/^    `/p' "$out/$f" | grep -v -E 'template: `|^    `$|<app-code' || true
done
