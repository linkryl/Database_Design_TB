<!--
仿百度贴吧风格的首页
TreeHole开发组
-->

<template>
  <div class="tieba-home">
    <!-- 顶部导航栏 -->
    <div class="top-navbar">
      <div class="navbar-content">
        <div class="logo-section">
          <img src="/images/TreeHoleLogo.png" alt="TreeHole" class="logo">
          <span class="site-name">树洞论坛</span>
        </div>
        
        <div class="search-section">
          <div class="search-container">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索贴吧、帖子、用户..."
              class="main-search"
              @keyup.enter="performSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
            <el-button type="primary" @click="performSearch">搜索</el-button>
          </div>
          
          <!-- 热门搜索词 -->
          <div class="hot-searches">
            <span class="hot-label">热门：</span>
            <el-tag
              v-for="keyword in hotKeywords.slice(0, 6)"
              :key="keyword"
              size="small"
              effect="plain"
              @click="searchKeyword = keyword; performSearch()"
              class="hot-tag"
            >
              {{ keyword }}
            </el-tag>
          </div>
        </div>
        
        <div class="user-section">
          <div v-if="currentUserId" class="user-info">
            <el-dropdown @command="handleUserCommand">
              <div class="user-avatar-container">
                <img :src="userInfo?.avatarUrl || '/images/default-avatar.png'" class="user-avatar">
                <span class="user-name">{{ userInfo?.userName }}</span>
                <el-icon><ArrowDown /></el-icon>
              </div>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="profile">个人主页</el-dropdown-item>
                  <el-dropdown-item command="settings">设置</el-dropdown-item>
                  <el-dropdown-item command="messages">消息</el-dropdown-item>
                  <el-dropdown-item divided command="logout">退出登录</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
          <div v-else class="login-section">
            <el-button @click="$router.push('/login')">登录</el-button>
            <el-button type="primary" @click="$router.push('/register')">注册</el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="main-content">
      <!-- 左侧边栏 -->
      <div class="left-sidebar">
        <!-- 用户快速信息 -->
        <div v-if="currentUserId" class="user-quick-info">
          <div class="user-card">
            <img :src="userInfo?.avatarUrl || '/images/default-avatar.png'" class="avatar">
            <div class="user-details">
              <div class="username">{{ userInfo?.userName }}</div>
              <div class="user-level">
                <span class="level-badge">Lv.{{ userLevel }}</span>
                <span class="exp-text">{{ userInfo?.experiencePoints || 0 }}经验</span>
              </div>
            </div>
          </div>
          
          <div class="quick-actions">
            <el-button size="small" @click="$router.push('/PostNew')" type="primary" class="post-btn">
              📝 发帖
            </el-button>
            <el-button size="small" @click="checkIn" :disabled="checkedIn" class="checkin-btn">
              {{ checkedIn ? '✓ 已签到' : '📅 签到' }}
            </el-button>
          </div>
        </div>

        <!-- 我关注的贴吧 -->
        <div class="followed-bars-section">
          <h3 class="section-title">我关注的贴吧</h3>
          <div class="followed-bars">
            <div
              v-for="bar in followedBars.slice(0, 8)"
              :key="bar.barId"
              class="bar-item"
              @click="$router.push(`/bar/${bar.barId}`)"
            >
              <img :src="bar.avatarUrl || '/images/default-bar.png'" class="bar-avatar">
              <div class="bar-info">
                <div class="bar-name">{{ bar.barName }}</div>
                <div class="bar-stats">{{ bar.postCount }}帖</div>
              </div>
            </div>
          </div>
          <div class="more-bars">
            <el-button text @click="$router.push('/bars')">查看更多 ></el-button>
          </div>
        </div>

        <!-- 热门贴吧推荐 -->
        <div class="hot-bars-section">
          <h3 class="section-title">热门贴吧</h3>
          <div class="hot-bars">
            <div
              v-for="(bar, index) in hotBars.slice(0, 10)"
              :key="bar.barId"
              class="hot-bar-item"
              @click="$router.push(`/bar/${bar.barId}`)"
            >
              <span class="rank">{{ index + 1 }}</span>
              <img :src="bar.avatarUrl || '/images/default-bar.png'" class="bar-avatar">
              <div class="bar-info">
                <div class="bar-name">{{ bar.barName }}</div>
                <div class="bar-followers">{{ bar.followedCount }}关注</div>
              </div>
              <el-button size="small" @click.stop="followBar(bar)" :loading="bar.followLoading">
                {{ bar.isFollowed ? '已关注' : '关注' }}
              </el-button>
            </div>
          </div>
        </div>
      </div>

      <!-- 中央内容区 -->
      <div class="center-content">
        <!-- 顶部标签页 -->
        <div class="content-tabs">
          <el-tabs v-model="activeTab" @tab-change="handleTabChange">
            <el-tab-pane label="推荐" name="recommend">
              <div class="tab-icon">🔥</div>
            </el-tab-pane>
            <el-tab-pane label="关注" name="following">
              <div class="tab-icon">👥</div>
            </el-tab-pane>
            <el-tab-pane label="最新" name="latest">
              <div class="tab-icon">🕐</div>
            </el-tab-pane>
            <el-tab-pane label="精华" name="featured">
              <div class="tab-icon">⭐</div>
            </el-tab-pane>
          </el-tabs>
        </div>

        <!-- 帖子列表 -->
        <div class="posts-container">
          <!-- 置顶帖子 -->
          <div v-if="stickyPosts.length > 0" class="sticky-posts">
            <div class="sticky-header">
              <span class="sticky-icon">📌</span>
              <span class="sticky-text">置顶帖子</span>
            </div>
            <PostDetailCard
              v-for="postId in stickyPosts"
              :key="`sticky-${postId}`"
              :post-id="postId"
              class="sticky-post"
              @post-deleted="handlePostDeleted"
              @post-reported="handlePostReported"
            />
          </div>

          <!-- 普通帖子 -->
          <div class="normal-posts">
            <PostDetailCard
              v-for="postId in currentPosts"
              :key="postId"
              :post-id="postId"
              @post-deleted="handlePostDeleted"
              @post-reported="handlePostReported"
            />
          </div>

          <!-- 加载更多 -->
          <div v-if="hasMore" class="load-more-section">
            <el-button @click="loadMorePosts" :loading="loadingMore" size="large">
              加载更多帖子
            </el-button>
          </div>

          <!-- 无更多内容提示 -->
          <div v-else-if="currentPosts.length > 0" class="no-more-posts">
            <div class="no-more-content">
              <span class="no-more-icon">🏁</span>
              <span class="no-more-text">已经到底了，没有更多内容</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧边栏 -->
      <div class="right-sidebar">
        <!-- 签到日历 -->
        <div class="checkin-calendar">
          <h3 class="section-title">签到日历</h3>
          <div class="calendar-grid">
            <div
              v-for="day in 7"
              :key="day"
              class="calendar-day"
              :class="{ 
                checked: isCheckedDay(day),
                today: isToday(day)
              }"
            >
              <div class="day-number">{{ getDayNumber(day) }}</div>
              <div class="day-status">{{ getCheckinStatus(day) }}</div>
            </div>
          </div>
          <el-button 
            v-if="!checkedIn" 
            @click="checkIn" 
            type="primary" 
            size="small" 
            class="checkin-btn"
          >
            立即签到
          </el-button>
        </div>

        <!-- 实时热榜 -->
        <div class="hot-ranking">
          <h3 class="section-title">
            <span class="title-icon">🔥</span>
            实时热榜
          </h3>
          <div class="hot-list">
            <div
              v-for="(post, index) in hotPosts"
              :key="post.postId"
              class="hot-item"
              @click="$router.push(`/PostPage/${post.postId}`)"
            >
              <div class="hot-rank" :class="`rank-${index + 1}`">{{ index + 1 }}</div>
              <div class="hot-content">
                <div class="hot-title">{{ post.title }}</div>
                <div class="hot-stats">
                  <span class="hot-count">{{ post.heatScore }}热度</span>
                  <span class="hot-source">{{ post.barName || '树洞' }}</span>
                </div>
              </div>
              <div class="hot-trend">
                <span class="trend-icon">📈</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 今日话题 -->
        <div class="daily-topics">
          <h3 class="section-title">今日话题</h3>
          <div class="topics-list">
            <div
              v-for="topic in dailyTopics"
              :key="topic.id"
              class="topic-item"
              @click="searchTopic(topic.name)"
            >
              <div class="topic-icon">{{ topic.icon }}</div>
              <div class="topic-info">
                <div class="topic-name">#{{ topic.name }}</div>
                <div class="topic-desc">{{ topic.description }}</div>
                <div class="topic-stats">{{ topic.postCount }}讨论</div>
              </div>
            </div>
          </div>
        </div>

        <!-- 论坛统计 -->
        <div class="forum-stats">
          <h3 class="section-title">论坛数据</h3>
          <div class="stats-grid">
            <div class="stat-item">
              <div class="stat-number">{{ forumStats.totalUsers }}</div>
              <div class="stat-label">注册用户</div>
            </div>
            <div class="stat-item">
              <div class="stat-number">{{ forumStats.totalPosts }}</div>
              <div class="stat-label">帖子总数</div>
            </div>
            <div class="stat-item">
              <div class="stat-number">{{ forumStats.onlineUsers }}</div>
              <div class="stat-label">在线用户</div>
            </div>
            <div class="stat-item">
              <div class="stat-number">{{ forumStats.totalBars }}</div>
              <div class="stat-label">贴吧数量</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 浮动发帖按钮 -->
    <el-button
      class="floating-post-btn"
      type="primary"
      size="large"
      @click="$router.push('/PostNew')"
      v-if="currentUserId"
    >
      <el-icon><Edit /></el-icon>
      发帖
    </el-button>

    <!-- 回到顶部 -->
    <el-backtop :right="100" :bottom="100" />
  </div>
</template>

<script setup lang='ts'>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Search, ArrowDown, Edit } from '@element-plus/icons-vue'
import PostDetailCard from '@/components/PostDetailCard.vue'
import { getCurrentUserId } from '@/utils/auth'
import {
  getTreeholePostIds,
  getUserById,
  getUserFollowedBars,
  getPopularBars,
  comprehensiveSearch
} from '@/api/index'

// 路由
const router = useRouter()

// 响应式数据
const searchKeyword = ref('')
const activeTab = ref('recommend')
const currentUserId = ref(getCurrentUserId() ? parseInt(getCurrentUserId()!) : null)
const userInfo = ref<any>(null)
const userLevel = ref(1)
const checkedIn = ref(false)

// 帖子相关
const currentPosts = ref<number[]>([])
const stickyPosts = ref<number[]>([])
const hasMore = ref(true)
const loadingMore = ref(false)

// 贴吧相关
const followedBars = ref<any[]>([])
const hotBars = ref<any[]>([])

// 搜索和推荐
const hotKeywords = ref(['树洞论坛', '日常分享', '技术讨论', '学习交流', '生活感悟', '校园生活'])
const hotPosts = ref<any[]>([])
const dailyTopics = ref([
  { id: 1, name: '今日分享', icon: '📝', description: '分享你的日常生活', postCount: 156 },
  { id: 2, name: '学习打卡', icon: '📚', description: '一起学习进步', postCount: 89 },
  { id: 3, name: '技术交流', icon: '💻', description: '技术问题讨论', postCount: 234 },
  { id: 4, name: '校园生活', icon: '🏫', description: '校园趣事分享', postCount: 178 }
])

// 论坛统计
const forumStats = ref({
  totalUsers: 0,
  totalPosts: 0,
  onlineUsers: 0,
  totalBars: 0
})

// 计算属性
const userLevel = computed(() => {
  if (!userInfo.value?.experiencePoints) return 1
  const exp = userInfo.value.experiencePoints
  if (exp < 100) return 1
  if (exp < 300) return 2
  if (exp < 700) return 3
  if (exp < 1500) return 4
  if (exp < 3000) return 5
  return Math.min(20, Math.floor(exp / 1000) + 5)
})

// 组件挂载时初始化
onMounted(async () => {
  await Promise.all([
    loadUserInfo(),
    loadPosts(),
    loadFollowedBars(),
    loadHotBars(),
    loadHotPosts(),
    loadForumStats(),
    checkTodayCheckIn()
  ])
})

// 加载用户信息
const loadUserInfo = async () => {
  if (!currentUserId.value) return
  
  try {
    userInfo.value = await getUserById(currentUserId.value)
  } catch (error) {
    console.error('加载用户信息失败:', error)
  }
}

// 加载帖子 - 现在只显示树洞社区的帖子，过滤举报的帖子
const loadPosts = async () => {
  try {
    const data = await getTreeholePostIds(20, currentUserId.value || undefined)
    
    // 分离置顶帖子和普通帖子
    // 这里需要根据实际的帖子数据结构来判断是否置顶
    stickyPosts.value = [] // 暂时为空，需要后端支持
    currentPosts.value = data || []
    
  } catch (error) {
    console.error('加载树洞社区帖子失败:', error)
  }
}

// 加载关注的贴吧
const loadFollowedBars = async () => {
  if (!currentUserId.value) return
  
  try {
    followedBars.value = await getUserFollowedBars(currentUserId.value)
  } catch (error) {
    console.error('加载关注贴吧失败:', error)
  }
}

// 加载热门贴吧
const loadHotBars = async () => {
  try {
    const data = await getPopularBars(10)
    hotBars.value = data.map((bar: any) => ({
      ...bar,
      followLoading: false,
      isFollowed: false
    }))
  } catch (error) {
    console.error('加载热门贴吧失败:', error)
  }
}

// 加载热门帖子
const loadHotPosts = async () => {
  try {
    // 模拟热门帖子数据，实际应该从后端API获取
    hotPosts.value = Array.from({ length: 10 }, (_, i) => ({
      postId: i + 1,
      title: `热门帖子标题 ${i + 1}`,
      heatScore: Math.floor(Math.random() * 10000),
      barName: i % 3 === 0 ? '技术讨论' : i % 3 === 1 ? '生活分享' : null
    }))
  } catch (error) {
    console.error('加载热门帖子失败:', error)
  }
}

// 加载论坛统计
const loadForumStats = async () => {
  try {
    // 这里应该从后端API获取实际统计数据
    forumStats.value = {
      totalUsers: Math.floor(Math.random() * 10000) + 5000,
      totalPosts: Math.floor(Math.random() * 50000) + 20000,
      onlineUsers: Math.floor(Math.random() * 500) + 100,
      totalBars: Math.floor(Math.random() * 1000) + 500
    }
  } catch (error) {
    console.error('加载论坛统计失败:', error)
  }
}

// 检查今日签到状态
const checkTodayCheckIn = async () => {
  if (!currentUserId.value) return
  
  try {
    // 这里应该调用API检查签到状态
    checkedIn.value = false // 模拟数据
  } catch (error) {
    console.error('检查签到状态失败:', error)
  }
}

// 标签页切换
const handleTabChange = async (tabName: string) => {
  activeTab.value = tabName
  
  // 根据不同标签加载不同内容
  switch (tabName) {
    case 'recommend':
      // 加载推荐帖子
      await loadRecommendedPosts()
      break
    case 'following':
      // 加载关注用户的帖子
      await loadFollowingPosts()
      break
    case 'latest':
      // 加载最新帖子
      await loadPosts()
      break
    case 'featured':
      // 加载精华帖子
      await loadFeaturedPosts()
      break
  }
}

// 搜索功能
const performSearch = () => {
  if (!searchKeyword.value.trim()) return
  
  router.push({
    path: '/search',
    query: { q: searchKeyword.value.trim() }
  })
}

// 搜索话题
const searchTopic = (topicName: string) => {
  router.push({
    path: '/search',
    query: { q: `#${topicName}` }
  })
}

// 签到功能
const checkIn = async () => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  try {
    // 调用签到API
    checkedIn.value = true
    ElMessage.success('签到成功！获得5经验值')
    
    // 刷新用户信息
    await loadUserInfo()
  } catch (error) {
    console.error('签到失败:', error)
    ElMessage.error('签到失败，请重试')
  }
}

// 关注贴吧
const followBar = async (bar: any) => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  bar.followLoading = true
  
  try {
    if (bar.isFollowed) {
      // 取消关注逻辑
      bar.isFollowed = false
      bar.followedCount--
      ElMessage.success('已取消关注')
    } else {
      // 关注逻辑
      bar.isFollowed = true
      bar.followedCount++
      ElMessage.success('关注成功')
    }
  } catch (error) {
    console.error('关注操作失败:', error)
    ElMessage.error('操作失败')
  } finally {
    bar.followLoading = false
  }
}

// 用户下拉菜单操作
const handleUserCommand = (command: string) => {
  switch (command) {
    case 'profile':
      router.push(`/profile/${currentUserId.value}`)
      break
    case 'settings':
      router.push('/settings')
      break
    case 'messages':
      router.push('/messages')
      break
    case 'logout':
      logout()
      break
  }
}

// 退出登录
const logout = () => {
  localStorage.removeItem('jwtToken')
  localStorage.removeItem('currentUserId')
  localStorage.removeItem('userRole')
  localStorage.removeItem('isAdmin')
  
  ElMessage.success('已退出登录')
  router.push('/login')
}

// 加载更多帖子 - 树洞社区专属，过滤举报的帖子
const loadMorePosts = async () => {
  if (loadingMore.value) return
  
  loadingMore.value = true
  
  try {
    // 这里应该实现分页加载逻辑
    const newPosts = await getTreeholePostIds(20, currentUserId.value || undefined) // 使用树洞专属API，过滤举报的帖子
    currentPosts.value.push(...newPosts)
    
    if (newPosts.length < 20) {
      hasMore.value = false
    }
  } catch (error) {
    console.error('加载更多树洞社区帖子失败:', error)
    ElMessage.error('加载失败')
  } finally {
    loadingMore.value = false
  }
}

// 辅助方法
const isCheckedDay = (day: number) => {
  // 模拟签到状态，实际应该从API获取
  return day <= 3
}

const isToday = (day: number) => {
  return day === 4 // 模拟今天是第4天
}

const getDayNumber = (day: number) => {
  const today = new Date()
  const date = new Date(today)
  date.setDate(today.getDate() - (7 - day))
  return date.getDate()
}

const getCheckinStatus = (day: number) => {
  if (isToday(day)) return '今日'
  if (isCheckedDay(day)) return '✓'
  return ''
}

// 处理帖子删除事件
const handlePostDeleted = (postId: number) => {
  console.log('📨 主页：接收到帖子删除事件，帖子ID:', postId)
  console.log('🔍 删除前普通帖子列表:', currentPosts.value.slice(0, 5))
  console.log('🔍 删除前置顶帖子列表:', stickyPosts.value)
  
  // 从列表中移除删除的帖子
  const oldCurrentLength = currentPosts.value.length
  const oldStickyLength = stickyPosts.value.length
  
  currentPosts.value = currentPosts.value.filter(id => id !== postId)
  stickyPosts.value = stickyPosts.value.filter(id => id !== postId)
  
  const newCurrentLength = currentPosts.value.length
  const newStickyLength = stickyPosts.value.length
  
  console.log(`✅ 主页：帖子 ${postId} 已删除，普通帖子 ${oldCurrentLength}→${newCurrentLength}，置顶帖子 ${oldStickyLength}→${newStickyLength}`)
}

// 处理帖子举报事件
const handlePostReported = (postId: number) => {
  console.log('📨 主页：接收到帖子举报事件，帖子ID:', postId)
  console.log('🔍 举报前普通帖子列表:', currentPosts.value.slice(0, 5))
  console.log('🔍 举报前置顶帖子列表:', stickyPosts.value)
  
  // 从列表中移除举报的帖子
  const oldCurrentLength = currentPosts.value.length
  const oldStickyLength = stickyPosts.value.length
  
  currentPosts.value = currentPosts.value.filter(id => id !== postId)
  stickyPosts.value = stickyPosts.value.filter(id => id !== postId)
  
  const newCurrentLength = currentPosts.value.length
  const newStickyLength = stickyPosts.value.length
  
  console.log(`✅ 主页：帖子 ${postId} 已举报，普通帖子 ${oldCurrentLength}→${newCurrentLength}，置顶帖子 ${oldStickyLength}→${newStickyLength}`)
}

// 其他加载方法的模拟实现
const loadRecommendedPosts = async () => {
  // 实现推荐算法 - 树洞社区推荐
  await loadPosts()
}

const loadFollowingPosts = async () => {
  // 加载关注用户的帖子 - 限制在树洞社区
  await loadPosts()
}

const loadFeaturedPosts = async () => {
  // 加载精华帖子 - 限制在树洞社区
  await loadPosts()
}
</script>

<style scoped>
.tieba-home {
  min-height: 100vh;
  background: #f5f6fa;
}

/* 顶部导航栏 */
.top-navbar {
  background: white;
  border-bottom: 1px solid #e4e7ed;
  position: sticky;
  top: 0;
  z-index: 100;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.navbar-content {
  max-width: 1400px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  padding: 12px 20px;
  gap: 24px;
}

.logo-section {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.logo {
  width: 32px;
  height: 32px;
}

.site-name {
  font-size: 20px;
  font-weight: 600;
  color: #409eff;
}

.search-section {
  flex: 1;
  max-width: 600px;
}

.search-container {
  display: flex;
  gap: 8px;
  margin-bottom: 8px;
}

.main-search {
  flex: 1;
}

.hot-searches {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.hot-label {
  font-size: 12px;
  color: #999;
}

.hot-tag {
  cursor: pointer;
  transition: all 0.2s;
}

.hot-tag:hover {
  background: #409eff;
  color: white;
}

.user-section {
  flex-shrink: 0;
}

.user-avatar-container {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
  transition: background-color 0.2s;
}

.user-avatar-container:hover {
  background: #f0f0f0;
}

.user-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.user-name {
  font-size: 14px;
  font-weight: 500;
}

/* 主要内容区域 */
.main-content {
  max-width: 1400px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 240px 1fr 280px;
  gap: 20px;
  padding: 20px;
  align-items: start;
}

/* 左侧边栏 */
.left-sidebar {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.user-quick-info {
  background: white;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.user-card {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
}

.user-details .username {
  font-weight: 600;
  margin-bottom: 4px;
}

.user-level {
  display: flex;
  align-items: center;
  gap: 8px;
}

.level-badge {
  background: linear-gradient(135deg, #409eff, #67c23a);
  color: white;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.exp-text {
  font-size: 12px;
  color: #999;
}

.quick-actions {
  display: flex;
  gap: 8px;
}

.post-btn, .checkin-btn {
  flex: 1;
}

/* 关注的贴吧 */
.followed-bars-section, .hot-bars-section {
  background: white;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.section-title {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 12px;
  color: #333;
  display: flex;
  align-items: center;
  gap: 8px;
}

.followed-bars, .hot-bars {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.bar-item, .hot-bar-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px;
  border-radius: 6px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.bar-item:hover, .hot-bar-item:hover {
  background: #f0f0f0;
}

.bar-avatar {
  width: 32px;
  height: 32px;
  border-radius: 6px;
}

.bar-info {
  flex: 1;
  min-width: 0;
}

.bar-name {
  font-size: 14px;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.bar-stats, .bar-followers {
  font-size: 12px;
  color: #999;
}

.rank {
  width: 20px;
  text-align: center;
  font-size: 12px;
  font-weight: 600;
  color: #999;
}

/* 中央内容区 */
.center-content {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  overflow: hidden;
}

.content-tabs {
  border-bottom: 1px solid #e4e7ed;
}

.content-tabs :deep(.el-tabs__header) {
  margin: 0;
  padding: 0 20px;
}

.content-tabs :deep(.el-tabs__item) {
  padding: 16px 20px;
  font-size: 16px;
  font-weight: 500;
}

.posts-container {
  padding: 0 20px 20px;
}

.sticky-posts {
  margin-bottom: 20px;
}

.sticky-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 0;
  font-size: 14px;
  color: #f56c6c;
  font-weight: 600;
  border-bottom: 1px solid #f0f0f0;
  margin-bottom: 12px;
}

.sticky-post {
  border-left: 3px solid #f56c6c;
}

.load-more-section {
  text-align: center;
  margin: 30px 0;
}

.no-more-posts {
  text-align: center;
  padding: 30px;
  color: #999;
}

.no-more-content {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

/* 右侧边栏 */
.right-sidebar {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.checkin-calendar, .hot-ranking, .daily-topics, .forum-stats {
  background: white;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

/* 签到日历 */
.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 4px;
  margin-bottom: 12px;
}

.calendar-day {
  text-align: center;
  padding: 8px 4px;
  border-radius: 4px;
  font-size: 12px;
  transition: all 0.2s;
}

.calendar-day.checked {
  background: #67c23a;
  color: white;
}

.calendar-day.today {
  background: #409eff;
  color: white;
  font-weight: 600;
}

.checkin-btn {
  width: 100%;
}

/* 热榜 */
.hot-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.hot-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px;
  border-radius: 6px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.hot-item:hover {
  background: #f0f0f0;
}

.hot-rank {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
  color: white;
  flex-shrink: 0;
}

.hot-rank.rank-1 { background: #ff4757; }
.hot-rank.rank-2 { background: #ff6b7a; }
.hot-rank.rank-3 { background: #ff7f8a; }
.hot-rank { background: #c7c7c7; }

.hot-content {
  flex: 1;
  min-width: 0;
}

.hot-title {
  font-size: 14px;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin-bottom: 2px;
}

.hot-stats {
  display: flex;
  gap: 8px;
  font-size: 12px;
  color: #999;
}

/* 今日话题 */
.topics-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.topic-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.topic-item:hover {
  background: #e9ecef;
  transform: translateY(-1px);
}

.topic-icon {
  font-size: 24px;
}

.topic-info {
  flex: 1;
}

.topic-name {
  font-weight: 600;
  color: #409eff;
  margin-bottom: 4px;
}

.topic-desc {
  font-size: 12px;
  color: #666;
  margin-bottom: 4px;
}

.topic-stats {
  font-size: 12px;
  color: #999;
}

/* 论坛统计 */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}

.stat-item {
  text-align: center;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 6px;
}

.stat-number {
  font-size: 18px;
  font-weight: 600;
  color: #409eff;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 12px;
  color: #666;
}

/* 浮动发帖按钮 */
.floating-post-btn {
  position: fixed;
  bottom: 80px;
  right: 40px;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  box-shadow: 0 4px 12px rgba(64, 158, 255, 0.4);
  z-index: 1000;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .main-content {
    grid-template-columns: 200px 1fr 240px;
    gap: 16px;
    padding: 16px;
  }
}

@media (max-width: 992px) {
  .main-content {
    grid-template-columns: 1fr 240px;
  }
  
  .left-sidebar {
    display: none;
  }
}

@media (max-width: 768px) {
  .main-content {
    grid-template-columns: 1fr;
    padding: 12px;
  }
  
  .right-sidebar {
    display: none;
  }
  
  .navbar-content {
    flex-direction: column;
    gap: 12px;
    padding: 12px;
  }
  
  .search-section {
    width: 100%;
    max-width: none;
  }
}
</style>
