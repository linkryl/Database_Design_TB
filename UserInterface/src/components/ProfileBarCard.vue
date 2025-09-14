<!--
  组件名称: BarCard.vue
  组件功能: 贴吧卡片组件
  暂定信息组件
-->

<template>
  <div class='bar-card' @click='handleClick'>
    <div class='bar-header'>
      <div class='bar-icon'>
        <el-icon :size='40'>
          <House/>
        </el-icon>
      </div>
      <div class='bar-badge' v-if='isOwner'>
        <el-icon><Trophy/></el-icon>
        吧主
      </div>
    </div>
    
    <div class='bar-body'>
      <h3 class='bar-name'>{{ barData.barName }}</h3>
      <p class='bar-description'>{{ barData.description || '这个贴吧还没有简介...' }}</p>
    </div>
    
    <div class='bar-stats'>
      <div class='stat-row'>
        <div class='stat-item'>
          <span class='stat-value'>{{ formatNumber(barData.memberCount || 0) }}</span>
          <span class='stat-label'>成员</span>
        </div>
        <div class='stat-item'>
          <span class='stat-value'>{{ formatNumber(barData.postCount || 0) }}</span>
          <span class='stat-label'>帖子</span>
        </div>
        <div class='stat-item'>
          <span class='stat-value'>{{ formatNumber(barData.todayPosts || 0) }}</span>
          <span class='stat-label'>今日</span>
        </div>
      </div>
    </div>
    
    <div class='bar-footer'>
      <div class='bar-tags'>
        <el-tag v-if='barData.isHot' type='danger' size='small'>
          <el-icon><Flame/></el-icon>
          热门
        </el-tag>
        <el-tag v-if='barData.isRecommended' type='success' size='small'>
          <el-icon><Star/></el-icon>
          推荐
        </el-tag>
        <el-tag v-if='barData.category' size='small'>
          {{ barData.category }}
        </el-tag>
      </div>
      <span class='create-time'>{{ formatTime }}</span>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { computed, onMounted, ref, defineProps } from 'vue'
import { 
  House, Trophy, Star 
} from '@element-plus/icons-vue'
import { formatDateTimeToCST } from '../globals'
import { useRouter } from 'vue-router'
import axiosInstance from '../utils/axios'

const props = defineProps<{ barId: number }>()
const router = useRouter()

const barData = ref({
  barId: 0,
  barName: '',
  description: '',
  category: '',
  memberCount: 0,
  postCount: 0,
  todayPosts: 0,
  createTime: '',
  isHot: false,
  isRecommended: false,
  ownerId: 0
})

const isOwner = ref(false)

// 计算属性
const formatTime = computed(() => {
  if (!barData.value.createTime) return ''
  try {
    const result = formatDateTimeToCST(barData.value.createTime)
    return result.date
  } catch {
    return ''
  }
})

// 格式化数字
const formatNumber = (num: number): string => {
  if (num >= 10000) {
    return (num / 10000).toFixed(1) + 'w'
  } else if (num >= 1000) {
    return (num / 1000).toFixed(1) + 'k'
  }
  return num.toString()
}

// 获取贴吧数据
onMounted(async () => {
  try {
    const response = await axiosInstance.get(`bar/${props.barId}`)
    barData.value = response.data
    
    // 检查是否是吧主
    const currentUserId = localStorage.getItem('currentUserId')
    if (currentUserId && barData.value.ownerId) {
      isOwner.value = parseInt(currentUserId) === barData.value.ownerId
    }
  } catch (error) {
    console.error('获取贴吧信息失败:', error)
  }
})

// 处理点击事件
const handleClick = () => {
  router.push(`/bar/${props.barId}`)
}
</script>

<style scoped>
.bar-card {
  background: white;
  border-radius: 12px;
  padding: 24px;
  border: 1px solid #e2e8f0;
  transition: all 0.3s;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  position: relative;
  overflow: hidden;
}

.bar-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 100px;
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  opacity: 0.05;
  transition: opacity 0.3s;
}

.bar-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 24px rgba(0,0,0,0.08);
  border-color: #cbd5e1;
}

.bar-card:hover::before {
  opacity: 0.1;
}

.bar-header {
  display: flex;
  justify-content: center;
  margin-bottom: 20px;
  position: relative;
  z-index: 1;
}

.bar-icon {
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.25);
}

.bar-badge {
  position: absolute;
  top: -5px;
  right: -5px;
  background: linear-gradient(135deg, #fbbf24, #f59e0b);
  color: white;
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 4px;
  box-shadow: 0 2px 8px rgba(245, 158, 11, 0.3);
}

.bar-body {
  flex: 1;
  text-align: center;
  margin-bottom: 20px;
  z-index: 1;
}

.bar-name {
  font-size: 20px;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 12px 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.bar-description {
  font-size: 14px;
  color: #475569;
  line-height: 1.5;
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  min-height: 42px;
}

.bar-stats {
  margin-bottom: 20px;
  z-index: 1;
}

.stat-row {
  display: flex;
  justify-content: space-around;
  padding: 16px;
  background: #f8fafc;
  border-radius: 10px;
}

.stat-item {
  text-align: center;
}

.stat-value {
  display: block;
  font-size: 20px;
  font-weight: 700;
  color: #2563eb;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 12px;
  color: #64748b;
  font-weight: 500;
}

.bar-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16px;
  border-top: 1px solid #e2e8f0;
  z-index: 1;
}

.bar-tags {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.bar-tags .el-tag {
  display: flex;
  align-items: center;
  gap: 4px;
}

.create-time {
  font-size: 12px;
  color: #94a3b8;
  flex-shrink: 0;
}
</style>