function formatUtcToLocalDateTimeString(utcDateTimeString, options) {
    if (!utcDateTimeString || utcDateTimeString.trim() === "") return "";
    try {
        const dateObj = new Date(utcDateTimeString);
        return isNaN(dateObj.getTime()) ? "" : dateObj.toLocaleTimeString([], options);
    } catch (e) {
        return "";
    }
}

function formatUtcToLocalDateString(utcDateString, options) {
    if (!utcDateString || utcDateString.trim() === "") return "";
    try {
        const dateObj = new Date(utcDateString);
        return isNaN(dateObj.getTime()) ? "" : dateObj.toLocaleDateString(undefined, options);
    } catch (e) {
        return "";
    }
}

$(function () {
    const table = $('#allAttendanceTable').DataTable({
        "pageLength": 10,
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        "columnDefs": [{ "orderable": false, "targets": [0] }],
        "drawCallback": function (settings) {
            $('.local-display-date', this.api().table().body()).each(function () {
                const utcDateStr = $(this).data('utc-date');
                const localDate = formatUtcToLocalDateString(utcDateStr, { day: '2-digit', month: 'short', year: 'numeric' });
                $(this).text(localDate || $(this).text());
            });

            $('.local-display-time', this.api().table().body()).each(function () {
                const utcDateTimeStr = $(this).data('utc-datetime');
                const localTime = formatUtcToLocalDateTimeString(utcDateTimeStr, { hour: '2-digit', minute: '2-digit', hour12: true });
                $(this).text(localTime || $(this).text());
            });
        }
    });

    $('.local-display-date').each(function () {
        const utcDateStr = $(this).data('utc-date');
        const localDate = formatUtcToLocalDateString(utcDateStr, { day: '2-digit', month: 'short', year: 'numeric' });
        if (localDate) $(this).text(localDate);
    });

    $('.local-display-time').each(function () {
        const utcDateTimeStr = $(this).data('utc-datetime');
        const localTime = formatUtcToLocalDateTimeString(utcDateTimeStr, { hour: '2-digit', minute: '2-digit', hour12: true });
        if (localTime) $(this).text(localTime);
    });
});
