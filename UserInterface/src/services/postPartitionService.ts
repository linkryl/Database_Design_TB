/*
帖子分区服务 - 不修改数据库的分区实现
TreeHole开发组
*/

import { getLatestPostIds, getPostById } from '@/api/index'
import { 
  isVisibleInTreehole, 
  isVisibleInBar, 
  decodePostPartition,
  debugPostPartition 
} from '@/utils/postPartitionFixed'

// 获取树洞可见的帖子ID列表
export async function getTreeholePostIds(count: number = 50): Promise<number[]> {
  try {
    // 获取更多帖子来进行过滤
    const allPostIds = await getLatestPostIds(count * 2) // 获取2倍数量用于过滤
    
    if (!allPostIds || allPostIds.length === 0) {
      return []
    }

    // 批量获取帖子信息来检查分区
    const postPromises = allPostIds.slice(0, Math.min(100, allPostIds.length)).map(async (postId: number) => {
      try {
        const post = await getPostById(postId)
        return {
          postId,
          title: post.title || post.Title || '',
          post
        }
      } catch (error) {
        console.warn(`获取帖子${postId}失败:`, error)
        return null
      }
    })

    const posts = await Promise.all(postPromises)
    const validPosts = posts.filter(p => p !== null)

    // 过滤树洞可见的帖子
    const treeholeVisiblePosts = validPosts.filter(post => {
      const isVisible = isVisibleInTreehole(post!.title)
      if (process.env.NODE_ENV === 'development') {
        debugPostPartition(post!.title)
      }
      return isVisible
    })

    // 返回前N个帖子ID
    return treeholeVisiblePosts.slice(0, count).map(post => post!.postId)
    
  } catch (error) {
    console.error('获取树洞帖子失败:', error)
    return []
  }
}

// 获取指定贴吧的帖子ID列表
export async function getBarPostIds(barId: number, count: number = 20): Promise<number[]> {
  try {
    // 获取更多帖子来进行过滤
    const allPostIds = await getLatestPostIds(count * 3) // 获取3倍数量用于过滤
    
    if (!allPostIds || allPostIds.length === 0) {
      return []
    }

    // 批量获取帖子信息来检查分区
    const postPromises = allPostIds.slice(0, Math.min(150, allPostIds.length)).map(async (postId: number) => {
      try {
        const post = await getPostById(postId)
        return {
          postId,
          title: post.title || post.Title || '',
          post
        }
      } catch (error) {
        console.warn(`获取帖子${postId}失败:`, error)
        return null
      }
    })

    const posts = await Promise.all(postPromises)
    const validPosts = posts.filter(p => p !== null)

    // 过滤属于指定贴吧的帖子
    const barPosts = validPosts.filter(post => {
      const isVisible = isVisibleInBar(post!.title, barId)
      if (process.env.NODE_ENV === 'development' && isVisible) {
        console.log(`🏠 贴吧${barId}的帖子:`, post!.title)
        debugPostPartition(post!.title)
      }
      return isVisible
    })

    // 返回前N个帖子ID
    return barPosts.slice(0, count).map(post => post!.postId)
    
  } catch (error) {
    console.error(`获取贴吧${barId}帖子失败:`, error)
    return []
  }
}

// 获取帖子的完整分区信息
export async function getPostPartitionInfo(postId: number) {
  try {
    const post = await getPostById(postId)
    const rawTitle = post.title || post.Title || ''
    
    const { originalTitle, partitionInfo } = decodePostPartition(rawTitle)
    
    return {
      postId,
      originalTitle,
      displayTitle: originalTitle,
      partitionInfo,
      sourceInfo: {
        isFromBar: partitionInfo.type !== 'treehole',
        barId: partitionInfo.barId,
        barName: partitionInfo.barName,
        isCrossPost: partitionInfo.type === 'bar-cross'
      }
    }
  } catch (error) {
    console.error(`获取帖子${postId}分区信息失败:`, error)
    return null
  }
}

// 批量获取帖子分区信息
export async function getBatchPostPartitionInfo(postIds: number[]) {
  const promises = postIds.map(postId => getPostPartitionInfo(postId))
  const results = await Promise.allSettled(promises)
  
  return results
    .filter((result): result is PromiseFulfilledResult<any> => 
      result.status === 'fulfilled' && result.value !== null
    )
    .map(result => result.value)
}

// 调试：显示所有帖子的分区信息
export async function debugAllPostPartitions(limit: number = 20) {
  console.log('🔍 开始调试帖子分区信息...')
  
  try {
    const allPostIds = await getLatestPostIds(limit)
    const partitionInfos = await getBatchPostPartitionInfo(allPostIds)
    
    console.table(partitionInfos.map(info => ({
      帖子ID: info.postId,
      显示标题: info.displayTitle,
      分区类型: info.partitionInfo.type,
      贴吧ID: info.partitionInfo.barId || 'N/A',
      贴吧名称: info.partitionInfo.barName || 'N/A',
      树洞可见: info.partitionInfo.isTreeholeVisible ? '是' : '否',
      贴吧专属: info.partitionInfo.isBarExclusive ? '是' : '否'
    })))
  } catch (error) {
    console.error('调试分区信息失败:', error)
  }
}
