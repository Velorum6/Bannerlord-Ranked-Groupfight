using TaleWorlds.Library;

namespace DoFAdminTools
{
    public static class Helper
    {
        private const string Prefix = "[DAT] ";
        private const string WarningPrefix = Prefix + "[WARN] ";
        private const string ErrorPrefix = "[ERROR] ";

        public static void Print(string message) => 
            Debug.Print(Prefix + message, 0, Debug.DebugColor.DarkGreen);
        
        
        public static void PrintWarning(string message) => 
            Debug.Print(WarningPrefix + message, 0, Debug.DebugColor.DarkYellow);
        
        
        public static void PrintError(string message) => 
            Debug.Print(ErrorPrefix + message, 0, Debug.DebugColor.DarkRed);
    }
}