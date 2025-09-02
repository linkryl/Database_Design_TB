<template>
  <div class="user-info">
    <h2 class="title">个人资料</h2>

    <form @submit.prevent="handleSubmit">
      <div class="form-row">
        <label>性别</label>
        <label class="radio-label">
          <input type="radio"
                 name="set-gender"
                 value="male"
                 v-model="myform.gender" />
          男
        </label>
        <label class="radio-label">
          <input type="radio"
                 name="set-gender"
                 value="female"
                 v-model="myform.gender" />
          女
        </label>
      </div>

      <div class="form-row">
        <label>出生日期</label>
        <input type="date"
               v-model="myform.birthdate"
               v-bind:max="today" />
      </div>

      <div class="form-row">
        <label>个人简介 {{myform.profile.length}}/500</label>
        <textarea v-model="myform.profile"
                  maxlength="500"
                  placeholder="介绍自己！不超过 500 字......"></textarea>
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
        //以下变量是与后端的接口
        myform: {
          gender: "male",
          birthdate: "2025-08-30",
          profile: ""
        },
        loading: false
      }
    },
    computed: {
      today() {
        //.toISOString() 返回字符串 "2025-08-30T14:23:45.123Z"
        return new Date().toISOString().split("T")[0]
      }
    },
    methods: {
      handleSubmit() {
        /* TODO: 提交修改 */
        this.loading = true;
        //暂时测试
        setTimeout(() => {
          this.loading = false;
        }, 1000);
      }
    },
    mounted() {

    }
  }
</script>

<style scoped>
  .user-info{
    width:100%;
    box-sizing:border-box;
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

  .radio-label{
    display:inline-flex;
    align-items:center;
    margin:0 20px 0 0;
    font-weight:400;
    cursor:pointer;
  }

  .radio-label input{
    margin:0 6px 0 0;
  }

  input[type="date"]{
    box-sizing:border-box;
    width:30%;
    padding:8px 10px 8px 10px;
    border:2px solid #d9d9d9;
    border-radius:4px;
    font-size:16px;
  }

  input[type="date"]:hover{
    border-color:#1890ff;
    outline:none;
  }

  textarea{
    box-sizing:border-box;
    width:61.8%;
    min-height:30vh;
    padding:24px 24px 24px 24px;
    border:2px solid #d9d9d9;
    border-radius:4px;
    font-size:16px;
    resize:none;
  }

  textarea:focus{
    border-color:#1890ff;
    outline:none;
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

  .btn-primary:hover {
    background: #579dfa;
  }

  .btn-primary:disabled{
      opacity:0.6;
      cursor:not-allowed;
  }
</style>
