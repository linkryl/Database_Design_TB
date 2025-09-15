<!--
  组件名称: ProfileBarCard.vue
  组件功能: 贴吧卡片组件
-->

<template>
  <div class='bar-card' @click='handleClick'>
    <!-- 贴吧头像 -->
    <div class='bar-avatar-wrapper'>
      <el-avatar 
        class='bar-avatar' 
        :src='barAvatarUrl' 
        :size='60'>
        <el-icon :size='30'>
          <House/>
        </el-icon>
      </el-avatar>
    </div>
    
    <!-- 贴吧信息 -->
    <div class='bar-info'>
      <div class='bar-header'>
        <h3 class='bar-name'>{{ barData.barName }}</h3>
        <div class='bar-badges'>
          <el-tag v-if='isOwner' type='warning' size='small'>
            <el-icon><Trophy/></el-icon>
            吧主
          </el-tag>
          <el-tag v-if='barData.status === 1' type='info' size='small'>
            已归档
          </el-tag>
          <el-tag v-if='barData.status === 2' type='danger' size='small'>
            已解散
          </el-tag>
        </div>
      </div>
      
      <p class='bar-description'>{{ barData.description || '这个贴吧还没有简介...' }}</p>
      
      <div class='bar-meta'>
        <span class='meta-item'>
          <el-icon><User/></el-icon>
          吧主：{{ ownerName }}
        </span>
        <span class='meta-item'>
          <el-icon><UserFilled/></el-icon>
          {{ formatNumber(barData.followedCount) }} 关注
        </span>
        <span class='meta-item'>
          <el-icon><Document/></el-icon>
          {{ formatNumber(barData.postCount) }} 帖子
        </span>
        <span class='meta-item'>
          <el-icon><Calendar/></el-icon>
          {{ formatTime }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { computed, onMounted, ref, defineProps } from 'vue'
import { 
  House, Trophy, User, UserFilled,
  Document, Calendar
} from '@element-plus/icons-vue'
import { formatDateTimeToCST, ossBaseUrl } from '../globals'
import { useRouter } from 'vue-router'
import axiosInstance from '../utils/axios'

// Props定义
const props = defineProps<{ 
  barId: number 
}>()

const router = useRouter()

// 贴吧数据结构（与 THBar 接口保持一致）
const barData = ref({
  barId: 0,
  ownerId: 0,
  barName: '',
  description: '',
  avatarUrl: '',
  coverUrl: '',
  creationDate: '',
  updateDate: '',
  status: 0, // 0=正常，1=归档，2=已解散
  followedCount: 0,
  postCount: 0,
  rules: '',
  tags: ''
})

// 吧主信息
const ownerName = ref('加载中...')
const isOwner = ref(false)

// 计算属性
const barAvatarUrl = computed(() => {
  if (barData.value.avatarUrl) {
    return `${ossBaseUrl}${barData.value.avatarUrl}`
  }
  return ''
})

const formatTime = computed(() => {
  if (!barData.value.creationDate) return ''
  try {
    const result = formatDateTimeToCST(barData.value.creationDate)
    return result.date
  } catch {
    return '未知时间'
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
const fetchBarData = async () => {
  try {
    // 获取贴吧信息
    const response = await axiosInstance.get(`bar/${props.barId}`)
    const data = response.data
    
    // 更新贴吧数据，确保与 THBar 接口一致
    barData.value = {
      barId: data.barId || props.barId,
      ownerId: data.ownerId || 0,
      barName: data.barName || '',
      description: data.description || '',
      avatarUrl: data.avatarUrl || '',
      coverUrl: data.coverUrl || '',
      creationDate: data.creationDate || '',
      updateDate: data.updateDate || '',
      status: data.status !== undefined ? data.status : 0,
      followedCount: data.followedCount || 0,
      postCount: data.postCount || 0,
      rules: data.rules || '',
      tags: data.tags || ''
    }
    
    // 获取吧主信息
    if (barData.value.ownerId) {
      try {
        const ownerResponse = await axiosInstance.get(`user/${barData.value.ownerId}`)
        ownerName.value = ownerResponse.data.userName || '未知用户'
        
        // 检查是否是吧主
        const currentUserId = localStorage.getItem('currentUserId')
        if (currentUserId) {
          isOwner.value = parseInt(currentUserId) === barData.value.ownerId
        }
      } catch (error) {
        console.error('获取吧主信息失败:', error)
        ownerName.value = '未知用户'
      }
    }
  } catch (error) {
    console.error('获取贴吧信息失败:', error)
    // 设置默认值
    barData.value = {
      barId: props.barId,
      ownerId: 0,
      barName: '加载失败',
      description: '无法加载贴吧信息',
      avatarUrl: '',
      coverUrl: '',
      creationDate: '',
      updateDate: '',
      status: 0,
      followedCount: 0,
      postCount: 0,
      rules: '',
      tags: ''
    }
    ownerName.value = '未知'
  }
}

// 处理点击事件
const handleClick = () => {
  router.push(`/bar/${props.barId}`)
}

// 组件挂载时获取数据
onMounted(() => {
  fetchBarData()
})
</script>

<style scoped>
.bar-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  border: 1px solid #e2e8f0;
  transition: all 0.3s;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 20px;
  width: 100%;
}

.bar-card:hover {
  transform: translateX(4px);
  box-shadow: 0 6px 16px rgba(0,0,0,0.08);
  border-color: #cbd5e1;
}

/* 贴吧头像 */
.bar-avatar-wrapper {
  flex-shrink: 0;
}

.bar-avatar {
  border: 2px solid #f1f5f9;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
}

/* 贴吧信息 */
.bar-info {
  flex: 1;
  min-width: 0; /* 防止内容溢出 */
}

.bar-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}

.bar-name {
  font-size: 18px;
  font-weight: 600;
  color: #1e293b;
  margin: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 300px;
}

.bar-badges {
  display: flex;
  gap: 6px;
  flex-shrink: 0;
}

.bar-badges .el-tag {
  display: flex;
  align-items: center;
  gap: 2px;
}

.bar-description {
  font-size: 14px;
  color: #475569;
  line-height: 1.5;
  margin: 0 0 12px 0;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.bar-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  font-size: 13px;
  color: #64748b;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.meta-item .el-icon {
  font-size: 14px;
}

/* 操作按钮 */
.bar-actions {
  flex-shrink: 0;
}

.bar-actions .el-button {
  min-width: 90px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .bar-card {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .bar-header {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .bar-name {
    max-width: 100%;
  }
  
  .bar-meta {
    flex-direction: column;
    gap: 8px;
  }
  
  .bar-actions {
    width: 100%;
    margin-top: 12px;
  }
  
  .bar-actions .el-button {
    width: 100%;
  }
}
</style>