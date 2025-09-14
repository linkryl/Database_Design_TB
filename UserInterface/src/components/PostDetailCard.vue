<!--
帖子详情卡片组件 - 在社区页面完整展示帖子信息
2351134 吕奎辰
-->



<template>
  <div class="post-detail-card" v-loading="loading">
    <!-- 帖子头部信息 -->
    <div class="post-header">
      <div class="user-info">
        <div class="user-avatar">
          <img :src="githubLogoUrl" :alt="userInfo?.UserName || userInfo?.userName || userInfo?.USER_NAME || '用户'" />
        </div>
        <div class="user-details">
          <div class="username">{{ userInfo?.UserName || userInfo?.userName || userInfo?.USER_NAME || '未知用户' }}</div>
          <div class="post-time">{{ formatTime(postInfo?.CreationDate || postInfo?.creationDate || postInfo?.CREATION_DATE) }}</div>
        </div>
      </div>
      <div class="post-category">
        <span class="category-tag">{{ getRandomCategory() }}</span>
      </div>
    </div>

    <!-- 帖子内容 -->
    <div class="post-content">
      <h3 class="post-title">{{ postInfo?.Title || postInfo?.title || postInfo?.TITLE || '无标题' }}</h3>
      <div class="post-text" :class="{ 
        expanded: isContentExpanded,
        'has-expand-button': shouldShowExpandButton 
      }">
        {{ postInfo?.Content || postInfo?.content || postInfo?.CONTENT || '暂无内容' }}
        <span v-if="shouldShowExpandButton && !isContentExpanded" class="ellipsis-hint">...</span>
      </div>
      <button 
        v-if="shouldShowExpandButton" 
        class="expand-button" 
        @click="toggleContentExpansion"
      >
        <span v-if="!isContentExpanded">📖 展开阅读全文</span>
        <span v-else>📄 收起</span>
      </button>
      <!-- 举报按钮 -->
      <button @click="showReport = true" class="report-btn">举报</button>
      <ReportPage v-if="showReport" @close="showReport = false" :postId="postInfo?.PostId || postInfo?.postId" />
    </div>
    
    <!-- 长帖提示条 -->
    <div v-if="shouldShowExpandButton && !isContentExpanded" class="long-post-hint">
      <div class="hint-content">
        <span class="hint-icon">📝</span>
        <span class="hint-text">这是一篇长帖，点击上方按钮查看完整内容</span>
      </div>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { ref, onMounted, computed } from 'vue'
import axiosInstance from '../utils/axios'
import { ElMessage } from 'element-plus'
import githubLogo from '/images/GitHubLogo.png'
import ReportPage from '@/views/ReportPage.vue'

const showReport = ref(false)
// Props
const props = defineProps<{
  postId: number
}>()

// 响应式数据
const loading = ref(true)
const postInfo = ref(null)
const userInfo = ref(null)
const githubLogoUrl = githubLogo
const isContentExpanded = ref(false)

// 校园树洞分类列表 - 简化分类
const campusCategories = [
  '闲聊', '奇思妙想', '日常吐槽', '分享交流'
]

// 根据帖子ID生成随机分类（确保同一帖子总是显示相同分类）
const getRandomCategory = () => {
  if (!props.postId) return '闲聊'
  
  // 使用帖子ID作为种子，确保同一帖子总是显示相同分类
  const seed = props.postId
  const index = seed % campusCategories.length
  return campusCategories[index]
}

// 计算属性
const formatTime = (timestamp) => {
  if (!timestamp) return '未知时间'
  
  // 解析原始时间
  let date
  if (typeof timestamp === 'string') {
    date = new Date(timestamp)
  } else {
    date = new Date(timestamp)
  }
  
  // 检查日期是否有效
  if (isNaN(date.getTime())) {
    return '时间格式错误'
  }
  
  // 手动加8小时调整时区
  const adjustedDate = new Date(date.getTime() + 8 * 60 * 60 * 1000)
  
  const now = new Date()
  const diff = now - adjustedDate
  
  // 如果调整后的时间超过当前时间，显示"刚刚"
  if (diff < 0) {
    return '刚刚'
  }
  
  if (diff < 60000) return '刚刚'
  if (diff < 3600000) return `${Math.floor(diff / 60000)}分钟前`
  if (diff < 86400000) return `${Math.floor(diff / 3600000)}小时前`
  if (diff < 2592000000) return `${Math.floor(diff / 86400000)}天前`
  
  return adjustedDate.toLocaleDateString('zh-CN')
}

// 判断是否需要显示展开按钮
const shouldShowExpandButton = computed(() => {
  const content = postInfo.value?.Content || postInfo.value?.content || postInfo.value?.CONTENT
  if (!content) return false
  // 如果内容超过200字符，显示展开按钮
  return content.length > 200
})

// 切换内容展开状态
const toggleContentExpansion = () => {
  isContentExpanded.value = !isContentExpanded.value
}



// 获取帖子详情
const fetchPostDetail = async () => {
  try {
    loading.value = true
    console.log(`正在获取帖子详情: ${props.postId}`)
    
    // 获取帖子信息
    const postResponse = await axiosInstance.get(`post/${props.postId}`)
    postInfo.value = postResponse.data
    console.log('原始帖子信息:', postResponse.data)
    console.log('帖子信息类型:', typeof postResponse.data)
    console.log('帖子信息键:', Object.keys(postResponse.data || {}))
    
    // 检查所有可能的字段名称
    console.log('字段检查:', {
      'PostId': postInfo.value?.PostId,
      'postId': postInfo.value?.postId,
      'POST_ID': postInfo.value?.POST_ID,
      'UserId': postInfo.value?.UserId,
      'userId': postInfo.value?.userId,
      'USER_ID': postInfo.value?.USER_ID,
      'CategoryId': postInfo.value?.CategoryId,
      'categoryId': postInfo.value?.categoryId,
      'CATEGORY_ID': postInfo.value?.CATEGORY_ID,
      'Title': postInfo.value?.Title,
      'title': postInfo.value?.title,
      'TITLE': postInfo.value?.TITLE,
      'Content': postInfo.value?.Content,
      'content': postInfo.value?.content,
      'CONTENT': postInfo.value?.CONTENT,
      'CreationDate': postInfo.value?.CreationDate,
      'creationDate': postInfo.value?.creationDate,
      'CREATION_DATE': postInfo.value?.CREATION_DATE
    })
    
    // 获取用户信息 - 尝试不同的字段名称
    const userId = postInfo.value?.UserId || postInfo.value?.userId || postInfo.value?.USER_ID
    if (userId) {
      console.log(`正在获取用户信息: ${userId}`)
      const userResponse = await axiosInstance.get(`user/${userId}`)
      userInfo.value = userResponse.data
      console.log('原始用户信息:', userResponse.data)
      console.log('用户信息键:', Object.keys(userResponse.data || {}))
    } else {
      console.warn('帖子中没有找到用户ID字段')
    }
    
    
  } catch (error) {
    console.error('获取帖子详情失败:', error)
    console.error('错误详情:', error.response?.data)
    console.error('错误状态码:', error.response?.status)
    
    // 如果是404错误，说明帖子不存在，显示占位内容
    if (error.response?.status === 404) {
      console.warn(`帖子ID ${props.postId} 不存在，显示占位内容`)
      postInfo.value = {
        PostId: props.postId,
        UserId: 0,
        CategoryId: 0,
        Title: '帖子不存在',
        Content: '抱歉，这个帖子可能已被删除或不存在。',
        CreationDate: new Date().toISOString()
      }
      userInfo.value = {
        UserId: 0,
        UserName: '未知用户'
      }
    } else {
      ElMessage.error('获取帖子详情失败')
    }
  } finally {
    loading.value = false
  }
}

// 组件挂载时获取数据
onMounted(() => {
  fetchPostDetail()
})
</script>

<style scoped>
.post-detail-card {
  background: white;
  border-radius: 12px;
  padding: 24px 32px;
  margin-bottom: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid #e8e8e8;
  transition: box-shadow 0.3s ease;
  width: 100%;
  max-width: 100%;
  min-width: 800px;
}

.post-detail-card:hover {
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

/* 帖子头部 */
.post-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #f0f0f0;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  overflow: hidden;
  flex-shrink: 0;
}

.user-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}


.user-details {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.username {
  font-weight: 600;
  color: #333;
  font-size: 14px;
}

.post-time {
  color: #999;
  font-size: 12px;
}

.post-category {
  flex-shrink: 0;
}

.category-tag {
  background: #e8f4fd;
  color: #4a90e2;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

/* 帖子内容 */
.post-content {
  margin-bottom: 16px;
}

.post-title {
  font-size: 20px;
  font-weight: 600;
  color: #333;
  margin: 0 0 16px 0;
  line-height: 1.5;
  word-break: break-word;
  max-width: 100%;
  padding-right: 20px;
}

.post-text {
  color: #666;
  line-height: 1.7;
  font-size: 15px;
  margin-bottom: 16px;
  white-space: pre-wrap;
  word-break: break-word;
  max-height: 240px;
  overflow: hidden;
  position: relative;
  text-align: justify;
  max-width: 100%;
  padding-right: 20px;
}

.post-text.expanded {
  max-height: none;
}

.post-text.has-expand-button::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 30px;
  background: linear-gradient(transparent, rgba(255, 255, 255, 0.8), white);
  pointer-events: none;
  opacity: 1;
  transition: opacity 0.3s ease;
}

.post-text.has-expand-button.expanded::after {
  opacity: 0;
}

.post-text.expanded::after {
  display: none;
}

.expand-button {
  background: linear-gradient(135deg, #4a90e2 0%, #357abd 100%);
  border: none;
  border-radius: 8px;
  color: white;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  padding: 10px 20px;
  margin-top: 16px;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s ease;
  box-shadow: 0 2px 6px rgba(74, 144, 226, 0.3);
}

.expand-button:hover {
  background: linear-gradient(135deg, #357abd 0%, #2c5aa0 100%);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.4);
}

.expand-button:active {
  transform: translateY(-1px);
  box-shadow: 0 2px 6px rgba(74, 144, 226, 0.3);
}

.ellipsis-hint {
  color: #999;
  font-weight: bold;
  font-size: 16px;
  margin-left: 4px;
}

.long-post-hint {
  margin-top: 16px;
  padding: 14px 18px;
  background: linear-gradient(135deg, #f0f7ff 0%, #e3f2fd 100%);
  border: 1px solid #bbdefb;
  border-radius: 10px;
  border-left: 5px solid #4a90e2;
  box-shadow: 0 2px 4px rgba(74, 144, 226, 0.1);
}

.hint-content {
  display: flex;
  align-items: center;
  gap: 8px;
}

.hint-icon {
  font-size: 16px;
}

.hint-text {
  color: #6c757d;
  font-size: 13px;
  font-weight: 500;
}


/* 响应式设计 */
@media (max-width: 768px) {
  .post-detail-card {
    padding: 16px;
    margin-bottom: 16px;
  }
  
  .post-title {
    font-size: 16px;
  }
}
</style>
