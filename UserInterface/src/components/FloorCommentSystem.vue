<!--
仿百度贴吧楼层回复系统
TreeHole开发组
-->

<template>
  <div class="floor-comment-system">
    <!-- 楼主帖子 -->
    <div class="floor-item floor-main">
      <div class="floor-header">
        <div class="floor-number">楼主</div>
        <div class="user-info">
          <img :src="mainPost.user.avatarUrl || '/images/default-avatar.png'" class="user-avatar">
          <div class="user-details">
            <div class="username">{{ mainPost.user.userName }}</div>
            <div class="user-level">Lv.{{ mainPost.user.level }}</div>
            <div class="post-time">{{ formatTime(mainPost.creationDate) }}</div>
          </div>
        </div>
        <div class="floor-actions">
          <el-dropdown @command="handleFloorAction">
            <el-button text size="small">⋮</el-button>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="share">分享</el-dropdown-item>
                <el-dropdown-item command="collect">收藏</el-dropdown-item>
                <el-dropdown-item command="report">举报</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </div>
      
      <div class="floor-content">
        <h2 class="post-title">{{ mainPost.title }}</h2>
        <div class="post-content" v-html="processContent(mainPost.content)"></div>
        
        <!-- 帖子图片/媒体 -->
        <div v-if="mainPost.images && mainPost.images.length > 0" class="post-media">
          <div class="image-grid" :class="`grid-${Math.min(mainPost.images.length, 9)}`">
            <div
              v-for="(image, index) in mainPost.images.slice(0, 9)"
              :key="index"
              class="image-item"
              @click="previewImage(mainPost.images, index)"
            >
              <img :src="image.thumbnail || image.url" :alt="`图片${index + 1}`">
              <div v-if="index === 8 && mainPost.images.length > 9" class="more-images">
                +{{ mainPost.images.length - 9 }}
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <div class="floor-stats">
        <div class="stats-left">
          <span class="stat-item">
            <span class="stat-icon">👁️</span>
            <span class="stat-text">{{ mainPost.viewCount || 0 }}</span>
          </span>
          <span class="stat-item">
            <span class="stat-icon">💬</span>
            <span class="stat-text">{{ comments.length }}</span>
          </span>
        </div>
        
        <div class="stats-right">
          <button class="action-btn" :class="{ active: mainPost.isLiked }" @click="toggleLike">
            <span class="btn-icon">👍</span>
            <span class="btn-text">{{ mainPost.likeCount || 0 }}</span>
          </button>
          <button class="action-btn reply-btn" @click="showReplyForm = !showReplyForm">
            <span class="btn-icon">💬</span>
            <span class="btn-text">回复</span>
          </button>
        </div>
      </div>
    </div>

    <!-- 回复表单 -->
    <div v-if="showReplyForm" class="reply-form">
      <div class="form-header">
        <span>回复楼主</span>
        <el-button text size="small" @click="showReplyForm = false">取消</el-button>
      </div>
      
      <el-input
        v-model="replyContent"
        type="textarea"
        placeholder="写下你的回复..."
        :rows="3"
        maxlength="500"
        show-word-limit
      />
      
      <div class="form-actions">
        <div class="form-tools">
          <el-button size="small" @click="insertEmoji">😀</el-button>
          <el-button size="small" @click="insertImage">🖼️</el-button>
          <el-button size="small" @click="insertMention">@</el-button>
        </div>
        
        <el-button 
          type="primary" 
          size="small" 
          @click="submitReply"
          :disabled="!replyContent.trim()"
          :loading="submitting"
        >
          发表回复
        </el-button>
      </div>
    </div>

    <!-- 评论楼层列表 -->
    <div class="comments-list">
      <div
        v-for="(comment, index) in comments"
        :key="comment.commentId"
        class="floor-item"
        :class="{ 'author-reply': comment.userId === mainPost.userId }"
      >
        <div class="floor-header">
          <div class="floor-number">{{ index + 2 }}楼</div>
          <div class="user-info">
            <img :src="comment.user.avatarUrl || '/images/default-avatar.png'" class="user-avatar">
            <div class="user-details">
              <div class="username">
                {{ comment.user.userName }}
                <span v-if="comment.userId === mainPost.userId" class="author-tag">楼主</span>
              </div>
              <div class="user-level">Lv.{{ comment.user.level }}</div>
              <div class="post-time">{{ formatTime(comment.commentTime) }}</div>
            </div>
          </div>
          <div class="floor-actions">
            <el-dropdown @command="(cmd) => handleCommentAction(cmd, comment)">
              <el-button text size="small">⋮</el-button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="reply">回复</el-dropdown-item>
                  <el-dropdown-item command="quote">引用</el-dropdown-item>
                  <el-dropdown-item command="report">举报</el-dropdown-item>
                  <el-dropdown-item 
                    v-if="comment.userId === currentUserId" 
                    command="delete"
                    divided
                  >
                    删除
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
        </div>
        
        <!-- 回复引用 -->
        <div v-if="comment.replyTo" class="reply-quote">
          <div class="quote-header">
            回复 {{ comment.replyTo.userName }}：
          </div>
          <div class="quote-content">{{ comment.replyTo.content }}</div>
        </div>
        
        <div class="floor-content">
          <div class="comment-content" v-html="processContent(comment.content)"></div>
          
          <!-- 评论图片 -->
          <div v-if="comment.images && comment.images.length > 0" class="comment-media">
            <div class="image-grid small">
              <div
                v-for="(image, imgIndex) in comment.images.slice(0, 3)"
                :key="imgIndex"
                class="image-item"
                @click="previewImage(comment.images, imgIndex)"
              >
                <img :src="image.thumbnail || image.url" :alt="`图片${imgIndex + 1}`">
              </div>
            </div>
          </div>
        </div>
        
        <div class="floor-stats">
          <div class="stats-left">
            <span class="floor-position">#{{ index + 2 }}</span>
          </div>
          
          <div class="stats-right">
            <button 
              class="action-btn" 
              :class="{ active: comment.isLiked }" 
              @click="toggleCommentLike(comment)"
            >
              <span class="btn-icon">👍</span>
              <span class="btn-text">{{ comment.likeCount || 0 }}</span>
            </button>
            
            <button class="action-btn reply-btn" @click="replyToComment(comment)">
              <span class="btn-icon">💬</span>
              <span class="btn-text">回复</span>
            </button>
          </div>
        </div>

        <!-- 子回复列表 -->
        <div v-if="comment.replies && comment.replies.length > 0" class="sub-replies">
          <div
            v-for="reply in comment.replies"
            :key="reply.commentId"
            class="sub-reply-item"
          >
            <div class="sub-reply-header">
              <img :src="reply.user.avatarUrl || '/images/default-avatar.png'" class="sub-avatar">
              <span class="sub-username">{{ reply.user.userName }}</span>
              <span class="sub-time">{{ formatTime(reply.commentTime) }}</span>
            </div>
            
            <div class="sub-reply-content">
              <span v-if="reply.replyToUser" class="reply-target">
                回复 @{{ reply.replyToUser }}：
              </span>
              {{ reply.content }}
            </div>
          </div>
          
          <!-- 展开更多回复 -->
          <div v-if="comment.totalReplies > comment.replies.length" class="expand-replies">
            <el-button text size="small" @click="loadMoreReplies(comment)">
              展开更多回复 ({{ comment.totalReplies - comment.replies.length }})
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 底部分页 -->
    <div class="pagination-section">
      <el-pagination
        v-model:current-page="currentPage"
        :page-size="pageSize"
        :total="totalComments"
        layout="prev, pager, next, jumper"
        @current-change="handlePageChange"
      />
    </div>
  </div>
</template>

<script setup lang='ts'>
import { ref, onMounted, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { getCurrentUserId } from '@/utils/auth'

// Props
const props = defineProps<{
  postId: number
}>()

// 响应式数据
const currentUserId = ref(getCurrentUserId() ? parseInt(getCurrentUserId()!) : null)
const mainPost = ref<any>({})
const comments = ref<any[]>([])
const showReplyForm = ref(false)
const replyContent = ref('')
const submitting = ref(false)
const currentPage = ref(1)
const pageSize = ref(20)
const totalComments = ref(0)

// 模拟数据
onMounted(() => {
  loadPostData()
})

const loadPostData = () => {
  // 模拟楼主帖子数据
  mainPost.value = {
    postId: props.postId,
    title: '这是一个示例帖子标题',
    content: '这是帖子的主要内容，可以包含文字、图片、链接等多种元素。',
    user: {
      userId: 1,
      userName: '楼主用户',
      avatarUrl: '/images/avatar1.png',
      level: 5
    },
    creationDate: new Date().toISOString(),
    likeCount: 42,
    viewCount: 1234,
    isLiked: false,
    images: []
  }
  
  // 模拟评论数据
  comments.value = Array.from({ length: 10 }, (_, i) => ({
    commentId: i + 1,
    userId: i + 2,
    content: `这是第${i + 2}楼的回复内容，可能会包含一些有趣的讨论。`,
    user: {
      userId: i + 2,
      userName: `用户${i + 2}`,
      avatarUrl: `/images/avatar${(i % 5) + 1}.png`,
      level: Math.floor(Math.random() * 10) + 1
    },
    commentTime: new Date(Date.now() - Math.random() * 86400000).toISOString(),
    likeCount: Math.floor(Math.random() * 20),
    isLiked: false,
    replyTo: i > 3 && Math.random() > 0.7 ? {
      userName: `用户${Math.floor(Math.random() * i) + 1}`,
      content: '之前的回复内容...'
    } : null,
    replies: Math.random() > 0.8 ? [
      {
        commentId: `${i + 1}-1`,
        userId: 999,
        content: '这是一个子回复',
        user: { userName: '回复用户', avatarUrl: '/images/avatar1.png' },
        commentTime: new Date().toISOString(),
        replyToUser: `用户${i + 2}`
      }
    ] : [],
    totalReplies: Math.floor(Math.random() * 5)
  }))
  
  totalComments.value = comments.value.length
}

// 格式化时间
const formatTime = (timestamp: string) => {
  const date = new Date(timestamp)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  if (diff < 60000) return '刚刚'
  if (diff < 3600000) return `${Math.floor(diff / 60000)}分钟前`
  if (diff < 86400000) return `${Math.floor(diff / 3600000)}小时前`
  
  return date.toLocaleDateString('zh-CN')
}

// 处理内容（支持@提及、话题等）
const processContent = (content: string) => {
  if (!content) return ''
  
  // 处理@提及
  content = content.replace(/@(\w+)/g, '<span class="mention">@$1</span>')
  
  // 处理话题标签
  content = content.replace(/#([^#\s]+)#/g, '<span class="topic">#$1#</span>')
  
  // 处理链接
  content = content.replace(/(https?:\/\/[^\s]+)/g, '<a href="$1" target="_blank" class="link">$1</a>')
  
  return content
}

// 点赞功能
const toggleLike = async () => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  mainPost.value.isLiked = !mainPost.value.isLiked
  mainPost.value.likeCount += mainPost.value.isLiked ? 1 : -1
}

const toggleCommentLike = async (comment: any) => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  comment.isLiked = !comment.isLiked
  comment.likeCount += comment.isLiked ? 1 : -1
}

// 回复功能
const replyToComment = (comment: any) => {
  replyContent.value = `回复 @${comment.user.userName}：`
  showReplyForm.value = true
}

const submitReply = async () => {
  if (!replyContent.value.trim()) {
    ElMessage.warning('请输入回复内容')
    return
  }
  
  submitting.value = true
  
  try {
    // 这里应该调用API提交回复
    const newComment = {
      commentId: Date.now(),
      userId: currentUserId.value,
      content: replyContent.value,
      user: {
        userId: currentUserId.value,
        userName: '当前用户',
        avatarUrl: '/images/default-avatar.png',
        level: 1
      },
      commentTime: new Date().toISOString(),
      likeCount: 0,
      isLiked: false,
      replies: [],
      totalReplies: 0
    }
    
    comments.value.push(newComment)
    replyContent.value = ''
    showReplyForm.value = false
    
    ElMessage.success('回复发表成功')
  } catch (error) {
    console.error('发表回复失败:', error)
    ElMessage.error('发表回复失败')
  } finally {
    submitting.value = false
  }
}

// 处理楼层操作
const handleFloorAction = (command: string) => {
  switch (command) {
    case 'share':
      ElMessage.success('分享功能开发中')
      break
    case 'collect':
      ElMessage.success('收藏功能开发中')
      break
    case 'report':
      ElMessage.success('举报功能开发中')
      break
  }
}

// 处理评论操作
const handleCommentAction = (command: string, comment: any) => {
  switch (command) {
    case 'reply':
      replyToComment(comment)
      break
    case 'quote':
      replyContent.value = `引用 ${comment.user.userName} 的话：\n"${comment.content}"\n\n`
      showReplyForm.value = true
      break
    case 'report':
      ElMessage.success('举报功能开发中')
      break
    case 'delete':
      deleteComment(comment)
      break
  }
}

// 删除评论
const deleteComment = async (comment: any) => {
  try {
    const index = comments.value.findIndex(c => c.commentId === comment.commentId)
    if (index > -1) {
      comments.value.splice(index, 1)
      ElMessage.success('评论已删除')
    }
  } catch (error) {
    console.error('删除评论失败:', error)
    ElMessage.error('删除失败')
  }
}

// 图片预览
const previewImage = (images: any[], index: number) => {
  // 这里可以实现图片预览功能
  console.log('预览图片:', images, index)
}

// 分页处理
const handlePageChange = (page: number) => {
  currentPage.value = page
  // 加载对应页的评论
  loadComments(page)
}

const loadComments = async (page: number = 1) => {
  // 实现分页加载评论
  console.log('加载第', page, '页评论')
}

// 加载更多子回复
const loadMoreReplies = async (comment: any) => {
  // 实现加载更多子回复
  console.log('加载更多回复:', comment)
}

// 工具函数
const insertEmoji = () => {
  ElMessage.info('表情功能开发中')
}

const insertImage = () => {
  ElMessage.info('图片插入功能开发中')
}

const insertMention = () => {
  ElMessage.info('@提及功能开发中')
}
</script>

<style scoped>
.floor-comment-system {
  background: white;
  border-radius: 8px;
  overflow: hidden;
}

/* 楼层样式 */
.floor-item {
  border-bottom: 1px solid #f0f0f0;
  padding: 16px;
  transition: background-color 0.2s;
}

.floor-item:hover {
  background: #fafafa;
}

.floor-item.floor-main {
  background: #f8f9fa;
  border-left: 4px solid #409eff;
}

.floor-item.author-reply {
  background: #fff7e6;
  border-left: 4px solid #e6a23c;
}

.floor-header {
  display: flex;
  align-items: center;
  margin-bottom: 12px;
  gap: 12px;
}

.floor-number {
  background: #409eff;
  color: white;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  min-width: 40px;
  text-align: center;
  flex-shrink: 0;
}

.floor-main .floor-number {
  background: #67c23a;
}

.author-reply .floor-number {
  background: #e6a23c;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
}

.user-details {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.username {
  font-weight: 600;
  font-size: 14px;
  display: flex;
  align-items: center;
  gap: 6px;
}

.author-tag {
  background: #e6a23c;
  color: white;
  padding: 1px 6px;
  border-radius: 8px;
  font-size: 10px;
}

.user-level {
  font-size: 12px;
  color: #409eff;
  background: #e8f4fd;
  padding: 1px 6px;
  border-radius: 8px;
  display: inline-block;
}

.post-time {
  font-size: 12px;
  color: #999;
}

.floor-actions {
  flex-shrink: 0;
}

/* 内容样式 */
.floor-content {
  margin-bottom: 12px;
}

.post-title {
  font-size: 20px;
  font-weight: 600;
  margin-bottom: 12px;
  color: #333;
  line-height: 1.4;
}

.post-content, .comment-content {
  line-height: 1.6;
  color: #333;
  word-break: break-word;
}

/* 回复引用样式 */
.reply-quote {
  background: #f0f9ff;
  border-left: 3px solid #409eff;
  padding: 8px 12px;
  margin-bottom: 8px;
  border-radius: 0 6px 6px 0;
}

.quote-header {
  font-size: 12px;
  color: #409eff;
  font-weight: 600;
  margin-bottom: 4px;
}

.quote-content {
  font-size: 13px;
  color: #666;
  line-height: 1.4;
}

/* 媒体内容样式 */
.post-media, .comment-media {
  margin: 12px 0;
}

.image-grid {
  display: grid;
  gap: 4px;
  border-radius: 8px;
  overflow: hidden;
}

.image-grid.grid-1 { grid-template-columns: 1fr; }
.image-grid.grid-2 { grid-template-columns: repeat(2, 1fr); }
.image-grid.grid-3 { grid-template-columns: repeat(3, 1fr); }
.image-grid.grid-4 { grid-template-columns: repeat(2, 1fr); }
.image-grid.grid-5, .image-grid.grid-6 { grid-template-columns: repeat(3, 1fr); }
.image-grid.grid-7, .image-grid.grid-8, .image-grid.grid-9 { grid-template-columns: repeat(3, 1fr); }

.image-grid.small {
  grid-template-columns: repeat(3, 1fr);
  max-width: 300px;
}

.image-item {
  position: relative;
  aspect-ratio: 1;
  cursor: pointer;
  overflow: hidden;
  border-radius: 4px;
}

.image-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.2s;
}

.image-item:hover img {
  transform: scale(1.05);
}

.more-images {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  font-weight: 600;
}

/* 楼层统计 */
.floor-stats {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 8px;
  border-top: 1px solid #f0f0f0;
}

.stats-left {
  display: flex;
  gap: 16px;
  align-items: center;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: #999;
}

.floor-position {
  font-size: 12px;
  color: #999;
  font-weight: 500;
}

.stats-right {
  display: flex;
  gap: 8px;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  border: 1px solid #e4e7ed;
  background: white;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 12px;
  color: #666;
}

.action-btn:hover {
  background: #f0f0f0;
  border-color: #409eff;
  color: #409eff;
}

.action-btn.active {
  background: #e8f4fd;
  border-color: #409eff;
  color: #409eff;
}

/* 回复表单 */
.reply-form {
  background: #f8f9fa;
  padding: 16px;
  border-bottom: 1px solid #e4e7ed;
}

.form-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  font-weight: 600;
}

.form-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 12px;
}

.form-tools {
  display: flex;
  gap: 8px;
}

/* 子回复样式 */
.sub-replies {
  margin-top: 12px;
  padding-left: 20px;
  border-left: 2px solid #e4e7ed;
}

.sub-reply-item {
  padding: 8px;
  background: #fafafa;
  border-radius: 6px;
  margin-bottom: 8px;
}

.sub-reply-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}

.sub-avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
}

.sub-username {
  font-size: 13px;
  font-weight: 500;
  color: #409eff;
}

.sub-time {
  font-size: 11px;
  color: #999;
  margin-left: auto;
}

.sub-reply-content {
  font-size: 13px;
  line-height: 1.4;
  color: #333;
}

.reply-target {
  color: #409eff;
}

.expand-replies {
  text-align: center;
  padding: 8px;
}

/* 分页样式 */
.pagination-section {
  padding: 20px;
  text-align: center;
  border-top: 1px solid #e4e7ed;
  background: #fafafa;
}

/* 内容处理样式 */
:deep(.mention) {
  color: #409eff;
  background: #e8f4fd;
  padding: 1px 4px;
  border-radius: 3px;
  font-weight: 500;
  cursor: pointer;
}

:deep(.topic) {
  color: #67c23a;
  background: #f0f9ff;
  padding: 1px 4px;
  border-radius: 3px;
  font-weight: 500;
  cursor: pointer;
}

:deep(.link) {
  color: #409eff;
  text-decoration: none;
}

:deep(.link:hover) {
  text-decoration: underline;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .floor-item {
    padding: 12px;
  }
  
  .floor-header {
    flex-wrap: wrap;
    gap: 8px;
  }
  
  .user-info {
    min-width: 0;
  }
  
  .user-details {
    min-width: 0;
  }
  
  .username {
    font-size: 13px;
  }
  
  .floor-stats {
    flex-direction: column;
    gap: 8px;
    align-items: stretch;
  }
  
  .stats-right {
    justify-content: center;
  }
  
  .sub-replies {
    padding-left: 12px;
  }
  
  .image-grid {
    grid-template-columns: repeat(2, 1fr) !important;
  }
}
</style>
