namespace WinUIMSALApp.Configuration;

public sealed class WinUISettings
{
    public string Authority { get; set; }
    public string MsGraphURL { get; set; }
    public string RedirectURL { get; set; }
    public string ClientId { get; set; }
    public string TenantId { get; set; }
    public string Scopes { get; set; }
    public string CacheFileName { get; set; }
    public string CacheDir { get; set; }
}
