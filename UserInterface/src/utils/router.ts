/*
Vue Router 配置文件
TreeHole制作组
*/

import {createRouter, RouteRecordRaw, Router, createWebHistory} from 'vue-router'
import {isProgressVisible} from '../globals'
import i18n from './i18n'

const routes: Array<RouteRecordRaw> = [
    {
        path: '/',
        redirect: '/home'
    },
    {
        path: '/:pathMatch(.*)*',
        redirect: '/404'
    },
    {
        path: '/api',
        redirect: () => {
            window.location.href = '[TODO: APIRedirectUrl]'
            return '/'
        }
    },
    {
        path: '/home',
        name: 'HomePage',
        // @ts-ignore
        component: () => import('../views/HomePage.vue'),
        meta: {
            title: 'RouterTitle.HomePage'
        }
    },
    {
        path: '/login',
        name: 'LoginPage',
        // @ts-ignore
        component: () => import('../views/LoginPage.vue'),
        meta: {
            title: 'RouterTitle.LoginPage'
        }
    },
    {
        path: '/pet-community',
        name: 'PetCommunityPage',
        // @ts-ignore
        component: () => import('../views/PetCommunityPage.vue'),
        meta: {
            title: 'RouterTitle.PetCommunityPage'
        }
    },
    {
        path: '/profile/:id',
        name: 'ProfilePage',
        // @ts-ignore
        component: () => import('../views/ProfilePage.vue'),
        meta: {
            title: 'RouterTitle.ProfilePage'
        }
    },
    {
        path: '/register',
        name: 'RegisterPage',
        // @ts-ignore
        component: () => import('../views/RegisterPage.vue'),
        meta: {
            title: 'RouterTitle.RegisterPage'
        }
    },
]

const router: Router = createRouter({
    history: createWebHistory(),
    routes
})

router.beforeEach((to, _, next) => {
    if (to.meta && to.meta.title) {
        document.title = i18n.global.t(to.meta.title as string)
    } else {
        document.title = 'PetJoy'
    }
    isProgressVisible.value = true
    setTimeout(() => {
        isProgressVisible.value = false
    }, 1480)
    next()
})

export default router
