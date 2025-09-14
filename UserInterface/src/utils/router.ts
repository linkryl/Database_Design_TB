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
    // 贴吧相关路由
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
    // 管理员相关路由
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

        path: '/admin-login',
        name: 'AdminLoginPage',
        // @ts-ignore
        component: () => import('../views/AdminLoginPage.vue'),
        meta: {
            title: '管理员登录'
        }
    },
    {
        path: '/user-management',
        name: 'UserManagementPage',
        // @ts-ignore
        component: () => import('../views/UserManagementPage.vue'),
        meta: {
            title: '用户管理',
            requiresAuth: true,
            requiresAdmin: true
        }
    },
    // 帖子详情页
    {
        path: '/PostPage/:id',
        name: 'PostPage',
        // @ts-ignore
        component: () => import('../views/PostPage.vue'),
        meta: {
            title: '帖子详情'
        }
    },
    // 404页面路由
    {
        path: '/404',
        name: 'NotFound',
        // @ts-ignore
        component: () => import('../views/NotFoundPage.vue'),
        meta: {
            title: '页面不存在'
        }
    },
    // 通配符路由，捕获所有未匹配的路由
    {
        path: '/:pathMatch(.*)*',
        redirect: '/404'
    }
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
    
    // 检查是否需要登录
    if (to.meta.requiresAuth) {
        const token = localStorage.getItem('jwtToken')
        const userId = localStorage.getItem('currentUserId')
        
        if (!token || !userId || userId === '0') {
            next('/login')
            return
        }
    }
    
    // 检查是否需要管理员权限
    if (to.meta.requiresAdmin) {
        const userRole = localStorage.getItem('userRole')
        const isAdmin = localStorage.getItem('isAdmin')
        
        if (userRole !== '1' || isAdmin !== 'true') {
            next('/CommunityPage')
            return
        }
    }
    
    isProgressVisible.value = true
    setTimeout(() => {
        isProgressVisible.value = false
    }, 1480)
    next()
})

export default router
