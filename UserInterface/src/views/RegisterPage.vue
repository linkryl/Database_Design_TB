<!--
  Project Name:  UserInterface
  File Name:     RegisterPage.vue
  File Function: 注册页面
  Author:        2351131 韦世贸
-->

<template>
  <div class="container">
    <!-- 表单 -->
    <div class="left">
      <div class="form-container">
        <el-card class="form-card" shadow="hover">
          <!-- 插画部分 -->
          <div class="form-image"> 
            <img :src='`${ossBaseUrl}LogosAndIcons/TreeHoleLogo.png`' alt='TreeHoleTitle'>
          </div>

          <!-- 步骤条 -->
          <h1 class="form-title">注册账号</h1>
          <el-steps :active="activeStep" finish-status="success" align-center>
            <el-step title="账号注册" />
            <el-step title="完善信息" />
          </el-steps>

          <!-- 表单内容容器 - 添加固定高度和滚动 -->
          <div class="form-content-container">
            
            <!-- Step 1 账号密码 -->
            <div v-if="activeStep == 0" class="step-content">
              <el-form
                ref="formRefStep2"
                :model="formStep1"
                :rules="formRules"
                class="form-body"
              >
                <el-form-item prop="account">
                  <!-- 输入账号 -->
                    <el-input
                      v-model="formStep1.account"
                      type="account"
                      size="large"
                      :prefix-icon="User"
                      placeholder="请输入7位账号"
                      autocomplete="off"
                      @input="checkAccountLength"
                      show-account
                    />
                </el-form-item>

                 <!-- 输入密码 -->
                <el-form-item prop="password">
                  <el-tooltip
                    :visible="formStep1.password !== ''"
                    :content="passwordStrengthText()"
                    placement="right"
                    raw-content
                  >
                    <el-input
                      v-model="formStep1.password"
                      type="password"
                      size="large"
                      :prefix-icon="Unlock"
                      placeholder="请输入密码"
                      autocomplete="off"
                      @input="testPasswordStrength"
                      show-password
                    />
                  </el-tooltip>
                </el-form-item>

                 <!-- 确认密码 -->
                <el-form-item prop="passwordConfirm">
                  <el-input
                    v-model="formStep1.passwordConfirm"
                    type="password"
                    size="large"
                    :prefix-icon="Lock"
                    placeholder="请确认密码"
                    autocomplete="off"
                    show-password
                  />
                </el-form-item>

                <div class="button-group">
                  <el-button
                    type="primary"
                    size="large"
                    @click="submitStep(formRefStep1)"
                    style="width: 100%; margin-bottom: 12px;"
                  >
                    下一步
                  </el-button>
                  <!-- 跳过按钮 -->
                  <el-button 
                    type="primary"
                    size="large"
                    @click="activeStep++"
                    style="width: 100%; margin-bottom: 12px; margin-left: 1px;"
                  >
                    跳过
                  </el-button>
                </div>
              </el-form>
            </div>

            <!-- Step 2 完善信息 -->
            <div v-else class="step-content">
              <el-form
                ref="formRefStep3"
                :model="formStep2"
                :rules="formRules"
                class="form-body"
              >
                <el-form-item prop="username">
                  <el-input
                    v-model="formStep2.username"
                    size="large"
                    :prefix-icon="Edit"
                    :disabled="activeStep == 2"
                    placeholder="请输入用户昵称（注册后不可修改）"
                    autocomplete="off"
                    clearable
                  />
                </el-form-item>

                <div class="form-row">
                  <el-form-item prop="gender" class="form-half">
                    <el-select
                      v-model="formStep2.gender"
                      :disabled="activeStep == 2"
                      placeholder="请选择性别"
                      size="large"
                      style="width: 240px"
                    >
                      <el-option label="男性" value="Male" />
                      <el-option label="女性" value="Female" />
                    </el-select>
                  </el-form-item>

                  <el-form-item prop="birthdate" class="form-half">
                    <el-date-picker
                      v-model="formStep2.birthdate"
                      :disabled="activeStep == 2"
                      type="date"
                      placeholder="请选择出生日期"
                      :disabled-date="disabledDate"
                      style="width: 100%"
                      size="large"
                      :editable="false"
                      :clearable="false"
                    />
                  </el-form-item>
                </div>

                <el-button
                  v-if="activeStep == 1"
                  type="primary"
                  size="large"
                  @click="submitFinal(formRefStep3)"
                  style="width: 100%; margin-bottom: 12px;"
                >
                  加入 TreeHole
                </el-button>

                <el-button
                  v-if="activeStep == 2"
                  type="primary"
                  size="large"
                  @click="router.push('/login')"
                  style="width: 100%; margin-bottom: 16px;"
                >
                  现在前往登录
                </el-button>
              </el-form>
            </div>
            
            <!-- 将登录链接移到表单内容容器内部 -->
            <el-link
              style="display: block; text-align: center; margin-top: 5px;"
              type="primary"
              :underline="false"
              @click="router.push('/login')"
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
import { ossBaseUrl } from '../globals'

// 路由
const router = useRouter();

// 步骤状态
const activeStep = ref(0);

// 表单 ref
const formRefStep1 = ref<FormInstance>();
const formRefStep2 = ref<FormInstance>();
const formRefStep3 = ref<FormInstance>();

// 账号长度
const accountLengthPrompt = ref("");

// 密码强度
const passwordStrength = ref("low");
const passwordStrengthPrompt = ref("");

// 表单数据
const formStep1 = reactive({
  account: "",
  password: "",
  passwordConfirm: "",
});

const formStep2 = reactive({
  username: "",
  gender: "",
  birthdate: "",
});

// 规则
const formRules: FormRules = {
  account: [{ required: true, message: "请输入7位账号", trigger: "blur" }],
  password: [{ required: true, message: "请输入密码", trigger: "blur" }],
  passwordConfirm: [{ required: true, message: "请确认密码", trigger: "blur" }],
  username: [{ required: true, message: "请输入昵称", trigger: "blur" }],
  gender: [{ required: true, message: "请选择性别", trigger: "change" }],
  birthdate: [{ required: true, message: "请选择出生日期", trigger: "change" }],
};

// 模拟校验函数
const disabledDate = (time: Date) => {
  return time.getTime() > Date.now();
};

// 表单提交
const submitStep = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate((valid) => {
    if (valid) activeStep.value++;
  });
};

const submitFinal = async (formEl: FormInstance | undefined) => {
  if (!formEl) return;
  await formEl.validate(async (valid) => {
    if (valid) {
      const result = await postRegistration();
      if (result) {
        activeStep.value++;
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

// 模拟接口
async function postRegistration() {
  try {
    const response = await axiosInstance.post("user", {
      username: formStep2.username,
      password: sha256(formStep1.password),
      account: formStep1.account,
    });
    return response.status === 201;
  } catch {
    return false;
  }
}

// 账号长度检测
function checkAccountLength() {
  const account = formStep1.account;
  if(account.length < 7){
    accountLengthPrompt.value = "请输入7位账号";
  }
}

// 密码强度检测
function testPasswordStrength() {
  const password = formStep1.password;
  const hasUpper = /[A-Z]/.test(password);
  const hasLower = /[a-z]/.test(password);
  const hasNumber = /[0-9]/.test(password);
  if (password.length < 8 || !hasUpper || !hasLower || !hasNumber) {
    passwordStrength.value = "low";
    passwordStrengthPrompt.value = "您的密码强度为：低";
  } else if (password.length <= 16) {
    passwordStrength.value = "medium";
    passwordStrengthPrompt.value = "您的密码强度为：中";
  } else {
    passwordStrength.value = "high";
    passwordStrengthPrompt.value = "您的密码强度为：高";
  }
}

function passwordStrengthText() {
  return passwordStrengthPrompt.value;
}

// 自适应
const windowWidth = ref(window.innerWidth);
function updateWidth() {
  windowWidth.value = window.innerWidth;
}
onMounted(() => window.addEventListener("resize", updateWidth));
onUnmounted(() => window.removeEventListener("resize", updateWidth));
</script>

<style scoped>
.container {
  display: flex;
  background: url('../assets/BackgroundImages/L&RBackgroundImage.png') no-repeat center center fixed;
  background-size: 100% 100%;
  justify-content: center;
  height: calc(100vh - 135px);
}



.left {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
  margin-left: 500px;
}

.form-container {
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

.form-image {
  text-align: center;
  margin-bottom: 6px;
}

.form-image img {
  max-width: 200px;
}

.form-title {
  text-align: center;
  margin-bottom: 20px;
}

.form-content-container {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.step-content {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.button-group {
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
}

.form-half {
  flex: 1;
  margin-right: 10px;
}
.form-half:last-child {
  margin-right: 0;
}

.carousel-container {
  width: 100%;
}

.carousel-image {
  max-width: 100%;
}

/* 响应式调整 */
@media (max-width: 1200px) {
  .container {
    flex-direction: column;
    height: auto;
    min-height: calc(100vh - 135px);
  }
  
  .left {
    width: 100%;
    flex: none;
  }
  
  .form-card {
    margin: 20px 0;
  }
}
</style>
