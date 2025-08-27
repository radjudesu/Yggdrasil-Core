using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
        private ObservableCollection<string> profileNames = new ObservableCollection<string>();
        private string selectedProfile;

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

        public ObservableCollection<string> ProfileNames
        {
            get => profileNames;
            set { profileNames = value; OnPropChanged(nameof(ProfileNames)); }
        }

        public string SelectedProfile
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

        public MacroPadVM()
        {
            var general = new MacroFolder { Name = "General" };
            general.Macros.Add(new Macro { Name = "Macro1" });
            general.Macros.Add(new Macro { Name = "Macro2" });
            Folders.Add(general);
            ProfileNames.Add("Default");
            ProfileNames.Add("Profile1");

            CreateFolderCommand = new RelayCmd(_ => CreateFolder());
            CreateMacroCommand = new RelayCmd(_ => CreateMacro());
            EditMacroCommand = new RelayCmd(_ => EditMacro(), _ => SelectedMacro != null);
            DeleteCommand = new RelayCmd(_ => DeleteSelected(), _ => CanDelete());
            ImportMacroCommand = new RelayCmd(_ => ImportMacro());
            LoadProfileCommand = new RelayCmd(_ => LoadProfile(), _ => !string.IsNullOrEmpty(SelectedProfile));
            SaveProfileCommand = new RelayCmd(_ => SaveProfile(), _ => !string.IsNullOrEmpty(SelectedProfile));
            DeleteProfileCommand = new RelayCmd(_ => DeleteProfile(), _ => !string.IsNullOrEmpty(SelectedProfile));
            ImportProfileCommand = new RelayCmd(_ => ImportProfile());
        }

        private void UpdateSelected()
        {
            SelectedFolder = SelectedTreeItem as MacroFolder;
            SelectedMacro = SelectedTreeItem as Macro;
        }

        private void CreateFolder()
        {
            string name;
            do
            {
                name = InputDialog.Show("New Folder", "Enter folder name:");
                if (name == null) return;
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Name cannot be empty.");
                    continue;
                }
                if (Folders.Any(f => f.Name == name))
                {
                    MessageBox.Show("Folder name already exists.");
                    continue;
                }
                break;
            } while (true);

            var newFolder = new MacroFolder { Name = name };
            Folders.Add(newFolder);
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() => SelectedTreeItem = newFolder), DispatcherPriority.Render);
            Logger.Info($"New folder created: {name}");
        }

        private void CreateMacro()
        {
            string name;
            var targetFolder = GetCurrentFolder();
            do
            {
                name = InputDialog.Show("New Macro", "Enter macro name:");
                if (name == null) return;
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Name cannot be empty.");
                    continue;
                }
                if (targetFolder.Macros.Any(m => m.Name == name))
                {
                    MessageBox.Show("Macro name already exists in this folder.");
                    continue;
                }
                break;
            } while (true);

            var newMacro = new Macro { Name = name };
            targetFolder.Macros.Add(newMacro);
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() => SelectedTreeItem = newMacro), DispatcherPriority.Render);
            Logger.Info($"New macro created: {name}");
        }

        private void EditMacro()
        {
            Logger.Info($"Opening ScriptEditor for macro: {SelectedMacro.Name}");
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
                var result = MessageBox.Show($"Are you sure you want to delete the macro '{SelectedMacro.Name}'?", "Confirm Delete", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var folder = Folders.FirstOrDefault(f => f.Macros.Contains(SelectedMacro));
                    if (folder != null)
                    {
                        folder.Macros.Remove(SelectedMacro);
                        Logger.Info("Macro deleted.");
                    }
                    SelectedTreeItem = null;
                }
            }
            else if (SelectedFolder != null && SelectedFolder.Name != "General")
            {
                bool isEmpty = SelectedFolder.Macros.Count == 0;
                MessageBoxResult result = MessageBoxResult.No;
                if (!isEmpty)
                {
                    result = MessageBox.Show($"Are you sure you want to delete the folder '{SelectedFolder.Name}' and all its macros?", "Confirm Delete", MessageBoxButton.YesNo);
                }
                if (isEmpty || result == MessageBoxResult.Yes)
                {
                    Folders.Remove(SelectedFolder);
                    SelectedTreeItem = null;
                    Logger.Info("Folder deleted.");
                }
            }
        }

        private void ImportMacro()
        {
            var targetFolder = GetCurrentFolder();
            Logger.Info("Import macro (placeholder).");
        }

        private void LoadProfile()
        {
            Folders.Clear();
            var general = new MacroFolder { Name = "General" };
            general.Macros.Add(new Macro { Name = $"Loaded Macro from {SelectedProfile}" });
            Folders.Add(general);
            Logger.Info($"Profile loaded: {SelectedProfile}");
        }

        private void SaveProfile()
        {
            if (!ProfileNames.Contains(SelectedProfile))
            {
                ProfileNames.Add(SelectedProfile);
            }
            Logger.Info($"Profile saved: {SelectedProfile}");
        }

        private void DeleteProfile()
        {
            if (ProfileNames.Contains(SelectedProfile))
            {
                ProfileNames.Remove(SelectedProfile);
                Logger.Info("Profile deleted.");
            }
        }

        private void ImportProfile()
        {
            Logger.Info("Import profile (placeholder).");
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