using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Yggdrasil_Core.Models;
using Yggdrasil_Core.Utils;

namespace Yggdrasil_Core.ViewModels
{
    public partial class ScriptPadVM : INotifyPropertyChanged
    {
        private TextEditor editor;
        private Macro currentMacro;
        private ObservableCollection<MacroFolder> folders;
        private ObservableCollection<Profile> profiles;
        private object selectedTreeItem;
        private MacroFolder selectedFolder;
        private Macro selectedMacro;
        private Profile selectedProfile;
        private TextDocument document = new TextDocument();
        private IHighlightingDefinition syntaxHighlighter;
        private bool isSyntaxEnabled = true;
        private bool isDarkMode = true;
        private string selectedFolderName;
        private bool isToggle;
        private bool isRepeatedly;
        private bool isHold;
        private bool isAuto;
        private int repeatCount;
        private string startKey;
        private string stopKey;
        private List<string> variables = new List<string>(); // Dynamic variables
        private readonly string[] roots = { "Keyboard.", "Keyboard.type", "MouseClick.", "MouseMove", "Wait", "Repeat", "EndRepeat", "Label", "Goto", "If", "EndIf", "func", "endfunc" };
        private readonly string[] actions = { "press", "release", "tap" };
        private readonly string[] mouseBtns = { "Left", "Right", "Middle" };
        private readonly string[] keys =
        {
            "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
            "0","1","2","3","4","5","6","7","8","9",
            "F1","F2","F3","F4","F5","F6","F7","F8","F9","F10","F11","F12","F13","F14","F15","F16","F17","F18","F19","F20","F21","F22","F23","F24",
            "ESC","TAB","ENTER","SPACE","LEFT","RIGHT","UP","DOWN","SHIFT","CTRL","ALT","WIN"
        };
        private readonly string[] operators = { "==", "!=", ">", "<", ">=", "<=" };
        private AutoCompletionHandler completionHandler;

        public event PropertyChangedEventHandler PropertyChanged;
        public Macro CurrentMacro
        {
            get => currentMacro;
            set
            {
                currentMacro = value;
                OnPropChanged(nameof(CurrentMacro));
                LoadMacro();
            }
        }
        public ObservableCollection<MacroFolder> Folders
        {
            get => folders;
            set
            {
                folders = value;
                OnPropChanged(nameof(Folders));
            }
        }
        public ObservableCollection<Profile> Profiles
        {
            get => profiles;
            set
            {
                profiles = value;
                OnPropChanged(nameof(Profiles));
            }
        }
        public object SelectedTreeItem
        {
            get => selectedTreeItem;
            set
            {
                selectedTreeItem = value;
                OnPropChanged(nameof(SelectedTreeItem));
                UpdateSelected();
            }
        }
        public Profile SelectedProfile
        {
            get => selectedProfile;
            set
            {
                selectedProfile = value;
                OnPropChanged(nameof(SelectedProfile));
            }
        }
        public TextDocument Document
        {
            get => document;
            set
            {
                document = value;
                OnPropChanged(nameof(Document));
            }
        }
        public IHighlightingDefinition SyntaxHighlighter
        {
            get => isSyntaxEnabled ? syntaxHighlighter : null;
            set
            {
                syntaxHighlighter = value;
                OnPropChanged(nameof(SyntaxHighlighter));
            }
        }
        public List<string> FolderNames => Folders.Select(f => f.Name).ToList();
        public string SelectedFolderName
        {
            get => selectedFolderName;
            set
            {
                selectedFolderName = value;
                OnPropChanged(nameof(SelectedFolderName));
            }
        }
        public bool IsToggle
        {
            get => isToggle;
            set
            {
                isToggle = value;
                UpdateMode();
                OnPropChanged(nameof(IsToggle));
            }
        }
        public bool IsRepeatedly
        {
            get => isRepeatedly;
            set
            {
                isRepeatedly = value;
                UpdateMode();
                OnPropChanged(nameof(IsRepeatedly));
            }
        }
        public bool IsHold
        {
            get => isHold;
            set
            {
                isHold = value;
                UpdateMode();
                OnPropChanged(nameof(IsHold));
                OnPropChanged(nameof(CanSetStopKey));
            }
        }
        public bool IsAuto
        {
            get => isAuto;
            set
            {
                isAuto = value;
                UpdateMode();
                OnPropChanged(nameof(IsAuto));
                OnPropChanged(nameof(CanSetKeys));
                OnPropChanged(nameof(CanSetStopKey));
            }
        }
        public int RepeatCount
        {
            get => repeatCount;
            set
            {
                repeatCount = value;
                OnPropChanged(nameof(RepeatCount));
            }
        }
        public string StartKey
        {
            get => startKey;
            set
            {
                startKey = value;
                OnPropChanged(nameof(StartKey));
            }
        }
        public string StopKey
        {
            get => stopKey;
            set
            {
                stopKey = value;
                OnPropChanged(nameof(StopKey));
            }
        }
        public bool CanSetKeys => !IsAuto;
        public bool CanSetStopKey => !IsAuto && !IsHold;
        public ScriptPadVM(Macro macro, ObservableCollection<MacroFolder> folders, ObservableCollection<Profile> profiles, TextEditor editor)
        {
            this.editor = editor;
            this.folders = folders;
            this.profiles = profiles;
            CurrentMacro = macro ?? new Macro { Name = "New Macro" };
            LoadSyntaxHighlighter();
            completionHandler = new AutoCompletionHandler(editor, variables, roots, actions, mouseBtns, keys, operators);
            InitializeCommands();
        }
        protected void OnPropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private void LoadMacro()
        {
            Document.Text = CurrentMacro.Script ?? string.Empty;
            SelectedFolderName = GetCurrentFolder()?.Name ?? "General";
            IsToggle = CurrentMacro.Mode == MacroMode.Toggle;
            IsRepeatedly = CurrentMacro.Mode == MacroMode.Repeatedly;
            RepeatCount = CurrentMacro.RepeatCount;
            IsHold = CurrentMacro.Mode == MacroMode.Hold;
            IsAuto = CurrentMacro.Mode == MacroMode.Auto;
            StartKey = CurrentMacro.StartKey;
            StopKey = CurrentMacro.StopKey;
        }
        private void UpdateSelected()
        {
            selectedFolder = SelectedTreeItem as MacroFolder;
            selectedMacro = SelectedTreeItem as Macro;
            if (selectedMacro != null) CurrentMacro = selectedMacro;
        }
        private void UpdateMode()
        {
        }
        private void LoadSyntaxHighlighter()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Yggdrasil_Core.Resources.CustomScript.xshd"))
            {
                using (var reader = new XmlTextReader(stream))
                {
                    SyntaxHighlighter = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
        public void RegisterScriptVariable(string name, string address, bool readOnly)
        {
            if (!variables.Contains(name))
            {
                variables.Add(name);
            }
        }
        private void ToggleSyntax()
        {
            isSyntaxEnabled = !isSyntaxEnabled;
            OnPropChanged(nameof(SyntaxHighlighter));
        }
        private void ToggleTheme()
        {
            isDarkMode = !isDarkMode;
            editor.Background = isDarkMode ? Brushes.Black : Brushes.White;
            editor.Foreground = isDarkMode ? Brushes.White : Brushes.Black;
        }
        private void InsertText(string text)
        {
            editor.Document.Insert(editor.CaretOffset, text);
        }
        private void SaveMacro()
        {
            if (string.IsNullOrEmpty(CurrentMacro.Name))
            {
                CurrentMacro.Name = InputDialog.Show("Macro Name", "Enter macro name:");
                if (string.IsNullOrEmpty(CurrentMacro.Name)) return;
            }
            var targetFolder = Folders.FirstOrDefault(f => f.Name == SelectedFolderName) ?? Folders.First(f => f.Name == "General");
            if (targetFolder.Macros.Any(m => m.Name == CurrentMacro.Name && m != CurrentMacro))
            {
                MessageBox.Show("Macro name already exists in this folder.");
                return;
            }
            CurrentMacro.Script = Document.Text;
            CurrentMacro.Mode = IsToggle ? MacroMode.Toggle : IsRepeatedly ? MacroMode.Repeatedly : IsHold ? MacroMode.Hold : MacroMode.Auto;
            CurrentMacro.RepeatCount = RepeatCount;
            CurrentMacro.StartKey = StartKey;
            CurrentMacro.StopKey = StopKey;
            if (!targetFolder.Macros.Contains(CurrentMacro))
            {
                targetFolder.Macros.Add(CurrentMacro);
            }
            Logger.Info($"Macro saved: {CurrentMacro.Name}");
        }
        public void AutoSaveIfNeeded()
        {
            if (!string.IsNullOrEmpty(Document.Text) && string.IsNullOrEmpty(CurrentMacro.Name))
            {
                CurrentMacro.Name = GetUniqueUnnamedName();
                SaveMacro();
            }
        }
        private string GetUniqueUnnamedName()
        {
            int i = 1;
            while (true)
            {
                var name = $"Unnamed Macro ({i})";
                if (!Folders.SelectMany(f => f.Macros).Any(m => m.Name == name)) return name;
                i++;
            }
        }
        private void CreateFolder()
        {
            string name;
            do
            {
                name = InputDialog.Show("New Folder", "Enter folder name:");
                if (name == null) return;
                if (string.IsNullOrWhiteSpace(name) || Folders.Any(f => f.Name == name)) continue;
                break;
            } while (true);
            var newFolder = new MacroFolder { Name = name };
            Folders.Add(newFolder);
            SelectedTreeItem = newFolder;
        }
        private void CreateMacro()
        {
            string name;
            var target = GetCurrentFolder();
            do
            {
                name = InputDialog.Show("New Macro", "Enter macro name:");
                if (name == null) return;
                if (string.IsNullOrWhiteSpace(name) || target.Macros.Any(m => m.Name == name)) continue;
                break;
            } while (true);
            var newMacro = new Macro { Name = name };
            target.Macros.Add(newMacro);
            SelectedTreeItem = newMacro;
            CurrentMacro = newMacro;
        }
        private void RenameSelected(object parameter)
        {
            var item = parameter ?? SelectedTreeItem;
            if (item is Macro macro)
            {
                string newName = InputDialog.Show("Rename Macro", "Enter new name:", macro.Name);
                if (newName != null && !string.IsNullOrEmpty(newName))
                {
                    var folder = GetCurrentFolder();
                    if (folder.Macros.Any(m => m.Name == newName && m != macro)) return;
                    macro.Name = newName;
                }
            }
            else if (item is MacroFolder folder && folder.Name != "General")
            {
                string newName = InputDialog.Show("Rename Folder", "Enter new name:", folder.Name);
                if (newName != null && !string.IsNullOrEmpty(newName) && !Folders.Any(f => f.Name == newName))
                {
                    folder.Name = newName;
                }
            }
        }
        private void DeleteSelected(object parameter)
        {
            var item = parameter ?? SelectedTreeItem;
            if (item is Macro macro)
            {
                var result = MessageBox.Show($"Delete {macro.Name}?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    GetCurrentFolder()?.Macros.Remove(macro);
                }
            }
            else if (item is MacroFolder folder && folder.Name != "General")
            {
                bool empty = folder.Macros.Count == 0;
                MessageBoxResult result = empty ? MessageBoxResult.Yes : MessageBox.Show($"Delete folder {folder.Name} and contents?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Folders.Remove(folder);
                }
            }
        }
        private void ImportMacro()
        {
            Logger.Info("Import macro");
        }
        private void ExportMacro(object parameter)
        {
            var item = parameter ?? SelectedTreeItem;
            if (item is Macro macro)
            {
                Logger.Info($"Export macro: {macro.Name}");
            }
        }
        private void ToggleEnable(object parameter)
        {
            var item = parameter ?? SelectedTreeItem;
            if (item is Macro m) m.IsEnabled = !m.IsEnabled;
            if (item is MacroFolder f) f.IsEnabled = !f.IsEnabled;
        }
        private void ImportFolder()
        {
            Logger.Info("Import folder");
        }
        private void ExportFolder(object parameter)
        {
            var item = parameter ?? SelectedTreeItem;
            if (item is MacroFolder folder)
            {
                Logger.Info($"Export folder: {folder.Name}");
            }
        }
        private void ToggleLock(MacroFolder folder)
        {
            if (folder != null) folder.IsLocked = !folder.IsLocked;
        }
        private void SetActive(Profile profile)
        {
            if (profile == null) return;
            foreach (var p in Profiles) p.IsActive = false;
            profile.IsActive = true;
        }
        private void DeleteProfile(Profile profile)
        {
            if (profile == null || profile.IsDefault) return;
            var result = MessageBox.Show($"Delete {profile.Name}?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Profiles.Remove(profile);
            }
        }
        private void RenameProfile(Profile profile)
        {
            if (profile == null || profile.IsDefault) return;
            string newName = InputDialog.Show("Rename Profile", "Enter new name:", profile.Name);
            if (newName != null && !string.IsNullOrEmpty(newName) && !Profiles.Any(p => p.Name == newName))
            {
                profile.Name = newName;
            }
        }
        private void ImportProfile()
        {
            Logger.Info("Import profile");
        }
        private void ExportProfile(Profile profile)
        {
            if (profile != null)
            {
                Logger.Info($"Export profile: {profile.Name}");
            }
        }
        private MacroFolder GetCurrentFolder()
        {
            if (selectedFolder != null) return selectedFolder;
            if (selectedMacro != null) return Folders.FirstOrDefault(f => f.Macros.Contains(selectedMacro));
            return Folders.FirstOrDefault(f => f.Name == "General");
        }
        public void StartDrag(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && SelectedTreeItem != null)
            {
                DragDrop.DoDragDrop(e.Source as DependencyObject, SelectedTreeItem, DragDropEffects.Move);
            }
        }
        public void HandleDragOver(DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }
        public void HandleDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Macro)) || e.Data.GetDataPresent(typeof(MacroFolder)))
            {
                var dropped = e.Data.GetData(typeof(object));
                var target = (e.OriginalSource as FrameworkElement)?.DataContext;
                if (dropped is Macro macro && target is MacroFolder newFolder)
                {
                    var oldFolder = Folders.FirstOrDefault(f => f.Macros.Contains(macro));
                    if (oldFolder != null && !oldFolder.IsLocked)
                    {
                        oldFolder.Macros.Remove(macro);
                        newFolder.Macros.Add(macro);
                    }
                }
            }
        }
    }
    public class MacroVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Macro ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => DependencyProperty.UnsetValue;
    }
    public class FolderVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is MacroFolder ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => DependencyProperty.UnsetValue;
    }
}