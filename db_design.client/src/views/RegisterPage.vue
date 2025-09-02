<template>
  <div class="register-page-container">
    <div class="register-box">
      <h1>欢迎注册 TreeHole</h1>

      <p class="have-account">
        已有账号？
        <router-link to="/login" class="to-login">点击登录</router-link>
      </p>

      <form @submit.prevent="handleSubmit">
        <label for="username">
          用户名
        </label>
        <div class="input-wrapper">
          <input id="username"
                 type="text"
                 @focus="toShowTip.username=true"
                 @blur="toShowTip.username=false"
                 v-model="myform.username"
                 autocomplete="username"
          />
          <transition name="fade">
            <div v-if="toShowTip.username" class="tooltip">
              中英文均可，最长64个中文汉字或最长64个英文字母
            </div>
          </transition>
        </div>

        <label for="password">
          密码
        </label>
        <div class="input-wrapper">
          <input id="password"
                 type="password"
                 @focus="toShowTip.password=true"
                 @blur="toShowTip.password=false"
                 v-model="myform.password"
                 autocomplete="password"
          />
          <transition name="fade">
            <div v-if="toShowTip.password" class="tooltip">
              最短8个字符，最长16个字符，不允许出现中文和空格，其余ASCII可见字符均可
            </div>
          </transition>
        </div>

        <label for="phone">
          手机号
        </label>
        <div class="input-wrapper">
          <input id="phone"
                 type="tel"
                 @focus="toShowTip.phone=true"
                 @blur="toShowTip.phone=false"
                 v-model="myform.phone"
                 autocomplete="tel"
          />
          <transition name="fade">
            <div v-if="toShowTip.phone" class="tooltip">
              最长16个字符
            </div>
          </transition>
        </div>

        <button type="submit"
                v-bind:disabled="!agreed||loading||
                                 !myform.username||!myform.password||!myform.phone">
          {{loading ? "注册中..." : "注 册"}}
        </button>
      </form>

      <p class="statement">
        <input id="checkbox-input" type="checkbox" v-model="agreed" />
        <label for="checkbox-input">我已阅读并接受</label>
        <a href="https://privacy.baidu.com/policy/children-privacy-policy/index.html"
           class="back-home"
           target="_blank"
           rel="noopener noreferrer"
           >
          《儿童个人信息保护声明》
        </a>
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
        agreed:false,
        loading: false,
        toShowTip: {
          username: false,
          password: false,
          phone: false
        },
        myform: {
          username: "",
          password: "",
          phone: ""
        }
      }
    },
    methods: {
      handleSubmit() {
        if (!this.agreed || this.loading) {
          return
        }
        this.loading = true
        //暂时模拟注册1000ms后loading改为false
        setTimeout(() => {
          this.loading = false
        },1000)
      }
    }
  }
</script>

<style scoped>
  .register-page-container{
      display:flex;
      flex-direction:column;
      justify-content:center;
      min-height:97vh;
      background:#f6f8fa;
      font-family:Arial;
  }

  .register-box{
      box-sizing:border-box;
      width:100%;
      max-width:600px;
      padding:25px;
      margin:0 200px 0 auto;
      background:#ffffff;
      border:1px solid #d0d7de;
      border-radius:16px;
  }

  h1{
      font-size:32px;
      font-weight:500;
      text-align:left;
      margin:16px 0 4px 0;
  }

  .have-account{
      margin:4px 0 16px 0;
      font-size:18px;
      font-weight:400;
  }

  .to-login {
      color: #0969da;
      text-decoration: none;
  }

  .to-login:hover{
      text-decoration:underline;
  }

  form label{
      display:block;
      font-size:18px;
      margin:20px 0 0 0;
      font-weight:600;
  }

  .input-wrapper{
      position:relative;
  }

  form input{
      width:100%;
      box-sizing:border-box;
      padding:10px 16px 10px 16px;
      margin:0 0 20px 0;
      font-size:20px;
      font-weight:500;
      border:3px solid #d0d7de;
      border-radius:6px;
  }

  form input:focus{
      border-color:#0969da;
      outline:none;
      box-shadow:0 0 0 0 rgba(9,105,218,0.3);
  }
  .tooltip {
      position: absolute;
      right: 0;
      bottom: 100%;
      margin-bottom: 8px;
      padding: 4px;
      font-size: 14px;
      line-height: 1.4;
      color: #ffffff;
      background: rgba(0,0,0,0.75);
      border-radius: 4px;
      white-space: nowrap;
      z-index: 10;
  }

  .tooltip::after {
      content: '';
      position: absolute;
      top: 100%;
      left: 24px;
      border: 6px solid transparent;
      border-top-color: rgba(0,0,0,0.75);
  }

  button{
      width:100%;
      padding:10px 16px 10px 16px;
      margin:40px 0 20px 0;
      font-size:18px;
      font-weight:500;
      color:#ffffff;
      background:#3f89ec;
      border:1px solid rgba(27,31,36,0.3);
      border-radius:6px;
      cursor:pointer;
  }

  button:hover:not(:disabled){
      background:#579dfa;
  }

  button:disabled{
      opacity:0.6;
      cursor:not-allowed;
  }

  .statement{
      margin:16px 0 16px 0;
      padding:16px 0 16px 0;
      font-size:18px;
      text-align:center;
  }

  .back-home {
      font-size:18px;
      color:#0969da;
      text-decoration:none;
  }

  .back-home:hover {
     text-decoration:underline;
  }
</style>
