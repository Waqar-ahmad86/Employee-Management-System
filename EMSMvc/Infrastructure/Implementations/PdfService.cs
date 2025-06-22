using EMSMvc.Core.Application.Interfaces;
using EMSMvc.ViewModels;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Globalization;

namespace EMSMvc.Infrastructure.Implementations
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateMonthlyAttendanceReportPdf(MonthlyReportRequestVM viewModel)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new PdfWriter(memoryStream))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        Document document = new Document(pdf);

                        PdfFont helveticaBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        PdfFont helvetica = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                        PdfFont helveticaOblique = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

                        Paragraph title = new Paragraph($"Monthly Attendance Report - {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(viewModel.Month)}, {viewModel.Year}")
                            .SetFont(helveticaBold).SetFontSize(18).SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(20);
                        document.Add(title);

                        if (!string.IsNullOrEmpty(viewModel.Name))
                        {
                            Paragraph empNameFilterInfo = new Paragraph($"Employee Name: \"{viewModel.Name}\"")
                                .SetFont(helvetica).SetFontSize(9).SetTextAlignment(TextAlignment.LEFT)
                                .SetMarginBottom(5);
                            document.Add(empNameFilterInfo);
                        }
                        if (!string.IsNullOrEmpty(viewModel.RoleName))
                        {
                            Paragraph roleFilterInfo = new Paragraph($"Role: \"{viewModel.RoleName}\"")
                                .SetFont(helvetica).SetFontSize(9).SetTextAlignment(TextAlignment.LEFT)
                                .SetMarginBottom(10);
                            document.Add(roleFilterInfo);
                        }

                        if (viewModel.ReportData != null && viewModel.ReportData.Any())
                        {
                            Table table = new Table(UnitValue.CreatePercentArray(new float[]
                            {
                                0.5f,    // Sr. No.
                                1.4f,    // Employee Name
                                0.8f,    // Total Days
                                1.1f,    // Working Days
                                0.6f,    // Present
                                0.6f,    // Absent
                                0.8f,    // On Leave
                                1.2f,    // Total Work Hrs
                                1.2f     // Avg. Work Hrs
                            }))
                                .UseAllAvailableWidth().SetMarginBottom(15);

                            table.AddHeaderCell(new Cell().Add(new Paragraph("Sr.No.").SetFont(helveticaBold).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Employee Name").SetFont(helveticaBold).SetFontSize(9).SetTextAlignment(TextAlignment.LEFT)));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Total Days").SetFont(helveticaBold).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Working Days").SetFont(helveticaBold).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Present").SetFont(helveticaBold).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Absent").SetFont(helveticaBold).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("On Leave").SetFont(helveticaBold).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Total Work Hrs").SetFont(helveticaBold).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Avg. Work Hrs").SetFont(helveticaBold).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));

                            int srNo = 1;
                            foreach (var item in viewModel.ReportData)
                            {
                                table.AddCell(new Cell().Add(new Paragraph(srNo.ToString()).SetFont(helvetica).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)));
                                table.AddCell(new Cell().Add(new Paragraph(item.UserName ?? "").SetFont(helvetica).SetFontSize(8).SetTextAlignment(TextAlignment.LEFT)));
                                table.AddCell(new Cell().Add(new Paragraph(item.TotalDaysInMonth.ToString()).SetFont(helvetica).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)));
                                table.AddCell(new Cell().Add(new Paragraph(item.WorkingDaysInMonth.ToString()).SetFont(helvetica).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)));
                                table.AddCell(new Cell().Add(new Paragraph(item.PresentDays.ToString()).SetFont(helvetica).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)));
                                table.AddCell(new Cell().Add(new Paragraph(item.AbsentDays.ToString()).SetFont(helvetica).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)));
                                table.AddCell(new Cell().Add(new Paragraph(item.LeaveDays.ToString()).SetFont(helvetica).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)));
                                table.AddCell(new Cell().Add(new Paragraph(item.TotalWorkHours.ToString("N2")).SetFont(helvetica).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)));
                                table.AddCell(new Cell().Add(new Paragraph(item.AverageWorkHours.ToString("N2")).SetFont(helvetica).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)));
                                srNo++;
                            }
                            document.Add(table);
                        }
                        else
                        {
                            Paragraph noDataPara = new Paragraph("No attendance data found for the selected criteria.")
                                .SetFont(helveticaOblique).SetTextAlignment(TextAlignment.CENTER);
                            document.Add(noDataPara);
                        }
                        document.Close();
                    }
                }
                return memoryStream.ToArray();
            }
        }
    }
}