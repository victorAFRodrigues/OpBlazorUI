# OpBlazorUI.Playwright

Testes de UI (foco em overlays) rodando contra o Showcase via Playwright + Chromium.

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

- `ConfirmDialog`: `DefaultFocus="accept"` foca o botão de aceitar; `Tab`/`Shift+Tab` ficam presos no diálogo; `Escape` fecha e devolve o foco ao botão que abriu.
- `Dialog` modal: foco inicial na raiz, `Tab` preso, `Escape` fecha e restaura o foco ao gatilho.

## CI

Nenhum workflow roda estes testes hoje (o `ci.yml` só faz restore/build/pack). Por ora, são locais.
