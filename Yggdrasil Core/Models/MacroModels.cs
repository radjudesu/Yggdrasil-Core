// MacroModels.cs
using System.Collections.ObjectModel;

namespace Yggdrasil_Core.Models
{
    public enum MacroMode
    {
        Toggle,
        Repeatedly,
        Hold,
        Auto
    }
    public class Macro
    {
        public string Name { get; set; }
        public string Script { get; set; }
        public MacroMode Mode { get; set; }
        public int RepeatCount { get; set; }
        public string StartKey { get; set; }
        public string StopKey { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
    public class MacroFolder
    {
        public string Name { get; set; }
        public ObservableCollection<Macro> Macros { get; set; } = new ObservableCollection<Macro>();
        public bool IsEnabled { get; set; } = true;
        public bool IsLocked { get; set; }
    }
    public class Profile
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public ObservableCollection<MacroFolder> Folders { get; set; } = new ObservableCollection<MacroFolder>();
    }
}