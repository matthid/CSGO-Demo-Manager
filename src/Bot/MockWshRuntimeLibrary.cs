
namespace IWshRuntimeLibrary
{
/*
			WshShell shell = new WshShell();
			IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
			shortcut.Description = "CSGO Demos Manager Suspects BOT";
			shortcut.IconLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "app.ico";
			shortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			shortcut.Save(); */
    public interface IWshShortcut
    {
        string Description {get;set;}
        string IconLocation {get;set;}
        string TargetPath {get;set;}
        void Save();
    }

    public class WshShortcut : IWshShortcut
    {
        public string Description {get;set;}
        public string IconLocation {get;set;}
        public string TargetPath {get;set;}
        public void Save()
        {

        }
    }
    public class WshShell
    {
        public IWshShortcut CreateShortcut(string shortCut)
        {
            return new WshShortcut();
        }
    }
}
