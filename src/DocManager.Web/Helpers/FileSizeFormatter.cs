namespace DocManager.Web.Helpers;

public static class FileSizeFormatter
{
    private const double Kilobyte = 1024;
    private static readonly string[] Units = ["bytes", "KB", "MB", "GB"];

    public static string Format(long bytes)
    {
        var size = (double)bytes;
        var unit = 0;
        while (size >= Kilobyte && unit < Units.Length - 1)
        {
            size /= Kilobyte;
            unit++;
        }

        return unit == 0 ? $"{bytes} {Units[0]}" : $"{size:0.#} {Units[unit]}";
    }
}
