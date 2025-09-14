/*
Vue Router 配置文件
2352031 古振
*/

import {createRouter, RouteRecordRaw, Router, createWebHistory} from 'vue-router'
import {isProgressVisible} from '../globals'

const routes: Array<RouteRecordRaw> = [
    {
        path: '/',
        redirect: '/home'
    },
    {
        path: '/home',
        name: 'HomePage',
        // @ts-ignore
        component: () => import('../views/HomePage.vue'),
        meta: {
            title: '首页'
        }
    },
    {
        path: '/login',
        name: 'LoginPage',
        // @ts-ignore
        component: () => import('../views/LoginPage.vue'),
        meta: {
            title: '登陆页面'
        }
    },
    {
        path: '/CommunityPage',
        name: 'CommunityPage',
        // @ts-ignore
        component: () => import('../views/CommunityPage.vue'),
        meta: {
            title: '树洞社区'
        }
    },
    {
        path: '/profile/:id',
        name: 'ProfilePage',
        // @ts-ignore
        component: () => import('../views/ProfilePage.vue'),
        meta: {
            title: '个人主页'
        }
    },
    {
        path: '/register',
        name: 'RegisterPage',
        // @ts-ignore
        component: () => import('../views/RegisterPage.vue'),
        meta: {
            title: '注册页面'
        }
    },
    {
        path: '/PostNew',
        name: 'PostEdit',
        // @ts-ignore
        component: () => import('../views/PostEdit.vue'),
        meta: {
            title: '发表树洞',
            requiresAuth: true
        }
    },
    {
        path: '/bars',
        name: 'BarListPage',
        // @ts-ignore
        component: () => import('../views/BarListPage.vue'),
        meta: {
            title: '贴吧广场'
        }
    },
    {
        path: '/bar/:id',
        name: 'BarDetailPage',
        // @ts-ignore
        component: () => import('../views/BarDetailPage.vue'),
        meta: {
            title: '贴吧详情'
        }
    },
    {
        path: '/bar/:id/edit',
        name: 'BarEditPage',
        // @ts-ignore
        component: () => import('../views/BarEditPage.vue'),
        meta: {
            title: '编辑贴吧',
            requiresAuth: true
        }
    },
]

const router: Router = createRouter({
    history: createWebHistory(),
    routes
})

router.beforeEach((to, _, next) => {
    if (to.meta && to.meta.title) {
        document.title = to.meta.title as string
    } else {
        document.title = 'TreeHole'
    }
    isProgressVisible.value = true
    setTimeout(() => {
        isProgressVisible.value = false
    }, 1480)
    next()
})

export default router
