<template>
  <div class="ucenter-page">
    <aside class="left-sidebar">
      <div class="user-box">
        <img class="avatar" v-bind:src="avatarUrl" alt="avatar"/>

        <span class="username">{{username}}</span>
        <br/>
        用户中心
      </div>

      <nav class="menu">
        <div
             v-for="item in menuList"
             v-bind:key="item.key"
             class="menu-item"
             v-bind:class="{active:activeKey===item.key}"
             @click="handleMenuClick(item.key)"
        >
          {{item.label}}
          <span class="arrow">>></span>
        </div>
      </nav>

      <router-link to="/"class="back-home">返回首页</router-link>
    </aside>

    <main class="content">
      <!-- 根据 activeKey 渲染对应组件 -->
      <AccountSetting v-if="activeKey === 'account'" />
      <UserInfo v-if="activeKey === 'uinfo'" />
      <MyTreeHole v-if="activeKey === 'treehole'" />
    </main>

  </div>
</template>

<script>
  import AccountSetting from "@/views/AccountSetting.vue"
  import UserInfo from "@/views/UserInfo.vue"
  import MyTreeHole from "@/views/MyTreeHole.vue"

  export default {
    components: {
      AccountSetting,
      UserInfo,
      MyTreeHole
    },
    data() {
      return {
        //以下变量是与后端的接口
        avatarUrl: "https://gss0.baidu.com/7Ls0a8Sm2Q5IlBGlnYG/sys/portraith/item/tb.1.bdb1c536.7qSuTE4CMJHwpEF4Qw60Mw",
        username: "_h_our",
        activeKey: "treehole",
        //菜单项数组
        menuList: [
          { key: "uinfo", label: "个人资料" },
          { key: "account", label: "账号设置" },
          { key: "treehole", label: "TreeHole" }
        ]
      }
    },
    computed: {
      // 临时用不同背景色区分占位区域，可删除
      boxColor() {
        switch (this.activeKey) {
          case "uinfo": return "#e3f2fd";
          case "account": return "#fff3e0";
          case "treehole": return "#e8f5e9";
          default : return "fafafa";
        }
      }
    },
    methods: {
      handleMenuClick(key) {
        this.activeKey = key
        //TODO:
      }
    }
  }
</script>

<style scoped>
  .ucenter-page{
    display:flex;
    min-height:97vh;
    background:#f5f5f5;
    font-family:Arial;
  }

  .left-sidebar{
    width:20%;
    background:#ffffff;
    border-right:1px solid #e0e0e0;
    padding:24px 0 24px 0;
  }

  .user-box{
    display:flex;
    flex-direction:column;
    align-items:center;
    padding:20px 20px 20px 20px;
    margin:0 auto 0 auto;
  }

  .avatar{
      width:35%;
      height:35%;
      border-radius:50%;
      object-fit:cover;
      margin-bottom:12px;
  }

  .username{
    font-size:30px;
    font-weight:600;
    color:#333333;
  }

  .menu{
      margin:0px 0 16px 0;
      padding: 20px 20px 20px 20px;
  }

  .menu-item{
      padding:12px 16px 12px 13px;
      border-left:3px solid #b0b0b0;
      margin:4px auto 4px auto;
      cursor:pointer;
      transition:background-color 0.3s;
      font-size:20px;
      color:#666666;
      position:relative;
  }

  .arrow{
      position:absolute;
      right:12px;
      top:50%;
      color:#bbbbbb;
      font-size:20px;
      transform: translateY(-50%);
  }

  .menu-item:hover{
      background-color:#f1f1f1;
      border-left-color: #d9d9d9;
  }

  .menu-item.active{
      background-color:#e6f7ff;
      color:#1890ff;
      font-weight:600;
      border-left-color:#1890ff;
  }

  .menu-item.active .arrow{
      color:#1890ff;
  }

  .back-home{
      font-size:16px;
      color:#0969da;
      text-decoration:none;
      display:block;
      width:fit-content;
      margin:0 auto 0 auto;
  }

  .back-home:hover{
      text-decoration:underline;
  }

  .content{
      flex:1;
      padding:24px 24px 24px 24px;
      box-sizing:content-box;
  }

  .content-box {
    height: 100%;
    border-radius: 4px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 18px;
    color: #555555;
  }

</style>
