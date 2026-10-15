#!/usr/bin/env python3
"""Compara as seções (DocSection) das páginas do Showcase com as seções da doc
do upstream (PrimeNG/Optimus UI) extraída em ``tools/doc-upstream/out``.

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
from pathlib import Path

SHOWCASE_SECTION_RE = re.compile(r'<DocSection\b[^>]*\bId="([^"]+)"')
UPSTREAM_ID_RE = re.compile(r"id:\s*['\"]([^'\"]+)['\"]")

# Páginas que não são componentes de doc (ou não têm seção no upstream).
SKIP = {
    "ComingSoon",
    "NotFound",
    "Home",
    "Installation",
    "Configuration",
    "Chart",
    "FilterService",
}


def find_repo_root(start: Path) -> Path:
    for candidate in [start, *start.parents]:
        if (candidate / "OpBlazorUI.slnx").exists():
            return candidate
    raise SystemExit("Raiz do repositório (OpBlazorUI.slnx) não encontrada.")


def extract_showcase_sections(page: Path) -> list[str]:
    return sorted(set(SHOWCASE_SECTION_RE.findall(page.read_text(encoding="utf-8"))))


def extract_upstream_sections(index_file: Path) -> list[str]:
    if not index_file.exists():
        return []
    return sorted(set(UPSTREAM_ID_RE.findall(index_file.read_text(encoding="utf-8"))))


def main() -> int:
    here = Path(__file__).resolve().parent
    root = find_repo_root(here)
    pages_dir = root / "src" / "OpBlazorUI.Showcase" / "Components" / "Pages"
    upstream_dir = root / "tools" / "doc-upstream" / "out"

    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--report", type=Path, default=here / "compare-report.json")
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
        upstream = extract_upstream_sections(folder / "index.ts")
        missing = [s for s in upstream if s not in showcase]
        extra = [s for s in showcase if s not in upstream]
        matched = [s for s in upstream if s in showcase]

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

    print("=== AUDITORIA SHOWCASE vs UPSTREAM ===")
    for entry in report:
        if entry["has_upstream"] and (entry["missing"] or entry["extra"]):
            print(f"\n{entry['component']}")
            if entry["missing"]:
                print("  FALTANDO: " + ", ".join(entry["missing"]))
            if entry["extra"]:
                print("  EXTRAS:   " + ", ".join(entry["extra"]))

    print("\n=== RESUMO ===")
    print(f"Com upstream: {totals['with_upstream']}")
    print(f"Sem upstream: {totals['without']}")
    print(f"Seções faltando: {totals['missing']}")
    print(f"Seções extras: {totals['extra']}")
    print(f"Seções OK: {totals['matched']}")
    print(f"\nRelatório: {args.report}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
