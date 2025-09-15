/*
TreeHole API接口封装 - 解决Git冲突后的干净版本
2351270 王天一
*/

import axios from 'axios'

// TreeHole API 基础配置
const thApiBaseUrl = (import.meta as any).env?.VITE_API_BASE || '/api'

const thApiClient = axios.create({
  baseURL: thApiBaseUrl,
  timeout: 15000, // 增加超时时间到15秒
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

// ==================== 数据接口定义 ====================

/**
 * 创建新帖子请求数据接口
 */
export interface THCreatePostRequest {
  userId: number
  categoryId: number
  barId?: number | null // 贴吧ID，可选
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
  alsoInTreehole?: number // 是否同时在树洞显示：1=是，0=否
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
 * 帖子点赞数据接口
 */
export interface THPostLike {
  postId: number
  userId: number
  likeTime: string
}

/**
 * 帖子点踩数据接口
 */
export interface THPostDislike {
  postId: number
  userId: number
  dislikeTime: string
}

/**
 * 帖子收藏数据接口
 */
export interface THPostFavorite {
  postId: number
  userId: number
  favoriteTime: string
}

/**
 * 帖子评论数据接口
 */
export interface THPostComment {
  commentId?: number
  postId: number
  userId: number
  content: string
  commentTime: string
  parentCommentId?: number | null
}

/**
 * 评论点赞数据接口
 */
export interface THCommentLike {
  commentId: number
  userId: number
  likeTime: string
}

/**
 * 评论点踩数据接口
 */
export interface THCommentDislike {
  commentId: number
  userId: number
  dislikeTime: string
}

/**
 * 帖子举报数据接口
 */
export interface THPostReport {
  postId: number
  userId: number
  reportReason: string
  reportTime: string
}

/**
 * 评论举报数据接口
 */
export interface THCommentReport {
  commentId: number
  userId: number
  reportReason: string
  reportTime: string
}

/**
 * 用户个人信息更新接口
 */
export interface THPersonalInfoUpdate {
  profile: string
  gender: string
  birthdate: string
}

/**
 * 用户头像更新接口
 */
export interface THAvatarUpdate {
  avatarUrl: string
}

/**
 * 用户密码更新接口
 */
export interface THPasswordUpdate {
  plainPassword: string
}

/**
 * 贴吧数据接口
 */
export interface THBar {
  barId?: number
  ownerId: number
  barName: string
  description?: string
  avatarUrl?: string
  coverUrl?: string
  creationDate?: string
  updateDate?: string
  status: number // 0=正常，1=归档，2=已解散
  followedCount: number
  postCount: number
  rules?: string
  tags?: string
}

/**
 * 贴吧关注数据接口
 */
export interface THBarFollow {
  barId: number
  userId: number
  followTime: string
  isActive: number
}

// ==================== 帖子相关接口 ====================

/**
 * 创建新帖子
 */
export const createPost = async (data: THCreatePostRequest): Promise<THCreatePostResponse> => {
  const response = await thApiClient.post('/post', data)
  return response.data
}

/**
 * 获取帖子详情
 */
export const getPostById = async (postId: number) => {
  const response = await thApiClient.get(`/post/${postId}`)
  return response.data
}

/**
 * 获取最新帖子ID列表
 */
export const getLatestPostIds = async (count: number = 10) => {
  const response = await thApiClient.get(`/post/latest-ids?count=${count}`)
  return response.data
}

/**
 * 获取树洞社区的帖子ID列表
 */
export const getTreeholePostIds = async (count: number = 20) => {
  const response = await thApiClient.get(`/post/treehole-ids?count=${count}`)
  return response.data
}

/**
 * 获取指定贴吧的帖子ID列表
 */
export const getBarPostIds = async (barId: number, count: number = 20) => {
  const response = await thApiClient.get(`/post/bar/${barId}/post-ids?count=${count}`)
  return response.data
}

// ==================== 点赞相关接口 ====================

/**
 * 点赞帖子
 */
export const likePost = async (data: THPostLike) => {
  const response = await thApiClient.post('/post-like', data)
  return response.data
}

/**
 * 取消点赞帖子
 */
export const unlikePost = async (postId: number, userId: number) => {
  const response = await thApiClient.delete(`/post-like/${postId}-${userId}`)
  return response.data
}

/**
 * 检查是否已点赞帖子
 */
export const checkPostLiked = async (postId: number, userId: number) => {
  try {
    const response = await thApiClient.get(`/post-like/${postId}-${userId}`)
    return response.status === 200
  } catch (error: any) {
    if (error.response?.status === 404) {
      return false
    }
    throw error
  }
}

// ==================== 点踩相关接口 ====================

/**
 * 点踩帖子
 */
export const dislikePost = async (data: THPostDislike) => {
  const response = await thApiClient.post('/post-dislike', data)
  return response.data
}

/**
 * 取消点踩帖子
 */
export const undislikePost = async (postId: number, userId: number) => {
  const response = await thApiClient.delete(`/post-dislike/${postId}-${userId}`)
  return response.data
}

/**
 * 检查是否已点踩帖子
 */
export const checkPostDisliked = async (postId: number, userId: number) => {
  try {
    const response = await thApiClient.get(`/post-dislike/${postId}-${userId}`)
    return response.status === 200
  } catch (error: any) {
    if (error.response?.status === 404) {
      return false
    }
    throw error
  }
}

// ==================== 收藏相关接口 ====================

/**
 * 收藏帖子
 */
export const favoritePost = async (data: THPostFavorite) => {
  const response = await thApiClient.post('/post-favorite', data)
  return response.data
}

/**
 * 取消收藏帖子
 */
export const unfavoritePost = async (postId: number, userId: number) => {
  const response = await thApiClient.delete(`/post-favorite/${postId}-${userId}`)
  return response.data
}

/**
 * 检查是否已收藏帖子
 */
export const checkPostFavorited = async (postId: number, userId: number) => {
  try {
    const response = await thApiClient.get(`/post-favorite/${postId}-${userId}`)
    return response.status === 200
  } catch (error: any) {
    if (error.response?.status === 404) {
      return false
    }
    throw error
  }
}

/**
 * 获取用户收藏的帖子ID列表
 */
export const getUserFavoritePostIds = async (userId: number) => {
  const response = await thApiClient.get(`/post-favorite/user/${userId}`)
  return response.data
}

// ==================== 评论相关接口 ====================

/**
 * 发表评论
 */
export const createComment = async (data: THPostComment) => {
  const response = await thApiClient.post('/post-comment', data)
  return response.data
}

/**
 * 获取帖子的所有评论
 */
export const getCommentsByPostId = async (postId: number) => {
  const response = await thApiClient.get(`/post-comment/post-${postId}`)
  return response.data
}

/**
 * 删除评论
 */
export const deleteComment = async (commentId: number) => {
  const response = await thApiClient.delete(`/post-comment/${commentId}`)
  return response.data
}

// ==================== 评论点赞相关接口 ====================

/**
 * 点赞评论
 */
export const likeComment = async (data: THCommentLike) => {
  const response = await thApiClient.post('/post-comment-like', data)
  return response.data
}

/**
 * 取消点赞评论
 */
export const unlikeComment = async (commentId: number, userId: number) => {
  const response = await thApiClient.delete(`/post-comment-like/${commentId}-${userId}`)
  return response.data
}

/**
 * 检查是否已点赞评论
 */
export const checkCommentLiked = async (commentId: number, userId: number) => {
  try {
    const response = await thApiClient.get(`/post-comment-like/${commentId}-${userId}`)
    return response.status === 200
  } catch (error: any) {
    if (error.response?.status === 404) {
      return false
    }
    throw error
  }
}

// ==================== 评论点踩相关接口 ====================

/**
 * 点踩评论
 */
export const dislikeComment = async (data: THCommentDislike) => {
  const response = await thApiClient.post('/post-comment-dislike', data)
  return response.data
}

/**
 * 取消点踩评论
 */
export const undislikeComment = async (commentId: number, userId: number) => {
  const response = await thApiClient.delete(`/post-comment-dislike/${commentId}-${userId}`)
  return response.data
}

/**
 * 检查是否已点踩评论
 */
export const checkCommentDisliked = async (commentId: number, userId: number) => {
  try {
    const response = await thApiClient.get(`/post-comment-dislike/${commentId}-${userId}`)
    return response.status === 200
  } catch (error: any) {
    if (error.response?.status === 404) {
      return false
    }
    throw error
  }
}

// ==================== 举报相关接口 ====================

/**
 * 举报帖子
 */
export const reportPost = async (data: THPostReport) => {
  const response = await thApiClient.post('/post-report', data)
  return response.data
}

/**
 * 举报评论
 */
export const reportComment = async (data: THCommentReport) => {
  const response = await thApiClient.post('/post-comment-report', data)
  return response.data
}

// ==================== 用户相关接口 ====================

/**
 * 获取用户信息
 */
export const getUserById = async (userId: number) => {
  const response = await thApiClient.get(`/user/${userId}`)
  return response.data
}

/**
 * 更新用户个人信息
 */
export const updateUserPersonalInfo = async (userId: number, data: THPersonalInfoUpdate) => {
  const response = await thApiClient.put(`/user/personal-information/${userId}`, data)
  return response.data
}

/**
 * 更新用户头像
 */
export const updateUserAvatar = async (userId: number, data: THAvatarUpdate) => {
  const response = await thApiClient.put(`/user/avatar-url/${userId}`, data)
  return response.data
}

/**
 * 更新用户密码
 */
export const updateUserPassword = async (userId: number, data: THPasswordUpdate) => {
  const response = await thApiClient.put(`/user/password/${userId}`, data)
  return response.data
}

/**
 * 获取帖子分类
 */
export const getPostCategoryById = async (categoryId: number) => {
  const response = await thApiClient.get(`/post-category/${categoryId}`)
  return response.data
}

/**
 * 获取所有帖子分类
 */
export const getAllPostCategories = async () => {
  const response = await thApiClient.get('/post-category')
  return response.data
}

// ==================== 贴吧相关接口 ====================

/**
 * 获取所有贴吧
 */
export const getAllBars = async (page: number = 1, pageSize: number = 20) => {
  const response = await thApiClient.get('/bar', {
    params: { page, pageSize }
  })
  return response.data
}

/**
 * 获取热门贴吧
 */
export const getPopularBars = async (count: number = 10) => {
  const response = await thApiClient.get('/bar/popular', {
    params: { count }
  })
  return response.data
}

/**
 * 根据ID获取贴吧详情
 */
export const getBarById = async (barId: number) => {
  const response = await thApiClient.get(`/bar/${barId}`)
  return response.data
}

/**
 * 根据吧主ID获取贴吧列表
 */
export const getBarsByOwnerId = async (ownerId: number) => {
  const response = await thApiClient.get(`/bar/owner/${ownerId}`)
  return response.data
}

/**
 * 创建贴吧
 */
export const createBar = async (data: THBar) => {
  const response = await thApiClient.post('/bar', data)
  return response.data
}

/**
 * 更新贴吧信息
 */
export const updateBar = async (barId: number, data: THBar) => {
  const response = await thApiClient.put(`/bar/${barId}`, data)
  return response.data
}

/**
 * 更新贴吧状态
 */
export const updateBarStatus = async (barId: number, status: number) => {
  const response = await thApiClient.put(`/bar/${barId}/status`, { status })
  return response.data
}

/**
 * 搜索贴吧
 */
export const searchBars = async (keyword: string) => {
  const response = await thApiClient.get('/bar/search', {
    params: { keyword }
  })
  return response.data
}

// ==================== 贴吧关注相关接口 ====================

/**
 * 关注贴吧
 */
export const followBar = async (data: THBarFollow) => {
  const response = await thApiClient.post('/bar-follow', data)
  return response.data
}

/**
 * 取消关注贴吧
 */
export const unfollowBar = async (barId: number, userId: number) => {
  const response = await thApiClient.delete(`/bar-follow/${barId}/${userId}`)
  return response.data
}

/**
 * 检查贴吧关注状态
 */
export const checkBarFollowStatus = async (barId: number, userId: number) => {
  try {
    const response = await thApiClient.get(`/bar-follow/${barId}/${userId}`)
    return response.status === 200
  } catch (error: any) {
    if (error.response?.status === 404) {
      return false
    }
    throw error
  }
}

/**
 * 获取用户关注的贴吧列表
 */
export const getUserFollowedBars = async (userId: number) => {
  const response = await thApiClient.get(`/bar-follow/user/${userId}`)
  return response.data
}

/**
 * 获取贴吧的粉丝列表
 */
export const getBarFollowers = async (barId: number, page: number = 1, pageSize: number = 20) => {
  const response = await thApiClient.get(`/bar-follow/bar/${barId}/followers`, {
    params: { page, pageSize }
  })
  return response.data
}

/**
 * 获取用户关注的贴吧ID列表
 */
export const getUserFollowedBarIds = async (userId: number) => {
  const response = await thApiClient.get(`/bar-follow/user/${userId}/bar-ids`)
  return response.data
}

export default thApiClient
