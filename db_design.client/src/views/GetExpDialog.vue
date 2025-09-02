<template>
  <transition name="fade">
    <div v-if="modelValue" class="mask">
      <div class="dialog" @click.stop>
        <div class="header">
          <span class="title">获取经验</span>
          <span class="close" @click="handleClose">✕</span>
        </div>

        <main class="main-context">
          <p>您在 TreeHole 获取的经验值可以用来提升等级...</p>
          <p>以下方式可以帮助您获取经验值：</p>
          <h3>签到说明</h3>
          <table>
            <thead>
              <tr>
                <th>连续签到</th>
                <th>经验值</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="row in signRows" v-bind:key="row.day">
                <td>{{row.day}} 天</td>
                <td>+ {{row.exp}}</td>
              </tr>
            </tbody>
          </table>
          <h3>积分说明...</h3>
          <h3>优质帖子...</h3>
        </main>

        <div class="footer">
          <button class="confirm" @click="handleClose">我已知晓</button>
        </div>
      </div>
    </div>
  </transition>
</template>

<script>
  export default {
    props: {
      modelValue: Boolean
    },
    emits: [
      "update:modelValue"
    ],
    data() {
      return {
        signRows: [
          { day: 1, exp: 2 },
          { day: 2, exp: 2 },
          { day: 10, exp: 4 },
          { day: 20, exp: 4 },
          { day: 30, exp: 6 },
        ]
      }
    },
    methods: {
      handleClose() {
        this.$emit("update:modelValue", false);
      }
    }
  }
</script>

<style scoped>
  .mask {
    position:absolute;
    inset: 0;
    background: rgba(0,0,0,0.45);
    display: flex;
    justify-content: center;
    align-items: start;
    z-index: 9999;
  }

  .dialog {
    margin: 160px 0 0 0;
    width: 680px;
    background: #ffffff;
    border-radius: 4px;
    padding: 0 8px 4px 8px;
    box-shadow: 0 4px 24px rgba(0,0,0,0.15);
  }

  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin: 0 0 16px 0;
  }

  .title {
    font-size: 16px;
    font-weight: 500;
  }

  .close {
    cursor: pointer;
    font-size: 18px;
    color: #666666;
  }

  .main-context {
    display: block;
    padding: 12px 16px 12px 16px;
    color: #000000;
    height: 360px;
    overflow-y: auto;
  }

  p{
      font-size:16px;
      font-weight:400;
      margin:4px 0 4px 0;
  }

  h3{
      font-size:18px;
      font-weight:500;
      margin:8px 0 4px 0;
  }

  table{
    width:100%;
    box-sizing:border-box;
    border-collapse:collapse;
    font-size:14px;
  }

  table th,
  table td{
      border:1px solid #e5e5e5;
      padding:8px;
      text-align:center;
  }

  table th{
      background:#e7e7e7;
      font-weight:500;
  }

  .footer {
    display: flex;
    justify-content: center;
    gap: 24px;
  }

  .confirm {
    background: #1890ff;
    color: #ffffff;
    border: none;
    padding: 6px 18px 6px 18px;
    border-radius: 4px;
    cursor: pointer;
  }

  .fade-enter-active,
  .fade-leave-active {
    transition: opacity .2s;
  }

  .fade-enter,
  .fade-leave-to {
    opacity: 0;
  }
</style>
