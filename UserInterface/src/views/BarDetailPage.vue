<!--
贴吧详情页面
TreeHole开发组
2025-09-14
-->

<template>
  <div class="bar-detail-container" v-loading="loading">
    <!-- 贴吧头部信息 -->
    <div class="bar-header">
      <div class="bar-cover" :style="{ backgroundImage: `url(${barInfo?.coverUrl || 'public/images/BackgroundImage.jpg'})` }">
        <div class="cover-overlay"></div>
        <div class="bar-main-info">
          <div class="bar-avatar-large">
            <img :src="barInfo?.avatarUrl || '/images/TreeHoleLogo.png'" :alt="barInfo?.barName" />
          </div>
          <div class="bar-details">
            <h1 class="bar-name">{{ barInfo?.barName || '未知贴吧' }}</h1>
            <p class="bar-description">{{ barInfo?.description || '暂无描述' }}</p>
            <div class="bar-stats">
              <div class="stat-item">
                <span class="stat-number">{{ barInfo?.followedCount || 0 }}</span>
                <span class="stat-label">关注者</span>
              </div>
              <div class="stat-item">
                <span class="stat-number">{{ barInfo?.postCount || 0 }}</span>
                <span class="stat-label">帖子</span>
              </div>
              <div class="stat-item">
                <span class="stat-number">{{ getBarAge() }}</span>
                <span class="stat-label">天前创建</span>
              </div>
            </div>
          </div>
          <div class="bar-actions">
            <!-- 关注按钮 -->
            <el-button 
              v-if="currentUserId && currentUserId !== barInfo?.ownerId"
              :type="isFollowed ? 'info' : 'primary'"
              size="large"
              @click="toggleFollow"
              :loading="followLoading"
            >
              {{ isFollowed ? '已关注' : '关注贴吧' }}
            </el-button>
            
            <!-- 管理按钮（仅吧主可见） -->
            <el-button 
              v-if="currentUserId === barInfo?.ownerId"
              type="warning"
              size="large"
              @click="openManageDialog"
            >
              <el-icon><Setting /></el-icon>
              管理贴吧
            </el-button>

            <!-- 吧主选举入口（所有用户可见） -->
            <el-button 
              type="primary"
              size="large"
              @click="router.push(`/bar/${barId}/election`)"
            >
              吧主选举
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 贴吧内容区 -->
    <div class="bar-content">
      <div class="content-wrapper">
        <!-- 侧边栏 -->
        <div class="sidebar">
          <!-- 贴吧信息卡片 -->
          <div class="info-card">
            <h3 class="card-title">
              <el-icon><InfoFilled /></el-icon>
              贴吧信息
            </h3>
            <div class="info-list">
              <div class="info-item">
                <span class="info-label">吧主</span>
                <span class="info-value">{{ ownerInfo?.userName || '未知' }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">创建时间</span>
                <span class="info-value">{{ formatDate(barInfo?.creationDate) }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">状态</span>
                <span class="info-value">{{ getStatusText(barInfo?.status) }}</span>
              </div>
            </div>
          </div>

          <!-- 贴吧规则 -->
          <div v-if="barInfo?.rules" class="rules-card">
            <h3 class="card-title">
              <el-icon><Document /></el-icon>
              贴吧规则
            </h3>
            <div class="rules-content">
              {{ barInfo.rules }}
            </div>
          </div>

          <!-- 贴吧标签 -->
          <div v-if="barInfo?.tags" class="tags-card">
            <h3 class="card-title">
              <el-icon><Collection /></el-icon>
              相关标签
            </h3>
            <div class="tags-content">
              <el-tag 
                v-for="tag in barInfo.tags.split(',')" 
                :key="tag" 
                class="bar-tag"
              >
                {{ tag.trim() }}
              </el-tag>
            </div>
          </div>
        </div>

        <!-- 主内容区 -->
        <div class="main-content">
          <!-- 功能标签页 -->
          <el-tabs v-model="activeTab" class="content-tabs">
            <el-tab-pane label="📝 帖子" name="posts">
              <div class="posts-section">
                <div class="posts-header">
                  <h3>贴吧帖子 ({{ barPosts.length }})</h3>
                  <el-button v-if="currentUserId" type="primary" @click="createPost">
                    发表帖子
                  </el-button>
                </div>
                
                <div v-if="loadingPosts" class="loading-posts" v-loading="loadingPosts">
                  <div style="height: 60px;">加载帖子中...</div>
                </div>
                
                <div v-else-if="barPosts.length === 0" class="empty-posts">
                  <div class="empty-icon">📝</div>
                  <div class="empty-text">这个贴吧还没有帖子</div>
                  <div class="empty-hint">快来发表第一个帖子吧！</div>
                  <el-button v-if="currentUserId" type="primary" @click="createPost" class="empty-action-btn">
                    立即发帖
                  </el-button>
                </div>
                
                <div v-else class="posts-list">
                  <div 
                    v-for="postId in barPosts" 
                    :key="postId" 
                    class="post-item"
                  >
                    <PostDetailCard 
                      :post-id="postId" 
                      @post-deleted="handlePostDeleted"
                      @post-reported="handlePostReported"
                    />
                  </div>
                </div>
              </div>
            </el-tab-pane>
            
            <el-tab-pane label="👥 成员" name="members">
              <div class="members-section">
                <div class="members-header">
                  <h3>贴吧成员 ({{ barInfo?.followedCount || 0 }})</h3>
                </div>
                
                <div v-if="loadingMembers" class="loading-members" v-loading="loadingMembers">
                  <div style="height: 60px;">加载成员中...</div>
                </div>
                
                <div v-else-if="members.length === 0" class="empty-members">
                  <div class="empty-icon">👥</div>
                  <div class="empty-text">还没有成员关注这个贴吧</div>
                </div>
                
                <div v-else class="members-list">
                  <div 
                    v-for="member in members" 
                    :key="member.userId" 
                    class="member-item"
                  >
                    <div class="member-avatar">
                      <img src="/images/GitHubLogo.png" :alt="`用户${member.userId}`" />
                    </div>
                    <div class="member-info">
                      <div class="member-id">用户 {{ member.userId }}</div>
                      <div class="member-join-time">{{ formatTime(member.followTime) }} 加入</div>
                    </div>
                  </div>
                </div>
              </div>
            </el-tab-pane>
          </el-tabs>
        </div>
      </div>
    </div>

    <!-- 管理对话框 -->
    <el-dialog
      v-model="manageDialogVisible"
      title="管理贴吧"
      width="500px"
      :close-on-click-modal="false"
    >
      <div class="manage-options">
        <el-button type="primary" @click="editBar" class="manage-btn">
          <el-icon><Edit /></el-icon>
          编辑贴吧信息
        </el-button>
        
        <el-button 
          v-if="barInfo?.status === 0"
          type="warning" 
          @click="archiveBar"
          class="manage-btn"
        >
          <el-icon><FolderOpened /></el-icon>
          归档贴吧
        </el-button>
        
        <el-button 
          v-if="barInfo?.status === 1"
          type="success" 
          @click="unarchiveBar"
          class="manage-btn"
        >
          <el-icon><FolderOpened /></el-icon>
          取消归档
        </el-button>
        
        <el-button 
          type="danger" 
          @click="confirmDissolveBar"
          class="manage-btn"
        >
          <el-icon><Delete /></el-icon>
          解散贴吧
        </el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { 
  Plus, Setting, InfoFilled, Document, Collection, Edit, 
  FolderOpened, Delete 
} from '@element-plus/icons-vue'
import { getCurrentUserId } from '@/utils/auth'
import {
  getBarById,
  getUserById,
  followBar,
  unfollowBar,
  checkBarFollowStatus,
  getBarFollowers,
  updateBarStatus,
  getBarPostIds,
  type THBar,
  type THBarFollow
} from '@/api/index'
import PostDetailCard from '@/components/PostDetailCard.vue'

// 路由和用户信息
const route = useRoute()
const router = useRouter()
const barId = parseInt(route.params.id as string)
const currentUserId = ref(getCurrentUserId() ? parseInt(getCurrentUserId()!) : null)

// 响应式数据
const loading = ref(true)
const followLoading = ref(false)
const loadingMembers = ref(false)
const loadingPosts = ref(false)
const manageDialogVisible = ref(false)

const barInfo = ref<any>(null)
const ownerInfo = ref<any>(null)
const isFollowed = ref(false)
const members = ref<any[]>([])
const barPosts = ref<number[]>([])

const activeTab = ref('posts')

// 计算贴吧创建天数
const getBarAge = () => {
  if (!barInfo.value?.creationDate) return 0
  try {
    const created = new Date(barInfo.value.creationDate)
    const now = new Date()
    const diff = Math.abs(now.getTime() - created.getTime())
    return Math.floor(diff / (1000 * 60 * 60 * 24))
  } catch {
    return 0
  }
}

// 格式化日期
const formatDate = (timestamp: string) => {
  if (!timestamp) return '未知'
  try {
    const date = new Date(timestamp)
    return date.toLocaleDateString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    })
  } catch {
    return '未知'
  }
}

// 格式化时间
const formatTime = (timestamp: string) => {
  if (!timestamp) return '未知时间'
  try {
    const date = new Date(timestamp)
    return date.toLocaleDateString('zh-CN', {
      month: '2-digit',
      day: '2-digit'
    })
  } catch {
    return '未知时间'
  }
}

// 获取状态文本
const getStatusText = (status?: number) => {
  switch (status) {
    case 0: return '正常'
    case 1: return '已归档'
    case 2: return '已解散'
    default: return '未知'
  }
}

// 加载贴吧详情
const loadBarDetail = async () => {
  try {
    loading.value = true
    
    // 获取贴吧信息
    const barData = await getBarById(barId)
    barInfo.value = barData
    
    // 获取吧主信息
    if (barData.ownerId) {
      try {
        const ownerData = await getUserById(barData.ownerId)
        ownerInfo.value = ownerData
      } catch (error) {
        console.error('获取吧主信息失败:', error)
        ownerInfo.value = { userName: '未知用户' }
      }
    }
    
    // 检查关注状态
    if (currentUserId.value) {
      await checkFollowStatus()
    }
    
  } catch (error) {
    console.error('加载贴吧详情失败:', error)
    ElMessage.error('加载贴吧详情失败')
    router.push('/bars')
  } finally {
    loading.value = false
  }
}

// 检查关注状态
const checkFollowStatus = async () => {
  if (!currentUserId.value) return
  
  try {
    isFollowed.value = await checkBarFollowStatus(barId, currentUserId.value)
  } catch (error) {
    isFollowed.value = false
  }
}

// 加载成员列表
const loadMembers = async () => {
  try {
    loadingMembers.value = true
    const membersData = await getBarFollowers(barId, 1, 20)
    members.value = membersData || []
  } catch (error) {
    console.error('加载成员列表失败:', error)
    members.value = []
  } finally {
    loadingMembers.value = false
  }
}

// 加载贴吧专属帖子，过滤举报的帖子
const loadBarPosts = async () => {
  try {
    loadingPosts.value = true
    
    // 使用贴吧专属API，只显示属于当前贴吧的帖子，过滤掉举报的帖子
    const barPostIds = await getBarPostIds(barId, 20, currentUserId.value || undefined)
    barPosts.value = barPostIds || []
    
    console.log(`贴吧${barId}专属帖子:`, barPosts.value)
    
    if (barPosts.value.length === 0) {
      console.log('🏠 贴吧暂无帖子')
    } else {
      console.log(`🏠 显示${barPosts.value.length}个贴吧专属帖子`)
    }
  } catch (error) {
    console.error('加载贴吧帖子失败:', error)
    // 如果获取失败，尝试显示空数组而不是报错
    barPosts.value = []
  } finally {
    loadingPosts.value = false
  }
}

// 切换关注状态
const toggleFollow = async () => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  followLoading.value = true
  
  try {
    if (isFollowed.value) {
      // 取消关注
      await unfollowBar(barId, currentUserId.value)
      isFollowed.value = false
      if (barInfo.value) {
        barInfo.value.followedCount = Math.max(0, barInfo.value.followedCount - 1)
      }
      ElMessage.success('已取消关注')
    } else {
      // 关注
      const followData: THBarFollow = {
        barId: barId,
        userId: currentUserId.value,
        followTime: new Date().toISOString(),
        isActive: 1
      }
      
      await followBar(followData)
      isFollowed.value = true
      if (barInfo.value) {
        barInfo.value.followedCount = barInfo.value.followedCount + 1
      }
      ElMessage.success('关注成功')
    }
  } catch (error) {
    console.error('关注操作失败:', error)
    ElMessage.error('操作失败，请重试')
  } finally {
    followLoading.value = false
  }
}

// 创建帖子
const createPost = () => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  // 跳转到发帖页面，通过URL参数传递贴吧ID
  router.push(`/PostNew?barId=${barId}&barName=${encodeURIComponent(barInfo.value?.barName || '')}`)
}

// 打开管理对话框
const openManageDialog = () => {
  manageDialogVisible.value = true
}

// 编辑贴吧
const editBar = () => {
  manageDialogVisible.value = false
  router.push(`/bar/${barId}/edit`)
}

// 归档贴吧
const archiveBar = async () => {
  try {
    await ElMessageBox.confirm(
      '确定要归档这个贴吧吗？归档后贴吧将不再显示在公开列表中。',
      '确认归档',
      { type: 'warning' }
    )
    
    await updateBarStatus(barId, 1)
    barInfo.value.status = 1
    manageDialogVisible.value = false
    ElMessage.success('贴吧已归档')
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('归档失败，请重试')
    }
  }
}

// 取消归档
const unarchiveBar = async () => {
  try {
    await updateBarStatus(barId, 0)
    barInfo.value.status = 0
    manageDialogVisible.value = false
    ElMessage.success('已取消归档')
  } catch (error) {
    ElMessage.error('操作失败，请重试')
  }
}

// 解散贴吧
const confirmDissolveBar = async () => {
  try {
    await ElMessageBox.confirm(
      '确定要解散这个贴吧吗？解散后贴吧将无法恢复，所有数据将被永久删除。',
      '确认解散',
      { 
        type: 'error',
        confirmButtonText: '确定解散',
        cancelButtonText: '取消'
      }
    )
    
    await updateBarStatus(barId, 2)
    manageDialogVisible.value = false
    ElMessage.success('贴吧已解散')
    
    // 延迟跳转
    setTimeout(() => {
      router.push('/bars')
    }, 1000)
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('解散失败，请重试')
    }
  }
}

// 处理帖子删除事件
const handlePostDeleted = (postId: number) => {
  console.log('📨 贴吧页面：接收到帖子删除事件，帖子ID:', postId)
  console.log('🔍 删除前贴吧帖子列表:', barPosts.value.slice(0, 5))
  
  // 从贴吧帖子列表中移除删除的帖子
  const oldLength = barPosts.value.length
  barPosts.value = barPosts.value.filter(id => id !== postId)
  const newLength = barPosts.value.length
  
  console.log(`✅ 贴吧页面：帖子 ${postId} 已删除，列表长度从 ${oldLength} 变为 ${newLength}`)
  
  // 更新贴吧帖子数量
  if (barInfo.value) {
    barInfo.value.postCount = Math.max(0, barInfo.value.postCount - 1)
    console.log('📊 更新贴吧帖子数量:', barInfo.value.postCount)
  }
}

// 处理帖子举报事件
const handlePostReported = (postId: number) => {
  console.log('📨 贴吧页面：接收到帖子举报事件，帖子ID:', postId)
  console.log('🔍 举报前贴吧帖子列表:', barPosts.value.slice(0, 5))
  
  // 从贴吧帖子列表中移除举报的帖子
  const oldLength = barPosts.value.length
  barPosts.value = barPosts.value.filter(id => id !== postId)
  const newLength = barPosts.value.length
  
  console.log(`✅ 贴吧页面：帖子 ${postId} 已举报，列表长度从 ${oldLength} 变为 ${newLength}`)
}

// 组件挂载时加载数据
onMounted(async () => {
  await loadBarDetail()
  await Promise.all([
    loadMembers(),
    loadBarPosts()
  ])
})
</script>

<style scoped>
.bar-detail-container {
  min-height: 100vh;
  background: #f5f5f5;
}

/* 贴吧头部 */
.bar-header {
  position: relative;
  margin-bottom: 20px;
}

.bar-cover {
  height: 300px;
  background-size: cover;
  background-position: center;
  position: relative;
  display: flex;
  align-items: flex-end;
}

.cover-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(transparent, rgba(0, 0, 0, 0.7));
}

.bar-main-info {
  position: relative;
  z-index: 2;
  display: flex;
  align-items: flex-end;
  gap: 20px;
  padding: 30px;
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
}

.bar-avatar-large {
  width: 120px;
  height: 120px;
  border-radius: 16px;
  overflow: hidden;
  border: 4px solid white;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.3);
  flex-shrink: 0;
}

.bar-avatar-large img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.bar-details {
  flex: 1;
  color: white;
}

.bar-name {
  font-size: 32px;
  font-weight: 700;
  margin: 0 0 8px 0;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.5);
}

.bar-description {
  font-size: 16px;
  margin: 0 0 16px 0;
  line-height: 1.5;
  opacity: 0.9;
}

.bar-stats {
  display: flex;
  gap: 24px;
}

.stat-item {
  text-align: center;
}

.stat-number {
  display: block;
  font-size: 24px;
  font-weight: 700;
  color: white;
}

.stat-label {
  display: block;
  font-size: 12px;
  opacity: 0.8;
  margin-top: 4px;
}

.bar-actions {
  flex-shrink: 0;
}

/* 内容区域 */
.bar-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.content-wrapper {
  display: grid;
  grid-template-columns: 300px 1fr;
  gap: 20px;
}

/* 侧边栏 */
.sidebar {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.info-card,
.rules-card,
.tags-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.card-title {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0 0 16px 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.info-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.info-label {
  color: #666;
  font-size: 14px;
}

.info-value {
  color: #333;
  font-weight: 500;
  font-size: 14px;
}

.rules-content {
  color: #555;
  line-height: 1.6;
  font-size: 14px;
}

.tags-content {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.bar-tag {
  background: #e3f2fd !important;
  color: #1976d2 !important;
  border: none !important;
}

/* 主内容区 */
.main-content {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.content-tabs {
  padding: 20px;
}

/* 帖子区域 */
.posts-section {
  min-height: 400px;
}

.posts-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.posts-header h3 {
  margin: 0;
  color: #333;
  font-size: 18px;
}

.coming-soon {
  text-align: center;
  padding: 80px 20px;
}

.coming-icon {
  font-size: 64px;
  margin-bottom: 20px;
  opacity: 0.6;
}

.coming-text {
  font-size: 18px;
  color: #666;
  margin-bottom: 8px;
}

/* 帖子区域样式 */
.loading-posts {
  display: flex;
  justify-content: center;
  padding: 60px;
}

.empty-posts {
  text-align: center;
  padding: 60px 20px;
}

.empty-posts .empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.6;
}

.empty-posts .empty-text {
  font-size: 16px;
  color: #666;
  margin-bottom: 8px;
}

.empty-posts .empty-hint {
  font-size: 14px;
  color: #999;
  margin-bottom: 24px;
}

.empty-action-btn {
  padding: 12px 32px;
  font-size: 16px;
}

.posts-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.post-item {
  background: white;
  border-radius: 8px;
  overflow: hidden;
}

/* 成员区域 */
.members-section {
  min-height: 400px;
}

.members-header {
  margin-bottom: 20px;
}

.members-header h3 {
  margin: 0;
  color: #333;
  font-size: 18px;
}

.loading-members {
  display: flex;
  justify-content: center;
  padding: 60px;
}

.empty-members {
  text-align: center;
  padding: 60px 20px;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.6;
}

.empty-text {
  color: #666;
  font-size: 16px;
}

.members-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 16px;
}

.member-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 8px;
}

.member-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  overflow: hidden;
}

.member-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.member-info {
  flex: 1;
}

.member-id {
  font-weight: 500;
  color: #333;
  font-size: 14px;
}

.member-join-time {
  color: #666;
  font-size: 12px;
  margin-top: 2px;
}

/* 管理对话框 */
.manage-options {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.manage-btn {
  width: 100%;
  justify-content: flex-start;
  padding: 12px 20px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .bar-main-info {
    flex-direction: column;
    align-items: center;
    text-align: center;
    gap: 16px;
  }
  
  .bar-stats {
    justify-content: center;
  }
  
  .content-wrapper {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .sidebar {
    order: 2;
  }
  
  .main-content {
    order: 1;
  }
}
</style>
