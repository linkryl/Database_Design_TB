<!--
搜索结果页面
TreeHole开发组
-->

<template>
  <div class="search-page">
    <!-- 页面头部 -->
    <div class="search-header">
      <div class="header-content">
        <h1 class="page-title">🔍 全站搜索</h1>
        <p class="page-subtitle">发现更多有趣的内容</p>
      </div>
    </div>

    <!-- 搜索组件 -->
    <div class="search-container">
      <SearchComponent ref="searchComponentRef" />
    </div>

    <!-- 推荐内容（无搜索时显示） -->
    <div v-if="!hasSearchResults" class="recommendations">
      <div class="recommendation-section">
        <h3 class="section-title">🔥 热门帖子</h3>
        <div class="hot-posts">
          <div
            v-for="post in hotPosts"
            :key="post.postId"
            class="hot-post-item"
            @click="navigateToPost(post.postId)"
          >
            <div class="post-rank">{{ post.rank }}</div>
            <div class="post-info">
              <div class="post-title">{{ post.title }}</div>
              <div class="post-stats">
                <span>👍 {{ post.likeCount }}</span>
                <span>💬 {{ post.commentCount }}</span>
                <span>🔥 {{ post.heatScore }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="recommendation-section">
        <h3 class="section-title">👥 活跃用户</h3>
        <div class="active-users">
          <div
            v-for="user in activeUsers"
            :key="user.userId"
            class="active-user-item"
            @click="navigateToUser(user.userId)"
          >
            <img :src="user.avatarUrl || '/images/default-avatar.png'" class="user-avatar">
            <div class="user-info">
              <div class="user-name">{{ user.userName }}</div>
              <div class="user-level">Lv.{{ user.level }}</div>
            </div>
            <div class="user-exp">{{ user.experiencePoints }}经验</div>
          </div>
        </div>
      </div>

      <div class="recommendation-section">
        <h3 class="section-title">🏠 推荐贴吧</h3>
        <div class="recommended-bars">
          <div
            v-for="bar in recommendedBars"
            :key="bar.barId"
            class="recommended-bar-item"
            @click="navigateToBar(bar.barId)"
          >
            <img :src="bar.avatarUrl || '/images/default-bar.png'" class="bar-avatar">
            <div class="bar-info">
              <div class="bar-name">{{ bar.barName }}</div>
              <div class="bar-description">{{ bar.description || '暂无简介' }}</div>
              <div class="bar-stats">
                <span>👥 {{ bar.followedCount }}</span>
                <span>📝 {{ bar.postCount }}</span>
              </div>
            </div>
            <el-button size="small" type="primary" @click.stop="followBar(bar.barId)">
              {{ bar.isFollowed ? '已关注' : '关注' }}
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 搜索技巧 -->
    <div class="search-tips">
      <h3 class="tips-title">💡 搜索技巧</h3>
      <div class="tips-content">
        <div class="tip-item">
          <span class="tip-label">精确搜索：</span>
          <span class="tip-text">使用引号包围关键词，如 "树洞论坛"</span>
        </div>
        <div class="tip-item">
          <span class="tip-label">排除词语：</span>
          <span class="tip-text">在词语前加减号，如 编程 -Java</span>
        </div>
        <div class="tip-item">
          <span class="tip-label">用户搜索：</span>
          <span class="tip-text">使用 @用户名 搜索特定用户</span>
        </div>
        <div class="tip-item">
          <span class="tip-label">贴吧搜索：</span>
          <span class="tip-text">使用 #贴吧名 搜索特定贴吧</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { ref, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import SearchComponent from '@/components/SearchComponent.vue'
import axios from 'axios'

// 路由
const router = useRouter()
const route = useRoute()

// 组件引用
const searchComponentRef = ref<InstanceType<typeof SearchComponent>>()

// 响应式数据
const hasSearchResults = ref(false)
const hotPosts = ref<any[]>([])
const activeUsers = ref<any[]>([])
const recommendedBars = ref<any[]>([])

// API基础URL
const apiBaseUrl = '/api'

// 计算属性
const currentUserId = computed(() => {
  const storedValue = localStorage.getItem('currentUserId')
  return storedValue ? parseInt(storedValue) : 0
})

// 组件挂载时初始化
onMounted(async () => {
  // 如果URL中有搜索参数，执行搜索
  const keyword = route.query.q as string
  if (keyword) {
    // 等待搜索组件加载完成后执行搜索
    setTimeout(() => {
      if (searchComponentRef.value) {
        searchComponentRef.value.performSearch()
      }
    }, 100)
  }

  // 加载推荐内容
  await loadRecommendations()
})

// 加载推荐内容
const loadRecommendations = async () => {
  try {
    // 并发加载所有推荐内容
    const [hotPostsRes, activeUsersRes, recommendedBarsRes] = await Promise.allSettled([
      loadHotPosts(),
      loadActiveUsers(),
      loadRecommendedBars()
    ])

    if (hotPostsRes.status === 'fulfilled') {
      hotPosts.value = hotPostsRes.value
    }
    if (activeUsersRes.status === 'fulfilled') {
      activeUsers.value = activeUsersRes.value
    }
    if (recommendedBarsRes.status === 'fulfilled') {
      recommendedBars.value = recommendedBarsRes.value
    }
  } catch (error) {
    console.error('加载推荐内容失败:', error)
  }
}

// 加载热门帖子
const loadHotPosts = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/post/hot`, {
      params: { limit: 10 }
    })
    
    return response.data.map((post: any, index: number) => ({
      ...post,
      rank: index + 1,
      heatScore: calculateHeatScore(post)
    }))
  } catch (error) {
    console.error('加载热门帖子失败:', error)
    // 返回模拟数据
    return Array.from({ length: 10 }, (_, i) => ({
      postId: i + 1,
      title: `热门帖子 ${i + 1}`,
      likeCount: Math.floor(Math.random() * 100),
      commentCount: Math.floor(Math.random() * 50),
      rank: i + 1,
      heatScore: Math.floor(Math.random() * 1000)
    }))
  }
}

// 加载活跃用户
const loadActiveUsers = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/user/active`, {
      params: { limit: 8 }
    })
    
    return response.data.map((user: any) => ({
      ...user,
      level: calculateUserLevel(user.experiencePoints)
    }))
  } catch (error) {
    console.error('加载活跃用户失败:', error)
    // 返回模拟数据
    return Array.from({ length: 8 }, (_, i) => ({
      userId: i + 1,
      userName: `活跃用户${i + 1}`,
      avatarUrl: `/images/avatar${(i % 5) + 1}.png`,
      experiencePoints: Math.floor(Math.random() * 10000),
      level: Math.floor(Math.random() * 20) + 1
    }))
  }
}

// 加载推荐贴吧
const loadRecommendedBars = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/bar/recommended`, {
      params: { limit: 6 }
    })
    
    // 检查用户是否已关注这些贴吧
    if (currentUserId.value) {
      const followedBarsRes = await axios.get(`${apiBaseUrl}/bar-follow/user/${currentUserId.value}/bar-ids`)
      const followedBarIds = followedBarsRes.data
      
      return response.data.map((bar: any) => ({
        ...bar,
        isFollowed: followedBarIds.includes(bar.barId)
      }))
    }
    
    return response.data.map((bar: any) => ({
      ...bar,
      isFollowed: false
    }))
  } catch (error) {
    console.error('加载推荐贴吧失败:', error)
    // 返回模拟数据
    return Array.from({ length: 6 }, (_, i) => ({
      barId: i + 1,
      barName: `推荐贴吧${i + 1}`,
      description: `这是一个很有趣的贴吧，欢迎大家来讨论相关话题`,
      avatarUrl: `/images/bar${(i % 3) + 1}.png`,
      followedCount: Math.floor(Math.random() * 1000),
      postCount: Math.floor(Math.random() * 5000),
      isFollowed: false
    }))
  }
}

// 计算热度分数
const calculateHeatScore = (post: any) => {
  const now = new Date()
  const postDate = new Date(post.creationDate)
  const hoursDiff = (now.getTime() - postDate.getTime()) / (1000 * 60 * 60)
  
  // 简单的热度算法：点赞数 + 评论数*2 - 时间衰减
  const baseScore = (post.likeCount || 0) + (post.commentCount || 0) * 2
  const timeDecay = Math.max(0, 1 - hoursDiff / 168) // 一周内的时间衰减
  
  return Math.floor(baseScore * timeDecay)
}

// 计算用户等级
const calculateUserLevel = (experiencePoints: number) => {
  if (experiencePoints < 100) return 1
  if (experiencePoints < 500) return 2
  if (experiencePoints < 1000) return 3
  if (experiencePoints < 2000) return 4
  if (experiencePoints < 5000) return 5
  return Math.min(20, Math.floor(experiencePoints / 1000) + 5)
}

// 关注贴吧
const followBar = async (barId: number) => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }

  try {
    const bar = recommendedBars.value.find(b => b.barId === barId)
    if (!bar) return

    if (bar.isFollowed) {
      // 取消关注
      await axios.delete(`${apiBaseUrl}/bar-follow/${barId}/${currentUserId.value}`)
      bar.isFollowed = false
      bar.followedCount--
      ElMessage.success('已取消关注')
    } else {
      // 关注
      await axios.post(`${apiBaseUrl}/bar-follow`, {
        barId: barId,
        userId: currentUserId.value,
        followTime: new Date().toISOString(),
        isActive: 1
      })
      bar.isFollowed = true
      bar.followedCount++
      ElMessage.success('关注成功')
    }
  } catch (error) {
    console.error('关注操作失败:', error)
    ElMessage.error('操作失败，请稍后重试')
  }
}

// 导航方法
const navigateToPost = (postId: number) => {
  router.push(`/PostPage/${postId}`)
}

const navigateToUser = (userId: number) => {
  router.push(`/ProfilePage/${userId}`)
}

const navigateToBar = (barId: number) => {
  router.push(`/BarPage/${barId}`)
}

// 监听搜索结果变化
const handleSearchResults = (hasResults: boolean) => {
  hasSearchResults.value = hasResults
}
</script>

<style scoped>
.search-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
}

.search-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 60px 0 40px;
  text-align: center;
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.page-title {
  font-size: 48px;
  font-weight: 700;
  margin-bottom: 16px;
  text-shadow: 0 2px 4px rgba(0,0,0,0.3);
}

.page-subtitle {
  font-size: 18px;
  opacity: 0.9;
  margin: 0;
}

.search-container {
  max-width: 1200px;
  margin: -30px auto 40px;
  padding: 0 20px;
  position: relative;
  z-index: 10;
}

/* 推荐内容样式 */
.recommendations {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px 40px;
}

.recommendation-section {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 30px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.section-title {
  font-size: 20px;
  font-weight: 600;
  margin-bottom: 20px;
  color: #333;
  border-left: 4px solid #409eff;
  padding-left: 12px;
}

/* 热门帖子样式 */
.hot-posts {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.hot-post-item {
  display: flex;
  align-items: center;
  padding: 16px;
  background: #f8f9fa;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s;
}

.hot-post-item:hover {
  background: #e9ecef;
  transform: translateX(4px);
}

.post-rank {
  width: 32px;
  height: 32px;
  background: linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%);
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  margin-right: 16px;
  flex-shrink: 0;
}

.post-info {
  flex: 1;
}

.post-title {
  font-weight: 600;
  margin-bottom: 8px;
  color: #333;
  font-size: 16px;
}

.post-stats {
  display: flex;
  gap: 16px;
  font-size: 14px;
  color: #666;
}

/* 活跃用户样式 */
.active-users {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 16px;
}

.active-user-item {
  display: flex;
  align-items: center;
  padding: 16px;
  background: #f8f9fa;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s;
}

.active-user-item:hover {
  background: #e9ecef;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

.user-avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  margin-right: 12px;
  flex-shrink: 0;
}

.user-info {
  flex: 1;
}

.user-name {
  font-weight: 600;
  color: #333;
  margin-bottom: 4px;
}

.user-level {
  font-size: 12px;
  color: #409eff;
  background: #e8f4fd;
  padding: 2px 6px;
  border-radius: 4px;
  display: inline-block;
}

.user-exp {
  font-size: 12px;
  color: #666;
  text-align: right;
}

/* 推荐贴吧样式 */
.recommended-bars {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}

.recommended-bar-item {
  display: flex;
  align-items: center;
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
  transition: all 0.3s;
}

.recommended-bar-item:hover {
  background: #e9ecef;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

.bar-avatar {
  width: 60px;
  height: 60px;
  border-radius: 8px;
  margin-right: 16px;
  flex-shrink: 0;
}

.bar-info {
  flex: 1;
  margin-right: 16px;
}

.bar-name {
  font-weight: 600;
  color: #333;
  margin-bottom: 8px;
  font-size: 16px;
}

.bar-description {
  color: #666;
  font-size: 14px;
  line-height: 1.4;
  margin-bottom: 8px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.bar-stats {
  display: flex;
  gap: 12px;
  font-size: 12px;
  color: #999;
}

/* 搜索技巧样式 */
.search-tips {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px 40px;
}

.search-tips {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.tips-title {
  font-size: 20px;
  font-weight: 600;
  margin-bottom: 20px;
  color: #333;
  border-left: 4px solid #67c23a;
  padding-left: 12px;
}

.tips-content {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 16px;
}

.tip-item {
  padding: 16px;
  background: #f0f9ff;
  border-radius: 8px;
  border-left: 3px solid #409eff;
}

.tip-label {
  font-weight: 600;
  color: #409eff;
  display: block;
  margin-bottom: 4px;
}

.tip-text {
  color: #666;
  font-size: 14px;
  line-height: 1.4;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .page-title {
    font-size: 36px;
  }
  
  .page-subtitle {
    font-size: 16px;
  }
  
  .search-container {
    margin: -20px auto 30px;
  }
  
  .recommendation-section {
    padding: 20px;
    margin-bottom: 20px;
  }
  
  .active-users {
    grid-template-columns: 1fr;
  }
  
  .recommended-bars {
    grid-template-columns: 1fr;
  }
  
  .recommended-bar-item {
    padding: 16px;
  }
  
  .bar-avatar {
    width: 48px;
    height: 48px;
  }
  
  .tips-content {
    grid-template-columns: 1fr;
  }
  
  .hot-post-item {
    padding: 12px;
  }
  
  .post-rank {
    width: 28px;
    height: 28px;
    font-size: 14px;
    margin-right: 12px;
  }
  
  .post-title {
    font-size: 15px;
  }
}

@media (max-width: 480px) {
  .search-header {
    padding: 40px 0 30px;
  }
  
  .page-title {
    font-size: 28px;
  }
  
  .recommendation-section {
    padding: 16px;
  }
  
  .section-title {
    font-size: 18px;
  }
  
  .recommended-bar-item {
    flex-direction: column;
    text-align: center;
    gap: 12px;
  }
  
  .bar-info {
    margin-right: 0;
  }
}
</style>
