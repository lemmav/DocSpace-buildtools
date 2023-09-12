const globalTypes = {
  direction: {
    description: "Interface direction",
    defaultValue: "ltr",
    toolbar: {
      title: "Dir",
      icon: "paragraph",
      items: ["ltr", "rtl"],
      dynamicTitle: true,
    },
  },

  locale: {
    name: "Locale",
    description: "Internationalization locale",
    toolbar: {
      icon: "globe",
      items: [
        { value: "ar-SA", title: "عربي (المملكة العربية السعودية)" },
        { value: "az", title: "Azərbaycan (Latın, Azərbaycan)" },
        { value: "bg", title: "Български (България)" },
        { value: "cs", title: "Český (Česká republika)" },
        { value: "de", title: "Deutsch (Deutschland)" },
        { value: "el-GR", title: "Ελληνικά (Ελλάδα)" },
        { value: "en", title: "English" },
        { value: "es", title: "Español (España)" },
        { value: "fi", title: "Suomi (Suomi)" },
        { value: "fr", title: "Français (France)" },
        { value: "hy-AM", title: "Հայերեն (Հայաստան)" },
        { value: "it", title: "Italiano (Italia)" },
        { value: "ja-JP", title: "日本語（日本）" },
        { value: "ko-KR", title: "한국어(대한민국)" },
        { value: "lo-LA", title: "ພາສາລາວ" },
        { value: "lv", title: "Latviešu (Latvija)" },
        { value: "nl", title: "Nederlands (Nederland)" },
        { value: "pl", title: "Polski (Polska)" },
        { value: "pt", title: "Português (Portugal)" },
        { value: "pt-BR", title: "Português (Brasil)" },
        { value: "ro", title: "Română (România)" },
        { value: "ru", title: "Русский" },
        { value: "sk", title: "Slovenčina (Slovensko)" },
        { value: "sl", title: "Slovensko (Slovenija)" },
        { value: "tr", title: "Türkçe (Türkiye)" },
        { value: "uk-UA", title: "Українська (Україна)" },
        { value: "vi", title: "Tiếng Việt (Việt Nam)" },
        { value: "zh-CN", title: "中文（简体，中国）" },
      ],
      showName: true,
    },
  },
};

export default globalTypes;
