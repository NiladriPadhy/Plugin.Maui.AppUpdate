namespace Plugin.Maui.AppUpdate;

static class VersionComparer
{
    public static bool IsAtLeast(string? actual, string? minimum) =>
        string.IsNullOrWhiteSpace(minimum) || Compare(actual, minimum) >= 0;

    public static bool IsNewerThan(string? candidate, string? current) =>
        !string.IsNullOrWhiteSpace(candidate) && Compare(candidate, current) > 0;

    public static int Compare(string? left, string? right)
    {
        var a = Parse(left);
        var b = Parse(right);
        var length = Math.Max(a.Length, b.Length);

        for (var i = 0; i < length; i++)
        {
            var leftPart = i < a.Length ? a[i] : 0;
            var rightPart = i < b.Length ? b[i] : 0;
            var cmp = leftPart.CompareTo(rightPart);
            if (cmp != 0)
            {
                return cmp;
            }
        }

        return 0;
    }

    static int[] Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var parts = new List<int>();
        foreach (var token in value.Split(['.', '-', '_', '+'], StringSplitOptions.RemoveEmptyEntries))
        {
            var end = 0;
            while (end < token.Length && char.IsDigit(token[end]))
            {
                end++;
            }

            if (end == 0)
            {
                break;
            }

            if (int.TryParse(token.AsSpan(0, end), NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            {
                parts.Add(number);
            }
            else
            {
                break;
            }
        }

        return [.. parts];
    }
}
