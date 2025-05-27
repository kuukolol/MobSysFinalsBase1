namespace MyContact.Services
{
    public interface IDefaultDialerService
    {
        Task<bool> RequestDefaultDialerAsync();
    }
}