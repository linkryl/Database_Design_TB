<template>
  <div class="group-chat-container">
    <!-- 群组信息头部 -->
    <div class="chat-header">
      <el-button type="text" @click="goBack" class="back-btn">
        <el-icon><ArrowLeft /></el-icon>
      </el-button>
      <div class="group-info">
        <h3>{{ groupInfo?.groupName || '群聊' }}</h3>
        <span class="member-count">{{ groupInfo?.memberCount || 0 }}人</span>
      </div>
      <el-button type="text" @click="showGroupInfo = true">
        <el-icon><More /></el-icon>
      </el-button>
    </div>

    <!-- 消息列表 -->
    <div class="message-list" ref="messageContainer">
      <div 
        v-for="message in messages" 
        :key="message.messageId"
        class="message-item"
        :class="{ 'own-message': message.senderId === currentUserId }"
      >
        <div class="message-avatar">
          <el-avatar :size="32">{{ message.senderName?.charAt(0) || 'U' }}</el-avatar>
        </div>
        <div class="message-content">
          <div class="message-header">
            <span class="sender-name">{{ message.senderName }}</span>
            <span class="send-time">{{ formatTime(message.sendTime) }}</span>
          </div>
          <div class="message-text">{{ message.content }}</div>
          <div v-if="message.replyToMessageId" class="reply-info">
            回复了一条消息
          </div>
        </div>
      </div>
      
      <!-- 加载更多 -->
      <div v-if="hasMore" class="load-more">
        <el-button @click="loadMoreMessages" :loading="loading">加载更多</el-button>
      </div>
      
      <!-- 无消息提示 -->
      <div v-if="messages.length === 0 && !loading" class="no-messages">
        <el-empty description="暂无聊天记录，开始聊天吧！" />
      </div>
    </div>

    <!-- 消息输入框 -->
    <div class="message-input">
      <div class="input-toolbar">
        <el-button type="text" @click="selectEmoji">
          <el-icon><ChatDotRound /></el-icon>
        </el-button>
        <el-button type="text" @click="selectFile">
          <el-icon><Plus /></el-icon>
        </el-button>
      </div>
      <div class="input-area">
        <el-input
          v-model="messageText"
          type="textarea"
          :rows="2"
          placeholder="输入消息..."
          @keydown.enter.prevent="sendMessage"
          maxlength="1000"
          show-word-limit
        />
        <el-button 
          type="primary" 
          @click="sendMessage"
          :disabled="!messageText.trim()"
          :loading="sending"
        >
          发送
        </el-button>
      </div>
    </div>

    <!-- 群组信息弹窗 -->
    <el-dialog v-model="showGroupInfo" title="群组信息" width="400px">
      <div class="group-detail">
        <div class="group-name">{{ groupInfo?.groupName }}</div>
        <div class="group-desc">{{ groupInfo?.groupDesc || '暂无群组描述' }}</div>
        <div class="group-members">
          <div class="members-title">群成员 ({{ groupInfo?.memberCount }})</div>
          <!-- 这里可以添加成员列表 -->
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { ArrowLeft, More, ChatDotRound, Plus } from '@element-plus/icons-vue'
import axios from '../components/axios'

// 路由相关
const route = useRoute()
const router = useRouter()
const groupId = ref(Number(route.params.groupId))

// 数据状态
const groupInfo = ref<any>(null)
const messages = ref<any[]>([])
const messageText = ref('')
const currentUserId = ref(Number(localStorage.getItem('userId') || '56')) // 临时使用56，该用户是群组成员
const loading = ref(false)
const sending = ref(false)
const hasMore = ref(true)
const currentPage = ref(1)

// UI状态
const showGroupInfo = ref(false)
const messageContainer = ref<HTMLElement>()

// 获取群组信息
const loadGroupInfo = async () => {
  try {
    const response = await axios.get(`/group/${groupId.value}`)
    groupInfo.value = response.data
  } catch (error) {
    console.error('获取群组信息失败:', error)
    ElMessage.error('获取群组信息失败')
  }
}

// 加载消息列表
const loadMessages = async (page = 1, append = false) => {
  if (loading.value) return
  
  loading.value = true
  try {
    const response = await axios.get(`/group-message/${groupId.value}`, {
      params: { page, pageSize: 50 }
    })
    
    const newMessages = response.data || []
    
    if (append) {
      messages.value = [...newMessages, ...messages.value]
    } else {
      messages.value = newMessages
    }
    
    hasMore.value = newMessages.length === 50
    
    if (!append) {
      await nextTick()
      scrollToBottom()
    }
  } catch (error) {
    console.error('加载消息失败:', error)
    ElMessage.error('加载消息失败')
  } finally {
    loading.value = false
  }
}

// 加载更多消息
const loadMoreMessages = async () => {
  currentPage.value++
  await loadMessages(currentPage.value, true)
}

// 发送消息
const sendMessage = async () => {
  if (!messageText.value.trim() || sending.value) return
  
  sending.value = true
  try {
    const request = {
      groupId: groupId.value,
      senderId: currentUserId.value,
      content: messageText.value.trim(),
      messageType: 0
    }
    
    const response = await axios.post('/group-message', request)
    
    // 添加新消息到列表
    const newMessage = {
      ...response.data,
      senderName: '我' // 简化处理，实际应该从用户信息获取
    }
    messages.value.push(newMessage)
    
    messageText.value = ''
    
    await nextTick()
    scrollToBottom()
    
    ElMessage.success('发送成功')
  } catch (error) {
    console.error('发送消息失败:', error)
    ElMessage.error('发送消息失败')
  } finally {
    sending.value = false
  }
}

// 滚动到底部
const scrollToBottom = () => {
  if (messageContainer.value) {
    messageContainer.value.scrollTop = messageContainer.value.scrollHeight
  }
}

// 格式化时间
const formatTime = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  if (diff < 60000) { // 1分钟内
    return '刚刚'
  } else if (diff < 3600000) { // 1小时内
    return `${Math.floor(diff / 60000)}分钟前`
  } else if (diff < 86400000) { // 24小时内
    return date.toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
  } else {
    return date.toLocaleDateString('zh-CN') + ' ' + 
           date.toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
  }
}

// 选择表情（占位）
const selectEmoji = () => {
  ElMessage.info('表情功能开发中...')
}

// 选择文件（占位）
const selectFile = () => {
  ElMessage.info('文件发送功能开发中...')
}

// 返回上一页
const goBack = () => {
  router.back()
}

// 监听群组ID变化
watch(() => route.params.groupId, (newId) => {
  if (newId) {
    groupId.value = Number(newId)
    currentPage.value = 1
    hasMore.value = true
    loadGroupInfo()
    loadMessages()
  }
})

// 组件挂载
onMounted(() => {
  loadGroupInfo()
  loadMessages()
})

// 定时刷新消息（简单轮询，生产环境建议使用WebSocket）
let refreshInterval: NodeJS.Timeout
onMounted(() => {
  refreshInterval = setInterval(() => {
    if (!loading.value && !sending.value) {
      loadMessages(1, false)
    }
  }, 5000) // 每5秒刷新一次
})

// 组件卸载时清除定时器
import { onUnmounted } from 'vue'
onUnmounted(() => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
  }
})
</script>

<style scoped>
.group-chat-container {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background-color: #f5f5f5;
}

.chat-header {
  display: flex;
  align-items: center;
  padding: 12px 16px;
  background-color: white;
  border-bottom: 1px solid #e0e0e0;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

.back-btn {
  margin-right: 12px;
  padding: 8px;
}

.group-info {
  flex: 1;
  text-align: center;
}

.group-info h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
}

.member-count {
  font-size: 12px;
  color: #666;
}

.message-list {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
  background-color: #f5f5f5;
}

.message-item {
  display: flex;
  margin-bottom: 16px;
  align-items: flex-start;
}

.message-item.own-message {
  flex-direction: row-reverse;
}

.message-avatar {
  margin: 0 8px;
}

.message-content {
  max-width: 70%;
  background-color: white;
  border-radius: 8px;
  padding: 8px 12px;
  box-shadow: 0 1px 2px rgba(0,0,0,0.1);
}

.own-message .message-content {
  background-color: #409eff;
  color: white;
}

.message-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
  font-size: 12px;
}

.sender-name {
  font-weight: 600;
}

.own-message .sender-name {
  color: rgba(255,255,255,0.9);
}

.send-time {
  color: #999;
}

.own-message .send-time {
  color: rgba(255,255,255,0.7);
}

.message-text {
  line-height: 1.4;
  word-wrap: break-word;
}

.reply-info {
  font-size: 12px;
  color: #666;
  margin-top: 4px;
  font-style: italic;
}

.load-more {
  text-align: center;
  margin: 16px 0;
}

.no-messages {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 200px;
}

.message-input {
  background-color: white;
  border-top: 1px solid #e0e0e0;
  padding: 12px 16px;
}

.input-toolbar {
  display: flex;
  gap: 8px;
  margin-bottom: 8px;
}

.input-area {
  display: flex;
  gap: 8px;
  align-items: flex-end;
}

.input-area .el-input {
  flex: 1;
}

.group-detail {
  text-align: center;
}

.group-name {
  font-size: 18px;
  font-weight: 600;
  margin-bottom: 8px;
}

.group-desc {
  color: #666;
  margin-bottom: 24px;
}

.members-title {
  font-weight: 600;
  margin-bottom: 12px;
  text-align: left;
}
</style>