/*
TreeHole API接口封装
TreeHole制作组
*/

import axios from 'axios'

// TreeHole API 基础配置
const thApiBaseUrl = import.meta.env.VITE_API_BASE || '/api'

const thApiClient = axios.create({
  baseURL: thApiBaseUrl,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// TreeHole 请求拦截器 - 添加认证token等
thApiClient.interceptors.request.use(
  (config) => {
    // TODO: 后续对接JWT token时在此处添加Authorization头
    const token = localStorage.getItem('jwtToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    console.log('TreeHole API Request:', config.method?.toUpperCase(), config.url, config.data)
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// TreeHole 响应拦截器 - 统一处理错误
thApiClient.interceptors.response.use(
  (response) => {
    console.log('TreeHole API Response:', response.status, response.data)
    return response
  },
  (error) => {
    console.error('TreeHole API Error:', error.response?.data || error.message)
    return Promise.reject(error)
  }
)

// TreeHole 帖子相关API接口

/**
 * 创建新帖子请求数据接口
 */
export interface THCreatePostRequest {
  title: string
  content: string
}

/**
 * 创建新帖子响应数据接口
 */
export interface THCreatePostResponse {
  id: number
  title: string
  content: string
  userId: number
  createdAt: string
  // TODO: 根据实际后端接口调整返回字段
}

/**
 * 创建新帖子
 * @param data 帖子数据
 * @returns Promise<THCreatePostResponse>
 */
export const createPost = async (data: THCreatePostRequest): Promise<THCreatePostResponse> => {
  const response = await thApiClient.post('/posts', data)
  return response.data
}

// TODO: 后续添加其他TreeHole相关接口
// export const getThPosts = async () => { ... }
// export const getThPostById = async (id: number) => { ... }  
// export const updateThPost = async (id: number, data: any) => { ... }
// export const deleteThPost = async (id: number) => { ... }

export default thApiClient
