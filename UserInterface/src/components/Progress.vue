<!--
顶部加载的进度条组件
2352031 古振
-->

<template>
  <el-progress v-if='isProgressVisible'
               :percentage='100'
               :show-text='false'
               :indeterminate='true'
               :duration='1.5'
               class='progress-bar'/>
  <div v-else class='progress-bar-background'/>
</template>

<script setup lang='ts'>
import {isProgressVisible} from '../globals'
</script>

<style scoped>

.progress-bar {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 4px;
  z-index: 1001;
}

.progress-bar-background {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 5px;
  background-color: #EBEEF5;
  z-index: 1001;
}

</style>
