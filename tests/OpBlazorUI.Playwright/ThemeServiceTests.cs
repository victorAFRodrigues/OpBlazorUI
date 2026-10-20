using OpBlazorUI.Services;
using Xunit;

namespace OpBlazorUI.Playwright;

/// <summary>Contrato do estado inicial de OpThemeOptions/OpThemeService.</summary>
public class ThemeServiceTests
{
    [Fact]
    public void Opcoes_comecam_no_modo_claro()
    {
        var options = new OpThemeOptions();

        Assert.Equal(OpDarkMode.Light, options.DarkMode);
        var service = new OpThemeService(options);
        Assert.False(service.DarkMode);
    }

    [Fact]
    public void Dark_inicia_escuro_e_SetDarkMode_atualiza_o_estado()
    {
        var service = new OpThemeService(new OpThemeOptions { DarkMode = OpDarkMode.Dark });
        Assert.True(service.DarkMode);

        service.SetDarkMode(false);
        Assert.False(service.DarkMode);

        service.ToggleDarkMode();
        Assert.True(service.DarkMode);
    }

    [Fact]
    public void System_nao_forca_escuro_antes_do_setup()
    {
        var service = new OpThemeService(new OpThemeOptions { DarkMode = OpDarkMode.System });

        Assert.False(service.DarkMode);
    }
}
