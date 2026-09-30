import { createRouter, createWebHashHistory } from 'vue-router';
import Home from '../features/home/views/Home.vue';
import ComingSoon from '../views/ComingSoon.vue';
import Profile from '../features/profile/views/Profile.vue';

// Views are picked by the last url segment, case-insensitive (same convention as SafetyNet):
//   /admin/users -> UsersSearch.vue (or Users.vue), detail /admin/users/:key -> UsersDetails.vue
// When several screens share a last segment (/assessments/files, /services/files, /suicides/files) the whole path
// wins: /assessments/files -> AssessmentsFiles.vue. Menu urls with no matching view fall back to ComingSoon.
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
            const parts = r.url.split('/').filter(Boolean).map((x) => x.toLowerCase());
            const seg = parts[parts.length - 1];
            const full = parts.join('');
            const pick = (suffix) => byName[`${full}${suffix}`] ?? byName[`${seg}${suffix}`];
            const list = [{
                path: r.url,
                component: pick('search') ?? pick('') ?? ComingSoon,
                meta: { title: r.name }
            }];
            // A detail view is only reachable through its permitted list url.
            if (pick('details'))
                list.push({ path: `${r.url}/:key`, component: pick('details'), meta: { title: `${r.name} - Details` } });
            return list;
        });

    return createRouter({
        history: createWebHashHistory(),
        routes: [
            { path: '/', component: Home, meta: { title: 'Home' } },
            // Not a menu item: opened from the user menu.
            { path: '/profile', component: Profile, meta: { title: 'My Profile' } },
            ...routes,
            // Only urls the server permitted are registered; anything else goes home.
            { path: '/:pathMatch(.*)*', redirect: '/' }
        ]
    });
}
