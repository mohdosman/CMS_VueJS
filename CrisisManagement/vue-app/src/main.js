import { createApp } from 'vue';
import { createPinia } from 'pinia';
import Toast from 'vue-toastification';
import 'vue-toastification/dist/index.css';
import './assets/css/validation.css';

import App from './App.vue';
import AppButton from './common/components/AppButton.vue';
import DetailPanel from './common/components/DetailPanel.vue';
import SearchPanel from './common/components/SearchPanel.vue';
import SearchPaging from './common/components/SearchPaging.vue';
import SortHeader from './common/components/SortHeader.vue';
import { createAppRouter } from './router/index.js';
import { readServerBootstrap } from './bootstrap/bootstrap.js';
import { useAppStore } from './stores/useAppStore.js';

const pinia = createPinia();
const app = createApp(App);
app.use(pinia);

// Hydrate the store from the server-injected boot blob, then register only the routes the server
// permitted for this user.
const bootstrapData = readServerBootstrap();
useAppStore().hydrate(bootstrapData);
const router = createAppRouter(bootstrapData.routes);

// Announce SPA navigation to assistive tech: update the title and reset focus to the app root.
router.afterEach((to) => {
    const name = useAppStore().applicationName;
    document.title = to.meta?.title ? `${to.meta.title} - ${name}` : name;
    document.getElementById('vue-app')?.focus();
});

app.use(router);

app.use(Toast, {
    timeout: 4000,
    container: document.body,
    position: 'top-right',
    // useLogger speaks every toast through the shared live region instead; leaving
    // role="alert" on as well makes screen readers read each message twice.
    accessibility: { toastRole: 'none' }
});

// Page building blocks nearly every screen uses.
app.component('AppButton', AppButton);
app.component('DetailPanel', DetailPanel);
app.component('SearchPanel', SearchPanel);
app.component('SearchPaging', SearchPaging);
app.component('SortHeader', SortHeader);

app.config.errorHandler = (err, _instance, info) => {
    console.error('[CMS] Vue error:', err, info);
};

app.mount('#vue-app');
