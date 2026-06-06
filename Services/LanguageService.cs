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
        ["blog.read"]       = "Đọc tiếp",
        ["blog.empty"]      = "Chưa có bài viết nào.",
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
        ["blog.read"]       = "Read more",
        ["blog.empty"]      = "No posts yet.",
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
    };
}
