import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { providersApi } from '../api/providersApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { createShowError, createTouch } from '../../../utils/validationUtils.js';

// Port of ManageProvider.aspx: one form for a new provider (/admin/providers/0) and an existing one (/admin/providers/:key).
export function useProviderDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logError, logApiError } = useLogger();

    // ================================================================
    // Blank shapes
    // ================================================================
    const blankContact = () => ({ title: '', firstName: '', lastName: '', emailAddress: '', phone: '', wirelessPhone: '' });
    const blankAddress = () => ({
        addressLine1: '', addressLine2: '', city: '', stateId: null, countyId: null, zipcode: '', zipExtension: '', contact: blankContact()
    });
    const blankForm = () => ({
        rowVersion: null, name: '', abbreviation: '', edisonNumber: '', npi: '',
        physicalAddress: blankAddress(), remitAddress: blankAddress()
    });

    // The server sends nulls; inputs want strings.
    const strings = (o) => Object.fromEntries(Object.entries(o).map(([k, v]) => [k, v ?? '']));
    const address = (a) => ({
        ...blankAddress(), ...strings(a),
        stateId: a.stateId ?? null, countyId: a.countyId ?? null,
        contact: strings({ ...blankContact(), ...a.contact })
    });

    // ================================================================
    // State
    // ================================================================
    const isNew = computed(() => route.params.key === '0');
    const title = computed(() => (isNew.value ? 'Add Provider' : 'Provider Details'));

    const form = reactive(blankForm());
    const providerId = ref(0);
    const info = ref(null);
    const states = ref([]);
    const counties = ref([]);
    const serverErrors = ref({});    // { 'physicalAddress.city': [messages] } from a 400 validation response
    const dialog = ref('');          // '' or 'delete'
    const tab = ref('demographics'); // 'demographics', 'physicalAddress' or 'remitAddress'
    const isLoading = ref(true);
    const isSaving = ref(false);

    // The tabs after Demographics, and the fields in each address and its contact.
    const sections = [
        { key: 'physicalAddress', title: 'Physical Address' },
        { key: 'remitAddress', title: 'Remit Address' }
    ];
    const addressFields = [
        { f: 'addressLine1', label: 'Address line 1', col: 'col-12' },
        { f: 'addressLine2', label: 'Address line 2', col: 'col-12' },
        { f: 'city', label: 'City', col: 'col-md-6' }
    ];
    const contactFields = [
        { f: 'title', label: 'Title', col: 'col-md-4' },
        { f: 'firstName', label: 'First Name', col: 'col-md-4' },
        { f: 'lastName', label: 'Last Name', col: 'col-md-4' },
        { f: 'emailAddress', label: 'Email', col: 'col-md-6', type: 'email' },
        { f: 'phone', label: 'Phone', col: 'col-md-3', type: 'tel' },
        { f: 'wirelessPhone', label: 'Mobile', col: 'col-md-3', type: 'tel' }
    ];

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('providers.edit');

    // ================================================================
    // Validation
    // ================================================================
    const submitted = ref(false);
    // Which fields the user has visited. A field only shows its message once it was left, or after a submit.
    const touched = reactive({});
    const touch = createTouch(touched);

    const digits = (value, count) => new RegExp(`^\\d{${count}}$`).test(String(value ?? '').trim());
    const filled = (value) => !!String(value ?? '').trim();

    // The error message for each field; a field that is fine has no entry. The server also checks the NPI and Edison number are unique.
    const clientErrors = computed(() => {
        const e = {};
        const name = form.name.trim();
        if (!name) {
            e.name = 'Provider name is required.';
        } else if (name.length <= 10) {
            e.name = 'Provider name must be more than 10 characters.';
        }
        if (!form.abbreviation.trim()) {
            e.abbreviation = 'Provider abbreviation is required.';
        }
        if (!form.edisonNumber.trim()) {
            e.edisonNumber = 'Edison number is required.';
        } else if (!digits(form.edisonNumber, 10)) {
            e.edisonNumber = 'Edison number must be exactly 10 digits.';
        }
        if (filled(form.npi) && !digits(form.npi, 10)) {
            e.npi = 'NPI must be exactly 10 digits.';
        }

        // An address, with its contact, counts once any of it is filled in.
        for (const { key, title } of sections) {
            const address = form[key];
            if (filled(address.zipcode) && !digits(address.zipcode, 5)) {
                e[`${key}.zipcode`] = 'ZIP must be a 5 digit number.';
            }
            if (filled(address.zipExtension) && !digits(address.zipExtension, 4)) {
                e[`${key}.zipExtension`] = 'ZIP+4 must be a 4 digit number.';
            }
            if (filled(address.contact.emailAddress) && !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(address.contact.emailAddress.trim())) {
                e[`${key}.contact.emailAddress`] = `${title} contact email is not valid.`;
            }
        }
        return e;
    });

    const isValid = computed(() => Object.keys(clientErrors.value).length === 0);
    // A field shows its message only after it was touched, or after a submit was attempted.
    const showError = createShowError(touched, submitted, clientErrors);

    // What the screen shows for a field: the server's message when it sent one, else the form's own once it is due.
    const msg = (field) => serverErrors.value[field]?.join(' ') || (showError(field) ? clientErrors.value[field] : '');
    const err = (field) => (msg(field) ? 1 : 0);

    function resetValidation() {
        submitted.value = false;
        serverErrors.value = {};
        Object.keys(touched).forEach((field) => delete touched[field]);
    }

    // Shows the first tab that has a problem.
    function showTabWithError(fields) {
        const inAddress = (field) => field.startsWith('physicalAddress') || field.startsWith('remitAddress');
        const firstAddressField = fields.find(inAddress);
        tab.value = fields.some((field) => !inAddress(field)) ? 'demographics' : firstAddressField.split('.')[0];
    }

    // ================================================================
    // Loading
    // ================================================================
    function fill(detail) {
        providerId.value = detail.id;
        info.value = detail;
        Object.assign(form, blankForm(), {
            rowVersion: detail.rowVersion, name: detail.name ?? '', abbreviation: detail.abbreviation ?? '',
            edisonNumber: detail.edisonNumber ?? '', npi: detail.npi ?? '',
            physicalAddress: address(detail.physicalAddress), remitAddress: address(detail.remitAddress)
        });
        resetValidation();
    }

    async function getPageData() {
        try {
            const lookups = await providersApi.lookups();
            states.value = lookups.states;
            counties.value = lookups.counties;
            if (!isNew.value) {
                fill(await providersApi.get(route.params.key));
            }
        } catch (e) {
            logApiError(e, { fallback: 'Provider not found.' });
        }
    }

    // ================================================================
    // Navigation
    // ================================================================
    function backToList() {
        restoreSearchOnReturn();
        router.push('/admin/providers');
    }

    function cancel() {
        backToList();
    }

    // ================================================================
    // Save and delete
    // ================================================================
    // Field problems (400) show next to their inputs; anything else (403, 409 conflict, ...) is a toast.
    function fail(e) {
        const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
        if (!fieldErrors) {
            logApiError(e);
            return;
        }
        serverErrors.value = fieldErrors;
        logError('Please correct the validation errors first.');
        showTabWithError(Object.keys(fieldErrors));
    }

    // Checks permission, then saves. On success it returns to the list.
    async function save() {
        if (isSaving.value) {
            return;
        }
        if (!canEdit) {
            logError('You do not have permission to manage providers.');
            return;
        }
        isSaving.value = true;
        try {
            if (isNew.value) {
                await providersApi.create(form);
            } else {
                await providersApi.update(providerId.value, form);
            }
            logSuccess('Provider saved.');
            backToList();
        } catch (e) {
            fail(e);
        } finally {
            isSaving.value = false;
        }
    }

    // The form's submit: validates first, and only saves when everything is valid.
    function onSubmit() {
        submitted.value = true;
        serverErrors.value = {};
        if (!isValid.value) {
            logError('Please correct the validation errors first.');
            showTabWithError(Object.keys(clientErrors.value));
            return;
        }
        save();
    }

    async function remove() {
        try {
            await providersApi.remove(providerId.value);
            logSuccess('Provider deleted.');
            backToList();
        } catch (e) {
            // 409 explains itself ("still used by users, contracts, ...").
            dialog.value = '';
            logApiError(e);
        }
    }

    // ================================================================
    // Page load
    // ================================================================
    useActivate(async () => {
        isLoading.value = true;
        await getPageData();
        isLoading.value = false;
    });

    return {
        // Form
        form, info, states, counties, tab, dialog, title, isNew, sections, addressFields, contactFields,

        // Busy and validation state
        isLoading, isSaving, submitted, touched, touch, isValid, showError, msg, err,

        // User and permissions
        canEdit,

        // Actions
        save, onSubmit, remove, cancel
    };
}
