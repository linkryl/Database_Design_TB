<!--
创建贴吧表单组件
TreeHole开发组
2025-09-14
-->

<template>
  <div class="create-bar-form">
    <el-form 
      ref="formRef"
      :model="barForm" 
      :rules="formRules" 
      label-width="80px"
      class="bar-form"
    >
      <!-- 贴吧名称 -->
      <el-form-item label="贴吧名称" prop="barName">
        <el-input 
          v-model="barForm.barName"
          placeholder="请输入贴吧名称"
          maxlength="50"
          show-word-limit
          :disabled="loading"
        />
        <div class="form-hint">贴吧名称将作为唯一标识，创建后不可修改</div>
      </el-form-item>

      <!-- 贴吧描述 -->
      <el-form-item label="贴吧描述" prop="description">
        <el-input 
          v-model="barForm.description"
          type="textarea"
          placeholder="简单介绍一下这个贴吧..."
          :rows="4"
          maxlength="500"
          show-word-limit
          :disabled="loading"
        />
      </el-form-item>

      <!-- 贴吧规则 -->
      <el-form-item label="贴吧规则">
        <el-input 
          v-model="barForm.rules"
          type="textarea"
          placeholder="设置贴吧的管理规则（可选）"
          :rows="3"
          maxlength="1000"
          show-word-limit
          :disabled="loading"
        />
      </el-form-item>

      <!-- 贴吧标签 -->
      <el-form-item label="贴吧标签">
        <el-input 
          v-model="barForm.tags"
          placeholder="用逗号分隔的标签，如：技术,编程,学习"
          maxlength="100"
          show-word-limit
          :disabled="loading"
        />
        <div class="form-hint">标签有助于其他用户发现你的贴吧</div>
      </el-form-item>

      <!-- 头像上传 -->
      <el-form-item label="贴吧头像">
        <div class="avatar-upload-section">
          <el-upload
            class="avatar-uploader"
            action="#"
            :show-file-list="false"
            :before-upload="beforeAvatarUpload"
            :http-request="handleAvatarUpload"
            :disabled="loading"
          >
            <div class="avatar-preview">
              <img v-if="barForm.avatarUrl" :src="barForm.avatarUrl" alt="贴吧头像" />
              <div v-else class="avatar-placeholder">
                <el-icon><Plus /></el-icon>
                <div class="placeholder-text">点击上传头像</div>
              </div>
            </div>
          </el-upload>
          <div class="avatar-hint">
            <p>建议尺寸：200x200像素</p>
            <p>支持：JPG、PNG，最大2MB</p>
          </div>
        </div>
      </el-form-item>
    </el-form>

    <!-- 操作按钮 -->
    <div class="form-actions">
      <el-button @click="handleCancel" :disabled="loading">
        取消
      </el-button>
      <el-button 
        type="primary" 
        @click="handleSubmit"
        :loading="loading"
      >
        {{ loading ? (props.isEditMode ? '更新中...' : '创建中...') : (props.isEditMode ? '更新贴吧' : '创建贴吧') }}
      </el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, watch } from 'vue'
import { ElMessage, type FormInstance, type FormRules, type UploadRequestOptions } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import { getCurrentUserId } from '@/utils/auth'
import { createBar, updateBar, type THBar } from '@/api/index'

// Props
const props = defineProps<{
  initialData?: any
  isEditMode?: boolean
}>()

// Emits
const emit = defineEmits<{
  success: []
  cancel: []
}>()

// 响应式数据
const formRef = ref<FormInstance>()
const loading = ref(false)
const currentUserId = getCurrentUserId() ? parseInt(getCurrentUserId()!) : null

// 表单数据
const barForm = reactive({
  barName: '',
  description: '',
  rules: '',
  tags: '',
  avatarUrl: ''
})

// 初始化表单数据
const initializeForm = () => {
  if (props.isEditMode && props.initialData) {
    barForm.barName = props.initialData.barName || ''
    barForm.description = props.initialData.description || ''
    barForm.rules = props.initialData.rules || ''
    barForm.tags = props.initialData.tags || ''
    barForm.avatarUrl = props.initialData.avatarUrl || ''
  }
}

// 监听初始数据变化
watch(() => props.initialData, initializeForm, { immediate: true })

// 表单验证规则
const formRules: FormRules = {
  barName: [
    { required: true, message: '请输入贴吧名称', trigger: 'blur' },
    { min: 2, max: 50, message: '贴吧名称长度应在2-50个字符之间', trigger: 'blur' },
    { 
      pattern: /^[a-zA-Z0-9\u4e00-\u9fa5_-]+$/, 
      message: '贴吧名称只能包含中文、英文、数字、下划线和连字符', 
      trigger: 'blur' 
    }
  ],
  description: [
    { required: true, message: '请输入贴吧描述', trigger: 'blur' },
    { min: 10, message: '贴吧描述至少需要10个字符', trigger: 'blur' }
  ]
}

// 头像上传前验证
const beforeAvatarUpload = (file: File) => {
  const isImage = file.type === 'image/jpeg' || file.type === 'image/png'
  const isLt2M = file.size / 1024 / 1024 < 2

  if (!isImage) {
    ElMessage.error('头像只能是 JPG 或 PNG 格式!')
    return false
  }
  if (!isLt2M) {
    ElMessage.error('头像大小不能超过 2MB!')
    return false
  }
  return true
}

// 头像上传处理
const handleAvatarUpload = (options: UploadRequestOptions) => {
  try {
    // 创建本地预览URL
    const url = URL.createObjectURL(options.file)
    barForm.avatarUrl = url
    
    // 这里可以集成到对象存储，暂时使用本地预览
    options.onSuccess({ url })
    ElMessage.success('头像上传成功')
  } catch (error) {
    ElMessage.error('头像上传失败')
    options.onError(new Error('上传失败'))
  }
}

// 提交表单
const handleSubmit = async () => {
  if (!formRef.value || !currentUserId) return

  try {
    await formRef.value.validate()
    
    loading.value = true

    if (props.isEditMode && props.initialData) {
      // 编辑模式
      const barData: THBar = {
        barId: props.initialData.barId,
        ownerId: props.initialData.ownerId,
        barName: barForm.barName.trim(),
        description: barForm.description.trim(),
        rules: barForm.rules.trim() || undefined,
        tags: barForm.tags.trim() || undefined,
        avatarUrl: barForm.avatarUrl || undefined,
        status: props.initialData.status,
        followedCount: props.initialData.followedCount,
        postCount: props.initialData.postCount
      }

      console.log('更新贴吧数据:', barData)
      await updateBar(props.initialData.barId, barData)
    } else {
      // 创建模式
      const barData: THBar = {
        ownerId: currentUserId,
        barName: barForm.barName.trim(),
        description: barForm.description.trim(),
        rules: barForm.rules.trim() || undefined,
        tags: barForm.tags.trim() || undefined,
        avatarUrl: barForm.avatarUrl || undefined,
        status: 0,
        followedCount: 0,
        postCount: 0
      }

      console.log('创建贴吧数据:', barData)
      await createBar(barData)
    }
    
    // 不在编辑模式时才重置表单
    if (!props.isEditMode) {
      resetForm()
    }
    
    emit('success')
  } catch (error: any) {
    console.error(`${props.isEditMode ? '更新' : '创建'}贴吧失败:`, error)
    
    if (error.response?.data?.includes('贴吧名称已存在')) {
      ElMessage.error('贴吧名称已存在，请选择其他名称')
    } else if (error.response?.status === 400) {
      ElMessage.error('请求参数有误，请检查输入内容')
    } else {
      ElMessage.error(`${props.isEditMode ? '更新' : '创建'}贴吧失败，请重试`)
    }
  } finally {
    loading.value = false
  }
}

// 重置表单
const resetForm = () => {
  barForm.barName = ''
  barForm.description = ''
  barForm.rules = ''
  barForm.tags = ''
  barForm.avatarUrl = ''
}

// 取消操作
const handleCancel = () => {
  // 只在创建模式下重置表单
  if (!props.isEditMode) {
    resetForm()
  }
  
  emit('cancel')
}

// 组件挂载时初始化表单
onMounted(() => {
  initializeForm()
})
</script>

<style scoped>
.create-bar-form {
  padding: 20px 0;
}

.bar-form {
  margin-bottom: 20px;
}

.form-hint {
  font-size: 12px;
  color: #666;
  margin-top: 4px;
  line-height: 1.4;
}

/* 头像上传 */
.avatar-upload-section {
  display: flex;
  gap: 20px;
  align-items: flex-start;
}

.avatar-uploader {
  display: block;
}

.avatar-preview {
  width: 100px;
  height: 100px;
  border: 2px dashed #d9d9d9;
  border-radius: 8px;
  overflow: hidden;
  cursor: pointer;
  transition: border-color 0.3s ease;
}

.avatar-preview:hover {
  border-color: #409eff;
}

.avatar-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #fafafa;
  color: #999;
}

.placeholder-text {
  font-size: 12px;
  margin-top: 8px;
}

.avatar-hint {
  color: #666;
  font-size: 12px;
  line-height: 1.4;
}

.avatar-hint p {
  margin: 0 0 4px 0;
}

/* 操作按钮 */
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding-top: 20px;
  border-top: 1px solid #eee;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .avatar-upload-section {
    flex-direction: column;
    gap: 12px;
  }
  
  .form-actions {
    justify-content: center;
  }
}
</style>
