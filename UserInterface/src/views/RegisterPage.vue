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
              <el-form
                ref="stepOneFormRef"
                :model="stepOneFormData"
                :rules="formValidationRules"
                class="form-body"
              >
                <el-form-item prop="account">
                  <!-- 账号输入 -->
                    <el-input
                      v-model="stepOneFormData.account"
                      type="text"
                      size="large"
                      :prefix-icon="User"
                      placeholder="请输入7位账号"
                      autocomplete="off"
                      @input="validateAccountLength"
                      show-word-limit
                      maxlength="7"
                    />
                </el-form-item>

                 <!-- 密码输入 -->
                <el-form-item prop="password">
                  <el-tooltip
                    :visible="stepOneFormData.password !== ''"
                    :content="getPasswordStrengthText()"
                    placement="right"
                    raw-content
                  >
                    <el-input
                      v-model="stepOneFormData.password"
                      type="password"
                      size="large"
                      :prefix-icon="Unlock"
                      placeholder="请输入密码"
                      autocomplete="off"
                      @input="calculatePasswordStrength"
                      show-password
                    />
                  </el-tooltip>
                </el-form-item>

                 <!-- 密码确认 -->
                <el-form-item prop="passwordConfirm">
                  <el-input
                    v-model="stepOneFormData.passwordConfirm"
                    type="password"
                    size="large"
                    :prefix-icon="Lock"
                    placeholder="请确认密码"
                    autocomplete="off"
                    show-password
                  />
                </el-form-item>

                <div class="action-buttons">
                  <el-button
                    type="primary"
                    size="large"
                    @click="proceedToNextStep(stepOneFormRef)"
                    style="width: 100%; margin-bottom: 12px;"
                  >
                    下一步
                  </el-button>
                  <!-- 跳过按钮 -->
                  <el-button 
                    type="primary"
                    size="large"
                    @click="currentStep++"
                    style="width: 100%; margin-bottom: 12px; margin-left: 1px;"
                  >
                    跳过
                  </el-button>
                </div>
              </el-form>
            </div>

            <!-- 第二步：个人信息 -->
            <div v-else class="step-content">
              <el-form
                ref="stepTwoFormRef"
                :model="stepTwoFormData"
                :rules="formValidationRules"
                class="form-body"
              >
                <el-form-item prop="username">
                  <el-input
                    v-model="stepTwoFormData.username"
                    size="large"
                    :prefix-icon="Edit"
                    :disabled="currentStep === 2"
                    placeholder="请输入用户昵称（注册后不可修改）"
                    autocomplete="off"
                    clearable
                  />
                </el-form-item>

                <div class="form-row">
                  <el-form-item prop="gender" class="form-half-item">
                    <el-select
                      v-model="stepTwoFormData.gender"
                      :disabled="currentStep === 2"
                      placeholder="请选择性别"
                      size="large"
                      style="width: 240px"
                    >
                      <el-option label="男性" value="Male" />
                      <el-option label="女性" value="Female" />
                    </el-select>
                  </el-form-item>

                  <el-form-item prop="birthdate" class="form-half-item">
                    <el-date-picker
                      v-model="stepTwoFormData.birthdate"
                      :disabled="currentStep === 2"
                      type="date"
                      placeholder="请选择出生日期"
                      :disabled-date="disableFutureDates"
                      style="width: 100%"
                      size="large"
                      :editable="false"
                      :clearable="false"
                    />
                  </el-form-item>
                </div>

                <el-button
                  v-if="currentStep === 1"
                  type="primary"
                  size="large"
                  @click="completeRegistration(stepTwoFormRef)"
                  style="width: 100%; margin-bottom: 12px;"
                >
                  加入 TreeHole
                </el-button>

                <el-button
                  v-if="currentStep === 2"
                  type="primary"
                  size="large"
                  @click="navigateToLogin"
                  style="width: 100%; margin-bottom: 16px;"
                >
                  现在前往登录
                </el-button>
              </el-form>
            </div>
            
            <!-- 登录链接 -->
            <el-link
              style="display: block; text-align: center; margin-top: 5px;"
              type="primary"
              :underline="false"
              @click="navigateToLogin"
            >
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
import { Lock, Unlock, Edit, User } from "@element-plus/icons-vue";

// 路由管理
const router = useRouter();

// 当前步骤状态
const currentStep = ref(0);

// 表单引用
const stepOneFormRef = ref<FormInstance>();
const stepTwoFormRef = ref<FormInstance>();

// 账号长度提示
const accountLengthMessage = ref("");

// 密码强度相关
const passwordStrengthLevel = ref("low");
const passwordStrengthMessage = ref("");

// 表单数据
const stepOneFormData = reactive({
  account: "",
  password: "",
  passwordConfirm: "",
});

const stepTwoFormData = reactive({
  username: "",
  gender: "",
  birthdate: "",
});

// 表单验证规则
const formValidationRules: FormRules = {
  account: [{ required: true, message: "请输入7位账号", trigger: "blur" }],
  password: [{ required: true, message: "请输入密码", trigger: "blur" }],
  passwordConfirm: [{ required: true, message: "请确认密码", trigger: "blur" }],
  username: [{ required: true, message: "请输入昵称", trigger: "blur" }],
  gender: [{ required: true, message: "请选择性别", trigger: "change" }],
  birthdate: [{ required: true, message: "请选择出生日期", trigger: "change" }],
};

// 禁用未来日期选择
const disableFutureDates = (time: Date) => {
  return time.getTime() > Date.now();
};

// 步骤提交处理
const proceedToNextStep = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
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

// 提交注册数据到API
async function submitRegistrationData() {
  try {
    const response = await axiosInstance.post("user", {
      username: stepTwoFormData.username,
      password: sha256(stepOneFormData.password),
      account: stepOneFormData.account,
    });
    return response.status === 201;
  } catch {
    return false;
  }
}

// 账号长度验证
function validateAccountLength() {
  const account = stepOneFormData.account;
  if(account.length < 7){
    accountLengthMessage.value = "请输入7位账号";
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
  margin-left: 500px;
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
  color: #303133;
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