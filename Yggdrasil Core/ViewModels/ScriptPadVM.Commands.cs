// ViewModels/ScriptPadVM.Commands.cs (new file - add this)
using System.Windows.Input;
using Yggdrasil_Core.Models;
using Yggdrasil_Core.ViewModels;

namespace Yggdrasil_Core.ViewModels
{
    public partial class ScriptPadVM
    {
        public ICommand SaveMacroCommand { get; private set; }
        public ICommand ToggleSyntaxCommand { get; private set; }
        public ICommand ToggleThemeCommand { get; private set; }
        public ICommand InsertIfCommand { get; private set; }
        public ICommand InsertHPCommand { get; private set; }
        public ICommand InsertKeyboardCommand { get; private set; }
        public ICommand InsertMouseCommand { get; private set; }
        public ICommand InsertWaitCommand { get; private set; }
        public ICommand InsertFuncCommand { get; private set; }
        public ICommand CreateFolderCommand { get; private set; }
        public ICommand CreateMacroCommand { get; private set; }
        public ICommand RenameCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand ImportMacroCommand { get; private set; }
        public ICommand ExportMacroCommand { get; private set; }
        public ICommand ToggleEnableCommand { get; private set; }
        public ICommand ToggleFolderEnableCommand { get; private set; }
        public ICommand DeleteFolderCommand { get; private set; }
        public ICommand RenameFolderCommand { get; private set; }
        public ICommand ImportFolderCommand { get; private set; }
        public ICommand ExportFolderCommand { get; private set; }
        public ICommand ToggleLockFolderCommand { get; private set; }
        public ICommand SetActiveProfileCommand { get; private set; }
        public ICommand DeleteProfileCommand { get; private set; }
        public ICommand RenameProfileCommand { get; private set; }
        public ICommand ImportProfileCommand { get; private set; }
        public ICommand ExportProfileCommand { get; private set; }
        public ICommand SetStartKey { get; private set; }
        public ICommand SetStopKey { get; private set; }

        private void InitializeCommands()
        {
            SaveMacroCommand = new RelayCmd(_ => SaveMacro());
            ToggleSyntaxCommand = new RelayCmd(_ => ToggleSyntax());
            ToggleThemeCommand = new RelayCmd(_ => ToggleTheme());
            InsertIfCommand = new RelayCmd(_ => InsertText("If <Variable> == <Value>\n{\n}\nEndIf"));
            InsertHPCommand = new RelayCmd(_ => InsertText("If HP == 50%\n{\n    Keyboard.F1 tap\n}\nEndIf"));
            InsertKeyboardCommand = new RelayCmd(_ => InsertText("Keyboard.F1 tap"));
            InsertMouseCommand = new RelayCmd(_ => InsertText("MouseClick.Left tap"));
            InsertWaitCommand = new RelayCmd(_ => InsertText("Wait(1000ms)"));
            InsertFuncCommand = new RelayCmd(_ => InsertText("func example()\n{\n}\nendfunc"));
            CreateFolderCommand = new RelayCmd(_ => CreateFolder());
            CreateMacroCommand = new RelayCmd(_ => CreateMacro());
            RenameCommand = new RelayCmd(p => RenameSelected(p));
            DeleteCommand = new RelayCmd(p => DeleteSelected(p));
            ImportMacroCommand = new RelayCmd(_ => ImportMacro());
            ExportMacroCommand = new RelayCmd(p => ExportMacro(p));
            ToggleEnableCommand = new RelayCmd(p => ToggleEnable(p));
            ToggleFolderEnableCommand = new RelayCmd(p => ToggleEnable(p));
            DeleteFolderCommand = new RelayCmd(p => DeleteSelected(p));
            RenameFolderCommand = new RelayCmd(p => RenameSelected(p));
            ImportFolderCommand = new RelayCmd(_ => ImportFolder());
            ExportFolderCommand = new RelayCmd(p => ExportFolder(p));
            ToggleLockFolderCommand = new RelayCmd(p => ToggleLock(p as MacroFolder));
            SetActiveProfileCommand = new RelayCmd(_ => SetActive(SelectedProfile));
            DeleteProfileCommand = new RelayCmd(_ => DeleteProfile(SelectedProfile));
            RenameProfileCommand = new RelayCmd(_ => RenameProfile(SelectedProfile));
            ImportProfileCommand = new RelayCmd(_ => ImportProfile());
            ExportProfileCommand = new RelayCmd(_ => ExportProfile(SelectedProfile));
            SetStartKey = new RelayCmd(e => { if (e is KeyEventArgs ke) { StartKey = ke.Key.ToString(); ke.Handled = true; } });
            SetStopKey = new RelayCmd(e => { if (e is KeyEventArgs ke) { StopKey = ke.Key.ToString(); ke.Handled = true; } });
        }
    }
}