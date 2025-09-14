<!--
TreeHole 发帖页面
2351270 王天一
-->

<template>
  <div class="th-post-edit-container">
    <!-- 添加背景图片 -->
    <img :src="`${ossBaseUrl}HomePage/BackgroundImage.jpg`" alt="Background" class="th-background-image">
    
    <el-card class="th-post-edit-card">
      <template #header>
        <div class="th-card-header">
          <h2>📝 发表树洞</h2>
          <span class="th-subtitle">分享你的心声</span>
        </div>
      </template>
      
      <el-form 
        :model="thPostForm" 
        :rules="thFormRules" 
        ref="thFormRef"
        label-position="top" 
        class="th-post-form"
      >
        <el-form-item label="标题" prop="title">
          <el-input 
            v-model="thPostForm.title" 
            placeholder="给你的树洞起个标题吧..."
            maxlength="100"
            show-word-limit
            :disabled="thLoading"
          />
        </el-form-item>
        
        <el-form-item label="内容" prop="content">
          <el-input 
            v-model="thPostForm.content" 
            type="textarea" 
            :rows="8"
            placeholder="说出你的心里话..."
            maxlength="2000"
            show-word-limit
            :disabled="thLoading"
          />
        </el-form-item>
        
        <el-form-item>
          <div class="th-form-actions">
            <el-button @click="handleCancel" :disabled="thLoading">
              取消
            </el-button>
            <el-button 
              type="primary" 
              @click="handleSubmit" 
              :loading="thLoading"
            >
              {{ thLoading ? '发布中...' : '发布树洞' }}
            </el-button>
          </div>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { createPost } from '@/api/index'
import { getCurrentUserId } from '@/utils/auth'
import { ossBaseUrl } from '@/globals'
import axiosInstance from '@/utils/axios'

// TreeHole前缀命名
const router = useRouter()
const thFormRef = ref<FormInstance>()
const thLoading = ref(false)

// 表单数据
const thPostForm = reactive({
  title: '',
  content: ''
})

// 表单验证规则
const thFormRules: FormRules = {
  title: [
    { required: true, message: '请输入标题', trigger: 'blur' },
    { max: 100, message: '标题长度不能超过100个字符', trigger: 'blur' }
  ],
  content: [
    { required: true, message: '请输入内容', trigger: 'blur' }
  ]
}

// 检查登录状态
onMounted(() => {
  const thCurrentUserId = getCurrentUserId()
  if (!thCurrentUserId) {
    ElMessage.warning('请先登录')
    router.push('/login')
    return
  }
})

// 处理取消
const handleCancel = () => {
  router.back()
}

// 处理提交
const handleSubmit = async () => {
  if (!thFormRef.value) return
  
  try {
    // 表单验证
    await thFormRef.value.validate()
    
    // 检查用户登录状态
    const thCurrentUserId = getCurrentUserId()
    if (!thCurrentUserId) {
      ElMessage.error('用户未登录')
      router.push('/login')
      return
    }
    
    thLoading.value = true
    
    // 调用API创建帖子 - 构造完整的Post对象
    let categoryId = 1 // 默认分类ID
    
    // 尝试获取可用的分类，如果失败则使用默认值
    try {
      const categoriesResponse = await axiosInstance.get('post-category')
      if (categoriesResponse.data && categoriesResponse.data.length > 0) {
        categoryId = categoriesResponse.data[0].categoryId // 使用第一个可用分类
        console.log('使用分类ID:', categoryId, '分类名:', categoriesResponse.data[0].category)
      }
    } catch (error) {
      console.warn('获取分类失败，使用默认分类ID:', categoryId, error)
    }
    
    const thCreateData = {
      userId: parseInt(thCurrentUserId),
      categoryId: categoryId, // 使用动态获取的分类ID
      title: thPostForm.title.trim(),
      content: thPostForm.content.trim(),
      creationDate: new Date().toISOString(),
      updateDate: new Date().toISOString(),
      isSticky: 0, // 不置顶
      likeCount: 0,
      dislikeCount: 0,
      favoriteCount: 0,
      commentCount: 0,
      imageUrl: null as any
    }
    
    console.log('TreeHole: 正在创建帖子:', thCreateData)
    
    const thResult = await createPost(thCreateData)
    
    console.log('TreeHole: 创建成功:', thResult)
    
    ElMessage.success('树洞发布成功！')
    
    // 成功后跳转到首页，延迟0.8秒以显示成功消息
    setTimeout(() => {
      router.push('/')
    }, 800)
    
  } catch (error: any) {
    console.error('TreeHole: 发布失败:', error)
    
    if (error.response?.data?.message) {
      ElMessage.error(`发布失败: ${error.response.data.message}`)
    } else {
      ElMessage.error('发布失败，请重试')
    }
  } finally {
    thLoading.value = false
  }
}
</script>

<style scoped>
.th-post-edit-container {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

/* 背景图片样式 */
.th-background-image {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  z-index: -1;
  opacity: 0.8;
}

.th-post-edit-card {
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  max-width: 800px;
  width: 100%;
}

.th-card-header {
  text-align: center;
}

.th-card-header h2 {
  margin: 0;
  color: #409eff;
  font-size: 24px;
  font-weight: 600;
}

.th-subtitle {
  color: #909399;
  font-size: 14px;
}

.th-post-form {
  margin-top: 20px;
}

.th-form-actions {
  display: flex;
  justify-content: center;
  gap: 16px;
  margin-top: 20px;
}

:deep(.el-textarea__inner) {
  resize: vertical;
  min-height: 120px;
}

:deep(.el-form-item__label) {
  font-weight: 500;
  color: #303133;
}
</style>
