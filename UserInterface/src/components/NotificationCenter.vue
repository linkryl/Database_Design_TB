<!--
通知中心组件 - 实时通知显示和管理
TreeHole开发组
-->

<template>
  <div class="notification-center">
    <!-- 通知铃铛图标 -->
    <div class="notification-bell" @click="toggleNotificationPanel">
      <el-badge :value="unreadCount" :max="99" :hidden="unreadCount === 0">
        <div class="bell-icon" :class="{ active: hasUnread, shake: newNotification }">
          🔔
        </div>
      </el-badge>
    </div>

    <!-- 通知面板 -->
    <el-drawer
      v-model="showNotificationPanel"
      title="消息通知"
      direction="rtl"
      size="400px"
      :with-header="true"
    >
      <template #header>
        <div class="notification-header">
          <h3>消息通知</h3>
          <div class="header-actions">
            <el-button 
              text 
              size="small" 
              @click="markAllAsRead"
              :disabled="unreadCount === 0"
            >
              全部已读
            </el-button>
            <el-button text size="small" @click="refreshNotifications">
              刷新
            </el-button>
          </div>
        </div>
      </template>

      <!-- 通知筛选标签 -->
      <div class="notification-filters">
        <el-tabs v-model="activeFilter" @tab-change="handleFilterChange">
          <el-tab-pane label="全部" name="all" />
          <el-tab-pane label="未读" name="unread" />
          <el-tab-pane label="系统" name="system" />
          <el-tab-pane label="互动" name="interaction" />
        </el-tabs>
      </div>

      <!-- 通知列表 -->
      <div class="notification-list" v-loading="loading">
        <div v-if="filteredNotifications.length === 0" class="empty-notifications">
          <div class="empty-icon">📭</div>
          <div class="empty-text">暂无通知</div>
        </div>

        <div
          v-for="notification in filteredNotifications"
          :key="notification.id"
          class="notification-item"
          :class="{ 
            unread: !notification.isRead,
            clickable: !!notification.actionUrl 
          }"
          @click="handleNotificationClick(notification)"
        >
          <div class="notification-avatar">
            <img 
              v-if="notification.senderAvatarUrl" 
              :src="notification.senderAvatarUrl" 
              :alt="notification.senderUserName"
            />
            <div v-else class="notification-type-icon">
              {{ getNotificationIcon(notification.type) }}
            </div>
          </div>

          <div class="notification-content">
            <div class="notification-title">{{ notification.title }}</div>
            <div class="notification-text">{{ notification.content }}</div>
            <div class="notification-meta">
              <span class="notification-time">{{ formatTime(notification.createdAt) }}</span>
              <span v-if="notification.senderUserName" class="notification-sender">
                来自 {{ notification.senderUserName }}
              </span>
            </div>
          </div>

          <div class="notification-actions">
            <el-button 
              v-if="!notification.isRead" 
              text 
              size="small"
              @click.stop="markAsRead(notification.id)"
            >
              标记已读
            </el-button>
            <el-dropdown @command="handleNotificationAction">
              <el-button text size="small">
                ⋮
              </el-button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item :command="`delete_${notification.id}`">
                    删除
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
        </div>

        <!-- 加载更多 -->
        <div v-if="hasMore" class="load-more">
          <el-button @click="loadMoreNotifications" :loading="loadingMore">
            加载更多
          </el-button>
        </div>
      </div>
    </el-drawer>

    <!-- 通知弹窗 -->
    <el-notification
      v-for="popup in notificationPopups"
      :key="popup.id"
      :title="popup.title"
      :message="popup.content"
      :type="getNotificationElType(popup.type)"
      :duration="popup.duration || 4500"
      :position="popup.position || 'top-right'"
      @close="removeNotificationPopup(popup.id)"
    />

    <!-- 系统广播横幅 -->
    <el-alert
      v-if="currentBroadcast"
      :title="currentBroadcast.title"
      :description="currentBroadcast.content"
      type="info"
      :closable="true"
      show-icon
      @close="dismissBroadcast"
      class="broadcast-alert"
    />
  </div>
</template>

<script setup lang='ts'>
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElNotification } from 'element-plus'
import * as signalR from '@microsoft/signalr'
import axios from 'axios'

// 路由
const router = useRouter()

// 响应式数据
const showNotificationPanel = ref(false)
const loading = ref(false)
const loadingMore = ref(false)
const notifications = ref<any[]>([])
const unreadCount = ref(0)
const activeFilter = ref('all')
const currentPage = ref(1)
const hasMore = ref(true)
const newNotification = ref(false)

// SignalR连接
const connection = ref<signalR.HubConnection | null>(null)

// 通知弹窗
const notificationPopups = ref<any[]>([])

// 系统广播
const currentBroadcast = ref<any>(null)

// API基础URL
const apiBaseUrl = '/api'

// 计算属性
const hasUnread = computed(() => unreadCount.value > 0)

const filteredNotifications = computed(() => {
  switch (activeFilter.value) {
    case 'unread':
      return notifications.value.filter(n => !n.isRead)
    case 'system':
      return notifications.value.filter(n => n.type === 0 || n.type === 9) // System, Announcement
    case 'interaction':
      return notifications.value.filter(n => [1, 2, 3, 4].includes(n.type)) // Like, Comment, Follow, Mention
    default:
      return notifications.value
  }
})

// 组件挂载时初始化
onMounted(async () => {
  await initializeSignalR()
  await loadNotifications()
  await loadUnreadCount()
})

// 组件卸载时清理
onUnmounted(() => {
  if (connection.value) {
    connection.value.stop()
  }
})

// 初始化SignalR连接
const initializeSignalR = async () => {
  try {
    const token = localStorage.getItem('jwtToken')
    if (!token) return

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl('/notificationHub', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build()

    // 监听通知事件
    connection.value.on('ReceiveNotification', handleRealtimeNotification)
    connection.value.on('ReceiveBroadcast', handleBroadcast)
    connection.value.on('UserOnline', handleUserOnline)
    connection.value.on('UserOffline', handleUserOffline)

    // 开始连接
    await connection.value.start()
    console.log('SignalR连接已建立')

  } catch (error) {
    console.error('SignalR连接失败:', error)
  }
}

// 处理实时通知
const handleRealtimeNotification = (notification: any) => {
  console.log('收到实时通知:', notification)
  
  // 添加到通知列表
  notifications.value.unshift(notification)
  
  // 更新未读数量
  if (!notification.isRead) {
    unreadCount.value++
  }
  
  // 显示通知动画
  triggerNotificationAnimation()
  
  // 显示弹窗通知
  showNotificationPopup(notification)
}

// 处理系统广播
const handleBroadcast = (broadcast: any) => {
  console.log('收到系统广播:', broadcast)
  currentBroadcast.value = broadcast
  
  // 显示弹窗通知
  showNotificationPopup({
    ...broadcast,
    duration: 6000
  })
}

// 处理用户上线
const handleUserOnline = (userId: string) => {
  console.log('用户上线:', userId)
  // 可以在这里更新好友在线状态
}

// 处理用户离线
const handleUserOffline = (userId: string) => {
  console.log('用户离线:', userId)
  // 可以在这里更新好友在线状态
}

// 显示通知弹窗
const showNotificationPopup = (notification: any) => {
  const popup = {
    id: notification.id || Date.now().toString(),
    title: notification.title,
    content: notification.content,
    type: notification.type || 0,
    duration: notification.duration || 4500,
    position: 'top-right'
  }
  
  notificationPopups.value.push(popup)
  
  // 自动移除弹窗
  setTimeout(() => {
    removeNotificationPopup(popup.id)
  }, popup.duration)
}

// 移除通知弹窗
const removeNotificationPopup = (popupId: string) => {
  const index = notificationPopups.value.findIndex(p => p.id === popupId)
  if (index > -1) {
    notificationPopups.value.splice(index, 1)
  }
}

// 触发通知动画
const triggerNotificationAnimation = () => {
  newNotification.value = true
  setTimeout(() => {
    newNotification.value = false
  }, 1000)
}

// 切换通知面板
const toggleNotificationPanel = () => {
  showNotificationPanel.value = !showNotificationPanel.value
  if (showNotificationPanel.value) {
    refreshNotifications()
  }
}

// 加载通知列表
const loadNotifications = async (page = 1) => {
  try {
    loading.value = page === 1
    
    const response = await axios.get(`${apiBaseUrl}/notifications`, {
      params: { page, pageSize: 20 }
    })
    
    const newNotifications = response.data
    
    if (page === 1) {
      notifications.value = newNotifications
    } else {
      notifications.value.push(...newNotifications)
    }
    
    hasMore.value = newNotifications.length === 20
    currentPage.value = page
    
  } catch (error: any) {
    console.error('加载通知失败:', error)
    ElMessage.error('加载通知失败')
  } finally {
    loading.value = false
  }
}

// 加载未读数量
const loadUnreadCount = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/notifications/unread-count`)
    unreadCount.value = response.data
  } catch (error) {
    console.error('加载未读数量失败:', error)
  }
}

// 刷新通知
const refreshNotifications = async () => {
  currentPage.value = 1
  await Promise.all([
    loadNotifications(1),
    loadUnreadCount()
  ])
}

// 加载更多通知
const loadMoreNotifications = async () => {
  if (loadingMore.value || !hasMore.value) return
  
  loadingMore.value = true
  try {
    await loadNotifications(currentPage.value + 1)
  } finally {
    loadingMore.value = false
  }
}

// 标记单个通知为已读
const markAsRead = async (notificationId: string) => {
  try {
    await axios.put(`${apiBaseUrl}/notifications/${notificationId}/read`)
    
    // 更新本地状态
    const notification = notifications.value.find(n => n.id === notificationId)
    if (notification && !notification.isRead) {
      notification.isRead = true
      notification.readAt = new Date().toISOString()
      unreadCount.value = Math.max(0, unreadCount.value - 1)
    }
    
  } catch (error) {
    console.error('标记已读失败:', error)
    ElMessage.error('标记已读失败')
  }
}

// 标记所有通知为已读
const markAllAsRead = async () => {
  try {
    await axios.put(`${apiBaseUrl}/notifications/read-all`)
    
    // 更新本地状态
    notifications.value.forEach(notification => {
      if (!notification.isRead) {
        notification.isRead = true
        notification.readAt = new Date().toISOString()
      }
    })
    
    unreadCount.value = 0
    ElMessage.success('所有通知已标记为已读')
    
  } catch (error) {
    console.error('标记所有已读失败:', error)
    ElMessage.error('标记所有已读失败')
  }
}

// 处理通知点击
const handleNotificationClick = (notification: any) => {
  // 如果未读，先标记为已读
  if (!notification.isRead) {
    markAsRead(notification.id)
  }
  
  // 如果有操作链接，进行跳转
  if (notification.actionUrl) {
    if (notification.actionUrl.startsWith('http')) {
      window.open(notification.actionUrl, '_blank')
    } else {
      router.push(notification.actionUrl)
      showNotificationPanel.value = false
    }
  }
}

// 处理筛选变化
const handleFilterChange = (filterName: string) => {
  activeFilter.value = filterName
}

// 处理通知操作
const handleNotificationAction = (command: string) => {
  const [action, notificationId] = command.split('_')
  
  switch (action) {
    case 'delete':
      deleteNotification(notificationId)
      break
  }
}

// 删除通知
const deleteNotification = (notificationId: string) => {
  const index = notifications.value.findIndex(n => n.id === notificationId)
  if (index > -1) {
    const notification = notifications.value[index]
    notifications.value.splice(index, 1)
    
    // 如果是未读通知，减少未读数量
    if (!notification.isRead) {
      unreadCount.value = Math.max(0, unreadCount.value - 1)
    }
  }
}

// 关闭广播
const dismissBroadcast = () => {
  currentBroadcast.value = null
}

// 获取通知图标
const getNotificationIcon = (type: number): string => {
  const iconMap: { [key: number]: string } = {
    0: '📢', // System
    1: '👍', // Like
    2: '💬', // Comment
    3: '👤', // Follow
    4: '@',  // Mention
    5: '✉️', // PrivateMessage
    6: '👥', // GroupMessage
    7: '🎉', // LevelUp
    8: '🏆', // Badge
    9: '📣'  // Announcement
  }
  
  return iconMap[type] || '🔔'
}

// 获取Element Plus通知类型
const getNotificationElType = (type: number): 'success' | 'warning' | 'info' | 'error' => {
  const typeMap: { [key: number]: 'success' | 'warning' | 'info' | 'error' } = {
    0: 'info',    // System
    1: 'success', // Like
    2: 'info',    // Comment
    3: 'success', // Follow
    4: 'warning', // Mention
    5: 'info',    // PrivateMessage
    6: 'info',    // GroupMessage
    7: 'success', // LevelUp
    8: 'success', // Badge
    9: 'warning'  // Announcement
  }
  
  return typeMap[type] || 'info'
}

// 格式化时间
const formatTime = (timestamp: string): string => {
  const date = new Date(timestamp)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  if (diff < 60000) return '刚刚'
  if (diff < 3600000) return `${Math.floor(diff / 60000)}分钟前`
  if (diff < 86400000) return `${Math.floor(diff / 3600000)}小时前`
  if (diff < 604800000) return `${Math.floor(diff / 86400000)}天前`
  
  return date.toLocaleDateString('zh-CN')
}

// 暴露方法给父组件
defineExpose({
  refreshNotifications,
  showNotificationPopup,
  getUnreadCount: () => unreadCount.value
})
</script>

<style scoped>
.notification-center {
  position: relative;
}

.notification-bell {
  cursor: pointer;
  padding: 8px;
  border-radius: 50%;
  transition: background-color 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.notification-bell:hover {
  background-color: rgba(0, 0, 0, 0.1);
}

.bell-icon {
  font-size: 20px;
  transition: all 0.3s;
}

.bell-icon.active {
  color: #f56c6c;
}

.bell-icon.shake {
  animation: shake 0.5s ease-in-out;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-5px); }
  75% { transform: translateX(5px); }
}

.notification-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0;
}

.notification-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
}

.header-actions {
  display: flex;
  gap: 8px;
}

.notification-filters {
  margin-bottom: 16px;
}

.notification-list {
  max-height: calc(100vh - 200px);
  overflow-y: auto;
}

.empty-notifications {
  text-align: center;
  padding: 40px 20px;
  color: #999;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 12px;
}

.empty-text {
  font-size: 16px;
}

.notification-item {
  display: flex;
  padding: 12px;
  border-bottom: 1px solid #f0f0f0;
  transition: background-color 0.3s;
  gap: 12px;
}

.notification-item:hover {
  background-color: #f8f9fa;
}

.notification-item.unread {
  background-color: #e8f4fd;
  border-left: 3px solid #409eff;
}

.notification-item.clickable {
  cursor: pointer;
}

.notification-avatar {
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.notification-avatar img {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
}

.notification-type-icon {
  font-size: 20px;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: #f0f0f0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.notification-content {
  flex: 1;
  min-width: 0;
}

.notification-title {
  font-size: 14px;
  font-weight: 600;
  color: #333;
  margin-bottom: 4px;
  line-height: 1.4;
}

.notification-text {
  font-size: 13px;
  color: #666;
  line-height: 1.4;
  margin-bottom: 8px;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.notification-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: #999;
}

.notification-time {
  flex-shrink: 0;
}

.notification-sender {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.notification-actions {
  display: flex;
  align-items: flex-start;
  gap: 4px;
  flex-shrink: 0;
}

.load-more {
  text-align: center;
  padding: 16px;
}

.broadcast-alert {
  position: fixed;
  top: 60px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 2000;
  max-width: 600px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

/* 自定义滚动条 */
.notification-list::-webkit-scrollbar {
  width: 6px;
}

.notification-list::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.notification-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.notification-list::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}

/* Element Plus 抽屉样式覆盖 */
:deep(.el-drawer__body) {
  padding: 0 20px 20px;
}

:deep(.el-tabs__nav-wrap::after) {
  display: none;
}

:deep(.el-tabs__item) {
  font-size: 14px;
}

:deep(.el-badge__content) {
  font-size: 10px;
  padding: 0 4px;
  height: 16px;
  line-height: 16px;
  min-width: 16px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .notification-item {
    padding: 10px 8px;
  }
  
  .notification-avatar {
    width: 36px;
    height: 36px;
  }
  
  .notification-type-icon {
    width: 36px;
    height: 36px;
    font-size: 18px;
  }
  
  .notification-title {
    font-size: 13px;
  }
  
  .notification-text {
    font-size: 12px;
  }
  
  .broadcast-alert {
    top: 50px;
    left: 10px;
    right: 10px;
    transform: none;
    max-width: none;
  }
}
</style>
