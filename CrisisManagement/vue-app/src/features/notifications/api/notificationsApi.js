import http from '../../../common/api/http.js';

// Thin axios wrappers; each resolves to the response body.
export const notificationsApi = {
    // criteria uses the shared search-grid names (orderBy/reverse); the API names them sortBy/sortDesc.
    async search(pageIndex, pageSize, criteria) {
        const { orderBy, reverse } = criteria;
        const { data } = await http.post('notifications/search', { pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },
    get: async (id) => (await http.get(`notifications/${id}`)).data,
    create: async (body) => (await http.post('notifications', body)).data,
    update: async (id, body) => (await http.put(`notifications/${id}`, body)).data,
    remove: async (id) => { await http.delete(`notifications/${id}`); }
};
