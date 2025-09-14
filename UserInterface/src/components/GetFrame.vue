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
import { ref, onMounted } from 'vue'
import { ElFormItem, ElMessage, FormInstance, FormRules } from 'element-plus'

const FrameList = ref<FrameItem[]>([
    { name: '红色',  color: '#ff4d4f', owned: false },
    { name: '深蓝',  color: '#003a8c', owned: false },
    { name: '黑色',  color: '#000000', owned: false }
])

interface FrameItem {
  name: string
  color: string
  owned: boolean
}

function handleBuy(item:FrameItem){
    if (item.owned) return
    /* 这里调 axios 扣金币、写库 */
    item.owned = true
    ElMessage.success(`成功购买「${item.name}」头像框！`)
}
</script>

<style scoped>
.container {
    width: 100%;
    box-sizing: border-box;
    min-height: 100vh;
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