namespace vodongha.Services;

public class LanguageService
{
    private string _lang = "en";
    public string Current => _lang;
    public bool IsVi => _lang == "vi";

    public event Action? OnChange;

    public void Set(string lang)
    {
        _lang = lang;
        OnChange?.Invoke();
    }

    public string T(string key) => _lang == "vi"
        ? Vi.GetValueOrDefault(key, key)
        : En.GetValueOrDefault(key, key);

    private static readonly Dictionary<string, string> Vi = new()
    {
        // Nav
        ["nav.skills"]      = "Kỹ năng",
        ["nav.projects"]    = "Dự án",
        ["nav.experience"]  = "Kinh nghiệm",
        ["nav.blog"]        = "Blog",
        ["nav.contact"]     = "Liên hệ",
        // Hero
        ["hero.greeting"]   = "👋 Xin chào, tôi là",
        ["hero.role"]       = "Building modern web experiences",
        ["hero.bio"]        = "Tôi xây dựng các ứng dụng web hiện đại với .NET, Blazor và PostgreSQL. Đam mê tạo ra những sản phẩm sạch, hiệu quả và đẹp mắt.",
        ["hero.cta.view"]   = "Xem dự án",
        ["hero.cta.contact"]= "Liên hệ",
        // Skills
        ["skills.label"]    = "Năng lực",
        ["skills.title"]    = "Kỹ năng & Công nghệ",
        ["skills.subtitle"] = "Các công nghệ tôi sử dụng để xây dựng sản phẩm",
        // Projects
        ["projects.label"]  = "Portfolio",
        ["projects.title"]  = "Dự án nổi bật",
        ["projects.subtitle"]= "Những dự án tôi đã và đang phát triển",
        ["projects.github"] = "GitHub",
        ["projects.live"]   = "Xem trực tiếp",
        ["projects.tech"]   = "Công nghệ",
        // Blog
        ["blog.label"]      = "Bài viết",
        ["blog.title"]      = "Blog",
        ["blog.subtitle"]   = "Chia sẻ kiến thức và kinh nghiệm",
        ["blog.read"]           = "Đọc tiếp",
        ["blog.readtime"]       = "phút đọc",
        ["blog.back"]           = "Quay lại",
        ["blog.updated"]        = "Cập nhật",
        ["blog.search"]         = "Tìm kiếm bài viết...",
        ["blog.filter.all"]     = "Tất cả",
        ["blog.results.empty"]  = "Không tìm thấy bài viết nào.",
        ["blog.empty"]          = "Chưa có bài viết nào.",
        ["blog.toc.title"]      = "Mục lục",
        ["blog.views"]          = "lượt xem",
        ["blog.related.title"]  = "Bài viết liên quan",
        ["blog.viewall"]        = "Xem tất cả bài viết",
        ["blog.page.prev"]      = "Trước",
        ["blog.page.next"]      = "Tiếp",
        ["blog.page.of"]        = "trong",
        ["blog.page.count"]     = "bài viết",
        ["blog.share.title"]    = "Chia sẻ bài viết",
        ["blog.share.copy"]     = "Sao chép liên kết",
        ["blog.share.copied"]   = "Đã sao chép!",
        ["blog.code.copy"]      = "Sao chép",
        ["blog.code.copied"]    = "Đã sao chép!",
        // Contact
        ["contact.label"]   = "Liên hệ",
        ["contact.title"]   = "Liên hệ với tôi",
        ["contact.subtitle"]= "Có dự án muốn hợp tác? Hãy nhắn tin cho tôi!",
        ["contact.name"]    = "Họ tên",
        ["contact.email"]   = "Email",
        ["contact.message"] = "Tin nhắn",
        ["contact.send"]    = "Gửi tin nhắn",
        ["contact.success"] = "Tin nhắn đã được gửi thành công!",
        ["contact.error"]   = "Có lỗi xảy ra, vui lòng thử lại.",
        // Experience
        ["exp.label"]       = "Kinh nghiệm",
        ["exp.title"]       = "Kinh nghiệm làm việc",
        ["exp.present"]     = "Hiện tại",
        // Education
        ["edu.label"]       = "Học vấn",
        ["edu.title"]       = "Trình độ học vấn",
        // Expand / collapse
        ["common.showmore"] = "Xem thêm",
        ["common.showless"] = "Thu gọn",
        // Footer
        ["footer.rights"]   = "Tất cả quyền được bảo lưu.",
        ["footer.built"]    = "Xây dựng với Claude",
        // Chat widget
        ["chat.title"]          = "Võ Đông Hà",
        ["chat.status"]         = "● Online",
        ["chat.close"]          = "Đóng",
        ["chat.form.title"]     = "Bắt đầu cuộc trò chuyện",
        ["chat.form.subtitle"]  = "Điền thông tin để mình có thể liên hệ lại với bạn.",
        ["chat.form.name"]      = "Họ tên *",
        ["chat.form.phone"]     = "Số điện thoại *",
        ["chat.form.email"]     = "Email *",
        ["chat.form.name.placeholder"]  = "Nguyễn Văn A",
        ["chat.form.phone.placeholder"] = "9xxxxxxxx",
        ["chat.form.name.required"]  = "Vui lòng nhập họ tên",
        ["chat.form.phone.invalid"] = "Số điện thoại không hợp lệ",
        ["chat.form.email.invalid"] = "Email không hợp lệ",
        ["chat.form.start"]     = "Bắt đầu chat",
        ["chat.empty"]          = "Gửi tin nhắn để bắt đầu cuộc trò chuyện 👋",
        ["chat.placeholder"]    = "Nhập tin nhắn...",
        ["chat.new"]            = "Tin nhắn mới",
        ["chat.today"]          = "Hôm nay",
        ["chat.yesterday"]      = "Hôm qua",
        ["chat.sent"]           = "Đã gửi",
        ["chat.read"]           = "Đã đọc",
        ["chat.push.denied"]    = "Thông báo bị chặn. Bật lại để nhận tin nhắn khi có phản hồi.",
        ["chat.push.howto"]     = "Hướng dẫn",
        ["chat.fab.title"]      = "Chat với mình",
    };

    private static readonly Dictionary<string, string> En = new()
    {
        // Nav
        ["nav.skills"]      = "Skills",
        ["nav.projects"]    = "Projects",
        ["nav.experience"]  = "Experience",
        ["nav.blog"]        = "Blog",
        ["nav.contact"]     = "Contact",
        // Hero
        ["hero.greeting"]   = "👋 Hi, I'm",
        ["hero.role"]       = "Building modern web experiences",
        ["hero.bio"]        = "I build modern web applications with .NET, Blazor, and PostgreSQL. Passionate about creating clean, efficient, and beautiful products.",
        ["hero.cta.view"]   = "View Projects",
        ["hero.cta.contact"]= "Contact Me",
        // Skills
        ["skills.label"]    = "Expertise",
        ["skills.title"]    = "Skills & Technologies",
        ["skills.subtitle"] = "Technologies I use to build products",
        // Projects
        ["projects.label"]  = "Portfolio",
        ["projects.title"]  = "Featured Projects",
        ["projects.subtitle"]= "Projects I have built and am working on",
        ["projects.github"] = "GitHub",
        ["projects.live"]   = "Live Demo",
        ["projects.tech"]   = "Technologies",
        // Blog
        ["blog.label"]      = "Writing",
        ["blog.title"]      = "Blog",
        ["blog.subtitle"]   = "Sharing knowledge and experience",
        ["blog.read"]           = "Read more",
        ["blog.readtime"]       = "min read",
        ["blog.back"]           = "Back",
        ["blog.updated"]        = "Updated",
        ["blog.search"]         = "Search posts...",
        ["blog.filter.all"]     = "All",
        ["blog.results.empty"]  = "No posts found.",
        ["blog.empty"]          = "No posts yet.",
        ["blog.toc.title"]      = "Contents",
        ["blog.views"]          = "views",
        ["blog.related.title"]  = "Related posts",
        ["blog.viewall"]        = "View all posts",
        ["blog.page.prev"]      = "Previous",
        ["blog.page.next"]      = "Next",
        ["blog.page.of"]        = "of",
        ["blog.page.count"]     = "posts",
        ["blog.share.title"]    = "Share this post",
        ["blog.share.copy"]     = "Copy link",
        ["blog.share.copied"]   = "Copied!",
        ["blog.code.copy"]      = "Copy",
        ["blog.code.copied"]    = "Copied!",
        // Contact
        ["contact.label"]   = "Get in touch",
        ["contact.title"]   = "Contact Me",
        ["contact.subtitle"]= "Have a project in mind? Let's talk!",
        ["contact.name"]    = "Full name",
        ["contact.email"]   = "Email",
        ["contact.message"] = "Message",
        ["contact.send"]    = "Send message",
        ["contact.success"] = "Message sent successfully!",
        ["contact.error"]   = "Something went wrong, please try again.",
        // Experience
        ["exp.label"]       = "Experience",
        ["exp.title"]       = "Work Experience",
        ["exp.present"]     = "Present",
        // Education
        ["edu.label"]       = "Education",
        ["edu.title"]       = "Education",
        // Expand / collapse
        ["common.showmore"] = "Show more",
        ["common.showless"] = "Show less",
        // Footer
        ["footer.rights"]   = "All rights reserved.",
        ["footer.built"]    = "Built with Claude",
        // Chat widget
        ["chat.title"]          = "Võ Đông Hà",
        ["chat.status"]         = "● Online",
        ["chat.close"]          = "Close",
        ["chat.form.title"]     = "Start a conversation",
        ["chat.form.subtitle"]  = "Fill in your details so I can follow up with you.",
        ["chat.form.name"]      = "Full name *",
        ["chat.form.phone"]     = "Phone number *",
        ["chat.form.email"]     = "Email *",
        ["chat.form.name.placeholder"]  = "John Doe",
        ["chat.form.phone.placeholder"] = "1 234 567 890",
        ["chat.form.name.required"]  = "Name is required",
        ["chat.form.phone.invalid"] = "Invalid phone number",
        ["chat.form.email.invalid"] = "Invalid email address",
        ["chat.form.start"]     = "Start chat",
        ["chat.empty"]          = "Send a message to start the conversation 👋",
        ["chat.placeholder"]    = "Type a message...",
        ["chat.new"]            = "New messages",
        ["chat.today"]          = "Today",
        ["chat.yesterday"]      = "Yesterday",
        ["chat.sent"]           = "Sent",
        ["chat.read"]           = "Read",
        ["chat.push.denied"]    = "Notifications blocked. Enable them to get replies.",
        ["chat.push.howto"]     = "How to enable",
        ["chat.fab.title"]      = "Chat with me",
    };
}
