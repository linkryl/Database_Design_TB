<!--
评论系统组件
包含评论列表显示、发表评论、评论互动功能
TreeHole开发组
-->

<template>
  <div class="comment-section">
    <!-- 评论标题 -->
    <div class="comment-header">
      <h4 class="comment-title">
        <span class="comment-icon">💬</span>
        评论 ({{ comments.length }})
      </h4>
    </div>

    <!-- 发表评论表单 -->
    <div v-if="currentUserId" class="comment-form">
      <div class="comment-input-wrapper">
        <el-input
          v-model="newCommentContent"
          type="textarea"
          placeholder="写下你的评论..."
          :rows="3"
          maxlength="500"
          show-word-limit
          :disabled="isSubmitting"
        />
        <div class="comment-actions">
          <el-button 
            type="primary" 
            size="small" 
            @click="submitComment"
            :loading="isSubmitting"
            :disabled="!newCommentContent.trim()"
          >
            {{ isSubmitting ? '发表中...' : '发表评论' }}
          </el-button>
        </div>
      </div>
    </div>

    <!-- 未登录提示 -->
    <div v-else class="login-tip">
      <span>请先<a href="/login" class="login-link">登录</a>后再评论</span>
    </div>

    <!-- 评论列表 -->
    <div class="comments-list">
      <div v-if="loading" class="loading-container" v-loading="loading">
        <div style="height: 60px;">加载评论中...</div>
      </div>
      
      <div v-else-if="comments.length === 0" class="empty-comments">
        <div class="empty-icon">💭</div>
        <div class="empty-text">还没有评论，快来抢沙发吧！</div>
      </div>
      
      <div v-else class="comment-list">
        <div 
          v-for="comment in comments" 
          :key="comment.commentId" 
          class="comment-item"
        >
          <div class="comment-avatar">
            <img :src="githubLogoUrl" :alt="comment.userName || '用户'" />
          </div>
          
          <div class="comment-content">
            <div class="comment-header-info">
              <span class="comment-author">{{ comment.userName || '未知用户' }}</span>
              <span class="comment-time">{{ formatTime(comment.commentTime) }}</span>
            </div>
            
            <div class="comment-text">{{ comment.content }}</div>
            
            <!-- 评论互动按钮 -->
            <div class="comment-interactions">
              <div class="interaction-buttons">
                <!-- 点赞按钮 -->
                <button 
                  class="interaction-btn"
                  :class="{ active: comment.isLiked }"
                  @click="toggleCommentLike(comment)"
                  :disabled="!currentUserId"
                >
                  <span class="btn-icon">👍</span>
                  <span class="btn-text">{{ comment.likeCount || 0 }}</span>
                </button>
                
                <!-- 点踩按钮 -->
                <button 
                  class="interaction-btn dislike-btn"
                  :class="{ active: comment.isDisliked }"
                  @click="toggleCommentDislike(comment)"
                  :disabled="!currentUserId"
                >
                  <span class="btn-icon">👎</span>
                  <span class="btn-text">{{ comment.dislikeCount || 0 }}</span>
                </button>
                
                <!-- 举报按钮 -->
                <button 
                  v-if="currentUserId && currentUserId !== comment.userId"
                  class="interaction-btn report-btn"
                  @click="reportComment(comment)"
                >
                  <span class="btn-icon">🚨</span>
                  <span class="btn-text">举报</span>
                </button>
                
                <!-- 删除按钮 (仅评论作者可见) -->
                <button 
                  v-if="currentUserId && currentUserId === comment.userId"
                  class="interaction-btn delete-btn"
                  @click="deleteComment(comment)"
                >
                  <span class="btn-icon">🗑️</span>
                  <span class="btn-text">删除</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 举报对话框 -->
    <el-dialog
      v-model="reportDialogVisible"
      title="举报评论"
      width="400px"
      :close-on-click-modal="false"
    >
      <div class="report-form">
        <p class="report-hint">请选择举报理由：</p>
        <el-radio-group v-model="reportReason">
          <el-radio value="spam">垃圾信息</el-radio>
          <el-radio value="inappropriate">内容不当</el-radio>
          <el-radio value="harassment">骚扰行为</el-radio>
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

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { 
  getCommentsByPostId, 
  createComment, 
  deleteComment as apiDeleteComment,
  likeComment,
  unlikeComment,
  dislikeComment,
  undislikeComment,
  checkCommentLiked,
  checkCommentDisliked,
  reportComment as apiReportComment,
  getUserById,
  type THPostComment,
  type THCommentLike,
  type THCommentDislike,
  type THCommentReport
} from '@/api/index'
import { getCurrentUserId } from '@/utils/auth'

// Props
const props = defineProps<{
  postId: number
}>()

// 响应式数据
const currentUserId = ref(getCurrentUserId() ? parseInt(getCurrentUserId()!) : null)
const comments = ref<any[]>([])
const loading = ref(true)
const isSubmitting = ref(false)
const newCommentContent = ref('')
const githubLogoUrl = '/images/GitHubLogo.png'

// 举报相关
const reportDialogVisible = ref(false)
const reportReason = ref('')
const customReportReason = ref('')
const reportingComment = ref<any>(null)

// 时间格式化函数 - 修复时区问题
const formatTime = (timestamp: string) => {
  if (!timestamp) return '未知时间'
  
  try {
    // 解析时间戳，处理UTC时间
    let date = new Date(timestamp)
    
    // 如果是ISO格式但没有时区标识，假设是UTC时间
    if (timestamp.includes('T') && !timestamp.includes('Z') && !timestamp.includes('+') && !timestamp.includes('-', 10)) {
      date = new Date(timestamp + 'Z')
    }
    
    const now = new Date()
    const diff = now.getTime() - date.getTime()
    
    // 使用绝对值处理可能的时区差异
    const absDiff = Math.abs(diff)
    
    if (absDiff < 60000) return '刚刚'
    if (absDiff < 3600000) return `${Math.floor(absDiff / 60000)}分钟前`
    if (absDiff < 86400000) return `${Math.floor(absDiff / 3600000)}小时前`
    if (absDiff < 604800000) return `${Math.floor(absDiff / 86400000)}天前`
    
    return date.toLocaleDateString('zh-CN', {
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch (error) {
    console.error('评论时间格式化错误:', error, timestamp)
    return '时间错误'
  }
}

// 加载评论列表
const loadComments = async () => {
  try {
    loading.value = true
    const response = await getCommentsByPostId(props.postId)
    comments.value = response || []
    
    // 为每个评论加载用户信息和互动状态
    for (const comment of comments.value) {
      // 加载用户名
      try {
        const userInfo = await getUserById(comment.userId)
        comment.userName = userInfo.userName
      } catch (error) {
        comment.userName = '未知用户'
      }
      
      // 检查当前用户是否已点赞/点踩该评论
      if (currentUserId.value) {
        try {
          comment.isLiked = await checkCommentLiked(comment.commentId, currentUserId.value)
          comment.isDisliked = await checkCommentDisliked(comment.commentId, currentUserId.value)
        } catch (error) {
          comment.isLiked = false
          comment.isDisliked = false
        }
      }
      
      // 初始化计数（后端API可能不返回计数，这里先设为0）
      comment.likeCount = comment.likeCount || 0
      comment.dislikeCount = comment.dislikeCount || 0
    }
  } catch (error) {
    console.error('加载评论失败:', error)
    ElMessage.error('加载评论失败')
  } finally {
    loading.value = false
  }
}

// 发表评论
const submitComment = async () => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  if (!newCommentContent.value.trim()) {
    ElMessage.warning('请输入评论内容')
    return
  }
  
  try {
    isSubmitting.value = true
    
    const commentData: THPostComment = {
      postId: props.postId,
      userId: currentUserId.value,
      content: newCommentContent.value.trim(),
      commentTime: new Date().toISOString()
    }
    
    await createComment(commentData)
    
    newCommentContent.value = ''
    ElMessage.success('评论发表成功！')
    
    // 重新加载评论列表
    await loadComments()
  } catch (error) {
    console.error('发表评论失败:', error)
    ElMessage.error('发表评论失败，请重试')
  } finally {
    isSubmitting.value = false
  }
}

// 切换评论点赞状态
const toggleCommentLike = async (comment: any) => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  try {
    if (comment.isLiked) {
      // 取消点赞
      await unlikeComment(comment.commentId, currentUserId.value)
      comment.isLiked = false
      comment.likeCount = Math.max(0, comment.likeCount - 1)
    } else {
      // 点赞
      const likeData: THCommentLike = {
        commentId: comment.commentId,
        userId: currentUserId.value,
        likeTime: new Date().toISOString()
      }
      
      await likeComment(likeData)
      comment.isLiked = true
      comment.likeCount = comment.likeCount + 1
      
      // 如果之前是点踩状态，则取消点踩
      if (comment.isDisliked) {
        await undislikeComment(comment.commentId, currentUserId.value)
        comment.isDisliked = false
        comment.dislikeCount = Math.max(0, comment.dislikeCount - 1)
      }
    }
  } catch (error) {
    console.error('操作失败:', error)
    ElMessage.error('操作失败，请重试')
  }
}

// 切换评论点踩状态
const toggleCommentDislike = async (comment: any) => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  try {
    if (comment.isDisliked) {
      // 取消点踩
      await undislikeComment(comment.commentId, currentUserId.value)
      comment.isDisliked = false
      comment.dislikeCount = Math.max(0, comment.dislikeCount - 1)
    } else {
      // 点踩
      const dislikeData: THCommentDislike = {
        commentId: comment.commentId,
        userId: currentUserId.value,
        dislikeTime: new Date().toISOString()
      }
      
      await dislikeComment(dislikeData)
      comment.isDisliked = true
      comment.dislikeCount = comment.dislikeCount + 1
      
      // 如果之前是点赞状态，则取消点赞
      if (comment.isLiked) {
        await unlikeComment(comment.commentId, currentUserId.value)
        comment.isLiked = false
        comment.likeCount = Math.max(0, comment.likeCount - 1)
      }
    }
  } catch (error) {
    console.error('操作失败:', error)
    ElMessage.error('操作失败，请重试')
  }
}

// 举报评论
const reportComment = async (comment: any) => {
  reportingComment.value = comment
  reportDialogVisible.value = true
  reportReason.value = ''
  customReportReason.value = ''
}

// 提交举报
const submitReport = async () => {
  if (!reportingComment.value || !currentUserId.value) return
  
  try {
    const reason = reportReason.value === 'other' ? customReportReason.value : reportReason.value
    
    const reportData: THCommentReport = {
      commentId: reportingComment.value.commentId,
      userId: currentUserId.value,
      reportReason: reason,
      reportTime: new Date().toISOString()
    }
    
    await apiReportComment(reportData)
    
    reportDialogVisible.value = false
    ElMessage.success('举报已提交，感谢您维护社区环境！')
  } catch (error) {
    console.error('举报失败:', error)
    ElMessage.error('举报失败，请重试')
  }
}

// 删除评论
const deleteComment = async (comment: any) => {
  try {
    console.log('准备删除评论:', {
      commentId: comment.commentId,
      userId: comment.userId,
      currentUserId: currentUserId.value,
      content: comment.content?.substring(0, 50) + '...'
    })

    await ElMessageBox.confirm(
      '确定要删除这条评论吗？删除后无法恢复。',
      '确认删除',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    
    console.log('用户确认删除，调用API删除评论ID:', comment.commentId)
    await apiDeleteComment(comment.commentId)
    
    console.log('评论删除成功')
    ElMessage.success('评论删除成功')
    
    // 重新加载评论列表
    await loadComments()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('删除评论失败:', error)
      console.error('错误详细信息:', {
        message: error.message,
        status: error.response?.status,
        data: error.response?.data
      })
      ElMessage.error('删除评论失败，请重试')
    } else {
      console.log('用户取消删除操作')
    }
  }
}

// 组件挂载时加载评论
onMounted(() => {
  loadComments()
})
</script>

<style scoped>
.comment-section {
  margin-top: 20px;
}

.comment-header {
  margin-bottom: 16px;
}

.comment-title {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.comment-icon {
  font-size: 18px;
}

/* 评论表单样式 */
.comment-form {
  margin-bottom: 24px;
  padding: 16px;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e9ecef;
}

.comment-input-wrapper {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.comment-actions {
  display: flex;
  justify-content: flex-end;
}

.login-tip {
  text-align: center;
  padding: 16px;
  background: #f8f9fa;
  border-radius: 8px;
  color: #6c757d;
  margin-bottom: 24px;
}

.login-link {
  color: #4a90e2;
  text-decoration: none;
}

.login-link:hover {
  text-decoration: underline;
}

/* 评论列表样式 */
.loading-container {
  display: flex;
  justify-content: center;
  padding: 40px;
}

.empty-comments {
  text-align: center;
  padding: 40px;
  color: #6c757d;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 12px;
  opacity: 0.5;
}

.empty-text {
  font-size: 14px;
}

.comment-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.comment-item {
  display: flex;
  gap: 12px;
  padding: 16px;
  background: white;
  border-radius: 8px;
  border: 1px solid #e9ecef;
  transition: box-shadow 0.2s ease;
}

.comment-item:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.comment-avatar {
  flex-shrink: 0;
}

.comment-avatar img {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  object-fit: cover;
}

.comment-content {
  flex: 1;
}

.comment-header-info {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}

.comment-author {
  font-weight: 600;
  color: #333;
  font-size: 14px;
}

.comment-time {
  color: #6c757d;
  font-size: 12px;
}

.comment-text {
  color: #495057;
  line-height: 1.5;
  margin-bottom: 12px;
  word-break: break-word;
}

/* 评论互动按钮样式 */
.comment-interactions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.interaction-buttons {
  display: flex;
  gap: 8px;
}

.interaction-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  border: 1px solid #e9ecef;
  background: white;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 12px;
  color: #6c757d;
}

.interaction-btn:hover:not(:disabled) {
  background: #f8f9fa;
  border-color: #dee2e6;
}

.interaction-btn:disabled {
  cursor: not-allowed;
  opacity: 0.6;
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

.interaction-btn.report-btn:hover {
  background: #fff3cd;
  border-color: #ffc107;
  color: #856404;
}

.interaction-btn.delete-btn:hover {
  background: #f8d7da;
  border-color: #dc3545;
  color: #721c24;
}

.btn-icon {
  font-size: 14px;
}

.btn-text {
  font-size: 12px;
  font-weight: 500;
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

/* 响应式设计 */
@media (max-width: 768px) {
  .comment-item {
    padding: 12px;
  }
  
  .comment-avatar img {
    width: 32px;
    height: 32px;
  }
  
  .interaction-buttons {
    flex-wrap: wrap;
    gap: 6px;
  }
  
  .interaction-btn {
    padding: 3px 6px;
    font-size: 11px;
  }
}
</style>
