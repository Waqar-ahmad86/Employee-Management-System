namespace EMSWebApi.Application.Helpers
{
    public static class AttendanceHelper
    {
        public static int GetWorkingDaysInMonth(int year, int month)
        {
            int count = 0;
            int totalDays = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= totalDays; day++)
            {
                var date = new DateTime(year, month, day);
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    count++;
            }
            return count;
        }
    }
}
