<!--
帖子页面
2352031 古振
-->

<template>
  <div class='post-side-color'>
    <el-container class='post-main-container'>
      <el-aside width='25%'>
        <div class='post-author-card'>
          <p style='font-style: italic; color: var(--post-author-intro-color)'>AUTHOR</p>
          <el-avatar :src='`${ossBaseUrl}${postData.userAvatarUrl}`' size='large'
                     @click='router.push(`/profile/${postData.userId}`)' style='cursor: pointer'/>
          <p class='post-author-name'>{{ postData.userName }}</p>
          <p class='post-author-intro'>{{ postData.userProfile }}</p>
        </div>
      </el-aside>
      <div class='post-line'/>
      <el-main>
        <div class='post-card'>
          <div class='post-header'>
            <h2 class='post-title'>{{ postData.title }}</h2>
            <button v-if='currentUserId == postData.userId || isAdmin'
                    @click='handleDelete(postId)'
                    class='post-delete-button'>
              {{ "新闻管理" }}
            </button>
          </div>
          <p class='post-content' v-html='formattedContent'/>
          <el-image v-if='postData.imageUrl'
                    class='post-image'
                    :src='`${ossBaseUrl}${postData.imageUrl}`'
                    alt='PostImage'
                    :preview-src-list='[`${ossBaseUrl}${postData.imageUrl}`]'/>
          <div class='post-bottom'>
            <div class='post-date-container'>
              <p class='post-date'>{{ postData.creationDate }}</p>
              <button v-if="postData.userId!=currentUserId"
                      @click="reportDialogVisible = true"
                      class="report-button-icon">
                举报
              </button>
                <el-dialog v-model="reportDialogVisible" width="400px" :close-on-click-modal="false">
                  <ReportPage
                    :postId="postId"
                    :reportedUserId="postData.userId"
                    :reporterId="currentUserId"
                    @close="reportDialogVisible = false"
                  />
                </el-dialog>
            </div>
            <div class='post-stats'>
              <div class='stat-item'>
                <div class='like-button' @click='handleLike'>
                  <span class='button-icon'>👍</span>
                </div>
                <span class='stat-text' style='color: #6F9DDF'>{{ postData.likeCount }}</span>
              </div>
              <div class='stat-item'>
                <div class='favorite-button' @click='handleFavorite'>
                  <span class='button-icon'>⭐</span>
                </div>
                <span class='stat-text' style='color: #6F9DDF'>{{ postData.favoriteCount }}</span>
              </div>
              <div class='stat-item'>
                <el-popover :visible='commentVisible'
                            placement='bottom-end'
                            :width='500'>
                  <template #reference>
                    <div class='comment-button' @click="commentVisible=!commentVisible; commentContent=''">
                      <span class='button-icon'>💬</span>
                    </div>
                  </template>
                  <el-input v-model='commentContent'
                            size='large'
                            maxlength='128'
                            placeholder="请输入您的评论"
                            show-word-limit/>
                  <div style='margin-top: 10px; display: flex; justify-content: space-between; align-items: center'>
                    <p style='margin: 0; font-size: 12px'>{{ "请保持友善，共同营造和谐的社区氛围 "}}</p>
                    <el-button-group>
                      <el-button size='small' @click="commentVisible=false; commentContent=''">
                        {{ "取消" }}
                      </el-button>
                      <el-button type='primary' size='small' @click='sendComment'>
                        {{ "评论"}}
                      </el-button>
                    </el-button-group>
                  </div>
                </el-popover>
                <span class='stat-text' style='color: #6F9DDF'>{{ postData.commentCount }}</span>
              </div>
              <div class='stat-item'>
                <div class='dislike-button' @click='handleDislike'>
                  <span class='button-icon'>👎</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </el-main>
    </el-container>
    <div>
      <PostCommentCard v-for='comment in paginatedComments'
                       :key='comment.commentId'
                       :comment='comment'
                       :id='postId'
                       :user='comment.commentUserId'
                       :post-user='postData.userId'/>
    </div>
  </div>
</template>

<script setup lang='ts'>
import {useRoute} from 'vue-router'
import {onMounted, ref, watch, computed} from 'vue'
import axiosInstance from '../utils/axios'
import {ElMessage, ElMessageBox, ElNotification} from 'element-plus'
import {ossBaseUrl, formatDateTimeToCST} from '../globals'
import {useRouter} from 'vue-router'
import PostCommentCard from '../components/PostCommentCard.vue'
import ReportPage from './ReportPage.vue'

interface PostData {
  userId: number
  title: string
  categoryId: number
  content: string
  creationDate: string
  updateDate: string
  likeCount: number
  dislikeCount: number
  favoriteCount: number
  commentCount: number
  imageUrl: string
  userAvatarUrl: string
  userName: string
  userProfile: string
}

interface CommentAuthor {
  avatarUrl: string
  userName: string
  profile: string
}

interface Comment {
  commentId: number
  parentCommentId: number | null
  content: string
  commentUserId: number
  time: string
  commentLikeCount: number
  commentDislikeCount: number
  parentUserName: string
  parentCommentUserId: number | null
  author: CommentAuthor
}

interface CommentsData {
  comments: Comment[]
  currentPage: number
}

const route = useRoute()
const router = useRouter()
const postId = ref(parseInt(<string>route.params.id))
const storedValue = localStorage.getItem('currentUserId')
const storedUserId = storedValue ? parseInt(storedValue) : 0
const currentUserId = ref(isNaN(storedUserId) ? 0 : storedUserId)
const commentVisible = ref(false)
const commentContent = ref('')
const isAdmin = ref(false)
const reportDialogVisible = ref(false)

const postData = ref<PostData>({
  userId: 0,
  title: '',
  categoryId: 0,
  content: '',
  creationDate: '',
  updateDate: '',
  likeCount: 0,
  dislikeCount: 0,
  favoriteCount: 0,
  commentCount: 0,
  imageUrl: '',
  userAvatarUrl: '',
  userName: '',
  userProfile: ''
})

const commentsData = ref<CommentsData>({
  comments: [],
  currentPage: 1
})

const formattedContent = computed(() => {
  return postData.value.content.replace(/\n/g, '<br>')
})

const paginatedComments = computed(() => {
  const start = (commentsData.value.currentPage - 1) * 10
  const end = start + 10
  return commentsData.value.comments.slice(start, end)
})

const fetchPageData = async () => {
  if (postId.value != 0) {
    try {
      const response = await axiosInstance.get(`post/${postId.value}`)
      if (response.status == 404) {
        await router.push('/404')
      } else {
        postData.value = response.data
        postData.value.creationDate = formatDateTimeToCST(postData.value.creationDate).dateTime
      }
    } catch (error: any) {
      if (error.response?.status == 404) {
        await router.push('/404')
      } else {
        ElMessage.error("GET 请求失败，请检查网络连接情况或稍后重试。")
      }
    }
  }

  if (postData.value.userId != 0) {
    try {
      const response = await axiosInstance.get(`user/${postData.value.userId}`)
      const userData = response.data
      postData.value.userAvatarUrl = userData.avatarUrl
      postData.value.userName = userData.userName
      postData.value.userProfile = userData.profile
    } catch (error: any) {
      ElMessage.error("GET 请求失败，请检查网络连接情况或稍后重试。")
    }
  }

  if (postData.value.commentCount != 0) {
    try {
      const response = await axiosInstance.get(`/post-comment/post-${postId.value}`)
      commentsData.value.comments = response.data.map((commentData: any): Comment => ({
        commentId: commentData.commentId,
        parentCommentId: commentData.parentCommentId,
        content: commentData.content,
        commentUserId: commentData.userId,
        time: formatDateTimeToCST(commentData.commentTime).dateTime,
        commentLikeCount: commentData.likeCount,
        commentDislikeCount: commentData.dislikeCount,
        parentUserName: '',
        parentCommentUserId: null,
        author: {
          avatarUrl: '',
          userName: '',
          profile: ''
        }
      }))

      const userPromises = commentsData.value.comments.map(async (comment: Comment) => {
        try {
          const response = await axiosInstance.get(`user/${comment.commentUserId}`)
          const userData = response.data
          comment.author.avatarUrl = userData.avatarUrl
          comment.author.userName = userData.userName
          comment.author.profile = userData.profile
          if (comment.parentCommentId) {
            const parentResponse = await axiosInstance.get(`post-comment/${comment.parentCommentId}`)
            const parentCommentData = parentResponse.data
            comment.parentCommentUserId = parentCommentData.userId
            const parentUserResponse = await axiosInstance.get(`user/${comment.parentCommentUserId}`)
            comment.parentUserName = parentUserResponse.data.userName
          }
        } catch (error: any) {
          ElMessage.error("GET 请求失败，请检查网络连接情况或稍后重试。")
        }
      })
      await Promise.all(userPromises)
    } catch (error: any) {
      ElMessage.error("GET 请求失败，请检查网络连接情况或稍后重试。")
    }
  }
}

onMounted(() => {
  fetchPageData()
})

watch(() => route.params.id, (newId) => {
  postId.value = parseInt(<string>newId)
  fetchPageData()
})

onMounted(() => {
  if (currentUserId.value == 0) {
    router.push('/login')
    ElMessageBox.alert("请先进行登录！", "提示", {
      showClose: false
    })
  }
})

async function handleLike() {
  try {
    await axiosInstance.get(`post-like/${postId.value}-${currentUserId.value}`)
    try {
      await axiosInstance.delete(`post-like/${postId.value}-${currentUserId.value}`)
      postData.value.likeCount = postData.value.likeCount - 1
      ElMessage.success("已取消点赞")
    } catch (error: any) {
      ElMessage.error("DELETE 请求失败，请检查网络连接情况或稍后重试。")
    }
  } catch (error: any) {
    try {
      await axiosInstance.post('post-like', {
        postId: postId.value,
        userId: currentUserId.value,
        likeTime: new Date().toISOString()
      })
      postData.value.likeCount = postData.value.likeCount + 1
      ElMessage.success("已点赞")
    } catch (error: any) {
      ElMessage.error("POST 请求失败，请检查网络连接情况或稍后重试。")
    }
  }
}

async function handleFavorite() {
  try {
    await axiosInstance.get(`post-favorite/${postId.value}-${currentUserId.value}`)
    try {
      await axiosInstance.delete(`post-favorite/${postId.value}-${currentUserId.value}`)
      postData.value.favoriteCount = postData.value.favoriteCount - 1
      ElMessage.success("已取消收藏")
    } catch (error: any) {
      ElMessage.error("DELETE 请求失败，请检查网络连接情况或稍后重试。")
    }
  } catch (error: any) {
    try {
      await axiosInstance.post('post-favorite', {
        postId: postId.value,
        userId: currentUserId.value,
        favoriteTime: new Date().toISOString()
      })
      postData.value.favoriteCount = postData.value.favoriteCount + 1
      ElMessage.success("已收藏")
    } catch (error: any) {
      ElMessage.error("POST 请求失败，请检查网络连接情况或稍后重试。")
    }
  }
}

async function handleDislike() {
  try {
    await axiosInstance.get(`post-dislike/${postId.value}-${currentUserId.value}`)
    try {
      await axiosInstance.delete(`post-dislike/${postId.value}-${currentUserId.value}`)
      ElMessage.success("已取消点踩")
    } catch (error: any) {
      ElMessage.error("DELETE 请求失败，请检查网络连接情况或稍后重试。")
    }
  } catch (error: any) {
    try {
      await axiosInstance.post('post-dislike', {
        postId: postId.value,
        userId: currentUserId.value,
        dislikeTime: new Date().toISOString()
      })
      ElMessage.success("已点踩")
    } catch (error: any) {
      ElMessage.error("POST 请求失败，请检查网络连接情况或稍后重试。")
    }
  }
}

async function sendComment() {
  if (commentContent.value.trim().length != 0) {
    try {
      await axiosInstance.post('post-comment', {
        postId: postId.value,
        userId: currentUserId.value,
        parentCommentId: null,
        content: commentContent.value,
        commentTime: new Date().toISOString(),
        likeCount: 0,
        dislikeCount: 0
      })
      window.location.reload()
    } catch (error: any) {
      ElNotification({
        title: "评论失败",
        message: "评论失败，请检查网络连接情况或稍后重试。",
        type: 'error'
      })
    }
  }
}

async function handleDelete(id: number) {
  try {
    await axiosInstance.delete(`post/${id}`)
    await router.push('/pet-community')
  } catch (error: any) {
    ElNotification({
      title: "刪除失败",
      message: "刪除失败，请检查网络连接情况或稍后重试。",
      type: 'error'
    })
  }
}
</script>

<style scoped>
:global(:root) {
  --post-side-color: #F0F0F0;
  --post-border-color: #DCDFE6;
  --post-background-color: #FFFFFF;
  --post-author-intro-color: #666666;
  --post-date-color: #888888;
}

/* noinspection CssUnusedSymbol */
:global(.dark) {
  --post-side-color: #373737;
  --post-border-color: #4C4D4F;
  --post-background-color: #27292b;
  --post-author-intro-color: #C1C1C1;
  --post-date-color: #888888;
}

.post-side-color {
  background-color: var(--post-side-color);
  min-height: calc(100vh - 135px);
  display: flex;
  flex-direction: column;
}

.post-main-container {
  width: 65%;
  margin: 0 auto;
  min-width: 1200px;
  border-right-style: solid;
  border-left-style: solid;
  border-width: 1px;
  border-color: var(--post-border-color);
  background-color: var(--post-background-color);
  flex: 1;
}

.post-author-card {
  text-align: center;
  padding: 40px;
  line-height: 1.8;
}

.post-author-name {
  font-weight: bold;
  margin-top: 10px;
  line-height: 1.8;
}

.post-author-intro {
  margin-top: 10px;
  color: var(--post-author-intro-color);
  line-height: 1.8;
}

.post-line {
  margin-top: 15px;
  margin-bottom: 15px;
  background: var(--post-border-color);
  width: 1px;
  height: auto;
  position: relative;
  float: left;
}

.post-card {
  margin-bottom: 10px;
  display: flex;
  flex-direction: column;
  margin-left: 15px;
  line-height: 1.8;
}

.post-title {
  font-size: 24px;
  font-weight: bold;
  line-height: 1.8;
}

.post-content {
  margin: 16px 0;
  white-space: pre-wrap;
  line-height: 1.8;
}

.post-date {
  margin-top: 24px;
  color: var(--post-date-color);
  line-height: 1.8;
}

.post-image {
  width: 40%;
  margin-top: 10px;
  border-radius: 5px;
  cursor: pointer;
}

.post-bottom {
  width: 100%;
  display: flex;
  justify-content: space-between;
  margin-bottom: 25px;
}

.post-stats {
  display: flex;
  align-items: center;
  gap: 30px;
  margin-right: 10px;
}

.stat-item {
  display: flex;
  align-items: center;
}

.stat-text {
  font-size: 18px;
  margin-left: 5px;
}

/* 按钮样式 */
.like-button, .favorite-button, .comment-button, .dislike-button {
  cursor: pointer;
  padding: 8px 12px;
  border-radius: 8px;
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  transition: all 0.2s ease-in-out;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 40px;
  height: 32px;
}

.like-button:hover {
  background: #e3f2fd;
  border-color: #4a90e2;
  transform: scale(1.05);
}

.favorite-button:hover {
  background: #fff3e0;
  border-color: #ff9800;
  transform: scale(1.05);
}

.comment-button:hover {
  background: #f3e5f5;
  border-color: #9c27b0;
  transform: scale(1.05);
}

.dislike-button:hover {
  background: #ffebee;
  border-color: #f44336;
  transform: scale(1.05);
}

.button-icon {
  font-size: 16px;
  line-height: 1;
}

.post-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.post-delete-button {
  background-color: rgba(244, 67, 54, 0.1);
  border: none;
  color: #FF5722;
  cursor: pointer;
  font-size: 16px;
  padding: 6px 12px;
  margin-right: 12px;
  transition: background-color 0.3s ease, color 0.3s ease;
}

.post-delete-button:hover {
  background-color: rgba(244, 67, 54, 0.2);
  color: #E64A19;
}

.post-date-container {
  display: flex;
  align-items: center;
}

.report-button-icon {
  background-color: transparent;
  border: none;
  color: #FF5722;
  cursor: pointer;
  font-size: 14px;
  margin-left: 16px;
  margin-top: 5px;
}

.report-button-icon:hover {
  color: #E64A19;
}
</style>