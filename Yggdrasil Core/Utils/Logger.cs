using Serilog;
using System;
using System.Windows;

namespace Yggdrasil_Core.Utils
{
    public static class Logger
    {
        private static ILogger log;
        private static HandyControl.Controls.TextBox consoleLog;

        public static void Init(HandyControl.Controls.TextBox console)
        {
            consoleLog = console;
            log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        public static void Info(string msg)
        {
            log.Information(msg);
            UpdateConsole(msg);
        }

        public static void Error(string msg)
        {
            log.Error(msg);
            UpdateConsole($"ERROR: {msg}");
        }

        private static void UpdateConsole(string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                consoleLog.AppendText($"{DateTime.Now}: {msg}\n");
                consoleLog.ScrollToEnd();
            });
        }
    }
}