$(function () {
    var $deleteSelectedButton = $('#deleteSelectedUsersBtn').detach();

    var usersTable = $('#usersTable').DataTable({
        "pageLength": 10,
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        "columnDefs": [
            { "orderable": false, "targets": [0, 1, 7] },
            { "className": "dt-center", "targets": [0, 1, 5, 6, 7] }
        ],
        "dom": "<'row mb-3 align-items-center'<'col-sm-12 col-md-auto me-auto'l><'col-sm-12 col-md-auto'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        "initComplete": function (settings, json) {
            var $lengthMenuDiv = $('div.dataTables_length');
            if ($lengthMenuDiv.length) {
                $deleteSelectedButton.addClass('ms-2').appendTo($lengthMenuDiv);
                $lengthMenuDiv.css({ 'display': 'flex', 'align-items': 'center' });
            }
        },
        "fnDrawCallback": function (oSettings) {
            this.api().column(1, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                cell.innerHTML = i + 1;
            });
        }
    });

    // Select All / Individual Checkbox Logic
    $('#selectAllUsersCheckbox').on('click', function () {
        var rows = usersTable.rows({ 'search': 'applied' }).nodes();
        $('input[type="checkbox"].user-checkbox', rows).prop('checked', this.checked);
        toggleDeleteSelectedUsersButton();
    });

    $('#usersTable tbody').on('change', 'input[type="checkbox"].user-checkbox', function () {
        updateSelectAllUsersCheckboxState();
        toggleDeleteSelectedUsersButton();
    });

    function toggleDeleteSelectedUsersButton() {
        var anyChecked = $('.user-checkbox:checked', usersTable.rows({ 'search': 'applied' }).nodes()).length > 0;
        if (anyChecked && $deleteSelectedButton.length) {
            $deleteSelectedButton.fadeIn();
        } else if ($deleteSelectedButton.length) {
            $deleteSelectedButton.fadeOut();
            $('#selectAllUsersCheckbox').prop('checked', false);
        }
    }

    function updateSelectAllUsersCheckboxState() {
        var allFilteredRows = usersTable.rows({ 'search': 'applied' }).nodes();
        if (allFilteredRows.length === 0) {
            $('#selectAllUsersCheckbox').prop('checked', false);
            return;
        }
        var allCheckedInFiltered = $(allFilteredRows).find('input[type="checkbox"].user-checkbox:not(:disabled):checked').length === $(allFilteredRows).find('input[type="checkbox"].user-checkbox:not(:disabled)').length;
        $('#selectAllUsersCheckbox').prop('checked', allCheckedInFiltered);
    }

    // User Lock/Unlock Functionality
    function handleUserLockToggle(button, lockAction) {
        var userId = $(button).data('userid');
        var token = $('input[name="__RequestVerificationToken"]').val();
        var userName = $(button).closest('tr').find('td:eq(3)').text() || 'this user';

        Swal.fire({
            title: 'Are you sure?',
            text: `You are about to ${lockAction ? 'lock' : 'unlock'} user "${userName}".`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: lockAction ? '#d33' : '#28a745',
            cancelButtonColor: '#6c757d',
            confirmButtonText: `Yes, ${lockAction ? 'lock it!' : 'unlock it!'}`,
            cancelButtonText: 'No, cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/UserManagement/ToggleUserLock',
                    type: 'POST',
                    data: {
                        userId: userId,
                        lockUser: lockAction,
                        __RequestVerificationToken: token
                    },
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Success!',
                                text: response.message,
                                icon: 'success',
                                timer: 1500,
                                showConfirmButton: false
                            }).then(() => {
                                setTimeout(function () {
                                    window.location.reload();
                                }, 0);
                            });
                        } else {
                            Swal.fire('Error!', response.message || 'Operation failed.', 'error');
                        }
                    },
                    error: function (xhr) {
                        let errorMsg = 'An error occurred.';
                        try {
                            const jsonResponse = JSON.parse(xhr.responseText);
                            if (jsonResponse.message) errorMsg = jsonResponse.message;
                        } catch (e) {
                            errorMsg = xhr.responseText || errorMsg;
                        }
                        Swal.fire('Error!', errorMsg, 'error');
                    }
                });
            }
        });
    }

    $(document).on('click', '.btn-lock', function () {
        handleUserLockToggle(this, true);
    });

    $(document).on('click', '.btn-unlock', function () {
        handleUserLockToggle(this, false);
    });

    // --- Single User Delete Functionality ---
    $(document).on('click', '.btn-delete-user', function () {
        var userId = $(this).data('userid');
        var userName = $(this).closest('tr').find('td:eq(3)').text() || 'this user';
        var token = $('input[name="__RequestVerificationToken"]').val();

        Swal.fire({
            title: 'Are you sure?',
            text: `You are about to permanently delete "${userName}".`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, Delete',
            cancelButtonText: 'No, cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/UserManagement/DeleteUserConfirmed/' + userId,
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
                            usersTable.row('#user-row-' + userId).remove().draw(false);
                            updateSelectAllUsersCheckboxState();
                            toggleDeleteSelectedUsersButton();
                        } else {
                            Swal.fire('Error!', response.message, 'error');
                        }
                    },
                    error: function (xhr) {
                        Swal.fire('Error!', 'Failed to delete user. ' + (xhr.responseJSON?.message || xhr.responseText), 'error');
                    }
                });
            }
        });
    });

    // --- Delete Selected Users Functionality ---
    if ($deleteSelectedButton.length === 0) {
        $deleteSelectedButton = $('#deleteSelectedUsersBtn');
    }

    $deleteSelectedButton.on('click', function () {
        var selectedIds = [];
        $('.user-checkbox:checked', usersTable.rows({ 'search': 'applied' }).nodes()).each(function () {
            selectedIds.push($(this).val());
        });

        if (selectedIds.length === 0) {
            Swal.fire('No Selection', 'Please select at least one user to delete.', 'info');
            return;
        }

        var token = $('input[name="__RequestVerificationToken"]').val();

        Swal.fire({
            title: 'Are you sure?',
            text: "This will permanently delete " + selectedIds.length + " selected row.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, Delete them!',
            cancelButtonText: 'No, cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/UserManagement/DeleteSelectedUsersConfirmed',
                    type: 'POST',
                    headers: { "RequestVerificationToken": token },
                    data: { ids: selectedIds },
                    traditional: true,
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Processed!',
                                text: response.message,
                                icon: 'success',
                                timer: 2500,
                                showConfirmButton: false
                            });
                            selectedIds.forEach(function (id) {
                                usersTable.row('#user-row-' + id).remove();
                            });
                            usersTable.draw(false);
                        } else {
                            Swal.fire('Notice', response.message, 'warning');
                        }
                        $('#selectAllUsersCheckbox').prop('checked', false);
                        toggleDeleteSelectedUsersButton();
                    },
                    error: function (xhr) {
                        Swal.fire('Error!', 'Failed to delete selected users. ' + (xhr.responseJSON?.message || xhr.responseText), 'error');
                    }
                });
            }
        });
    });

    usersTable.on('draw.dt search.dt page.dt', function () {
        updateSelectAllUsersCheckboxState();
        toggleDeleteSelectedUsersButton();
    });

    updateSelectAllUsersCheckboxState();
    toggleDeleteSelectedUsersButton();
});
