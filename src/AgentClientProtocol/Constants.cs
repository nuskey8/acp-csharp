namespace AgentClientProtocol;

public static class AgentMethods
{
    public const string Authenticate = "authenticate";
    public const string Initialize = "initialize";
    public const string SessionCancel = "session/cancel";
    public const string SessionLoad = "session/load";
    public const string SessionNew = "session/new";
    public const string SessionPrompt = "session/prompt";
    public const string SessionSetMode = "session/set_mode";
    public const string SessionSetModel = "session/set_model";
}

public static class ClientMethods
{
    public const string FsReadTextFile = "fs/read_text_file";
    public const string FsWriteTextFile = "fs/write_text_file";
    public const string SessionRequestPermission = "session/request_permission";
    public const string SessionUpdate = "session/update";
    public const string TerminalCreate = "terminal/create";
    public const string TerminalKill = "terminal/kill";
    public const string TerminalOutput = "terminal/output";
    public const string TerminalRelease = "terminal/release";
    public const string TerminalWaitForExit = "terminal/wait_for_exit";
}