using Microsoft.Win32;
using System;

namespace WindowsTaskbarApp
{
    public static class ContextMenuManager
    {
        private const string GitBashMenuKeyPath = @"*\shell\RunInGitBash";
        private const string CmdMenuKeyPath = @"*\shell\RunInCmd";
        private const string GitBashCommandKeyPath = @"*\shell\RunInGitBash\command";
        private const string CmdCommandKeyPath = @"*\shell\RunInCmd\command";
        private const string ClipboardCmdKeyPath = @"Directory\Background\shell\RunClipboardInCmd";
        private const string ClipboardGitBashKeyPath = @"Directory\Background\shell\RunClipboardInGitBash";
        private const string ClipboardCmdCommandKeyPath = @"Directory\Background\shell\RunClipboardInCmd\command";
        private const string ClipboardGitBashCommandKeyPath = @"Directory\Background\shell\RunClipboardInGitBash\command";

        public static void AddContextMenus(string gitBashScriptPath, string cmdScriptPath, string appPath)
        {
            try
            {
                // Add Git Bash context menu
                using (var gitBashKey = Registry.ClassesRoot.CreateSubKey(GitBashMenuKeyPath))
                {
                    if (gitBashKey != null)
                    {
                        gitBashKey.SetValue("", "Run in Git Bash");
                    }
                }
                using (var gitBashCommandKey = Registry.ClassesRoot.CreateSubKey(GitBashCommandKeyPath))
                {
                    if (gitBashCommandKey != null)
                    {
                        gitBashCommandKey.SetValue("", $"\"C:\\Program Files\\Git\\bin\\bash.exe\" -c \"\\\"{gitBashScriptPath}\\\" \\\"%1\\\"\"");
                    }
                }
                // Add cmd context menu
                using (var cmdKey = Registry.ClassesRoot.CreateSubKey(CmdMenuKeyPath))
                {
                    if (cmdKey != null)
                    {
                        cmdKey.SetValue("", "Run in CMD");
                    }
                }
                using (var cmdCommandKey = Registry.ClassesRoot.CreateSubKey(CmdCommandKeyPath))
                {
                    if (cmdCommandKey != null)
                    {
                        cmdCommandKey.SetValue("", $"cmd.exe /k \"{cmdScriptPath}\" \"%1\"");
                    }
                }
                // Add CMD context menu for clipboard
                using (var clipboardCmdKey = Registry.ClassesRoot.CreateSubKey(ClipboardCmdKeyPath))
                {
                    if (clipboardCmdKey != null)
                    {
                        clipboardCmdKey.SetValue("", "Run Clipboard in CMD");
                    }
                }
                using (var clipboardCmdCommandKey = Registry.ClassesRoot.CreateSubKey(ClipboardCmdCommandKeyPath))
                {
                    if (clipboardCmdCommandKey != null)
                    {
                        clipboardCmdCommandKey.SetValue("", $"\"{appPath}\" cmd");
                    }
                }
                // Add Git Bash context menu for clipboard
                using (var clipboardGitBashKey = Registry.ClassesRoot.CreateSubKey(ClipboardGitBashKeyPath))
                {
                    if (clipboardGitBashKey != null)
                    {
                        clipboardGitBashKey.SetValue("", "Run Clipboard in Git Bash");
                    }
                }
                using (var clipboardGitBashCommandKey = Registry.ClassesRoot.CreateSubKey(ClipboardGitBashCommandKeyPath))
                {
                    if (clipboardGitBashCommandKey != null)
                    {
                        clipboardGitBashCommandKey.SetValue("", $"\"{appPath}\" gitbash");
                    }
                }

                Console.WriteLine("Context menus added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding context menus: {ex.Message}");
            }
        }

        public static void RemoveContextMenus()
        {
            try
            {
                // Remove Git Bash context menu
                Registry.ClassesRoot.DeleteSubKeyTree(GitBashMenuKeyPath, false);
                // Remove cmd context menu
                Registry.ClassesRoot.DeleteSubKeyTree(CmdMenuKeyPath, false);
                // Remove clipboard context menus
                Registry.ClassesRoot.DeleteSubKeyTree(ClipboardCmdKeyPath, false);
                Registry.ClassesRoot.DeleteSubKeyTree(ClipboardGitBashKeyPath, false);
                Console.WriteLine("Context menus removed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing context menus: {ex.Message}");
            }
        }
    }
}