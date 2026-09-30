import { ref, reactive, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { providersApi } from '../api/providersApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';

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

// One form for both create (/admin/providers/0) and edit (/admin/providers/:key, the provider id).
export function useProviderDetail() {
    const route = useRoute();
    const router = useRouter();
    const { can } = useCapabilities();
    const { logSuccess, logApiError } = useLogger();

    const isNew = route.params.key === '0';
    const canEdit = can('providers.edit');

    const form = reactive(blankForm());
    const providerId = ref(0);
    const info = ref(null);
    const states = ref([]);
    const counties = ref([]);
    const errors = ref({});          // { 'physicalAddress.city': [messages] } from a 400 validation response
    const formError = ref('');       // page-level message: a load failure
    const dialog = ref('');          // '' or 'delete'
    const isLoading = ref(true);
    const isSaving = ref(false);

    const title = computed(() => (isNew ? 'Add Provider' : 'Provider Details'));

    function fill(detail) {
        providerId.value = detail.id;
        info.value = detail;
        Object.assign(form, blankForm(), {
            rowVersion: detail.rowVersion, name: detail.name ?? '', abbreviation: detail.abbreviation ?? '',
            edisonNumber: detail.edisonNumber ?? '', npi: detail.npi ?? '',
            physicalAddress: address(detail.physicalAddress), remitAddress: address(detail.remitAddress)
        });
    }

    // Field problems (400) show next to their inputs; anything else (403, 409 conflict, ...) is a toast.
    function fail(e) {
        const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
        if (fieldErrors) errors.value = fieldErrors;
        else logApiError(e);
    }

    async function save() {
        errors.value = {};
        isSaving.value = true;
        try {
            const detail = isNew ? await providersApi.create(form) : await providersApi.update(providerId.value, form);
            logSuccess('Provider saved.');
            if (isNew) {
                router.replace(`/admin/providers/${detail.id}`);
                return;
            }
            fill(detail);
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
            router.push('/admin/providers');
        } catch (e) {
            // 409 explains itself ("still used by users, contracts, ...").
            dialog.value = '';
            logApiError(e);
        }
    }

    const cancel = () => router.push('/admin/providers');

    onMounted(async () => {
        try {
            const lookups = await providersApi.lookups();
            states.value = lookups.states;
            counties.value = lookups.counties;
            if (!isNew) fill(await providersApi.get(route.params.key));
        } catch (e) {
            // Nothing to edit: keep the reason on the page.
            formError.value = e.response?.status === 404 ? 'Provider not found.' : apiErrorMessage(e);
        } finally {
            isLoading.value = false;
        }
    });

    return { isNew, canEdit, title, form, info, states, counties, errors, formError, dialog, isLoading, isSaving, save, remove, cancel };
}
