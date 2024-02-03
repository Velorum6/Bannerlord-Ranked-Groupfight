namespace DoFAdminTools;

public class DoFConfigOptions
{
    public static DoFConfigOptions Instance { get; } = new();

    public string CommandPrefix { get; set; } = "!";
        
    public string BanListFileName { get; set; } = "banlist.txt";

    public bool ShowJoinLeaveMessages { get; set; } = true;
}