using System.ComponentModel;
using System.Reflection;

namespace SmartKey.Domain.Common
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Lấy giá trị mô tả của Enum (Description)
        /// Nếu không có Description, trả về tên của Enum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// Lấy tên Enum (string) thay vì giá trị số
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetName(this Enum value)
        {
            return Enum.GetName(value.GetType(), value) ?? string.Empty;
        }

        /// <summary>
        /// Chuyển từ chuỗi tên hoặc mô tả thành Enum tương ứng
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum
        {
            foreach (var field in typeof(TEnum).GetFields())
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null && string.Equals(attribute.Description, value, StringComparison.OrdinalIgnoreCase))
                    return (TEnum)field.GetValue(null)!;
            }

            if (Enum.TryParse(value, true, out TEnum result))
                return result;

            throw new ArgumentException($"Không tìm thấy giá trị Enum hợp lệ cho '{value}' trong {typeof(TEnum).Name}");
        }
    }
}

// 1. Sử dụng Enum với DescriptionAttribute

//public enum Status
//{
//    [Description("Bản nháp")]
//    Draft = 0,

//    [Description("Đang chạy")]
//    Running = 1,

//    [Description("Hoàn thành")]
//    Completed = 2,

//    [Description("Đã hủy")]
//    Canceled = 3
//}


// 2. Sử dụng các phương thức mở rộng

//var status = Status.Running;
//Console.WriteLine(status.GetName());
// => "Running"

//// Lấy mô tả
//Console.WriteLine(status.GetDescription());
// => "Đang chạy"

//// Chuyển chuỗi mô tả → enum
//var parsed = EnumExtensions.ParseEnum<Status>("Hoàn thành");
// => WorkflowStatus.Completed

//// Hoặc chuyển từ tên enum
//var parsed2 = EnumExtensions.ParseEnum<Status>("Canceled");
// => WorkflowStatus.Canceled