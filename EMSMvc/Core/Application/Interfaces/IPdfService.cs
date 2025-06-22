using EMSMvc.ViewModels;

namespace EMSMvc.Core.Application.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateMonthlyAttendanceReportPdf(MonthlyReportRequestVM viewModel);
    }
}
