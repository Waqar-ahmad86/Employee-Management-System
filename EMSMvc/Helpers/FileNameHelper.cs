using System.Globalization;
using System.Text.RegularExpressions;

namespace EMSMvc.Helpers
{
    public static class FileNameHelper
    {
        public static string GenerateMonthlyAttendanceReportFileName(int year, int month, string? employeeName, string? roleName)
        {
            string reportMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            string baseFileName = $"MonthlyAttendance_{reportMonthName}_{year}";

            if (!string.IsNullOrEmpty(employeeName))
            {
                string safeNamePart = SanitizeFileNamePart(employeeName, 15);
                if (!string.IsNullOrEmpty(safeNamePart))
                {
                    baseFileName += $"_Name_{safeNamePart}";
                }
            }

            if (!string.IsNullOrEmpty(roleName))
            {
                string safeRolePart = SanitizeFileNamePart(roleName, 10);
                if (!string.IsNullOrEmpty(safeRolePart))
                {
                    baseFileName += $"_Role_{safeRolePart}";
                }
            }

            return baseFileName + ".pdf";
        }

        private static string SanitizeFileNamePart(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string regex = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            string sanitized = Regex.Replace(input, regex, "_");

            sanitized = Regex.Replace(sanitized, @"_+", "_");
            sanitized = sanitized.Trim('_');

            if (sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength);
                sanitized = sanitized.TrimEnd('_');
            }

            return sanitized;
        }
    }
}
