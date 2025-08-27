using System.Diagnostics;
using System.Threading.Tasks;
using Yggdrasil_Core.Utils;

namespace Yggdrasil_Core.Services
{
    public static class ClientHandler
    {
        private static Process roProcess;
        private static bool isAttached;

        public static async Task<bool> Attach(Process selectedProcess)
        {
            try
            {
                roProcess = selectedProcess;
                isAttached = true;
                Logger.Info("Attached to RO client.");
                await Task.Delay(1);
                return true;
            }
            catch
            {
                Logger.Error("Failed to attach to RO client.");
                return false;
            }
        }

        public static void Detach()
        {
            roProcess = null;
            isAttached = false;
            Logger.Info("Detached from RO client.");
        }

        public static bool IsAttached => isAttached;

        public static Process GetProcess => roProcess;

        public static void RequestServerSupport()
        {
        }
    }
}