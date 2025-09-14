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
            // 清除本地存储的认证信息并跳转
            localStorage.removeItem('jwtToken')
            localStorage.removeItem('currentUserId')
            localStorage.removeItem('userRole')
            localStorage.removeItem('isAdmin')
            
            // 使用 window.location 进行页面跳转，避免 router 未初始化的问题
            window.location.href = '/login'
        }
        return Promise.reject(error)
    }
)

export default axiosInstance
