using EMSMvc.Core.Application.Services;
using EMSMvc.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace EMSMvc.Helpers
{
    public static class DropdownHelper
    {
        public static void PopulateYearMonthDropdowns(MonthlyReportRequestVM viewModel)
        {
            int currentYear = DateTime.Today.Year;
            for (int i = 0; i < 5; i++)
            {
                viewModel.Years.Add(new SelectListItem 
                { 
                    Value = (currentYear - i).ToString(), 
                    Text = (currentYear - i).ToString() 
                });
            }

            viewModel.Years = viewModel.Years.OrderBy(y => y.Text).ToList();

            for (int i = 1; i <= 12; i++)
            {
                viewModel.Months.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i)
                });
            }
        }

        public static async Task<List<SelectListItem>> GetRoleDropdownItemsAsync(
            UserManagementService userManagementService, 
            string? selectedRoleName = null)
        {
            var allRoles = await userManagementService.GetAllRolesAsync() ?? new List<string>();

            return allRoles.Select(r => new SelectListItem
            {
                Value = r,
                Text = r,
                Selected = r == selectedRoleName
            }).ToList();
        }
    }
}
