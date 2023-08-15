namespace Network.Account.Serialized
{
    public class VerifyRequest
    {
        public bool status { get; set; }
        public string verifyToken { get; set; }
        public StatusCode Code { get; set; }
    }
}