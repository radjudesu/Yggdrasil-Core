using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;

namespace Yggdrasil_Core.ViewModels
{
    public class AutoCompletionHandler
    {
        private readonly TextEditor editor;
        private readonly List<string> variables;
        private readonly string[] roots;
        private readonly string[] actions;
        private readonly string[] mouseBtns;
        private readonly string[] keys;
        private readonly string[] operators;
        private CompletionWindow completionWindow;

        public AutoCompletionHandler(TextEditor editor, List<string> variables, string[] roots, string[] actions, string[] mouseBtns, string[] keys, string[] operators)
        {
            this.editor = editor;
            this.variables = variables;
            this.roots = roots;
            this.actions = actions;
            this.mouseBtns = mouseBtns;
            this.keys = keys;
            this.operators = operators;

            editor.TextArea.TextEntered += TextEntered;
            editor.TextArea.PreviewKeyDown += PreviewKeyDown;
        }

        private void TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (completionWindow != null) return;
            if (char.IsLetterOrDigit(e.Text[0]) || char.IsPunctuation(e.Text[0]))
            {
                int caret = editor.CaretOffset;
                int wordStart = caret;
                while (wordStart > 0 && !char.IsWhiteSpace(editor.Document.GetCharAt(wordStart - 1)))
                {
                    wordStart--;
                }
                string currentWord = editor.Document.GetText(wordStart, caret - wordStart);
                var suggestions = GetSuggestions(currentWord);
                if (suggestions.Any())
                {
                    completionWindow = new CompletionWindow(editor.TextArea);
                    completionWindow.StartOffset = wordStart;
                    completionWindow.EndOffset = caret;
                    foreach (var s in suggestions)
                    {
                        completionWindow.CompletionList.CompletionData.Add(new CompletionData(s));
                    }
                    completionWindow.Show();
                    completionWindow.Closed += (s, args) => completionWindow = null;
                }
            }
            if (e.Text == ".")
            {
                int dotOffset = editor.CaretOffset - 1;
                int prefixStart = dotOffset;
                while (prefixStart > 0 && !char.IsWhiteSpace(editor.Document.GetCharAt(prefixStart - 1)))
                {
                    prefixStart--;
                }
                string prefix = editor.Document.GetText(prefixStart, dotOffset - prefixStart);
                var subSuggestions = GetSubSuggestions(prefix);
                if (subSuggestions.Any())
                {
                    completionWindow = new CompletionWindow(editor.TextArea);
                    completionWindow.StartOffset = editor.CaretOffset;
                    completionWindow.EndOffset = editor.CaretOffset;
                    foreach (var s in subSuggestions)
                    {
                        completionWindow.CompletionList.CompletionData.Add(new CompletionData(s));
                    }
                    completionWindow.Show();
                    completionWindow.Closed += (s, args) => completionWindow = null;
                }
            }
            if (e.Text == " ")
            {
                int spaceOffset = editor.CaretOffset - 1;
                int prevWordStart = spaceOffset;
                while (prevWordStart > 0 && !char.IsWhiteSpace(editor.Document.GetCharAt(prevWordStart - 1)))
                {
                    prevWordStart--;
                }
                string prevWord = editor.Document.GetText(prevWordStart, spaceOffset - prevWordStart);
                var contextSuggestions = GetContextSuggestionsAfterSpace(prevWord);
                if (contextSuggestions.Any())
                {
                    completionWindow = new CompletionWindow(editor.TextArea);
                    completionWindow.StartOffset = editor.CaretOffset;
                    completionWindow.EndOffset = editor.CaretOffset;
                    foreach (var s in contextSuggestions)
                    {
                        completionWindow.CompletionList.CompletionData.Add(new CompletionData(s));
                    }
                    completionWindow.Show();
                    completionWindow.Closed += (s, args) => completionWindow = null;
                }
            }
        }
        private void PreviewKeyDown(object sender, KeyEventArgs e)
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
        private List<string> GetSuggestions(string currentWord)
        {
            var suggestions = new List<string>();
            var lowerLastWord = currentWord.ToLower();
            suggestions.AddRange(roots.Where(r => r.ToLower().StartsWith(lowerLastWord)));
            return suggestions;
        }
        private List<string> GetSubSuggestions(string prefix)
        {
            var suggestions = new List<string>();
            var lowerPrefix = prefix.ToLower();
            if (lowerPrefix == "keyboard")
            {
                suggestions.AddRange(keys);
                suggestions.AddRange(actions);
            }
            else if (lowerPrefix == "mouseclick")
            {
                suggestions.AddRange(mouseBtns.Select(b => b + " "));
            }
            return suggestions;
        }
        private List<string> GetContextSuggestionsAfterSpace(string prevWord)
        {
            var suggestions = new List<string>();
            var lowerPrev = prevWord.ToLower();
            if (keys.Any(k => lowerPrev == k.ToLower()) && GetLinePrefixLower().Contains("keyboard."))
            {
                suggestions.AddRange(actions);
            }
            if (lowerPrev == "if")
            {
                suggestions.AddRange(variables);
            }
            if (variables.Any(v => lowerPrev == v.ToLower()) && GetLinePrefixLower().Contains("if "))
            {
                suggestions.AddRange(operators);
            }
            return suggestions;
        }
        private string GetLinePrefixLower()
        {
            var line = editor.Document.GetLineByOffset(editor.CaretOffset);
            return editor.Document.GetText(line.Offset, editor.CaretOffset - line.Offset).ToLower();
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
}