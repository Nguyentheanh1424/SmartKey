using SmartKey.Application.Common.Cache;

namespace SmartKey.Application.Common.Interfaces.Services
{
    public interface ICacheService
    {
        /// <summary>
        /// Lấy dữ liệu từ cache theo khóa
        /// Tự động gia hạn thời gian hết hạn trượt nếu có
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<T?> GetAsync<T>(string cacheKey)
            where T : CacheModelBase;

        /// <summary>
        /// Lưu đối tượng vào cache (có TTL)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="absoluteTtl"></param>
        /// <param name="slidingTtl"></param>
        /// <returns></returns>
        Task SetAsync<T>(T model, TimeSpan? absoluteTtl = null, TimeSpan? slidingTtl = null)
            where T : CacheModelBase;

        /// <summary>
        /// Xóa đối tượng khỏi cache theo khóa
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task RemoveAsync(string cacheKey);

        /// <summary>
        /// Kiểm tra đối tượng có tồn tại trong cache hay không
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string cacheKey);
    }
}
