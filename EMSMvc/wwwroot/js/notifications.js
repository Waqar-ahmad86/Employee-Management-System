"use strict";

var notificationConnection = null;

/**
 * @param {number} count - The number of unread notifications.
 */
function updateNotificationBell(count) {
    const bell = $('#notificationBell');
    const badge = $('#notificationBadge');

    if (bell.length && badge.length) {
        if (count > 0) {
            badge.text(count).show();
            bell.addClass('has-notifications');
        } else {
            badge.text('0').hide();
            bell.removeClass('has-notifications');
        }
    }
}

/**
 * Adds a single notification to the top of the dropdown list.
 * @param {object} notification - The notification object (should match NotificationVM).
 */
function addNotificationToDropdown(notification) {
    const dropdown = $('#notificationDropdownMenu');
    if (dropdown.length) {
        dropdown.find('.no-notifications-item, .loading-notifications-item').remove();

        const linkHref = notification.link ? notification.link : '#';
        const isUnreadClass = notification.isRead ? '' : 'fw-bold';
        const timeDisplay = notification.timeAgo || new Date(notification.createdAt).toLocaleString();

        const itemHtml = `
            <li>
                <a class="dropdown-item notification-item ${isUnreadClass}" href="${linkHref}" data-notification-id="${notification.id}">
                    <div class="small">${notification.message}</div>
                    <div class="text-muted small time-ago">${timeDisplay}</div>
                </a>
            </li>`;

        dropdown.prepend(itemHtml);

        const maxItemsInDropdown = 5;
        while (dropdown.children('li.notification-item').length > maxItemsInDropdown) {
            dropdown.children('li.notification-item:last-child').remove();
        }

        if (dropdown.children('li.notification-item').length === 0) {
            dropdown.append('<li class="no-notifications-item"><span class="dropdown-item text-muted text-center">No new notifications</span></li>');
        }
    }
}

async function fetchAndUpdateUnreadCount() {
    try {
        if (typeof isUserLoggedIn !== 'undefined' && isUserLoggedIn) {
            $.get("/Notifications/GetUnreadCount", function (data) {
                if (data && typeof data.count !== 'undefined') {
                    updateNotificationBell(data.count);
                }
            }).fail(function (xhr) {
                console.error("Error fetching unread notification count:", xhr.responseText);
            });
        }
    } catch (e) {
        console.error("Exception in fetchAndUpdateUnreadCount:", e);
    }
}

async function populateNotificationDropdown() {
    try {
        if (typeof isUserLoggedIn !== 'undefined' && isUserLoggedIn) {
            $.get("/Notifications/GetRecentNotifications", function (notifications) {
                const dropdown = $('#notificationDropdownMenu');
                if (!dropdown.length) return;

                dropdown.find('li.notification-item').remove();
                dropdown.find('.no-notifications-item, .loading-notifications-item').remove();


                if (notifications && notifications.length > 0) {
                    notifications.forEach(addNotificationToDropdown);
                } else {
                    dropdown.children('li:has(a[asp-controller="Notifications"][asp-action="Index"])')
                        .first()
                        .prev('li:has(hr.dropdown-divider)')
                        .before(
                        '<li class="no-notifications-item"><span class="dropdown-item text-muted text-center">No new notifications</span></li>'
                    );
                }
            }).fail(function (xhr) {
                console.error("Error fetching recent notifications:", xhr.responseText);
                const dropdown = $('#notificationDropdownMenu');
                dropdown.find('li.notification-item').remove();
                dropdown.find('.no-notifications-item, .loading-notifications-item').remove();
                dropdown.children('li:has(a[asp-controller="Notifications"][asp-action="Index"])').first().prev('li:has(hr.dropdown-divider)').before(
                    '<li class="no-notifications-item"><span class="dropdown-item text-danger text-center">Error loading</span></li>'
                );
            });
        }
    } catch (e) {
        console.error("Exception in populateNotificationDropdown:", e);
    }
}

function startNotificationConnection() {
    if (typeof isUserLoggedIn === 'undefined' || !isUserLoggedIn) {
        console.log("User not logged in, SignalR connection for notifications not started.");
        return;
    }
    if (!$('#notificationBellContainer').length) {
        console.log("Notification UI container not present, SignalR connection not started.");
        return;
    }

    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub", {
            accessTokenFactory: () => {
                return window.apiAuthToken || null;
            }
        })
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .build();

    notificationConnection.on("ReceiveNotification", function (notification) {
        console.log("SignalR Notification Received:", notification);
        addNotificationToDropdown(notification);
        fetchAndUpdateUnreadCount();
    });

    notificationConnection.start()
        .then(function () {
            console.log("Notification SignalR Connected.");
            fetchAndUpdateUnreadCount();
        })
        .catch(function (err) {
            console.error("Notification SignalR Connection Error: " + err.toString());
        });

    notificationConnection.onreconnecting(error => {
        console.warn(`Notification SignalR connection lost due to error "${error}". Reconnecting.`);
    });

    notificationConnection.onreconnected(connectionId => {
        console.log(`Notification SignalR connection reestablished. Connected with connectionId "${connectionId}".`);
        fetchAndUpdateUnreadCount();
        populateNotificationDropdown();
    });
}

/**
 * Marks a specific notification as read on the server and updates UI.
 * @param {string} notificationId - The GUID of the notification.
 * @param {string} [navigateToLink=null] - Optional URL to navigate to after success.
 */
function markNotificationAsReadClientSide(notificationId, navigateToLink = null) {
    var token = $('input[name="__RequestVerificationToken"]').val();
    if (!token && document.querySelector('meta[name="__RequestVerificationToken"]')) {
        token = document.querySelector('meta[name="__RequestVerificationToken"]').getAttribute("content");
    }

    $.ajax({
        url: '/Notifications/MarkAsRead',
        type: 'POST',
        headers: { "RequestVerificationToken": token },
        data: { id: notificationId },
        success: function (response) {
            $(`.notification-item[data-notification-id="${notificationId}"]`).removeClass('fw-bold');
            fetchAndUpdateUnreadCount();
            if (navigateToLink && navigateToLink !== '#') {
                window.location.href = navigateToLink;
            }
        },
        error: function (xhr) {
            console.error('Failed to mark notification as read:', xhr.responseText);
            if (navigateToLink && navigateToLink !== '#') {
                window.location.href = navigateToLink;
            }
        }
    });
}


$(document).ready(function () {
    $('.auto-dismiss-alert').each(function () {
        var alert = $(this);
        setTimeout(function () {
            var bsAlert = new bootstrap.Alert(alert[0]);
            bsAlert.close();
        }, 3000);
    });

    if (typeof isUserLoggedIn !== 'undefined' && isUserLoggedIn && $('#notificationBellContainer').length > 0) {
        startNotificationConnection();
        populateNotificationDropdown();
    }

    $('#notificationDropdownMenu').on('click', '.notification-item', function (e) {
        const notificationId = $(this).data('notification-id');
        const link = $(this).attr('href');
        const isUnread = $(this).hasClass('fw-bold');

        if (notificationId && isUnread) {
            e.preventDefault();
            markNotificationAsReadClientSide(notificationId, link);
        } else if (link === '#') {
            e.preventDefault();
        }
    });

    $('#notificationDropdownMenu').on('click', '#markAllNotificationsAsRead', function (e) {
        e.preventDefault();
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (!token && document.querySelector('meta[name="__RequestVerificationToken"]')) {
            token = document.querySelector('meta[name="__RequestVerificationToken"]').getAttribute("content");
        }

        $.ajax({
            url: '/Notifications/MarkAllAsRead',
            type: 'POST',
            headers: { "RequestVerificationToken": token },
            success: function (response) {
                console.log('All notifications marked as read.');
                $('#notificationDropdownMenu .notification-item').removeClass('fw-bold');
                updateNotificationBell(0);
            },
            error: function (xhr) {
                console.error('Failed to mark all notifications as read:', xhr.responseText);
            }
        });
    });

    var notificationDropdownEl = document.getElementById('notificationBellContainer');
    if (notificationDropdownEl) {
        notificationDropdownEl.addEventListener('show.bs.dropdown', function () {
            const badge = $('#notificationBadge');

            if (badge.length && parseInt(badge.text()) > 0) {

                var token = $('input[name="__RequestVerificationToken"]').val();
                if (!token && document.querySelector('meta[name="__RequestVerificationToken"]')) {
                    token = document.querySelector('meta[name="__RequestVerificationToken"]').getAttribute("content");
                }

                $.ajax({
                    url: '/Notifications/MarkAllAsRead',
                    type: 'POST',
                    headers: { "RequestVerificationToken": token },
                    success: function (response) {
                        $('#notificationDropdownMenu .notification-item').removeClass('fw-bold');
                        updateNotificationBell(0);
                    },
                    error: function (xhr) {
                        console.error('Error marking all as read on dropdown open:', xhr.responseText);
                    }
                });
            }
            populateNotificationDropdown();
        });
    }
});