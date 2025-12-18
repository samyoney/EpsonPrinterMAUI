using Microsoft.Extensions.DependencyInjection;
using EpsonPrinter.Services;

namespace EpsonPrinter
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            MainPage = serviceProvider.GetRequiredService<MainPage>();
        }
    }
}
