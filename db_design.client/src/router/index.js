//定义路由表的源文件
import { createRouter, createWebHistory } from "vue-router"
//引入页面级.vue组件
import Home from "@/views/HomePage.vue"
import Login from "@/views/LoginPage.vue"
import Register from "@/views/RegisterPage.vue"
import Ucenter from "@/views/UcenterPage.vue"
import Promotion from "@/views/PromotionPage.vue"
import Appeal from "@/views/AppealPage.vue"
import MyFollower from "@/views/MyFollowerPage.vue"
import Market from "@/views/MarketPage.vue"
import Chat from "@/views/ChatPage.vue"
import MyFollowingBar from "@/views/MyFollowingBarPage.vue"
import MyFollowingUser from "@/views/MyFollowingUserPage.vue"
import MyFavoritePost from "@/views/MyFavoritePostPage.vue"

//定义路由表
const routes = [
  { path: "/", component: Home },
  { path: "/login", component: Login },
  { path: "/register", component: Register },
  { path: "/ucenter", component: Ucenter },
  { path: "/promotion", component: Promotion },
  { path: "/Appeal", component: Appeal },
  { path: "/my-follower", component: MyFollower },
  { path: "/market", component: Market },
  { path: "/chat", component: Chat },
  { path: "/my-following-bar", component: MyFollowingBar },
  { path: "/my-following-user", component: MyFollowingUser },
  { path: "/my-favorite-post", component: MyFavoritePost }
]

//创建路由实例
const router = createRouter({
  history: createWebHistory(),
  routes
})

//导出路由实例，让main.js使用
export default router
