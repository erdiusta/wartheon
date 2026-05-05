using System.Text.RegularExpressions;

public static class EnumExtensions
{
    public static string ToPrettyString(this System.Enum value)
    {
        return Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");
    }
}
