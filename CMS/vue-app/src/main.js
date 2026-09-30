import { createApp } from 'vue';
import './assets/css/validation.css';
import App from './App.vue';
import DetailPanel from './common/components/DetailPanel.vue';
import AppButton from './common/components/AppButton.vue';
import SearchPanel from './common/components/SearchPanel.vue';
import SearchPaging from './common/components/SearchPaging.vue';
import SortHeader from './common/components/SortHeader.vue';
import { boot } from './boot.js';
import { createAppRouter } from './router/index.js';

const router = createAppRouter(boot.routes ?? []);

// Announce SPA navigation to assistive tech: update the title and reset focus to the app root.
router.afterEach((to) => {
    document.title = to.meta?.title ? `${to.meta.title} - ${boot.applicationName}` : boot.applicationName;
    document.getElementById('vue-app')?.focus();
});

createApp(App)
    .use(router)
    // Page building blocks nearly every screen uses.
    .component('AppButton', AppButton)
    .component('DetailPanel', DetailPanel)
    .component('SearchPanel', SearchPanel)
    .component('SearchPaging', SearchPaging)
    .component('SortHeader', SortHeader)
    .mount('#vue-app');
