/*
TreeHole API接口封装
2351270 王天一
*/

import axios from 'axios'

// TreeHole API 基础配置
const thApiBaseUrl = (import.meta as any).env?.VITE_API_BASE || '/api'

const thApiClient = axios.create({
  baseURL: thApiBaseUrl,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// TreeHole 请求拦截器
thApiClient.interceptors.request.use(
  (config) => {
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

// TreeHole 响应拦截器
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

/**
 * 创建新帖子请求数据接口
 */
export interface THCreatePostRequest {
  userId: number
  categoryId: number
  title: string
  content: string
  creationDate: string
  updateDate: string
  isSticky: number
  likeCount: number
  dislikeCount: number
  favoriteCount: number
  commentCount: number
  imageUrl: string | null
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
}

/**
 * 创建新帖子
 * @param data 帖子数据
 * @returns Promise<THCreatePostResponse>
 */
export const createPost = async (data: THCreatePostRequest): Promise<THCreatePostResponse> => {
  const response = await thApiClient.post('/post', data)
  return response.data
}

export default thApiClient
