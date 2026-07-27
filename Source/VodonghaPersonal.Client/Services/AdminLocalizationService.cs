namespace VodonghaPersonal.Client.Services;

public class AdminLocalizationService
{
    public string Lang { get; private set; } = "VI";
    public event Func<Task>? OnChanged;

    public void SetLang(string lang)
    {
        if (Lang == lang)
        {
            return;
        }

        Lang = lang;
        _ = OnChanged?.Invoke();
    }

    public void Toggle() => SetLang(Lang == "VI" ? "EN" : "VI");

    public string T(string key)
    {
        if (Lang == "EN")
        {
            return key;
        }

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
        ["CV"] = "CV",
        ["Messages"] = "Tin nhắn",
        ["Chats"] = "Chat",
        ["Settings"] = "Cài đặt",
        ["Logout"] = "Đăng xuất",
        ["View website"] = "Website",

        // Common
        ["page"] = "trang",
        ["Search..."] = "Tìm kiếm...",
        ["No results"] = "Không tìm thấy kết quả",
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
        ["Category"] = "Danh mục",
        ["Icon (devicon class)"] = "Icon (devicon class)",
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
        ["Live URL"] = "Live URL",
        ["Search projects..."] = "Tìm dự án...",
        ["Drag to reorder"] = "Kéo để sắp xếp",
        ["Order saved"] = "Đã lưu thứ tự",

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
        ["Search company, role..."] = "Tìm kiếm công ty, vai trò...",

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
        ["Marked all as read"] = "Đã đánh dấu tất cả đã đọc",
        ["No messages yet."] = "Chưa có tin nhắn.",
        ["View"] = "Xem",
        ["From:"] = "Từ:",
        ["Reply"] = "Trả lời",
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
        ["Back"] = "Quay lại",
        ["Sent"] = "Đã gửi",

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

        // Infrastructure Costs
        ["Infrastructure Costs"] = "Chi phí hạ tầng",
        ["Cache"] = "Cache",
        ["min"] = "phút",
        ["Est. this month"] = "Ước tính tháng này",
        ["Day 1–"] = "Ngày 1–",
        ["Updated"] = "Cập nhật",
        ["Free tier"] = "Miễn phí",
        ["Project"] = "Dự án",
        ["Plan"] = "Gói",
        ["Region"] = "Khu vực",
        ["Storage used"] = "Bộ nhớ đã dùng",
        ["Storage usage"] = "Mức sử dụng bộ nhớ",
        ["free"] = "miễn phí",
        ["Est. cost / month"] = "Ước tính chi phí / tháng",
        ["Estimated max is theoretical (24/7). With <code>auto_stop_machines = &quot;suspend&quot;</code> actual cost is significantly lower."] = "Ước tính tối đa là lý thuyết (24/7). Với <code>auto_stop_machines = &quot;suspend&quot;</code> chi phí thực tế thấp hơn đáng kể.",
        ["Console"] = "Console",

        // API Keys
        ["API Keys"] = "API Keys",

        // Dependencies
        ["Dependencies"] = "Thư viện",
        ["Package"] = "Gói",
        ["Current"] = "Hiện tại",
        ["Latest"] = "Mới nhất",
        ["outdated"] = "lỗi thời",
        ["up-to-date"] = "mới nhất",
        ["unknown"] = "không xác định",
        ["Checking versions..."] = "Đang kiểm tra phiên bản...",
        ["UpToDate"] = "Mới nhất",
        ["All"] = "Tất cả",
        ["Outdated"] = "Lỗi thời",
        ["Unknown"] = "Không xác định",
        ["NuGet"] = "NuGet",
        ["Npm"] = "npm",
        ["Cdn"] = "CDN",
        ["GitHubActions"] = "GitHub Actions",
        ["hour"] = "giờ",
        ["Costs"] = "Chi phí",
        ["Changes apply immediately"] = "Thay đổi có hiệu lực ngay",
        ["Values are encrypted in the database. DB overrides take effect immediately without redeployment. Leave blank to fall back to environment variables."] = "Giá trị được mã hóa trong cơ sở dữ liệu. Ghi đè DB có hiệu lực ngay không cần triển khai lại. Để trống để dùng biến môi trường.",
        ["Overridden in database"] = "Đang ghi đè trong DB",
        ["Set via environment variable"] = "Được đặt qua biến môi trường",
        ["Not set"] = "Chưa cấu hình",
        ["from DB"] = "từ DB",
        ["from ENV"] = "từ ENV",
        ["Edit"] = "Sửa",
        ["Remove DB override, fall back to ENV"] = "Xoá ghi đè DB, dùng lại ENV",
        ["Save failed. Check logs."] = "Lưu thất bại. Kiểm tra logs.",
        ["Saved successfully"] = "Lưu thành công",
        ["DB override removed"] = "Đã xoá ghi đè DB",

        // Dashboard
        ["Unique visitors"] = "Lượt khách duy nhất",
        ["Views (30 days)"] = "Lượt xem (30 ngày)",
        ["Blog reads"] = "Lượt đọc blog",
        ["msg"] = "tin",
        ["chat"] = "chat",
        ["Skills by category"] = "Kỹ năng theo danh mục",
        ["Blog posts by views"] = "Bài viết theo lượt xem",
        ["Page views — last 14 days"] = "Lượt xem trang — 14 ngày qua",
        ["Content overview"] = "Tổng quan nội dung",
        ["Blog posts"] = "Bài viết",
        ["Experiences"] = "Kinh nghiệm",
        ["Educations"] = "Học vấn",
        ["Unread msgs"] = "Tin chưa đọc",
        ["Recent contacts"] = "Liên hệ gần đây",
        ["View all →"] = "Xem tất cả →",

        // Nav
        ["Menu"] = "Menu",

        // Nav groups
        ["Portfolio"] = "Portfolio",
        ["Communication"] = "Liên lạc",
        ["Insights"] = "Thống kê",
        ["System"] = "Hệ thống",

        // Analytics
        ["Analytics"] = "Phân tích",
        ["days"] = "ngày",
        ["Last @_days days"] = "@_days ngày qua",
        ["page views"] = "lượt xem",
        ["All time"] = "Tổng cộng",
        ["Daily avg"] = "Trung bình / ngày",
        ["views / day"] = "lượt / ngày",
        ["Daily views"] = "Lượt xem theo ngày",
        ["Top pages"] = "Trang nhiều lượt nhất",
        ["Top countries"] = "Quốc gia",
        ["Top referrers"] = "Nguồn truy cập",
        ["No data yet"] = "Chưa có dữ liệu",
        ["No geo data yet — refreshes as visitors arrive"] = "Chưa có dữ liệu — cập nhật khi có visitor",
        ["No referrer data yet"] = "Chưa có dữ liệu",

        // Common toast
        ["Deleted"] = "Đã xoá",

        // Settings
        ["Personal information"] = "Thông tin cá nhân",
        ["Click to change photo"] = "Bấm để đổi ảnh",
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

        // CV
        ["CV / Resume"] = "CV / Resume",
        ["Download PDF"] = "Tải PDF",
        ["Templates"] = "Mẫu CV",
        ["Click a template, then Download PDF"] = "Chọn mẫu rồi tải PDF",
        ["Preview shows data that will be included in the PDF. Edit content in Settings, Skills, Experience, Education and Projects pages."] = "Xem trước dữ liệu sẽ được đưa vào PDF. Chỉnh sửa trong trang Settings, Skills, Experience, Education và Projects.",
        ["Profile"] = "Hồ sơ",
        ["Featured Projects"] = "Dự án nổi bật",
        ["Profile saved successfully"] = "Đã lưu hồ sơ thành công",
        ["Avatar uploaded successfully"] = "Đã tải ảnh lên thành công",
        ["Error"] = "Lỗi",
    };
}
