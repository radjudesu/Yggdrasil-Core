using System.Collections.ObjectModel;

namespace Yggdrasil_Core.Models
{
    public class MacroFolder
    {
        public string Name { get; set; }
        public ObservableCollection<Macro> Macros { get; set; } = new ObservableCollection<Macro>();
    }
}