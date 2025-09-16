/*
帖子分区工具类 - 使用标题编码的分区方案
TreeHole开发组
*/

// 分区标识符
export const PARTITION_MARKERS = {
  TREEHOLE: '[TH]',
  BAR_ONLY: '[BO]',
  BAR_CROSS: '[BC]'
} as const

// 帖子分区类型
export type PostPartitionType = 'treehole' | 'bar-only' | 'bar-cross'

// 帖子分区信息
export interface PostPartitionInfo {
  type: PostPartitionType
  barId?: number
  barName?: string
  isTreeholeVisible: boolean
  isBarExclusive: boolean
}

// Base64编码（浏览器安全）
function encodeBase64(str: string): string {
  try {
    return btoa(unescape(encodeURIComponent(str)))
  } catch {
    return str
  }
}

// Base64解码（浏览器安全）
function decodeBase64(str: string): string {
  try {
    return decodeURIComponent(escape(atob(str)))
  } catch {
    return str
  }
}

// 编码帖子分区信息到标题
export function encodePostPartition(
  originalTitle: string,
  partitionType: PostPartitionType,
  barId?: number,
  barName?: string
): string {
  let marker = ''
  
  switch (partitionType) {
    case 'treehole':
      marker = PARTITION_MARKERS.TREEHOLE
      break
    case 'bar-only':
    case 'bar-cross':
      // 创建分区数据对象
      const partitionData = {
        type: partitionType,
        barId: barId || 0,
        barName: barName || 'Unknown'
      }
      
      // 使用Base64编码避免任何字符冲突
      const encodedData = encodeBase64(JSON.stringify(partitionData))
      marker = partitionType === 'bar-only' 
        ? `${PARTITION_MARKERS.BAR_ONLY}${encodedData}|`
        : `${PARTITION_MARKERS.BAR_CROSS}${encodedData}|`
      break
  }
  
  return `${marker}${originalTitle}`
}

// 从标题解码分区信息
export function decodePostPartition(encodedTitle: string): {
  originalTitle: string
  partitionInfo: PostPartitionInfo
} {
  // 默认分区信息（树洞原创）
  let partitionInfo: PostPartitionInfo = {
    type: 'treehole',
    isTreeholeVisible: true,
    isBarExclusive: false
  }
  
  let originalTitle = encodedTitle
  
  try {
    // 检查树洞标识
    if (encodedTitle.startsWith(PARTITION_MARKERS.TREEHOLE)) {
      originalTitle = encodedTitle.substring(PARTITION_MARKERS.TREEHOLE.length)
      partitionInfo = {
        type: 'treehole',
        isTreeholeVisible: true,
        isBarExclusive: false
      }
    }
    // 检查贴吧专属标识
    else if (encodedTitle.startsWith(PARTITION_MARKERS.BAR_ONLY)) {
      const markerEnd = encodedTitle.indexOf('|', PARTITION_MARKERS.BAR_ONLY.length)
      if (markerEnd > 0) {
        const encodedData = encodedTitle.substring(PARTITION_MARKERS.BAR_ONLY.length, markerEnd)
        const decodedData = decodeBase64(encodedData)
        const barData = JSON.parse(decodedData)
        originalTitle = encodedTitle.substring(markerEnd + 1)
        
        partitionInfo = {
          type: 'bar-only',
          barId: barData.barId || barData.id,
          barName: barData.barName || barData.name,
          isTreeholeVisible: false,
          isBarExclusive: true
        }
      }
    }
    // 检查贴吧跨发布标识
    else if (encodedTitle.startsWith(PARTITION_MARKERS.BAR_CROSS)) {
      const markerEnd = encodedTitle.indexOf('|', PARTITION_MARKERS.BAR_CROSS.length)
      if (markerEnd > 0) {
        const encodedData = encodedTitle.substring(PARTITION_MARKERS.BAR_CROSS.length, markerEnd)
        const decodedData = decodeBase64(encodedData)
        const barData = JSON.parse(decodedData)
        originalTitle = encodedTitle.substring(markerEnd + 1)
        
        partitionInfo = {
          type: 'bar-cross',
          barId: barData.barId || barData.id,
          barName: barData.barName || barData.name,
          isTreeholeVisible: true,
          isBarExclusive: false
        }
      }
    }
  } catch (error) {
    console.warn('解码帖子分区信息失败，使用默认值:', error)
    console.log('问题标题:', encodedTitle)
    // 如果解码失败，保持原标题和默认分区信息
  }
  
  return {
    originalTitle,
    partitionInfo
  }
}

// 检查帖子是否在树洞可见
export function isVisibleInTreehole(encodedTitle: string): boolean {
  const { partitionInfo } = decodePostPartition(encodedTitle)
  return partitionInfo.isTreeholeVisible
}

// 检查帖子是否属于指定贴吧
export function isVisibleInBar(encodedTitle: string, barId: number): boolean {
  const { partitionInfo } = decodePostPartition(encodedTitle)
  return partitionInfo.barId === barId
}

// 获取帖子的贴吧信息
export function getPostBarInfo(encodedTitle: string): { barId?: number, barName?: string } | null {
  const { partitionInfo } = decodePostPartition(encodedTitle)
  
  if (partitionInfo.barId && partitionInfo.barName) {
    return {
      barId: partitionInfo.barId,
      barName: partitionInfo.barName
    }
  }
  
  return null
}

// 过滤帖子列表 - 树洞可见的帖子
export function filterTreeholePosts(posts: any[]): any[] {
  return posts.filter(post => {
    const title = post.title || post.Title || ''
    return isVisibleInTreehole(title)
  })
}

// 过滤帖子列表 - 特定贴吧的帖子
export function filterBarPosts(posts: any[], barId: number): any[] {
  return posts.filter(post => {
    const title = post.title || post.Title || ''
    return isVisibleInBar(title, barId)
  })
}

// 处理帖子显示标题（移除分区标识）
export function getDisplayTitle(encodedTitle: string): string {
  const { originalTitle } = decodePostPartition(encodedTitle)
  return originalTitle
}

// 获取帖子来源信息（用于显示标签）
export function getPostSourceInfo(encodedTitle: string): {
  isFromBar: boolean
  barId?: number
  barName?: string
  isCrossPost: boolean
} {
  const { partitionInfo } = decodePostPartition(encodedTitle)
  
  return {
    isFromBar: partitionInfo.type !== 'treehole',
    barId: partitionInfo.barId,
    barName: partitionInfo.barName,
    isCrossPost: partitionInfo.type === 'bar-cross'
  }
}

// 生成帖子分区的调试信息
export function debugPostPartition(encodedTitle: string): void {
  const { originalTitle, partitionInfo } = decodePostPartition(encodedTitle)
  
  console.log('🔍 帖子分区调试信息:', {
    原始标题: originalTitle,
    编码标题: encodedTitle,
    分区类型: partitionInfo.type,
    贴吧ID: partitionInfo.barId,
    贴吧名称: partitionInfo.barName,
    树洞可见: partitionInfo.isTreeholeVisible,
    贴吧专属: partitionInfo.isBarExclusive
  })
}

// 测试编码解码功能
export function testPartitionEncoding() {
  console.log('🧪 测试帖子分区编码...')
  
  const testCases = [
    { title: '普通树洞帖子', type: 'treehole' as PostPartitionType },
    { title: '王天一吧的专属帖子', type: 'bar-only' as PostPartitionType, barId: 11, barName: '王天一吧' },
    { title: '技术讨论:跨发布帖子', type: 'bar-cross' as PostPartitionType, barId: 5, barName: '技术讨论' }
  ]
  
  testCases.forEach(testCase => {
    const encoded = encodePostPartition(testCase.title, testCase.type, testCase.barId, testCase.barName)
    const decoded = decodePostPartition(encoded)
    
    console.log('测试用例:', testCase.title)
    console.log('编码后:', encoded)
    console.log('解码后:', decoded.originalTitle)
    console.log('分区信息:', decoded.partitionInfo)
    console.log('---')
  })
}
