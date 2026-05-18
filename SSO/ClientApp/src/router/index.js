import { createWebHistory, createRouter } from "vue-router";
import { canActivate } from '@/services/auth-guard.service';
import Cookies from 'js-cookie';

const routes = [
    {
        path: "/",
        name: "System",
        component: () => import('@/pages/System.vue'),
    },
    {
        path: "/404",
        name: "404",
        component: () => import('@/pages/404.vue'),
    },
    {
        path: "/login",
        name: "Login",
        component: () => import('@/pages/Login.vue'),
    },
    {
        path: "/forgotpassword",
        name: "ForgotPassword",
        component: () => import('@/pages/ForgotPassword.vue'),
    },
    {
        path: "/resetpassword",
        name: "ResetPassword",
        component: () => import('@/pages/ResetPassword.vue'),
    },
    {
        path: '/logout',
        name: "Logout",
        beforeEnter(to, from, next) {
            Cookies.remove("appId");
            Cookies.remove("token");
            Cookies.remove("system");

            let baseUrl = process.env.VUE_APP_API_URL ?? '';
            if (to.query.applicationId && to.query.callbackUrl) {
                window.location.href = `${baseUrl}/api/authentication/logout?applicationId=${to.query.applicationId}&callbackurl=${to.query.callbackUrl}`;
            }
            else {
                window.location.href = `${baseUrl}/api/authentication/logout`;
            }
        }
    },
    {
        path: "/init",
        name: "Init",
        component: () => import('@/pages/Init.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate())
        }
    },
    {
        path: "/changepassword",
        name: "ChangePassword",
        component: () => import('@/pages/ChangePassword.vue'),
    },
    {
        path: "/main",
        name: "Main",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Main.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/applications",
        name: "Applications",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Applications/Manage.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/applications/new",
        name: "New Application",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Applications/New.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/applications/edit/:id",
        name: "Update Application",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Applications/Edit.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/applications/copy/:id",
        name: "Copy Application",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Applications/Copy.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/users",
        name: "Users",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Users/Manage.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/users/new",
        name: "New User",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Users/New.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/users/edit/:id",
        name: "Update User",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Users/Edit.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/users/copy/:id",
        name: "Copy User",
        meta: { mustHaveRealmAccess: true, idp: 'Default' },
        component: () => import('@/pages/Users/Copy.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/groups",
        name: "Groups",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Groups/Manage.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/groups/new",
        name: "New Group",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Groups/New.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/groups/edit/:id",
        name: "Update Group",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Groups/Edit.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/settings",
        name: "Settings",
        meta: { mustHaveRealmAccess: true },
        component: () => import('@/pages/Settings/Main.vue'),
        beforeEnter: (to, from, next) => {
            next(canActivate(to))
        }
    },
    {
        path: "/:pathMatch(.*)*",
        redirect: "/404"
    },
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;