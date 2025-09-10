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
          <img :src="githubLogoUrl" :alt="userInfo?.username || '用户'" />
        </div>
        <div class="user-details">
          <div class="username">{{ userInfo?.username || '未知用户' }}</div>
          <div class="post-time">{{ formatTime(postInfo?.createdAt) }}</div>
        </div>
      </div>
      <div class="post-category" v-if="categoryInfo">
        <span class="category-tag">{{ categoryInfo.category }}</span>
      </div>
    </div>

    <!-- 帖子内容 -->
    <div class="post-content">
      <h3 class="post-title">{{ postInfo?.title || '无标题' }}</h3>
      <div class="post-text" :class="{ 
        expanded: isContentExpanded,
        'has-expand-button': shouldShowExpandButton 
      }">
        {{ postInfo?.content || '暂无内容' }}
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
import githubLogo from '../assets/LogosAndIcons/GitHubLogo.png'

// Props
const props = defineProps<{
  postId: number
}>()

// 响应式数据
const loading = ref(true)
const postInfo = ref(null)
const userInfo = ref(null)
const categoryInfo = ref(null)
const githubLogoUrl = githubLogo
const isContentExpanded = ref(false)

// 计算属性
const formatTime = (timestamp) => {
  if (!timestamp) return '未知时间'
  const date = new Date(timestamp)
  const now = new Date()
  const diff = now - date
  
  if (diff < 60000) return '刚刚'
  if (diff < 3600000) return `${Math.floor(diff / 60000)}分钟前`
  if (diff < 86400000) return `${Math.floor(diff / 3600000)}小时前`
  if (diff < 2592000000) return `${Math.floor(diff / 86400000)}天前`
  
  return date.toLocaleDateString('zh-CN')
}

// 判断是否需要显示展开按钮
const shouldShowExpandButton = computed(() => {
  if (!postInfo.value?.content) return false
  // 如果内容超过200字符，显示展开按钮
  return postInfo.value.content.length > 200
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
    console.log('帖子信息:', postInfo.value)
    
    // 获取用户信息
    if (postInfo.value?.userId) {
      const userResponse = await axiosInstance.get(`user/${postInfo.value.userId}`)
      userInfo.value = userResponse.data
      console.log('用户信息:', userInfo.value)
    }
    
    // 获取分类信息
    if (postInfo.value?.categoryId) {
      const categoryResponse = await axiosInstance.get(`post-category/${postInfo.value.categoryId}`)
      categoryInfo.value = categoryResponse.data
      console.log('分类信息:', categoryInfo.value)
    }
    
  } catch (error) {
    console.error('获取帖子详情失败:', error)
    ElMessage.error('获取帖子详情失败')
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
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid #e8e8e8;
  transition: box-shadow 0.3s ease;
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
  font-size: 18px;
  font-weight: 600;
  color: #333;
  margin: 0 0 12px 0;
  line-height: 1.4;
}

.post-text {
  color: #666;
  line-height: 1.6;
  font-size: 14px;
  margin-bottom: 16px;
  white-space: pre-wrap;
  word-break: break-word;
  max-height: 200px;
  overflow: hidden;
  position: relative;
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
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  color: #4a90e2;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  padding: 8px 16px;
  margin-top: 12px;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: all 0.2s ease;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.expand-button:hover {
  background: #e3f2fd;
  border-color: #4a90e2;
  color: #357abd;
  transform: translateY(-1px);
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
}

.expand-button:active {
  transform: translateY(0);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.ellipsis-hint {
  color: #999;
  font-weight: bold;
  font-size: 16px;
  margin-left: 4px;
}

.long-post-hint {
  margin-top: 16px;
  padding: 12px 16px;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border: 1px solid #dee2e6;
  border-radius: 8px;
  border-left: 4px solid #4a90e2;
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
