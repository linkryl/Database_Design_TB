/*
 * Project Name:  UserInterface
 * File Name:     router.ts
 * File Function: Vue Router 配置文件
 * Author:        宠悦 | PetJoy 项目开发组
 * Update Date:   2024-09-07
 * License:       Creative Commons Attribution 4.0 International License
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
        path: '/about-petjoy',
        name: 'AboutPetJoyPage',
        // @ts-ignore
        component: () => import('../views/AboutPetJoyPage.vue'),
        meta: {
            title: 'RouterTitle.AboutPetJoyPage'
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
        path: '/post/:id',
        name: 'PostPage',
        // @ts-ignore
        component: () => import('../views/PostPage.vue'),
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
