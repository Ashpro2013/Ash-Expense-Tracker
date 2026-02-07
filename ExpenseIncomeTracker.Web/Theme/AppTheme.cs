using MudBlazor;

namespace ExpenseIncomeTracker.Web.Theme;

public static class AppTheme
{
    public static MudTheme Create()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#1B6F5F",
                Secondary = "#F4B740",
                Background = "#F7F7FB",
                Surface = "#FFFFFF",
                AppbarBackground = "#FFFFFF",
                AppbarText = "#1F2937",
                TextPrimary = "#1F2937",
                TextSecondary = "#6B7280",
                DrawerBackground = "#0F172A",
                DrawerText = "#E2E8F0",
                DrawerIcon = "#94A3B8"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "16px"
            }
        };
    }
}
