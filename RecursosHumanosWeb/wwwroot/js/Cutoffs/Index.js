// Parse date from DD/MM/YYYY format to Date object
function parseDate(dateStr) {
    if (!dateStr || dateStr === '-' || dateStr === 'Sin modificar') return null;
    const parts = dateStr.split('/');
    if (parts.length === 3) {
        return new Date(parts[2], parts[1] - 1, parts[0]);
    }
    return null;
}

// Check if date is within range
function isDateInRange(dateStr, fromDate, toDate) {
    const date = parseDate(dateStr);
    if (!date) return !fromDate && !toDate;

    const from = fromDate ? new Date(fromDate) : null;
    let to = toDate ? new Date(toDate) : null;

    if (to) {
        to.setHours(23, 59, 59, 999);
    }

    if (from && to) {
        return date >= from && date <= to;
    } else if (from) {
        return date >= from;
    } else if (to) {
        return date <= to;
    }
    return true;
}

// Filter functionality
function filterTable() {
    const statusFilter = document.getElementById('statusFilter').value;
    const dateFrom = document.getElementById('dateFrom').value;
    const dateTo = document.getElementById('dateTo').value;
    const createdByFilter = document.getElementById('createdByFilter').value;

    const tableBody = document.getElementById('tableBody');
    const rows = tableBody.getElementsByTagName('tr');
    const emptyState = document.getElementById('emptyState');
    let visibleRows = 0;

    for (let i = 0; i < rows.length; i++) {
        const row = rows[i];
        const cells = row.getElementsByTagName('td');
        let showRow = true;

        // Status filter
        if (statusFilter && showRow) {
            const statusText = cells[4].textContent.trim();
            showRow = statusText.includes(statusFilter);
        }

        // Date range filter
        if (showRow && (dateFrom || dateTo)) {
            const startDate = cells[0].textContent.trim();
            const endDate = cells[1].textContent.trim();

            let dateInRange = false;

            if (isDateInRange(startDate, dateFrom, dateTo)) {
                dateInRange = true;
            }

            if (isDateInRange(endDate, dateFrom, dateTo)) {
                dateInRange = true;
            }

            if (dateFrom && dateTo) {
                const inicioDate = parseDate(startDate);
                const finDate = parseDate(endDate);
                const filterFrom = new Date(dateFrom);
                const filterTo = new Date(dateTo);

                if (inicioDate && finDate) {
                    if (inicioDate <= filterTo && finDate >= filterFrom) {
                        dateInRange = true;
                    }
                }
            }

            showRow = dateInRange;
        }

        // User filter
        if (showRow && createdByFilter) {
            const createdBy = cells[5].textContent.trim();
            showRow = createdBy.includes(createdByFilter);
        }

        row.style.display = showRow ? '' : 'none';
        if (showRow) visibleRows++;
    }

    document.getElementById('cutoffsTable').parentElement.parentElement.style.display = visibleRows > 0 ? '' : 'none';
    emptyState.style.display = visibleRows > 0 ? 'none' : 'block';
}

// Clear all filters
function clearAllFilters() {
    document.getElementById('statusFilter').value = '';
    document.getElementById('createdByFilter').value = '';
    document.getElementById('dateFrom').value = '';
    document.getElementById('dateTo').value = '';

    filterTable();

    showNotification('✅ Todos los filtros han sido limpiados', 'success');
}

// Export functions (adapt to your needs)
function exportToExcel(type) {
    // Implement your export logic here
    showNotification('Función de exportación pendiente de implementar', 'info');
}

// Show notification
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `fixed top-4 right-4 z-50 px-6 py-4 rounded-lg shadow-lg text-white font-medium transition-all duration-300 transform translate-x-full`;

    if (type === 'success') {
        notification.className += ' bg-gradient-to-r from-green-500 to-green-600';
    } else {
        notification.className += ' bg-gradient-to-r from-blue-500 to-blue-600';
    }

    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        notification.classList.remove('translate-x-full');
    }, 100);

    setTimeout(() => {
        notification.classList.add('translate-x-full');
        setTimeout(() => {
            document.body.removeChild(notification);
        }, 300);
    }, 3000);
}