<!--
底部导航栏组件
2352031 古振
-->

<template>
  <el-container class='FooterNavbar'>
    <a href='https://sse.tongji.edu.cn' target='_blank'>
      <img :src='`${ossBaseUrl}LogosAndIcons/TongjiSSELogo.png`'
           class='FooterSseLogoLeft'
           alt='TongjiSSELogo'
           height='40px'/>
    </a>

    <a href='https://github.com/linkryl/Database_Design_TB' target='_blank'>
      <el-tooltip :content="t('FooterNavbar.GitHubRepository')" placement='top'>
        <img :src='`${ossBaseUrl}LogosAndIcons/GitHubLogo.png`'
             class='FooterGithubLogoRight'
             alt='GitHubLogo'
             height='40px'/>
      </el-tooltip>
    </a>

    <div class='footer-content'>
      <p>{{ t('FooterNavbar.ProjectDescription') }}</p>
    </div>
  </el-container>
</template>

<script setup lang='ts'>
import {useI18n} from 'vue-i18n'
import {ossBaseUrl} from '../globals'

const {t} = useI18n()
</script>

<style scoped>

.footer-content {
  color: #909399;
  font-size: 12px;
  line-height: 0.5;
  text-align: center;
}

.FooterNavbar {
  position: relative;
  width: 100%;
  height: 70px;
  background-color: #F0F2F5;
  border-top: 1px solid #DCDFE6;
  justify-content: center;
}

.FooterSseLogoLeft {
  position: absolute;
  left: 15px;
  bottom: 15px;
  filter: invert(50%) sepia(100%) saturate(500%) hue-rotate(180deg) brightness(80%);
}

.FooterGithubLogoRight {
  position: absolute;
  right: 15px;
  bottom: 15px;
  filter: invert(0%);
}


</style>