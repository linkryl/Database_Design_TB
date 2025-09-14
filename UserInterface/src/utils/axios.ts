/*
Vue Axios 配置文件
TreeHole制作组
*/

import axios from 'axios'
import {apiBaseUrl} from '../globals'
// import {useRouter} from 'vue-router' // 移除，避免在setup外调用

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
            // 移除localStorage中的用户信息并跳转到登录页
            localStorage.removeItem('currentUserId')
            window.location.href = '/login'
        }
        return Promise.reject(error)
    }
)

export default axiosInstance
