/*
Vue Axios 配置文件
TreeHole制作组
*/

import axios from 'axios'
import {apiBaseUrl} from '../globals'

const axiosInstance = axios.create({
    baseURL: apiBaseUrl
})

axiosInstance.interceptors.request.use(
    async config => {
        const token = localStorage.getItem('jwtToken')
        if (token) {
            config.headers.Authorization = `Bearer ${token}`
        }
        return config
    },
    error => {
        return Promise.reject(error)
    }
)

axiosInstance.interceptors.response.use(
    response => response, error => {
        if (error.response && error.response.status == 401) {
            // 检查是否是管理员删除帖子的请求
            const isAdminDeleteRequest = error.config?.url?.includes('/post/admin/')
            
            if (isAdminDeleteRequest) {
                // 对于管理员删除请求，不自动跳转，让组件自己处理
                console.log('管理员删除请求401错误，由组件处理')
            } else {
                // 对于其他请求，清除本地存储的认证信息并跳转
                localStorage.removeItem('jwtToken')
                localStorage.removeItem('currentUserId')
                localStorage.removeItem('userRole')
                localStorage.removeItem('isAdmin')
                
                // 使用 window.location 进行页面跳转，避免 router 未初始化的问题
                window.location.href = '/login'
            }
        }
        return Promise.reject(error)
    }
)

export default axiosInstance
