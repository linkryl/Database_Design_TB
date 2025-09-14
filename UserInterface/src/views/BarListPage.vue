<!--
贴吧列表页面
TreeHole开发组
2025-09-14
-->

<template>
  <div class="bar-list-container">
    <!-- 页面标题 -->
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">
          <span class="title-icon">🏠</span>
          贴吧广场
        </h1>
        <p class="page-subtitle">发现有趣的社区，找到志同道合的朋友</p>
      </div>
      
      <!-- 搜索和操作区 -->
      <div class="header-actions">
        <div class="search-bar">
          <el-input
            v-model="searchKeyword"
            placeholder="搜索贴吧..."
            class="search-input"
            @keyup.enter="performSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
          <el-button type="primary" @click="performSearch" :loading="searching">
            搜索
          </el-button>
        </div>
        
        <el-button 
          v-if="currentUserId"
          type="success" 
          class="create-bar-btn"
          @click="openCreateBarDialog"
        >
          <el-icon><Plus /></el-icon>
          创建贴吧
        </el-button>
      </div>
    </div>

    <!-- 贴吧列表 -->
    <div class="bars-content">
      <!-- 热门贴吧（仅首页显示） -->
      <div v-if="!isSearchMode && popularBars.length > 0" class="popular-section">
        <h2 class="section-title">
          <span class="section-icon">🔥</span>
          热门贴吧
        </h2>
        <div class="popular-bars-grid">
          <div 
            v-for="bar in popularBars" 
            :key="bar.barId" 
            class="popular-bar-card"
            @click="goToBarDetail(bar.barId)"
          >
            <div class="bar-avatar">
              <img :src="bar.avatarUrl || 'images/TreeHoleLogo.png'" :alt="bar.barName" />
            </div>
            <h3 class="bar-name">{{ bar.barName }}</h3>
            <p class="bar-followers">{{ bar.followedCount }} 关注</p>
          </div>
        </div>
      </div>

      <!-- 所有贴吧列表 -->
      <div class="all-bars-section">
        <h2 class="section-title">
          <span class="section-icon">📋</span>
          {{ isSearchMode ? `搜索结果 (${bars.length})` : '所有贴吧' }}
        </h2>
        
        <div v-if="loading" class="loading-container" v-loading="loading">
          <div style="height: 80px;">加载贴吧中...</div>
        </div>
        
        <div v-else-if="bars.length === 0" class="empty-container">
          <div class="empty-icon">🏗️</div>
          <div class="empty-text">
            {{ isSearchMode ? '没有找到相关贴吧' : '还没有贴吧' }}
          </div>
          <div class="empty-hint">
            {{ isSearchMode ? '试试其他关键词' : '快来创建第一个贴吧吧！' }}
          </div>
          <el-button 
            v-if="!isSearchMode && currentUserId" 
            type="primary" 
            @click="openCreateBarDialog"
            class="empty-action-btn"
          >
            立即创建
          </el-button>
        </div>
        
        <div v-else class="bars-grid">
          <div 
            v-for="bar in bars" 
            :key="bar.barId" 
            class="bar-card"
            @click="goToBarDetail(bar.barId)"
          >
            <div class="bar-header">
              <div class="bar-avatar-small">
                <img :src="bar.avatarUrl || '/images/TreeHoleLogo.png'" :alt="bar.barName" />
              </div>
              <div class="bar-info">
                <h3 class="bar-title">{{ bar.barName }}</h3>
                <div class="bar-stats">
                  <span class="stat">
                    <el-icon><User /></el-icon>
                    {{ bar.followedCount }} 关注
                  </span>
                  <span class="stat">
                    <el-icon><ChatDotSquare /></el-icon>
                    {{ bar.postCount }} 帖子
                  </span>
                </div>
              </div>
              <div class="bar-follow-area">
                <el-button 
                  v-if="currentUserId"
                  :type="bar.isFollowed ? 'info' : 'primary'"
                  size="small"
                  @click.stop="toggleFollowBar(bar)"
                  :loading="bar.followLoading"
                >
                  {{ bar.isFollowed ? '已关注' : '关注' }}
                </el-button>
              </div>
            </div>
            
            <div v-if="bar.description" class="bar-description">
              {{ bar.description }}
            </div>
            
            <div v-if="bar.tags" class="bar-tags">
              <el-tag 
                v-for="tag in bar.tags.split(',')" 
                :key="tag" 
                size="small" 
                class="bar-tag"
              >
                {{ tag.trim() }}
              </el-tag>
            </div>
            
            <div class="bar-footer">
              <span class="bar-owner">吧主：{{ bar.ownerName || '未知' }}</span>
              <span class="bar-time">{{ formatTime(bar.creationDate) }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 分页器 -->
      <div v-if="bars.length > 0" class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          :page-size="pageSize"
          :total="totalBars"
          layout="prev, pager, next"
          @current-change="handlePageChange"
        />
      </div>
    </div>

    <!-- 创建贴吧对话框 -->
    <el-dialog
      v-model="createBarDialogVisible"
      title="创建贴吧"
      width="600px"
      :close-on-click-modal="false"
    >
      <CreateBarForm @success="handleCreateSuccess" @cancel="createBarDialogVisible = false" />
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Search, Plus, User, ChatDotSquare } from '@element-plus/icons-vue'
import { getCurrentUserId } from '@/utils/auth'
import {
  getAllBars,
  getPopularBars,
  searchBars,
  followBar,
  unfollowBar,
  checkBarFollowStatus,
  getUserFollowedBarIds,
  type THBar,
  type THBarFollow
} from '@/api/index'
import CreateBarForm from '@/components/CreateBarForm.vue'

// 响应式数据
const router = useRouter()
const currentUserId = ref(getCurrentUserId() ? parseInt(getCurrentUserId()!) : null)

const loading = ref(true)
const searching = ref(false)
const searchKeyword = ref('')
const isSearchMode = ref(false)

const bars = ref<any[]>([])
const popularBars = ref<any[]>([])
const userFollowedBarIds = ref<number[]>([])

const currentPage = ref(1)
const pageSize = ref(20)
const totalBars = ref(0)

const createBarDialogVisible = ref(false)

// 时间格式化
const formatTime = (timestamp: string) => {
  if (!timestamp) return ''
  try {
    const date = new Date(timestamp)
    const now = new Date()
    const diff = Math.abs(now.getTime() - date.getTime())
    
    if (diff < 86400000) return '今天创建'
    if (diff < 604800000) return `${Math.floor(diff / 86400000)}天前创建`
    
    return date.toLocaleDateString('zh-CN', { month: '2-digit', day: '2-digit' }) + '创建'
  } catch (error) {
    return ''
  }
}

// 加载贴吧列表
const loadBars = async () => {
  try {
    loading.value = true
    const data = await getAllBars(currentPage.value, pageSize.value)
    bars.value = data || []
    totalBars.value = bars.value.length * 10 // 临时总数估算
    
    // 加载用户关注状态
    if (currentUserId.value) {
      await loadUserFollowStatus()
    }
  } catch (error: any) {
    console.error('加载贴吧列表失败:', error)
    
    if (error.code === 'ECONNABORTED') {
      ElMessage.error('后端服务器响应超时，请联系技术管理员检查服务器状态')
    } else if (error.response?.status === 500) {
      ElMessage.error('后端服务器内部错误，请联系技术管理员')
    } else {
      ElMessage.error('加载贴吧列表失败，请刷新重试')
    }
    
    // 设置空状态，避免无限加载
    bars.value = []
  } finally {
    loading.value = false
  }
}

// 加载热门贴吧
const loadPopularBars = async () => {
  try {
    const data = await getPopularBars(8)
    popularBars.value = data || []
  } catch (error) {
    console.error('加载热门贴吧失败:', error)
  }
}

// 加载用户关注状态
const loadUserFollowStatus = async () => {
  if (!currentUserId.value) return
  
  try {
    userFollowedBarIds.value = await getUserFollowedBarIds(currentUserId.value)
    
    // 更新贴吧关注状态
    bars.value.forEach(bar => {
      bar.isFollowed = userFollowedBarIds.value.includes(bar.barId)
      bar.followLoading = false
    })
    
    popularBars.value.forEach(bar => {
      bar.isFollowed = userFollowedBarIds.value.includes(bar.barId)
    })
  } catch (error) {
    console.error('加载用户关注状态失败:', error)
  }
}

// 搜索贴吧
const performSearch = async () => {
  if (!searchKeyword.value.trim()) {
    // 清空搜索，回到普通模式
    isSearchMode.value = false
    currentPage.value = 1
    await loadBars()
    return
  }
  
  try {
    searching.value = true
    isSearchMode.value = true
    
    const data = await searchBars(searchKeyword.value.trim())
    bars.value = data || []
    
    // 加载用户关注状态
    if (currentUserId.value) {
      await loadUserFollowStatus()
    }
  } catch (error) {
    console.error('搜索贴吧失败:', error)
    ElMessage.error('搜索贴吧失败')
  } finally {
    searching.value = false
  }
}

// 切换关注状态
const toggleFollowBar = async (bar: any) => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  bar.followLoading = true
  
  try {
    if (bar.isFollowed) {
      // 取消关注
      await unfollowBar(bar.barId, currentUserId.value)
      bar.isFollowed = false
      bar.followedCount = Math.max(0, bar.followedCount - 1)
      ElMessage.success(`已取消关注 ${bar.barName}`)
    } else {
      // 关注
      const followData: THBarFollow = {
        barId: bar.barId,
        userId: currentUserId.value,
        followTime: new Date().toISOString(),
        isActive: 1
      }
      
      await followBar(followData)
      bar.isFollowed = true
      bar.followedCount = bar.followedCount + 1
      ElMessage.success(`已关注 ${bar.barName}`)
    }
  } catch (error) {
    console.error('关注操作失败:', error)
    ElMessage.error('操作失败，请重试')
  } finally {
    bar.followLoading = false
  }
}

// 跳转到贴吧详情
const goToBarDetail = (barId: number) => {
  router.push(`/bar/${barId}`)
}

// 打开创建贴吧对话框
const openCreateBarDialog = () => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  createBarDialogVisible.value = true
}

// 处理创建成功
const handleCreateSuccess = () => {
  createBarDialogVisible.value = false
  ElMessage.success('贴吧创建成功！')
  // 重新加载列表
  currentPage.value = 1
  isSearchMode.value = false
  searchKeyword.value = ''
  loadBars()
}

// 分页处理
const handlePageChange = (page: number) => {
  currentPage.value = page
  if (isSearchMode.value) {
    performSearch()
  } else {
    loadBars()
  }
}

// 组件挂载时加载数据
onMounted(async () => {
  await Promise.all([
    loadBars(),
    loadPopularBars()
  ])
})
</script>

<style scoped>
.bar-list-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  padding: 20px 0;
}

/* 页面头部 */
.page-header {
  max-width: 1200px;
  margin: 0 auto 40px;
  padding: 0 20px;
}

.header-content {
  text-align: center;
  margin-bottom: 30px;
}

.page-title {
  font-size: 36px;
  font-weight: 700;
  color: #2c3e50;
  margin: 0 0 10px 0;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
}

.title-icon {
  font-size: 40px;
}

.page-subtitle {
  font-size: 16px;
  color: #7f8c8d;
  margin: 0;
}

.header-actions {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 20px;
  flex-wrap: wrap;
}

.search-bar {
  display: flex;
  gap: 12px;
  align-items: center;
}

.search-input {
  width: 300px;
}

.create-bar-btn {
  padding: 8px 20px;
  font-weight: 600;
}

/* 贴吧内容区 */
.bars-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

/* 热门贴吧区域 */
.popular-section {
  margin-bottom: 40px;
}

.section-title {
  font-size: 24px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 20px 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.section-icon {
  font-size: 26px;
}

.popular-bars-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 20px;
}

.popular-bar-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  text-align: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  cursor: pointer;
}

.popular-bar-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
}

.bar-avatar {
  width: 60px;
  height: 60px;
  margin: 0 auto 12px;
  border-radius: 50%;
  overflow: hidden;
}

.bar-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.bar-name {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.bar-followers {
  font-size: 14px;
  color: #7f8c8d;
  margin: 0;
}

/* 贴吧列表 */
.all-bars-section {
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
}

.loading-container {
  display: flex;
  justify-content: center;
  padding: 60px;
}

.empty-container {
  text-align: center;
  padding: 60px 20px;
}

.empty-icon {
  font-size: 64px;
  margin-bottom: 20px;
  opacity: 0.6;
}

.empty-text {
  font-size: 18px;
  color: #666;
  margin-bottom: 8px;
}

.empty-hint {
  font-size: 14px;
  color: #999;
  margin-bottom: 24px;
}

.empty-action-btn {
  padding: 12px 32px;
  font-size: 16px;
}

.bars-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 20px;
}

.bar-card {
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 12px;
  padding: 20px;
  transition: all 0.3s ease;
  cursor: pointer;
}

.bar-card:hover {
  background: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.bar-header {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  margin-bottom: 12px;
}

.bar-avatar-small {
  width: 48px;
  height: 48px;
  border-radius: 8px;
  overflow: hidden;
  flex-shrink: 0;
}

.bar-avatar-small img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.bar-info {
  flex: 1;
}

.bar-title {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px 0;
  line-height: 1.3;
}

.bar-stats {
  display: flex;
  gap: 16px;
}

.stat {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 14px;
  color: #666;
}

.bar-follow-area {
  flex-shrink: 0;
}

.bar-description {
  color: #555;
  line-height: 1.5;
  margin-bottom: 12px;
  overflow: hidden;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.bar-tags {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  margin-bottom: 12px;
}

.bar-tag {
  background: #e3f2fd !important;
  color: #1976d2 !important;
  border: none !important;
}

.bar-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  color: #999;
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid #eee;
}

.bar-owner {
  font-weight: 500;
}

.bar-time {
  font-style: italic;
}

/* 分页器 */
.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 40px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .page-title {
    font-size: 28px;
  }
  
  .header-actions {
    flex-direction: column;
    gap: 16px;
  }
  
  .search-input {
    width: 250px;
  }
  
  .popular-bars-grid {
    grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
    gap: 16px;
  }
  
  .bars-grid {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .bar-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  
  .bar-stats {
    flex-direction: column;
    gap: 8px;
  }
}
</style>
