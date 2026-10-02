import http from '../../../common/api/http.js';

// Thin axios wrappers; each resolves to the response body.
export const servicesApi = {
    providers: async () => (await http.get('services/providers')).data,
    serviceCodes: async () => (await http.get('services/service-codes')).data,
    entryProviders: async () => (await http.get('services/entry-providers')).data,
    // criteria uses the shared search-grid names (orderBy/reverse); the API names them sortBy/sortDesc.
    async search(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        const { data } = await http.post('services/search', { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },
    async searchCurrentSession(pageIndex, pageSize, { orderBy, reverse }) {
        const { data } = await http.post('services/search-current-session', { pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },

    lookups: async () => (await http.get('services/lookups')).data,
    get: async (id) => (await http.get(`services/${id}`)).data,
    existingPatients: async (providerId, providerPatientNo) =>
        (await http.get('services/existing-patient', { params: { providerId, providerPatientNo } })).data,
    create: async (model) => (await http.post('services', model)).data,
    update: async (id, model) => (await http.put(`services/${id}`, model)).data,
    remove: async (id) => { await http.delete(`services/${id}`); },

    fileProviders: async () => (await http.get('services/files/providers')).data,
    async searchFiles(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        const { data } = await http.post('services/files/search', { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },
    fileRaw: async (id) => (await http.get(`services/files/${id}/raw`)).data,
    fileErrors: async (id, pageIndex, pageSize) => (await http.post(`services/files/${id}/errors`, { pageIndex, pageSize })).data,
    uploadFile: async (file) => {
        const body = new FormData();
        body.append('file', file);
        return (await http.post('services/files', body)).data;
    }
};
