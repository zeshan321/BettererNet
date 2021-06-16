#nullable enable

namespace SampleProject
{
    public class BadStuff
    {
        public static string GetFullName(string? firstName, string? lastName)
        {
            if (firstName != null && lastName != null)
            {
                return $"{firstName}{lastName}";
            }

            return null;
        }
    }
}