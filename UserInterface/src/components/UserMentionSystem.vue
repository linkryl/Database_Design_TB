<!--
用户提及系统 - @功能实现
TreeHole开发组
-->

<template>
  <div class="mention-system">
    <!-- @提及输入框 -->
    <div class="mention-input-container" v-if="showMentionInput">
      <div class="mention-header">
        <span>@提及用户</span>
        <el-button text size="small" @click="closeMentionInput">×</el-button>
      </div>
      
      <el-input
        v-model="mentionSearch"
        placeholder="输入用户名搜索..."
        @input="searchUsers"
        @keyup.enter="selectFirstUser"
        @keyup.esc="closeMentionInput"
        ref="mentionInputRef"
      />
      
      <!-- 用户搜索结果 -->
      <div v-if="searchResults.length > 0" class="mention-results">
        <div
          v-for="(user, index) in searchResults"
          :key="user.userId"
          class="mention-user-item"
          :class="{ highlighted: highlightedIndex === index }"
          @click="selectUser(user)"
          @mouseenter="highlightedIndex = index"
        >
          <img :src="user.avatarUrl || '/images/default-avatar.png'" class="user-avatar">
          <div class="user-info">
            <div class="user-name">{{ user.userName }}</div>
            <div class="user-desc">
              <span class="user-level">Lv.{{ user.level }}</span>
              <span class="user-stats">{{ user.followedCount }}粉丝</span>
            </div>
          </div>
          <div class="user-badges">
            <span v-if="user.isOnline" class="online-badge">在线</span>
            <span v-if="user.isVip" class="vip-badge">VIP</span>
          </div>
        </div>
      </div>
      
      <!-- 最近@过的用户 -->
      <div v-if="recentMentions.length > 0 && !mentionSearch" class="recent-mentions">
        <div class="recent-header">最近@过的用户</div>
        <div class="recent-users">
          <div
            v-for="user in recentMentions"
            :key="user.userId"
            class="recent-user"
            @click="selectUser(user)"
          >
            <img :src="user.avatarUrl || '/images/default-avatar.png'" class="recent-avatar">
            <span class="recent-name">{{ user.userName }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- @提及显示组件 -->
    <div class="mention-display">
      <span
        v-for="mention in mentions"
        :key="mention.userId"
        class="mention-tag"
        @click="viewUserProfile(mention.userId)"
      >
        @{{ mention.userName }}
      </span>
    </div>

    <!-- @提及通知 -->
    <div v-if="mentionNotifications.length > 0" class="mention-notifications">
      <div class="notification-header">
        <span>有人@了你</span>
        <el-button text size="small" @click="clearAllNotifications">清空</el-button>
      </div>
      
      <div class="notification-list">
        <div
          v-for="notification in mentionNotifications"
          :key="notification.id"
          class="mention-notification"
          @click="goToMention(notification)"
        >
          <img :src="notification.fromUser.avatarUrl" class="notification-avatar">
          <div class="notification-content">
            <div class="notification-text">
              <strong>{{ notification.fromUser.userName }}</strong> 
              在 <span class="post-title">{{ notification.postTitle }}</span> 中@了你
            </div>
            <div class="notification-time">{{ formatTime(notification.time) }}</div>
          </div>
          <div class="notification-actions">
            <el-button size="small" @click.stop="markAsRead(notification.id)">
              标记已读
            </el-button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { ref, computed, onMounted, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { searchUsers as apiSearchUsers } from '@/api/index'
import { getCurrentUserId } from '@/utils/auth'

// Props
const props = defineProps<{
  modelValue?: string
  mentions?: any[]
  showInput?: boolean
}>()

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: string]
  'mention-added': [user: any]
  'mention-removed': [userId: number]
}>()

// 响应式数据
const mentionSearch = ref('')
const searchResults = ref<any[]>([])
const highlightedIndex = ref(-1)
const showMentionInput = ref(props.showInput || false)
const mentionInputRef = ref<any>()

// 当前用户
const currentUserId = ref(getCurrentUserId() ? parseInt(getCurrentUserId()!) : null)

// 最近@过的用户
const recentMentions = ref<any[]>([])

// @提及通知
const mentionNotifications = ref<any[]>([])

// 路由
const router = useRouter()

// 计算属性
const mentions = computed(() => props.mentions || [])

// 组件挂载时初始化
onMounted(() => {
  loadRecentMentions()
  loadMentionNotifications()
})

// 监听搜索输入
watch(mentionSearch, (newValue) => {
  if (newValue.length >= 2) {
    searchUsers()
  } else {
    searchResults.value = []
    highlightedIndex.value = -1
  }
})

// 搜索用户
const searchUsers = async () => {
  if (!mentionSearch.value || mentionSearch.value.length < 2) {
    searchResults.value = []
    return
  }
  
  try {
    const results = await apiSearchUsers(mentionSearch.value, 1, 10)
    
    // 过滤掉当前用户
    searchResults.value = results.filter((user: any) => user.userId !== currentUserId.value)
      .map((user: any) => ({
        ...user,
        level: calculateUserLevel(user.experiencePoints || 0),
        isOnline: Math.random() > 0.7, // 模拟在线状态
        isVip: Math.random() > 0.9 // 模拟VIP状态
      }))
    
    highlightedIndex.value = searchResults.value.length > 0 ? 0 : -1
    
  } catch (error) {
    console.error('搜索用户失败:', error)
    searchResults.value = []
  }
}

// 选择用户
const selectUser = (user: any) => {
  emit('mention-added', user)
  
  // 添加到最近@过的用户
  addToRecentMentions(user)
  
  // 关闭输入框
  closeMentionInput()
  
  ElMessage.success(`已@${user.userName}`)
}

// 选择第一个用户（回车键）
const selectFirstUser = () => {
  if (searchResults.value.length > 0 && highlightedIndex.value >= 0) {
    selectUser(searchResults.value[highlightedIndex.value])
  }
}

// 关闭@输入框
const closeMentionInput = () => {
  showMentionInput.value = false
  mentionSearch.value = ''
  searchResults.value = []
  highlightedIndex.value = -1
}

// 打开@输入框
const openMentionInput = () => {
  showMentionInput.value = true
  nextTick(() => {
    mentionInputRef.value?.focus()
  })
}

// 加载最近@过的用户
const loadRecentMentions = () => {
  // 从localStorage或API加载
  const stored = localStorage.getItem('recentMentions')
  if (stored) {
    try {
      recentMentions.value = JSON.parse(stored).slice(0, 5)
    } catch (error) {
      recentMentions.value = []
    }
  }
}

// 添加到最近@过的用户
const addToRecentMentions = (user: any) => {
  // 移除已存在的用户
  recentMentions.value = recentMentions.value.filter(u => u.userId !== user.userId)
  
  // 添加到开头
  recentMentions.value.unshift(user)
  
  // 只保留最近5个
  recentMentions.value = recentMentions.value.slice(0, 5)
  
  // 保存到localStorage
  localStorage.setItem('recentMentions', JSON.stringify(recentMentions.value))
}

// 加载@提及通知
const loadMentionNotifications = async () => {
  if (!currentUserId.value) return
  
  try {
    // 这里应该调用API获取@提及通知
    // 模拟数据
    mentionNotifications.value = [
      {
        id: 1,
        fromUser: {
          userId: 2,
          userName: '用户A',
          avatarUrl: '/images/avatar1.png'
        },
        postId: 123,
        postTitle: '关于学习方法的讨论',
        time: new Date(Date.now() - 3600000).toISOString(),
        isRead: false
      }
    ]
  } catch (error) {
    console.error('加载@提及通知失败:', error)
  }
}

// 查看用户资料
const viewUserProfile = (userId: number) => {
  router.push(`/profile/${userId}`)
}

// 跳转到@提及的位置
const goToMention = (notification: any) => {
  router.push(`/PostPage/${notification.postId}#mention-${notification.id}`)
  markAsRead(notification.id)
}

// 标记通知为已读
const markAsRead = (notificationId: number) => {
  const notification = mentionNotifications.value.find(n => n.id === notificationId)
  if (notification) {
    notification.isRead = true
  }
}

// 清空所有通知
const clearAllNotifications = () => {
  mentionNotifications.value = []
  ElMessage.success('已清空所有@提及通知')
}

// 工具函数
const calculateUserLevel = (experiencePoints: number): number => {
  if (experiencePoints < 100) return 1
  if (experiencePoints < 300) return 2
  if (experiencePoints < 700) return 3
  if (experiencePoints < 1500) return 4
  if (experiencePoints < 3000) return 5
  return Math.min(20, Math.floor(experiencePoints / 1000) + 5)
}

const formatTime = (timestamp: string): string => {
  const date = new Date(timestamp)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  if (diff < 60000) return '刚刚'
  if (diff < 3600000) return `${Math.floor(diff / 60000)}分钟前`
  if (diff < 86400000) return `${Math.floor(diff / 3600000)}小时前`
  
  return date.toLocaleDateString('zh-CN')
}

// 暴露方法给父组件
defineExpose({
  openMentionInput,
  closeMentionInput
})
</script>

<style scoped>
.mention-system {
  position: relative;
}

/* @输入框样式 */
.mention-input-container {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: white;
  border: 1px solid #e4e7ed;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 1000;
  max-height: 400px;
  overflow: hidden;
}

.mention-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f8f9fa;
  border-bottom: 1px solid #e4e7ed;
  font-size: 14px;
  font-weight: 600;
}

.mention-results {
  max-height: 250px;
  overflow-y: auto;
}

.mention-user-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.mention-user-item:hover,
.mention-user-item.highlighted {
  background: #f0f9ff;
}

.user-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.user-info {
  flex: 1;
  min-width: 0;
}

.user-name {
  font-size: 14px;
  font-weight: 500;
  color: #333;
  margin-bottom: 2px;
}

.user-desc {
  display: flex;
  gap: 8px;
  font-size: 12px;
  color: #999;
}

.user-level {
  background: #e8f4fd;
  color: #409eff;
  padding: 1px 4px;
  border-radius: 3px;
}

.user-badges {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.online-badge {
  background: #67c23a;
  color: white;
  padding: 1px 4px;
  border-radius: 8px;
  font-size: 10px;
  text-align: center;
}

.vip-badge {
  background: linear-gradient(135deg, #ffd700, #ffed4e);
  color: #333;
  padding: 1px 4px;
  border-radius: 8px;
  font-size: 10px;
  font-weight: 600;
  text-align: center;
}

/* 最近@过的用户 */
.recent-mentions {
  padding: 12px;
  border-top: 1px solid #f0f0f0;
}

.recent-header {
  font-size: 12px;
  color: #999;
  margin-bottom: 8px;
}

.recent-users {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.recent-user {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  background: #f8f9fa;
  border-radius: 12px;
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: 12px;
}

.recent-user:hover {
  background: #e9ecef;
}

.recent-avatar {
  width: 16px;
  height: 16px;
  border-radius: 50%;
}

.recent-name {
  color: #333;
  font-weight: 500;
}

/* @提及显示样式 */
.mention-display {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  margin: 8px 0;
}

.mention-tag {
  background: #e8f4fd;
  color: #409eff;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid #b3d8ff;
}

.mention-tag:hover {
  background: #409eff;
  color: white;
  transform: translateY(-1px);
}

/* @提及通知样式 */
.mention-notifications {
  background: #fff7e6;
  border: 1px solid #e6a23c;
  border-radius: 8px;
  margin: 12px 0;
  overflow: hidden;
}

.notification-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #fdf6ec;
  border-bottom: 1px solid #e6a23c;
  font-size: 14px;
  font-weight: 600;
  color: #e6a23c;
}

.notification-list {
  max-height: 200px;
  overflow-y: auto;
}

.mention-notification {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  cursor: pointer;
  transition: background-color 0.2s;
  border-bottom: 1px solid #f0f0f0;
}

.mention-notification:hover {
  background: #fdf6ec;
}

.mention-notification:last-child {
  border-bottom: none;
}

.notification-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.notification-content {
  flex: 1;
  min-width: 0;
}

.notification-text {
  font-size: 13px;
  color: #333;
  line-height: 1.4;
  margin-bottom: 2px;
}

.post-title {
  color: #409eff;
  font-weight: 500;
}

.notification-time {
  font-size: 11px;
  color: #999;
}

.notification-actions {
  flex-shrink: 0;
}

/* 滚动条样式 */
.mention-results::-webkit-scrollbar,
.notification-list::-webkit-scrollbar {
  width: 4px;
}

.mention-results::-webkit-scrollbar-track,
.notification-list::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.mention-results::-webkit-scrollbar-thumb,
.notification-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 2px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .mention-input-container {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: 90%;
    max-width: 400px;
    max-height: 70vh;
  }
  
  .mention-user-item {
    padding: 12px;
  }
  
  .user-avatar {
    width: 40px;
    height: 40px;
  }
  
  .user-name {
    font-size: 15px;
  }
  
  .recent-users {
    flex-direction: column;
  }
  
  .recent-user {
    justify-content: flex-start;
    padding: 8px;
  }
  
  .recent-avatar {
    width: 20px;
    height: 20px;
  }
}

/* 动画效果 */
.mention-input-container {
  animation: slideDown 0.2s ease;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.mention-tag {
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: scale(0.8);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

/* 深色模式支持 */
@media (prefers-color-scheme: dark) {
  .mention-input-container {
    background: #2c2c2c;
    border-color: #404040;
  }
  
  .mention-header {
    background: #363636;
    border-color: #404040;
    color: white;
  }
  
  .mention-user-item {
    color: white;
  }
  
  .mention-user-item:hover,
  .mention-user-item.highlighted {
    background: #404040;
  }
  
  .user-name {
    color: white;
  }
  
  .recent-mentions {
    border-color: #404040;
  }
  
  .recent-user {
    background: #404040;
    color: white;
  }
  
  .recent-user:hover {
    background: #4a4a4a;
  }
  
  .mention-notifications {
    background: #2c2c2c;
    border-color: #e6a23c;
  }
  
  .notification-header {
    background: #363636;
  }
  
  .mention-notification {
    border-color: #404040;
  }
  
  .mention-notification:hover {
    background: #363636;
  }
  
  .notification-text {
    color: white;
  }
}
</style>
