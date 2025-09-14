<!--
管理员登录页面
TreeHole开发组
-->

<template>
  <div class='admin-login-container'>
    <div class='admin-card-wrapper'>
      <el-card class='admin-card-style' shadow='always'>
        <div class='admin-header'>
          <h1>🔐 管理员登录</h1>
          <p class='admin-subtitle'>TreeHole 管理后台</p>
        </div>

        <el-form 
          ref='adminLoginFormRef' 
          style='max-width: 100%' 
          :model='adminLoginForm' 
          :rules='adminLoginRules'
        > 
          <el-form-item prop="username">
            <label for='input-admin-username'>管理员账号</label>
            <el-input
              id='input-admin-username'
              v-model='adminLoginForm.username'
              placeholder='请输入管理员账号'
              size='large'
              type='text'
              :prefix-icon='User'
              autocomplete='on'
              clearable
            />
          </el-form-item>
          
          <el-form-item prop='password'>
            <label for='input-admin-password'>管理员密码</label>
            <el-input 
              id='input-admin-password'
              v-model='adminLoginForm.password' 
              type='password' 
              placeholder="请输入管理员密码" 
              :prefix-icon='Lock'
              size='large' 
              autocomplete='off' 
              show-password 
              clearable
            />
          </el-form-item>

          <el-button 
            class='btn-admin-login' 
            type='primary' 
            size='large'
            plain
            round 
            :loading='isLoading'
            @click='handleAdminLogin(adminLoginFormRef)'
          >
            <span v-if='!isLoading'>管理员登录</span>
            <span v-else>登录中...</span>
          </el-button>
        </el-form>

        <div class='admin-link-wrapper'>
          <el-link 
            type='primary' 
            :underline='true'
            @click="router.push('/login')"
          >
            ← 返回普通用户登录
          </el-link>
        </div>

        <div class='admin-tips'>
          <p class='tips-title'>💡 管理员提示</p>
          <ul class='tips-list'>
            <li>管理员可以封禁和解封用户</li>
            <li>管理员可以查看举报信息</li>
            <li>管理员可以管理用户内容</li>
          </ul>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { useRouter } from 'vue-router'
import { onMounted, reactive, ref } from 'vue'
import { ElFormItem, ElMessage, FormInstance, FormRules } from 'element-plus'
import axiosInstance from '../utils/axios'
import { User, Lock } from '@element-plus/icons-vue'

const router = useRouter()
const adminLoginFormRef = ref<FormInstance>()
const isLoading = ref(false)

interface AdminLoginForm {
  username: string
  password: string
}

const adminLoginForm = reactive<AdminLoginForm>({
  username: '',
  password: ''
})

const adminLoginRules: FormRules = {
  username: [
    {
      required: true,
      message: '管理员账号为必填项',
      trigger: 'change'
    }
  ],
  password: [
    {
      required: true,
      message: '管理员密码为必填项',
      trigger: 'change'
    }
  ]
}

const handleAdminLogin = async (elFormRef: FormInstance | undefined) => {
  if (!elFormRef || !adminLoginForm.password || !adminLoginForm.username) {
    ElMessage.error('管理员账号和密码都不能为空')
    return
  }

  try {
    const isValid = await elFormRef.validate()
    if (isValid) {
      isLoading.value = true
      
      try {
        console.log('发送管理员登录请求...')
        console.log('请求URL:', axiosInstance.defaults.baseURL + 'admin-login')
        console.log('请求数据:', { username: adminLoginForm.username, password: '***' })
        
        const response = await axiosInstance.post('http://localhost:5101/api/admin-login', {
          username: adminLoginForm.username,
          password: adminLoginForm.password
        })
        
        console.log('响应状态:', response.status)
        console.log('响应数据:', response.data)

        if (response.data && response.data.token) {
          // 保存管理员token和用户信息
          localStorage.setItem('jwtToken', response.data.token)
          localStorage.setItem('currentUserId', response.data.userId.toString())
          localStorage.setItem('userRole', response.data.role.toString())
          localStorage.setItem('isAdmin', 'true')
          
          // 触发自定义事件，通知其他组件状态已更新
          window.dispatchEvent(new CustomEvent('authStateChanged', {
            detail: {
              userId: response.data.userId,
              userRole: response.data.role,
              isAdmin: true
            }
          }))
          
          ElMessage.success('管理员登录成功！')
          
          // 跳转到用户管理页面
          router.push('/user-management')
        } else {
          ElMessage.error('登录失败，请检查账号密码')
        }
      } catch (error: any) {
        console.error('管理员登录失败:', error)
        console.error('错误详情:', error.response?.data)
        
        if (error.response?.status === 401) {
          ElMessage.error('管理员账号或密码错误')
        } else if (error.response?.status === 500) {
          const errorMessage = error.response?.data?.message || '服务器内部错误'
          ElMessage.error(`服务器错误: ${errorMessage}`)
        } else {
          ElMessage.error(`登录失败: ${error.message}`)
        }
      } finally {
        isLoading.value = false
      }
    }
  } catch (error) {
    console.error('表单验证失败:', error)
    isLoading.value = false
  }
}

onMounted(() => {
  // 检查是否已经是管理员登录状态
  const isAdmin = localStorage.getItem('isAdmin')
  const userRole = localStorage.getItem('userRole')
  
  if (isAdmin === 'true' && userRole === '1') {
    ElMessage.info('您已经是管理员登录状态')
    router.push('/CommunityPage')
  }
})
</script>

<style scoped>
.admin-login-container {
  display: flex;
  height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.admin-card-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 20px;
}

.admin-card-style {
  width: 100%;
  max-width: 500px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.admin-header {
  text-align: center;
  margin-bottom: 30px;
}

.admin-header h1 {
  color: #2c3e50;
  font-size: 28px;
  font-weight: bold;
  margin: 0 0 10px 0;
}

.admin-subtitle {
  color: #7f8c8d;
  font-size: 16px;
  margin: 0;
}

label {
  font-weight: 600;
  font-size: 15px;
  color: #2c3e50;
  display: block;
  margin-bottom: 8px;
}

.admin-link-wrapper {
  display: flex;
  justify-content: center;
  margin: 20px 0;
}

.btn-admin-login {
  margin-top: 30px;
  margin-bottom: 20px;
  width: 100%;
  height: 50px;
  font-size: 16px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  border-radius: 25px;
  transition: all 0.3s ease;
}

.btn-admin-login:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 20px rgba(102, 126, 234, 0.3);
}

.admin-tips {
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border-radius: 15px;
  padding: 20px;
  margin-top: 20px;
  border-left: 4px solid #667eea;
}

.tips-title {
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 15px 0;
  font-size: 16px;
}

.tips-list {
  margin: 0;
  padding-left: 20px;
  color: #6c757d;
  line-height: 1.6;
}

.tips-list li {
  margin-bottom: 8px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .admin-card-style {
    margin: 20px;
    width: calc(100% - 40px);
  }
  
  .admin-header h1 {
    font-size: 24px;
  }
}
</style>

