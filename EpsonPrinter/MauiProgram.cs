using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EpsonPrinter.Services;

namespace EpsonPrinter
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register PrintService
            builder.Services.AddSingleton<PrintService>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            return app;
        }
    }
}
