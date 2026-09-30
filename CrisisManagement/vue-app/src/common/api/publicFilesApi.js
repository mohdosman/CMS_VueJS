import http from './http.js';

export const publicFilesApi = {
    helpFiles: async () => (await http.get('publicfiles/help')).data,
    // A plain link the browser follows: the server answers with an attachment.
    downloadUrl: (id) => `${http.defaults.baseURL}/publicfiles/${id}/download`
};
