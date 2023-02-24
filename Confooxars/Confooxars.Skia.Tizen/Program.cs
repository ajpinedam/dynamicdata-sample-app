using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace Confooxars.Skia.Tizen
{
    public sealed class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new Confooxars.App());
            host.Run();
        }
    }
}
