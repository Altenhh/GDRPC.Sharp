namespace Tsubasa.Database
{
    public class Configuration
    {
        public string Version;
        
        public AccountState Account { get; set; }

        public bool RpcEnabled { get; set; }

        public Configuration()
        {
            Version = Program.CONFIG_VERSION;

            Account = new AccountState
            {
                Username = "",
                Password = ""
            };

            RpcEnabled = true;
        }
    }
}