namespace SmartKey.Application.Common.Cache
{
    public abstract class CacheModelBase
    {
        /// <summary>
        /// Khóa định danh cho đối tượng được lưu trong cache, ví dụ: "PendingUser"
        /// </summary>
        public string CacheKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Hết hạn tuyệt đối: dữ liệu bị xóa sau thời điểm này, dù có truy cập hay không.
        /// </summary>
        public DateTime? AbsoluteExpiresAt { get; set; }

        /// <summary>
        /// Hết hạn trượt (sliding): nếu truy cập trong thời gian này, TTL sẽ được gia hạn lại.
        /// </summary>
        public DateTime? SlidingExpiresAt { get; set; }

        /// <summary>
        /// Hàm khởi tạo dành cho Json deserialize
        /// </summary>
        protected CacheModelBase() { }

        protected CacheModelBase(string cacheKey, TimeSpan? absoluteExpiresAt = null, TimeSpan? slidingExpiresAt = null)
        {
            CacheKey = cacheKey;
            if (absoluteExpiresAt.HasValue)
            {
                AbsoluteExpiresAt = CreatedAt.Add(absoluteExpiresAt.Value);
            }
            if (slidingExpiresAt.HasValue)
            {
                SlidingExpiresAt = CreatedAt.Add(slidingExpiresAt.Value);
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
        /// <param name="slidingExpiresAt"></param>
        public void RefreshSlidingExpiration(TimeSpan? slidingExpiresAt)
        {
            if (slidingExpiresAt.HasValue)
                SlidingExpiresAt = DateTime.UtcNow.Add(slidingExpiresAt.Value);
            else
                SlidingExpiresAt = DateTime.UtcNow.AddHours(1);
        }

        public virtual string GetCacheKey()
        {
            return CacheKey;
        }
    }
}
