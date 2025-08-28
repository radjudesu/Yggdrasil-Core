// ViewModels/ScriptPadVM.cs (replace the entire file with this)
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
using System.Text.RegularExpressions;
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
        private CompletionWindow completionWindow;
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
            editor.TextArea.TextEntered += TextArea_TextEntered;
            editor.TextArea.PreviewKeyDown += TextArea_PreviewKeyDown;

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

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (CaretInComment()) return;

            if (e.Text == ".")
            {
                var (tokenStart, token, qualifier) = GetCompletionContext();
                var suggestions = BuildCandidates(qualifier, token);
                ShowCompletionAt(editor.CaretOffset, editor.CaretOffset, suggestions);
                return;
            }

            char ch = e.Text[0];
            if (!char.IsLetterOrDigit(ch)) return;

            var (start, token2, qual) = GetCompletionContext();
            var suggestions2 = BuildCandidates(qual, token2);
            ShowCompletionAt(start, editor.CaretOffset, suggestions2);
        }

        private void TextArea_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var line = editor.Document.GetLineByOffset(editor.CaretOffset);
                var text = editor.Document.GetText(line);
                if (!IsInComment(text))
                {
                    var fixedLine = AutoFixLine(text);
                    editor.Document.Replace(line, fixedLine);
                }
            }

            if (e.Key == Key.Tab && completionWindow != null)
            {
                e.Handled = true;
                completionWindow.CompletionList.RequestInsertion(e);
            }
        }

        private static bool IsWordChar(char c)
            => char.IsLetterOrDigit(c) || c == '_';

        private bool CaretInComment()
        {
            var caret = editor.CaretOffset;
            var line = editor.Document.GetLineByOffset(caret);
            var text = editor.Document.GetText(line.Offset, line.Length);
            int i = 0;
            while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
            return (i < text.Length && text[i] == '#');
        }

        private (int tokenStart, string token, string qualifier) GetCompletionContext()
        {
            int caret = editor.CaretOffset;
            if (caret < 0) return (caret, "", null);

            int start = caret;
            while (start > 0 && IsWordChar(editor.Document.GetCharAt(start - 1)))
                start--;

            string token = editor.Document.GetText(start, caret - start);

            string qualifier = null;
            int dotPos = start - 1;
            if (dotPos >= 0 && editor.Document.GetCharAt(dotPos) == '.')
            {
                int qStart = dotPos;
                while (qStart > 0 && IsWordChar(editor.Document.GetCharAt(qStart - 1)))
                    qStart--;
                qualifier = editor.Document.GetText(qStart, dotPos - qStart);
            }

            return (start, token, qualifier);
        }

        private IEnumerable<string> BuildCandidates(string qualifier, string token)
        {
            if (!string.IsNullOrEmpty(qualifier))
            {
                if (qualifier.Equals("Keyboard", StringComparison.OrdinalIgnoreCase))
                {
                    var set = keys.Concat(actions);
                    return FirstLetterFilter(set, token);
                }
                if (qualifier.Equals("MouseClick", StringComparison.OrdinalIgnoreCase))
                {
                    var set = mouseBtns.Concat(actions);
                    return FirstLetterFilter(set, token);
                }
                return Array.Empty<string>();
            }

            var rootsPlus = roots.AsEnumerable();
            var varsAndOps = variables.Concat(operators);

            var top = rootsPlus.Concat(varsAndOps).Distinct(StringComparer.OrdinalIgnoreCase);

            return FirstLetterFilter(top, token);
        }

        private IEnumerable<string> FirstLetterFilter(IEnumerable<string> items, string token)
        {
            if (string.IsNullOrEmpty(token)) return items;
            char first = char.ToUpperInvariant(token[0]);
            return items.Where(s => s.Length > 0 && char.ToUpperInvariant(s[0]) == first);
        }

        private void ShowCompletionAt(int startOffset, int endOffset, IEnumerable<string> suggestions)
        {
            var list = suggestions?.ToList();
            if (list == null || list.Count == 0) return;

            completionWindow?.Close();
            completionWindow = new CompletionWindow(editor.TextArea)
            {
                StartOffset = startOffset,
                EndOffset = endOffset
            };

            var data = completionWindow.CompletionList.CompletionData;
            foreach (var s in list)
                data.Add(new CompletionData(s));

            if (completionWindow.CompletionList.CompletionData.Count > 0)
                completionWindow.CompletionList.SelectedItem = completionWindow.CompletionList.CompletionData[0];

            completionWindow.Closed += (s, a) => completionWindow = null;
            completionWindow.Show();
        }

        private bool IsInComment(string lineText)
        {
            return lineText.TrimStart().StartsWith("#");
        }

        private string AutoFixLine(string line)
        {
            if (IsInComment(line)) return line;

            line = Regex.Replace(line, @"\bKeyboard\s+", "Keyboard.", RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"\bMouse\s+Left\b", "MouseClick.Left tap", RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"\bMouse\s+Right\b", "MouseClick.Right tap", RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"\bMouseClick\.\s*Left\s*click\b", "MouseClick.Left tap", RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"\bwait\(", "Wait(", RegexOptions.IgnoreCase);

            return NormalizeCase(line);
        }

        private string NormalizeCase(string line)
        {
            foreach (var root in roots)
            {
                line = Regex.Replace(line, $@"\b{Regex.Escape(root.ToLower())}\b", root, RegexOptions.IgnoreCase);
            }
            return line;
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

    public class CompletionData : ICompletionData
    {
        public string Text { get; }
        public object Content => Text;
        public object Description => null;
        public double Priority => 0;
        public ImageSource Image => null;

        public CompletionData(string text)
        {
            Text = text;
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
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