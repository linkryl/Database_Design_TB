<!--
顶部导航栏组件
2352031 古振
-->

<template>
  <el-menu :default-active='activeIndex' class='header-navbar' mode='horizontal' :ellipsis='false'>

    <el-container ref='tourRef1' class='logo-title' @click="router.push('/')">
      <img :src='`${ossBaseUrl}LogosAndIcons/TreeHoleLogo.png`' style='height: 45px; margin-left: 10px' alt='TreeHoleLogo'>
      <h1 style='white-space: nowrap'>树洞 Treehole</h1>
    </el-container>

    <el-menu-item ref='tourRef2' index='1' class='navbar-item' @click="router.push('/pet-community')">
      {{ t('HeaderNavbar.PetCommunity') }}
    </el-menu-item>

    <div class='flex-grow'/>

    <el-dropdown size='large' :class="{ 'disable-dropdown': currentUserId == 0 || currentUserRole != 1 }">
      <el-icon v-if='currentUserId != 0 && currentUserRole == 1'
               color='#4888F6'
               :size='24'
               style='margin-right: 8px; margin-bottom: 2px; outline: none'>
        <Operation/>
      </el-icon>
      <el-icon v-else
               color='#00000000'
               :size='24'
               style='margin-right: 8px; margin-bottom: 2px; outline: none'>
        <Operation/>
      </el-icon>
      <template #dropdown>
        <el-dropdown-menu>
          <el-dropdown-item @click="router.push('/post-report-management')">
            {{ t('HeaderNavbar.PostReportManagement') }}
          </el-dropdown-item>
          <el-dropdown-item @click="router.push('/post-comment-report-management')">
            {{ t('HeaderNavbar.PostCommentReportManagement') }}
          </el-dropdown-item>
          <el-dropdown-item @click="router.push('/news-comment-report-management')">
            {{ t('HeaderNavbar.NewsCommentReportManagement') }}
          </el-dropdown-item>
          <el-dropdown-item @click="router.push('/news-management')">
            {{ t('NewsManagementPage.NewsManagement') }}
          </el-dropdown-item>
          <el-dropdown-item @click="router.push('/user-feedback-management')">
            {{ t('HeaderNavbar.UserFeedbackManagement') }}
          </el-dropdown-item>
          <el-dropdown-item @click="router.push('/api')">
            {{ t('HeaderNavbar.DatabaseWebAPI') }}
          </el-dropdown-item>
        </el-dropdown-menu>
      </template>
    </el-dropdown>

    <el-dropdown ref='tourRef3' size='large'>
      <el-avatar class='avatar' :src='`${ossBaseUrl}${currentUserAvatarUrl}`'>
        <el-icon :size='24'>
          <Avatar/>
        </el-icon>
      </el-avatar>
      <template #dropdown>

        <!--未登录下拉栏 -->
        <el-dropdown-menu style='width: 210px' v-if='currentUserId==0'>
          <h2 style='text-align: center'>{{ t('HomePage.TourRefTitle11') }}</h2>

          <!--登陆按钮-->
          <el-dropdown-item :icon='Connection' @click="router.push('/login')">
            <div class='dropdown-item'>
              <span>{{ t('HeaderNavbar.Login') }}</span>
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>
            </div>
          </el-dropdown-item>

          <!--注册按钮-->
          <el-dropdown-item :icon='CirclePlus' @click="router.push('/register')">
            <div class='dropdown-item'>
              <span>{{ t('HeaderNavbar.Register') }}</span>
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>
            </div>
          </el-dropdown-item>


        </el-dropdown-menu>

        <!--已登录下拉栏-->
        <el-dropdown-menu v-else style='width: 250px'>

          <!--个人中心按钮-->  
          <el-dropdown-item :icon='User' @click="router.push(`/profile/${currentUserId}`)">  
            <div class='dropdown-item'>          
              <span>{{ t('HeaderNavbar.PersonalCenter') }}</span>         
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>    
            </div>
  
          </el-dropdown-item>

          <!--退出-->
          <el-dropdown-item :icon='Link' @click='logout'>
            <div class='dropdown-item'>
              <span>{{ t('HeaderNavbar.Logout') }}</span>
              <span><el-icon :size='12' class='dropdown-item-icon'><ArrowRightBold/></el-icon></span>
            </div>
          </el-dropdown-item>
        </el-dropdown-menu>


      </template>
    </el-dropdown>
  </el-menu>
</template>

<script setup lang='ts'>
import {onMounted, ref, watch, onUnmounted} from 'vue'
import {useRouter, useRoute} from 'vue-router'
import {useI18n} from 'vue-i18n'
import axiosInstance from '../utils/axios'
import {ElMessage} from 'element-plus'
import {
  ossBaseUrl,
  tourRef1,
  tourRef2,
  tourRef3
} from '../globals'
import {
  User,
  Connection,
  Link,
  CirclePlus,
  Avatar,
  ArrowRightBold,
  Operation,
} from '@element-plus/icons-vue'

const {t} = useI18n()
const activeIndex = ref('0')
const router = useRouter()
const route = useRoute()
const storedValue = localStorage.getItem('currentUserId')
const storedUserId = storedValue ? parseInt(storedValue) : 0
const currentUserId = ref(isNaN(storedUserId) ? 0 : storedUserId)
const currentUserAvatarUrl = ref('')
const currentUserName = ref('')
const currentUserRole = ref(0)
const userExperiencePoints = ref(0)
const userFollowsCount = ref(0)
const userFollowedCount = ref(0)
const userLikedCount = ref(0)


watch(route, (newRoute) => {
  if (newRoute.path === '/pet-community') {
    activeIndex.value = '1'
  } else {
    activeIndex.value = '0'
  }
}, {immediate: true})

function logout() {
  localStorage.setItem('currentUserId', '0')
  router.push('/')
  window.location.href = '/'
}

onMounted(async () => {
  if (currentUserId.value != 0) {
    try {
      const response = await axiosInstance.get(`user/${currentUserId.value}`)
      currentUserAvatarUrl.value = response.data.avatarUrl
      currentUserName.value = response.data.userName
      userFollowsCount.value = response.data.followsCount
      userFollowedCount.value = response.data.followedCount
      userLikedCount.value = response.data.likedCount
      userExperiencePoints.value = response.data.experiencePoints
      currentUserRole.value = response.data.role
    } catch (error) {
      ElMessage.error(t('ErrorMessage.GetErrorMessage'))
    }
  }
})

const handleScroll = () => {
  const header = document.querySelector('.header-navbar')
  if (window.scrollY > 60) {
    header.classList.add('transparent-header')
  } else {
    header.classList.remove('transparent-header')
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
