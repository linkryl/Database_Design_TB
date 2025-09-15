<!--
  组件名称: ProfilePostCard.vue
  组件功能: 帖子卡片组件
-->

<template>
  <div class='post-card' @click='handleClick'>
    <div class='post-header'>
      <el-avatar 
        class='author-avatar' 
        :src='avatarUrl' 
        :size='40'>
        <el-icon :size='20'>
          <UserFilled/>
        </el-icon>
      </el-avatar>
      <div class='author-info'>
        <span class='author-name'>{{ postData.userName }}</span>
        <span class='post-time'>{{ formatTime }}</span>
      </div>
    </div>
    
    <div class='post-body'>
      <h3 class='post-title'>{{ postData.title }}</h3>
      <p class='post-content'>{{ postData.content }}</p>
      <div v-if='postData.imageUrl' class='post-image'>
        <img :src='`${ossBaseUrl}${postData.imageUrl}`' alt='帖子图片'/>
      </div>
    </div>
    
    <div class='post-footer'>
      <div class='footer-stats'>
        <div class='stat-item'>
          <el-icon><ChatDotRound/></el-icon>
          <span>{{ postData.commentCount || 0 }}</span>
        </div>
        <div class='stat-item'>
          <el-icon><Pointer/></el-icon>
          <span>{{ postData.likeCount || 0 }}</span>
        </div>
        <div class='stat-item'>
          <el-icon><Star/></el-icon>
          <span>{{ postData.favoriteCount || 0 }}</span>
        </div>
      </div>
      <div class='footer-category' v-if='categoryName'>
        <el-tag size='small'>{{ categoryName }}</el-tag>
      </div>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { computed, onMounted, ref, defineProps } from 'vue'
import { 
  UserFilled, ChatDotRound, 
  Pointer, Star 
} from '@element-plus/icons-vue'
import { ossBaseUrl, formatDateTimeToCST } from '../globals'
import { useRouter } from 'vue-router'
import axiosInstance from '../utils/axios'

// Props定义
const props = defineProps<{ 
  postId: number 
}>()

const router = useRouter()

// 帖子数据结构
const postData = ref({
  postId: 0,
  userId: 0,
  categoryId: 0,
  barId: null as number | null,
  alsoInTreehole: 0,
  title: '',
  content: '',
  creationDate: '',
  updateDate: '',
  isSticky: 0,
  likeCount: 0,
  dislikeCount: 0,
  favoriteCount: 0,
  commentCount: 0,
  imageUrl: '',
  // 额外的用户信息字段
  userName: '',
  userAvatarUrl: '',
  userProfile: ''
})

const categoryName = ref('')

// 计算属性
const avatarUrl = computed(() => {
  if (postData.value.userAvatarUrl) {
    return `${ossBaseUrl}${postData.value.userAvatarUrl}`
  }
  return ''
})

const formatTime = computed(() => {
  if (!postData.value.creationDate) return ''
  try {
    const result = formatDateTimeToCST(postData.value.creationDate)
    return result.dateTime || result.date
  } catch {
    return postData.value.creationDate
  }
})

// 获取帖子数据
const fetchPostData = async () => {
  try {
    // 获取帖子信息
    const postResponse = await axiosInstance.get(`post/${props.postId}`)
    const post = postResponse.data
    
    // 初始化帖子基本数据
    postData.value = {
      postId: post.postId || props.postId,
      userId: post.userId || 0,
      categoryId: post.categoryId || 0,
      barId: post.barId !== undefined ? post.barId : null,
      alsoInTreehole: post.alsoInTreehole || 0,
      title: post.title || '',
      content: post.content || '',
      creationDate: post.creationDate || '',
      updateDate: post.updateDate || '',
      isSticky: post.isSticky || 0,
      likeCount: post.likeCount || 0,
      dislikeCount: post.dislikeCount || 0,
      favoriteCount: post.favoriteCount || 0,
      commentCount: post.commentCount || 0,
      imageUrl: post.imageUrl || '',
      // 初始化额外字段
      userName: '',
      userAvatarUrl: '',
      userProfile: '',
    }
    
    // 获取作者信息
    if (post.userId) {
      try {
        const userResponse = await axiosInstance.get(`user/${post.userId}`)
        const userData = userResponse.data
        
        // 更新用户相关信息
        postData.value = {
          ...postData.value,
          userName: userData.userName || '未知用户',
          userAvatarUrl: userData.avatarUrl || '',
          userProfile: userData.profile || ''
        }
      } catch (error) {
        console.error('获取用户信息失败:', error)
        // 即使用户信息获取失败，也保留默认值
        postData.value = {
          ...postData.value,
          userName: '未知用户',
          userAvatarUrl: '',
          userProfile: ''
        }
      }
    }
    // 获取分类信息
    if (post.categoryId) {
      try {
        const categoryResponse = await axiosInstance.get(`post-category/${post.categoryId}`)
        categoryName.value = categoryResponse.data.category || categoryResponse.data.categoryName || ''
      } catch (error) {
        console.error('获取分类信息失败:', error)

        categoryName.value = '未分类'
      }
    }
  } catch (error) {
    console.error('获取帖子信息失败:', error)
    // 设置默认值
    postData.value = {
      postId: props.postId,
      userId: 0,
      categoryId: 0,
      barId: null,
      alsoInTreehole: 0,
      title: '加载失败',
      content: '无法加载帖子内容',
      creationDate: '',
      updateDate: '',
      isSticky: 0,
      likeCount: 0,
      dislikeCount: 0,
      favoriteCount: 0,
      commentCount: 0,
      imageUrl: '',
      userName: '未知用户',
      userAvatarUrl: '',
      userProfile: ''
    }
  }
}

// 处理点击事件
const handleClick = () => {
  router.push(`/PostPage/${props.postId}`)
}

// 组件挂载时获取数据
onMounted(() => {
  fetchPostData()
})
</script>

<style scoped>
.post-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  border: 1px solid #e2e8f0;
  transition: all 0.3s;
  cursor: pointer;
  display: flex;
  flex-direction: column;
}

.post-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0,0,0,0.06);
  border-color: #cbd5e1;
}

.post-header {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}

.author-avatar {
  margin-right: 12px;
  border: 2px solid #f1f5f9;
}

.author-info {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.author-name {
  font-size: 14px;
  font-weight: 600;
  color: #334155;
  margin-bottom: 2px;
}

.post-time {
  font-size: 12px;
  color: #94a3b8;
}

.post-body {
  flex: 1;
  margin-bottom: 16px;
}

.post-title {
  font-size: 18px;
  font-weight: 600;
  color: #1e293b;
  margin: 0 0 12px 0;
  line-height: 1.4;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.post-content {
  font-size: 14px;
  color: #475569;
  line-height: 1.6;
  margin: 0 0 12px 0;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
}

.post-image {
  width: 100%;
  height: 200px;
  border-radius: 8px;
  overflow: hidden;
  background: #f8fafc;
}

.post-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.post-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16px;
  border-top: 1px solid #f1f5f9;
}

.footer-stats {
  display: flex;
  gap: 20px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 13px;
  color: #64748b;
}

.stat-item .el-icon {
  font-size: 16px;
}

.stat-item:hover {
  color: #2563eb;
}

.footer-category {
  flex-shrink: 0;
}
</style>