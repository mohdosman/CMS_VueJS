import http from '../../../common/api/http.js';

// Thin axios wrappers; each resolves to the response body.
export const rolesApi = {
    // criteria uses the shared search-grid names (orderBy/reverse); the API names them sortBy/sortDesc.
    async search(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        const { data } = await http.post('roles/search', { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },
    get: async (id) => (await http.get(`roles/${id}`)).data,
    permissions: async () => (await http.get('roles/permissions')).data,
    create: async (body) => (await http.post('roles', body)).data,
    update: async (id, body) => (await http.put(`roles/${id}`, body)).data,
    remove: async (id) => { await http.delete(`roles/${id}`); }
};
