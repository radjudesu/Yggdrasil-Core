using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Yggdrasil_Core.Models;
using Yggdrasil_Core.Utils;

namespace Yggdrasil_Core.ViewModels
{
    public class MacroPadVM : INotifyPropertyChanged
    {
        private ObservableCollection<MacroFolder> folders = new ObservableCollection<MacroFolder>();
        private object selectedTreeItem;
        private MacroFolder selectedFolder;
        private Macro selectedMacro;
        private ObservableCollection<Profile> profiles = new ObservableCollection<Profile>();
        private Profile selectedProfile;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MacroFolder> Folders
        {
            get => folders;
            set { folders = value; OnPropChanged(nameof(Folders)); }
        }

        public object SelectedTreeItem
        {
            get => selectedTreeItem;
            set { selectedTreeItem = value; OnPropChanged(nameof(SelectedTreeItem)); UpdateSelected(); }
        }

        public MacroFolder SelectedFolder
        {
            get => selectedFolder;
            set { selectedFolder = value; OnPropChanged(nameof(SelectedFolder)); }
        }

        public Macro SelectedMacro
        {
            get => selectedMacro;
            set { selectedMacro = value; OnPropChanged(nameof(SelectedMacro)); }
        }

        public ObservableCollection<Profile> Profiles
        {
            get => profiles;
            set { profiles = value; OnPropChanged(nameof(Profiles)); }
        }

        public Profile SelectedProfile
        {
            get => selectedProfile;
            set { selectedProfile = value; OnPropChanged(nameof(SelectedProfile)); }
        }

        public ICommand CreateFolderCommand { get; }
        public ICommand CreateMacroCommand { get; }
        public ICommand EditMacroCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ImportMacroCommand { get; }
        public ICommand LoadProfileCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand DeleteProfileCommand { get; }
        public ICommand ImportProfileCommand { get; }
        public ICommand OpenScriptPadCommand { get; }

        public MacroPadVM()
        {
            var general = new MacroFolder { Name = "General" };
            general.Macros.Add(new Macro { Name = "Macro1" });
            general.Macros.Add(new Macro { Name = "Macro2" });
            Folders.Add(general);

            var defaultProfile = new Profile { Name = "Default", IsDefault = true };
            defaultProfile.Folders = Folders;
            Profiles.Add(defaultProfile);
            SelectedProfile = defaultProfile;

            CreateFolderCommand = new RelayCmd(_ => CreateFolder());
            CreateMacroCommand = new RelayCmd(_ => CreateMacro());
            EditMacroCommand = new RelayCmd(_ => EditMacro(), _ => SelectedMacro != null);
            DeleteCommand = new RelayCmd(_ => DeleteSelected(), _ => CanDelete());
            ImportMacroCommand = new RelayCmd(_ => ImportMacro());
            LoadProfileCommand = new RelayCmd(_ => LoadProfile(), _ => SelectedProfile != null);
            SaveProfileCommand = new RelayCmd(_ => SaveProfile(), _ => SelectedProfile != null);
            DeleteProfileCommand = new RelayCmd(_ => DeleteProfile(), _ => SelectedProfile != null && !SelectedProfile.IsDefault);
            ImportProfileCommand = new RelayCmd(_ => ImportProfile());
            OpenScriptPadCommand = new RelayCmd(_ => OpenScriptPad());
        }

        private void UpdateSelected()
        {
            SelectedFolder = SelectedTreeItem as MacroFolder;
            SelectedMacro = SelectedTreeItem as Macro;
        }

        private void CreateFolder()
        {
            var newFolder = new MacroFolder { Name = "New Folder" };
            Folders.Add(newFolder);
            SelectedTreeItem = newFolder;
            Logger.Info("New folder created.");
        }

        private void CreateMacro()
        {
            var targetFolder = GetCurrentFolder();
            var newMacro = new Macro { Name = "New Macro" };
            targetFolder.Macros.Add(newMacro);
            SelectedTreeItem = newMacro;
            Logger.Info("New macro created.");
        }

        private void EditMacro()
        {
            if (SelectedMacro != null)
            {
                var scriptPad = new Forms.ScriptPad(SelectedMacro, Folders, Profiles);
                scriptPad.ShowDialog();
            }
        }

        private bool CanDelete()
        {
            if (SelectedMacro != null) return true;
            if (SelectedFolder != null && SelectedFolder.Name != "General") return true;
            return false;
        }

        private void DeleteSelected()
        {
            if (SelectedMacro != null)
            {
                var folder = GetCurrentFolder();
                if (folder != null)
                {
                    folder.Macros.Remove(SelectedMacro);
                    Logger.Info("Macro deleted.");
                }
                SelectedTreeItem = null;
            }
            else if (SelectedFolder != null && SelectedFolder.Name != "General")
            {
                Folders.Remove(SelectedFolder);
                SelectedTreeItem = null;
                Logger.Info("Folder deleted.");
            }
        }

        private void ImportMacro()
        {
            var targetFolder = GetCurrentFolder();
            Logger.Info("Import macro (placeholder).");
        }

        private void LoadProfile()
        {
            if (SelectedProfile != null)
            {
                Folders = new ObservableCollection<MacroFolder>(SelectedProfile.Folders);
                Logger.Info($"Profile loaded: {SelectedProfile.Name}");
            }
        }

        private void SaveProfile()
        {
            if (SelectedProfile != null)
            {
                SelectedProfile.Folders = new ObservableCollection<MacroFolder>(Folders);
                Logger.Info($"Profile saved: {SelectedProfile.Name}");
            }
        }

        private void DeleteProfile()
        {
            if (SelectedProfile != null && !SelectedProfile.IsDefault)
            {
                Profiles.Remove(SelectedProfile);
                Logger.Info("Profile deleted.");
            }
        }

        private void ImportProfile()
        {
            Logger.Info("Import profile (placeholder).");
        }

        private void OpenScriptPad()
        {
            var scriptPad = new Forms.ScriptPad(null, Folders, Profiles);
            scriptPad.ShowDialog();
        }

        private MacroFolder GetCurrentFolder()
        {
            if (SelectedFolder != null) return SelectedFolder;
            if (SelectedMacro != null)
            {
                return Folders.FirstOrDefault(f => f.Macros.Contains(SelectedMacro));
            }
            return Folders.FirstOrDefault(f => f.Name == "General");
        }

        private void OnPropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}