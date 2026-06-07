namespace vodongha.Services;

public class AdminLocalizationService
{
    public string Lang { get; private set; } = "VI";
    public event Func<Task>? OnChanged;

    public void SetLang(string lang)
    {
        if (Lang == lang) return;
        Lang = lang;
        _ = OnChanged?.Invoke();
    }

    public void Toggle() => SetLang(Lang == "VI" ? "EN" : "VI");

    public string T(string key)
    {
        if (Lang == "EN") return key;
        return _vi.TryGetValue(key, out string? val) ? val : key;
    }

    private static readonly Dictionary<string, string> _vi = new()
    {
        // Navigation
        ["Dashboard"] = "Dashboard",
        ["Skills"] = "Kỹ năng",
        ["Projects"] = "Dự án",
        ["Education"] = "Học vấn",
        ["Experience"] = "Kinh nghiệm",
        ["Blog"] = "Blog",
        ["Messages"] = "Tin nhắn",
        ["Chats"] = "Chat",
        ["Settings"] = "Thông tin",
        ["Logout"] = "Đăng xuất",
        ["View website"] = "Xem website",

        // Common
        ["page"] = "trang",
        ["Search..."] = "Tìm kiếm...",
        ["Add new"] = "Thêm mới",
        ["Save"] = "Lưu",
        ["Save all"] = "Lưu tất cả",
        ["Cancel"] = "Huỷ",
        ["Delete"] = "Xoá",
        ["Actions"] = "Hành động",
        ["Order"] = "Thứ tự",
        ["Name"] = "Tên",
        ["Email"] = "Email",
        ["Phone"] = "Điện thoại",
        ["Date"] = "Ngày",
        ["Description VI"] = "Mô tả 🇻🇳",
        ["Description EN"] = "Mô tả 🇬🇧",
        ["Website URL"] = "Website URL",

        // Skills
        ["Skill name"] = "Tên kỹ năng",
        ["Level"] = "Cấp độ",
        ["Add skill"] = "Thêm kỹ năng",
        ["Edit skill"] = "Sửa kỹ năng",
        ["Search skills..."] = "Tìm kỹ năng...",

        // Projects
        ["Title"] = "Tiêu đề",
        ["Technologies"] = "Công nghệ",
        ["Featured"] = "Nổi bật",
        ["Add project"] = "Thêm dự án",
        ["Edit project"] = "Sửa dự án",
        ["Title EN"] = "Tiêu đề 🇬🇧",
        ["Project URL"] = "URL dự án",
        ["GitHub URL"] = "GitHub URL",
        ["Image URL"] = "URL ảnh",
        ["Search projects..."] = "Tìm dự án...",

        // Education
        ["School"] = "Trường",
        ["Degree"] = "Bằng cấp",
        ["Field"] = "Ngành",
        ["Year"] = "Năm",
        ["Start year"] = "Năm bắt đầu",
        ["End year"] = "Năm kết thúc",
        ["Add education"] = "Thêm học vấn",
        ["Edit education"] = "Sửa học vấn",
        ["Present"] = "Hiện tại",

        // Experience
        ["Company"] = "Công ty",
        ["Role"] = "Vai trò",
        ["Location"] = "Địa điểm",
        ["Duration"] = "Thời gian",
        ["Start month"] = "Tháng bắt đầu",
        ["End month"] = "Tháng kết thúc",
        ["Currently working"] = "Đang làm việc",
        ["Add experience"] = "Thêm kinh nghiệm",
        ["Edit experience"] = "Sửa kinh nghiệm",

        // Blog
        ["Slug"] = "Slug",
        ["Tags"] = "Tags",
        ["Published"] = "Published",
        ["Created date"] = "Ngày tạo",
        ["Summary VI"] = "Tóm tắt 🇻🇳",
        ["Summary EN"] = "Tóm tắt 🇬🇧",
        ["Content VI"] = "Nội dung 🇻🇳",
        ["Content EN"] = "Nội dung 🇬🇧",
        ["Cover Image URL"] = "Cover Image URL",
        ["Add post"] = "Thêm bài viết",
        ["Edit post"] = "Sửa bài viết",
        ["Tags (comma separated)"] = "Tags (phân cách bằng dấu phẩy)",
        ["Search title, tags..."] = "Tìm tiêu đề, tags...",

        // Contacts
        ["Subject"] = "Tiêu đề",
        ["Status"] = "Trạng thái",
        ["Read"] = "Đã đọc",
        ["Unread"] = "Chưa đọc",
        ["unread"] = "chưa đọc",
        ["Mark all read"] = "Đánh dấu tất cả đã đọc",
        ["No messages yet."] = "Chưa có tin nhắn.",
        ["View"] = "Xem",
        ["Search name, email, subject..."] = "Tìm tên, email, tiêu đề...",

        // Chats
        ["Live Chats"] = "Trò chuyện",
        ["No chat sessions yet."] = "Chưa có cuộc trò chuyện.",
        ["Select a conversation to view"] = "Chọn một cuộc trò chuyện để xem",
        ["Type a reply..."] = "Nhập tin nhắn trả lời...",
        ["Send"] = "Gửi",
        ["New messages"] = "Tin nhắn mới",
        ["Delete conversation"] = "Xoá cuộc trò chuyện",
        ["No messages yet"] = "Chưa có tin nhắn",
        ["User"] = "User",
        ["Admin"] = "Admin",

        // Health Monitor
        ["Server Status"] = "Trạng thái Server",
        ["Health"] = "Monitor",
        ["Auto-refresh"] = "Tự động làm mới",
        ["Refresh"] = "Làm mới",
        ["Server"] = "Server",
        ["Database"] = "Cơ sở dữ liệu",
        ["Uptime"] = "Thời gian hoạt động",
        ["Memory"] = "Bộ nhớ",
        ["Threads"] = "Luồng",
        ["Started at"] = "Khởi động lúc",
        ["Online"] = "Hoạt động",
        ["Unreachable"] = "Không kết nối",
        ["Memory Usage"] = "Sử dụng bộ nhớ",
        ["DB Response Time"] = "Thời gian phản hồi DB",
        ["Recent snapshots"] = "Lịch sử gần đây",
        ["Time"] = "Thời gian",
        ["DB Ping"] = "DB Ping",
        ["items"] = "mục",
        ["Page"] = "Trang",

        // Settings
        ["Personal information"] = "Thông tin cá nhân",
        ["Introduction"] = "Giới thiệu",
        ["Contact"] = "Liên hệ",
        ["Social media"] = "Mạng xã hội",
        ["Full name"] = "Tên",
        ["Job title"] = "Chức danh",
        ["Tagline"] = "Tagline",
        ["Bio VI"] = "Bio 🇻🇳",
        ["Bio EN"] = "Bio 🇬🇧",
        ["Address"] = "Địa điểm",
        ["Change photo"] = "Đổi ảnh",
        ["LinkedIn URL"] = "LinkedIn URL",
        ["Facebook URL"] = "Facebook URL",
    };
}
