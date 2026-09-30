import http from '../../../common/api/http.js';

// The Public Files screen. Listing the help documents for the Help dialog and downloading one are in common/api/publicFilesApi.js.
export const publicFilesAdminApi = {
    // criteria uses the shared search-grid names (orderBy/reverse); the API names them sortBy/sortDesc.
    async search(pageIndex, pageSize, criteria) {
        const { orderBy, reverse, ...filters } = criteria;
        const { data } = await http.post('publicfiles/search', { ...filters, pageIndex, pageSize, sortBy: orderBy, sortDesc: reverse });
        return data;
    },
    upload: async (file) => {
        const body = new FormData();
        body.append('file', file);
        return (await http.post('publicfiles', body)).data;
    },
    remove: async (id) => { await http.delete(`publicfiles/${id}`); }
};
