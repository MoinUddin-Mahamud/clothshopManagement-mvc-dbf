/**
 * ClothShop Management - Client-side Validation and AJAX Helper Functions
 * Provides common validation and AJAX functionality for CRUD operations
 */

// Document Ready
$(document).ready(function () {
    // Initialize form validation
    initializeValidation();
    
    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();
    
    // Initialize popovers
    $('[data-toggle="popover"]').popover();
});

/**
 * Initialize Client-side Validation
 */
function initializeValidation() {
    // Custom validation for price fields
    $.validator.addMethod("currencycheck", function (value, element) {
        return /^\d+(\.\d{1,2})?$/.test(value);
    }, "Please enter a valid currency amount (e.g., 100.50)");

    // Custom validation for phone
    $.validator.addMethod("phonecheck", function (value, element) {
        return /^\d{10,11}$/.test(value.replace(/\D/g, ''));
    }, "Please enter a valid phone number (10-11 digits)");

    // Initialize jQuery Validate on all forms
    $('form').validate({
        errorClass: "is-invalid",
        validClass: "is-valid",
        errorElement: "small",
        errorPlacement: function (error, element) {
            error.addClass("form-text text-danger");
            if (element.type === "checkbox") {
                element.closest(".form-group").append(error);
            } else {
                element.closest(".form-group").append(error);
            }
        },
        highlight: function (element, errorClass, validClass) {
            $(element).addClass("is-invalid").removeClass("is-valid");
        },
        unhighlight: function (element, errorClass, validClass) {
            $(element).addClass("is-valid").removeClass("is-invalid");
        }
    });
}

/**
 * Show notification toast
 * @param {string} message - Message to display
 * @param {string} type - Type: 'success', 'error', 'warning', 'info'
 */
function showNotification(message, type = 'info') {
    const alertClass = {
        'success': 'alert-success',
        'error': 'alert-danger',
        'warning': 'alert-warning',
        'info': 'alert-info'
    };

    const iconClass = {
        'success': 'fa-check-circle',
        'error': 'fa-times-circle',
        'warning': 'fa-exclamation-circle',
        'info': 'fa-info-circle'
    };

    const html = `
        <div class="alert ${alertClass[type]} alert-dismissible fade show" role="alert">
            <i class="fas ${iconClass[type]}"></i> ${message}
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
    `;

    // Add to top of page
    $('body').prepend(html);

    // Auto dismiss after 5 seconds
    setTimeout(function () {
        $('.alert:first').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);
}

/**
 * Format currency value
 * @param {number} value - Number to format
 * @returns {string} Formatted currency string
 */
function formatCurrency(value) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(value);
}

/**
 * Format date value
 * @param {string|Date} date - Date to format
 * @param {string} format - Format pattern (optional)
 * @returns {string} Formatted date string
 */
function formatDate(date, format = 'yyyy-MM-dd') {
    if (typeof date === 'string') {
        date = new Date(date);
    }

    const pad = (n) => n < 10 ? '0' + n : n;

    return format
        .replace('yyyy', date.getFullYear())
        .replace('MM', pad(date.getMonth() + 1))
        .replace('dd', pad(date.getDate()))
        .replace('HH', pad(date.getHours()))
        .replace('mm', pad(date.getMinutes()))
        .replace('ss', pad(date.getSeconds()));
}

/**
 * Generic AJAX Delete Function
 * @param {string} url - URL to delete
 * @param {string} itemName - Name of item being deleted (for confirmation)
 * @param {function} callback - Callback function after successful deletion
 */
function deleteItemAjax(url, itemName, callback) {
    if (confirm(`Are you sure you want to delete ${itemName}? This action cannot be undone.`)) {
        $.ajax({
            url: url,
            type: 'POST',
            headers: {
                'X-CSRF-TOKEN': $('[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    showNotification(`${itemName} deleted successfully!`, 'success');
                    if (typeof callback === 'function') {
                        callback();
                    }
                } else {
                    showNotification(`Error: ${response.message}`, 'error');
                }
            },
            error: function (xhr, status, error) {
                showNotification(`An error occurred: ${error}`, 'error');
            }
        });
    }
}

/**
 * Generic AJAX Status Toggle Function
 * @param {string} url - URL to toggle status
 * @param {string} itemId - ID of item
 * @param {function} callback - Callback function after successful toggle
 */
function toggleStatusAjax(url, itemId, callback) {
    $.ajax({
        url: url,
        type: 'POST',
        data: { id: itemId },
        headers: {
            'X-CSRF-TOKEN': $('[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                showNotification('Status updated successfully!', 'success');
                if (typeof callback === 'function') {
                    callback(response);
                }
            } else {
                showNotification(`Error: ${response.message}`, 'error');
            }
        },
        error: function (xhr, status, error) {
            showNotification(`An error occurred: ${error}`, 'error');
        }
    });
}

/**
 * Load content into a modal via AJAX
 * @param {string} url - URL to load
 * @param {string} modalId - ID of modal element
 */
function loadModalContent(url, modalId) {
    $.ajax({
        url: url,
        type: 'GET',
        success: function (data) {
            $('#' + modalId).html(data);
            $('#' + modalId).modal('show');
        },
        error: function () {
            showNotification('Failed to load content', 'error');
        }
    });
}

/**
 * Validate image file size
 * @param {File} file - File object
 * @param {number} maxSizeMB - Maximum size in MB
 * @returns {boolean} True if valid
 */
function validateImageFile(file, maxSizeMB = 2) {
    const maxSizeBytes = maxSizeMB * 1024 * 1024;
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/bmp'];

    if (!allowedTypes.includes(file.type)) {
        showNotification('Please select a valid image file (jpg, png, gif, bmp)', 'error');
        return false;
    }

    if (file.size > maxSizeBytes) {
        showNotification(`File size must not exceed ${maxSizeMB}MB`, 'error');
        return false;
    }

    return true;
}

/**
 * Generate table row for list items
 * @param {object} item - Data object
 * @param {array} columns - Array of column definitions
 * @returns {string} HTML table row
 */
function generateTableRow(item, columns) {
    let html = '<tr>';

    columns.forEach(col => {
        let value = item[col.field];
        
        // Apply formatter if provided
        if (col.formatter && typeof col.formatter === 'function') {
            value = col.formatter(value);
        }

        html += `<td>${value || '-'}</td>`;
    });

    html += '</tr>';
    return html;
}

/**
 * Disable form submit buttons during processing
 * @param {jQuery} $form - Form element
 */
function disableFormButtons($form) {
    $form.find('button[type="submit"]')
        .prop('disabled', true)
        .html('<i class="fas fa-spinner fa-spin"></i> Processing...');
}

/**
 * Enable form submit buttons
 * @param {jQuery} $form - Form element
 * @param {string} text - Button text to restore
 */
function enableFormButtons($form, text = '<i class="fas fa-save"></i> Save') {
    $form.find('button[type="submit"]')
        .prop('disabled', false)
        .html(text);
}

/**
 * Reset form validation
 * @param {jQuery} $form - Form element
 */
function resetFormValidation($form) {
    $form.find('input, textarea, select')
        .removeClass('is-valid is-invalid');
    $form.find('small.text-danger').remove();
}

/**
 * Clear all form errors
 * @param {jQuery} $form - Form element
 */
function clearFormErrors($form) {
    $form.find('.is-invalid').removeClass('is-invalid');
    $form.find('small.form-text.text-danger').remove();
}
