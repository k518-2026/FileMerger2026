using System.Text;

namespace FileMerger.Utils;

public static class EncodingUtil
{
    public static Encoding ShiftJis => Encoding.GetEncoding(932);

    private static readonly Encoding StrictUtf8 =
        Encoding.GetEncoding("utf-8", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

    /// <summary>
    /// BOM を見て文字コードを判定し、無ければ UTF-8 として復号できるか試す。
    /// どちらでもなければ fallback（既定は Shift_JIS）を使う。
    /// </summary>
    public static string ReadAllText(string path, Encoding? forced)
    {
        var bytes = File.ReadAllBytes(path);
        return Decode(bytes, forced);
    }

    public static string Decode(byte[] bytes, Encoding? forced)
    {
        // BOM は指定の有無にかかわらず尊重する
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);

        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            return new UTF32Encoding(false, true).GetString(bytes, 4, bytes.Length - 4);

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);

        if (forced is not null)
            return forced.GetString(bytes);

        try
        {
            return StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return ShiftJis.GetString(bytes);
        }
    }

    /// <summary>UI に並べる出力文字コードの選択肢。</summary>
    public static IReadOnlyList<(string Label, Encoding Encoding)> OutputChoices { get; } = new (string, Encoding)[]
    {
        ("UTF-8 (BOM 付き)", new UTF8Encoding(true)),
        ("UTF-8 (BOM なし)", new UTF8Encoding(false)),
        ("Shift_JIS", Encoding.GetEncoding(932)),
        ("UTF-16 LE (BOM 付き)", new UnicodeEncoding(false, true))
    };
}
