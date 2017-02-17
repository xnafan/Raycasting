using System;
using IWshRuntimeLibrary;

namespace Raycasting
{
    public static class SendToTool
    {
        //http://www.fluxbytes.com/csharp/create-shortcut-programmatically-in-c/
        //CooLMinE
        public static void AddSendToShortcutIfNotPresent(string shortcutName, string shortcutDescription, string targetFileLocation)
        {
            string shortcutLocation = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SendTo), shortcutName + ".lnk");
            if (!System.IO.File.Exists(shortcutLocation))
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

                shortcut.Description = shortcutDescription;   // The description of the shortcut
                shortcut.IconLocation = targetFileLocation;   // The icon of the shortcut
                shortcut.TargetPath = targetFileLocation;     // The path of the file that will launch when the shortcut is run
                shortcut.Save();                              // Save the shortcut
            }
        }
    }
}