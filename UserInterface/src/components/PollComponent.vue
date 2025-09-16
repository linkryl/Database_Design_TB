<!--
投票组件 - 仿贴吧投票功能
TreeHole开发组
-->

<template>
  <div class="poll-component">
    <!-- 投票标题 -->
    <div class="poll-header">
      <div class="poll-icon">📊</div>
      <div class="poll-title">{{ poll.title }}</div>
      <div class="poll-type">{{ poll.type === 'single' ? '单选' : '多选' }}</div>
    </div>

    <!-- 投票选项 -->
    <div class="poll-options">
      <div
        v-for="(option, index) in poll.options"
        :key="option.id"
        class="poll-option"
        :class="{ 
          selected: isSelected(option.id),
          disabled: hasVoted && !isSelected(option.id)
        }"
        @click="toggleOption(option.id)"
      >
        <div class="option-left">
          <div class="option-selector">
            <div v-if="poll.type === 'single'" class="radio-selector">
              <div class="radio-dot" :class="{ active: isSelected(option.id) }"></div>
            </div>
            <div v-else class="checkbox-selector">
              <div class="checkbox-box" :class="{ active: isSelected(option.id) }">
                <span v-if="isSelected(option.id)" class="checkmark">✓</span>
              </div>
            </div>
          </div>
          
          <div class="option-content">
            <div class="option-text">{{ option.text }}</div>
            <div v-if="option.description" class="option-desc">{{ option.description }}</div>
          </div>
        </div>

        <div class="option-right">
          <div class="vote-count">{{ option.voteCount }}</div>
          <div class="vote-percentage">{{ getPercentage(option.voteCount) }}%</div>
        </div>

        <!-- 投票进度条 -->
        <div class="vote-progress">
          <div 
            class="progress-bar" 
            :style="{ 
              width: `${getPercentage(option.voteCount)}%`,
              backgroundColor: getProgressColor(index)
            }"
          ></div>
        </div>
      </div>
    </div>

    <!-- 投票操作 -->
    <div class="poll-actions">
      <div class="poll-stats">
        <span class="total-votes">共 {{ totalVotes }} 人参与</span>
        <span class="poll-deadline" v-if="poll.deadline">
          截止时间：{{ formatDeadline(poll.deadline) }}
        </span>
      </div>
      
      <div class="action-buttons">
        <el-button 
          v-if="!hasVoted && !isPollExpired"
          type="primary" 
          size="small"
          @click="submitVote"
          :disabled="selectedOptions.length === 0"
          :loading="submitting"
        >
          提交投票
        </el-button>
        
        <el-button 
          v-if="hasVoted && canChangeVote"
          size="small"
          @click="changeVote"
        >
          修改投票
        </el-button>
        
        <el-button size="small" @click="showVoters = true">
          查看投票详情
        </el-button>
      </div>
    </div>

    <!-- 投票状态提示 -->
    <div v-if="hasVoted" class="vote-status">
      <div class="status-icon">✅</div>
      <div class="status-text">
        您已投票，感谢参与！
        <span v-if="userVoteTime">投票时间：{{ formatTime(userVoteTime) }}</span>
      </div>
    </div>

    <div v-if="isPollExpired" class="vote-status expired">
      <div class="status-icon">⏰</div>
      <div class="status-text">投票已截止</div>
    </div>

    <!-- 投票详情对话框 -->
    <el-dialog v-model="showVoters" title="投票详情" width="500px">
      <el-tabs v-model="activeVoterTab">
        <el-tab-pane
          v-for="option in poll.options"
          :key="option.id"
          :label="`${option.text} (${option.voteCount})`"
          :name="option.id.toString()"
        >
          <div class="voters-list">
            <div
              v-for="voter in option.voters || []"
              :key="voter.userId"
              class="voter-item"
            >
              <img :src="voter.avatarUrl || '/images/default-avatar.png'" class="voter-avatar">
              <div class="voter-info">
                <div class="voter-name">{{ voter.userName }}</div>
                <div class="voter-time">{{ formatTime(voter.voteTime) }}</div>
              </div>
              <div class="voter-level">Lv.{{ voter.level }}</div>
            </div>
          </div>
        </el-tab-pane>
      </el-tabs>
    </el-dialog>
  </div>
</template>

<script setup lang='ts'>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getCurrentUserId } from '@/utils/auth'

// Props
const props = defineProps<{
  pollData: any
  postId?: number
}>()

// Emits
const emit = defineEmits<{
  'vote-submitted': [voteData: any]
  'vote-changed': [voteData: any]
}>()

// 响应式数据
const poll = ref(props.pollData)
const selectedOptions = ref<number[]>([])
const hasVoted = ref(false)
const canChangeVote = ref(true)
const userVoteTime = ref<string>('')
const submitting = ref(false)
const showVoters = ref(false)
const activeVoterTab = ref('1')

// 当前用户ID
const currentUserId = ref(getCurrentUserId() ? parseInt(getCurrentUserId()!) : null)

// 计算属性
const totalVotes = computed(() => {
  return poll.value.options.reduce((sum: number, option: any) => sum + option.voteCount, 0)
})

const isPollExpired = computed(() => {
  if (!poll.value.deadline) return false
  return new Date(poll.value.deadline) < new Date()
})

// 组件挂载时初始化
onMounted(() => {
  checkUserVoteStatus()
})

// 检查用户投票状态
const checkUserVoteStatus = async () => {
  if (!currentUserId.value) return
  
  try {
    // 这里应该调用API检查用户是否已投票
    // 模拟数据
    hasVoted.value = false
    selectedOptions.value = []
    userVoteTime.value = ''
  } catch (error) {
    console.error('检查投票状态失败:', error)
  }
}

// 选项操作
const isSelected = (optionId: number): boolean => {
  return selectedOptions.value.includes(optionId)
}

const toggleOption = (optionId: number) => {
  if (hasVoted.value && !canChangeVote.value) return
  if (isPollExpired.value) return
  
  if (poll.value.type === 'single') {
    // 单选：清空其他选择
    selectedOptions.value = isSelected(optionId) ? [] : [optionId]
  } else {
    // 多选：切换选择状态
    const index = selectedOptions.value.indexOf(optionId)
    if (index > -1) {
      selectedOptions.value.splice(index, 1)
    } else {
      selectedOptions.value.push(optionId)
    }
  }
}

// 提交投票
const submitVote = async () => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  if (selectedOptions.value.length === 0) {
    ElMessage.warning('请选择投票选项')
    return
  }
  
  submitting.value = true
  
  try {
    // 调用API提交投票
    const voteData = {
      pollId: poll.value.id,
      postId: props.postId,
      userId: currentUserId.value,
      selectedOptions: selectedOptions.value,
      voteTime: new Date().toISOString()
    }
    
    // 更新本地数据
    selectedOptions.value.forEach(optionId => {
      const option = poll.value.options.find((opt: any) => opt.id === optionId)
      if (option) {
        option.voteCount++
      }
    })
    
    hasVoted.value = true
    userVoteTime.value = new Date().toISOString()
    
    emit('vote-submitted', voteData)
    ElMessage.success('投票提交成功')
    
  } catch (error) {
    console.error('提交投票失败:', error)
    ElMessage.error('投票失败，请重试')
  } finally {
    submitting.value = false
  }
}

// 修改投票
const changeVote = async () => {
  hasVoted.value = false
  selectedOptions.value = []
  ElMessage.info('可以重新选择投票选项')
}

// 工具函数
const getPercentage = (voteCount: number): number => {
  if (totalVotes.value === 0) return 0
  return Math.round((voteCount / totalVotes.value) * 100)
}

const getProgressColor = (index: number): string => {
  const colors = [
    '#409eff', '#67c23a', '#e6a23c', '#f56c6c', 
    '#909399', '#ff9a9e', '#87ceeb', '#dda0dd'
  ]
  return colors[index % colors.length]
}

const formatTime = (timestamp: string): string => {
  const date = new Date(timestamp)
  return date.toLocaleString('zh-CN')
}

const formatDeadline = (deadline: string): string => {
  const date = new Date(deadline)
  const now = new Date()
  const diff = date.getTime() - now.getTime()
  
  if (diff < 0) return '已截止'
  
  const days = Math.floor(diff / (1000 * 60 * 60 * 24))
  const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60))
  
  if (days > 0) return `${days}天${hours}小时后截止`
  if (hours > 0) return `${hours}小时后截止`
  
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))
  return `${minutes}分钟后截止`
}
</script>

<style scoped>
.poll-component {
  background: white;
  border: 1px solid #e4e7ed;
  border-radius: 8px;
  padding: 16px;
  margin: 16px 0;
}

.poll-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #f0f0f0;
}

.poll-icon {
  font-size: 20px;
}

.poll-title {
  flex: 1;
  font-size: 16px;
  font-weight: 600;
  color: #333;
}

.poll-type {
  background: #e8f4fd;
  color: #409eff;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.poll-options {
  margin-bottom: 16px;
}

.poll-option {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px;
  border: 1px solid #e4e7ed;
  border-radius: 8px;
  margin-bottom: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
  overflow: hidden;
}

.poll-option:hover:not(.disabled) {
  border-color: #409eff;
  box-shadow: 0 2px 4px rgba(64, 158, 255, 0.1);
}

.poll-option.selected {
  border-color: #409eff;
  background: #e8f4fd;
}

.poll-option.disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.option-left {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
}

.option-selector {
  flex-shrink: 0;
}

.radio-selector, .checkbox-selector {
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.radio-dot {
  width: 16px;
  height: 16px;
  border: 2px solid #dcdfe6;
  border-radius: 50%;
  transition: all 0.2s;
  position: relative;
}

.radio-dot.active {
  border-color: #409eff;
}

.radio-dot.active::after {
  content: '';
  width: 8px;
  height: 8px;
  background: #409eff;
  border-radius: 50%;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

.checkbox-box {
  width: 16px;
  height: 16px;
  border: 2px solid #dcdfe6;
  border-radius: 3px;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.checkbox-box.active {
  background: #409eff;
  border-color: #409eff;
}

.checkmark {
  color: white;
  font-size: 12px;
  font-weight: 600;
}

.option-content {
  flex: 1;
}

.option-text {
  font-size: 14px;
  color: #333;
  margin-bottom: 2px;
  line-height: 1.4;
}

.option-desc {
  font-size: 12px;
  color: #999;
  line-height: 1.3;
}

.option-right {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 2px;
  flex-shrink: 0;
  min-width: 60px;
}

.vote-count {
  font-size: 14px;
  font-weight: 600;
  color: #333;
}

.vote-percentage {
  font-size: 12px;
  color: #666;
}

.vote-progress {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 3px;
  background: #f0f0f0;
}

.progress-bar {
  height: 100%;
  transition: width 0.6s ease;
  border-radius: 0 3px 3px 0;
}

.poll-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 12px;
  border-top: 1px solid #f0f0f0;
}

.poll-stats {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.total-votes {
  font-size: 14px;
  color: #333;
  font-weight: 500;
}

.poll-deadline {
  font-size: 12px;
  color: #e6a23c;
}

.action-buttons {
  display: flex;
  gap: 8px;
}

.vote-status {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: #f0f9ff;
  border: 1px solid #b3d8ff;
  border-radius: 6px;
  margin-top: 12px;
}

.vote-status.expired {
  background: #fef0f0;
  border-color: #fbc4c4;
}

.status-icon {
  font-size: 16px;
}

.status-text {
  font-size: 13px;
  color: #666;
}

/* 投票详情对话框 */
.voters-list {
  max-height: 300px;
  overflow-y: auto;
}

.voter-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px;
  border-bottom: 1px solid #f0f0f0;
}

.voter-item:last-child {
  border-bottom: none;
}

.voter-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.voter-info {
  flex: 1;
}

.voter-name {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.voter-time {
  font-size: 12px;
  color: #999;
}

.voter-level {
  font-size: 12px;
  color: #409eff;
  background: #e8f4fd;
  padding: 2px 6px;
  border-radius: 8px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .poll-component {
    padding: 12px;
  }
  
  .poll-option {
    padding: 10px;
  }
  
  .option-left {
    gap: 8px;
  }
  
  .poll-actions {
    flex-direction: column;
    gap: 12px;
    align-items: stretch;
  }
  
  .action-buttons {
    justify-content: center;
    flex-wrap: wrap;
  }
  
  .option-right {
    min-width: 50px;
  }
  
  .vote-count {
    font-size: 13px;
  }
  
  .vote-percentage {
    font-size: 11px;
  }
}

/* 动画效果 */
@keyframes voteSubmitted {
  0% { transform: scale(1); }
  50% { transform: scale(1.05); }
  100% { transform: scale(1); }
}

.poll-option.selected {
  animation: voteSubmitted 0.3s ease;
}

/* 深色模式支持 */
@media (prefers-color-scheme: dark) {
  .poll-component {
    background: #2c2c2c;
    border-color: #404040;
  }
  
  .poll-title {
    color: white;
  }
  
  .option-text {
    color: white;
  }
  
  .vote-count {
    color: white;
  }
  
  .poll-option {
    border-color: #404040;
    background: #363636;
  }
  
  .poll-option:hover:not(.disabled) {
    border-color: #79bbff;
  }
  
  .poll-option.selected {
    background: #1a365d;
    border-color: #79bbff;
  }
  
  .vote-status {
    background: #1a365d;
    border-color: #2c5282;
    color: white;
  }
}
</style>
