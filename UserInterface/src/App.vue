<!--
父组件
2352031 古振
-->

<template>
  <div id='app'>
  <Progress/>                          <!-- 进度条组件 -->
  <HeaderNavbar class='header-navbar'/> <!-- 顶部导航栏，添加类名 -->
  <div class='router-view-container'>   <!-- 主要内容容器 -->
    <router-view/>                     <!-- Vue Router 的页面渲染区域 -->
  </div>
  <el-backtop :right='50' :bottom='50'/> <!-- Element Plus 返回顶部按钮 -->
  <FooterNavbar/>                      <!-- 底部导航栏 -->
  </div>
</template>

<script setup lang='ts'>
import {onMounted, onUnmounted} from 'vue'
import Progress from './components/Progress.vue'
import HeaderNavbar from './components/HeaderNavbar.vue'
import FooterNavbar from './components/FooterNavbar.vue'

const updateHeaderTransform = () => {               // 更新头部样式的函数
  const headerNavbar = document.querySelector('.header-navbar') as HTMLElement
  if (headerNavbar) {                               // 如果找到导航栏元素
    headerNavbar.style.transform = `translateX(-${window.scrollX}px)`
    // 根据滚动位置水平偏移导航栏（修复视觉对齐问题）
    
    // 添加透明效果判断
    if (window.scrollY > 60) {                      // 如果垂直滚动超过60px
      headerNavbar.classList.add('transparent-header') // 添加透明样式
    } else {                                        // 否则
      headerNavbar.classList.remove('transparent-header') // 移除透明样式
    }
  }
}

onMounted(() => {
  window.addEventListener('scroll', updateHeaderTransform)
  window.addEventListener('resize', updateHeaderTransform)
  updateHeaderTransform() // 初始化时也检查一次
})

onUnmounted(() => {
  window.removeEventListener('scroll', updateHeaderTransform)
  window.removeEventListener('resize', updateHeaderTransform)
})

</script>

<style scoped>
:global(:root) {
  --header-background-initial-color: rgba(255, 255, 255, 1);
  --header-background-transparent-color: rgba(255, 255, 255, 0.8);
  --header-shadow-color-1: rgba(0, 0, 0, 0.1);
  --header-shadow-color-2: rgba(0, 0, 0, 0.06);
}

/* noinspection CssUnusedSymbol */
:global(.dark) {
  --header-background-initial-color: rgba(0, 0, 0, 1);
  --header-background-transparent-color: rgba(0, 0, 0, 0.8);
  --header-shadow-color-1: rgba(255, 255, 255, 0.1);
  --header-shadow-color-2: rgba(255, 255, 255, 0.06);
}

.router-view-container {
  min-height: calc(100vh - 135px);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding-top: 65px;
}

.header-navbar {
  position: fixed;
  width: 100%;
  z-index: 1000;
  background-color: var(--header-background-initial-color);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  transition: background-color 0.3s ease;
  box-shadow: 0 4px 6px -1px var(--header-shadow-color-1), 0 2px 4px -1px var(--header-shadow-color-2);
}

/* noinspection CssUnusedSymbol */
.transparent-header {
  background-color: var(--header-background-transparent-color);
}
</style>
