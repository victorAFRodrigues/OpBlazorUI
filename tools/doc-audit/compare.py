#!/usr/bin/env python3
"""Compara as seções (``DocSection``) das páginas do Showcase com as seções da
documentação do upstream (PrimeNG/Optimus UI) extraída em ``tools/doc-upstream/out``.

O id de cada ``DocSection`` é o que o componente ``DocSection`` usa na âncora:
``Id`` quando informado, senão o ``Label`` normalizado. Como as páginas seguem o
padrão do ``Button`` (``Label`` em pt-BR + ``Id`` igual ao do upstream), este
script resolve os dois casos antes de comparar.

As seções de formulários do Angular (``forms``, ``reactive``, ``reactiveforms``,
``templatedriven``) são tratadas como um único grupo ``forms`` — no Blazor há só
o ``EditForm`` e uma seção ``forms`` cobre as três.

Uso:
    python3 tools/doc-audit/compare.py [--report CAMINHO.json]

Gera (por padrão) ``tools/doc-audit/compare-report.json`` com uma entrada por
componente e imprime um resumo. Independe de GNU grep/PCRE, então roda no macOS.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
import unicodedata
from pathlib import Path

# Tag de abertura de uma DocSection (até o primeiro ``>``).
DOC_SECTION_RE = re.compile(r"<DocSection\b([^>]*)>", re.S)
ID_ATTR_RE = re.compile(r'\bId="([^"]*)"')
LABEL_ATTR_RE = re.compile(r'\bLabel="([^"]*)"')

# Pares id/label do array ``docs`` do index.ts do upstream.
UPSTREAM_PAIR_RE = re.compile(
    r"id:\s*['\"]([^'\"]+)['\"]\s*,\s*label:\s*['\"]([^'\"]*)['\"]"
)
UPSTREAM_ID_RE = re.compile(r"id:\s*['\"]([^'\"]+)['\"]")

# Páginas que não são componentes de doc.
SKIP = {
    "ComingSoon",
    "NotFound",
    "Home",
    "Installation",
    "Configuration",
    "Chart",
    "FilterService",
}

# Seções que representam o mesmo conceito (formulários).
ALIASES = {
    "reactive": "forms",
    "reactiveforms": "forms",
    "templatedriven": "forms",
}


def find_repo_root(start: Path) -> Path:
    for candidate in [start, *start.parents]:
        if (candidate / "OpBlazorUI.slnx").exists():
            return candidate
    raise SystemExit("Raiz do repositório (OpBlazorUI.slnx) não encontrada.")


def slug(text: str) -> str:
    """Normaliza para o mesmo id de âncora usado pelo ``DocSection``."""
    normalized = unicodedata.normalize("NFKD", text)
    without_accents = "".join(c for c in normalized if not unicodedata.combining(c))
    dashed = re.sub(r"[^a-z0-9]+", "-", without_accents.lower())
    return dashed.strip("-")


def extract_showcase_sections(page: Path) -> list[str]:
    """Ids resolvidos (``Id ?? slug(Label)``) das DocSection da página."""
    text = page.read_text(encoding="utf-8")
    sections: list[str] = []
    for attrs in DOC_SECTION_RE.findall(text):
        id_match = ID_ATTR_RE.search(attrs)
        if id_match:
            sections.append(id_match.group(1))
            continue
        label_match = LABEL_ATTR_RE.search(attrs)
        if label_match:
            sections.append(slug(label_match.group(1)))
    return sorted(set(s for s in sections if s))


def extract_upstream_ids(index_file: Path) -> list[str]:
    if not index_file.exists():
        return []
    return sorted(set(UPSTREAM_ID_RE.findall(index_file.read_text(encoding="utf-8"))))


def extract_upstream_pairs(index_file: Path) -> list[tuple[str, str]]:
    if not index_file.exists():
        return []
    return UPSTREAM_PAIR_RE.findall(index_file.read_text(encoding="utf-8"))


def main() -> int:
    here = Path(__file__).resolve().parent
    root = find_repo_root(here)
    pages_dir = root / "src" / "OpBlazorUI.Showcase" / "Components" / "Pages"
    upstream_dir = root / "tools" / "doc-upstream" / "out"

    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--report", type=Path, default=here / "compare-report.json")
    parser.add_argument(
        "--show-pairs",
        action="store_true",
        help="mostra (id, label) do upstream por componente com gaps",
    )
    args = parser.parse_args()

    upstream_folders = {
        p.name.lower(): p for p in upstream_dir.iterdir() if p.is_dir()
    }

    report: list[dict[str, object]] = []
    totals = {"missing": 0, "extra": 0, "matched": 0, "with_upstream": 0, "without": 0}

    for page in sorted(pages_dir.glob("*.razor")):
        name = page.stem
        if name.startswith("_") or name.startswith("Tests") or name in SKIP:
            continue

        showcase = extract_showcase_sections(page)
        folder = upstream_folders.get(name.lower())

        if folder is None:
            totals["without"] += 1
            report.append(
                {
                    "component": name,
                    "has_upstream": False,
                    "showcase_sections": showcase,
                    "upstream_sections": [],
                    "missing": [],
                    "extra": showcase,
                    "matched": [],
                }
            )
            continue

        totals["with_upstream"] += 1
        upstream = extract_upstream_ids(folder / "index.ts")
        showcase_canonical = {ALIASES.get(s, s) for s in showcase}
        upstream_canonical = {ALIASES.get(s, s) for s in upstream}

        missing = [s for s in upstream if ALIASES.get(s, s) not in showcase_canonical]
        matched = [s for s in upstream if ALIASES.get(s, s) in showcase_canonical]
        extra = [s for s in showcase if ALIASES.get(s, s) not in upstream_canonical]

        totals["missing"] += len(missing)
        totals["extra"] += len(extra)
        totals["matched"] += len(matched)

        report.append(
            {
                "component": name,
                "has_upstream": True,
                "showcase_sections": showcase,
                "upstream_sections": upstream,
                "missing": missing,
                "extra": extra,
                "matched": matched,
            }
        )

    args.report.write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )

    ranked = sorted(
        (e for e in report if e["has_upstream"] and e["missing"]),
        key=lambda e: (-len(e["missing"]), e["component"]),
    )

    print("=== AUDITORIA SHOWCASE vs UPSTREAM ===")
    for entry in ranked:
        print(f"\n{entry['component']} ({len(entry['missing'])})")
        print("  FALTANDO: " + ", ".join(entry["missing"]))
        if args.show_pairs:
            folder = upstream_folders[entry["component"].lower()]
            pairs = extract_upstream_pairs(folder / "index.ts")
            labels = {i: l for i, l in pairs}
            for section in entry["missing"]:
                if section in labels:
                    print(f"    - {section} = {labels[section]}")

    print("\n=== RESUMO ===")
    print(f"Com upstream: {totals['with_upstream']}  |  Sem upstream: {totals['without']}")
    print(f"Seções faltando: {totals['missing']}")
    print(f"Seções extras:   {totals['extra']}")
    print(f"Seções OK:       {totals['matched']}")
    print(f"\nRelatório: {args.report}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
