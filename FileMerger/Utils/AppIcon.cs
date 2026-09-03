using System.Drawing;
using System.Reflection;

namespace FileMerger.Utils;

/// <summary>
/// 埋め込みリソースのアイコンを読み出す。
/// 実行ファイルのアイコンとは別に、ウィンドウ側にも同じものを設定するために使う。
/// </summary>
public static class AppIcon
{
    private const string ResourceName = "FileMerger.app.ico";

    private static Icon? _cached;
    private static bool _tried;

    public static Icon? Load()
    {
        if (_tried) return _cached;
        _tried = true;

        try
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
            if (stream is not null) _cached = new Icon(stream);
        }
        catch
        {
            // アイコンが読めなくても動作には支障がないので既定のまま進める
            _cached = null;
        }

        return _cached;
    }

    /// <summary>フォームにアプリのアイコンを設定する。</summary>
    public static void Apply(Form form)
    {
        var icon = Load();
        if (icon is not null) form.Icon = icon;
    }
}
