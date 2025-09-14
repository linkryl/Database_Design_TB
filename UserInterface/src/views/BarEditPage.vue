<!--
贴吧编辑页面
TreeHole开发组
2025-09-14
-->

<template>
  <div class="bar-edit-container">
    <div class="edit-wrapper">
      <el-card class="edit-card" shadow="hover">
        <template #header>
          <div class="card-header">
            <h2>编辑贴吧</h2>
            <span class="subtitle">{{ barInfo?.barName }}</span>
          </div>
        </template>

        <div v-if="loading" class="loading-container" v-loading="loading">
          <div style="height: 60px;">加载贴吧信息中...</div>
        </div>

        <div v-else-if="!barInfo" class="error-container">
          <div class="error-icon">❌</div>
          <div class="error-text">贴吧不存在或您没有权限编辑</div>
          <el-button type="primary" @click="goBack">返回</el-button>
        </div>

        <div v-else class="edit-form-container">
          <CreateBarForm 
            :initial-data="barInfo"
            :is-edit-mode="true"
            @success="handleEditSuccess" 
            @cancel="goBack" 
          />
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getCurrentUserId } from '@/utils/auth'
import { getBarById } from '@/api/index'
import CreateBarForm from '@/components/CreateBarForm.vue'

// 路由和用户信息
const route = useRoute()
const router = useRouter()
const barId = parseInt(route.params.id as string)
const currentUserId = ref(getCurrentUserId() ? parseInt(getCurrentUserId()!) : null)

// 响应式数据
const loading = ref(true)
const barInfo = ref<any>(null)

// 加载贴吧信息
const loadBarInfo = async () => {
  try {
    loading.value = true
    
    const data = await getBarById(barId)
    
    // 检查权限：只有吧主可以编辑
    if (!currentUserId.value || data.ownerId !== currentUserId.value) {
      ElMessage.error('您没有权限编辑这个贴吧')
      router.push(`/bar/${barId}`)
      return
    }
    
    barInfo.value = data
  } catch (error) {
    console.error('加载贴吧信息失败:', error)
    ElMessage.error('加载贴吧信息失败')
    router.push('/bars')
  } finally {
    loading.value = false
  }
}

// 处理编辑成功
const handleEditSuccess = () => {
  ElMessage.success('贴吧信息更新成功！')
  router.push(`/bar/${barId}`)
}

// 返回
const goBack = () => {
  router.push(`/bar/${barId}`)
}

// 组件挂载时加载数据
onMounted(() => {
  if (!currentUserId.value) {
    ElMessage.warning('请先登录')
    router.push('/login')
    return
  }
  
  loadBarInfo()
})
</script>

<style scoped>
.bar-edit-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 40px 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.edit-wrapper {
  width: 100%;
  max-width: 800px;
}

.edit-card {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
}

.card-header {
  text-align: center;
}

.card-header h2 {
  margin: 0 0 8px 0;
  color: #409eff;
  font-size: 24px;
  font-weight: 600;
}

.subtitle {
  color: #909399;
  font-size: 14px;
}

.loading-container {
  display: flex;
  justify-content: center;
  padding: 60px;
}

.error-container {
  text-align: center;
  padding: 60px 20px;
}

.error-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.error-text {
  font-size: 16px;
  color: #666;
  margin-bottom: 24px;
}

.edit-form-container {
  padding: 20px 0;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .bar-edit-container {
    padding: 20px 10px;
  }
  
  .edit-card {
    margin: 0;
  }
}
</style>
