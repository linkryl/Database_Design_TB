<!--
顶部导航栏组件
2352031 古振
-->

<template>
  <el-menu :default-active='activeIndex' class='header-navbar' mode='horizontal' :ellipsis='false'>

    <!--主页按钮-->
    <el-container ref='tourRef1' class='logo-title' @click="router.push('/')">
      <img :src='`${ossBaseUrl}LogosAndIcons/TreeHoleLogo.png`' style='height: 45px; margin-left: 10px' alt='TreeHoleLogo'>
      <h1 style='white-space: nowrap'>树洞 Treehole</h1>
    </el-container>
    
    <!--间隔-->
    <div style='margin-left: 60px'></div>

    <!--社区按钮-->
    <el-menu-item ref='tourRef2' index='1' class='navbar-item' @click="router.push('/CommunityPage')">
      {{"树洞社区" }}
    </el-menu-item>

    <!--超级占位符-->
    <div class='flex-grow'/>


    <el-dropdown ref='tourRef3' size='large'>
      
      <!--头像显示-->
      <el-avatar class='avatar' :src='`${ossBaseUrl}LogosAndIcons/GitHubLogo.png`'/>

      <template #dropdown>

        <!--未登录下拉栏 -->
        <el-dropdown-menu style='width: 210px' v-if='currentUserId==0'>
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


        </el-dropdown-menu>

        <!--已登录下拉栏-->
        <el-dropdown-menu v-else style='width: 250px'>

          <h2 style='text-align: center'>{{ "用户中心" }}</h2>

          <!--个人中心按钮-->  
          <el-dropdown-item :icon='User' @click="router.push(`/profile/${currentUserId}`)">  
            <div class='dropdown-item'>          
              <span>{{ "个人中心" }}</span>         
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
import {onMounted, ref, watch} from 'vue'
import {useRouter, useRoute} from 'vue-router'
import axiosInstance from '../utils/axios'
import {ElMessage} from 'element-plus'
import {
  ossBaseUrl,
} from '../globals'
import {
  User,
  Connection,
  Link,
  CirclePlus,
  ArrowRightBold,
} from '@element-plus/icons-vue'

const activeIndex = ref('0')
const router = useRouter()
const route = useRoute()
const storedValue = localStorage.getItem('currentUserId')
const storedUserId = storedValue ? parseInt(storedValue) : 0
const currentUserId = ref(isNaN(storedUserId) ? 0 : storedUserId)
const currentUserName = ref('')

/*跳转到论坛页面*/
watch(route, (newRoute) => {
  if (newRoute.path === '/CommunityPage') {
    activeIndex.value = '1' // 社区页面高亮
  } else {
    activeIndex.value = '0' // 其他页面不高亮
  }
}, {immediate: true})

/*登出*/
function logout() {
  localStorage.setItem('currentUserId', '0')  // 清除用户ID
  router.push('/')                            // 路由跳转到首页
  window.location.href = '/'                  // 强制刷新页面
}

onMounted(async () => {
  if (currentUserId.value != 0) {  // 如果用户已登录
    try {
      const response = await axiosInstance.get(`user/${currentUserId.value}`)
      currentUserName.value = response.data.userName          // 设置用户名
    } catch (error) {
      ElMessage.error("GET 请求失败，请检查网络连接情况或稍后重试。")
    }
  }
})

</script>

<style scoped>

.header-navbar {
  margin-top: 5px;
  align-items: center;
  height: 60px;
  min-width: 1200px;
  border-bottom-left-radius: 10px;
  border-bottom-right-radius: 10px;
  overflow: auto;
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
</style>
