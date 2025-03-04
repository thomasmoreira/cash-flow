namespace CashFlow.Shared;

public class AppSettings
{
    public Jwt Jwt { get; set; }
    public ServiceUrls ServiceUrls { get; set; }
    public Seq Seq { get; set; }
}

public class Jwt
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
}

public class ServiceUrls
{
    public string Transactions { get; set; }
    public string Consolidation { get; set; }
}

public class Seq
{
    public string Url { get; set; }
}
