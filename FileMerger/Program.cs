using System.Text;

namespace FileMerger;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Shift_JIS (CP932) などを .NET 8 で扱えるようにする
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
