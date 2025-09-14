<!--
用户管理页面
TreeHole开发组
-->

<template>
  <div class="user-management-container">
    <div class="management-header">
      <h1>👥 用户管理</h1>
      <p class="management-subtitle">管理员可以封禁和解封用户</p>
    </div>

    <div class="management-content">
      <!-- 搜索栏和操作按钮 -->
      <div class="search-section">
        <div class="search-controls">
          <el-input
            v-model="searchKeyword"
            placeholder="搜索用户名..."
            :prefix-icon="Search"
            clearable
            @input="handleSearch"
            class="search-input"
          />
          <el-button 
            type="warning" 
            :icon="Setting"
            @click="fixAllUserStatus"
            :loading="fixLoading"
            class="fix-button"
          >
            修复用户状态
          </el-button>
        </div>
      </div>

      <!-- 用户列表 -->
      <div class="user-list">
        <el-card 
          v-for="user in filteredUsers" 
          :key="user.userId" 
          class="user-card"
          shadow="hover"
        >
          <div class="user-info">
            <div class="user-avatar">
              <img :src="githubLogoUrl" :alt="user.userName" />
            </div>
            <div class="user-details">
              <h3 class="username">{{ user.userName }}</h3>
              <p class="user-id">ID: {{ user.userId }}</p>
              <p class="user-role">
                <el-tag :type="user.role === 1 ? 'danger' : 'primary'">
                  {{ user.role === 1 ? '管理员' : '普通用户' }}
                </el-tag>
              </p>
              <p class="user-status">
                <el-tag :type="user.status === 1 ? 'success' : 'danger'">
                  {{ user.status === 1 ? '正常' : '已封禁' }}
                </el-tag>
              </p>
            </div>
          </div>
          
          <div class="user-actions">
            <el-button 
              v-if="user.role !== 1"
              :type="user.status === 1 ? 'danger' : 'success'"
              :icon="user.status === 1 ? 'Lock' : 'Unlock'"
              @click="handleUserAction(user)"
              :loading="actionLoading[user.userId]"
            >
              {{ user.status === 1 ? '封禁用户' : '解封用户' }}
            </el-button>
            <el-button 
              v-else
              disabled
              type="info"
            >
              管理员用户
            </el-button>
          </div>
        </el-card>
      </div>

      <!-- 分页 -->
      <div class="pagination-section">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="totalUsers"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </div>

    <!-- 封禁确认对话框 -->
    <el-dialog
      v-model="banDialogVisible"
      title="封禁用户"
      width="500px"
      :before-close="handleBanDialogClose"
    >
      <div class="ban-dialog-content">
        <div class="user-info-preview">
          <img :src="githubLogoUrl" :alt="selectedUser?.userName" class="preview-avatar" />
          <div>
            <h4>{{ selectedUser?.userName }}</h4>
            <p>ID: {{ selectedUser?.userId }}</p>
          </div>
        </div>
        
        <el-form :model="banForm" :rules="banRules" ref="banFormRef">
          <el-form-item label="封禁原因" prop="reason">
            <el-input
              v-model="banForm.reason"
              type="textarea"
              :rows="4"
              placeholder="请输入封禁原因..."
              maxlength="500"
              show-word-limit
            />
          </el-form-item>
        </el-form>
      </div>
      
      <template #footer>
        <el-button @click="banDialogVisible = false">取消</el-button>
        <el-button 
          type="danger" 
          @click="confirmBanUser"
          :loading="banLoading"
        >
          确认封禁
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang='ts'>
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox, FormInstance, FormRules } from 'element-plus'
import { Search, Lock, Unlock } from '@element-plus/icons-vue'
import axiosInstance from '../utils/axios'

// 响应式数据
const users = ref([])
const filteredUsers = ref([])
const searchKeyword = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const totalUsers = ref(0)
const actionLoading = ref({})
const banDialogVisible = ref(false)
const banLoading = ref(false)
const fixLoading = ref(false)
const selectedUser = ref(null)
const banFormRef = ref<FormInstance>()
const githubLogoUrl = '/images/GitHubLogo.png'

// 封禁表单
const banForm = ref({
  reason: ''
})

const banRules: FormRules = {
  reason: [
    { required: true, message: '请输入封禁原因', trigger: 'blur' },
    { min: 5, message: '封禁原因至少5个字符', trigger: 'blur' }
  ]
}

// 计算属性
const isAdmin = computed(() => {
  const userRole = localStorage.getItem('userRole')
  const isAdminFlag = localStorage.getItem('isAdmin')
  return userRole === '1' && isAdminFlag === 'true'
})

// 获取用户列表
const fetchUsers = async () => {
  try {
    const response = await axiosInstance.get('user')
    users.value = response.data
    filteredUsers.value = response.data
    totalUsers.value = response.data.length
  } catch (error) {
    console.error('获取用户列表失败:', error)
    ElMessage.error('获取用户列表失败')
  }
}

// 搜索用户
const handleSearch = () => {
  if (!searchKeyword.value.trim()) {
    filteredUsers.value = users.value
  } else {
    filteredUsers.value = users.value.filter(user => 
      user.userName.toLowerCase().includes(searchKeyword.value.toLowerCase())
    )
  }
}

// 处理用户操作
const handleUserAction = (user) => {
  if (user.status === 1) {
    // 封禁用户
    selectedUser.value = user
    banForm.value.reason = ''
    banDialogVisible.value = true
  } else {
    // 解封用户
    confirmUnbanUser(user)
  }
}

// 确认封禁用户
const confirmBanUser = async () => {
  if (!banFormRef.value) return
  
  try {
    const isValid = await banFormRef.value.validate()
    if (isValid) {
      banLoading.value = true
      
      const response = await axiosInstance.post(`user/admin/ban/${selectedUser.value.userId}`, {
        reason: banForm.value.reason
      })
      
      ElMessage.success('用户封禁成功')
      banDialogVisible.value = false
      
      // 更新用户状态
      const userIndex = users.value.findIndex(u => u.userId === selectedUser.value.userId)
      if (userIndex !== -1) {
        users.value[userIndex].status = 0
      }
      
      // 重新搜索
      handleSearch()
    }
  } catch (error) {
    console.error('封禁用户失败:', error)
    if (error.response?.status === 403) {
      ElMessage.error('权限不足，只有管理员可以封禁用户')
    } else {
      ElMessage.error('封禁用户失败')
    }
  } finally {
    banLoading.value = false
  }
}

// 确认解封用户
const confirmUnbanUser = async (user) => {
  try {
    await ElMessageBox.confirm(
      `确定要解封用户 "${user.userName}" 吗？`,
      '解封用户确认',
      {
        confirmButtonText: '确定解封',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    
    actionLoading.value[user.userId] = true
    
    const response = await axiosInstance.post(`user/admin/unban/${user.userId}`)
    
    ElMessage.success('用户解封成功')
    
    // 更新用户状态
    const userIndex = users.value.findIndex(u => u.userId === user.userId)
    if (userIndex !== -1) {
      users.value[userIndex].status = 1
    }
    
    // 重新搜索
    handleSearch()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('解封用户失败:', error)
      if (error.response?.status === 403) {
        ElMessage.error('权限不足，只有管理员可以解封用户')
      } else {
        ElMessage.error('解封用户失败')
      }
    }
  } finally {
    actionLoading.value[user.userId] = false
  }
}

// 关闭封禁对话框
const handleBanDialogClose = () => {
  banForm.value.reason = ''
  selectedUser.value = null
  banDialogVisible.value = false
}

// 分页处理
const handleSizeChange = (val) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val) => {
  currentPage.value = val
}

// 修复所有用户状态
const fixAllUserStatus = async () => {
  try {
    await ElMessageBox.confirm(
      '确定要修复所有普通用户的状态吗？这将把所有被封禁的普通用户设置为正常状态。',
      '修复用户状态确认',
      {
        confirmButtonText: '确定修复',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    
    fixLoading.value = true
    
    // 直接在前端修复用户状态
    const normalUsers = users.value.filter(user => user.role === 0 && user.status === 0)
    
    for (const user of normalUsers) {
      try {
        await axiosInstance.post(`user/admin/unban/${user.userId}`)
        // 更新本地状态
        user.status = 1
      } catch (error) {
        console.error(`修复用户 ${user.userName} 状态失败:`, error)
      }
    }
    
    ElMessage.success(`已修复 ${normalUsers.length} 个用户的状态`)
    
    // 重新搜索以更新显示
    handleSearch()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('修复用户状态失败:', error)
      ElMessage.error('修复用户状态失败')
    }
  } finally {
    fixLoading.value = false
  }
}

// 检查管理员权限
const checkAdminPermission = () => {
  const userRole = localStorage.getItem('userRole')
  const isAdminFlag = localStorage.getItem('isAdmin')
  const token = localStorage.getItem('jwtToken')
  
  if (!token || userRole !== '1' || isAdminFlag !== 'true') {
    ElMessage.error('权限不足，只有管理员可以访问此页面')
    setTimeout(() => {
      window.location.href = '/CommunityPage'
    }, 2000)
    return false
  }
  return true
}

onMounted(() => {
  if (checkAdminPermission()) {
    fetchUsers()
  }
})
</script>

<style scoped>
.user-management-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
}

.management-header {
  text-align: center;
  margin-bottom: 30px;
  color: white;
}

.management-header h1 {
  font-size: 32px;
  font-weight: bold;
  margin: 0 0 10px 0;
}

.management-subtitle {
  font-size: 16px;
  margin: 0;
  opacity: 0.9;
}

.management-content {
  max-width: 1200px;
  margin: 0 auto;
  background: white;
  border-radius: 20px;
  padding: 30px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
}

.search-section {
  margin-bottom: 30px;
}

.search-controls {
  display: flex;
  gap: 15px;
  align-items: center;
  flex-wrap: wrap;
}

.search-input {
  max-width: 400px;
  flex: 1;
  min-width: 200px;
}

.fix-button {
  white-space: nowrap;
}

.user-list {
  margin-bottom: 30px;
}

.user-card {
  margin-bottom: 20px;
  transition: all 0.3s ease;
}

.user-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
}

.user-info {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 15px;
}

.user-avatar img {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  object-fit: cover;
}

.user-details h3 {
  margin: 0 0 8px 0;
  color: #2c3e50;
  font-size: 18px;
}

.user-details p {
  margin: 4px 0;
  color: #7f8c8d;
  font-size: 14px;
}

.user-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.pagination-section {
  display: flex;
  justify-content: center;
  margin-top: 30px;
}

.ban-dialog-content {
  padding: 20px 0;
}

.user-info-preview {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 20px;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 10px;
}

.preview-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  object-fit: cover;
}

.user-info-preview h4 {
  margin: 0 0 5px 0;
  color: #2c3e50;
}

.user-info-preview p {
  margin: 0;
  color: #7f8c8d;
  font-size: 14px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .management-content {
    margin: 0 10px;
    padding: 20px;
  }
  
  .user-info {
    flex-direction: column;
    text-align: center;
  }
  
  .user-actions {
    justify-content: center;
  }
}
</style>
