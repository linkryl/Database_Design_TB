<template>
  <div class='reset-page-container'>
    <div class='el-card-wrapper'>
      <el-card class='el-card-style' shadow='always'>
        <h1>重置密码</h1>
        <el-form ref='formRef' :model='form' :rules='rules' label-width='0'>
          <el-form-item prop='userName'>
            <label for='input-username'>用户名</label>
            <el-input id='input-username' v-model='form.userName' placeholder='请输入用户名' clearable />
          </el-form-item>
          <el-form-item prop='birthdate'>
            <label for='input-birthdate'>出生日期</label>
            <el-date-picker id='input-birthdate' v-model='form.birthdate' type='date' 
            placeholder="请选择出生日期" :disabled-date="disableFutureDates"
              format='YYYY-MM-DD' value-format='YYYY-MM-DD' />
          </el-form-item>
          <el-form-item prop='newPlainPassword'>
            <label for='input-password'>新密码</label>
            <el-input id='input-password' v-model='form.newPlainPassword' type='password' placeholder='请输入新密码'
              show-password />
          </el-form-item>
          <el-button type='primary' class='btn-reset' @click='onSubmit'>提交</el-button>
          <el-button class='btn-back' @click="router.push('/login')">返回登录</el-button>
        </el-form>
      </el-card>
    </div>
  </div>
</template>

<script setup lang='ts'>
import { reactive, ref } from 'vue'
import { ElMessage, FormInstance, FormRules } from 'element-plus'
import { useRouter } from 'vue-router'
import { resetPassword, THResetPasswordRequest } from '../api/index'

const router = useRouter()
const formRef = ref<FormInstance>()
const form = reactive<THResetPasswordRequest>({
  userName: '',
  birthdate: '',
  newPlainPassword: ''
})

// 表单规则
const rules: FormRules = {
  userName: [{ required: true, message: '用户名不能为空', trigger: 'blur' }],
  birthdate: [{ required: true, message: '出生日期不能为空', trigger: 'change' }],
  newPlainPassword: [{ required: true, message: '新密码不能为空', trigger: 'blur' }]
}

// 禁用未来日期选择
const disableFutureDates = (time: Date) => {
  return time.getTime() > Date.now();
};

// 提交数据
const onSubmit = async () => {
  try {
    const valid = await formRef.value?.validate()
    if (!valid) return
    await resetPassword(form)
    ElMessage.success('密码重置成功，请使用新密码登录')
    router.push('/login')
  } catch (err: any) {
    const msg = err?.response?.data?.message || '重置失败，请检查用户名与出生日期'
    ElMessage.error(msg)
  }
}
</script>

<style scoped>
.reset-page-container {
  display: flex;
  height: 100vh;
}

.el-card-wrapper {
  background-image: url('/images/BackgroundImage.png');
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
  background: rgba(255, 255, 255, 0.6);
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

.btn-reset {
  width: 100%;
  margin-top: 12px;
}

.btn-back {
  width: 100%;
  margin-top: 8px;
  margin-left: 0px;
}
</style>
