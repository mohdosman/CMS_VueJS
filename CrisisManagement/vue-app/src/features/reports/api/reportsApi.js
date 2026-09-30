import http from '../../../common/api/http.js';

// Thin axios wrappers; each resolves to the response body.
export const reportsApi = {
    // criteria uses the shared search-grid names (orderBy/reverse); the API names them sortBy/sortDesc.
    async search(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        const { data } = await http.post('reports/search', { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },
    available: async () => (await http.get('reports/available')).data,
    get: async (id) => (await http.get(`reports/${id}`)).data,
    create: async (model) => (await http.post('reports', model)).data,
    update: async (id, model) => (await http.put(`reports/${id}`, model)).data,
    remove: async (id) => { await http.delete(`reports/${id}`); },
    // Signs a token and logs the run; the answer is the link the report window opens.
    run: async (reportKey) => (await http.post(`reports/${reportKey}/run`)).data
};
