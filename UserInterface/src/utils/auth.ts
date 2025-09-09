/*
TreeHole 认证工具函数
用于管理用户登录状态和用户ID
TreeHole制作组
*/

const TH_USER_ID_KEY = 'currentUserId'

/**
 * 获取当前用户ID
 * @returns 用户ID字符串，未登录则返回null
 */
export const getCurrentUserId = (): string | null => {
  try {
    const thUserId = localStorage.getItem(TH_USER_ID_KEY)
    return thUserId || null
  } catch (error) {
    console.error('TreeHole: 获取用户ID失败:', error)
    return null
  }
}

/**
 * 设置当前用户ID（登录时调用）
 * @param userId 用户ID
 */
export const setCurrentUserId = (userId: string): void => {
  try {
    localStorage.setItem(TH_USER_ID_KEY, userId)
    console.log('TreeHole: 用户ID已设置:', userId)
  } catch (error) {
    console.error('TreeHole: 设置用户ID失败:', error)
  }
}

/**
 * 清除认证信息（登出时调用）
 */
export const clearAuth = (): void => {
  try {
    localStorage.removeItem(TH_USER_ID_KEY)
    // 同时清除JWT token
    localStorage.removeItem('jwtToken')
    console.log('TreeHole: 认证信息已清除')
  } catch (error) {
    console.error('TreeHole: 清除认证信息失败:', error)
  }
}

/**
 * 检查是否已登录
 * @returns 是否已登录
 */
export const isLoggedIn = (): boolean => {
  return getCurrentUserId() !== null
}

// TODO: 后续对接JWT token时，可添加以下函数：
// export const getAuthToken = (): string | null => { ... }
// export const setAuthToken = (token: string): void => { ... }
// export const clearAuthToken = (): void => { ... }
