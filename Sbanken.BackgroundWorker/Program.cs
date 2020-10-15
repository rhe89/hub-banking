using Microsoft.Extensions.Hosting;

namespace Sbanken.BackgroundWorker
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new SbankenWorkerTimerHostBuilder(args).Build().Run();
        }
    }
}