<template>
  <div>这是首页Home</div>
  <router-link to="/login" class="login-btn">{{login}}</router-link>
  <router-link to="/register" class="register-btn">{{register}}</router-link>
  <router-link to="/ucenter" class="ucenter-btn">{{ucenter}}</router-link>
</template>

<script>
  export default {
    data() {
      return {
        login: "登录按钮",
        register: "注册按钮",
        ucenter:"用户中心"
      }
    }
  }
</script>

<style scoped>
  .login-btn,
  .register-btn,
  .ucenter-btn {
    display: inline-block;
    padding: 5px 5px;
    margin: 5px;
    border: 4px solid #00ffff;
    color: #000000;
    background: #ffd800;
    text-decoration: none;
    border-radius: 4px;
    cursor: pointer;
  }

  .login-btn:hover,
  .register-btn:hover,
  .ucenter-btn:hover {
    background: #b6ff00;
  }
</style>
