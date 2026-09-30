/**
 * validation.js
 * Vanilla replacement for jquery.validate + jquery.validate.unobtrusive.
 * Reads the same data-val-* attributes ASP.NET's tag helpers already emit,
 * so no changes are needed to the Razor views themselves. Supports the four
 * validators this app actually uses: required, length, email, equalto.
 */
(function () {
    'use strict';

    var EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    function fieldsIn(form) {
        return Array.prototype.slice.call(form.querySelectorAll('[data-val="true"]'));
    }

    function resolveOtherField(form, field, otherRef) {
        // "*.Password" means "sibling of the current field within its naming container"
        var other = otherRef.replace(/^\*\./, '');
        var name = field.getAttribute('name') || '';
        var lastDot = name.lastIndexOf('.');
        var resolvedName = lastDot === -1 ? other : name.slice(0, lastDot + 1) + other;
        return form.querySelector('[name="' + resolvedName + '"]');
    }

    function validateField(form, field) {
        var value = field.value || '';
        var error = null;

        if (field.hasAttribute('data-val-required') && value.trim() === '') {
            error = field.getAttribute('data-val-required');
        } else if (value !== '') {
            if (field.hasAttribute('data-val-length')) {
                var min = field.hasAttribute('data-val-length-min') ? parseInt(field.getAttribute('data-val-length-min'), 10) : null;
                var max = field.hasAttribute('data-val-length-max') ? parseInt(field.getAttribute('data-val-length-max'), 10) : null;
                if ((min !== null && value.length < min) || (max !== null && value.length > max)) {
                    error = field.getAttribute('data-val-length');
                }
            }
            if (!error && field.hasAttribute('data-val-email') && !EMAIL_RE.test(value)) {
                error = field.getAttribute('data-val-email');
            }
            if (!error && field.hasAttribute('data-val-equalto')) {
                var other = resolveOtherField(form, field, field.getAttribute('data-val-equalto-other') || '');
                if (other && other.value !== value) {
                    error = field.getAttribute('data-val-equalto');
                }
            }
        }

        applyResult(form, field, error);
        return !error;
    }

    function applyResult(form, field, error) {
        var name = field.getAttribute('name') || '';
        var span = form.querySelector('[data-valmsg-for="' + name + '"]');
        var wrapper = field.closest('.mb-3') || field.closest('div');

        if (error) {
            if (wrapper) wrapper.classList.add('f_error');
            field.setAttribute('aria-invalid', 'true');
            if (span) {
                span.textContent = error;
                span.classList.remove('field-validation-valid');
                span.classList.add('field-validation-error');
                if (!span.id) span.id = (field.id || name.replace(/\./g, '_')) + '-error';
                var described = (field.getAttribute('aria-describedby') || '').split(/\s+/).filter(Boolean);
                if (described.indexOf(span.id) === -1) {
                    described.push(span.id);
                    field.setAttribute('aria-describedby', described.join(' '));
                }
            }
        } else {
            if (wrapper) wrapper.classList.remove('f_error');
            field.removeAttribute('aria-invalid');
            if (span) {
                span.textContent = '';
                span.classList.remove('field-validation-error');
                span.classList.add('field-validation-valid');
            }
        }
    }

    function wireForm(form) {
        if (form.dataset.cmsValidated) return;
        form.dataset.cmsValidated = 'true';

        var fields = fieldsIn(form);

        fields.forEach(function (field) {
            field.addEventListener('blur', function () { validateField(form, field); });
        });

        form.addEventListener('submit', function (e) {
            var allValid = true;
            var firstInvalid = null;

            fields.forEach(function (field) {
                if (!validateField(form, field)) {
                    allValid = false;
                    if (!firstInvalid) firstInvalid = field;
                }
            });

            if (!allValid) {
                e.preventDefault();
                firstInvalid.focus();
            }
        });
    }

    function autoWire() {
        document.querySelectorAll('form').forEach(function (form) {
            if (form.querySelector('[data-val="true"]')) wireForm(form);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', autoWire);
    } else {
        autoWire();
    }
}());
