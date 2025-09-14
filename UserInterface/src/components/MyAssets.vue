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
                <button class="confirm-btn" @click="handleConfirmBg">确认更换</button>
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
                <button class="confirm-btn" @click="handleConfirmFrame">确认更换</button>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElFormItem, ElMessage, FormInstance, FormRules } from 'element-plus'
const myBgList = ref<MyBgItem[]>([])
const usingBg = ref<MyBgItem>({
    url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1001.jpg",
    name: "海之梦"
})
const selectedBg = ref<MyBgItem | null>(null)
const myFrameList = ref<MyFrameItem[]>([])
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
    return new Promise<MyBgItem[]>((resolve) => {
        setTimeout(() => {
            resolve([//暂时测试数据
                { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1001.jpg", name: "海之梦" },
                { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1002.jpg", name: "千山雪" },
                { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1005.jpg", name: "春意浓" },
                { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1006.jpg", name: "出水芙蓉" },
                { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1010.jpg", name: "接天莲叶" },
                { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1011.jpg", name: "雪山日出" },
                { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1015.jpg", name: "水墨荷花" },
                { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1016.jpg", name: "金沙子" }
            ])
        }, 300)
    })
}

/* 调接口 */
async function getMyFrameList() {
    // TODO: 换成真实 axios 请求
    return new Promise<MyFrameItem[]>((resolve) => {
        setTimeout(() => {
            resolve([//暂时测试数据
                { name: '红色',  color: '#ff4d4f'},
                { name: '深蓝',  color: '#003a8c'},
                { name: '黑色',  color: '#000000'}
            ])
        }, 300)
    })
}

function handleConfirmBg() {
    if (!selectedBg.value) return
    usingBg.value = selectedBg.value
    // 这里加一个 axios 请求，把选择同步到后端
    ElMessage.success("背景切换成功")
}

function handleConfirmFrame() {
    if (!selectedFrame.value) return
    usingFrame.value = selectedFrame.value
    // 这里加一个 axios 请求，把选择同步到后端
    ElMessage.success("头像框切换成功")
}

onMounted(async () => {
    myBgList.value = await getMyBgList()
    myFrameList.value = await getMyFrameList()
    // 默认把第一张当“正在使用”
    if (myBgList.value.length) {
        usingBg.value = myBgList.value[0]
        selectedBg.value = myBgList.value[0]
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