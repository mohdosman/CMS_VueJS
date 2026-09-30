import http from '../../../common/api/http.js';

// Thin axios wrappers; each resolves to the response body.
export const suicidesApi = {
    // criteria uses the shared search-grid names (orderBy/reverse); the API names them sortBy/sortDesc.
    async searchFiles(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        const { data } = await http.post('suicides/files/search', { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },
    imports: async (id, pageIndex, pageSize, sortBy, sortDesc) => (await http.post(`suicides/files/${id}/imports`, { pageIndex, pageSize, sortBy, sortDesc })).data,
    // A plain link: the browser sends the sign-in cookie and saves the file.
    downloadUrl: (id) => `${http.defaults.baseURL}/suicides/files/${id}/download`,
    uploadFile: async (file) => {
        const body = new FormData();
        body.append('file', file);
        return (await http.post('suicides/files', body)).data;
    }
};
