using System.Globalization;

namespace FinanceManager.Statistics.Extensions;

public static class DatetimeExtensions
{
    public static (int weekNumberOfYear, int month, int year) SplitDate(this DateTime date)
    {
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        var tempDate = date;
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            tempDate = date.AddDays(3);
        }

        // Return the week of our adjusted day
        var weekNumberOfYear= CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            tempDate,
            CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);

        return (weekNumberOfYear, date.Month, date.Year);
    }
}