using System.Threading.Tasks;
using Yggdrasil_Core.Utils;

namespace Yggdrasil_Core.Security
{
    public static class LicenseValidator
    {
        public static async Task<bool> Validate()
        {
            await Task.Delay(100);
            Logger.Info("License validated (placeholder).");
            return true;
        }

        public static string GetLicenseNumber()
        {
            return "1234-XXXX-XXXX";
        }
    }
}