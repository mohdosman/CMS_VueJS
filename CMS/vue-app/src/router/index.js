import { createRouter, createWebHashHistory } from 'vue-router';
import Home from '../views/Home.vue';
import ComingSoon from '../views/ComingSoon.vue';

// Views are picked by the last url segment, case-insensitive (same convention as SafetyNet):
//   /admin/users -> UsersSearch.vue (or Users.vue), detail /admin/users/:key -> UsersDetails.vue
// Menu urls with no matching view fall back to ComingSoon until the screen is ported.
const views = {
    ...import.meta.glob('../views/*.vue', { eager: true }),
    ...import.meta.glob('../features/*/views/*.vue', { eager: true })
};
const byName = Object.fromEntries(
    Object.entries(views).map(([path, m]) => [path.split('/').pop().replace('.vue', '').toLowerCase(), m.default]));

export function createAppRouter(permittedRoutes) {
    const routes = permittedRoutes
        .filter((r) => r.url !== '/')
        .flatMap((r) => {
            const seg = r.url.split('/').pop().toLowerCase();
            const list = [{
                path: r.url,
                component: byName[`${seg}search`] ?? byName[seg] ?? ComingSoon,
                meta: { title: r.name }
            }];
            // A detail view is only reachable through its permitted list url.
            if (byName[`${seg}details`])
                list.push({ path: `${r.url}/:key`, component: byName[`${seg}details`], meta: { title: `${r.name} - Details` } });
            return list;
        });

    return createRouter({
        history: createWebHashHistory(),
        routes: [
            { path: '/', component: Home, meta: { title: 'Home' } },
            ...routes,
            // Only urls the server permitted are registered; anything else goes home.
            { path: '/:pathMatch(.*)*', redirect: '/' }
        ]
    });
}
