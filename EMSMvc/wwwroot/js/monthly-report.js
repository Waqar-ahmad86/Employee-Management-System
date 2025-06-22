$(function () {
    $('#monthlyReportTable').DataTable({
        "pageLength": 10,
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        "columnDefs": [
            { "orderable": false, "targets": [0] }
        ]
    });

    $('#downloadPdfButton').on('click', function () {
        const year = $('#reportYear').val();
        const month = $('#reportMonth').val();
        const employeeName = $('#reportEmployeeName').val();
        const roleName = $('#reportRoleName').val();

        if (!year || !month) {
            toastr.error('Please select a year and month to generate the PDF report.', 'Selection Missing');
            return;
        }

        const downloadUrl = `/Attendance/DownloadMonthlyReportPdf?year=${encodeURIComponent(year)}&month=${encodeURIComponent(month)}`
            + (employeeName ? `&employeeName=${encodeURIComponent(employeeName)}` : '')
            + (roleName ? `&roleName=${encodeURIComponent(roleName)}` : '');

        toastr.info('Preparing PDF, please wait...', 'Download Started');
        window.location.href = downloadUrl;
    });
});
