<!--
父组件
2352031 古振
-->

<template>
  <div id='app'>
    <Progress/>                          <!-- 进度条组件 -->
    <HeaderNavbar/> <!-- 顶部导航栏 -->
    <div class='router-view-container'>   <!-- 主要内容容器 -->
      <router-view/>                     <!-- Vue Router 的页面渲染区域 -->
    </div>
    <el-backtop :right='50' :bottom='50'/> <!-- Element Plus 返回顶部按钮 -->
    <FooterNavbar/>                      <!-- 底部导航栏 -->
  </div>
</template>

<script setup lang='ts'>
import Progress from './components/Progress.vue'
import HeaderNavbar from './components/HeaderNavbar.vue'
import FooterNavbar from './components/FooterNavbar.vue'

</script>

<style scoped>

.router-view-container {
  padding-top: 60px;   /* header 高度 */
  padding-bottom: 70px;/* footer 高度 */
  min-height: calc(100vh - 135px);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

</style>