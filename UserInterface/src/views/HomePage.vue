<!--
主页页面
2342301 古振
-->

<!--背景图片-->
<template>
  <div class='background-container'>
    <img :src='`${ossBaseUrl}HomePage/BackgroundImage.jpg`' alt='Background' class='background-image'>
    <img :src='`${ossBaseUrl}LogosAndIcons/TreeHoleLogo.png`' alt='PetJoyTitle' class='petjoy-title'>
    <div class='search-container'>
      <GlobalSearchBox/>
    </div>
  </div>
</template>

<script setup lang='ts'>
import {ref, onMounted} from 'vue'
import GlobalSearchBox from '../components/GlobalSearchBox.vue'
import {
  ossBaseUrl,
  showGuidedTour
} from '../globals'

const storedValue = localStorage.getItem('currentUserId')
const storedUserId = storedValue ? parseInt(storedValue) : 0
const currentUserId = ref(isNaN(storedUserId) ? 0 : storedUserId)

onMounted(() => {
  if (currentUserId.value == 0) {
    showGuidedTour.value = true
  }
})
</script>

<style scoped>

.search-container {
  position: absolute;
  top: 385px;
  left: 50%;
  transform: translateX(-50%);
}

.petjoy-title {
  position: absolute;
  top: 70px;
  width: 600px;
  left: 50%;
  transform: translateX(-50%);
}

.background-container {
  position: relative;
}

.ai-introduction-card {
  padding: 20px;
  margin: 50px;
  border-radius: 20px;
}

.content {
  display: flex;
  justify-content: flex-end;
  align-items: flex-start;
}

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

.petjoy-title {
  position: absolute;
  top: 70px;
  width: 300px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 2; /* 确保logo在背景图片之上 */
}

.search-container {
  position: absolute;
  top: 385px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 2; /* 确保搜索框在背景图片之上 */
}

</style>
