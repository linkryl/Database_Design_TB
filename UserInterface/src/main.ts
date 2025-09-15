/*
主入口
TreeHole制作组
*/

// @ts-ignore
import App from './App.vue'
import {createApp} from 'vue'
import router from './utils/router'
import 'element-plus/dist/index.css'
import './styles/ScrollbarCSS.css'
import { initializeUserId } from './globals'

// 初始化用户ID
initializeUserId()

const app = createApp(App)

app.use(router)
app.mount('#app')
