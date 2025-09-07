<template>
  <div class="account-setting">
    <h2 class="title">账号设置</h2>

    <form @submit.prevent="handleSubmit">
      <div class="form-row">
        <label>用户名</label>
        <div class="input-box">
          <input v-model="myform.username"
                 v-bind:disabled="!usernameEdit"
                 type="text"
                 v-bind:placeholder="myform.username" />
          <button type="button" class="btn-text" @click="toggleUsername">
            {{usernameEdit ? "确 认" : "修 改"}}
          </button>
        </div>
      </div>

      <div class="form-row">
        <label>新密码</label>
        <div class="input-box">
          <input v-model="myform.password"
                 type="password"
                 placeholder="只允许除空格外的ASCII可见字符" />
        </div>
      </div>

      <div class="form-row">
        <label>确认密码</label>
        <div class="input-box">
          <input v-model="myform.confirmPassword"
                 type="password"
                 placeholder="再次输入新密码以确认" />
        </div>
      </div>

      <div class="form-row">
        <label>手机号</label>
        <div class="input-box">
          <input v-model="myform.phone"
                 v-bind:disabled="!phoneEdit"
                 type="tel"
                 v-bind:placeholder="myform.phone" />
          <button type="button" class="btn-text" @click="togglePhone">
            {{phoneEdit ? "确 认" : "修 改"}}
          </button>
        </div>
      </div>

      <div class="form-row">
        <button type="submit" class="btn-primary" v-bind:disabled="loading">
          {{loading ? "提交中..." : "提 交 修 改"}}
        </button>
      </div>
    </form>
  </div>
</template>

<script>
  export default {
    data() {
      return {
        //以下变量是与后端交互的接口
        myform: {
          username: "_h_our",//当前用户名
          password: "",//新的密码
          confirmPassword: "",//确认的密码
          phone: "12345678901"//当前手机号
        },
        usernameEdit: false,
        phoneEdit: false,
        loading:false
      }
    },
    methods: {
      async getAccountInfo() {
        /* TODO: 获取账号信息 */
      },
      async handleSubmit() {
        /* TODO: 提交修改 */
        this.loading = true;
        //暂时测试
        setTimeout(() => {
          if (this.myform.password !== this.myform.confirmPassword) {
            console.log("密码不同");
          }
          else {
            console.log("提交成功");
            console.log(this.myform);
          }
          this.loading = false;
        }, 1000);
      },
      toggleUsername() {
        if (this.usernameEdit) {
          /* TODO: */
          console.log("username:" + this.myform.username);
        }
        this.usernameEdit = !this.usernameEdit;
      },
      togglePhone() {
        if (this.phoneEdit) {
          /* TODO: */
          console.log("phone:" + this.myform.phone);
        }
        this.phoneEdit = !this.phoneEdit;
      }
    },
    mounted() {
      this.getAccountInfo();
    }
  }
</script>

<style scoped>
  .account-setting{
    width:100%;
/*    height:200vh;*/
    box-sizing:border-box;
/*    max-width:460px;*/
    margin:0 auto 0 auto;
    background:#ffffff;
    padding:32px 40px 32px 40px;
    border-radius:4px;
    box-shadow:0 2px 12px rgba(0,0,0,0.06);
    font-family:Arial;
  }

  .title{
    font-size:30px;
    font-weight:600;
    margin:0 0 30px 0;
    color:#333333;
  }

  .form-row{
    margin:28px 0 28px 0;
  }

  label{
    display:block;
    margin:0 0 4px 0;
    font-size:16px;
    font-weight:400;
    color:#000000;
  }

  .input-box{
    display:flex;
    align-items:center;
  }

  input{
    box-sizing:border-box;
    width:30%;
    height:40px;
    padding:0 10px 0 10px;
    border:2px solid #d9d9d9;
    border-radius:4px;
    font-size:16px;
    transition: border-color 0.3s;
  }

  input:focus{
    border-color:#1890ff;
    outline:none;
  }

  input[disabled]{
    background:#f5f5f5;
    color:#999999;
    cursor:not-allowed;
  }

  .btn-text{
    margin:4px 8px 4px 20px;
    padding:0 4px 2px 4px;
    height:36px;
    width:72px;
    font-size:16px;
    font-weight:500;
    cursor:pointer;
    white-space:nowrap;
  }

  .btn-text:hover{
    color:#096dd9;
  }

  .btn-primary{
    box-sizing:border-box;
    width:30%;
    height:40px;
    padding:4px 8px 4px 8px;
    margin:24px 0 0 0;
    background:#3f89ec;
    border:none;
    font-weight:500;
    border-radius:4px;
    color:#ffffff;
    font-size:20px;
    cursor:pointer;
  }

  .btn-primary:hover{
    background:#579dfa;
  }

  .btn-primary:disabled{
      cursor:not-allowed;
      opacity:0.6;
  }
</style>
