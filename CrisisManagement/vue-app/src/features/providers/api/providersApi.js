import http from '../../../common/api/http.js';

// Thin axios wrappers; each resolves to the response body.
export const providersApi = {
    // criteria uses the shared search-grid names (orderBy/reverse); the API names them sortBy/sortDesc.
    async search(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        const { data } = await http.post('providers/search', { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },
    lookups: async () => (await http.get('providers/lookups')).data,
    get: async (id) => (await http.get(`providers/${id}`)).data,
    create: async (body) => (await http.post('providers', body)).data,
    update: async (id, body) => (await http.put(`providers/${id}`, body)).data,
    remove: async (id) => { await http.delete(`providers/${id}`); }
};
