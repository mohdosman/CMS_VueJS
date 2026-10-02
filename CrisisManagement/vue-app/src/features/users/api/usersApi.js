import http from '../../../common/api/http.js';

// Thin axios wrappers; each resolves to the response body.
export const usersApi = {
    // criteria uses the shared search-grid names (orderBy/reverse); the API names them sortBy/sortDesc.
    async search(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        const { data } = await http.post('users/search', { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },
    get: async (key) => (await http.get(`users/${key}`)).data,
    roles: async () => (await http.get('users/roles')).data,
    providers: async () => (await http.get('users/providers')).data,
    policy: async () => (await http.get('users/policy')).data,
    create: async (body) => (await http.post('users', body)).data,
    update: async (key, body) => (await http.put(`users/${key}`, body)).data,
    setPassword: async (key, body) => { await http.post(`users/${key}/password`, body); },
    resetMfa: async (key) => { await http.post(`users/${key}/mfa/reset`); },
    documents: async (key) => (await http.get(`users/${key}/documents`)).data,
    uploadDocument: async (key, file) => {
        const body = new FormData();
        body.append('file', file);
        return (await http.post(`users/${key}/documents`, body)).data;
    },
    removeDocument: async (key, id) => { await http.delete(`users/${key}/documents/${id}`); },
    // A plain link the browser follows: the server answers with an attachment.
    documentUrl: (key, id) => `${http.defaults.baseURL}/users/${key}/documents/${id}/download`
};
