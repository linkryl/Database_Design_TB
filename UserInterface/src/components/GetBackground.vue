<!--
道具商城中 获取背景 组件
TreeHole 开发组
-->

<template>
    <div class="container">
        <h3>背景商城</h3>
        <div class="row-wrapper">
            <div class="background-list">
                <div class="background-item" v-for="(item, idx) in BgList" :key="idx">
                    <img :src="item.url" :alt="item.name" />
                    <span>{{ item.name }} 价格: 5🪙</span>
                    <button class="btn-buy" :class="{ have: item.owned }" :disabled="item.owned"
                        @click="handleBuy(item)">
                        {{ item.owned ? '已拥有' : '购买背景' }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watchEffect} from 'vue'
import { ElMessage } from 'element-plus'
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import axiosInstance from './axios'

const router = useRouter()
const localStorageValue = localStorage.getItem('currentUserId')
const localStorageUserId = localStorageValue ? parseInt(localStorageValue) : 0
const currentUserId = ref(isNaN(localStorageUserId) ? 0 : localStorageUserId)

const props = defineProps<{
    coinCount: number
}>()

const emit = defineEmits<{
    "update:coinCount": [val: number]
}>()

const localCoinCount = computed<number>({
    get: () => props.coinCount,
    set: v => emit("update:coinCount", v)
})

interface BgItem {
    url: string
    name: string
    owned: boolean
}

const BgList = ref<BgItem[]>([
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1001.jpg", name: "海之梦", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1002.jpg", name: "千山雪", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1003.jpg", name: "长天一色", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1004.jpg", name: "银汉迢迢", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1005.jpg", name: "春意浓", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1006.jpg", name: "出水芙蓉", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1007.jpg", name: "白色飞羽", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1008.jpg", name: "寥落星河", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1009.jpg", name: "廊桥遗梦", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1010.jpg", name: "接天莲叶", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1011.jpg", name: "雪山日出", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1012.jpg", name: "原野晨曦", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1013.jpg", name: "三叶草", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1014.jpg", name: "层林尽染", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1015.jpg", name: "水墨荷花", owned: false },
    { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1016.jpg", name: "金沙子", owned: false }
])

watchEffect(() => {
  if (currentUserId.value === 0) {
    //router.push('/login') //TODO: 取消注释
  }
})

onMounted(async () => {
    /* 循环请求：/ownedbg/check-if-the-bg-owned?userId=xxxxxx&bgUrl=xxxxxx
       返回 1 表示已拥有，0 表示未拥有 */
    for (let i = 0; i < BgList.value.length; ++i) {
        const item = BgList.value[i]
        try {            // 检查数据库 OWNEDBG 这张表中是否存在这一行 userId bgUrl
            const response = await axiosInstance.get('/ownedbg/check-if-the-bg-owned', {
                params: {// GET 请求的 query 查询串               
                    userId: currentUserId.value,  //number类型
                    bgUrl: item.url               //string类型
                }
            })
            //返回 1 表示已拥有，0 表示未拥有
            item.owned = response.data === 1
        } catch (e) {
            ElMessage.error(`GET错误, 检查背景「${item.name}」失败`)
            console.error(`GET错误, 检查背景「${item.name}」失败`, e)
        }
    }
})

async function handleBuy(item: BgItem) {
    //购买背景
    if (item.owned) return
    if (localCoinCount.value >= 5) {
        try {                     // 向数据库 OWNEDBG 表中添加这一行 userId bgUrl bgName,表示该用户购买了该背景
            await axiosInstance.post('/ownedbg/buy-one-bg', {
                userId: currentUserId.value,
                bgUrl: item.url,
                bgName: item.name
            })
            localCoinCount.value -= 5
            item.owned = true
            ElMessage.success(`成功购买「${item.name}」背景！`)
        } catch (e) {
            ElMessage.error("POST 请求错误, " + `购买「${item.name}」背景失败`)
            console.error(`购买「${item.name}」背景失败`)
        }
    } else {
        ElMessage.error("金币不足, 购买失败")
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

h4 {
    padding: 0;
    margin: 0 0 10px 0;
    font-weight: 400;
}

.background-list {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;
}

.background-item {
    width: 160px;
    text-align: center;
    border: 2px solid transparent;
    border-radius: 8px;
    overflow: hidden;
    transition: border 0.2s;
}

.background-item img {
    width: 100%;
    height: 96px;
    object-fit: cover;
    display: block;
}

.background-item span {
    display: block;
    padding: 6px 0;
    font-size: 14px;
    color: #333333;
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