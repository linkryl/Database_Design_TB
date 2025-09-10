/*
TreeHole 认证工具函数
用于管理用户登录状态和用户ID
2351270 王天一
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

