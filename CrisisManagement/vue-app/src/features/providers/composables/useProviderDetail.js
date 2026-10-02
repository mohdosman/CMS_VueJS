import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { providersApi } from '../api/providersApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { fieldMessages } from '../../../utils/formErrors.js';

// Port of ManageProvider.aspx: one form for a new provider (/admin/providers/0) and an existing one (/admin/providers/:key).
export function useProviderDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logApiError } = useLogger();

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
    const errors = ref({});          // { 'physicalAddress.city': [messages] } from a 400 validation response
    const dialog = ref('');          // '' or 'delete'
    const tab = ref('demographics'); // 'demographics', 'physicalAddress' or 'remitAddress'
    const isLoading = ref(true);
    const isSaving = ref(false);

    const msg = fieldMessages(errors);
    const err = (field) => errors.value[field]?.length ?? 0;

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
        errors.value = fieldErrors;

        // Show the first tab that has a problem.
        const keys = Object.keys(fieldErrors);
        const inAddress = (key) => key.startsWith('physicalAddress') || key.startsWith('remitAddress');
        const firstAddressKey = keys.find(inAddress);
        tab.value = keys.some((key) => !inAddress(key)) ? 'demographics' : firstAddressKey.split('.')[0];
    }

    async function save() {
        errors.value = {};
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
        isLoading, isSaving, errors, msg, err,

        // User and permissions
        canEdit,

        // Actions
        save, remove, cancel
    };
}
