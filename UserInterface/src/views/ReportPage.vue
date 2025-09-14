<!-- filepath: d:\DB\Database_Design_TB\UserInterface\src\views\ReportPage.vue -->
<template>
  <div class="report-page">
    <h2>举报帖子</h2>
    <form @submit.prevent="submitReport">
      <div>
        <label for="postId">帖子ID：</label>
        <input v-model="postId" id="postId" type="number" required />
      </div>
      <div>
        <label for="reason">举报原因：</label>
        <select v-model="reason" id="reason" required>
          <option value="">请选择原因</option>
          <option value="广告">广告</option>
          <option value="不良信息">不良信息</option>
          <option value="骚扰">骚扰</option>
          <option value="其他">其他</option>
        </select>
      </div>
      <div>
        <label for="detail">详细说明：</label>
        <textarea v-model="detail" id="detail" rows="4" />
      </div>
      <button type="submit">提交举报</button>
    </form>
    <button @click="$emit('close')" style="margin-top:12px;">关闭</button>
    <div v-if="message" class="msg">{{ message }}</div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'

const postId = ref<number | null>(null)
const reason = ref('')
const detail = ref('')
const message = ref('')

const submitReport = async () => {
  try {
    await axios.post('/api/post-report', {
      postId: postId.value,
      reportReason: reason.value,
      reportDetail: detail.value
    })
    message.value = '举报成功，感谢您的反馈！'
    postId.value = null
    reason.value = ''
    detail.value = ''
  } catch (err) {
    message.value = '举报失败，请稍后重试。'
  }
}
</script>

<style scoped>
.report-page {
  max-width: 400px;
  margin: 40px auto;
  padding: 24px;
  border: 1px solid #eee;
  border-radius: 8px;
  background: #fafafa;
}
.report-page h2 {
  margin-bottom: 16px;
}
.report-page form > div {
  margin-bottom: 12px;
}
.report-page label {
  display: block;
  margin-bottom: 4px;
}
.report-page input,
.report-page select,
.report-page textarea {
  width: 100%;
  padding: 6px;
  box-sizing: border-box;
}
.report-page button {
  padding: 8px 16px;
}
.msg {
  margin-top: 16px;
  color: #409eff;
}
</style>