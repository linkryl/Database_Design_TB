<!--
评论卡片组件
2352031 古振
-->

<template>
  <div class="comment-card">
    <div class="comment-header">
      <el-avatar :src="`${ossBaseUrl}${comment.author.avatarUrl}`" size="small"/>
      <div class="comment-user-info">
        <span class="comment-username">{{ comment.author.userName }}</span>
        <span class="comment-time">{{ comment.time }}</span>
      </div>
    </div>
    <div class="comment-content">
      <p v-if="comment.parentCommentId" class="reply-to">
        回复 @{{ comment.parentUserName }}:
      </p>
      <p class="comment-text">{{ comment.content }}</p>
    </div>
    <div class="comment-actions">
      <div class="comment-stats">
        <span class="stat-item">
          <div class="comment-like-button" @click="handleCommentLike">
            <span class="comment-button-icon">👍</span>
          </div>
          <span class="stat-text">{{ comment.commentLikeCount }}</span>
        </span>
        <span class="stat-item">
          <div class="comment-dislike-button" @click="handleCommentDislike">
            <span class="comment-button-icon">👎</span>
          </div>
          <span class="stat-text">{{ comment.commentDislikeCount }}</span>
        </span>
      </div>
      <button class="reply-button" @click="toggleReply">
        回复
      </button>
    </div>
    
    <!-- 回复输入框 -->
    <div v-if="showReply" class="reply-input">
      <el-input v-model="replyContent" 
                placeholder="请输入回复内容" 
                maxlength="128" 
                show-word-limit/>
      <div class="reply-actions">
        <el-button size="small" @click="cancelReply">取消</el-button>
        <el-button type="primary" size="small" @click="sendReply">发送</el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { ref } from 'vue'
import axiosInstance from '../utils/axios'
import { ElMessage } from 'element-plus'
import { ossBaseUrl } from '../globals'

const props = defineProps<{
  comment: any
  id: number
  user: number
  postUser: number
}>()

const showReply = ref(false)
const replyContent = ref('')

const toggleReply = () => {
  showReply.value = !showReply.value
  if (!showReply.value) {
    replyContent.value = ''
  }
}

const cancelReply = () => {
  showReply.value = false
  replyContent.value = ''
}

const sendReply = async () => {
  if (replyContent.value.trim().length === 0) {
    ElMessage.warning('请输入回复内容')
    return
  }
  
  try {
    await axiosInstance.post('post-comment', {
      postId: props.id,
      userId: props.user,
      parentCommentId: props.comment.commentId,
      content: replyContent.value,
      commentTime: new Date().toISOString(),
      likeCount: 0,
      dislikeCount: 0
    })
    ElMessage.success('回复成功')
    showReply.value = false
    replyContent.value = ''
    // 刷新页面以显示新回复
    window.location.reload()
  } catch (error) {
    ElMessage.error('回复失败，请稍后重试')
  }
}

// 处理评论点赞
const handleCommentLike = async () => {
  try {
    // 检查是否已经点赞
    await axiosInstance.get(`post-comment-like/${props.comment.commentId}-${props.user}`)
    // 如果已经点赞，则取消点赞
    try {
      await axiosInstance.delete(`post-comment-like/${props.comment.commentId}-${props.user}`)
      props.comment.commentLikeCount = props.comment.commentLikeCount - 1
      ElMessage.success("已取消点赞")
    } catch (error: any) {
      ElMessage.error("取消点赞失败")
    }
  } catch (error) {
    // 如果没有点赞，则添加点赞
    try {
      await axiosInstance.post('post-comment-like', {
        commentId: props.comment.commentId,
        userId: props.user,
        likeTime: new Date().toISOString()
      })
      props.comment.commentLikeCount = props.comment.commentLikeCount + 1
      ElMessage.success("已点赞")
    } catch (error: any) {
      ElMessage.error("点赞失败")
    }
  }
}

// 处理评论点踩
const handleCommentDislike = async () => {
  try {
    // 检查是否已经点踩
    await axiosInstance.get(`post-comment-dislike/${props.comment.commentId}-${props.user}`)
    // 如果已经点踩，则取消点踩
    try {
      await axiosInstance.delete(`post-comment-dislike/${props.comment.commentId}-${props.user}`)
      props.comment.commentDislikeCount = props.comment.commentDislikeCount - 1
      ElMessage.success("已取消点踩")
    } catch (error: any) {
      ElMessage.error("取消点踩失败")
    }
  } catch (error) {
    // 如果没有点踩，则添加点踩
    try {
      await axiosInstance.post('post-comment-dislike', {
        commentId: props.comment.commentId,
        userId: props.user,
        dislikeTime: new Date().toISOString()
      })
      props.comment.commentDislikeCount = props.comment.commentDislikeCount + 1
      ElMessage.success("已点踩")
    } catch (error: any) {
      ElMessage.error("点踩失败")
    }
  }
}
</script>

<style scoped>
.comment-card {
  background: white;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 12px;
  border: 1px solid #e8e8e8;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.comment-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.comment-user-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.comment-username {
  font-weight: 600;
  color: #333;
  font-size: 14px;
}

.comment-time {
  color: #999;
  font-size: 12px;
}

.comment-content {
  margin-bottom: 12px;
}

.reply-to {
  color: #666;
  font-size: 13px;
  margin-bottom: 4px;
  font-style: italic;
}

.comment-text {
  color: #333;
  line-height: 1.5;
  margin: 0;
  word-break: break-word;
}

.comment-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.comment-stats {
  display: flex;
  gap: 16px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.stat-text {
  font-size: 12px;
  color: #666;
}

/* 评论按钮样式 */
.comment-like-button, .comment-dislike-button {
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  transition: all 0.2s ease-in-out;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 28px;
  height: 24px;
}

.comment-like-button:hover {
  background: #e3f2fd;
  border-color: #4a90e2;
  transform: scale(1.05);
}

.comment-dislike-button:hover {
  background: #ffebee;
  border-color: #f44336;
  transform: scale(1.05);
}

.comment-button-icon {
  font-size: 12px;
  line-height: 1;
}

.reply-button {
  background: none;
  border: none;
  color: #4a90e2;
  cursor: pointer;
  font-size: 12px;
  padding: 4px 8px;
  border-radius: 4px;
  transition: background-color 0.2s;
}

.reply-button:hover {
  background-color: #f0f7ff;
}

.reply-input {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid #f0f0f0;
}

.reply-actions {
  margin-top: 8px;
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>