<!--
底部导航栏组件
2352031 古振
-->

<template>
  <el-container class='footer-navbar'>
    <a href='https://sse.tongji.edu.cn' target='_blank'>
      <img :src='`${ossBaseUrl}LogosAndIcons/TongjiSSELogo.png`'
           class='footer-sse-logo-left'
           alt='TongjiSSELogo'
           height='40px'/>
    </a>

    <a href='https://github.com/MinmusLin/PetJoy' target='_blank'>
      <el-tooltip :content="t('FooterNavbar.GitHubRepository')" placement='top'>
        <img :src='`${ossBaseUrl}LogosAndIcons/GitHubLogo.png`'
             class='footer-github-logo-right'
             alt='GitHubLogo'
             height='40px'/>
      </el-tooltip>
    </a>

    <div class='footer-content'>
      <p>{{ t('FooterNavbar.ProjectDescription') }}</p>
      <p v-html="t('FooterNavbar.CopyrightStatement')"/>
      <p>
        <el-link :underline='false' class='footer-link' @click="router.push('/about-petjoy')">
          {{ t('FooterNavbar.AboutPetJoy') }}
        </el-link>
      </p>
    </div>
  </el-container>
</template>

<script setup lang='ts'>
import {useI18n} from 'vue-i18n'
import {useRouter} from 'vue-router'
import {ossBaseUrl} from '../globals'

const {t} = useI18n()
const router = useRouter()
</script>

<style scoped>
:global(:root) {
  --footer-bg-color: #F0F2F5;
  --footer-border-color: #DCDFE6;
  --footer-text-color: #909399;
  --footer-link-color: #909399;
  --footer-link-hover-color: #409EFF;
  --footer-sse-logo-filter: invert(50%) sepia(100%) saturate(500%) hue-rotate(180deg) brightness(80%);
  --footer-github-logo-filter: invert(0%);
}

/* noinspection CssUnusedSymbol */
:global(.dark) {
  --footer-bg-color: #303030;
  --footer-border-color: #4C4D4F;
  --footer-text-color: #A3A6AD;
  --footer-link-color: #A3A6AD;
  --footer-link-hover-color: #66BFFF;
  --footer-sse-logo-filter: brightness(90%);
  --footer-github-logo-filter: invert(100%);
}

.footer-content {
  color: var(--footer-text-color);
  font-size: 12px;
  line-height: 0.5;
  text-align: center;
}

.footer-navbar {
  position: relative;
  width: 100%;
  height: 70px;
  background-color: var(--footer-bg-color);
  border-top: 1px solid var(--footer-border-color);
  justify-content: center;
}

.footer-link {
  color: var(--footer-link-color);
  font-size: 12px;
  line-height: 0.5;
}

.footer-link:hover {
  color: var(--footer-link-hover-color);
}

.footer-sse-logo-left {
  position: absolute;
  left: 15px;
  bottom: 15px;
  filter: var(--footer-sse-logo-filter);
}

.footer-github-logo-right {
  position: absolute;
  right: 15px;
  bottom: 15px;
  filter: var(--footer-github-logo-filter);
}
</style>