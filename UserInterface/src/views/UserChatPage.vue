<template>
  <div class="chat-container">
    <div class="chat-header">
      <span>与 <b>{{ targetUserName }}</b> 私聊</span>
      <el-button type="text" style="float:right" @click="closeChat">关闭</el-button>
    </div>
    <div class="chat-messages">
      <div
        v-for="msg in messages"
        :key="msg.messageId"
        :class="msg.senderId === currentUserId ? 'my-message' : 'other-message'"
      >
        <div v-if="msg.senderId !== currentUserId" style="display: flex; flex-direction: column; align-items: flex-start; margin-bottom: 10px; width: 100%;">
          <span class="msg-time center-time">{{ formatTime(msg.sendTime) }}</span>
          <div style="display: flex; justify-content: flex-start; align-items: flex-end; width: 100%;">
            <span class="msg-user">{{ targetUserName }}</span>
            <span class="msg-content other-bubble">{{ msg.content }}</span>
          </div>
        </div>
        <div v-else style="display: flex; flex-direction: column; align-items: flex-end; margin-bottom: 10px; width: 100%;">
          <span class="msg-time center-time">{{ formatTime(msg.sendTime) }}</span>
          <div style="display: flex; justify-content: flex-end; align-items: flex-end; width: 100%;">
            <span class="msg-content my-bubble">{{ msg.content }}</span>
            <span class="msg-user">我</span>
          </div>
        </div>
      </div>
    </div>
    <div class="chat-input">
      <el-input v-model="inputContent" placeholder="输入消息..." @keyup.enter="sendMessage" />
      <el-button type="primary" @click="sendMessage">发送</el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axiosInstance from '../utils/axios'

const route = useRoute()
const currentUserId = Number(localStorage.getItem('currentUserId') || 0)
const targetUserId = Number(route.params.userId)
const targetUserName = ref('')
const messages = ref<any[]>([])
const inputContent = ref('')

const router = useRouter()
const closeChat = () => {
  router.back()
}

const formatTime = (t: string) => new Date(t).toLocaleString()

const loadTargetUserName = async () => {
  if (!targetUserId) return
  const res = await axiosInstance.get(`/user/${targetUserId}`)
  targetUserName.value = res.data.userName || '对方'
}

const loadMessages = async () => {
  if (!targetUserId) return
  const res = await axiosInstance.get(`/user-message/${currentUserId}/${targetUserId}`)
  messages.value = res.data
}

const sendMessage = async () => {
  if (!inputContent.value.trim() || !targetUserId) return
  await axiosInstance.post('/user-message', {
    senderId: currentUserId,
    receiverId: targetUserId,
    content: inputContent.value,
    sendTime: new Date().toISOString(),
    status: 0
  })
  inputContent.value = ''
  await loadMessages()
}

onMounted(async () => {
  await loadTargetUserName()
  await loadMessages()
})
</script>

<style scoped>
.chat-container { width: 700px; margin: 40px auto; border: 1px solid #eee; border-radius: 10px; background: #fff; box-shadow: 0 2px 8px #eee; }
.chat-header { padding: 16px; border-bottom: 1px solid #eee; }
.chat-messages { min-height: 300px; max-height: 400px; overflow-y: auto; padding: 16px; }
.my-message { justify-content: flex-end; }
.other-message { justify-content: flex-start; }
.msg-user { font-weight: bold; margin-right: 8px; }
.msg-content { padding: 6px 12px; border-radius: 8px; display: inline-block; max-width: 60%; word-break: break-all; }
.my-bubble { background: #e6f7ff; margin-left: 8px; }
.other-bubble { background: #f0f7ff; margin-right: 8px; }
.msg-time { color: #999; font-size: 12px; margin-left: 8px; }
.center-time { display: block; width: 100%; text-align: center; margin-bottom: 2px; margin-left: 0; }
.chat-input { display: flex; gap: 8px; padding: 16px; border-top: 1px solid #eee; }
</style>