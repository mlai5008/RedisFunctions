namespace WebApiApp.Services.Interfaces
{
    public interface IGenericRedisCacheService<T> where T : class
    {
        Task<T?> GetValueAsync<T>(string key);
        Task SetValueAsync<T>(string key, T value);
    }
}
