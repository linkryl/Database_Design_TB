/*
主入口
TreeHole制作组
*/

// @ts-ignore
import App from './App.vue'
import {createApp} from 'vue'
import router from './utils/router'
import 'element-plus/dist/index.css'
import 'element-plus/theme-chalk/dark/css-vars.css'
import './styles/ScrollbarCSS.css'

const app = createApp(App)

app.use(router)
app.mount('#app')
