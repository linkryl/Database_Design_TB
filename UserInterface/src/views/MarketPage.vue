<!--
商城页面
TreeHole 开发组
-->

<template>
    <div class="market-page-container">
        <div class="scroll-body">
            <aside class="left-side-bar">
                <div class="user-box">
                <!-- 头像 + 动态边框 -->
                <div class="avatar-wrapper" :style="{ borderColor: avatarFrame }">
                    <img class="avatar" :src="avatarUrl" alt="_头像占位"/>
                </div>
                    <div class="username">{{ username }}</div>
                    <div class="coincount">我的金币🪙：{{ coinCount }}</div>
                </div>

                <nav class="menu">
                    <div 
                        v-for="item in menuList" 
                        v-bind:key="item.key" 
                        class="menu-item"
                        v-bind:class="{ active: activeKey === item.key }" 
                        @click="handleMenuClick(item.key)">
                        {{ item.label }}
                        <span class="arrow">>></span>
                    </div>
                </nav>
            </aside>

            <main class="content">
                <!--根据 activeKey 渲染对应组件-->
                <MyAssets 
                    v-if="activeKey === 'assets'" 
                    v-model:avatar-frame="avatarFrame"
                />
                <GetCoin 
                    v-else-if="activeKey === 'coin'" 
                    v-model:coin-count="coinCount"
                />
                <GetBackground 
                    v-else-if="activeKey === 'background'"
                    v-model:coin-count="coinCount" 
                />
                <GetFrame 
                    v-else-if="activeKey === 'frame'" 
                    v-model:coin-count="coinCount"
                />
            </main>
        </div>
    </div>
</template>

<script setup lang='ts'>
import { onMounted, onUnmounted, reactive, ref, watchEffect} from 'vue'
import { ElMessage } from 'element-plus'
import axiosInstance from '../utils/axios'
import MyAssets from "../components/MyAssets.vue"
import GetCoin from "../components/GetCoin.vue"
import GetBackground from "../components/GetBackground.vue"
import GetFrame from "../components/GetFrame.vue"
import { useRouter } from 'vue-router'

const router = useRouter()
const localStorageValue = localStorage.getItem('currentUserId')
const localStorageUserId = localStorageValue ? parseInt(localStorageValue) : 0
const currentUserId = ref(isNaN(localStorageUserId) ? 0 : localStorageUserId)

const avatarUrl = ref("https://gss0.baidu.com/7Ls0a8Sm2Q5IlBGlnYG/sys/portraith/item/tb.1.bdb1c536.7qSuTE4CMJHwpEF4Qw60Mw")
const username = ref("_h_our")
const coinCount = ref(3)
const avatarFrame = ref("#ff4d4f")

const menuList = reactive([
    { key: "assets", label: "我的资产" },
    { key: "coin", label: "获取金币" },
    { key: "background", label: "购买背景" },
    { key: "frame", label: "购买头像框" },
])
const activeKey = ref("assets")
const windowWidth = ref(window.innerWidth)

watchEffect(() => {
  if (currentUserId.value === 0) {
    // router.push('/login') //TODO：取消注释, 跳转登录页面, 让用户登录
  }
})

onMounted(async ()=>{
    if (currentUserId.value === 0) {
        ElMessage.error("未找到登录信息, 请先登录")
        // router.push('/login') //TODO：到时候取消注释, 跳转登录页面, 让用户登录
        return
    }

    try {
        // 获取用户信息展示在商店, 包括：用户名、头像链接、金币个数、头像框颜色
        // TODO: 商量后端接口           读取user用户表, 返回data对象包含这4个值
        // const response = await axiosInstance.get(`user/user-info-for-market/${currentUserId.value}`)
        // username.value = response.data.username
        // avatarUrl.value = response.data.avatarUrl  //string类型
        // avatarFrame.value = response.data.frameColor //string类型(css的颜色值)
        // coinCount.value = response.data.coinCount    //number类型
    } catch (e) {
        ElMessage.error("获取用户信息失败, 请稍后重试")
        username.value = "用户名占位"
        avatarUrl.value = ""
        avatarFrame.value ="#ffffff"
        coinCount.value = 0
    }
})

function handleMenuClick(key: string) {
    activeKey.value = key
}

function updateWindowWidth() {
    windowWidth.value = window.innerWidth
}

onMounted(() => window.addEventListener('resize', updateWindowWidth))

onUnmounted(() => window.removeEventListener('resize', updateWindowWidth))
</script>

<style scoped>
.market-page-container {
    margin-top: 0;
    display: flex;
    height: 100vh;
    flex-direction: column;
}

.scroll-body {
    flex: 1;
    overflow-y: auto;
    display: flex;
}

.left-side-bar {
    width: 20%;
    background: #ffffff;
    border-right: 2px solid #e0e0e0;
    padding: 24px 0 24px 0;
}

.user-box {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 20px 20px 20px 20px;
    margin: 0 auto 0 auto;
}

/* 头像框 */
.avatar-wrapper {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  border: 8px solid;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 12px;
}

.avatar {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
}

.username .coincount {
    font-size: 30px;
    font-weight: 600;
    color: #333333;
    display: block;
    margin-top: 30px;
    margin-bottom: 30px;
}

.menu {
    margin: 0px 0 16px 0;
    padding: 20px 20px 20px 20px;
}

.menu-item {
    padding: 12px 16px 12px 13px;
    border-left: 3px solid #b0b0b0;
    margin: 4px auto 4px auto;
    cursor: pointer;
    transition: background-color 0.3s;
    font-size: 16px;
    color: #666666;
    position: relative;
}

.arrow {
    position: absolute;
    right: 12px;
    top: 50%;
    color: #bbbbbb;
    font-size: 20px;
    transform: translateY(-50%);
}

.menu-item:hover {
    background-color: #f1f1f1;
    border-left-color: #d9d9d9;
}

.menu-item.active {
    background-color: #e6f7ff;
    color: #1890ff;
    font-weight: 600;
    border-left-color: #1890ff;
}

.menu-item.active .arrow {
    color: #1890ff;
}

.content {
    flex: 1;
    padding: 24px 24px 24px 24px;
    box-sizing: border-box;
}
</style>