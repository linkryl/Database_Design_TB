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
          <img :src="githubLogoUrl" :alt="userInfo?.userName || '用户'" />
        </div>
        <div class="user-details">
          <div class="username">{{ userInfo?.userName || '未知用户' }}</div>
          <div class="post-time">{{ formatTime(postInfo?.creationDate) }}</div>
        </div>
      </div>
      <div class="post-category" v-if="categoryInfo && categoryInfo.category">
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

    <!-- 帖子互动区域 -->
    <div class="post-interactions">
      <div class="interaction-stats">
        <span class="stat-item">
          <span class="stat-icon">📅</span>
          <span class="stat-text">{{ formatTime(postInfo?.creationDate) }}</span>
        </span>
        <span class="stat-item">
          <span class="stat-icon">💬</span>
          <span class="stat-text">{{ postInfo?.commentCount || 0 }} 评论</span>
        </span>
      </div>

      <div class="interaction-buttons">
        <!-- 点赞按钮 -->
        <button 
          class="interaction-btn"
          :class="{ active: isLiked }"
          @click="toggleLike"
          :disabled="!currentUserId"
        >
          <span class="btn-icon">👍</span>
          <span class="btn-text">{{ postInfo?.likeCount || 0 }}</span>
        </button>

        <!-- 点踩按钮 -->
        <button 
          class="interaction-btn dislike-btn"
          :class="{ active: isDisliked }"
          @click="toggleDislike"
          :disabled="!currentUserId"
        >
          <span class="btn-icon">👎</span>
          <span class="btn-text">{{ postInfo?.dislikeCount || 0 }}</span>
        </button>

        <!-- 收藏按钮 -->
        <button 
          class="interaction-btn favorite-btn"
          :class="{ active: isFavorited }"
          @click="toggleFavorite"
          :disabled="!currentUserId"
        >
          <span class="btn-icon">{{ isFavorited ? '⭐' : '☆' }}</span>
          <span class="btn-text">{{ postInfo?.favoriteCount || 0 }}</span>
        </button>

        <!-- 举报按钮 -->
        <button 
          v-if="currentUserId && currentUserId !== postInfo?.userId"
          class="interaction-btn report-btn"
          @click="showReportDialog"
        >
          <span class="btn-icon">🚨</span>
          <span class="btn-text">举报</span>
        </button>
      </div>
    </div>

    <!-- 评论区域 -->
    <div class="comment-area">
      <CommentSection :post-id="postId" />
    </div>

    <!-- 举报对话框 -->
    <el-dialog
      v-model="reportDialogVisible"
      title="举报帖子"
      width="400px"
      :close-on-click-modal="false"
    >
      <div class="report-form">
        <p class="report-hint">请选择举报理由：</p>
        <el-radio-group v-model="reportReason">
          <el-radio value="spam">垃圾信息</el-radio>
          <el-radio value="inappropriate">内容不当</el-radio>
          <el-radio value="harassment">骚扰行为</el-radio>
          <el-radio value="fraud">虚假信息</el-radio>
          <el-radio value="copyright">版权侵犯</el-radio>
          <el-radio value="other">其他</el-radio>
        </el-radio-group>
        <el-input
          v-if="reportReason === 'other'"
          v-model="customReportReason"
          type="textarea"
          placeholder="请详细描述举报原因..."
          :rows="3"
          maxlength="200"
          show-word-limit
          style="margin-top: 10px;"
        />
      </div>
      <template #footer>
        <el-button @click="reportDialogVisible = false">取消</el-button>
        <el-button 
          type="primary" 
          @click="submitReport"
          :disabled="!reportReason || (reportReason === 'other' && !customReportReason.trim())"
        >
          提交举报
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang='ts'>
import { ref, onMounted, computed } from 'vue'
import { ElMessage } from 'element-plus'
import githubLogo from '../assets/LogosAndIcons/GitHubLogo.png'
import CommentSection from './CommentSection.vue'
import { getCurrentUserId } from '@/utils/auth'
import {
  getPostById,
  getUserById,
  getPostCategoryById,
  likePost,
  unlikePost,
  dislikePost,
  undislikePost,
  favoritePost,
  unfavoritePost,
  checkPostLiked,
  checkPostDisliked,
  checkPostFavorited,
  reportPost,
  type THPostLike,
  type THPostDislike,
  type THPostFavorite,
  type THPostReport
} from '@/api/index'

// Props
const props = defineProps<{
  postId: number
}>()

// 响应式数据
const loading = ref(true)
const postInfo = ref<any>(null)
const userInfo = ref<any>(null)
const categoryInfo = ref<any>(null)
const githubLogoUrl = githubLogo
const isContentExpanded = ref(false)

// 当前用户相关
const currentUserId = ref(getCurrentUserId() ? parseInt(getCurrentUserId()!) : null)

// 互动状态
const isLiked = ref(false)
const isDisliked = ref(false)
const isFavorited = ref(false)

// 举报相关
const reportDialogVisible = ref(false)
const reportReason = ref('')
const customReportReason = ref('')

// 时间格式化函数 - 修复时区问题
const formatTime = (timestamp: string) => {
  if (!timestamp) return '未知时间'
  
  try {
    // 解析时间戳
    let date = new Date(timestamp)
    
    // 检查后端返回的时间格式
    if (timestamp.includes('T')) {
      // ISO格式，可能是UTC时间
      // 如果没有时区标识符（Z或+），假设是UTC时间，需要转换为本地时间
      if (!timestamp.includes('Z') && !timestamp.includes('+') && !timestamp.includes('-', 10)) {
        // 后端返回的是UTC时间但没有Z标识，手动添加Z
        date = new Date(timestamp + 'Z')
      } else {
        // 有时区标识，直接解析
        date = new Date(timestamp)
      }
    } else {
      // 非ISO格式，假设是本地时间
      date = new Date(timestamp)
    }
    
    const now = new Date()
    
    // 计算时间差（毫秒）
    const diff = now.getTime() - date.getTime()
    
    // 调试信息（仅在开发环境）
    if (import.meta.env.DEV) {
      console.log('时间调试:', {
        原始: timestamp,
        解析: date.toLocaleString('zh-CN'),
        当前: now.toLocaleString('zh-CN'),
        差异小时: Math.round(diff / 3600000 * 10) / 10
      })
    }
    
    // 如果时间差为负且超过1小时，说明有问题
    if (diff < -3600000) {
      console.warn('时间异常，直接显示日期:', timestamp)
      return date.toLocaleString('zh-CN', {
        month: '2-digit',
        day: '2-digit', 
        hour: '2-digit',
        minute: '2-digit'
      })
    }
    
    // 使用绝对值计算相对时间
    const absDiff = Math.abs(diff)
    
    if (absDiff < 60000) return '刚刚'
    if (absDiff < 3600000) return `${Math.floor(absDiff / 60000)}分钟前`
    if (absDiff < 86400000) return `${Math.floor(absDiff / 3600000)}小时前`
    if (absDiff < 604800000) return `${Math.floor(absDiff / 86400000)}天前`
    
    // 超过一周，显示具体日期
    return date.toLocaleString('zh-CN', {
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch (error) {
    console.error('时间格式化错误:', error, timestamp)
    return '时间格式错误'
  }
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
    const postResponse = await getPostById(props.postId)
    postInfo.value = postResponse
    console.log('帖子信息:', postInfo.value)
    
    // 获取用户信息
    if (postInfo.value?.userId) {
      try {
        const userResponse = await getUserById(postInfo.value.userId)
        userInfo.value = userResponse
        console.log('用户信息:', userInfo.value)
      } catch (error) {
        console.error('获取用户信息失败:', error)
        userInfo.value = { userName: '未知用户' }
      }
    }
    
    // 获取分类信息
    if (postInfo.value?.categoryId) {
      try {
        const categoryResponse = await getPostCategoryById(postInfo.value.categoryId)
        categoryInfo.value = categoryResponse
        console.log('分类信息:', categoryInfo.value)
      } catch (error) {
        console.error('获取分类信息失败:', error)
        // 分类获取失败时，不显示分类标签
        categoryInfo.value = null
      }
    } else {
      // 没有分类ID时也不显示
      categoryInfo.value = null
    }
    
    // 如果用户已登录，检查互动状态
    if (currentUserId.value) {
      await checkInteractionStates()
    }
    
  } catch (error) {
    console.error('获取帖子详情失败:', error)
    ElMessage.error('获取帖子详情失败')
  } finally {
    loading.value = false
  }
}

// 检查用户与帖子的互动状态
const checkInteractionStates = async () => {
  if (!currentUserId.value || !postInfo.value) return
  
  try {
    // 并发检查各种状态
    const [liked, disliked, favorited] = await Promise.allSettled([
      checkPostLiked(props.postId, currentUserId.value),
      checkPostDisliked(props.postId, currentUserId.value),
      checkPostFavorited(props.postId, currentUserId.value)
    ])
    
    isLiked.value = liked.status === 'fulfilled' ? liked.value : false
    isDisliked.value = disliked.status === 'fulfilled' ? disliked.value : false
    isFavorited.value = favorited.status === 'fulfilled' ? favorited.value : false
    
    console.log('互动状态:', { 
      liked: isLiked.value, 
      disliked: isDisliked.value, 
      favorited: isFavorited.value 
    })
  } catch (error) {
    console.error('检查互动状态失败:', error)
  }
}

// 切换点赞状态
const toggleLike = async () => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  try {
    if (isLiked.value) {
      // 取消点赞
      await unlikePost(props.postId, currentUserId.value)
      isLiked.value = false
      postInfo.value.likeCount = Math.max(0, (postInfo.value.likeCount || 0) - 1)
    } else {
      // 点赞
      const likeData: THPostLike = {
        postId: props.postId,
        userId: currentUserId.value,
        likeTime: new Date().toISOString()
      }
      
      await likePost(likeData)
      isLiked.value = true
      postInfo.value.likeCount = (postInfo.value.likeCount || 0) + 1
      
      // 如果之前是点踩状态，则取消点踩
      if (isDisliked.value) {
        await undislikePost(props.postId, currentUserId.value)
        isDisliked.value = false
        postInfo.value.dislikeCount = Math.max(0, (postInfo.value.dislikeCount || 0) - 1)
      }
    }
  } catch (error) {
    console.error('点赞操作失败:', error)
    ElMessage.error('操作失败，请重试')
  }
}

// 切换点踩状态
const toggleDislike = async () => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  try {
    if (isDisliked.value) {
      // 取消点踩
      await undislikePost(props.postId, currentUserId.value)
      isDisliked.value = false
      postInfo.value.dislikeCount = Math.max(0, (postInfo.value.dislikeCount || 0) - 1)
    } else {
      // 点踩
      const dislikeData: THPostDislike = {
        postId: props.postId,
        userId: currentUserId.value,
        dislikeTime: new Date().toISOString()
      }
      
      await dislikePost(dislikeData)
      isDisliked.value = true
      postInfo.value.dislikeCount = (postInfo.value.dislikeCount || 0) + 1
      
      // 如果之前是点赞状态，则取消点赞
      if (isLiked.value) {
        await unlikePost(props.postId, currentUserId.value)
        isLiked.value = false
        postInfo.value.likeCount = Math.max(0, (postInfo.value.likeCount || 0) - 1)
      }
    }
  } catch (error) {
    console.error('点踩操作失败:', error)
    ElMessage.error('操作失败，请重试')
  }
}

// 切换收藏状态
const toggleFavorite = async () => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  try {
    if (isFavorited.value) {
      // 取消收藏
      await unfavoritePost(props.postId, currentUserId.value)
      isFavorited.value = false
      postInfo.value.favoriteCount = Math.max(0, (postInfo.value.favoriteCount || 0) - 1)
      ElMessage.success('已取消收藏')
    } else {
      // 收藏
      const favoriteData: THPostFavorite = {
        postId: props.postId,
        userId: currentUserId.value,
        favoriteTime: new Date().toISOString()
      }
      
      await favoritePost(favoriteData)
      isFavorited.value = true
      postInfo.value.favoriteCount = (postInfo.value.favoriteCount || 0) + 1
      ElMessage.success('收藏成功')
    }
  } catch (error) {
    console.error('收藏操作失败:', error)
    ElMessage.error('操作失败，请重试')
  }
}

// 显示举报对话框
const showReportDialog = () => {
  reportDialogVisible.value = true
  reportReason.value = ''
  customReportReason.value = ''
}

// 提交举报
const submitReport = async () => {
  if (!currentUserId.value || !postInfo.value) return
  
  try {
    const reason = reportReason.value === 'other' ? customReportReason.value : reportReason.value
    
    const reportData: THPostReport = {
      postId: props.postId,
      userId: currentUserId.value,
      reportReason: reason,
      reportTime: new Date().toISOString()
    }
    
    await reportPost(reportData)
    
    reportDialogVisible.value = false
    ElMessage.success('举报已提交，感谢您维护社区环境！')
  } catch (error) {
    console.error('举报失败:', error)
    ElMessage.error('举报失败，请重试')
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
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border: 1px solid #e8e8e8;
  transition: box-shadow 0.3s ease;
  width: 100%;
  min-width: 600px;
  max-width: 1200px;
  margin-left: auto;
  margin-right: auto;
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

/* 帖子互动区域样式 */
.post-interactions {
  margin-top: 20px;
  padding: 20px;
  background: #f8f9fa;
  border-radius: 12px;
  border: 1px solid #e9ecef;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
  min-height: 60px;
}

.interaction-stats {
  display: flex;
  gap: 16px;
  align-items: center;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 4px;
  color: #6c757d;
  font-size: 12px;
}

.stat-icon {
  font-size: 14px;
}

.stat-text {
  font-weight: 500;
}

.interaction-buttons {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
}

.interaction-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 6px 12px;
  border: 1px solid #e9ecef;
  background: white;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 13px;
  color: #6c757d;
  min-width: 60px;
  justify-content: center;
}

.interaction-btn:hover:not(:disabled) {
  background: #f8f9fa;
  border-color: #dee2e6;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.interaction-btn:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.interaction-btn:active {
  transform: translateY(0);
}

.interaction-btn.active {
  background: #e3f2fd;
  border-color: #4a90e2;
  color: #4a90e2;
}

.interaction-btn.dislike-btn.active {
  background: #ffeaa7;
  border-color: #fdcb6e;
  color: #e17055;
}

.interaction-btn.favorite-btn.active {
  background: #fff3cd;
  border-color: #ffc107;
  color: #856404;
}

.interaction-btn.report-btn:hover {
  background: #f8d7da;
  border-color: #dc3545;
  color: #721c24;
}

.btn-icon {
  font-size: 16px;
}

.btn-text {
  font-size: 12px;
  font-weight: 500;
}

/* 评论区域样式 */
.comment-area {
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px solid #e9ecef;
}

/* 举报对话框样式 */
.report-form {
  padding: 12px 0;
}

.report-hint {
  margin-bottom: 16px;
  color: #495057;
  font-weight: 500;
}

:deep(.el-radio-group) {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

:deep(.el-radio) {
  margin-right: 0;
  white-space: nowrap;
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
  
  .post-interactions {
    flex-direction: column;
    align-items: stretch;
  }
  
  .interaction-stats {
    justify-content: center;
  }
  
  .interaction-buttons {
    justify-content: center;
    flex-wrap: wrap;
  }
  
  .interaction-btn {
    flex: 1;
    min-width: 80px;
    padding: 8px 10px;
  }
  
  .comment-area {
    margin-top: 20px;
    padding-top: 16px;
  }
}

@media (max-width: 480px) {
  .interaction-buttons {
    grid-template-columns: repeat(2, 1fr);
    display: grid;
    gap: 8px;
    width: 100%;
  }
  
  .interaction-btn {
    min-width: auto;
  }
}
</style>
