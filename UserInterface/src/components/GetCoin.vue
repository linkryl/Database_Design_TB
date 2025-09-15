<!--
道具商城中 获取金币 组件
TreeHole 开发组
-->

<template>
    <div class="container">
        <el-button class="btn-buy" @click="handleBuy(2)">购买 2🪙</el-button>
        <el-button class="btn-buy" @click="handleBuy(5)">购买 5🪙</el-button>
        <el-button class="btn-buy" @click="handleBuy(10)">购买 10🪙</el-button>
        <el-button class="btn-buy" @click="handleBuy(20)">购买 20🪙</el-button>
        <el-button class="btn-buy" @click="handleBuy(50)">购买 50🪙</el-button>
        <el-button class="btn-buy" @click="handleBuy(100)">购买 100🪙</el-button>
    </div>
</template>

<script setup lang="ts">
import { ref, watchEffect} from 'vue'
import { computed } from 'vue'
import axiosInstance from './axios'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'

const router = useRouter()
const localStorageValue = localStorage.getItem('currentUserId')
const localStorageUserId = localStorageValue ? parseInt(localStorageValue) : 0
const currentUserId = ref(isNaN(localStorageUserId) ? 0 : localStorageUserId)

const props = defineProps<{
    coinCount:number
}>()

const emit = defineEmits<{
    "update:coinCount":[val:number]
}>()

const localCoinCount = computed({
    get: ()=>props.coinCount,
    set: v =>emit("update:coinCount",v) 
})

async function handleBuy(addCoin:number){
    let newCoinCount = localCoinCount.value + addCoin
    try{                             //根据 userId 更新 USER 用户表中的金币数量 coin
        // await axiosInstance.put(`user/update-coin-by-user-id/${currentUserId.value}`,{
        //     coinCount : newCoinCount  //number类型
        // })
        localCoinCount.value = newCoinCount
        ElMessage.success(`购买成功, 金币 +${addCoin}`)
    }catch(e){
        ElMessage.error("PUT失败, 购买金币失败")
    }
}

watchEffect(() => {
  if (currentUserId.value === 0) {
    //router.push('/login') //TODO: 取消注释
  }
})
</script>

<style scoped>
.container {
    width: 100%;
    box-sizing: border-box;
    min-height: 100vh;
    background-color:#ffffff;
    display: flex;
    align-items: left;
    justify-content: center;
    gap: 16px;
    padding: 200px 0;
}

.btn-buy {
    width:120px;
    margin: 0;
    color: #000000;
    font-weight: 500;
    text-align: center;
    border: 3px solid #1890ff;
}
</style>