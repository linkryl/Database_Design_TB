<template>
  <transition name="fade">
    <div v-if="modelValue" class="mask">
      <div class="dialog" @click.stop>
        <div class="header">
          <span class="title">获取金币</span>
          <button class="close" @click="handleClose" v-bind:disabled="loading">✕</button>
        </div>

        <div class="footer">
          <button class="buy" @click="handleBuy" v-bind:disabled="loading">
            {{loading ? "购买中..." : "购买"}}
          </button>
          <button class="cancle" @click="handleClose" v-bind:disabled="loading">取消</button>
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
        loading: false
      }
    },
    methods: {
      handleClose() {
        this.$emit("update:modelValue", false);
      },
      handleBuy() {
        this.loading = true;
        setTimeout(() => {
          this.loading = false;
          alert("购买成功");
          this.handleClose();
        }, 1000);
      }
    }
  }
</script>

<style scoped>
  .mask {
    position: absolute;
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
    border:#ffffff;
    background:#ffffff;
  }

  .close[disabled]{
    cursor:not-allowed;
    opacity:0.6;
  }

  .footer {
    display: flex;
    justify-content: center;
    gap: 24px;
  }

  .buy {
    width:80px;
    background: #f20404;
    color: #ffffff;
    font-size:14px;
    font-weight:600;
    border: none;
    padding: 4px 8px 4px 8px;
    border-radius: 4px;
    cursor: pointer;
  }

  .buy[disabled]{
    opacity:0.6;
    cursor:not-allowed;
  }

  .cancle {
    width: 80px;
    background: #ffffff;
    color: #333333;
    font-size: 14px;
    font-weight: 600;
    border: 1px solid #d9d9d9;
    padding: 3px 7px 3px 7px;
    border-radius: 4px;
    cursor: pointer;
  }

  .cancle[disabled]{
    opacity:0.6;
    cursor:not-allowed;
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
