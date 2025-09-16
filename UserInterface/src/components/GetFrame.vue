<!--
道具商城中 获取头像框 组件
TreeHole 开发组
-->

<template>
    <div class="container">
        <h3>头像框商城</h3>
        <div class="row-wrapper">
            <div class="frame-list">
                <div class="frame-item" v-for="(item, idx) in FrameList" :key="idx">
                    <!-- 圆形头像框 -->
                    <div class="frame-circle" :style="{ borderColor: item.color }"></div>
                    <span>{{ item.name }} 价格: 2🪙</span>
                    <!-- 购买按钮 -->
                    <button 
                        class="btn-buy" 
                        :class="{ have: item.owned }" 
                        :disabled="item.owned"
                        @click="handleBuy(item)">
                        {{ item.owned ? '已拥有' : '购买头像框' }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watchEffect } from 'vue'
import { ElMessage } from 'element-plus'
import { computed } from 'vue'
import axiosInstance from './axios'
import { useRouter } from 'vue-router'

const router = useRouter()
const localStorageValue = localStorage.getItem('currentUserId')
const localStorageUserId = localStorageValue ? parseInt(localStorageValue) : 0
const currentUserId = ref(isNaN(localStorageUserId) ? 0 : localStorageUserId)

const props = defineProps<{
    coinCount: number
}>()

const emit = defineEmits<{
    "update:coinCount":[val:number]
}>()

const localCoinCount = computed({
    get: ()=>props.coinCount,
    set: v =>emit("update:coinCount",v)
})

const FrameList = ref<FrameItem[]>([
    { name: '红色',  color: '#ff4d4f', owned: false },
    { name: '深蓝',  color: '#003a8c', owned: false },
    { name: '黑色',  color: '#000000', owned: false },
    { name: '金色',  color: '#eff967', owned: false },
    { name: '亮金色',  color: '#eff960', owned: false },
    { name: '浅绿色',  color: '#bfe967', owned: false },
    { name: '紫红色',  color: '#af1967', owned: false },
    { name: '碧绿色',  color: '#1ff947', owned: false },
    { name: '嫩绿色',  color: '#6ef947', owned: false },
    { name: '土色',  color: '#ddd372', owned: false },
    { name: '墨绿色',  color: '#4fc691', owned: false },
])

interface FrameItem {
  name: string
  color: string
  owned: boolean
}

watchEffect(() => {
  if (currentUserId.value === 0) {
    //router.push('/login') //TODO: 取消注释
  }
})

onMounted(async ()=>{
    /* 循环请求：/ownedframe/check-if-the-frame-owned?userId=xxx&frameColor=xxx
       返回 1 表示已拥有，0 表示未拥有 */
    for (let i = 0; i < FrameList.value.length; ++i) {
        const item = FrameList.value[i]
        try {            // 检查数据库 OWNEDFRAME 这张表中是否存在这一行 userId frameColor
            const response = await axiosInstance.get('/ownedframe/check-if-the-frame-owned', {
                params: {// GET 请求的 query 查询串               
                    userId: currentUserId.value,
                    frameColor: item.color
                }
            })
            item.owned = response.data === 1
        }catch(e){
            ElMessage.error(`GET错误, 检查头像框「${item.name}」失败`)
            console.error(`GET错误, 检查头像框「${item.name}」失败`, e)
        }
    }
})

async function handleBuy(item:FrameItem){
    //购买头像框
    if (item.owned) return
    if(localCoinCount.value >= 2){
        try {       // 向数据库 OWNEDFRAME 表中添加这一行 userId frameColor frameName,表示该用户购买了该头像框
            await axiosInstance.post('/ownedframe/buy-one-frame', {
                userId: currentUserId.value,  //number类型
                frameColor: item.color,       //string类型(css样式的颜色值)
                frameName: item.name           //string类型(头像框名称)
            })
            localCoinCount.value -= 2
            item.owned = true
            ElMessage.success(`成功购买「${item.name}」头像框!`)
        } catch (e) {
            ElMessage.error("POST 请求错误, " + `购买「${item.name}」头像框失败`)
            console.error(`购买「${item.name}」头像框失败`)
        }
    }else{
        ElMessage.error("金币不足, 无法购买")
    }
}
</script>

<style scoped>
.container {
    width: 100%;
    box-sizing: border-box;
    min-height: 100vh;
}

h3 {
    padding: 0;
    margin: 0 0 30px 0;
    font-weight: 500;
    text-align: center;
}

.frame-list {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;
}

.frame-item {
    width: 160px;
    display: flex;
    flex-direction: column;
    align-items: center;
}

/* 圆形头像框：尺寸与 MarketPage 头像一致（120×120） */
.frame-circle {
    width: 120px;
    height: 120px;
    border-radius: 50%;
    border: 8px solid;
    background: transparent;   /* 可换成透明 */
    margin-bottom: 8px;
}

.btn-buy {
    width: 160px;
    height: 36px;
    background: #1890ff;
    color: #fff;
    border: none;
    border-radius: 4px;
    font-size: 14px;
    cursor: pointer;
    text-align: center;
}

.btn-buy:hover {
    background: #66b1ff;
}

.btn-buy.have {
    cursor: not-allowed;
    background-color: #c3e2ff;
}
</style>