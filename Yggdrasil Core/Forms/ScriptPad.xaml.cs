// Forms/ScriptPad.xaml.cs (replace the entire file with this)
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using Yggdrasil_Core.Models;
using Yggdrasil_Core.ViewModels;

namespace Yggdrasil_Core.Forms
{
    public partial class ScriptPad : Window
    {
        public ScriptPad(Macro macro, ObservableCollection<MacroFolder> folders, ObservableCollection<Profile> profiles)
        {
            InitializeComponent();
            DataContext = new ScriptPadVM(macro, folders, profiles, scriptEditor);
            macrosTreeView.PreviewMouseMove += MacrosTreeView_PreviewMouseMove;
            macrosTreeView.DragOver += MacrosTreeView_DragOver;
            macrosTreeView.Drop += MacrosTreeView_Drop;
            startKeyTextBox.PreviewKeyDown += StartKeyTextBox_PreviewKeyDown;
            stopKeyTextBox.PreviewKeyDown += StopKeyTextBox_PreviewKeyDown;
        }

        private void MacrosTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (DataContext is ScriptPadVM vm)
            {
                vm.StartDrag(e);
            }
        }

        private void MacrosTreeView_DragOver(object sender, DragEventArgs e)
        {
            if (DataContext is ScriptPadVM vm)
            {
                vm.HandleDragOver(e);
            }
        }

        private void MacrosTreeView_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is ScriptPadVM vm)
            {
                vm.HandleDrop(e);
            }
        }

        private void StartKeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ScriptPadVM vm)
            {
                vm.StartKey = e.Key.ToString();
                e.Handled = true;
            }
        }

        private void StopKeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ScriptPadVM vm)
            {
                vm.StopKey = e.Key.ToString();
                e.Handled = true;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ScriptPadVM vm)
            {
                vm.AutoSaveIfNeeded();
            }
            base.OnClosing(e);
        }
    }
}