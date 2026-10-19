/**
 * Power Grid HaYTooL - Official Website Interactive Controller
 * Dual-Language (TR/EN), Dual-Theme (Dark/Light), Smart GPU Simulator & Gallery
 */

// --- 1. Localization Dictionary ---
const translations = {
  tr: {
    brand_sub: "Güç Yöneticisi",
    nav_features: "Özellikler",
    nav_simulator: "GPU Simülatörü",
    nav_gallery: "Ekran Görüntüleri",
    nav_faq: "SSS",
    nav_portal: "🏛️ HaYTooL Portal",
    btn_download_nav: "İndir",
    hero_pill: "🏛️ HaYTooL Portalının Resmi Güç Yöneticisi",
    hero_title: "Bilgisayarınızın Gücünü <span class='gradient-text'>Yapay Zeka Gibi Akıllı</span> Yönetin",
    hero_desc: "Ekran kartı yükünüze göre güç planlarını otomatik değiştiren, boşta watt tasarrufu yapıp işlemci termallerini düşüren, oyunda ise mikro takılmaları (micro-stuttering) ve gecikmeyi önleyerek kararlı maksimum FPS sunan modern Windows güç yöneticisi.",
    btn_download_portable: "⚡ Taşınabilir İndir (.exe - Kurulumsuz)",
    btn_download_setup: "📦 Otomatik Kurulum (.exe)",
    btn_github_repo: "⭐ GitHub'da İncele",
    trust_portable: "%100 Taşınabilir",
    trust_no_install: "Kurulumsuz",
    trust_latency: "Düşük Gecikme (Low Latency)",
    trust_watt: "Düşük Watt Tüketimi",
    trust_privacy: "Sıfır Telemetri & Açık Kaynak",
    badge_downloads: "Toplam İndirme",
    mascot_name: "⚡ Power Cat (Güç Kedisi)",
    mascot_quote: "\"Masaüstünde fısıltı kadar sessiz, oyunda şimşek kadar hızlıyım! 🐾⚡\"",
    
    // GPU Simulator
    sim_tag: "CANLI ETKİLEŞİMLİ DENEYİM",
    sim_title: "Akıllı GPU Otomatik Güç Anahtarını Test Edin",
    sim_desc: "Aşağıdaki kaydırıcıyı hareket ettirerek ekran kartı yükünü simüle edin ve Power Grid HaYTooL'un sistemi nasıl tam zamanında yönettiğini canlı görün.",
    sim_gpu_load: "GPU Kullanım Yükü:",
    sim_active_plan_label: "Aktif Güç Planı:",
    sim_active_plan_sub: "GPU Kullanım Eşiğine Göre Dinamik Olarak Belirlenir",
    stage_idle: "0% - 100% Simülasyon",
    metric_cpu_clock: "İşlemci Frekansı",
    metric_fan_noise: "Fan Hızı / Termaller",
    metric_power_draw: "Tahmini Güç Tüketimi",
    metric_status: "Sistem Durumu & Kararlılık",
    metric_sub_cpu: "Düşük voltaj ve soğuk çekirdekler",
    metric_sub_fan: "Tamamen sessiz masaüstü deneyimi",
    metric_sub_watt: "Boşta %40'a varan watt tasarrufu",
    metric_sub_status: "0ms gecikme ve sıfır mikro takılma",
    scen_idle_tag: "Masaüstü & Medya",
    scen_idle_text: "Gereksiz saat hızları engellenir, boşta <strong>%40'a varan watt tasarrufu</strong> sağlanır ve fanlar tamamen sessizleşir.",
    scen_work_tag: "Kodlama & Ofis",
    scen_work_text: "Sistem tepkiselliği anında dengelenir, ısı birikimi önlenir ve dengeli güç tüketimiyle akıcı çalışma ortamı sunulur.",
    scen_game_tag: "Ağır Oyun & Render",
    scen_game_text: "GPU %30'u aştığı milisaniyede yüksek performans devreye girer; çekirdek park etme çözülür, <strong>mikro takılmalar (micro-stuttering) ve gecikme sıfırlanır</strong>.",
    toast_switched_title: "🎮 Akıllı Güç Anahtarı",
    toast_switched_desc: "Planına başarıyla geçildi!",

    // Manifesto & Trust
    manifesto_tag: "GELİŞTİRİCİ MANİFESTOSU & GÜVEN",
    manifesto_title: "Neden Power Grid HaYTooL?",
    manifesto_subtitle: "Piyasadaki arka planda gizli servisler çalıştıran, telemetri toplayan ve sistemi yoran hantal araçlara tepki olarak doğdu.",
    man_item1_title: "%100 Temiz & Sıfır Telemetri",
    man_item1_desc: "Arka planda çalışan casus Windows servisleri, otomatik veri toplayıcılar veya reklam motorları kesinlikle barındırmaz. Kodlarımız GitHub üzerinde tamamen açıktır.",
    man_item2_title: "Yerel Windows Win32 / Kernel API",
    man_item2_desc: "Sisteminize yabancı veya tehlikeli üçüncü parti sürücüler yüklemez; doğrudan Windows'un yerel Powrprof.dll güç mimarisini yönetir.",
    man_item3_title: "Düşük Gecikme & Kararlılık Garantisi",
    man_item3_desc: "Oyun sırasında işlemci çekirdeklerini uyanık tutarak mikro takılmaları (micro-stuttering) ve FPS dalgalanmalarını sıfırlar; masaüstünde ise watt tüketimini minimize eder.",
    manifesto_signature: "👨‍💻 HaYTooL Portal Açık Kaynak Geliştirici Topluluğu",

    // Features Section
    features_tag: "GELİŞMİŞ DONANIM YÖNETİMİ",
    features_title: "Performans, Tasarruf ve Tam Kontrol",
    features_subtitle: "Karmaşık Windows Denetim Masası adımlarını unutun. İhtiyacınız olan her şey parmaklarınızın ucunda.",
    
    feat_gpu_title: "Akıllı GPU Yük İzleme & Oyun Optimizasyonu",
    feat_gpu_desc: "3 kademeli eşik sistemi (0-10% Boşta, 10-30% Günlük, 30-100% Oyun) ile güç planları arasında anında geçiş yaparak sıfır gecikme, kararlı saat hızları ve maksimum enerji tasarrufu sağlar.",
    
    feat_compare_title: "Yan Yana Plan Karşılaştırıcı",
    feat_compare_desc: "Sisteminizdeki herhangi iki güç planının işlemci sınırlarını, Turbo Boost durumlarını, soğutma ilkelerini ve uyku sürelerini satır satır görselleştirin.",
    
    feat_editor_title: "Derin Plan Düzenleyici",
    feat_editor_desc: "GUID kodları veya karmaşık komut istemleriyle uğraşmadan minimum/maksimum CPU durumlarını, fan politikasını ve zaman aşımlarını tek ekrandan ayarlayın.",
    
    feat_backup_title: "Otomatik ZIP Yedekleme",
    feat_backup_desc: "Herhangi bir değişiklik öncesinde tüm güç planlarınız tarih ve saat damgalı ZIP olarak yedeklenir. İstediğiniz an tek tıkla eski haline dönebilirsiniz.",
    
    feat_themes_title: "14 Modern Görsel Tema",
    feat_themes_desc: "Gün Batımı, Botanik Bahçe, İskandinav Laciverti, Obsidiyen Siyahı ve Kutup Beyazı dahil olmak üzere hem açık hem koyu modda kusursuz kontrast.",
    
    feat_toast_title: "Zarif Toast Bildirimleri",
    feat_toast_desc: "Plan ister elinizle ister otomatik değişsin; saatin hemen yanında beliren göz yormayan, estetik bildirim kartıyla sistem durumundan anında haberdar olun.",
    
    feat_context_title: "Masaüstü Sağ Tık Entegrasyonu",
    feat_context_desc: "Yönetici izni (UAC) pencereleriyle uğraşmadan, masaüstü sağ tık menüsüne tek tıkla güç planları menüsünü ekleyin veya kaldırın.",
    
    feat_startup_title: "Kullanıcıya Özel Açılış Planı",
    feat_startup_desc: "Windows oturumunuz her açıldığında doğrudan istediğiniz özel güç profilinin aktif olmasını sağlayın.",

    feat_unlock_title: "Gizli Güç Ayarlarını Açma",
    feat_unlock_desc: "Denetim Masasındaki kilitli çekirdek park etme (core parking), agresif Turbo Boost ve PCIe ASPM güç ayarlarının kilidini tek tıkla açın.",
    feat_cli_title: "Kapsamlı Komut Satırı (CLI)",
    feat_cli_desc: "Oyun başlatıcıları, Stream Deck veya betikleriniz için tam otomasyon: -smart on/off ile GPU anahtarını yönetin, planları listeleyin veya GUID ile anında geçiş yapın.",

    // Gallery
    gallery_tag: "KULLANICI ARAYÜZÜ",
    gallery_title: "Sade, Şık ve Modern Tasarım",
    gallery_subtitle: "Windows 11 tasarım diliyle kusursuz uyum sağlayan akıcı WPF arayüzü.",
    tab_main: "Ana Pencere",
    tab_gpu: "Akıllı GPU Anahtarı",
    tab_compare: "Plan Karşılaştırıcı",
    caption_main: "Tüm güç planlarının dinamik kartlar halinde listelendiği, tek tıkla geçiş sağlayan ana arayüz.",
    caption_gpu: "3 kademeli canlı GPU eşik ayarları, gecikme filtreleri ve bildirim kontrol paneli.",
    caption_compare: "İki güç planının işlemci ve süre parametrelerini yan yana karşılaştıran görsel araç.",

    // FAQ
    faq_tag: "MERAK EDİLENLER",
    faq_title: "Sıkça Sorulan Sorular",
    faq_q1: "Power Grid HaYTooL kurulum gerektirir mi?",
    faq_a1: "Hayır! Uygulama tamamen tek dosya taşınabilir (.exe) olarak çalışır. Kurulum yapmadan USB belleğinizden veya doğrudan istediğiniz klasörden çalıştırabilirsiniz.",
    faq_q2: "Akıllı GPU Anahtarı nasıl enerji tasarrufu sağlar?",
    faq_a2: "Oyun oynamadığınızda veya masaüstünde video izlerken bilgisayarınızı otomatik olarak Eco Stealth veya Dengeli plana geçirir. Bu sayede işlemci gereksiz yere yüksek frekansta çalışmaz, fanlar susar ve elektrik tasarrufu sağlanır.",
    faq_q5: "Oyunlarda mikro takılmaları (micro-stuttering) ve gecikmeyi önler mi?",
    faq_a5: "Evet! Standart Windows planları oyun ortasında işlemci çekirdeklerini gereksiz yere park edip frekans düşürebilir. Power Grid, GPU yükü algılandığında Apex moduna geçerek tüm çekirdekleri uyanık tutar ve ani FPS düşüşlerini önler.",
    faq_q6: "İşlemci sıcaklıklarını ve termal kısılmayı (thermal throttling) nasıl etkiler?",
    faq_a6: "Masaüstü ve hafif kullanımda voltajı ve gereksiz Turbo Boost sıçramalarını dizginleyerek donanımın serin kalmasını sağlar. Isı birikimi önlendiği için oyun açıldığında sistem termal sınıra çarpmadan maksimum saat hızında çalışır.",
    faq_q3: "Yönetici (Admin) izni gerektirir mi?",
    faq_a3: "Temel güç planı değişimleri ve masaüstü sağ tık entegrasyonu standart kullanıcı (HKCU) yetkisiyle yönetici izni gerekmeden çalışır. Sadece fabrika ayarlarına sıfırlama gibi derin sistem ayarlarında yetki talep edilir.",
    faq_q4: "Bozulursa eski planlarımı geri getirebilir miyim?",
    faq_a4: "Evet! Uygulama her değişiklikte otomatik ZIP yedeği alır. 'Otomatik ZIP Yedeğinden Geri Yükle' veya 'Varsayılanlara Sıfırla' menüleriyle anında eski orijinal planlarınıza dönebilirsiniz.",
    faq_q7: "Komut satırından (CLI / CMD) yönetilebilir mi?",
    faq_a7: "Evet! PowerGridHaYTooL.exe -smart on/off, -list, -apply {GUID} ve -exit gibi komutlarla başlatıcılarınıza, bat betiklerinize veya Stream Deck tuşlarınıza bağlayabilirsiniz.",

    // Comparison Table
    compare_tag: "NEDEN POWER GRID?",
    compare_title: "Windows Denetim Masası vs. Power Grid HaYTooL",
    th_feature: "Özellik",
    th_standard: "Klasik Windows Güç Seçenekleri",
    th_haytool: "⚡ Power Grid HaYTooL",
    row1_title: "GPU Yüküne Göre Otomatik Geçiş",
    row1_std: "❌ Yok (Plan sabit kalır)",
    row1_hay: "✅ Var (3 Kademeli Akıllı Otomasyon)",
    row2_title: "Planları Yan Yana Karşılaştırma",
    row2_std: "❌ Yok",
    row2_hay: "✅ Görsel Yan Yana Karşılaştırıcı",
    row3_title: "Masaüstü Sağ Tık Menüsü",
    row3_std: "❌ Yok",
    row3_hay: "✅ Tek Tıkla Entegrasyon (HKCU)",
    row4_title: "Otomatik ZIP Yedekleme",
    row4_std: "❌ Yok",
    row4_hay: "✅ Her Değişiklikte Tarihli ZIP",
    row5_title: "Gizli Güç Ayarları Kilit Çözücü",
    row5_std: "❌ Komut istemi / Kayıt Defteri bilgisi gerekir",
    row5_hay: "✅ Tek Tıkla Tüm Kilitleri Açma",
    row6_title: "Görsel Tema Seçenekleri",
    row6_std: "❌ Yok",
    row6_hay: "✅ 14 Özel Açık & Koyu Palet",
    row7_title: "Kurulum İhtiyacı",
    row7_std: "İşletim sistemi bileşeni",
    row7_hay: "✅ %100 Taşınabilir Tek .EXE",
    row8_title: "Komut Satırı (CLI) & Otomasyon",
    row8_std: "⚠️ Yalnızca karmaşık powercfg parametreleri",
    row8_hay: "✅ Tam Otomasyon (Akıllı anahtar, tepsi ve plan geçişi)",
    download_reqs: "Gereksinimler: Windows 10 / 11 (64-bit) • .NET 10.0 WPF • Tek dosya portable",

    // Download CTA
    download_tag: "HEMEN DENEYİN",
    download_title: "Bilgisayarınızın Kontrolünü Eline Alın",
    download_desc: "Power Grid HaYTooL tamamen ücretsiz ve açık kaynaklıdır. Hemen indirin, sessizliğin ve performansın tadını çıkarın.",
    footer_rights: "Power Grid HaYTooL, HaYTooL Portalının resmi açık kaynak projesidir.",
    footer_contact: "İletişim"
  },
  en: {
    brand_sub: "Power Manager",
    nav_features: "Features",
    nav_simulator: "GPU Simulator",
    nav_gallery: "Screenshots",
    nav_faq: "FAQ",
    nav_portal: "🏛️ HaYTooL Portal",
    btn_download_nav: "Download",
    hero_pill: "🏛️ Official Power Utility of HaYTooL Portal",
    hero_title: "Control PC Power <span class='gradient-text'>With Smart GPU Awareness</span>",
    hero_desc: "An intelligent Windows power manager that dynamically switches power schemes based on 3D GPU load — slashing idle wattage and processor thermals on desktop, while eliminating micro-stuttering and latency in games for rock-solid peak FPS.",
    btn_download_portable: "⚡ Download Portable (.exe - Zero Install)",
    btn_download_setup: "📦 Automated Installer (.exe)",
    btn_github_repo: "⭐ Star on GitHub",
    trust_portable: "100% Portable",
    trust_no_install: "No Installation",
    trust_latency: "Ultra-Low Latency",
    trust_watt: "Low Wattage Consumption",
    trust_privacy: "Zero Telemetry & Open Source",
    badge_downloads: "Total Downloads",
    mascot_name: "⚡ Power Cat",
    mascot_quote: "\"Whisper-silent on desktop, lightning-fast in games! 🐾⚡\"",

    // GPU Simulator
    sim_tag: "INTERACTIVE LIVE DEMO",
    sim_title: "Test Smart GPU-Aware Switching",
    sim_desc: "Drag the slider below to simulate real-time GPU load and witness how Power Grid HaYTooL transitions power states instantaneously.",
    sim_gpu_load: "GPU Usage Load:",
    sim_active_plan_label: "Active Power Plan:",
    sim_active_plan_sub: "Dynamically Calculated Based on Real-Time GPU Thresholds",
    stage_idle: "0% - 100% Simulation",
    metric_cpu_clock: "CPU Frequency",
    metric_fan_noise: "Fan Speed / Thermals",
    metric_power_draw: "Estimated Wattage",
    metric_status: "System Stability & Latency",
    metric_sub_cpu: "Low voltage and cool CPU cores",
    metric_sub_fan: "Pure silent desktop operation",
    metric_sub_watt: "Up to 40% idle wattage savings",
    metric_sub_status: "0ms latency and zero micro-stuttering",
    scen_idle_tag: "Desktop & Media",
    scen_idle_text: "Restricts unnecessary high clocks, cuts idle draw by <strong>up to 40% watts</strong>, and quiets fan noise completely.",
    scen_work_tag: "Coding & Office",
    scen_work_text: "Dynamically stabilizes responsiveness, stops heat build-up, and maintains fluid workflow with balanced power draw.",
    scen_game_tag: "Heavy Gaming & Compute",
    scen_game_text: "Instantly engages the microsecond GPU load crosses 30%; unparks all cores and <strong>eliminates micro-stuttering and frame drops</strong>.",
    toast_switched_title: "🎮 Smart Power Switch",
    toast_switched_desc: "Plan activated successfully!",

    // Manifesto & Trust
    manifesto_tag: "DEVELOPER MANIFESTO & TRUST",
    manifesto_title: "Why Power Grid HaYTooL?",
    manifesto_subtitle: "Engineered as a direct response to bloated utility apps that run stealth background services, eat RAM, and harvest user data.",
    man_item1_title: "100% Clean & Zero Telemetry",
    man_item1_desc: "No hidden background tracking services, no telemetry, and no ad engines. All source code is completely open and inspectable on GitHub.",
    man_item2_title: "Native Windows Win32 / Kernel APIs",
    man_item2_desc: "Zero dangerous third-party kernel drivers. Directly interacts with Windows native Powrprof.dll power architecture for flawless stability.",
    man_item3_title: "Low Latency & Micro-Stutter Prevention",
    man_item3_desc: "Prevents random core parking during gaming sessions to eliminate FPS dips and frametime latency, while drastically cutting idle power.",
    manifesto_signature: "👨‍💻 HaYTooL Portal Open Source Developer Community",

    // Features Section
    features_tag: "ADVANCED HARDWARE CONTROL",
    features_title: "Maximum FPS, Whisper Silence & Zero Waste",
    features_subtitle: "Ditch the convoluted Windows Control Panel. Everything you need is visual, instantaneous, and automated.",

    feat_gpu_title: "Smart GPU Load Tracking & Game Optimization",
    feat_gpu_desc: "Autonomous 3-tier threshold engine (0-10% Idle, 10-30% Daily, 30-100% Gaming) transitions power schemes on-the-fly, ensuring zero latency and max energy efficiency.",

    feat_compare_title: "Side-by-Side Plan Comparator",
    feat_compare_desc: "Compare any two power plans side-by-side to visually inspect differences in processor states, Turbo Boost profiles, cooling policies, and timeouts.",

    feat_editor_title: "Deep Plan Editor",
    feat_editor_desc: "Fine-tune CPU minimum/maximum throttle states, active fan cooling, and sleep timers directly without cryptic GUIDs or command line headaches.",

    feat_backup_title: "Automated ZIP Backups",
    feat_backup_desc: "All power schemes are automatically snapshotted into timestamped ZIP archives before modifications, with a built-in one-click restore dialog.",

    feat_themes_title: "14 Modern Visual Palettes",
    feat_themes_desc: "Sunset Boulevard, Botanical Garden, Nordic Slate, Obsidian Black, Arctic Frost, and more. Flawless contrast in both Dark and Light modes.",

    feat_toast_title: "Sleek Slide-in Toast Alerts",
    feat_toast_desc: "Non-intrusive notifications slide in next to the clock whenever a scheme is triggered, keeping you informed without interrupting fullscreen apps.",

    feat_context_title: "Desktop Right-Click Integration",
    feat_context_desc: "Add or remove the cascade 'Power Grid (HaYTooL)' desktop context menu in one click with zero administrator prompts (HKCU).",

    feat_startup_title: "User-Specific Startup Plan",
    feat_startup_desc: "Designate a dedicated power scheme that automatically activates whenever your Windows user logs into the system.",

    feat_unlock_title: "Unlock Hidden Power Settings",
    feat_unlock_desc: "Reveal buried processor attributes like aggressive Turbo Boost, core parking thresholds, and PCIe ASPM in standard Windows Control Panel with 1-click.",
    feat_cli_title: "Full Command-Line (CLI) Support",
    feat_cli_desc: "Total headless automation for launchers, Stream Deck or scripts: manage Smart GPU Switch (-smart on/off), list schemes, or activate plans by GUID instantly.",

    // Gallery
    gallery_tag: "USER INTERFACE",
    gallery_title: "Clean, Modern & Intuitive Layout",
    gallery_subtitle: "Engineered with native WPF to match the modern aesthetics of Windows 11.",
    tab_main: "Main Window",
    tab_gpu: "Smart GPU Switcher",
    tab_compare: "Plan Comparator",
    caption_main: "Dynamic card overview of all installed power plans with single-click activation.",
    caption_gpu: "Configurable 3-tier GPU load thresholds, delay filters, and notification settings.",
    caption_compare: "Side-by-side visual comparison tool showing detailed processor and timeout discrepancies.",

    // FAQ
    faq_tag: "FREQUENTLY ASKED QUESTIONS",
    faq_title: "Everything You Need to Know",
    faq_q1: "Does Power Grid HaYTooL require installation?",
    faq_a1: "No! It is fully portable. You can run the standalone .exe directly from a USB drive or any local folder with zero external dependencies.",
    faq_q2: "How does the Smart GPU Switcher save electricity?",
    faq_a2: "When not running 3D games, it down-clocks your CPU into Eco Stealth or Balanced mode, shutting down excessive core voltages and keeping fans quiet.",
    faq_q5: "Does it prevent in-game micro-stuttering and latency?",
    faq_a5: "Yes! Default Windows power plans frequently sleep CPU cores and down-clock mid-game. Power Grid senses 3D GPU load and triggers Apex mode immediately, keeping all cores unparked to prevent frame drops.",
    faq_q6: "How does it improve CPU temperatures and thermal throttling?",
    faq_a6: "By curbing unwanted Turbo Boost spikes and excessive core voltage during desktop tasks, your cooler stays cold. When a heavy game starts, the system has full thermal headroom to sustain maximum boost clocks without throttling.",
    faq_q3: "Does it need Administrator privileges?",
    faq_a3: "Core switching and desktop context menus operate completely under regular user privileges (HKCU). Admin is only requested for deep registry factory restores.",
    faq_q4: "Can I revert back to Windows default plans?",
    faq_a4: "Yes! The app takes automatic ZIP backups before any change. You can restore prior backups or reset Windows schemes to factory defaults in one click.",
    faq_q7: "Can it be controlled via Command Line (CLI / CMD)?",
    faq_a7: "Yes! Use PowerGridHaYTooL.exe -smart on/off, -list, -apply {GUID}, and -exit to seamlessly integrate with custom batch scripts, game launchers, or Stream Deck profiles.",

    // Comparison Table
    compare_tag: "WHY POWER GRID?",
    compare_title: "Windows Control Panel vs. Power Grid HaYTooL",
    th_feature: "Feature",
    th_standard: "Standard Windows Power Options",
    th_haytool: "⚡ Power Grid HaYTooL",
    row1_title: "Smart GPU-Load Auto Switching",
    row1_std: "❌ None (Static plan)",
    row1_hay: "✅ Yes (3-Tier Autonomous Switching)",
    row2_title: "Side-by-Side Plan Comparison",
    row2_std: "❌ None",
    row2_hay: "✅ Visual Side-by-Side Comparator",
    row3_title: "Desktop Context Menu Integration",
    row3_std: "❌ None",
    row3_hay: "✅ One-Click Integration (HKCU)",
    row4_title: "Automated ZIP Backups",
    row4_std: "❌ None",
    row4_hay: "✅ Timestamped ZIP with 1-Click Restore",
    row5_title: "Unlock Hidden Power Settings",
    row5_std: "❌ Requires Command Prompt / Registry Tweaks",
    row5_hay: "✅ 1-Click Unlock All Advanced Attributes",
    row6_title: "Visual Custom Themes",
    row6_std: "❌ None",
    row6_hay: "✅ 14 Curated Dark & Light Palettes",
    row7_title: "Installation Requirement",
    row7_std: "OS built-in component",
    row7_hay: "✅ 100% Portable Standalone .EXE",
    row8_title: "Command-Line (CLI) & Automation",
    row8_std: "⚠️ Cryptic and limited powercfg commands only",
    row8_hay: "✅ Full Native CLI (Smart switch, tray & scheme activation)",
    download_reqs: "Requirements: Windows 10 / 11 (64-bit) • .NET 10.0 WPF • Single portable executable",

    // Download CTA
    download_tag: "TRY IT TODAY",
    download_title: "Take Full Command of Your PC Power",
    download_desc: "Power Grid HaYTooL is 100% free and open-source. Download now and experience cooler temps, silent fans, and maximum FPS.",
    footer_rights: "Power Grid HaYTooL is an official open-source project of the HaYTooL Portal.",
    footer_contact: "Contact"
  }
};

// --- State Variables ---
let currentLang = localStorage.getItem('powergrid_lang') || 'en';
let currentTheme = localStorage.getItem('powergrid_theme') || 'dark';

// --- 2. Theme Management ---
function applyTheme(theme) {
  currentTheme = theme;
  document.documentElement.setAttribute('data-theme', theme);
  localStorage.setItem('powergrid_theme', theme);
  
  const themeBtn = document.getElementById('theme-toggle-btn');
  if (themeBtn) {
    themeBtn.innerHTML = theme === 'dark' ? '☀️ <span class="hide-mobile">Aydınlık</span>' : '🌙 <span class="hide-mobile">Karanlık</span>';
  }
}

function toggleTheme() {
  applyTheme(currentTheme === 'dark' ? 'light' : 'dark');
}

// --- 3. Language Management ---
function applyLanguage(lang) {
  currentLang = lang;
  localStorage.setItem('powergrid_lang', lang);

  // Update HTML lang attribute and Document Title
  document.documentElement.lang = lang;
  if (lang === 'tr') {
    document.title = "Power Grid HaYTooL - Akıllı Windows Güç ve GPU Yöneticisi";
  } else {
    document.title = "Power Grid HaYTooL - Intelligent Windows Power & GPU Manager";
  }

  const dict = translations[lang] || translations.en;
  
  // Use requestAnimationFrame to batch DOM reads and writes cleanly
  requestAnimationFrame(() => {
    // Replace all data-i18n elements efficiently
    document.querySelectorAll('[data-i18n]').forEach(el => {
      const key = el.getAttribute('data-i18n');
      const val = dict[key];
      if (val) {
        if (val.includes('<')) {
          el.innerHTML = val;
        } else {
          el.textContent = val;
        }
      }
    });

    // Vector SVG Flag Icons (Windows does not render country flag emojis natively)
    const FLAG_TR = `<svg class="flag-icon" viewBox="0 0 1200 800" width="18" height="12" style="border-radius:2px; vertical-align:middle; display:inline-block; margin-right:6px;"><rect width="1200" height="800" fill="#E30A17"/><circle cx="425" cy="400" r="200" fill="#ffffff"/><circle cx="475" cy="400" r="160" fill="#E30A17"/><polygon points="583.33,400 706.75,440.11 659.71,319.89 659.71,480.11 706.75,359.89" fill="#ffffff" transform="rotate(-18 583.33 400)"/></svg>`;
    const FLAG_EN = `<svg class="flag-icon" viewBox="0 0 60 30" width="18" height="12" style="border-radius:2px; vertical-align:middle; display:inline-block; margin-right:6px;"><clipPath id="s"><path d="M0,0 v30 h60 v-30 z"/></clipPath><clipPath id="t"><path d="M30,15 h30 v15 z v15 h-30 z h-30 v-15 z v-15 h30 z"/></clipPath><g clip-path="url(#s)"><path d="M0,0 v30 h60 v-30 z" fill="#012169"/><path d="M0,0 L60,30 M60,0 L0,30" stroke="#fff" stroke-width="6"/><path d="M0,0 L60,30 M60,0 L0,30" clip-path="url(#t)" stroke="#C8102E" stroke-width="4"/><path d="M30,0 v30 M0,15 h60" stroke="#fff" stroke-width="10"/><path d="M30,0 v30 M0,15 h60" stroke="#C8102E" stroke-width="6"/></g></svg>`;

    // Update Language toggle button label
    const langBtn = document.getElementById('lang-toggle-btn');
    if (langBtn) {
      langBtn.innerHTML = lang === 'tr' ? `${FLAG_EN} English` : `${FLAG_TR} Türkçe`;
    }

    const slider = document.getElementById('gpu-slider');
    if (slider) {
      updateSimulator(slider.value);
    }
  });
}

function toggleLanguage() {
  applyLanguage(currentLang === 'tr' ? 'en' : 'tr');
}

// --- 4. Interactive Smart GPU Simulator ---
let lastTier = -1;
let simRafId = null;

function updateSimulator(val) {
  if (simRafId) {
    cancelAnimationFrame(simRafId);
  }
  simRafId = requestAnimationFrame(() => {
    renderSimulator(val);
  });
}

function renderSimulator(val) {
  const load = parseInt(val, 10);
  const loadValueDisplay = document.getElementById('sim-load-val');
  const planBadge = document.getElementById('sim-active-plan');
  const cpuVal = document.getElementById('sim-cpu-val');
  const fanVal = document.getElementById('sim-fan-val');
  const wattVal = document.getElementById('sim-watt-val');
  const statusVal = document.getElementById('sim-status-val');
  const toast = document.getElementById('sim-toast');
  const toastDesc = document.getElementById('sim-toast-desc');
  const toastIcon = document.getElementById('sim-toast-icon');

  if (!loadValueDisplay || !planBadge) return;

  loadValueDisplay.textContent = `${load}%`;

  let tier = 1;
  let planName = "";
  let badgeClass = "";
  let cpu = "";
  let fan = "";
  let watt = "";
  let status = "";
  let icon = "🌱";

  if (load <= 10) {
    tier = 1;
    planName = currentLang === 'tr' ? "🌱 Power Grid Eco Stealth (%0 - %10)" : "🌱 Power Grid Eco Stealth (0 - 10%)";
    badgeClass = "badge-eco";
    cpu = "1.2 - 2.4 GHz (Low Voltage)";
    fan = currentLang === 'tr' ? "0 RPM (Sessiz Whisper)" : "0 RPM (Whisper Silent)";
    watt = "~18 - 25 Watt";
    status = currentLang === 'tr' ? "Masaüstü Boşta / Enerji Tasarrufu" : "Desktop Idle / Max Energy Saving";
    icon = "🌱";
  } else if (load <= 30) {
    tier = 2;
    planName = currentLang === 'tr' ? "⚖️ Power Grid Smart Balanced (%10 - %30)" : "⚖️ Power Grid Smart Balanced (10 - 30%)";
    badgeClass = "badge-balanced";
    cpu = "2.8 - 3.8 GHz (Adaptive)";
    fan = currentLang === 'tr' ? "850 RPM (Hafif Hava Akışı)" : "850 RPM (Light Airflow)";
    watt = "~45 - 65 Watt";
    status = currentLang === 'tr' ? "Video / Tarayıcı / Günlük Çalışma" : "Video Playback / Daily Browsing";
    icon = "⚖️";
  } else {
    tier = 3;
    planName = currentLang === 'tr' ? "🚀 Power Grid Apex Performance (%30 - %100)" : "🚀 Power Grid Apex Performance (30 - 100%)";
    badgeClass = "badge-apex";
    cpu = "4.8 - 5.4 GHz (All Cores Unparked)";
    fan = currentLang === 'tr' ? "1850 RPM (Aktif Soğutma)" : "1850 RPM (Active High Cooling)";
    watt = "~120 - 195 Watt";
    status = currentLang === 'tr' ? "3D Oyun / Render / Tam Güç" : "3D Gaming / Full Compute";
    icon = "🚀";
  }

  planBadge.className = `active-plan-badge ${badgeClass}`;
  planBadge.textContent = planName;
  cpuVal.textContent = cpu;
  fanVal.textContent = fan;
  wattVal.textContent = watt;
  statusVal.textContent = status;

  // Trigger Toast Notification on tier change
  if (lastTier !== -1 && lastTier !== tier && toast && toastIcon && toastDesc) {
    toastIcon.textContent = icon;
    toastDesc.textContent = `${planName.split('(')[0].trim()} ${translations[currentLang].toast_switched_desc}`;
    toast.classList.add('show');
    
    setTimeout(() => {
      toast.classList.remove('show');
    }, 3200);
  }

  lastTier = tier;
}

// --- 5. Gallery Tab Switching ---
const galleryData = {
  main: {
    src: "screenshots/main_window.png",
    srcWebp: "screenshots/main_window.webp",
    width: 416,
    height: 650,
    caption_tr: "Tüm güç planlarının dinamik kartlar halinde listelendiği, tek tıkla geçiş sağlayan ana arayüz.",
    caption_en: "Dynamic card overview of all installed power plans with single-click activation."
  },
  gpu: {
    src: "screenshots/smart_gpu_switch.png",
    srcWebp: "screenshots/smart_gpu_switch.webp",
    width: 626,
    height: 723,
    caption_tr: "3 kademeli canlı GPU eşik ayarları, gecikme filtreleri ve bildirim kontrol paneli.",
    caption_en: "Configurable 3-tier GPU load thresholds, delay filters, and notification settings."
  },
  compare: {
    src: "screenshots/plan_compare.png",
    srcWebp: "screenshots/plan_compare.webp",
    width: 1066,
    height: 723,
    caption_tr: "İki güç planının işlemci ve süre parametrelerini yan yana karşılaştıran görsel araç.",
    caption_en: "Side-by-side visual comparison tool showing detailed processor and timeout discrepancies."
  }
};

function selectGalleryTab(tabKey) {
  document.querySelectorAll('.gallery-tab').forEach(btn => {
    btn.classList.toggle('active', btn.getAttribute('data-tab') === tabKey);
  });

  const img = document.getElementById('gallery-display-img');
  const sourceWebp = document.getElementById('gallery-source-webp');
  const caption = document.getElementById('gallery-caption-text');
  const data = galleryData[tabKey];

  if (img && data) {
    img.style.opacity = '0';
    setTimeout(() => {
      if (sourceWebp && data.srcWebp) {
        sourceWebp.srcset = data.srcWebp;
      }
      img.src = data.src;
      if (data.width) img.width = data.width;
      if (data.height) img.height = data.height;
      caption.textContent = currentLang === 'tr' ? data.caption_tr : data.caption_en;
      img.style.opacity = '1';
    }, 150);
  }
}

// --- 6. Lightbox Zoom ---
function openLightbox(src) {
  const modal = document.getElementById('lightbox-modal');
  const img = document.getElementById('lightbox-img');
  img.src = src;
  modal.classList.add('active');
}

function closeLightbox() {
  document.getElementById('lightbox-modal').classList.remove('active');
}

// --- Initialization ---
document.addEventListener('DOMContentLoaded', () => {
  // Apply saved theme & language
  applyTheme(currentTheme);
  applyLanguage(currentLang);

  // Setup Event Listeners
  document.getElementById('theme-toggle-btn').addEventListener('click', toggleTheme);
  document.getElementById('lang-toggle-btn').addEventListener('click', toggleLanguage);

  // Setup Simulator Slider
  const slider = document.getElementById('gpu-slider');
  if (slider) {
    slider.addEventListener('input', (e) => {
      updateSimulator(e.target.value);
    }, { passive: true });
  }

  // Setup Gallery Tabs
  document.querySelectorAll('.gallery-tab').forEach(btn => {
    btn.addEventListener('click', () => {
      selectGalleryTab(btn.getAttribute('data-tab'));
    });
  });

  // Setup Lightbox trigger on gallery image
  const galleryImg = document.getElementById('gallery-display-img');
  if (galleryImg) {
    galleryImg.addEventListener('click', () => {
      openLightbox(galleryImg.src);
    });
  }

  const modalClose = document.getElementById('lightbox-close-btn');
  if (modalClose) {
    modalClose.addEventListener('click', closeLightbox);
  }

  const lightboxModal = document.getElementById('lightbox-modal');
  if (lightboxModal) {
    lightboxModal.addEventListener('click', (e) => {
      if (e.target === lightboxModal) closeLightbox();
    });
  }

  // Register Service Worker for aggressive persistent caching & offline support
  if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
      navigator.serviceWorker.register('./sw.js').catch(() => {});
    });
  }
});
