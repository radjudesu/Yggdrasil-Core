using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Yggdrasil_Core.Utils;
using Yggdrasil_Core.Services;

namespace Yggdrasil_Core.Managers
{
    public class ToggleManager
    {
        private Dictionary<string, ToggleButton> toggles = new Dictionary<string, ToggleButton>();
        private Action<string, bool> onToggleAction;

        public ToggleManager(Action<string, bool> toggleAction)
        {
            onToggleAction = toggleAction;
        }

        public void Register(string toolName, ToggleButton toggle)
        {
            toggles[toolName] = toggle;
            toggle.Checked += (s, e) => HandleToggle(toolName, true);
            toggle.Unchecked += (s, e) => HandleToggle(toolName, false);
        }

        private async void HandleToggle(string toolName, bool isEnabled)
        {
            if (!ClientHandler.IsAttached) return;
            Logger.Info($"{toolName} {(isEnabled ? "enabled" : "disabled")}.");
            onToggleAction(toolName, isEnabled);
            if (isEnabled)
            {
                await Task.Run(() => MemHook.ReadAsync(ClientHandler.GetProcess));
            }
        }
    }
}