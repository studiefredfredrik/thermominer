namespace WindowsGui
{
    public static class ApplicationState
    {
        public static bool TemperatureLimitEnabled;
        public static decimal TemperatureLimit;
        public static bool PostStatusToServer;

        public static string Url;
        public static string Wallet;
        public static string RigName;

        public static decimal? CurrentTemperature;
        public static decimal? CurrentHashrate;

        public static string EthminerFilePath;
        public static string EthminerBatFilePath;
        public static string EthminerDir;

        public static string ComPort;

    }
}
