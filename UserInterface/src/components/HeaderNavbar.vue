<!--
顶部导航栏组件
2352031 古振
-->

<template>
  <el-menu :default-active='activeIndex' class='header-navbar' mode='horizontal' :ellipsis='false'>

    <!--主页按钮-->
    <el-container ref='tourRef1' class='logo-title' @click="router.push('/')">
      <img :src='`${ossBaseUrl}TreeHoleLogo.png`' style='height: 45px; margin-left: 10px' alt='TreeHoleLogo'>
      <h1 style='white-space: nowrap'>树洞 Treehole</h1>
    </el-container>
    
    <!--间隔-->
    <div style='margin-left: 60px'></div>

    <!--社区按钮-->
    <el-menu-item ref='tourRef2' index='1' class='navbar-item' @click="router.push('/CommunityPage')">
      {{"树洞社区" }}
    </el-menu-item>

    <!--贴吧按钮-->
    <el-menu-item index='2' class='navbar-item' @click="router.push('/bars')">
      {{"贴吧广场" }}
    </el-menu-item>

    <!--商店按钮-->
    <el-menu-item index='3' class='navbar-item' @click="router.push('/market')">
      {{"装扮商城" }}
    </el-menu-item>

    <!--群聊按钮-->
    <el-menu-item v-if="isLoggedIn" index='4' class='navbar-item' @click="router.push('/group-list')">
      {{"群聊" }}
    </el-menu-item>

    <!--超级占位符-->
    <div class='flex-grow'/>

    <!-- 用户信息显示 -->
    <div v-if="currentUserId !== 0" class="user-info-display">
      <span class="user-name">{{ currentUserName || '用户' }}</span>
    </div>

    <el-dropdown ref='tourRef3' size='large'>
      
      <!--头像显示-->
      <el-avatar class='avatar' :src='`${ossBaseUrl}GitHubLogo.png`'/>

      <template #dropdown>

        <!--未登录下拉栏 -->
        <el-dropdown-menu style='width: 210px' v-if='!isLoggedIn'>
          <h2 style='text-align: center'>{{ "用户中心" }}</h2>

          <!--登陆按钮-->
          <el-dropdown-item :icon='Connection' @click="router.push('/login')">
            <div class='dropdown-item'>
              <span>{{ "登陆" }}</span>
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>
            </div>
          </el-dropdown-item>

          <!--注册按钮-->
          <el-dropdown-item :icon='CirclePlus' @click="router.push('/register')">
            <div class='dropdown-item'>
              <span>{{ "注册" }}</span>
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>
            </div>
          </el-dropdown-item>

          <!--管理员登录按钮-->
          <el-dropdown-item :icon='Setting' @click="router.push('/admin-login')">
            <div class='dropdown-item'>
              <span>{{ "管理员登录" }}</span>
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>
            </div>
          </el-dropdown-item>

        </el-dropdown-menu>

        <!--已登录下拉栏-->
        <el-dropdown-menu v-else style='width: 250px'>

          <h2 style='text-align: center'>{{ "用户中心" }}</h2>
          
          <!--用户身份提示-->
          <div class="user-indicator" :class="{ 'admin-indicator': isAdmin, 'normal-user-indicator': !isAdmin }">
            <el-icon :size="16" :color="isAdmin ? '#ff6b6b' : '#4a90e2'">
              <Setting v-if="isAdmin" />
              <User v-else />
            </el-icon>
            <span class="user-text">{{ isAdmin ? '管理员模式' : '普通用户' }}</span>
          </div>

          <!--个人中心按钮-->  
          <el-dropdown-item :icon='User' @click="router.push(`/profile/${currentUserId}`)">  
            <div class='dropdown-item'>          
              <span>{{ "个人中心" }}</span>         
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>    
            </div>
  
          </el-dropdown-item>

          <!--群聊按钮-->
          <el-dropdown-item :icon='ChatDotRound' @click="router.push('/group-list')">  
            <div class='dropdown-item'>          
              <span>{{ "群聊" }}</span>         
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>    
            </div>
          </el-dropdown-item>


          <!--用户管理按钮（仅管理员可见）-->
          <el-dropdown-item v-if="isAdmin" :icon='Setting' @click="router.push('/user-management')">  
            <div class='dropdown-item'>          
              <span>{{ "用户管理" }}</span>         
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>    
            </div>
          </el-dropdown-item>
  

          <!--注销账号按钮（仅普通用户可见）-->
          <el-dropdown-item v-if="!isAdmin" :icon='Delete' @click='handleDeleteAccount' class='delete-account-item'>
            <div class='dropdown-item'>
              <span>{{ "注销账号" }}</span>
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>
            </div>

          </el-dropdown-item>

          <!--退出-->
          <el-dropdown-item :icon='Link' @click='logout'>
            <div class='dropdown-item'>
              <span>{{ "退出登陆" }}</span>
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>
            </div>
          </el-dropdown-item>
        </el-dropdown-menu>


      </template>

    </el-dropdown>

  </el-menu>
</template>

<script setup lang='ts'>
import {onMounted, onUnmounted, ref, watch, computed} from 'vue'
import {useRouter, useRoute} from 'vue-router'
import axiosInstance from '../utils/axios'
import {ElMessage, ElMessageBox} from 'element-plus'
import {
  ossBaseUrl,
} from '../globals'
import {
  User,
  Connection,
  Link,
  CirclePlus,
  ArrowRightBold,
  Setting,
  Delete,
  ChatDotRound,
} from '@element-plus/icons-vue'

const activeIndex = ref('0')
const router = useRouter()
const route = useRoute()
const storedValue = localStorage.getItem('currentUserId')
const storedUserId = storedValue ? parseInt(storedValue) : 0
const currentUserId = ref(isNaN(storedUserId) ? 0 : storedUserId)
const currentUserName = ref('')
const isAdmin = ref(false)

// 响应式变量，用于触发重新计算
const authState = ref(0)

// 计算属性：判断用户是否已登录
const isLoggedIn = computed(() => {
  // 使用authState来触发重新计算
  authState.value
  const token = localStorage.getItem('jwtToken')
  const userId = localStorage.getItem('currentUserId')
  return !!(token && userId && userId !== '0')
})

/*跳转到论坛页面*/
watch(route, (newRoute) => {
  if (newRoute.path === '/CommunityPage') {
    activeIndex.value = '1' // 社区页面高亮
  } else if (newRoute.path === '/bars' || newRoute.path.startsWith('/bar/')) {
    activeIndex.value = '2' // 贴吧页面高亮
  } else if (newRoute.path === '/market') {
    activeIndex.value = '3' // 商店页面高亮
  } else if (newRoute.path === '/group-list' || newRoute.path.startsWith('/group-chat/') || newRoute.path.startsWith('/group/')) {
    activeIndex.value = '4' // 群聊页面高亮
  } else {
    activeIndex.value = '0' // 其他页面不高亮
  }
}, {immediate: true})

/*登出*/
function logout() {
  localStorage.setItem('currentUserId', '0')  // 清除用户ID
  localStorage.removeItem('jwtToken')         // 清除JWT Token
  currentUserId.value = 0                     // 重置当前用户ID
  currentUserName.value = ''                  // 重置用户名
  // 清除所有认证相关的本地存储
  localStorage.removeItem('jwtToken')
  localStorage.removeItem('currentUserId')
  localStorage.removeItem('userRole')
  localStorage.removeItem('isAdmin')
  
  // 重置组件状态
  currentUserId.value = 0
  currentUserName.value = ''
  isAdmin.value = false
  
  // 触发重新计算登录状态
  authState.value++
  
  // 触发自定义事件，通知其他组件状态已更新
  window.dispatchEvent(new CustomEvent('authStateChanged', {
    detail: {
      userId: 0,
      userRole: 0,
      isAdmin: false
    }
  }))
  
  router.push('/')                            // 路由跳转到首页
  window.location.href = '/'                  // 强制刷新页面
}

/*注销账号*/
async function handleDeleteAccount() {
  try {
    // 显示确认弹窗
    await ElMessageBox.confirm(
      '注销账号后，您的所有数据将被永久删除，此操作不可恢复。您确定要注销账号吗？',
      '注销账号确认',
      {
        confirmButtonText: '确认注销',
        cancelButtonText: '取消',
        type: 'warning',
        confirmButtonClass: 'el-button--danger',
        center: true,
        dangerouslyUseHTMLString: false,
        distinguishCancelAndClose: true,
        showClose: true,
        closeOnClickModal: false,
        closeOnPressEscape: false,
      }
    )
    
    // 用户确认后，发送注销请求
    try {
      await axiosInstance.delete(`/user/${currentUserId.value}`)
      
      // 注销成功
      ElMessage.success('账号注销成功，感谢您的使用！')
      
      // 清除所有认证相关的本地存储
      localStorage.removeItem('jwtToken')
      localStorage.removeItem('currentUserId')
      localStorage.removeItem('userRole')
      localStorage.removeItem('isAdmin')
      
      // 重置组件状态
      currentUserId.value = 0
      currentUserName.value = ''
      isAdmin.value = false
      
      // 触发重新计算登录状态
      authState.value++
      
      // 跳转到首页
      router.push('/')
      window.location.href = '/'
      
    } catch (error: any) {
      console.error('注销账号失败:', error)
      
      if (error.response?.status === 404) {
        ElMessage.error('用户不存在或已被删除')
      } else if (error.response?.status === 403) {
        ElMessage.error('没有权限执行此操作')
      } else if (error.response?.status === 500) {
        ElMessage.error('服务器内部错误，请稍后重试')
      } else {
        ElMessage.error('注销账号失败，请检查网络连接或稍后重试')
      }
    }
    
  } catch (error: any) {
    // 用户取消操作，不执行任何操作
    if (error !== 'cancel') {
      console.error('确认弹窗错误:', error)
    }
  }
}

// 监听认证状态变化事件
const handleAuthStateChange = (_e: Event) => {
  // 更新组件状态
  const storedValue = localStorage.getItem('currentUserId')
  const storedUserId = storedValue ? parseInt(storedValue) : 0
  currentUserId.value = isNaN(storedUserId) ? 0 : storedUserId
  
  // 触发重新计算登录状态
  authState.value++
  
  // 检查管理员权限
  checkAdminPermission()
  
  // 如果用户已登录，获取用户名
  if (currentUserId.value != 0) {
    axiosInstance.get(`user/${currentUserId.value}`)
      .then(response => {
        currentUserName.value = response.data.userName
      })
      .catch(error => {
        if (error?.response?.status === 404) {
          // 用户不存在，降级为未登录
          localStorage.removeItem('jwtToken')
          localStorage.setItem('currentUserId', '0')
          localStorage.removeItem('userRole')
          localStorage.removeItem('isAdmin')
          currentUserId.value = 0
          currentUserName.value = ''
          authState.value++
        } else {
          console.error('获取用户名失败:', error)
        }
      })
  } else {
    currentUserName.value = ''
  }
}

onMounted(async () => {
  if (currentUserId.value != 0) {  // 如果用户已登录
    try {
      const response = await axiosInstance.get(`user/${currentUserId.value}`)
      currentUserName.value = response.data.userName          // 设置用户名
    } catch (error: any) {
      if (error?.response?.status === 404) {
        // 本地存的 userId 在后端不存在，视为未登录状态并清理
        localStorage.removeItem('jwtToken')
        localStorage.setItem('currentUserId', '0')
        localStorage.removeItem('userRole')
        localStorage.removeItem('isAdmin')
        currentUserId.value = 0
        currentUserName.value = ''
        authState.value++
      } else {
        ElMessage.error('请求失败，请检查网络连接或稍后重试。')
      }
    }
  }
  
  // 检查管理员权限
  checkAdminPermission()
  
  // 添加认证状态变化监听
  window.addEventListener('authStateChanged', handleAuthStateChange)
})

onUnmounted(() => {
  // 移除认证状态变化监听
  window.removeEventListener('authStateChanged', handleAuthStateChange)
})

// 检查管理员权限
const checkAdminPermission = () => {
  const userRole = localStorage.getItem('userRole')
  const isAdminFlag = localStorage.getItem('isAdmin')
  const token = localStorage.getItem('jwtToken')
  
  // 只有同时满足所有条件才认为是管理员
  isAdmin.value = !!(token && userRole === '1' && isAdminFlag === 'true')
}

// 透明处理
const handleScroll = () => {

  const el = document.querySelector('.header-navbar') as HTMLElement

  if (!el) return

  // 滚动超过60像素就透明
  if (window.scrollY > 60) {
    el.classList.add('transparent-header')
  } 
  else {
    el.classList.remove('transparent-header')
  }
}

onMounted(() => {
  window.addEventListener('scroll', handleScroll)
})
onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})


</script>

<style scoped>

.header-navbar {

  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1000;

  margin-top: 0;
  align-items: center;
  height: 60px;
  min-width: 1200px;
  border-bottom-left-radius: 10px;
  border-bottom-right-radius: 10px;
  overflow: auto;

  background-color: rgba(255, 255, 255, 1);
  backdrop-filter: blur(6px);
  -webkit-backdrop-filter: blur(6px);
  transition: background-color 0.3s ease;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1),0 2px 4px -1px rgba(0,0,0,0.06);
}


.header-navbar.transparent-header {
  background-color: rgba(255, 255, 255, 0.7);
}


.logo-title {
  max-width: 210px;
  display: flex;
  align-items: center;
  cursor: pointer;
  transition: transform 0.3s ease, color 0.3s ease;
}

.logo-title:hover {
  transform: scale(1.05);
  color: var(--el-color-primary);
}

h1 {
  font-size: 24px;
  margin: 10px 10px;
}

.navbar-item {
  font-size: 16px;
}

.flex-grow {
  flex-grow: 1;
}

.avatar {
  margin-right: 10px;
  outline: none;
}

.dropdown-item {
  display: flex;
  justify-content: space-between;
  width: 100%;
}

.dropdown-item-icon {
  margin-right: 0;
}

.disable-dropdown {
  pointer-events: none;
  opacity: 0.5;
}

/* 用户信息显示样式 */
.user-info-display {
  display: flex;
  align-items: center;
  margin-right: 15px;
  color: #333;
  font-size: 14px;
}

.user-name {
  font-weight: 500;
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* 用户身份提示样式 */
.user-indicator {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 8px 16px;
  margin: 8px 16px 16px 16px;
  border-radius: 20px;
  color: white;
  font-size: 14px;
  font-weight: 600;
  text-align: center;
  transition: all 0.3s ease;
  position: relative;
}

/* 调整文字位置，向左偏移以与"用户中心"对齐 */
.user-indicator .user-text {
  position: relative;
  left: -12px;
}

/* 管理员身份提示样式 */
.admin-indicator {
  background: linear-gradient(135deg, #ff6b6b 0%, #ee5a52 100%);
  box-shadow: 0 2px 8px rgba(255, 107, 107, 0.3);
  animation: adminPulse 2s infinite;
}

/* 普通用户身份提示样式 */
.normal-user-indicator {
  background: linear-gradient(135deg, #4a90e2 0%, #357abd 100%);
  box-shadow: 0 2px 8px rgba(74, 144, 226, 0.3);
}

.user-text {
  font-size: 13px;
  letter-spacing: 0.5px;
}

@keyframes adminPulse {
  0%, 100% {
    box-shadow: 0 2px 8px rgba(255, 107, 107, 0.3);
  }
  50% {
    box-shadow: 0 4px 16px rgba(255, 107, 107, 0.5);
  }
}

/* 注销账号按钮样式 */
.delete-account-item {
  color: #f56c6c !important;
  border-top: 1px solid #f0f0f0;
  margin-top: 8px;
  padding-top: 8px;
}

.delete-account-item:hover {
  background-color: #fef0f0 !important;
  color: #f56c6c !important;
}

.delete-account-item .dropdown-item span:first-child {
  color: #f56c6c;
  font-weight: 500;
}

</style>
