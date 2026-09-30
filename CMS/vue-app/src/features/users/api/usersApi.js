import { api } from '../../../api/http.js';

export const usersApi = {
    // criteria uses the shared search-grid names (orderBy/reverse); the API names them sortBy/sortDesc.
    search(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        return api('users/search', { method: 'POST', body: { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse } });
    },
    get: (key) => api(`users/${key}`),
    roles: () => api('users/roles'),
    providers: () => api('users/providers'),
    policy: () => api('users/policy'),
    create: (body) => api('users', { method: 'POST', body }),
    update: (key, body) => api(`users/${key}`, { method: 'PUT', body }),
    setPassword: (key, body) => api(`users/${key}/password`, { method: 'POST', body }),
    remove: (key) => api(`users/${key}`, { method: 'DELETE' })
};
