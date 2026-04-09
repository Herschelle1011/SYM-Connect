//GROUP MODAL


// ── Cancel Modal ──────────────────────────────
function openCancelGroupModal(GroupId, Name) {
    document.getElementById('cancelGroupTitle').innerText = Name;
    document.getElementById('cancelGroupForm').action = '/SYMGroups/Cancel/' + GroupId;

    var modal = new bootstrap.Modal(document.getElementById('cancelGroupModal')); 
    modal.show();
}

//DELETE MODAL
function openDeleteGroupModal(GroupId, Name) {
    document.getElementById('deleteGroupTitle').innerText = Name;
    document.getElementById('deleteGroupForm').action = '/SYMGroups/Delete/' + GroupId;
    var modal = new bootstrap.Modal(document.getElementById('deleteGroupModal'));
    modal.show();
}


// Filter inactive groups search
function filterInactiveGroupRows(query) {
    const rows = document.querySelectorAll('.inactive-group-row');
    const noResults = document.getElementById('noInactiveGroupResults');
    const q = query.toLowerCase();
    let anyVisible = false;

    rows.forEach(row => {
        const name = row.getAttribute('data-name') || '';
        const match = name.includes(q);
        row.style.display = match ? 'flex' : 'none';
        if (match) anyVisible = true;
    });

    noResults.style.display = anyVisible ? 'none' : 'block';
}




//USERS MODAL

function openInactiveModal(Id, Email) {
    document.getElementById('InactiveUsersName').innerText = Email;
    document.getElementById('inactiveForm').action = '/Users/Inactive/' + Id;
    var modal = new bootstrap.Modal(document.getElementById('inactiveModal'));
    modal.show();
}

//DELETE MODAL
function openDisabledModal(Id, userName) {
    document.getElementById('disabledUserName').innerText = userName;
    document.getElementById('disabledForm').action = '/Users/Delete/' + Id;
    var modal = new bootstrap.Modal(document.getElementById('disabledModal'));
    modal.show();
}


//EVENTS MODAL

// ── Cancel Modal ──────────────────────────────
function openCancelModal(eventId, eventTitle) {
    document.getElementById('cancelEventTitle').innerText = eventTitle;
    document.getElementById('cancelForm').action = '/Events/Cancel/' + eventId;
    var modal = new bootstrap.Modal(document.getElementById('cancelModal'));
    modal.show();
}

//DELETE MODAL
function openDeleteModal(eventId, eventTitle) {
    document.getElementById('deleteEventTitle').innerText = eventTitle;
    document.getElementById('deleteForm').action = '/Events/Delete/' + eventId;
    var modal = new bootstrap.Modal(document.getElementById('deleteModal'));
    modal.show();
}




//ATTENDANCE RECORDS ---------------------


// ── Cancel Modal ──────────────────────────────
// DELETE MODAL — fixed variable names to match parameters
function openDeleteArchiveModal(attendanceId, userName, eventTitle, points) {
    // Populate modal with the correct data
    document.getElementById('modalUserName').innerText = userName;
    document.getElementById('modalEventTitle').innerText = eventTitle;
    document.getElementById('modalPoints').innerText = points;

    // Point the form to the correct delete URL
    document.getElementById('archiveDeleteForm').action =
        '/Attendance/Delete/' + attendanceId;

    // Show the modal
    var modal = new bootstrap.Modal(
        document.getElementById('archiveDeleteModal')
    );
    modal.show();
}




document.addEventListener("DOMContentLoaded", function () {

    //  get today date and time right now
    var now = new Date();

    // datetime-local needs: "YYYY-MM-DDTHH:MM"
    var year = now.getFullYear();
    var month = String(now.getMonth() + 1).padStart(2, '0');
    var day = String(now.getDate()).padStart(2, '0');
    var hours = String(now.getHours()).padStart(2, '0');
    var mins = String(now.getMinutes()).padStart(2, '0');

    var minValue = year + '-' + month + '-' + day + 'T' + hours + ':' + mins;

    // Step C — set it as the minimum allowed date
    document.getElementById('eventDateInput').min = minValue;
});


function togglePassword() {
    var input = document.getElementById('passwordInput');
    var eyeIcon = document.getElementById('eyeIcon');

    if (input.type === 'password') {
        input.type = 'text';
        eyeIcon.className = 'bi bi-eye-slash';
    } else {
        input.type = 'password';
        eyeIcon.className = 'bi bi-eye';
    }
}


function filterCancelledRows(val) {
    var rows = document.querySelectorAll('.cancelled-row');
    var anyVisible = false;

    rows.forEach(function (row) {
        var match = row.dataset.title
            .includes(val.toLowerCase());
        row.style.display = match ? 'flex' : 'none';
        if (match) anyVisible = true;
    });

    // Show/hide empty message
    var noMsg = document.getElementById('noCancelResults');
    if (noMsg) {
        noMsg.style.display = anyVisible ? 'none' : 'block';
    }

    // Clear search when modal closes
    var modal = document.getElementById('cancelledModal');
    modal.addEventListener('hidden.bs.modal', function () {
        document.getElementById('cancelSearch').value = '';
        filterCancelledRows('');
    });
}


function filterTable() {
    var input = document.getElementById('searchInput')
        .value.toLowerCase();

    // ✅ Get all rows inside the table body
    var rows = document.querySelectorAll('#tableBody tr');

    var visibleCount = 0;

    rows.forEach(function (row) {
        var fullName = row.cells[0]?.innerText.toLowerCase() ?? '';
        var email = row.cells[1]?.innerText.toLowerCase() ?? '';
        var role = row.cells[2]?.innerText.toLowerCase() ?? '';
        var status = row.cells[3]?.innerText.toLowerCase() ?? '';
        var joinedDate = row.cells[4]?.innerText.toLowerCase() ?? '';

        // Check if any column matches search
        var match = fullName.includes(input)
            || email.includes(input)
            || role.includes(input)
            || status.includes(input)
            || joinedDate.includes(input);

        row.style.display = match ? '' : 'none';

        if (match) visibleCount++;
    });

    var noResults = document.getElementById('noSearchResults');
    if (noResults) {
        noResults.style.display = visibleCount === 0 ? '' : 'none';
    }
}





// ── Open / Close dropdown ────────────────────────
function toggleRoleDropdown() {
    var dropdown = document.getElementById('roleDropdown');
    var isOpen = dropdown.style.display === 'block';
    dropdown.style.display = isOpen ? 'none' : 'block';
}

// ── Close dropdown when clicking outside ─────────
document.addEventListener('click', function (e) {
    var btn = document.getElementById('roleFilterBtn');
    var dropdown = document.getElementById('roleDropdown');
    if (!btn.contains(e.target) &&
        !dropdown.contains(e.target)) {
        dropdown.style.display = 'none';
    }
});




// ── Apply the filter ──────────────────────────────
function applyRoleFilter(role) {

    document.getElementById('roleFilterLabel').innerText =
        role === 'All' ? 'Role' : role;

    document.getElementById('roleDropdown').style.display = 'none';

    // ✅ Filter table rows
    var rows = document.querySelectorAll('#tableBody tr');

    rows.forEach(function (row) {
        // Skip the no-results row
        if (row.id === 'noSearchResults') return;

        // Role is in column index 2 (0=name, 1=email, 2=role)
        var rowRole = row.cells[2]?.innerText.trim() ?? '';

        if (role === 'All') {
            row.style.display = ''; // show all
        } else {
            row.style.display =
                rowRole.toLowerCase() === role.toLowerCase()
                    ? ''      // show
                    : 'none'; // hide
        }
    });

    // ✅ Check if any rows are visible
    var anyVisible = Array.from(rows).some(function (row) {
        return row.id !== 'noSearchResults' &&
            row.style.display !== 'none';
    });

    // ✅ Show/hide no results message
    var noMsg = document.getElementById('noSearchResults');
    if (noMsg) {
        noMsg.style.display = anyVisible ? 'none' : '';
    }

    // ✅ Highlight active filter option
    document.querySelectorAll('.role-option')
        .forEach(function (opt) {
            opt.style.background =
                opt.innerText.trim() === role
                    ? '#E1F5EE'   // highlight selected
                    : '';          // reset others
        });
}





function applyProgressFilter(selectedProgress) {

    // Update label
    document.getElementById('progressFilterLabel').innerText =
        selectedProgress === 'All' ? 'Progress' : selectedProgress;

    // Close dropdown
    document.getElementById('progressDropdown').style.display = 'none';

    // Filter rows
    var rows = document.querySelectorAll('#tableBody tr');

    rows.forEach(function (row) {

        if (row.id === 'noSearchResults') return;

        // Adjust index if needed
        var rowProgress = row.cells[7]?.innerText.trim() ?? '';

        if (selectedProgress === 'All') {
            row.style.display = '';
        } else {
            row.style.display =
                rowProgress.toLowerCase() === selectedProgress.toLowerCase()
                    ? ''
                    : 'none';
        }
    });

    // Check visible rows
    var anyVisible = Array.from(rows).some(function (row) {
        return row.id !== 'noSearchResults' &&
            row.style.display !== 'none';
    });

    // Show/hide no results
    var noMsg = document.getElementById('noSearchResults');
    if (noMsg) {
        noMsg.style.display = anyVisible ? 'none' : '';
    }

    // Highlight selected option
    document.querySelectorAll('.progress-option')
        .forEach(function (opt) {
            opt.style.background =
                opt.innerText.trim().includes(selectedProgress)
                    ? '#E1F5EE'
                    : '';
        });
}

// ── Open / Close dropdown ────────────────────────
function toggleProgressDropdown() {
    var dropdown = document.getElementById('progressDropdown');
    var isOpen = dropdown.style.display === 'block';
    dropdown.style.display = isOpen ? 'none' : 'block';
}

// ── Close dropdown when clicking outside ─────────
document.addEventListener('click', function (e) {
    var btn = document.getElementById('progressFilterBtn');
    var dropdown = document.getElementById('progressDropdown');
    if (!btn.contains(e.target) &&
        !dropdown.contains(e.target)) {
        dropdown.style.display = 'none';
    }

});


function applyStatusFilter(selectedStatus) {

    // Update label
    document.getElementById('roleFilterLabel').innerText =
        selectedStatus === 'All' ? 'Status' : selectedStatus;

    // Close dropdown
    document.getElementById('statusDropdown').style.display = 'none';

    // Filter rows
    var rows = document.querySelectorAll('#tableBody tr');

    rows.forEach(function (row) {

        if (row.id === 'noSearchResults') return;

        var rowStatus = row.cells[3]?.innerText.trim() ?? '';

        if (selectedStatus === 'All') {
            row.style.display = '';
        } else {
            row.style.display =
                rowStatus.toLowerCase() === selectedStatus.toLowerCase()
                    ? ''
                    : 'none';
        }
    });

    // Check visible rows
    var anyVisible = Array.from(rows).some(function (row) {
        return row.id !== 'noSearchResults' &&
            row.style.display !== 'none';
    });

    // Show/hide no results
    var noMsg = document.getElementById('noSearchResults');
    if (noMsg) {
        noMsg.style.display = anyVisible ? 'none' : '';
    }

    // Highlight selected option
    document.querySelectorAll('.status-option')
        .forEach(function (opt) {
            opt.style.background =
                opt.innerText.trim().includes(selectedStatus)
                    ? '#E1F5EE'
                    : '';
        });
}

function toggleStatusDropdown() {
    var dropdown = document.getElementById('statusDropdown');
    var isOpen = dropdown.style.display === 'block';
    dropdown.style.display = isOpen ? 'none' : 'block';
}

document.addEventListener('click', function (e) {
    var btn = document.getElementById('statusFilterBtn');
    var dropdown = document.getElementById('statusDropdown');

    if (!btn.contains(e.target) &&
        !dropdown.contains(e.target)) {
        dropdown.style.display = 'none';
    }
});