import http from '../../../common/api/http.js';

// Thin axios wrappers; each resolves to the response body.
export const menusApi = {
    list: async () => (await http.get('menus')).data,
    get: async (id) => (await http.get(`menus/${id}`)).data,
    parents: async (excludeId) => (await http.get('menus/parents', { params: excludeId ? { excludeId } : {} })).data,
    create: async (body) => (await http.post('menus', body)).data,
    update: async (id, body) => (await http.put(`menus/${id}`, body)).data,
    remove: async (id) => { await http.delete(`menus/${id}`); },

    permissions: async (menuId) => (await http.get(`menus/${menuId}/permissions`)).data,
    createPermission: async (body) => (await http.post('menus/permissions', body)).data,
    updatePermission: async (id, body) => (await http.put(`menus/permissions/${id}`, body)).data,
    removePermission: async (id) => { await http.delete(`menus/permissions/${id}`); },

    groups: async () => (await http.get('menus/groups')).data,
    createGroup: async (body) => (await http.post('menus/groups', body)).data,
    updateGroup: async (id, body) => (await http.put(`menus/groups/${id}`, body)).data
};
