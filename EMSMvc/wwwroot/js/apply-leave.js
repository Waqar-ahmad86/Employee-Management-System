function calculateWorkingDays(startDateStr, endDateStr) {
    let startDate = new Date(startDateStr);
    let endDate = new Date(endDateStr);
    let count = 0;

    if (isNaN(startDate.getTime()) || isNaN(endDate.getTime()) || endDate < startDate) {
        return 0;
    }

    let currentDate = new Date(startDate.toISOString().slice(0, 10));

    while (currentDate <= endDate) {
        const dayOfWeek = currentDate.getDay();
        if (dayOfWeek !== 0 && dayOfWeek !== 6) {
            count++;
        }
        currentDate.setDate(currentDate.getDate() + 1);
    }
    return count;
}

$(function () {
    function updateCalculatedDays() {
        const startDateVal = $('#leaveStartDate').val();
        const endDateVal = $('#leaveEndDate').val();
        if (startDateVal && endDateVal) {
            const days = calculateWorkingDays(startDateVal, endDateVal);
            $('#calculatedDays').val(days);
        } else {
            $('#calculatedDays').val(0);
        }
    }

    $('#leaveStartDate, #leaveEndDate').on('change', function () {
        updateCalculatedDays();
    });

    updateCalculatedDays();
});
