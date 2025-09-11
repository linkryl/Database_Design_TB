<!--
  个人资料展示页面
  2353814 马小龙 | TreeHole 项目开发组
-->

<template>
  <div class='personal-zone-container'>
    <!-- 顶部背景区域 -->
    <div class='profile-header-bg'>
      <div class='header-pattern'></div>
    </div>
    
    <!-- 主内容区 -->
    <div class='profile-main-wrapper'>
      <!-- 用户头像和基本信息栏 -->
      <div class='user-info-bar'>
        <el-avatar class='profile-avatar' :src='`${ossBaseUrl}${userInfo.avatarUrl}`'>
          <el-icon :size='60'>
            <UserFilled/>
          </el-icon>
        </el-avatar>
        
        <div class='user-basic-info'>
          <div class='name-row'>
            <h1 class='display-name'>{{ userInfo.userName }}</h1>
            <span class='uid-tag'>UID: {{ userInfo.uid }}</span>
          </div>
          <!-- 加入时间信息 -->
          <p class='join-info'>用户角色: {{ formatRole(userInfo.role) }}</p>
        </div>

        <!-- 操作按钮区 -->
        <div class='action-area'>
          <el-button v-if='isSelf' 
                     type='primary' 
                     class='edit-btn'
                     @click='openEditPanel'>
            <el-icon><EditPen/></el-icon>
            编辑资料
          </el-button>
        </div>
      </div>

      <!-- 内容展示区 -->
      <div class='content-display-area'>
        <!-- 个人信息展示卡片 -->
        <div class='info-display-card'>
          <div class='card-header'>
            <el-icon class='header-icon'><User/></el-icon>
            <span class='header-title'>个人信息</span>
          </div>
          
          <div class='info-content'>

            <!-- 基本信息网格 -->
            <div class='basic-info-grid'>
              <div class='info-grid-item'>
                <span class='grid-label'>性别</span>
                <span class='grid-value'>{{ formatGender(userInfo.gender) }}</span>
              </div>
              <div class='info-grid-item'>
                <span class='grid-label'>生日</span>
                <span class='grid-value'>{{ formatBirthdate(userInfo.birthdate) }}</span>
              </div>
              <div class='info-grid-item'>
                <span class='grid-label'>角色</span>
                <span class='grid-value'>{{ formatRole(userInfo.role) }}</span>
              </div>
              <div class='info-grid-item'>
                <span class='grid-label'>状态</span>
                <span class='grid-value'>{{ formatStatus(userInfo.status) }}</span>
              </div>
            </div>

            <!-- 个人简介 -->
            <div class='intro-section'>
              <div class='section-label'>个人简介</div>
              <div class='intro-text'>
                {{ userInfo.profile || '这个人很神秘，什么都没有留下...' }}
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>

    <!-- 编辑资料对话框 -->
    <el-dialog 
      v-model='showEditDialog' 
      title='编辑个人资料'
      width='650px'
      :close-on-click-modal='false'
      class='edit-dialog'>
      
      <el-form :model='editData' label-width='100px' class='edit-form'>
        <!-- UID -->
        <el-form-item label='UID'>
          <el-input 
            :value='userInfo.uid' 
            disabled
            style='background: #f5f5f5'/>
        </el-form-item>

        <!-- 昵称 -->
        <el-form-item label='昵称'>
          <el-input 
            :value='userInfo.userName' 
            disabled
            style='background: #f5f5f5'/>
        </el-form-item>

        <!-- 头像 -->
        <el-form-item label='头像'>
          <div class='avatar-upload-area'>
            <el-upload
              class='avatar-uploader'
              action='#'
              :show-file-list='false'
              :before-upload='validateAvatar'
              :http-request='handleAvatarUpload'>
              <el-avatar :size='100' :src='previewAvatarUrl || `${ossBaseUrl}${editData.avatarUrl}`'>
                <el-icon :size='40'><Plus/></el-icon>
              </el-avatar>
            </el-upload>
            <div class='avatar-tips'>
              点击上传新头像<br>
              只支持 JPG，最大 2MB
            </div>
          </div>
        </el-form-item>

        <!-- 性别 -->
        <el-form-item label='性别'>
          <el-radio-group v-model='editData.gender'>
            <el-radio :label='0'>男性</el-radio>
            <el-radio :label='1'>女性</el-radio>
          </el-radio-group>
        </el-form-item>

        <!-- 生日 -->
        <el-form-item label='生日'>
          <el-date-picker
            v-model='editData.birthdate'
            type='date'
            placeholder='选择出生日期'
            format='YYYY-MM-DD'
            value-format='YYYY-MM-DD'
            :disabled-date='disableFutureDate'
            style='width: 100%'/>
        </el-form-item>

        <!-- 个人简介 -->
        <el-form-item label='个人简介'>
          <el-input
            v-model='editData.profile'
            type='textarea'
            :rows='5'
            maxlength='300'
            show-word-limit
            placeholder='介绍一下自己，让大家更了解你~'/>
        </el-form-item>
      </el-form>

      <template #footer>
        <div class='dialog-footer'>
          <el-button @click='cancelEdit'>取消</el-button>
          <el-button type='primary' @click='updateProfile'>保存修改</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang='ts'>
import {computed, onMounted, onBeforeUnmount, ref, watch} from 'vue'
import axiosInstance from '../utils/axios'
import {ElMessage} from 'element-plus'
import {useRoute, useRouter} from 'vue-router'
import {formatDateTimeToCST, ossBaseUrl} from '../globals'
import {UserFilled, User, EditPen, Plus} from '@element-plus/icons-vue'

interface UploadRequestOptions {
  action: string
  method: string
  data: any
  filename: string
  file: File
  headers: Record<string, string>
  withCredentials: boolean
  onProgress: (evt: ProgressEvent) => void
  onSuccess: (response: any) => void
  onError: (error: Error) => void
}

const route = useRoute()
const router = useRouter()
const viewedUserId = ref(parseInt(route.params.id as string))
const localUserId = localStorage.getItem('currentUserId')
const currentUserId = ref(localUserId ? parseInt(localUserId) : 0)
const isSelf = computed(() => viewedUserId.value === currentUserId.value && currentUserId.value !== 0)

// 用户页面数据
const userInfo = ref({
  uid: viewedUserId.value,
  userName: '',
  role: 0,
  status: 0,
  avatarUrl: '',
  profile: '',
  gender: 0,
  birthdate: ''
})

const showEditDialog = ref(false)
const editData = ref({
  avatarUrl: '',
  profile: '',
  gender: 0,
  birthdate: ''
})

// 头像预览URL
const previewAvatarUrl = ref('')
const uploadedFile = ref<File | null>(null)

const formatGender = (gender: number) => {
  return gender === 0 ? '男' : '女'
}

const formatBirthdate = (birthdate: string) => {
  if (!birthdate) return '未设置'
  try {
    return formatDateTimeToCST(birthdate).date
  } catch {
    return birthdate
  }
}

const formatRole = (role: number) => {
  return role === 0 ? '普通用户' : '管理员'
}

const formatStatus = (status: number) => {
  return status === 0 ? '正常' : '已禁用'
}

// 获取用户信息
const fetchUserProfile = async () => {
  try {
    // GET接口，从后端获取信息
    const res = await axiosInstance.get(`user/${viewedUserId.value}`)
    if (res.status === 404) {
      await router.push('/404')
      return
    }
    
    const data = res.data
    userInfo.value = {
      uid: data.userId || viewedUserId.value,
      userName: data.userName || '',
      role: data.role || 0,
      status: data.status || 0,
      avatarUrl: data.avatarUrl || 'default-avatar.png',
      profile: data.profile || '',
      gender: data.gender !== undefined ? data.gender : 0,
      birthdate: data.birthdate || ''
    }
  } catch (error) {
    ElMessage.error('GET:获取用户信息失败')
    console.error('GET:获取用户信息失败:', error)
  }
}

// 打开编辑对话框
const openEditPanel = () => {
  editData.value = {
    avatarUrl: userInfo.value.avatarUrl,
    profile: userInfo.value.profile,
    gender: userInfo.value.gender,
    birthdate: userInfo.value.birthdate
  }
  // 重置预览URL和上传文件
  previewAvatarUrl.value = ''
  uploadedFile.value = null
  showEditDialog.value = true
}

// 取消编辑
const cancelEdit = () => {
  // 清理预览URL和上传文件
  previewAvatarUrl.value = ''
  uploadedFile.value = null
  showEditDialog.value = false
}

// 保存修改
const updateProfile = async () => {
  try {
    // 如果有新头像，先上传头像
    if (uploadedFile.value) {
      const formData = new FormData()
      formData.append('file', uploadedFile.value, 'avatar.jpg')
      
      try {
        // POST接口，头像文件上传到服务器
        const uploadRes = await axiosInstance.post('upload-avatar', formData)
        if (uploadRes.data && uploadRes.data.fileName) {
          // PUT接口，更新数据库里用户的 avatarUrl 字段
          await axiosInstance.put(`user/avatar-url/${viewedUserId.value}`, {
            avatarUrl: uploadRes.data.fileName
          })
          editData.value.avatarUrl = uploadRes.data.fileName
        }
      } catch (uploadError) {
        ElMessage.error('头像上传失败')
        console.error('头像上传失败:', uploadError)
        return
      }
    }
    
    // PUT接口，更新用户信息
    const updatePayload = {
      profile: editData.value.profile,
      gender: editData.value.gender,
      birthdate: editData.value.birthdate
    }
    
    await axiosInstance.put(`user/personal-information/${viewedUserId.value}`, updatePayload)
    
    // 更新显示数据
    userInfo.value.profile = editData.value.profile
    userInfo.value.gender = editData.value.gender
    userInfo.value.birthdate = editData.value.birthdate
    if (editData.value.avatarUrl) {
      userInfo.value.avatarUrl = editData.value.avatarUrl
    }
    
    // 清理并关闭对话框
    previewAvatarUrl.value = ''
    uploadedFile.value = null
    showEditDialog.value = false
    ElMessage.success('资料更新成功')
  } catch (error) {
    ElMessage.error('保存失败，请稍后重试')
  }
}

// 头像验证
const validateAvatar = (file: File): boolean => {
  const isImage = file.type === 'image/jpeg' || file.type === 'image/jpg'
  const isLt2M = file.size / 1024 / 1024 < 2
  
  if (!isImage) {
    ElMessage.error('只支持 JPG 格式的图片')
    return false
  }
  if (!isLt2M) {
    ElMessage.error('图片大小不能超过 2MB')
    return false
  }
  return true
}

// 处理头像上传
const handleAvatarUpload = (options: UploadRequestOptions) => {
  try {
    // 创建本地预览URL
    previewAvatarUrl.value = URL.createObjectURL(options.file)
    // 保存文件供后续上传
    uploadedFile.value = options.file
    // 调用成功回调以关闭上传loading状态
    options.onSuccess({ success: true })
    ElMessage.success('头像已选择，点击保存修改后生效')
  } catch (error) {
    ElMessage.error('头像上传失败')
  }
}

// 禁用未来日期
const disableFutureDate = (date: Date) => {
  return date.getTime() > Date.now()
}

// 监听路由变化
watch(() => route.params.id, (newId) => {
  viewedUserId.value = parseInt(newId as string)
  userInfo.value.uid = viewedUserId.value
  fetchUserProfile()
})

// 组件卸载时清理URL
onMounted(() => {
  fetchUserProfile()
})

// 组件卸载时清理URL
onBeforeUnmount(() => {
  if (previewAvatarUrl.value) {
    URL.revokeObjectURL(previewAvatarUrl.value)
  }
})
</script>

<style scoped>
/* 容器 */
.personal-zone-container {
  min-height: calc(100vh - 135px);
  background: #f0f2f5;
}

/* 顶部背景 */
.profile-header-bg {
  height: 260px;
  background: linear-gradient(120deg, #2563eb 0%, #3b82f6 100%);
  position: relative;
  overflow: hidden;
}

.header-pattern {
  position: absolute;
  width: 100%;
  height: 100%;
  opacity: 0.1;
  background-image: repeating-linear-gradient(45deg, transparent, transparent 35px, rgba(255,255,255,.1) 35px, rgba(255,255,255,.1) 70px);
}

/* 主内容包装器 */
.profile-main-wrapper {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
  position: relative;
  margin-top: -100px;
}

/* 用户信息栏 */
.user-info-bar {
  background: white;
  border-radius: 16px;
  padding: 30px;
  display: flex;
  align-items: center;
  gap: 30px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.08);
  margin-bottom: 30px;
}

.profile-avatar {
  width: 120px;
  height: 120px;
  border: 4px solid white;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  flex-shrink: 0;
}

.user-basic-info {
  flex: 1;
}

.name-row {
  display: flex;
  align-items: baseline;
  gap: 15px;
  margin-bottom: 10px;
}

.display-name {
  font-size: 32px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}

.uid-tag {
  background: #e3f2fd;
  color: #1976d2;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 14px;
}

.join-info {
  color: #757575;
  font-size: 14px;
  margin: 0;
}

.action-area {
  flex-shrink: 0;
}

.edit-btn {
  background: #2563eb;
  border: none;
  padding: 12px 24px;
  font-size: 15px;
}

.edit-btn:hover {
  background: #1d4ed8;
}

/* 内容展示区 */
.content-display-area {
  display: grid;
  gap: 30px;
  margin-bottom: 40px;
}

/* 信息展示卡片 */
.info-display-card {
  background: white;
  border-radius: 16px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.06);
}

.card-header {
  padding: 20px 25px;
  border-bottom: 1px solid #e8e8e8;
  display: flex;
  align-items: center;
  gap: 10px;
}

.header-icon {
  font-size: 20px;
  color: #2563eb;
}

.header-title {
  font-size: 18px;
  font-weight: 500;
  color: #1a1a1a;
}

.info-content {
  padding: 25px;
}

.intro-section {
  margin-top: 30px;
}

.section-label {
  font-size: 14px;
  color: #757575;
  margin-bottom: 10px;
}

.intro-text {
  font-size: 15px;
  line-height: 1.8;
  color: #424242;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 8px;
}

.basic-info-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
}

.info-grid-item {
  display: flex;
  align-items: center;
  gap: 12px;
}

.grid-label {
  color: #9e9e9e;
  font-size: 14px;
  min-width: 60px;
}

.grid-value {
  color: #424242;
  font-size: 15px;
  font-weight: 500;
}

/* 编辑对话框 */
.edit-form {
  padding: 10px 0;
}

.avatar-upload-area {
  display: flex;
  align-items: center;
  gap: 30px;
}

.avatar-uploader .el-avatar {
  cursor: pointer;
  transition: opacity 0.3s;
}

.avatar-uploader .el-avatar:hover {
  opacity: 0.8;
}

.avatar-tips {
  font-size: 13px;
  color: #757575;
  line-height: 1.6;
}

.dialog-footer {
  padding-top: 10px;
}
</style>