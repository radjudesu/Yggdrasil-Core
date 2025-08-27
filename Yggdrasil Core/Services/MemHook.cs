using System.Diagnostics;
using System.Threading.Tasks;

namespace Yggdrasil_Core.Services
{
    public static class MemHook
    {
        public static async Task ReadAsync(Process proc)
        {
            await Task.Delay(100);
        }
    }
}