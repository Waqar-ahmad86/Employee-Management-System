function formatUtcToLocalTimeString(utcDateTimeString, options) {
    if (!utcDateTimeString || utcDateTimeString.trim() === "") {
        return "";
    }
    try {
        const dateObj = new Date(utcDateTimeString);
        if (isNaN(dateObj.getTime())) {
            console.warn('Invalid date string for conversion:', utcDateTimeString);
            return "";
        }
        return dateObj.toLocaleTimeString([], options);
    } catch (e) {
        console.error("Error formatting time:", utcDateTimeString, e);
        return "";
    }
}

function formatUtcToLocalDateString(utcDateString, options) {
    if (!utcDateString || utcDateString.trim() === "") {
        return "";
    }
    try {
        const dateObj = new Date(utcDateString);
        if (isNaN(dateObj.getTime())) {
            console.warn('Invalid date string for conversion:', utcDateString);
            return "";
        }
        return dateObj.toLocaleDateString(undefined, options);
    } catch (e) {
        console.error("Error formatting date:", utcDateString, e);
        return "";
    }
}

$(function () {
    const utcCheckInFull = $('#utcCheckInFull').val();
    const utcCheckOutFull = $('#utcCheckOutFull').val();
    const recordDateForJs = $('#recordDateUtc').val();

    if (utcCheckInFull) {
        const localCheckInTime = formatUtcToLocalTimeString(utcCheckInFull, { hour: '2-digit', minute: '2-digit', hour12: true });
        if (localCheckInTime) {
            $('#checkInDisplay').text(localCheckInTime);
        }
    }

    if (utcCheckOutFull) {
        const localCheckOutTime = formatUtcToLocalTimeString(utcCheckOutFull, { hour: '2-digit', minute: '2-digit', hour12: true });
        if (localCheckOutTime) {
            $('#checkOutDisplay').text(localCheckOutTime);
        }
    }

    if (recordDateForJs) {
        const localDate = formatUtcToLocalDateString(recordDateForJs, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
        if (localDate) $('#displayDate').text(localDate);
    } else {
        $('#displayDate').text(new Date().toLocaleDateString(undefined, {
            weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
        }));
    }
});
