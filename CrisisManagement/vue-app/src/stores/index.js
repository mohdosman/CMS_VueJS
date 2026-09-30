import { defineStore } from 'pinia';
import { ref } from 'vue';

// Holds the server bootstrap data (current user + permissions, the permitted menu routes, url
// globals) read once from the host page. Same role as the SafetyNet app store.
export const useAppStore = defineStore('app', () => {
    const applicationName = ref('');
    const currentUser = ref(null);
    const routes = ref([]);
    const globals = ref({});

    function hydrate(bootstrap) {
        applicationName.value = bootstrap.applicationName;
        currentUser.value = bootstrap.currentUser;
        routes.value = bootstrap.routes;
        globals.value = bootstrap.globals;
    }

    return { applicationName, currentUser, routes, globals, hydrate };
});
