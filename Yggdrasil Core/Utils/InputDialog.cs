using System.Windows;
using System.Windows.Controls;

namespace Yggdrasil_Core.Utils
{
    public class InputDialog : Window
    {
        public string Input { get; private set; }

        public InputDialog(string title, string prompt)
        {
            Title = title;
            Width = 300;
            Height = 150;
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var lbl = new TextBlock { Text = prompt, Margin = new Thickness(10) };
            Grid.SetRow(lbl, 0);
            var tb = new TextBox { Margin = new Thickness(10) };
            Grid.SetRow(tb, 1);
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(10) };
            Grid.SetRow(btnPanel, 2);
            var okBtn = new Button { Content = "OK", Margin = new Thickness(0, 0, 5, 0) };
            okBtn.Click += (s, e) => { Input = tb.Text; DialogResult = true; };
            var cancelBtn = new Button { Content = "Cancel" };
            cancelBtn.Click += (s, e) => { DialogResult = false; };
            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);
            grid.Children.Add(lbl);
            grid.Children.Add(tb);
            grid.Children.Add(btnPanel);
            Content = grid;
        }

        public static string Show(string title, string prompt)
        {
            var dlg = new InputDialog(title, prompt);
            if (dlg.ShowDialog() == true)
                return dlg.Input;
            return null;
        }
    }
}