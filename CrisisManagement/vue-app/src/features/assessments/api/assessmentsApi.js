import http from '../../../common/api/http.js';

// Thin axios wrappers; each resolves to the response body.
export const assessmentsApi = {
    providers: async () => (await http.get('assessments/providers')).data,
    // criteria uses the shared search-grid names (orderBy/reverse); the API names them sortBy/sortDesc.
    async search(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        const { data } = await http.post('assessments/search', { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },

    lookups: async () => (await http.get('assessments/lookups')).data,
    get: async (key) => (await http.get(`assessments/detail/${key}`)).data,
    create: async (model) => (await http.post('assessments', model)).data,
    update: async (key, model) => (await http.put(`assessments/${key}`, model)).data,
    remove: async (key) => { await http.delete(`assessments/${key}`); },

    fileProviders: async () => (await http.get('assessments/files/providers')).data,
    async searchFiles(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        const { data } = await http.post('assessments/files/search', { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },
    fileRaw: async (id) => (await http.get(`assessments/files/${id}/raw`)).data,
    fileErrors: async (id) => (await http.get(`assessments/files/${id}/errors`)).data,
    uploadFile: async (file) => {
        const body = new FormData();
        body.append('file', file);
        return (await http.post('assessments/files', body)).data;
    }
};
