# OpBlazorUI.Playwright

Testes de UI rodando contra o Showcase via Playwright + Chromium. Cobrem, por bloco da auditoria
de out/2026:

- **Bloco 1–2:** base comum, descarte, interop, clique fora/Escape e cenários que derrubavam o circuito.
- **Bloco 3 (3.1–3.9):** seleção, formulários/`EditContext`, entrada de texto/números, DatePicker,
  árvores, abas/navegação, menus, overlays e ColorPicker/Slider/diretivas/FilterService.
- **Bloco 4 (4.1–4.4):** teclado/ARIA, recursos do DataTable (templates, paginação, filtro, lazy),
  overlays (blockScroll, drag/resize, tooltip, toast, context menu), recursos diversos,
  DynamicDialog e Chart.
- **Unitários:** `FilterService` (sem navegador).

Cenários que precisam de montagem específica usam páginas `/_tests/...` do Showcase (fora do menu).

## Pré-requisitos

Instalar o navegador do Playwright (uma vez):

```bash
# com PowerShell
pwsh bin/Debug/net10.0/playwright.ps1 install chromium

# sem PowerShell (macOS/Linux)
node bin/Debug/net10.0/.playwright/package/cli.js install chromium
```

## Rodar

O fixture sobe o Showcase automaticamente (`dotnet run`) numa porta livre e fecha ao final.

```bash
dotnet test tests/OpBlazorUI.Playwright/OpBlazorUI.Playwright.csproj
```

## O que cobre

- `ConfirmDialog`/`Dialog`: foco inicial, trap de `Tab`, `Escape`, restauração de foco e `blockScroll`.
- Teclado/ARIA de menus, abas, árvores, DatePicker, seleção, SplitButton/SpeedDial e DataTable.
- Recursos do DataTable (templates, paginação controlada, filtro, lazy) e overlays (drag/resize,
  tooltip, toast, context menu), DynamicDialog e Chart.

## CI

O workflow `ci.yml` tem o job `Test (Playwright)`: faz build em Debug, instala o Chromium
(`playwright.ps1 install --with-deps chromium`) e roda `dotnet test`. Localmente, instale o
navegador uma vez (ver Pré-requisitos) e rode `dotnet test`.
