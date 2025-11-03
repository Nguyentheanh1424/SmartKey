namespace SmartKey.Application.Common.Cache
{
    public abstract class CacheModelBase
    {
        /// <summary>
        /// Khóa định danh cho đối tượng được lưu trong cache, ví dụ: "PendingUser"
        /// </summary>
        public string CacheKey { get; protected set; } = string.Empty;
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

        /// <summary>
        /// Hết hạn tuyệt đối: dữ liệu bị xóa sau thời điểm này, dù có truy cập hay không.
        /// </summary>
        public DateTime? AbsoluteExpiresAt { get; protected set; }

        /// <summary>
        /// Hết hạn trượt (sliding): nếu truy cập trong thời gian này, TTL sẽ được gia hạn lại.
        /// </summary>
        public DateTime? SlidingExpiresAt { get; protected set; }

        protected CacheModelBase(string cacheKey, TimeSpan? absoluteTtl = null, TimeSpan? slidingTtl = null)
        {
            CacheKey = cacheKey;
            if (absoluteTtl.HasValue)
            {
                AbsoluteExpiresAt = CreatedAt.Add(absoluteTtl.Value);
            }
            if (slidingTtl.HasValue)
            {
                SlidingExpiresAt = CreatedAt.Add(slidingTtl.Value);
            }
        }

        /// <summary>
        /// Kiểm tra xem đã hết hạn tuyệt đối chưa
        /// </summary>
        /// <returns></returns>
        protected bool isAbsoluteExpired()
        {
            return AbsoluteExpiresAt.HasValue && DateTime.UtcNow >= AbsoluteExpiresAt.Value;
        }

        /// <summary>
        /// Làm mới thời gian hết hạn trượt
        /// Mặc định là 1 giờ nếu không truyền tham số
        /// </summary>
        /// <param name="slidingTtl"></param>
        public void RefreshSlidingExpiration(TimeSpan? slidingTtl)
        {
            if (slidingTtl.HasValue)
                SlidingExpiresAt = DateTime.UtcNow.Add(slidingTtl.Value);
            else
                SlidingExpiresAt = DateTime.UtcNow.AddHours(1);
        }

        public virtual string GetCacheKey()
        {
            return CacheKey;
        }
    }
}
