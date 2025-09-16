<!--
移动端底部导航栏 - 仿贴吧App风格
TreeHole开发组
-->

<template>
  <div class="mobile-bottom-navbar" v-if="isMobile">
    <div class="navbar-container">
      <div
        v-for="item in navItems"
        :key="item.name"
        class="nav-item"
        :class="{ active: activeRoute === item.route }"
        @click="navigateTo(item.route)"
      >
        <div class="nav-icon">{{ item.icon }}</div>
        <div class="nav-label">{{ item.label }}</div>
        <div v-if="item.badge && item.badge > 0" class="nav-badge">
          {{ item.badge > 99 ? '99+' : item.badge }}
        </div>
      </div>
    </div>

    <!-- 发帖浮动按钮 -->
    <div class="floating-post-button" @click="showPostOptions = true">
      <div class="post-icon">✏️</div>
    </div>

    <!-- 发帖选项弹窗 -->
    <el-drawer
      v-model="showPostOptions"
      direction="btt"
      size="300px"
      :with-header="false"
    >
      <div class="post-options">
        <h3 class="options-title">选择发帖方式</h3>
        
        <div class="option-item" @click="createPost('text')">
          <div class="option-icon">📝</div>
          <div class="option-info">
            <div class="option-name">文字帖</div>
            <div class="option-desc">发表文字内容</div>
          </div>
        </div>
        
        <div class="option-item" @click="createPost('image')">
          <div class="option-icon">🖼️</div>
          <div class="option-info">
            <div class="option-name">图片帖</div>
            <div class="option-desc">分享图片内容</div>
          </div>
        </div>
        
        <div class="option-item" @click="createPost('video')">
          <div class="option-icon">🎥</div>
          <div class="option-info">
            <div class="option-name">视频帖</div>
            <div class="option-desc">上传视频内容</div>
          </div>
        </div>
        
        <div class="option-item" @click="createPost('poll')">
          <div class="option-icon">📊</div>
          <div class="option-info">
            <div class="option-name">投票帖</div>
            <div class="option-desc">发起投票讨论</div>
          </div>
        </div>
      </div>
    </el-drawer>
  </div>
</template>

<script setup lang='ts'>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getCurrentUserId } from '@/utils/auth'

// 路由
const router = useRouter()
const route = useRoute()

// 响应式数据
const showPostOptions = ref(false)
const unreadMessages = ref(0)
const unreadNotifications = ref(0)

// 检测是否为移动端
const isMobile = ref(false)

// 导航项配置
const navItems = ref([
  {
    name: 'home',
    label: '首页',
    icon: '🏠',
    route: '/home',
    badge: 0
  },
  {
    name: 'community',
    label: '社区',
    icon: '💬',
    route: '/CommunityPage',
    badge: 0
  },
  {
    name: 'bars',
    label: '贴吧',
    icon: '🏢',
    route: '/bars',
    badge: 0
  },
  {
    name: 'messages',
    label: '消息',
    icon: '✉️',
    route: '/messages',
    badge: computed(() => unreadMessages.value)
  },
  {
    name: 'profile',
    label: '我的',
    icon: '👤',
    route: computed(() => {
      const userId = getCurrentUserId()
      return userId ? `/profile/${userId}` : '/login'
    }),
    badge: computed(() => unreadNotifications.value)
  }
])

// 计算属性
const activeRoute = computed(() => {
  const path = route.path
  
  // 匹配当前路由到导航项
  if (path === '/' || path === '/home') return '/home'
  if (path === '/CommunityPage') return '/CommunityPage'
  if (path.startsWith('/bars') || path.startsWith('/bar/')) return '/bars'
  if (path.startsWith('/messages') || path.startsWith('/chat/')) return '/messages'
  if (path.startsWith('/profile/')) return navItems.value[4].route
  
  return path
})

// 组件挂载时初始化
onMounted(() => {
  checkIfMobile()
  loadUnreadCounts()
  
  // 监听窗口大小变化
  window.addEventListener('resize', checkIfMobile)
})

// 检测移动端
const checkIfMobile = () => {
  isMobile.value = window.innerWidth <= 768
}

// 加载未读数量
const loadUnreadCounts = async () => {
  try {
    // 这里应该调用API获取未读消息和通知数量
    unreadMessages.value = Math.floor(Math.random() * 5)
    unreadNotifications.value = Math.floor(Math.random() * 3)
  } catch (error) {
    console.error('加载未读数量失败:', error)
  }
}

// 导航到指定路由
const navigateTo = (route: string | any) => {
  const targetRoute = typeof route === 'string' ? route : route.value
  router.push(targetRoute)
}

// 创建帖子
const createPost = (type: string) => {
  showPostOptions.value = false
  
  // 根据类型跳转到不同的发帖页面
  switch (type) {
    case 'text':
      router.push('/PostNew?type=text')
      break
    case 'image':
      router.push('/PostNew?type=image')
      break
    case 'video':
      router.push('/PostNew?type=video')
      break
    case 'poll':
      router.push('/PostNew?type=poll')
      break
    default:
      router.push('/PostNew')
  }
}
</script>

<style scoped>
.mobile-bottom-navbar {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  background: white;
  border-top: 1px solid #e4e7ed;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.1);
  safe-area-inset-bottom: env(safe-area-inset-bottom);
}

.navbar-container {
  display: flex;
  height: 60px;
  padding: 0 8px;
}

.nav-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  padding: 6px;
  border-radius: 8px;
}

.nav-item:active {
  transform: scale(0.95);
  background: rgba(64, 158, 255, 0.1);
}

.nav-item.active {
  color: #409eff;
}

.nav-item.active .nav-icon {
  transform: scale(1.1);
}

.nav-icon {
  font-size: 20px;
  margin-bottom: 2px;
  transition: transform 0.2s ease;
}

.nav-label {
  font-size: 10px;
  font-weight: 500;
  line-height: 1;
}

.nav-badge {
  position: absolute;
  top: 2px;
  right: 8px;
  background: #f56c6c;
  color: white;
  font-size: 8px;
  padding: 1px 4px;
  border-radius: 8px;
  min-width: 14px;
  height: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
}

/* 浮动发帖按钮 */
.floating-post-button {
  position: absolute;
  top: -25px;
  left: 50%;
  transform: translateX(-50%);
  width: 50px;
  height: 50px;
  background: linear-gradient(135deg, #409eff, #67c23a);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(64, 158, 255, 0.4);
  transition: all 0.3s ease;
}

.floating-post-button:hover {
  transform: translateX(-50%) translateY(-2px);
  box-shadow: 0 6px 16px rgba(64, 158, 255, 0.5);
}

.floating-post-button:active {
  transform: translateX(-50%) scale(0.95);
}

.post-icon {
  font-size: 20px;
  color: white;
}

/* 发帖选项弹窗 */
.post-options {
  padding: 20px;
}

.options-title {
  text-align: center;
  margin-bottom: 20px;
  font-size: 18px;
  font-weight: 600;
  color: #333;
}

.option-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  background: #f8f9fa;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
  margin-bottom: 12px;
}

.option-item:hover {
  background: #e9ecef;
  transform: translateY(-1px);
}

.option-item:active {
  transform: scale(0.98);
}

.option-icon {
  font-size: 24px;
  width: 40px;
  text-align: center;
}

.option-info {
  flex: 1;
}

.option-name {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin-bottom: 4px;
}

.option-desc {
  font-size: 14px;
  color: #666;
}

/* 适配不同屏幕 */
@media (max-width: 480px) {
  .navbar-container {
    height: 55px;
    padding: 0 4px;
  }
  
  .nav-icon {
    font-size: 18px;
  }
  
  .nav-label {
    font-size: 9px;
  }
  
  .floating-post-button {
    width: 45px;
    height: 45px;
    top: -22px;
  }
  
  .post-icon {
    font-size: 18px;
  }
}

/* 深色模式支持 */
@media (prefers-color-scheme: dark) {
  .mobile-bottom-navbar {
    background: #2c2c2c;
    border-top-color: #404040;
  }
  
  .nav-item.active {
    color: #79bbff;
  }
  
  .nav-item:active {
    background: rgba(121, 187, 255, 0.1);
  }
  
  .option-item {
    background: #404040;
    color: white;
  }
  
  .option-item:hover {
    background: #4a4a4a;
  }
  
  .options-title {
    color: white;
  }
  
  .option-name {
    color: white;
  }
  
  .option-desc {
    color: #ccc;
  }
}
</style>
