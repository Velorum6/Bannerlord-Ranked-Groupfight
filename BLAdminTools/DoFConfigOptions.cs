namespace DoFAdminTools
{
    public class DoFConfigOptions
    {
        public static DoFConfigOptions Instance { get; } = new DoFConfigOptions();

        public string CommandPrefix { get; set; } = "!";

        public bool ShowJoinLeaveMessages { get; set; } = true;
    }
}