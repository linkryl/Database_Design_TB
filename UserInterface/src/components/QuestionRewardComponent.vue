<!--
悬赏问答组件 - 区别于普通帖子的问答功能
TreeHole开发组
-->

<template>
  <div class="question-reward-component">
    <!-- 悬赏问题头部 -->
    <div class="question-header">
      <div class="question-badge">
        <span class="badge-icon">❓</span>
        <span class="badge-text">悬赏问答</span>
        <span class="reward-amount">{{ question.rewardPoints }}积分</span>
      </div>
      
      <div class="question-status">
        <el-tag 
          :type="getStatusType(question.status)"
          :effect="question.status === 1 ? 'dark' : 'plain'"
        >
          {{ getStatusText(question.status) }}
        </el-tag>
        
        <div v-if="question.deadline" class="deadline-info">
          <span class="deadline-icon">⏰</span>
          <span class="deadline-text">{{ formatDeadline(question.deadline) }}</span>
        </div>
      </div>
    </div>

    <!-- 问题详情 -->
    <div class="question-details">
      <div class="question-meta">
        <div class="meta-item">
          <span class="meta-icon">🏷️</span>
          <span class="meta-text">{{ getQuestionTypeText(question.questionType) }}</span>
        </div>
        
        <div class="meta-item">
          <span class="meta-icon">📊</span>
          <span class="meta-text">难度：{{ getDifficultyText(question.difficultyLevel) }}</span>
        </div>
        
        <div class="meta-item">
          <span class="meta-icon">👁️</span>
          <span class="meta-text">{{ question.viewCount || 0 }}浏览</span>
        </div>
        
        <div class="meta-item">
          <span class="meta-icon">💬</span>
          <span class="meta-text">{{ question.answerCount || 0 }}回答</span>
        </div>
      </div>

      <!-- 最佳答案展示 -->
      <div v-if="question.bestAnswer" class="best-answer-section">
        <div class="best-answer-header">
          <span class="best-icon">🏆</span>
          <span class="best-text">最佳答案</span>
          <span class="reward-received">获得{{ question.rewardPoints }}积分</span>
        </div>
        
        <div class="best-answer-content">
          <div class="answerer-info">
            <img :src="question.bestAnswer.user.avatarUrl || '/images/default-avatar.png'" class="answerer-avatar">
            <div class="answerer-details">
              <div class="answerer-name">{{ question.bestAnswer.user.userName }}</div>
              <div class="answerer-level">Lv.{{ question.bestAnswer.user.level }}</div>
              <div class="answer-time">{{ formatTime(question.bestAnswer.adoptedTime) }}</div>
            </div>
          </div>
          
          <div class="answer-text">{{ question.bestAnswer.content }}</div>
          
          <div class="answer-stats">
            <span class="helpful-count">👍 {{ question.bestAnswer.helpfulCount }}人觉得有用</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 回答列表 -->
    <div class="answers-section">
      <div class="answers-header">
        <h4>{{ question.answerCount || 0 }}个回答</h4>
        <div class="sort-options">
          <el-radio-group v-model="sortBy" size="small" @change="sortAnswers">
            <el-radio-button value="time">最新</el-radio-button>
            <el-radio-button value="helpful">最有用</el-radio-button>
          </el-radio-group>
        </div>
      </div>

      <!-- 回答表单（仅问题进行中时显示） -->
      <div v-if="question.status === 1 && currentUserId && currentUserId !== question.questionerId" class="answer-form">
        <div class="form-header">
          <span class="form-icon">💡</span>
          <span class="form-title">写下您的回答</span>
          <span class="reward-hint">优质回答有机会获得{{ question.rewardPoints }}积分奖励</span>
        </div>
        
        <el-input
          v-model="newAnswerContent"
          type="textarea"
          placeholder="请详细回答问题，提供具体的解决方案..."
          :rows="4"
          maxlength="1000"
          show-word-limit
          :disabled="submittingAnswer"
        />
        
        <div class="form-actions">
          <div class="form-tools">
            <el-button size="small" @click="insertCodeBlock">💻 代码</el-button>
            <el-button size="small" @click="insertImage">🖼️ 图片</el-button>
            <el-button size="small" @click="insertLink">🔗 链接</el-button>
          </div>
          
          <el-button 
            type="primary" 
            @click="submitAnswer"
            :disabled="!newAnswerContent.trim()"
            :loading="submittingAnswer"
          >
            {{ submittingAnswer ? '提交中...' : '提交回答' }}
          </el-button>
        </div>
      </div>

      <!-- 回答列表 -->
      <div class="answers-list">
        <div
          v-for="answer in sortedAnswers"
          :key="answer.answerId"
          class="answer-item"
          :class="{ 'best-answer': answer.isBestAnswer }"
        >
          <div class="answer-header">
            <div class="answerer-info">
              <img :src="answer.user.avatarUrl || '/images/default-avatar.png'" class="answerer-avatar">
              <div class="answerer-details">
                <div class="answerer-name">{{ answer.user.userName }}</div>
                <div class="answerer-badges">
                  <span class="answerer-level">Lv.{{ answer.user.level }}</span>
                  <span v-if="answer.user.expertTags" class="expert-tag">{{ answer.user.expertTags }}</span>
                </div>
                <div class="answer-time">{{ formatTime(answer.createdTime) }}</div>
              </div>
            </div>
            
            <div class="answer-actions">
              <!-- 采纳按钮（仅提问者可见，且问题未解决） -->
              <el-button 
                v-if="canAdoptAnswer(answer)"
                type="success"
                size="small"
                @click="adoptAnswer(answer)"
                :loading="answer.adopting"
              >
                ✓ 采纳答案
              </el-button>
              
              <!-- 最佳答案标识 -->
              <div v-if="answer.isBestAnswer" class="best-answer-badge">
                <span class="best-icon">🏆</span>
                <span class="best-text">最佳答案</span>
              </div>
            </div>
          </div>
          
          <div class="answer-content">
            <div class="answer-text" v-html="processAnswerContent(answer.content)"></div>
            
            <!-- 答案附件 -->
            <div v-if="answer.attachments && answer.attachments.length > 0" class="answer-attachments">
              <div
                v-for="attachment in answer.attachments"
                :key="attachment.url"
                class="attachment-item"
              >
                <img v-if="attachment.type === 'image'" :src="attachment.url" class="attachment-image">
                <video v-else-if="attachment.type === 'video'" :src="attachment.url" controls class="attachment-video">
                <a v-else :href="attachment.url" target="_blank" class="attachment-link">
                  📎 {{ attachment.name }}
                </a>
              </div>
            </div>
          </div>
          
          <div class="answer-footer">
            <div class="evaluation-buttons">
              <button 
                class="eval-btn helpful-btn"
                :class="{ active: answer.userEvaluation === 1 }"
                @click="evaluateAnswer(answer, 1)"
                :disabled="!currentUserId || currentUserId === answer.answererId"
              >
                <span class="eval-icon">👍</span>
                <span class="eval-text">有用 ({{ answer.helpfulCount || 0 }})</span>
              </button>
              
              <button 
                class="eval-btn unhelpful-btn"
                :class="{ active: answer.userEvaluation === 0 }"
                @click="evaluateAnswer(answer, 0)"
                :disabled="!currentUserId || currentUserId === answer.answererId"
              >
                <span class="eval-icon">👎</span>
                <span class="eval-text">无用 ({{ answer.unhelpfulCount || 0 }})</span>
              </button>
            </div>
            
            <div class="answer-meta">
              <span v-if="answer.isBestAnswer" class="reward-info">
                💰 获得{{ answer.rewardReceived }}积分奖励
              </span>
              <span class="answer-id">#{{ answer.answerId }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 无回答提示 -->
      <div v-if="answers.length === 0" class="no-answers">
        <div class="no-answers-icon">🤔</div>
        <div class="no-answers-text">还没有人回答这个问题</div>
        <div class="no-answers-hint">成为第一个回答者，有机会获得{{ question.rewardPoints }}积分奖励！</div>
      </div>
    </div>

    <!-- 问题操作面板（提问者专用） -->
    <div v-if="currentUserId === question.questionerId && question.status === 1" class="question-controls">
      <div class="controls-header">
        <span class="controls-icon">⚙️</span>
        <span class="controls-title">问题管理</span>
      </div>
      
      <div class="controls-actions">
        <el-button size="small" @click="addReward">💰 追加悬赏</el-button>
        <el-button size="small" @click="extendDeadline">⏰ 延长时间</el-button>
        <el-button size="small" type="warning" @click="cancelQuestion">❌ 取消问题</el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getCurrentUserId } from '@/utils/auth'

// Props
const props = defineProps<{
  questionData: any
  postId?: number
}>()

// Emits
const emit = defineEmits<{
  'answer-adopted': [answerId: number, rewardPoints: number]
  'question-updated': [questionData: any]
}>()

// 响应式数据
const question = ref(props.questionData)
const answers = ref<any[]>([])
const newAnswerContent = ref('')
const submittingAnswer = ref(false)
const sortBy = ref('time')

// 当前用户
const currentUserId = ref(getCurrentUserId() ? parseInt(getCurrentUserId()!) : null)

// 计算属性
const sortedAnswers = computed(() => {
  const sorted = [...answers.value]
  
  if (sortBy.value === 'helpful') {
    return sorted.sort((a, b) => {
      // 最佳答案优先
      if (a.isBestAnswer && !b.isBestAnswer) return -1
      if (!a.isBestAnswer && b.isBestAnswer) return 1
      
      // 按有用票数排序
      return (b.helpfulCount || 0) - (a.helpfulCount || 0)
    })
  } else {
    return sorted.sort((a, b) => {
      // 最佳答案优先
      if (a.isBestAnswer && !b.isBestAnswer) return -1
      if (!a.isBestAnswer && b.isBestAnswer) return 1
      
      // 按时间排序
      return new Date(b.createdTime).getTime() - new Date(a.createdTime).getTime()
    })
  }
})

// 组件挂载时加载数据
onMounted(() => {
  loadAnswers()
})

// 加载回答列表
const loadAnswers = async () => {
  try {
    // 这里应该调用API获取回答列表
    // 模拟数据
    answers.value = [
      {
        answerId: 1,
        answererId: 2,
        content: '根据我的经验，这个问题可以通过以下方式解决：\n\n1. 首先检查配置文件\n2. 然后重启服务\n3. 最后验证结果\n\n希望对你有帮助！',
        answerType: 1,
        isBestAnswer: false,
        helpfulCount: 15,
        unhelpfulCount: 2,
        createdTime: new Date(Date.now() - 3600000).toISOString(),
        user: {
          userId: 2,
          userName: '技术专家',
          avatarUrl: '/images/avatar1.png',
          level: 8,
          expertTags: '后端开发'
        },
        userEvaluation: null, // 当前用户的评价：1有用 0无用 null未评价
        adopting: false
      },
      {
        answerId: 2,
        answererId: 3,
        content: '我遇到过类似问题，可以试试这个方法...',
        answerType: 1,
        isBestAnswer: true,
        helpfulCount: 28,
        unhelpfulCount: 1,
        rewardReceived: question.value.rewardPoints,
        adoptedTime: new Date(Date.now() - 1800000).toISOString(),
        createdTime: new Date(Date.now() - 7200000).toISOString(),
        user: {
          userId: 3,
          userName: '解决方案达人',
          avatarUrl: '/images/avatar2.png',
          level: 6,
          expertTags: '问题解决'
        },
        userEvaluation: 1,
        adopting: false
      }
    ]
    
    // 如果有最佳答案，更新问题数据
    const bestAnswer = answers.value.find(a => a.isBestAnswer)
    if (bestAnswer) {
      question.value.bestAnswer = bestAnswer
      question.value.status = 2 // 已解决
    }
    
  } catch (error) {
    console.error('加载回答失败:', error)
  }
}

// 提交回答
const submitAnswer = async () => {
  if (!newAnswerContent.value.trim()) {
    ElMessage.warning('请输入回答内容')
    return
  }
  
  submittingAnswer.value = true
  
  try {
    // 这里应该调用API提交回答
    const newAnswer = {
      answerId: Date.now(),
      answererId: currentUserId.value,
      content: newAnswerContent.value,
      answerType: 1,
      isBestAnswer: false,
      helpfulCount: 0,
      unhelpfulCount: 0,
      createdTime: new Date().toISOString(),
      user: {
        userId: currentUserId.value,
        userName: '当前用户',
        avatarUrl: '/images/default-avatar.png',
        level: 1
      },
      userEvaluation: null,
      adopting: false
    }
    
    answers.value.unshift(newAnswer)
    question.value.answerCount = (question.value.answerCount || 0) + 1
    
    newAnswerContent.value = ''
    ElMessage.success('回答提交成功！')
    
  } catch (error) {
    console.error('提交回答失败:', error)
    ElMessage.error('提交回答失败，请重试')
  } finally {
    submittingAnswer.value = false
  }
}

// 采纳答案
const adoptAnswer = async (answer: any) => {
  try {
    await ElMessageBox.confirm(
      `确定采纳这个回答为最佳答案吗？\n\n回答者将获得${question.value.rewardPoints}积分奖励，问题将标记为已解决。`,
      '采纳最佳答案',
      {
        confirmButtonText: '确认采纳',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    
    answer.adopting = true
    
    // 调用API采纳答案
    // 这里应该调用后端API
    
    // 更新状态
    answers.value.forEach(a => {
      a.isBestAnswer = a.answerId === answer.answerId
    })
    
    question.value.status = 2 // 已解决
    question.value.bestAnswer = answer
    question.value.bestAnswerId = answer.answerId
    question.value.resolvedTime = new Date().toISOString()
    
    answer.isBestAnswer = true
    answer.rewardReceived = question.value.rewardPoints
    answer.adoptedTime = new Date().toISOString()
    
    emit('answer-adopted', answer.answerId, question.value.rewardPoints)
    ElMessage.success(`已采纳答案，${answer.user.userName}获得${question.value.rewardPoints}积分奖励！`)
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('采纳答案失败:', error)
      ElMessage.error('采纳失败，请重试')
    }
  } finally {
    answer.adopting = false
  }
}

// 评价答案
const evaluateAnswer = async (answer: any, evaluationType: number) => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    return
  }
  
  if (currentUserId.value === answer.answererId) {
    ElMessage.warning('不能评价自己的回答')
    return
  }
  
  try {
    const oldEvaluation = answer.userEvaluation
    
    // 更新评价
    if (oldEvaluation === evaluationType) {
      // 取消评价
      answer.userEvaluation = null
      if (evaluationType === 1) {
        answer.helpfulCount = Math.max(0, answer.helpfulCount - 1)
      } else {
        answer.unhelpfulCount = Math.max(0, answer.unhelpfulCount - 1)
      }
    } else {
      // 新评价或改变评价
      if (oldEvaluation !== null) {
        // 移除旧评价
        if (oldEvaluation === 1) {
          answer.helpfulCount = Math.max(0, answer.helpfulCount - 1)
        } else {
          answer.unhelpfulCount = Math.max(0, answer.unhelpfulCount - 1)
        }
      }
      
      // 添加新评价
      answer.userEvaluation = evaluationType
      if (evaluationType === 1) {
        answer.helpfulCount = (answer.helpfulCount || 0) + 1
      } else {
        answer.unhelpfulCount = (answer.unhelpfulCount || 0) + 1
      }
    }
    
    ElMessage.success('评价成功')
    
  } catch (error) {
    console.error('评价答案失败:', error)
    ElMessage.error('评价失败，请重试')
  }
}

// 检查是否可以采纳答案
const canAdoptAnswer = (answer: any): boolean => {
  return currentUserId.value === question.value.questionerId && 
         question.value.status === 1 && 
         !answer.isBestAnswer &&
         answer.answererId !== question.value.questionerId
}

// 工具方法
const getStatusType = (status: number): string => {
  switch (status) {
    case 1: return 'primary' // 进行中
    case 2: return 'success' // 已解决
    case 3: return 'warning' // 已截止
    case 4: return 'info'    // 已取消
    default: return 'info'
  }
}

const getStatusText = (status: number): string => {
  switch (status) {
    case 1: return '进行中'
    case 2: return '已解决'
    case 3: return '已截止'
    case 4: return '已取消'
    default: return '未知'
  }
}

const getQuestionTypeText = (type: number): string => {
  switch (type) {
    case 1: return '技术问题'
    case 2: return '学习问题'
    case 3: return '生活问题'
    case 4: return '其他问题'
    default: return '未分类'
  }
}

const getDifficultyText = (level: number): string => {
  const levels = ['', '简单', '一般', '中等', '困难', '极难']
  return levels[level] || '未知'
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

const processAnswerContent = (content: string): string => {
  if (!content) return ''
  
  // 处理代码块
  content = content.replace(/```(\w+)?\n([\s\S]*?)```/g, '<pre><code class="language-$1">$2</code></pre>')
  
  // 处理行内代码
  content = content.replace(/`([^`]+)`/g, '<code>$1</code>')
  
  // 处理换行
  content = content.replace(/\n/g, '<br>')
  
  // 处理链接
  content = content.replace(/(https?:\/\/[^\s]+)/g, '<a href="$1" target="_blank">$1</a>')
  
  return content
}

// 排序回答
const sortAnswers = () => {
  // 触发计算属性重新计算
}

// 工具函数（占位实现）
const insertCodeBlock = () => {
  newAnswerContent.value += '\n```\n// 在此输入代码\n```\n'
}

const insertImage = () => {
  ElMessage.info('图片上传功能开发中')
}

const insertLink = () => {
  newAnswerContent.value += '[链接文字](链接地址)'
}

const addReward = () => {
  ElMessage.info('追加悬赏功能开发中')
}

const extendDeadline = () => {
  ElMessage.info('延长时间功能开发中')
}

const cancelQuestion = async () => {
  try {
    await ElMessageBox.confirm('确定要取消这个问题吗？悬赏积分将退还给您。', '取消问题')
    question.value.status = 4
    ElMessage.success('问题已取消')
  } catch (error) {
    // 用户取消操作
  }
}
</script>

<style scoped>
.question-reward-component {
  background: white;
  border: 2px solid #e6a23c;
  border-radius: 12px;
  padding: 20px;
  margin: 16px 0;
  position: relative;
}

.question-reward-component::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #e6a23c, #f7ba2a);
  border-radius: 12px 12px 0 0;
}

/* 问题头部 */
.question-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #f0f0f0;
}

.question-badge {
  display: flex;
  align-items: center;
  gap: 8px;
  background: linear-gradient(135deg, #e6a23c, #f7ba2a);
  color: white;
  padding: 8px 16px;
  border-radius: 20px;
  font-weight: 600;
}

.badge-icon {
  font-size: 16px;
}

.reward-amount {
  background: rgba(255, 255, 255, 0.2);
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 12px;
}

.question-status {
  display: flex;
  align-items: center;
  gap: 12px;
}

.deadline-info {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: #e6a23c;
}

/* 问题详情 */
.question-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 16px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 13px;
  color: #666;
}

.meta-icon {
  font-size: 14px;
}

/* 最佳答案 */
.best-answer-section {
  background: linear-gradient(135deg, #fff7e6, #fef7e0);
  border: 1px solid #e6a23c;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 20px;
}

.best-answer-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  font-weight: 600;
  color: #e6a23c;
}

.best-icon {
  font-size: 18px;
}

.reward-received {
  background: #e6a23c;
  color: white;
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 12px;
  margin-left: auto;
}

/* 回答部分 */
.answers-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-bottom: 8px;
  border-bottom: 1px solid #f0f0f0;
}

.answer-form {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 20px;
}

.form-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
}

.form-title {
  font-weight: 600;
  color: #333;
}

.reward-hint {
  margin-left: auto;
  font-size: 12px;
  color: #e6a23c;
  background: #fff7e6;
  padding: 2px 8px;
  border-radius: 10px;
}

.form-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 12px;
}

.form-tools {
  display: flex;
  gap: 8px;
}

/* 回答项 */
.answer-item {
  background: #fafafa;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 16px;
  transition: all 0.3s;
}

.answer-item:hover {
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.answer-item.best-answer {
  background: linear-gradient(135deg, #fff7e6, #fef7e0);
  border-color: #e6a23c;
  border-width: 2px;
}

.answer-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.answerer-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.answerer-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
}

.answerer-name {
  font-weight: 600;
  color: #333;
}

.answerer-badges {
  display: flex;
  gap: 6px;
  margin: 2px 0;
}

.answerer-level {
  background: #e8f4fd;
  color: #409eff;
  padding: 1px 6px;
  border-radius: 8px;
  font-size: 11px;
}

.expert-tag {
  background: #e6a23c;
  color: white;
  padding: 1px 6px;
  border-radius: 8px;
  font-size: 11px;
}

.answer-time {
  font-size: 12px;
  color: #999;
}

.best-answer-badge {
  display: flex;
  align-items: center;
  gap: 4px;
  background: #e6a23c;
  color: white;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

/* 回答内容 */
.answer-content {
  margin-bottom: 12px;
}

.answer-text {
  line-height: 1.6;
  color: #333;
  word-break: break-word;
}

.answer-text :deep(code) {
  background: #f1f2f3;
  padding: 2px 4px;
  border-radius: 3px;
  font-family: monospace;
  font-size: 13px;
}

.answer-text :deep(pre) {
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 4px;
  padding: 12px;
  overflow-x: auto;
  margin: 8px 0;
}

.answer-text :deep(a) {
  color: #409eff;
  text-decoration: none;
}

.answer-text :deep(a:hover) {
  text-decoration: underline;
}

/* 评价按钮 */
.answer-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 8px;
  border-top: 1px solid #f0f0f0;
}

.evaluation-buttons {
  display: flex;
  gap: 8px;
}

.eval-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  border: 1px solid #e4e7ed;
  background: white;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 12px;
  color: #666;
}

.eval-btn:hover:not(:disabled) {
  border-color: #409eff;
  color: #409eff;
}

.eval-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.eval-btn.active.helpful-btn {
  background: #e8f4fd;
  border-color: #409eff;
  color: #409eff;
}

.eval-btn.active.unhelpful-btn {
  background: #fef0f0;
  border-color: #f56c6c;
  color: #f56c6c;
}

.answer-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 12px;
  color: #999;
}

.reward-info {
  color: #e6a23c;
  font-weight: 600;
}

/* 无回答提示 */
.no-answers {
  text-align: center;
  padding: 40px 20px;
  color: #999;
}

.no-answers-icon {
  font-size: 48px;
  margin-bottom: 12px;
}

.no-answers-text {
  font-size: 16px;
  margin-bottom: 8px;
}

.no-answers-hint {
  font-size: 14px;
  color: #e6a23c;
}

/* 问题控制面板 */
.question-controls {
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 16px;
  margin-top: 20px;
}

.controls-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  font-weight: 600;
  color: #333;
}

.controls-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .question-header {
    flex-direction: column;
    gap: 12px;
    align-items: stretch;
  }
  
  .question-meta {
    flex-direction: column;
    gap: 8px;
  }
  
  .answers-header {
    flex-direction: column;
    gap: 8px;
    align-items: stretch;
  }
  
  .answer-header {
    flex-direction: column;
    gap: 8px;
    align-items: stretch;
  }
  
  .answer-footer {
    flex-direction: column;
    gap: 8px;
    align-items: stretch;
  }
  
  .evaluation-buttons {
    justify-content: center;
  }
  
  .controls-actions {
    justify-content: center;
  }
}
</style>
