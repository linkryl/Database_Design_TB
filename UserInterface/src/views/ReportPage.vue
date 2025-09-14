<!-- filepath: d:\DB\Database_Design_TB\UserInterface\src\views\ReportPage.vue -->
<template>
  <div class="report-page">
    <h2>举报帖子</h2>
    <form @submit.prevent="submitReport">
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
        <textarea v-model="detail" id="detail" rows="4" placeholder="请补充举报说明（选填）"/>
      </div>
      <button type="submit" class="submit-btn">提交举报</button>
    </form>
    <button @click="$emit('close')" class="close-btn">关闭</button>
    <div v-if="message" class="msg">{{ message }}</div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'

const route = useRoute()
const postId = Number(route.params.postId)
const reason = ref('')
const detail = ref('')
const message = ref('')

const submitReport = async () => {
  try {
    await axios.post('/api/post-report', {
      postId: postId,
      reportReason: reason.value,
      reportDetail: detail.value
    })
    message.value = '举报成功，感谢您的反馈！'
    reason.value = ''
    detail.value = ''
  } catch (err) {
    message.value = '举报失败，请稍后重试1。'
  }
}
</script>

<style scoped>
.report-page {
  max-width: 340px;
  margin: 40px auto;
  padding: 24px 20px;
  border: 1px solid #e0e0e0;
  border-radius: 10px;
  background: #fff;
  box-shadow: 0 2px 8px #eee;
}
.report-page h2 {
  margin-bottom: 18px;
  font-size: 20px;
  color: #333;
  text-align: center;
}
.report-page form > div {
  margin-bottom: 14px;
}
.report-page label {
  display: block;
  margin-bottom: 6px;
  font-size: 15px;
  color: #555;
}
.report-page select,
.report-page textarea {
  width: 100%;
  padding: 7px;
  font-size: 15px;
  border: 1px solid #ccc;
  border-radius: 5px;
  box-sizing: border-box;
}
.report-page textarea {
  resize: vertical;
  min-height: 60px;
}
.submit-btn {
  width: 100%;
  padding: 9px 0;
  background: #409eff;
  color: #fff;
  border: none;
  border-radius: 5px;
  font-size: 16px;
  margin-top: 8px;
  cursor: pointer;
  transition: background 0.2s;
}
.submit-btn:hover {
  background: #3076c9;
}
.close-btn {
  width: 100%;
  padding: 8px 0;
  background: #f5f5f5;
  color: #888;
  border: none;
  border-radius: 5px;
  font-size: 15px;
  margin-top: 10px;
  cursor: pointer;
  transition: background 0.2s;
}
.close-btn:hover {
  background: #e0e0e0;
}
.msg {
  margin-top: 16px;
  color: #409eff;
  text-align: center;
  font-size: 15px;
}
</style>