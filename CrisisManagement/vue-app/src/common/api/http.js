import axios from 'axios';
import { useLoadingStore } from '../../stores/useLoadingStore.js';
import { getApiBaseUrl, getBaseUrl } from '../../utils/urlUtils.js';
import { readServerBootstrap } from '../../bootstrap/bootstrap.js';

// Shared axios instance (same shape as SafetyNet). baseURL is the MVC Web API root.
// X-XSRF-TOKEN: the API rejects POST/PUT/DELETE without the antiforgery token. It is rendered into
// the boot data with the page, so a full reload (e.g. after signing in again) picks up a fresh one.
const http = axios.create({
    baseURL: getApiBaseUrl(),
    withCredentials: true,
    headers: { 'X-XSRF-TOKEN': readServerBootstrap().globals?.antiforgeryToken ?? '' }
});

const LOADING_TRACKED = Symbol('loadingTracked');

http.interceptors.request.use(
    config => {
        if (config?.showGlobalLoading !== false) {
            useLoadingStore().show();
            config[LOADING_TRACKED] = true;
        }
        return config;
    },
    error => Promise.reject(error)
);

http.interceptors.response.use(
    response => {
        if (response.config[LOADING_TRACKED]) useLoadingStore().hide();
        return response;
    },
    error => {
        if (error.config?.[LOADING_TRACKED]) useLoadingStore().hide();
        // Session expired: back to the MVC login page.
        if (error.response?.status === 401) {
            window.location.href = `${getBaseUrl()}/Account/Login`;
        }
        return Promise.reject(error);
    }
);

export default http;
