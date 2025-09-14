<!--
  组件名称: UserCard.vue
  组件功能: 用户卡片组件
-->

<template>
  <div class='user-card' @click='handleClick'>
    <!-- 用户头像 -->
    <div class='avatar-section'>
      <el-avatar 
        class='user-avatar' 
        :src='`${ossBaseUrl}${userData.avatarUrl}`' 
        :size='80'>
        <el-icon :size='40'>
          <UserFilled/>
        </el-icon>
      </el-avatar>
      <div class='level-badge'>
        LV {{ currentLevel }}
      </div>
    </div>
    
    <!-- 用户信息 -->
    <div class='user-info'>
      <!-- 用户名 -->
      <div class='info-row'>
        <span class='info-label'>用户名</span>
        <span class='info-value user-name'>{{ userData.userName }}</span>
      </div>
      
      <!-- 用户ID -->
      <div class='info-row'>
        <span class='info-label'>用户ID</span>
        <span class='info-value user-id'>{{ props.userId }}</span>
      </div>
      
      <!-- 等级信息 -->
      <div class='info-row'>
        <span class='info-label'>等级</span>
        <span class='info-value level-info'>
          LV {{ currentLevel }} ({{ userData.experience || 0 }}经验)
        </span>
      </div>
      
      <!-- 关注数 -->
      <div class='info-row'>
        <span class='info-label'>关注</span>
        <span class='info-value'>{{ userData.followUser || 0 }}</span>
      </div>
      
      <!-- 粉丝数 -->
      <div class='info-row'>
        <span class='info-label'>粉丝</span>
        <span class='info-value'>{{ userData.followedCount || 0 }}</span>
      </div>
      
      <!-- 个人简介 -->
      <div class='info-row profile-row'>
        <span class='info-label'>简介</span>
        <span class='info-value profile-text'>
          {{ userData.profile || '这个人很神秘，什么都没有留下...' }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { computed, onMounted, ref, defineProps } from 'vue'
import { UserFilled } from '@element-plus/icons-vue'
import { ossBaseUrl } from '../globals'
import { useRouter } from 'vue-router'
import axiosInstance from '../utils/axios'

const props = defineProps<{ userId: number }>()
const router = useRouter()

const userData = ref({
  uid: 0,
  userName: '',
  role: 0,
  status: 0,
  avatarUrl: '',
  profile: '',
  gender: 0,
  birthdate: '',
  experience: 0,
  followBar: 0,
  followUser: 0,
  followedCount: 0,
  favoriteCount: 0,
  coinCount: 0
})

// 计算当前等级
const currentLevel = computed(() => {
  let level = 1
  let totalExp = 0
  const exp = userData.value.experience || 0
  
  while (totalExp + 100 * level <= exp) {
    totalExp += 100 * level
    level++
  }
  return level
})

// 获取用户数据
onMounted(async () => {
  try {
    const response = await axiosInstance.get(`user/${props.userId}`)
    // 适配个人资料页面的数据结构
    const data = response.data
    userData.value = {
      uid: data.userId || props.userId,
      userName: data.userName || '',
      role: data.role || 0,
      status: data.status || 0,
      avatarUrl: data.avatarUrl || 'default-avatar.png',
      profile: data.profile || '',
      gender: data.gender !== undefined ? data.gender : 0,
      birthdate: data.birthdate || '',
      experience: data.experiencePoints || data.experience || 0,
      followBar: data.followBarCount || data.followBar || 0,
      followUser: data.followsCount || data.followUser || 0,
      followedCount: data.followedCount || 0,
      favoriteCount: data.favoritesCount || data.favoriteCount || 0,
      coinCount: data.coinCount || data.coin_count || 0
    }
  } catch (error) {
    console.error('获取用户信息失败:', error)
  }
})

// 处理点击事件
const handleClick = () => {
  router.push(`/user/${props.userId}`)
}
</script>

<style scoped>
.user-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  border: 1px solid #e2e8f0;
  transition: all 0.3s ease;
  cursor: pointer;
  display: flex;
  gap: 20px;
  min-height: 200px;
  position: relative;
  overflow: hidden;
}

.user-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #2563eb, #3b82f6);
  transform: scaleX(0);
  transition: transform 0.3s ease;
}

.user-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 24px rgba(0,0,0,0.1);
  border-color: #cbd5e1;
}

.user-card:hover::before {
  transform: scaleX(1);
}

/* 头像区域 */
.avatar-section {
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.user-avatar {
  border: 3px solid #e2e8f0;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
  transition: all 0.3s ease;
}

.user-card:hover .user-avatar {
  border-color: #2563eb;
  transform: scale(1.05);
}

.level-badge {
  background: linear-gradient(135deg, #fbbf24, #f59e0b);
  color: white;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  box-shadow: 0 2px 8px rgba(245, 158, 11, 0.3);
  white-space: nowrap;
}

/* 用户信息区域 */
.user-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.info-row {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 8px 0;
  border-bottom: 1px solid #f1f5f9;
}

.info-row:last-child {
  border-bottom: none;
}

.info-label {
  color: #64748b;
  font-size: 13px;
  font-weight: 500;
  width: 60px;
  flex-shrink: 0;
  line-height: 1.5;
}

.info-value {
  color: #1e293b;
  font-size: 14px;
  font-weight: 600;
  flex: 1;
  line-height: 1.5;
}

/* 特殊样式 */
.user-name {
  color: #2563eb;
  font-size: 16px;
  font-weight: 700;
}

.user-id {
  font-family: 'Consolas', 'Monaco', monospace;
  background: #f1f5f9;
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 12px;
}

.level-info {
  color: #f59e0b;
}

.profile-row {
  flex: 1;
  align-items: flex-start;
}

.profile-text {
  font-size: 13px;
  color: #64748b;
  font-weight: 400;
  line-height: 1.6;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .user-card {
    flex-direction: column;
    text-align: center;
  }
  
  .avatar-section {
    align-self: center;
  }
  
  .info-row {
    justify-content: space-between;
    flex-direction: row;
  }
  
  .info-label {
    width: auto;
  }
  
  .profile-row {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }
}

@media (max-width: 480px) {
  .user-card {
    padding: 16px;
    gap: 16px;
  }
  
  .user-avatar {
    width: 60px;
    height: 60px;
  }
  
  .info-value {
    font-size: 13px;
  }
  
  .user-name {
    font-size: 15px;
  }
}
</style>