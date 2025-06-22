$(function () {
    var $deleteSelectedButton = $('#deleteSelectedBtn').detach();

    var employeesTable = $('#employeesTable').DataTable({
        "pageLength": 10,
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        "columnDefs": [
            { "orderable": false, "targets": [0, 1, 6] },
            { "className": "dt-center", "targets": "_all" }
        ],
        "dom": "<'row mb-3 align-items-center'<'col-sm-12 col-md-auto me-auto'l><'col-sm-12 col-md-auto'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        "initComplete": function (settings, json) {
            var $lengthMenuDiv = $('div.dataTables_length');
            $deleteSelectedButton.addClass('ms-2').appendTo($lengthMenuDiv);
            $lengthMenuDiv.css({ 'display': 'flex', 'align-items': 'center' });
        },
        "fnDrawCallback": function (oSettings) {
            this.api().column(1, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                cell.innerHTML = i + 1;
            });
        }
    });

    $('#selectAllCheckbox').on('click', function () {
        var rows = employeesTable.rows({ 'search': 'applied' }).nodes();
        $('input[type="checkbox"].employee-checkbox', rows).prop('checked', this.checked);
        toggleDeleteSelectedButton();
    });

    $('#employeesTable tbody').on('change', 'input[type="checkbox"].employee-checkbox', function () {
        updateSelectAllCheckboxState();
        toggleDeleteSelectedButton();
    });

    function toggleDeleteSelectedButton() {
        var anyChecked = $('.employee-checkbox:checked', employeesTable.rows({ 'search': 'applied' }).nodes()).length > 0;
        if (anyChecked) {
            $deleteSelectedButton.fadeIn();
        } else {
            $deleteSelectedButton.fadeOut();
            $('#selectAllCheckbox').prop('checked', false);
        }
    }

    function updateSelectAllCheckboxState() {
        var allFilteredRows = employeesTable.rows({ 'search': 'applied' }).nodes();
        if (allFilteredRows.length === 0) {
            $('#selectAllCheckbox').prop('checked', false);
            return;
        }

        var allCheckedInFiltered = true;
        $(allFilteredRows).each(function () {
            if (!$('input[type="checkbox"].employee-checkbox', this).prop('checked')) {
                allCheckedInFiltered = false;
                return false;
            }
        });
        $('#selectAllCheckbox').prop('checked', allCheckedInFiltered);
    }

    $(document).on('click', '.delete-btn', function () {
        var employeeId = $(this).data('id');
        Swal.fire({
            title: 'Are you sure?',
            text: "This action cannot be undone!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!',
            cancelButtonText: 'No, cancel!'
        }).then((result) => {
            if (result.isConfirmed) {
                var token = $('input[name="__RequestVerificationToken"]').val();
                $.ajax({
                    url: `/Employee/DeleteConfirmed/${employeeId}`,
                    type: 'POST',
                    headers: { "RequestVerificationToken": token },
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Deleted!',
                                text: response.message,
                                icon: 'success',
                                timer: 2000,
                                showConfirmButton: false
                            });
                            employeesTable.row('#employee-row-' + employeeId).remove().draw(false);
                            updateSelectAllCheckboxState();
                            toggleDeleteSelectedButton();
                        } else {
                            Swal.fire('Error!', response.message, 'error');
                        }
                    },
                    error: function (xhr) {
                        Swal.fire('Error!', 'Failed to delete employee. ' + xhr.responseText, 'error');
                    }
                });
            }
        });
    });

    $deleteSelectedButton.on('click', function () {
        var selectedIds = [];
        $('.employee-checkbox:checked', employeesTable.rows({ 'search': 'applied' }).nodes()).each(function () {
            selectedIds.push($(this).val());
        });

        if (selectedIds.length === 0) {
            Swal.fire('No Selection', 'Please select at least one employee to delete.', 'info');
            return;
        }

        Swal.fire({
            title: 'Are you sure?',
            text: "This will delete " + selectedIds.length + " selected employee(s)!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete them!',
            cancelButtonText: 'No, cancel!'
        }).then((result) => {
            if (result.isConfirmed) {
                var token = $('input[name="__RequestVerificationToken"]').val();
                $.ajax({
                    url: '/Employee/DeleteSelectedConfirmed',
                    type: 'POST',
                    headers: { "RequestVerificationToken": token },
                    data: { ids: selectedIds },
                    traditional: true,
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Deleted!',
                                text: response.message,
                                icon: 'success',
                                timer: 2000,
                                showConfirmButton: false
                            });
                            selectedIds.forEach(function (id) {
                                employeesTable.row('#employee-row-' + id).remove();
                            });
                            employeesTable.draw(false);
                            $('#selectAllCheckbox').prop('checked', false);
                            toggleDeleteSelectedButton();
                        } else {
                            Swal.fire('Error!', response.message, 'error');
                        }
                    },
                    error: function (xhr) {
                        Swal.fire('Error!', 'Failed to delete selected employees. ' + xhr.responseText, 'error');
                    }
                });
            }
        });
    });

    employeesTable.on('draw.dt search.dt page.dt', function () {
        updateSelectAllCheckboxState();
        toggleDeleteSelectedButton();
    });

    updateSelectAllCheckboxState();
    toggleDeleteSelectedButton();
});
