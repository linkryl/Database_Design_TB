<!--
主页页面
2342301 古振
-->

<!--背景图片-->
<template>
  <div class='background-container'>
    <img :src='`${ossBaseUrl}BackgroundImage.jpg`' alt='Background' class='background-image'>
    <img :src='`${ossBaseUrl}TreeHoleLogo.png`' alt='TreeHoleTitle' class='treehole-title'>

    <button class="join-community-button" @click="router.push('/PostNew')">
      加入社区，发布你的第一个帖子！
    </button>

  </div>
</template>

<script setup lang='ts'>
import {ref, onMounted} from 'vue'
import {useRouter} from 'vue-router'
import {
  ossBaseUrl,
  showGuidedTour
} from '../globals'

const storedValue = localStorage.getItem('currentUserId')
const storedUserId = storedValue ? parseInt(storedValue) : 0
const currentUserId = ref(isNaN(storedUserId) ? 0 : storedUserId)
const router = useRouter()

onMounted(() => {
  if (currentUserId.value == 0) {
    showGuidedTour.value = true
  }
})
</script>

<style scoped>

.background-container {
  position: relative;
  height: 100vh; /* 确保容器高度 */
}

/*背景图片*/
.background-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  position: absolute;
  top: 0;
  left: 0;
  z-index: 1;
}

.treehole-title {
  position: absolute;
  top: 70px;
  width: 300px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 2;
}

.join-community-button {
  position: absolute;
  top: 400px; /* logo 下方 */
  left: 50%;
  transform: translateX(-50%);
  padding: 18px 40px;
  font-size: 20px;
  font-weight: bold;
  color: #fff;                  /* 白色文字 */
  background-color: #6c63ff;    /* 紫色背景 */
  border: none;
  border-radius: 30px;          /* 圆角矩形 */
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  cursor: pointer;
  transition: all 0.3s ease;
  z-index: 2;                   /* 在背景上方 */
  min-width: 320px;           /* 最小宽度 */
}

.join-community-button:hover {
  background-color: #5a52d1;    /* 悬停时稍深的紫色 */
  transform: translateX(-50%) scale(1.05);
}

</style>
