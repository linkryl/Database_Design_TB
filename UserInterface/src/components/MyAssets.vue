<!--
道具商城中 我的资产 组件
TreeHole 开发组
-->

<template>
    <div class="container">
        <div class="myBackground">
            <h3>我的背景</h3>
            <div class="divider"></div>

            <div class="row-wrapper">
                <h4>正在使用</h4>
                <div class="using-bg-box">
                    <img :src="usingBg.url" alt="正在使用的背景" />
                    <span>{{ usingBg.name }}</span>
                </div>
            </div>

            <div class="row-wrapper">
                <h4>已拥有</h4>
                <div class="own-bg-list">
                    <div 
                        class="own-bg-item"
                        v-for="(item, idx) in myBgList" 
                        :key="idx"
                        :class="{ active: selectedBg && selectedBg.url === item.url }" 
                        @click="selectedBg = item"
                    >
                        <img :src="item.url" :alt="item.name" />
                        <span>{{ item.name }}</span>
                    </div>
                </div>
            </div>

            <div class="btn-line">
                <button 
                    class="confirm-btn" 
                    @click="handleConfirmBg"
                >
                    确认更换
                </button>
            </div>
        </div>

        <div class="myFrame">
            <h3>我的头像框</h3>
            <div class="divider"></div>

            <div class="row-wrapper">
                <h4>正在使用</h4>
                <div class="using-frame-box">
                    <!-- 圆形头像框 -->
                    <div class="frame-circle" :style="{ borderColor: usingFrame.color }"></div>
                    <span>{{ usingFrame.name }}</span>
                </div>
            </div>

            <div class="row-wrapper">
                <h4>已拥有</h4>
                <div class="own-frame-list">
                    <div 
                        class="own-frame-item"
                        v-for="(item,idx) in myFrameList"
                        :key="idx"
                        :class="{active: selectedFrame && selectedFrame.color === item.color}"
                        @click="selectedFrame = item"
                    >
                        <!-- 圆形头像框 -->
                        <div class="frame-circle" :style="{ borderColor: item.color }"></div>
                        <span>{{ item.name }}</span>
                    </div>
                </div>
            </div>

            <div class="btn-line">
                <button 
                    class="confirm-btn" 
                    @click="handleConfirmFrame"
                >
                    确认更换
                </button>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watchEffect } from 'vue'
import { ElMessage } from 'element-plus'
import axiosInstance from './axios'
import { computed } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const localStorageValue = localStorage.getItem('currentUserId')
const localStorageUserId = localStorageValue ? parseInt(localStorageValue) : 0
const currentUserId = ref(isNaN(localStorageUserId) ? 0 : localStorageUserId)

const props = defineProps<{
  avatarFrame: string
}>()

const emit = defineEmits<{
  "update:avatarFrame": [val: string]
}>()

const localFrame = computed({
  get: () => props.avatarFrame,
  set: v => emit("update:avatarFrame", v)
})

const myBgList = ref<MyBgItem[]>([
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1001.jpg", name: "海之梦" },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1002.jpg", name: "千山雪" },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1005.jpg", name: "春意浓" },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1006.jpg", name: "出水芙蓉" },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1010.jpg", name: "接天莲叶" },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1011.jpg", name: "雪山日出" },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1015.jpg", name: "水墨荷花" },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1016.jpg", name: "金沙子" }
])
const usingBg = ref<MyBgItem>({
    url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1001.jpg",
    name: "海之梦"
})
const selectedBg = ref<MyBgItem | null>(null)

const myFrameList = ref<MyFrameItem[]>([
    { name: '红色',  color: '#ff4d4f'},
    { name: '深蓝',  color: '#003a8c'},
    { name: '黑色',  color: '#000000'},
    { name: '金色',  color: '#eff967'}
])
const usingFrame = ref<MyFrameItem>({
    color:"#ff4d4f",
    name:"红色"
})
const selectedFrame = ref<MyFrameItem | null>(null)

interface MyBgItem {
    url: string
    name: string
}
interface MyFrameItem {
    color: string
    name: string
}

/* 调接口 */
async function getMyBgList() {
    // TODO: 换成真实 axios 请求
    try{                                 //根据 userId 获取 OWNEDBG 表中该 userId 对应的所有背景图片        
        // const response = await axiosInstance.get(`/ownedbg/get-my-bg-list/${currentUserId.value}`)
        // myBgList.value = response.data // 这里的data期望是一个数组 [{url:,name:},{},{}......]
    }catch(e){
        ElMessage.error("获取 已拥有的 背景图片失败")
        myBgList.value = []
    }
    /*  [{url:"",name:"雪山日出"},{url:"",name:"水墨荷花"},{url:"",name:"海之梦"}......]
    *   url 背景图片的网址, name 图片名字
    */

    // return new Promise<MyBgItem[]>((resolve) => {
    //     setTimeout(() => {
    //         resolve([//暂时测试数据
    //             { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1001.jpg", name: "海之梦" },
    //             { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1002.jpg", name: "千山雪" },
    //             { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1005.jpg", name: "春意浓" },
    //             { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1006.jpg", name: "出水芙蓉" },
    //             { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1010.jpg", name: "接天莲叶" },
    //             { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1011.jpg", name: "雪山日出" },
    //             { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1015.jpg", name: "水墨荷花" },
    //             { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1016.jpg", name: "金沙子" }
    //         ])
    //     }, 300)
    // })
}

/* 调接口 */
async function getMyFrameList() {
    // TODO: 换成真实 axios 请求
    try{              //根据 userId 查询 OWNEDFRAME 表中该 userId 对应的所有头像框颜色          
        // const response = await axiosInstance.get(`/ownedframe/get-my-frame-list/${currentUserId.value}`)
        // myFrameList.value = response.data // 这里的data期望是一个数组 [{name:,color:},{},{}......]
    }catch(e){
        ElMessage.error("获取 已拥有的 头像框失败")
        myFrameList.value = []
    }
    /*  [{name:"红色",color:"#ff4d4f"},{name:"深蓝",color:"#003a8c"},{name:"黑色",color:"#000000"}......]
    *   name 颜色名字, color 必须是css的颜色值
    */

    // return new Promise<MyFrameItem[]>((resolve) => {
    //     setTimeout(() => {
    //         resolve([//暂时测试数据
    //             { name: '红色',  color: '#ff4d4f'},
    //             { name: '深蓝',  color: '#003a8c'},
    //             { name: '黑色',  color: '#000000'}
    //         ])
    //     }, 300)
    // })
}

async function handleConfirmBg() {
    if (!selectedBg.value) return
    try{                    //向 USER 表中更新 userId 对应的 bgUrl 和名称bgName
        // await axiosInstance.put(`user/update-bg-by-user-id/${currentUserId.value}`,{
        //     bgUrl:selectedBg.value.url,   //string类型(背景图片的网址)
        //     bgName:selectedBg.value.name    //string类型(背景图片的名称)
        // }) // TODO: 
        usingBg.value = selectedBg.value
        ElMessage.success("背景切换成功")
    }catch(e){
        ElMessage.error("PUT失败, 背景切换失败, 请重试")
    }
}

async function handleConfirmFrame() {
    if (!selectedFrame.value) return
    try{                    //向 USER 表中更新 userId 对应的 frameColor 颜色和名称frameName   
        // await axiosInstance.put(`user/update-frame-by-user-id/${currentUserId.value}`,{
        //     frameColor:selectedFrame.value.color,  //string类型(css的颜色值)
        //     frameName:selectedFrame.value.name          //string类型(头像框的名称)
        // }) // TODO: 
        usingFrame.value = selectedFrame.value
        localFrame.value = selectedFrame.value.color
        ElMessage.success("头像框切换成功")
    }catch(e){
        ElMessage.error("PUT失败, 头像框切换失败, 请重试")
    }
}

async function getUsingBg() {
    try{                                      //通过 userId 获取用户表中的 背景图片url 以及图片名称name
        // const response = await axiosInstance.get(`user/get-bg-by-user-id/${currentUserId.value}`)
        // usingBg.value= response.data // 这里的data期望是一个对象{url:"https://......", name:"海之梦"}
    }catch(e){
        ElMessage.error("GET失败, 获取正在使用的背景失败")
        usingBg.value = {url:"",name:""}
    }
}

async function getUsingFrame() {
    try{                                     //通过 userId 获取用户表中的 头像框名称name 以及颜色color
        // const response = await axiosInstance.get(`user/get-frame-by-user-id/${currentUserId.value}`)
        // usingFrame.value= response.data      // 这里的data期望是一个对象{name:"红色",color:"#ff4d4f"}
    }catch(e){
        ElMessage.error("GET失败, 获取正在使用的头像框失败")
        usingFrame.value = {name:"",color:"#ffffff"}
    }
}


watchEffect(() => {
  if (currentUserId.value === 0) {
    //router.push('/login') //TODO: 取消注释
  }
})

onMounted(async () => {
    await getUsingBg()
    await getUsingFrame()
    await getMyBgList()
    await getMyFrameList()
    if (myBgList.value.length) {
        usingBg.value = myBgList.value[0]
        selectedBg.value = myBgList.value[0]
    }
    if (myFrameList.value.length) {
        usingFrame.value = myFrameList.value[0]
        selectedFrame.value = myFrameList.value[0]
    }
})
</script>

<style scoped>
.container {
    width: 100%;
    box-sizing: border-box;
    min-height: 100vh;
}

.myBackground,
.myBubble,
.myFrame {
    display: flex;
    flex-direction: column;
    min-height: 20vh;
    background-color: #fff;
    margin-bottom: 30px;
}

h3 {
    padding: 0;
    margin: 0 0 10px 0;
    font-weight: 400;
}

h4 {
    padding: 0;
    margin: 0 0 10px 0;
    font-weight: 400;
}

.divider {
    width: 50%;
    height: 1px;
    background: #dcdcdc;
    margin-bottom: 20px;
}

.using-bg-box {
    display: flex;
    flex-direction: column;
    align-items: center;
    width: 160px;
}

.using-bg-box img {
    width: 160px;
    height: 96px;
    object-fit: cover;
    border-radius: 4px;
    margin-bottom: 4px;
}

.own-bg-list {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;
}

.own-bg-item {
    width: 160px;
    cursor: pointer;
    text-align: center;
    border: 2px solid transparent;
    border-radius: 8px;
    overflow: hidden;
    transition: border 0.2s;
}

.own-bg-item.active {
    border-color: #409eff;
}

.own-bg-item img {
    width: 100%;
    height: 96px;
    object-fit: cover;
    display: block;
}

.own-bg-item span {
    display: block;
    padding: 6px 0;
    font-size: 14px;
    color: #333333;
}

.btn-line {
    width: 100%;
    box-sizing: border-box;
    margin-top: 24px;
}

.confirm-btn {
    width: 160px;
    height: 36px;
    background: #409eff;
    color: #fff;
    border: none;
    border-radius: 4px;
    font-size: 14px;
    cursor: pointer;
    text-align: center;
}

.confirm-btn:hover {
    background: #66b1ff;
}

.confirm-btn.disabled{
    cursor: not-allowed;
    opacity: 0.6;
}

.using-frame-box {
    width: 160px;
    display: flex;
    flex-direction: column;
    align-items: center;
}

.own-frame-list {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;
}

.own-frame-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    width: 160px;
    cursor: pointer;
    text-align: center;
    border: 2px solid transparent;
    border-radius: 8px;
    overflow: hidden;
    transition: border 0.2s;
    justify-content: center;
}

.own-frame-item.active {
    border-color: #409eff;
}

/* 圆形头像框：尺寸与 MarketPage 头像一致（120×120） */
.frame-circle {
    width: 120px;
    height: 120px;
    border-radius: 50%;
    border: 8px solid;
    background: transparent;
    margin-bottom: 8px;
}
</style>