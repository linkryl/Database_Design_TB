<!--
  Project Name:  UserInterface
  File Name:     RegisterPage.vue
  File Function: 用户注册页面
  Author:        2351131 韦世贸
-->

<template>
  <div class="register-container">
    <!-- 表单区域 -->
    <div class="form-section">
      <div class="form-wrapper">
        <el-card class="form-card" shadow="hover">
          <!-- 社区标识 -->
          <div class="society-logo">
            <img src='../assets/LogosAndIcons/TreeHoleLogo.png' alt="TreeHole Logo" />
          </div>

          <!-- 页面标题与步骤指示器 -->
          <h1 class="page-title">注册账号</h1>
          <el-steps :active="currentStep" finish-status="success" align-center>
            <el-step title="账号注册" />
            <el-step title="完善信息" />
          </el-steps>

          <!-- 表单内容区域 -->
          <div class="form-content-wrapper">

            <!-- 第一步：账号信息 -->
            <div v-if="currentStep === 0" class="step-content">
              <el-form ref="stepOneFormRef" :model="stepOneFormData" :rules="formValidationRules" class="form-body">
                <el-form-item prop="username">
                  <!-- 用户名输入 -->
                  <el-input v-model="stepOneFormData.username" type="text" size="large" :prefix-icon="User"
                    placeholder="请输入用户名" autocomplete="off" />
                </el-form-item>

                <!-- 密码输入 -->
                <el-form-item prop="password">
                  <el-tooltip :visible="stepOneFormData.password !== ''" :content="getPasswordStrengthText()"
                    placement="right" raw-content>
                    <el-input v-model="stepOneFormData.password" type="password" size="large" :prefix-icon="Unlock"
                      placeholder="请输入密码" autocomplete="off" @input="calculatePasswordStrength" show-password />
                  </el-tooltip>
                </el-form-item>

                <!-- 密码确认 -->
                <el-form-item prop="passwordConfirm">
                  <el-tooltip :visible="stepOneFormData.password !== ''" :content="getPasswordMatchText()"
                    placement="right" raw-content>
                    <el-input v-model="stepOneFormData.passwordConfirm" type="password" size="large" :prefix-icon="Lock"
                      placeholder="请确认密码" autocomplete="off" show-password @input="checkPasswordMatch" />
                  </el-tooltip>
                </el-form-item>

                <div class="action-buttons">
                  <el-button type="primary" size="large" @click="proceedToNextStep(stepOneFormRef)"
                    style="width: 100%; margin-bottom: 12px;">
                    下一步
                  </el-button>
                  <!-- 跳过按钮 -->
                  <!--<el-button type="primary" size="large" @click="currentStep++"
                    style="width: 100%; margin-bottom: 12px; margin-left: 1px;">
                    跳过
                  </el-button> -->
                </div>
              </el-form>
            </div>

            <!-- 第二步：个人信息 -->
            <div v-else class="step-content">
              <el-form ref="stepTwoFormRef" :model="stepTwoFormData" :rules="formValidationRules" class="form-body">

                <div class="form-row">
                  <el-form-item prop="gender" class="form-half-item">
                    <el-select v-model="stepTwoFormData.gender" :disabled="currentStep === 2" placeholder="请选择性别"
                      size="large" style="width: 240px">
                      <el-option label="男性" value="Male" />
                      <el-option label="女性" value="Female" />
                    </el-select>
                  </el-form-item>

                  <el-form-item prop="birthdate" class="form-half-item">
                    <el-date-picker v-model="stepTwoFormData.birthdate" :disabled="currentStep === 2" type="date"
                      placeholder="请选择出生日期" :disabled-date="disableFutureDates" style="width: 100%" size="large"
                      :editable="false" :clearable="false" />
                  </el-form-item>
                </div>

                <el-button v-if="currentStep === 1" type="primary" size="large"
                  @click="completeRegistration(stepTwoFormRef)" style="width: 100%; margin-bottom: 12px;">
                  加入 TreeHole
                </el-button>

                <el-button v-if="currentStep === 2" type="primary" size="large" @click="navigateToLogin"
                  style="width: 100%; margin-bottom: 16px;">
                  现在前往登录
                </el-button>
              </el-form>
            </div>

            <!-- 登录链接 -->
            <el-link style="display: block; text-align: center; margin-top: 5px;" type="primary" :underline="false"
              @click="navigateToLogin">
              已有账号？点击这里登录
            </el-link>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router";
import { onMounted, onUnmounted, reactive, ref } from "vue";
import { ElNotification, FormInstance, FormRules } from "element-plus";
import axiosInstance from "../utils/axios";
import { sha256 } from "js-sha256";
import { Lock, Unlock, User } from "@element-plus/icons-vue";
import axios from "axios";

// 路由管理
const router = useRouter();

// 当前步骤状态
const currentStep = ref(0);

// 表单引用
const stepOneFormRef = ref<FormInstance>();
const stepTwoFormRef = ref<FormInstance>();

// 密码相关
const passwordsMatch = ref(false);
const passwordsMatchMessage = ref("密码不匹配")
const passwordStrengthLevel = ref("low");
const passwordStrengthMessage = ref("");

// 表单数据
const stepOneFormData = reactive({
  username: "",
  password: "",
  passwordConfirm: "",
});

const stepTwoFormData = reactive({
  gender: "",
  birthdate: "",
});

// 表单验证规则
const formValidationRules: FormRules = {
  password: [{ required: true, message: "请输入密码", trigger: "blur" }],
  passwordConfirm: [{ required: true, message: "请确认密码", trigger: "blur" }],
  username: [{ required: true, message: "请输入用户名", trigger: "blur" }],
  gender: [{ required: true, message: "请选择性别", trigger: "change" }],
  birthdate: [{ required: true, message: "请选择出生日期", trigger: "change" }],
};

// 禁用未来日期选择
const disableFutureDates = (time: Date) => {
  return time.getTime() > Date.now();
};

// 密码一致性检查
const checkPasswordMatch = () => {
  passwordsMatch.value = stepOneFormData.password === stepOneFormData.passwordConfirm;
};

function getPasswordMatchText() {
  if (passwordsMatch.value) {
    passwordsMatchMessage.value = "密码匹配"
  }

  return passwordsMatchMessage.value
}

// 步骤提交处理
const proceedToNextStep = async (formEl: FormInstance | undefined) => {
  if (!formEl || !passwordsMatch.value) return;
  await formEl.validate((valid) => {
    if (valid) currentStep.value++;
  });
};

const completeRegistration = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate(async (valid) => {
    if (valid) {
      const isSuccess = await submitRegistrationData();
      if (isSuccess) {
        currentStep.value++;
        ElNotification({
          title: "注册成功",
          message: "注册账号成功，欢迎加入 TreeHole！",
          type: "success",
        });
      } else {
        ElNotification({
          title: "注册失败",
          message: "请检查网络连接情况或稍后重试。",
          type: "error",
        });
      }
    }
  });
};

// 东八区日期时间转换函数
const convertToEast8ISOString = (date: string | Date): string => {
  try {
    // 确保输入是Date对象
    const dateObj = date instanceof Date ? date : new Date(date);
    
    // 处理无效日期
    if (isNaN(dateObj.getTime())) {
      return ''; // 或者抛出错误
    }
    
    // 获取东八区时间（UTC+8）
    const offset = 8; // 东八区偏移量
    const utc = dateObj.getTime() + (dateObj.getTimezoneOffset() * 60000);
    // *7200000转换后强制成为东八区时间
    const east8Time = new Date(utc + (7200000 * offset));
    console.log('btime:', east8Time);
    // 转换为ISO字符串
    return east8Time.toISOString();
  } catch (error) {
    console.error('Date conversion error:', error);
    return ''; // 返回默认值
  }
};

// 提交注册数据到API
async function submitRegistrationData() {
  // 格式化日期
  const formattedDate = convertToEast8ISOString(stepTwoFormData.birthdate);
  
  // 当前时间(0时区时间)
  const now = new Date().toISOString();

  // 
  const newUserData = {
    userName: stepOneFormData.username,
    password: sha256(stepOneFormData.password),
    registrationDate: now,
    lastLoginTime: now,
    role: 0,
    status: 0,
    avatarUrl: "",
    profile: "",
    gender: (stepTwoFormData.gender == 'Male' ? 0 : 1),
    birthdate: formattedDate,
    experiencePoints: 0,
    followsCount: 0,
    followedCount: 0,
    favoritesCount: 0,
    favoritedCount: 0,
    likesCount: 0,
    likedCount: 0,
    messageCount: 0,
  }

  try {
    console.log('发送注册数据:', newUserData);

    const response = await axiosInstance.post('user', newUserData);

    return response.status == 201
  } catch (error) {
    console.error('注册请求错误:', error);

    let errorMessage = '注册失败，请稍后重试';

    if (axios.isAxiosError(error)) {
      if (error.response) {
        // 服务器响应了错误状态码
        if (error.response.data && (error.response.data as any).message) {
          errorMessage = (error.response.data as any).message;
        } else if (error.response.status === 400) {
          errorMessage = '请求无效';
        } else if (error.response.status >= 500) {
          errorMessage = '服务器内部错误，请稍后重试';
        }
      } else if (error.request) {
        errorMessage = '网络错误，请检查网络连接';
      }
    } else {
      // 处理非 Axios 错误
      errorMessage = '发生未知错误，请稍后重试';
      console.error('非 Axios 错误:', error);
    }
    ElNotification({
      title: '注册失败',
      message: errorMessage,
      type: 'error',
      duration: 5000
    });

    return false;
  }
}

// 密码强度计算
function calculatePasswordStrength() {
  const password = stepOneFormData.password;
  const hasUpper = /[A-Z]/.test(password);
  const hasLower = /[a-z]/.test(password);
  const hasNumber = /[0-9]/.test(password);
  if (password.length < 8 || !hasUpper || !hasLower || !hasNumber) {
    passwordStrengthLevel.value = "low";
    passwordStrengthMessage.value = "您的密码强度为：低";
  } else if (password.length <= 16) {
    passwordStrengthLevel.value = "medium";
    passwordStrengthMessage.value = "您的密码强度为：中";
  } else {
    passwordStrengthLevel.value = "high";
    passwordStrengthMessage.value = "您的密码强度为：高";
  }
}

// 获取密码强度文本
function getPasswordStrengthText() {
  return passwordStrengthMessage.value;
}

// 导航到登录页面
function navigateToLogin() {
  router.push('/login');
}

// 响应式布局处理
const windowWidth = ref(window.innerWidth);
function updateWindowWidth() {
  windowWidth.value = window.innerWidth;
}
onMounted(() => window.addEventListener("resize", updateWindowWidth));
onUnmounted(() => window.removeEventListener("resize", updateWindowWidth));
</script>

<style scoped>
.register-container {
  display: flex;
  background: url('../assets/BackgroundImages/L&RBackgroundImage.png') no-repeat center center fixed;
  background-size: cover;
  justify-content: center;
  align-items: center;
  height: 100vh;
}

.form-section {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  margin-left: 600px;
  opacity: 0.8;
}

.form-wrapper {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
}

.form-card {
  width: 100%;
  max-width: 420px;
  padding: 24px;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
}

.society-logo {
  text-align: center;
  margin-bottom: 6px;
}

.society-logo img {
  max-width: 200px;
}

.page-title {
  text-align: center;
  margin-bottom: 20px;
  color: #0084ff;
}

.form-content-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.step-content {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.action-buttons {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-top: 10px;
}

.form-body {
  margin-top: 20px;
}

.form-row {
  display: flex;
  width: 100%;
  gap: 10px;
}

.form-half-item {
  flex: 1;
}

/* 响应式调整 */
@media (max-width: 768px) {
  .register-container {
    padding: 20px;
    height: auto;
    min-height: 100vh;
  }

  .form-card {
    margin: 20px 0;
    max-width: 100%;
  }

  .form-row {
    flex-direction: column;
    gap: 0;
  }
}
</style>