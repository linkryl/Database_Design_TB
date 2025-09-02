<template>
  <div class="login-page-container">
    <div class="login-box">
      <h1>登录 TreeHole</h1>

      <form @submit.prevent="handleSubmit">
        <label for="username">用户名</label>
        <input id="username"
                v-model="myform.username"
                type="text"
                autocomplete="username"
        />

        <div class="psw-help">
          <label for="password">密码</label>
          <a href="javascript:void(0)" class="forgot">忘记密码?</a>
        </div>
        <input
           id="password"
           v-model="myform.password"
           type="password"
           autocomplete="current-password"
        />

        <button type="submit" v-bind:disabled="loading||!myform.username||!myform.password">
          {{loading ? "登录中...":"登 录"}}
        </button>
      </form>

      <p class="new-account">
        没有帐号？
        <router-link to="/register">点击创建账号</router-link>
        <br />
        <br />
        <router-link to="/" class="back-home">{{home}}</router-link>
      </p>

    </div>
  </div>
</template>

<script>
  export default {
    data() {
      return {
        home: "返回首页",
        loading: false,
        myform: {
          username: "",
          password: ""
        }
      }
    },
    methods: {
      handleSubmit() {
        this.loading = true
        //暂时模拟登录1000ms后loading改为false
        setTimeout(() => {
          //alert(`登录成功，你好 ${this.myform.username}！`)
          this.loading = false
        },1000)
      }
    }
  }
</script>

<style scoped>
  .login-page-container {
    display: flex;
    flex-direction: column;
    align-items:center;
    justify-content: center;
    min-height: 97vh;
    background: #f6f8fa;
    font-family: Arial;
  }

  .login-box{
      box-sizing:border-box;
      width:100%;
      max-width:500px;
      padding:25px;
      background:#ffffff;
      border:1px solid #d0d7de;
      border-radius:6px;
  }

  h1{
      font-size:32px;
      font-weight:500;
      text-align:center;
      margin:16px 0 16px 0;
  }

  label{
      display:block;
      margin:10px 0 0 0;
      font-size:18px;
      font-weight:600;
  }

  input{
      box-sizing:border-box;
      width:100%;
      padding:10px 16px 10px 16px;
      margin:0 0 20px 0;
      font-size:20px;
      font-weight:500;
      border:3px solid #d0d7de;
      border-radius:6px;
  }

  input:focus{
      border-color:#0969da;
      outline:none;
      box-shadow:0 0 0 0 rgba(9,105,218,0.3);
  }

  .psw-help{
      display:flex;
      justify-content:space-between;
      align-items:center;
  }

  .forgot{
      display:inline;
      font-size:18px;
      font-weight:600;
      color:#0969da;
      text-decoration:none;
  }

  .forgot:hover{
      text-decoration:underline;
  }

  button{
      width:100%;
      padding:10px 16px 10px 16px;
      margin:20px 0 20px 0;
      font-size:18px;
      font-weight:500;
      color:#ffffff;
      background:#3f89ec;
      border:1px solid rgba(27,31,36,0.3);
      border-radius:6px;
      cursor:pointer
  }

  button:hover:not(:disabled) {
    background: #579dfa;
  }

  button:disabled{
      opacity:0.6;
      cursor:not-allowed;
  }

  .new-account{
      margin:16px 0 16px 0;
      padding:16px 0 16px 0;
      font-size:20px;
      text-align:center;
  }

  .new-account a{
      color:#0969da;
      text-decoration:none;
  }

  .new-account a:hover{
      text-decoration:underline;
  }

  .back-home {
      font-size:20px;
      color:#0969da;
      text-decoration:none;
  }

  .back-home:hover{
      text-decoration:underline;
  }
</style>
