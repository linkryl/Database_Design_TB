<!--
组件内容: 登录页面
开发作者: TreeHole 开发组
-->

<template>
  <div class='login-page-container'>
    <div class='el-card-wrapper'>
      <el-card class='el-card-style' shadow='always'>
        <h1>登录 TreeHole</h1>

        <el-form 
          ref='loginFormRef' 
          style='max-width: 100%' 
          :model='loginForm' 
          :rules='loginRules'
        > 
          <el-form-item prop="username">
            <label for='input-username'>用户名</label>

            <el-input
              id='input-username'
              v-model='loginForm.username'
              placeholder='请输入用户名'
              size='large'
              type='text'
              :prefix-icon='User'
              autocomplete='on'
              clearable
            />
          </el-form-item>
          
          <el-form-item prop='password'>
            <label for='input-password'>密码</label>

            <el-input 
              id='input-password'
              v-model='loginForm.password' 
              type='password' 
              placeholder="请输入密码" 
              :prefix-icon='Lock'
              size='large' 
              autocomplete='off' 
              show-password 
              clearable
            />
          </el-form-item>

          <el-button 
            class='btn-login' 
            type='primary' 
            size='large'
            plain
            round 
            @click='handleSubmitLogin(loginFormRef)'
          >
            登 录
          </el-button>
        </el-form>

        <div class='el-link-wrapper'>
          <el-link 
            type='primary' 
            :underline='true'
            @click="router.push('/register')"
          >
            没有账号? 点击这里注册
          </el-link>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { useRouter } from 'vue-router'
import { onMounted, onUnmounted, reactive, ref, watch } from 'vue'
import { ElFormItem, ElMessage, FormInstance, FormRules } from 'element-plus'
import axiosInstance from '../utils/axios'
import { User, Lock } from '@element-plus/icons-vue'

const router = useRouter()
const isUsernameUnique = ref(-1)
const loginFormRef = ref<FormInstance>()
const localStorageValue = localStorage.getItem('currentUserId')
const localStorageUserId = localStorageValue ? parseInt(localStorageValue) : 0
const currentUserId = ref(isNaN(localStorageUserId) ? 0 : localStorageUserId)
const windowWidth = ref(window.innerWidth)

onMounted(() => {
  try{
    if (currentUserId.value != 0) {
      router.push(`/profile/${currentUserId.value}`)
    }
  }catch(error){
    console.log('个人资料加载失败')
  }
})

interface LoginForm {
  username: string
  password: string
}

const loginForm = reactive<LoginForm>({
  username: '',
  password: ''
})

const loginRules: FormRules = {
  // telephone: [// TODO: 到时候删掉
  //   {
  //     required: true,
  //     message: t('RegisterPage.EmptyTelephone'),
  //     trigger: 'change',
  //   },
  //   {
  //     validator: (rule, value, callback) => {
  //       const phoneRegex = /^\d{11}$/
  //       if (value && !phoneRegex.test(value)) {
  //         callback(new Error(t('RegisterPage.InvalidTelephone')))// TODO: 修改成检查用户名是否合法
  //       } else {
  //         callback()
  //       }
  //     },
  //     trigger: 'blur'
  //   }
  // ],
  username:[
    {
      required: true,
      message: '用户名为必填项, 不能为空',
      trigger: 'change'
    },
    {
      validator: (rule, value, callback) => {
        if (value && isUsernameUnique.value == 0) {
          callback(new Error('该用户名未注册'))
        } else {
          callback()
        }
      },
      trigger: 'change'
    }
  ],
  password: [
    {
      required: true,
      message: '密码为必填项, 不能为空',
      trigger: 'change'
    }
  ]
}

watch(() => loginForm.username, (newValue) => {
  if (newValue) {
    checkUsernameUnique(newValue)
  }
})

onMounted(() => {
  if (loginForm.username) {
    checkUsernameUnique(loginForm.username)
  }
})

const checkUsernameUnique = async (username: string) => {
  try {                                 
    const response = await axiosInstance.get(`user/check-username-unique/${username}`)
    isUsernameUnique.value = response.data
  } catch (error) {
    ElMessage.error('GET 请求失败，请检查网络连接情况或稍后重试')
    isUsernameUnique.value = -1
  }
}

const handleSubmitLogin = async (elFormRef: FormInstance | undefined) => {
  if (!elFormRef || !loginForm.password || !loginForm.username) {
    ElMessage.error('用户名和密码都不能为空')
    return
  }
  try {
    const isValid = await elFormRef.validate();
    if (isValid) {
      let userId = 0
      let isPasswordCorrect = -1
      try {                                   
        const response = await axiosInstance.get(`user/get-user-id-by-username/${loginForm.username}`)
        userId = response.data
        try {
          const response = await axiosInstance.post('user/verify-password', {
            userId: userId,
            plainPassword: loginForm.password
          })
          isPasswordCorrect = response.data
          if (isPasswordCorrect == -1) {
            ElMessage.error('网络发生错误!')
          } else if (isPasswordCorrect == 0) {
            ElMessage.error('密码不正确!')
          } else {
            try {
              axiosInstance.put(`user/last-login-time/${userId}`, {
                lastLoginTime: new Date().toISOString()
              }).then(() => {
                localStorage.setItem('currentUserId', userId.toString())
                router.push('/')
                window.location.href = '/'
              }).catch(() => {
                ElMessage.error('登录失败，请检查网络连接情况或稍后重试')
              })
            } catch (error) {
              ElMessage.error('登录失败，请检查网络连接情况或稍后重试')
            }
            // try {
            //   localStorage.setItem('currentUserId', userId.toString())
            //   router.push('/')
            //   window.location.href = '/'
            // } catch (error) {
            //   ElMessage.error('登录失败，请检查网络连接情况或稍后重试')
            // }
          }
        } catch (error) {
          ElMessage.error('登录失败，请检查网络连接情况或稍后重试')
        }
      } catch (error) {
        ElMessage.error('登录失败，请检查网络连接情况或稍后重试')
      }
    }
  } catch (error) {
  }
}

function updateWindowWidth() {
  windowWidth.value = window.innerWidth
}

onMounted(() => window.addEventListener('resize', updateWindowWidth))

onUnmounted(() => window.removeEventListener('resize', updateWindowWidth))
</script>

<style scoped>
.login-page-container {
  display: flex;
  height: 100vh;
}

.el-card-wrapper {
  background-image: url('../assets/LoginPage/BackgroundImage.png');
  background-repeat: no-repeat;
  background-position: center;
  background-size: cover;
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}

.el-card-style {
  width: 550px;
  margin-top: 16px;
  margin-bottom: 30px;
  background: rgba(255,255,255,0.6);
  margin-left: auto;
  margin-right: 15vw;
  box-shadow: 0px 0px 4px rgba(0, 0, 0, 0.6);
}

h1 {
  text-align: center;
  color: #0084ff;
}

label {
  font-weight: 600;
  font-size: 15px;
  color: #0084ff;
}

.el-link-wrapper {
  display: flex;
  justify-content: center;
}

.btn-login {
  margin-top: 30px;
  margin-bottom: 30px;
  width: 100%;
}
</style>
