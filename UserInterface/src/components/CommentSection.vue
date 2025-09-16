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
        评论 ({{ totalCommentsCount }})
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
            
            <!-- 父评论引用（如果是回复评论） -->
            <div v-if="comment.parentComment" class="parent-comment-ref">
              <div class="parent-quote">
                <span class="quote-icon">↳</span>
                <span class="quote-author">@{{ comment.parentComment.userName }}</span>
                <span class="quote-content">{{ comment.parentComment.content.substring(0, 50) }}{{ comment.parentComment.content.length > 50 ? '...' : '' }}</span>
              </div>
            </div>

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
                
                <!-- 回复按钮 -->
                <button 
                  v-if="currentUserId"
                  class="interaction-btn reply-btn"
                  @click="showReplyForm(comment)"
                >
                  <span class="btn-icon">↩️</span>
                  <span class="btn-text">回复</span>
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

            <!-- 回复表单 -->
            <div v-if="replyingToComment && replyingToComment.commentId === comment.commentId" class="reply-form">
              <div class="reply-input-wrapper">
                <el-input
                  v-model="replyContent"
                  type="textarea"
                  :placeholder="`回复 @${comment.userName}:`"
                  :rows="2"
                  maxlength="500"
                  show-word-limit
                  :disabled="isSubmittingReply"
                />
                <div class="reply-actions">
                  <el-button 
                    size="small" 
                    @click="cancelReply"
                  >
                    取消
                  </el-button>
                  <el-button 
                    type="primary" 
                    size="small" 
                    @click="submitReply"
                    :loading="isSubmittingReply"
                    :disabled="!replyContent.trim()"
                  >
                    {{ isSubmittingReply ? '回复中...' : '回复' }}
                  </el-button>
                </div>
              </div>
            </div>

            <!-- 子评论列表 -->
            <div v-if="comment.replies && comment.replies.length > 0" class="replies-list">
              <div 
                v-for="reply in comment.replies" 
                :key="reply.commentId" 
                class="reply-item"
              >
                <div class="reply-avatar">
                  <img :src="githubLogoUrl" :alt="reply.userName || '用户'" />
                </div>
                
                <div class="reply-content">
                  <div class="reply-header-info">
                    <span class="reply-author">{{ reply.userName || '未知用户' }}</span>
                    <span class="reply-time">{{ formatTime(reply.commentTime) }}</span>
                  </div>
                  
                  <div class="reply-text">{{ reply.content }}</div>
                  
                  <!-- 回复互动按钮 -->
                  <div class="reply-interactions">
                    <div class="interaction-buttons">
                      <!-- 点赞按钮 -->
                      <button 
                        class="interaction-btn"
                        :class="{ active: reply.isLiked }"
                        @click="toggleCommentLike(reply)"
                        :disabled="!currentUserId"
                      >
                        <span class="btn-icon">👍</span>
                        <span class="btn-text">{{ reply.likeCount || 0 }}</span>
                      </button>
                      
                      <!-- 点踩按钮 -->
                      <button 
                        class="interaction-btn dislike-btn"
                        :class="{ active: reply.isDisliked }"
                        @click="toggleCommentDislike(reply)"
                        :disabled="!currentUserId"
                      >
                        <span class="btn-icon">👎</span>
                        <span class="btn-text">{{ reply.dislikeCount || 0 }}</span>
                      </button>
                      
                      <!-- 删除按钮 (仅回复作者可见) -->
                      <button 
                        v-if="currentUserId && currentUserId === reply.userId"
                        class="interaction-btn delete-btn"
                        @click="deleteComment(reply)"
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

// 回复相关
const replyingToComment = ref<any>(null)
const replyContent = ref('')
const isSubmittingReply = ref(false)

// 举报相关
const reportDialogVisible = ref(false)
const reportReason = ref('')
const customReportReason = ref('')
const reportingComment = ref<any>(null)

// 计算总评论数（包括回复）
const totalCommentsCount = computed(() => {
  let total = comments.value.length
  for (const comment of comments.value) {
    if (comment.replies) {
      total += comment.replies.length
    }
  }
  return total
})

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
    const allComments = response || []
    
    // 分离主评论和回复
    const mainComments = allComments.filter(comment => !comment.parentCommentId)
    const replyComments = allComments.filter(comment => comment.parentCommentId)
    
    // 为每个主评论加载用户信息和互动状态
    for (const comment of mainComments) {
      await loadCommentUserInfo(comment)
      
      // 查找该评论的所有回复
      comment.replies = replyComments.filter(reply => reply.parentCommentId === comment.commentId)
      
      // 为回复加载用户信息
      for (const reply of comment.replies) {
        await loadCommentUserInfo(reply)
      }
    }
    
    // 为回复评论加载父评论信息
    for (const reply of replyComments) {
      const parentComment = mainComments.find(c => c.commentId === reply.parentCommentId)
      if (parentComment) {
        reply.parentComment = {
          userName: parentComment.userName,
          content: parentComment.content
        }
      }
    }
    
    comments.value = mainComments
    console.log('加载的评论结构:', {
      主评论数量: mainComments.length,
      回复数量: replyComments.length,
      总评论数: allComments.length
    })
  } catch (error) {
    console.error('加载评论失败:', error)
    ElMessage.error('加载评论失败')
  } finally {
    loading.value = false
  }
}

// 加载评论用户信息的辅助函数
const loadCommentUserInfo = async (comment: any) => {
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
  
  // 初始化计数
  comment.likeCount = comment.likeCount || 0
  comment.dislikeCount = comment.dislikeCount || 0
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
      commentTime: new Date().toISOString(),
      parentCommentId: null // 主评论没有父评论
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

// 显示回复表单
const showReplyForm = (comment: any) => {
  replyingToComment.value = comment
  replyContent.value = ''
  console.log('显示回复表单，回复评论:', comment.userName)
}

// 取消回复
const cancelReply = () => {
  replyingToComment.value = null
  replyContent.value = ''
}

// 提交回复
const submitReply = async () => {
  if (!currentUserId.value || !replyingToComment.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  if (!replyContent.value.trim()) {
    ElMessage.warning('请输入回复内容')
    return
  }
  
  try {
    isSubmittingReply.value = true
    
    const replyData: THPostComment = {
      postId: props.postId,
      userId: currentUserId.value,
      content: replyContent.value.trim(),
      commentTime: new Date().toISOString(),
      parentCommentId: replyingToComment.value.commentId // 设置父评论ID
    }
    
    console.log('提交回复数据:', replyData)
    
    await createComment(replyData)
    
    replyContent.value = ''
    replyingToComment.value = null
    ElMessage.success('回复发表成功！')
    
    // 重新加载评论列表
    await loadComments()
  } catch (error) {
    console.error('发表回复失败:', error)
    ElMessage.error('发表回复失败，请重试')
  } finally {
    isSubmittingReply.value = false
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

// 删除评论（支持删除主评论和回复）
const deleteComment = async (comment: any) => {
  try {
    const isReply = comment.parentCommentId ? true : false
    const confirmMessage = isReply 
      ? '确定要删除这条回复吗？删除后无法恢复。'
      : '确定要删除这条评论吗？删除后无法恢复。如果该评论有回复，回复也会一起删除。'
    
    console.log('准备删除评论:', {
      commentId: comment.commentId,
      userId: comment.userId,
      currentUserId: currentUserId.value,
      content: comment.content?.substring(0, 50) + '...',
      isReply: isReply
    })

    await ElMessageBox.confirm(
      confirmMessage,
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
    ElMessage.success(isReply ? '回复删除成功' : '评论删除成功')
    
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
      ElMessage.error('删除失败，请重试')
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

/* 父评论引用样式 */
.parent-comment-ref {
  margin-bottom: 8px;
  padding: 8px 12px;
  background: #f8f9fa;
  border-left: 3px solid #6c757d;
  border-radius: 4px;
  font-size: 12px;
}

.parent-quote {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #6c757d;
}

.quote-icon {
  font-size: 14px;
  color: #4a90e2;
}

.quote-author {
  font-weight: 600;
  color: #4a90e2;
}

.quote-content {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* 回复表单样式 */
.reply-form {
  margin-top: 12px;
  padding: 12px;
  background: #f1f3f4;
  border-radius: 6px;
  border: 1px solid #e9ecef;
}

.reply-input-wrapper {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.reply-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.reply-btn:hover {
  background: #e3f2fd;
  border-color: #4a90e2;
  color: #4a90e2;
}

/* 回复列表样式 */
.replies-list {
  margin-top: 16px;
  margin-left: 20px;
  border-left: 2px solid #e9ecef;
  padding-left: 16px;
}

.reply-item {
  display: flex;
  gap: 10px;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 6px;
  margin-bottom: 8px;
  border: 1px solid #e9ecef;
}

.reply-item:last-child {
  margin-bottom: 0;
}

.reply-avatar img {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  object-fit: cover;
}

.reply-content {
  flex: 1;
}

.reply-header-info {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}

.reply-author {
  font-weight: 600;
  color: #333;
  font-size: 13px;
}

.reply-time {
  color: #6c757d;
  font-size: 11px;
}

.reply-text {
  color: #495057;
  line-height: 1.4;
  margin-bottom: 8px;
  word-break: break-word;
  font-size: 13px;
}

.reply-interactions {
  display: flex;
  justify-content: flex-start;
}

.reply-interactions .interaction-buttons {
  gap: 6px;
}

.reply-interactions .interaction-btn {
  padding: 2px 6px;
  font-size: 11px;
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
  
  .replies-list {
    margin-left: 10px;
    padding-left: 10px;
  }
  
  .reply-item {
    padding: 8px;
  }
  
  .reply-avatar img {
    width: 24px;
    height: 24px;
  }
}
</style>
