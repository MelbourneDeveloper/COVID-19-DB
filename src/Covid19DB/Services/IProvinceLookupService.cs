namespace Covid19DB.Services
{
    public interface IProvinceLookupService
    {
        string GetProvinceName(string country, string provinceCode);
    }
}