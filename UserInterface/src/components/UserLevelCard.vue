<!--
用户等级卡片组件 - 显示用户等级信息和成长进度
TreeHole开发组
-->

<template>
  <div class="user-level-card">
    <div class="level-header">
      <div class="level-icon-container">
        <div class="level-icon" :style="{ color: levelInfo.levelColor }">
          {{ levelInfo.levelIcon }}
        </div>
        <div class="level-ring" :style="{ borderColor: levelInfo.levelColor }">
          <svg class="progress-ring" width="80" height="80">
            <circle
              class="progress-ring-bg"
              cx="40" cy="40" r="35"
              fill="transparent"
              stroke="#e5e7eb"
              stroke-width="4"
            />
            <circle
              class="progress-ring-fill"
              cx="40" cy="40" r="35"
              fill="transparent"
              :stroke="levelInfo.levelColor"
              stroke-width="4"
              stroke-linecap="round"
              :stroke-dasharray="circumference"
              :stroke-dashoffset="progressOffset"
              transform="rotate(-90 40 40)"
            />
          </svg>
        </div>
      </div>
      
      <div class="level-info">
        <div class="level-name">{{ levelInfo.levelName }}</div>
        <div class="level-title">{{ levelPrivileges.title }}</div>
        <div class="level-description">{{ levelPrivileges.description }}</div>
      </div>
    </div>

    <div class="experience-section">
      <div class="experience-bar-container">
        <div class="experience-labels">
          <span class="current-exp">{{ levelInfo.currentExperience }} EXP</span>
          <span class="next-level-exp" v-if="levelInfo.experienceToNextLevel > 0">
            还需 {{ levelInfo.experienceToNextLevel }} EXP 升级
          </span>
          <span class="max-level" v-else>已达最高等级</span>
        </div>
        
        <div class="experience-bar">
          <div 
            class="experience-progress" 
            :style="{ 
              width: `${levelInfo.progressPercentage}%`,
              backgroundColor: levelInfo.levelColor 
            }"
          ></div>
        </div>
      </div>
    </div>

    <div class="stats-section">
      <div class="stat-item">
        <div class="stat-icon">🏆</div>
        <div class="stat-content">
          <div class="stat-value">{{ levelInfo.badgeCount }}</div>
          <div class="stat-label">勋章数量</div>
        </div>
      </div>
      
      <div class="stat-item">
        <div class="stat-icon">📅</div>
        <div class="stat-content">
          <div class="stat-value">{{ levelInfo.consecutiveCheckInDays }}</div>
          <div class="stat-label">连续签到</div>
        </div>
      </div>
      
      <div class="stat-item" v-if="userRank">
        <div class="stat-icon">🎖️</div>
        <div class="stat-content">
          <div class="stat-value"># {{ userRank.experienceRank }}</div>
          <div class="stat-label">经验排名</div>
        </div>
      </div>
    </div>

    <!-- 签到按钮 -->
    <div class="action-section" v-if="isCurrentUser">
      <el-button 
        type="primary" 
        :disabled="checkInLoading || todayCheckedIn"
        @click="performCheckIn"
        class="check-in-btn"
        :class="{ 'checked-in': todayCheckedIn }"
      >
        <span v-if="checkInLoading">签到中...</span>
        <span v-else-if="todayCheckedIn">✓ 今日已签到</span>
        <span v-else>📅 立即签到</span>
      </el-button>
      
      <div class="check-in-reward" v-if="lastCheckInResult && lastCheckInResult.success">
        <div class="reward-text">
          签到成功！获得 {{ lastCheckInResult.experienceGained }} 经验值
          <span v-if="lastCheckInResult.bonusApplied" class="bonus-text">
            (连续签到奖励!)
          </span>
        </div>
      </div>
    </div>

    <!-- 等级特权展示 -->
    <div class="privileges-section" v-if="showPrivileges">
      <div class="privileges-header">
        <h4>等级特权</h4>
        <el-button 
          text 
          @click="showPrivileges = !showPrivileges"
          class="toggle-btn"
        >
          {{ showPrivileges ? '收起' : '展开' }}
        </el-button>
      </div>
      
      <div class="privileges-list">
        <div class="privilege-item">
          <div class="privilege-icon">📝</div>
          <div class="privilege-text">
            每日发帖限制: {{ levelInfo.dailyPostLimit }} 篇
          </div>
        </div>
        
        <div class="privilege-item">
          <div class="privilege-icon">💬</div>
          <div class="privilege-text">
            每日评论限制: {{ levelInfo.dailyCommentLimit }} 条
          </div>
        </div>
        
        <div class="privilege-item" v-if="levelInfo.canCreateBar">
          <div class="privilege-icon">🏠</div>
          <div class="privilege-text">可以创建贴吧</div>
        </div>
        
        <div class="privilege-item" v-if="levelInfo.canPinPost">
          <div class="privilege-icon">📌</div>
          <div class="privilege-text">可以置顶帖子</div>
        </div>
        
        <div class="privilege-item">
          <div class="privilege-icon">💾</div>
          <div class="privilege-text">
            存储配额: {{ formatFileSize(levelInfo.storageQuota) }}
          </div>
        </div>
      </div>
    </div>

    <!-- 升级动画效果 -->
    <div v-if="showLevelUpAnimation" class="level-up-animation">
      <div class="animation-content">
        <div class="level-up-icon">🎉</div>
        <div class="level-up-text">恭喜升级！</div>
        <div class="new-level">{{ levelInfo.levelName }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { ref, computed, onMounted, watch } from 'vue'
import { ElMessage } from 'element-plus'
import axios from 'axios'

// Props
const props = defineProps<{
  userId: number
  isCurrentUser?: boolean
  showDetails?: boolean
}>()

// 响应式数据
const levelInfo = ref<any>({
  userId: 0,
  currentLevel: 1,
  levelName: '新手',
  levelIcon: '🌱',
  levelColor: '#67c23a',
  currentExperience: 0,
  experienceToNextLevel: 100,
  progressPercentage: 0,
  badgeCount: 0,
  consecutiveCheckInDays: 0,
  privileges: {},
  dailyPostLimit: 5,
  dailyCommentLimit: 20,
  canCreateBar: false,
  canPinPost: false,
  storageQuota: 52428800
})

const userRank = ref<any>(null)
const checkInLoading = ref(false)
const todayCheckedIn = ref(false)
const lastCheckInResult = ref<any>(null)
const showPrivileges = ref(false)
const showLevelUpAnimation = ref(false)

// API基础URL
const apiBaseUrl = '/api'

// 计算属性
const circumference = computed(() => 2 * Math.PI * 35)
const progressOffset = computed(() => {
  const progress = levelInfo.value.progressPercentage / 100
  return circumference.value * (1 - progress)
})

const levelPrivileges = computed(() => {
  const privileges = levelInfo.value.privileges || {}
  return {
    title: privileges.title || '论坛用户',
    description: privileges.description || '欢迎来到树洞论坛'
  }
})

// 组件挂载时初始化
onMounted(async () => {
  await loadUserLevelInfo()
  if (props.isCurrentUser) {
    await checkTodayCheckInStatus()
  }
  if (props.showDetails) {
    await loadUserRank()
  }
})

// 监听用户ID变化
watch(() => props.userId, async (newUserId) => {
  if (newUserId) {
    await loadUserLevelInfo()
    if (props.showDetails) {
      await loadUserRank()
    }
  }
})

// 加载用户等级信息
const loadUserLevelInfo = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/user-level/${props.userId}`)
    const oldLevel = levelInfo.value.currentLevel
    levelInfo.value = response.data
    
    // 检查是否升级了
    if (oldLevel > 0 && response.data.currentLevel > oldLevel) {
      showLevelUpAnimation.value = true
      setTimeout(() => {
        showLevelUpAnimation.value = false
      }, 3000)
    }
  } catch (error: any) {
    console.error('加载用户等级信息失败:', error)
    if (error.response?.status !== 404) {
      ElMessage.error('加载等级信息失败')
    }
  }
}

// 加载用户排名
const loadUserRank = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/user-level/${props.userId}/rank`)
    userRank.value = response.data
  } catch (error) {
    console.error('加载用户排名失败:', error)
  }
}

// 检查今日签到状态
const checkTodayCheckInStatus = async () => {
  try {
    const today = new Date().toISOString().split('T')[0]
    const response = await axios.get(`${apiBaseUrl}/user-level/${props.userId}/check-in-history`, {
      params: { days: 1 }
    })
    
    const todayRecord = response.data.find((record: any) => 
      record.checkInDate.startsWith(today)
    )
    
    todayCheckedIn.value = !!todayRecord
  } catch (error) {
    console.error('检查签到状态失败:', error)
  }
}

// 执行签到
const performCheckIn = async () => {
  if (checkInLoading.value || todayCheckedIn.value) return

  checkInLoading.value = true
  
  try {
    const response = await axios.post(`${apiBaseUrl}/user-level/check-in`)
    lastCheckInResult.value = response.data
    
    if (response.data.success) {
      todayCheckedIn.value = true
      ElMessage.success(response.data.message)
      
      // 刷新等级信息
      await loadUserLevelInfo()
      
      // 显示奖励信息
      setTimeout(() => {
        lastCheckInResult.value = null
      }, 5000)
    } else {
      ElMessage.warning(response.data.message)
    }
  } catch (error: any) {
    console.error('签到失败:', error)
    ElMessage.error(error.response?.data?.message || '签到失败，请稍后重试')
  } finally {
    checkInLoading.value = false
  }
}

// 格式化文件大小
const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 B'
  
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i]
}

// 暴露方法给父组件
defineExpose({
  refreshLevelInfo: loadUserLevelInfo,
  performCheckIn
})
</script>

<style scoped>
.user-level-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 16px;
  padding: 24px;
  color: white;
  position: relative;
  overflow: hidden;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
}

.user-level-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(255,255,255,0.1) 0%, transparent 50%);
  pointer-events: none;
}

.level-header {
  display: flex;
  align-items: center;
  margin-bottom: 24px;
  gap: 20px;
}

.level-icon-container {
  position: relative;
  flex-shrink: 0;
}

.level-icon {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 32px;
  z-index: 2;
}

.level-ring {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  position: relative;
}

.progress-ring {
  transform: rotate(-90deg);
}

.progress-ring-fill {
  transition: stroke-dashoffset 0.6s ease-in-out;
}

.level-info {
  flex: 1;
}

.level-name {
  font-size: 28px;
  font-weight: 700;
  margin-bottom: 4px;
  text-shadow: 0 2px 4px rgba(0,0,0,0.3);
}

.level-title {
  font-size: 16px;
  opacity: 0.9;
  margin-bottom: 4px;
  font-weight: 500;
}

.level-description {
  font-size: 14px;
  opacity: 0.8;
  line-height: 1.4;
}

.experience-section {
  margin-bottom: 24px;
}

.experience-bar-container {
  background: rgba(255, 255, 255, 0.15);
  border-radius: 12px;
  padding: 16px;
  backdrop-filter: blur(10px);
}

.experience-labels {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  font-size: 14px;
}

.current-exp {
  font-weight: 600;
}

.next-level-exp, .max-level {
  opacity: 0.9;
}

.experience-bar {
  height: 8px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 4px;
  overflow: hidden;
}

.experience-progress {
  height: 100%;
  border-radius: 4px;
  transition: width 0.6s ease-in-out;
  box-shadow: 0 0 8px rgba(255, 255, 255, 0.3);
}

.stats-section {
  display: flex;
  justify-content: space-around;
  margin-bottom: 20px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 16px;
  backdrop-filter: blur(10px);
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 8px;
  text-align: center;
}

.stat-icon {
  font-size: 20px;
}

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-size: 18px;
  font-weight: 600;
  line-height: 1.2;
}

.stat-label {
  font-size: 12px;
  opacity: 0.8;
}

.action-section {
  text-align: center;
  margin-bottom: 20px;
}

.check-in-btn {
  width: 100%;
  height: 48px;
  font-size: 16px;
  font-weight: 600;
  border: none;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.2);
  color: white;
  backdrop-filter: blur(10px);
  transition: all 0.3s ease;
}

.check-in-btn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.3);
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
}

.check-in-btn.checked-in {
  background: rgba(76, 175, 80, 0.3);
  cursor: default;
}

.check-in-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.check-in-reward {
  margin-top: 12px;
  padding: 8px 16px;
  background: rgba(76, 175, 80, 0.2);
  border-radius: 8px;
  border: 1px solid rgba(76, 175, 80, 0.3);
}

.reward-text {
  font-size: 14px;
  color: #c8e6c9;
}

.bonus-text {
  color: #ffeb3b;
  font-weight: 600;
}

.privileges-section {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 16px;
  backdrop-filter: blur(10px);
}

.privileges-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.privileges-header h4 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
}

.toggle-btn {
  color: white;
  font-size: 14px;
}

.privileges-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.privilege-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  opacity: 0.9;
}

.privilege-icon {
  font-size: 16px;
  width: 20px;
  text-align: center;
}

.privilege-text {
  flex: 1;
}

/* 升级动画 */
.level-up-animation {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(255, 193, 7, 0.95) 0%, rgba(255, 152, 0, 0.95) 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 16px;
  z-index: 100;
  animation: levelUpFadeIn 0.5s ease-out;
}

.animation-content {
  text-align: center;
  animation: levelUpBounce 0.6s ease-out 0.2s both;
}

.level-up-icon {
  font-size: 48px;
  margin-bottom: 12px;
}

.level-up-text {
  font-size: 24px;
  font-weight: 700;
  margin-bottom: 8px;
  text-shadow: 0 2px 4px rgba(0,0,0,0.3);
}

.new-level {
  font-size: 20px;
  font-weight: 600;
  opacity: 0.9;
}

@keyframes levelUpFadeIn {
  from {
    opacity: 0;
    transform: scale(0.8);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes levelUpBounce {
  0% {
    transform: scale(0.3) translateY(100px);
    opacity: 0;
  }
  50% {
    transform: scale(1.05) translateY(-10px);
  }
  70% {
    transform: scale(0.95) translateY(0);
  }
  100% {
    transform: scale(1) translateY(0);
    opacity: 1;
  }
}

/* 响应式设计 */
@media (max-width: 768px) {
  .user-level-card {
    padding: 20px;
  }
  
  .level-header {
    flex-direction: column;
    text-align: center;
    gap: 16px;
  }
  
  .level-name {
    font-size: 24px;
  }
  
  .stats-section {
    flex-direction: column;
    gap: 12px;
  }
  
  .stat-item {
    justify-content: center;
  }
  
  .privileges-list {
    gap: 12px;
  }
  
  .privilege-item {
    flex-direction: column;
    text-align: center;
    gap: 4px;
  }
}

@media (max-width: 480px) {
  .user-level-card {
    padding: 16px;
  }
  
  .level-icon-container {
    transform: scale(0.8);
  }
  
  .level-name {
    font-size: 20px;
  }
  
  .experience-bar-container {
    padding: 12px;
  }
  
  .stats-section {
    padding: 12px;
  }
  
  .check-in-btn {
    height: 44px;
    font-size: 15px;
  }
}
</style>
