<template>
  <div class="my-treehole"
       v-bind:style="{backgroundImage:`url(${bgSetting.bgUrl})`}">
    <div class="top-banner">
      <button class="btn-bg-setting" @click="bgSetting.showBgSetting = true">
        Bg
      </button>
    </div>

    <div class="main-content">
      <!--第 1 行-->
      <div class="row-top">
        <!--左 1 块-->
        <div class="block">
          <div class="mini-row">
            <span>我的角色：{{myRoleText}}</span>
            <router-link v-if="myRole===0"
                         to="/promotion"
                         target="_blank"
                         class="btn-link">
              晋升
            </router-link>
          </div>

          <div class="mini-row">
            <span>我的状态：{{myStatusText}}</span>
            <router-link v-if="myStatus===1"
                         to="/appeal"
                         target="_blank"
                         class="btn-link">
              申诉
            </router-link>
          </div>
        </div>

        <!--左 2 块-->
        <div class="block">
          <div class="mini-row">
            被关注数：{{myFollowerCount}}
          </div>

          <div class="mini-row">
            <router-link to="/my-follower"
                         target="_blank"
                         class="btn-link">
              谁关注了我
            </router-link>
          </div>
        </div>

        <!--左 3 块-->
        <div class="block">
          <div class="mini-row">
            <span>我的等级：{{myLevel}}</span>
            <button class="btn-link" @click="showLevelRule=true">
              等级规则
            </button>
          </div>

          <div class="mini-row">
            <span>经验值：{{myExp}}/{{levelExpMax}}</span>
            <button class="btn-link" @click="showGetExp=true">
              获取经验
            </button>
          </div>
        </div>

        <!--左 4 块-->
        <div class="block coin-block">
          <div class="mini-row">
            <span>我的金币🪙： {{myCoin}}</span>
          </div>

          <div class="mini-row">
            <button class="btn-link" @click="showGetCoin=true">
              获取金币
            </button>
            <router-link to="/market"
                         target="_blank"
                         class="btn-link">
              道具商城
            </router-link>
          </div>
        </div>
      </div>

      <!--第 二 行-->
      <div class="row row-menu">
        <router-link to="#"
                     exact-active-class="active"
                     class="menu-item">
          我的主页
        </router-link>

        <router-link to="/chat"
                     target="_blank"
                     class="menu-item">
          私信/群聊
        </router-link>

        <router-link to="/my-following-bar"
                     target="_blank"
                     class="menu-item">
          关注的吧：{{myFollowingBarCount}}
        </router-link>

        <router-link to="/my-following-user"
                     target="_blank"
                     class="menu-item">
          关注的人：{{myFollowingUserCount}}
        </router-link>

        <router-link to="/my-favorite-post"
                     target="_blank"
                     class="menu-item">
          我的收藏：{{myFavoritePostCount}}
        </router-link>
      </div>

      <!--第 三 行-->
      <div class="section-title">
        爱逛的吧
      </div>

      <!--第 四 行-->
      <div class="section-title">
        热门动态
      </div>
    </div>

    <!--弹窗-->
    <BgSettingDialog v-bind:modelValue="bgSetting" @update:modelValue="onBgSetting" />
    <LevelRuleDialog v-bind:modelValue="showLevelRule" @update:modelValue="onLevelRule" />
    <GetExpDialog v-bind:modelValue="showGetExp" @update:modelValue="onGetExp" />
    <GetCoinDialog v-bind:modelValue="showGetCoin" @update:modelValue="onGetCoin"/>
  </div>
</template>

<script>
  import BgSettingDialog from "@/views/BgSettingDialog.vue"
  import LevelRuleDialog from "@/views/LevelRuleDialog.vue"
  import GetExpDialog from "@/views/GetExpDialog.vue"
  import GetCoinDialog from "@/views/GetCoinDialog.vue"

  export default {
    components: {
      BgSettingDialog,
      LevelRuleDialog,
      GetExpDialog,
      GetCoinDialog
    },
    data() {
      return {
        //以下变量是与后端的接口
        myRole: 0,
        myStatus: 1,
        myFollowerCount: 0,
        myLevel: 0,
        myExp: 0,
        levelExpMax: 100,
        myCoin: 0,
        myFollowingBarCount: 0,
        myFollowingUserCount: 0,
        myFavoritePostCount: 0,

        //渲染弹窗时 与子组件通信的参数
        bgSetting: {
          showBgSetting: false,
          //背景 URL 从后端获取
          bgUrl: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1001.jpg"
        },
        showLevelRule: false,
        showGetExp: false,
        showGetCoin: false,
        
      }
    },
    computed: {
      myRoleText() {
        return this.myRole === 0 ? "普通用户" : "管理员";
      },
      myStatusText() {
        return this.myStatus === 0 ? "正常" : "封禁";
      }
    },
    methods: {
      onBgSetting({show,url}) {
        this.bgSetting.showBgSetting = show;
        if (url !== undefined) {
          this.bgSetting.bgUrl = url;
        }
      },
      onLevelRule(show) {
        this.showLevelRule = show;
      },
      onGetExp(show) {
        this.showGetExp = show;
      },
      onGetCoin(show) {
        this.showGetCoin = show;
      }
    }
  }
</script>

<style scoped>
  .my-treehole {
    width: 100%;
    min-height: 300vh;
    position:relative;
    box-sizing: border-box;
    margin: 0 auto 0 auto;
    padding: 32px 40px 32px 40px;
    border-radius: 4px;
    box-shadow: 0 2px 12px rgba(0,0,0,0.06);
    font-family: Arial;
    /*上方背景图片*/
    background-size: 227%;
    background-position: center -20px;
    background-repeat: no-repeat;
    /*最后的白色背景*/
    background-color:#ffffff;
  }

  .my-treehole::before{
    content:'';
    position:absolute;
    left:0;
    top:904px;
    width:100%;
    box-sizing:border-box;
    height:300px;
    pointer-events:none;
    /*渐变分界线*/
    background-image:linear-gradient(
      to bottom,
      rgba(255,255,255,0) 0,
      #ffffff 300px
    );
  }

  .top-banner{
    width:100%;
    box-sizing:border-box;
    position:relative;
    height:234px;
    background:none;
  }

  .btn-bg-setting{
    position:absolute;
    top:0;
    right:0;
    padding:4px;
    background:#ffffff;
    color:#000000;
    border-radius:4px;
    border-color:#ffffff;
    font-size:12px;
    font-weight:600;
    text-decoration:none;
    cursor:pointer;
  }

  .main-content{
      width:100%;
      box-sizing:border-box;
      background:#ffffff;
  }

  .row{
      display:flex;
      justify-content:space-between;
      align-items:flex-start;
      padding:12px 16px 12px 16px;
      border-bottom:1px solid #eeeeee;
  }

  .row-top {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin:0 0 16px 0;
      padding: 0 0 16px 0;
      border-bottom: 1px solid #eeeeee;
  }

  .block{
      flex:1;
      display:flex;
      flex-direction:column;
      align-items:flex-start;
      margin:0 8px 0 8px;
  }

  .coin-block{
      border-top:2px solid #ff8c00;
  }

  .mini-row{
      width:100%;
      box-sizing:border-box;
      display:flex;
      align-items:center;
      margin:4px 0 4px 0;
      font-size:16px;
  }

  span{
      margin:0 10px 0 0;
  }

  .btn-link{
      margin:0 10px 0 0;
      padding:2px 6px 2px 6px;  
      background:#f2f2f2;
      border:none;
      border-radius:4px;
      font-size:16px;
      font-weight:400;
      color:#1890ff;
      cursor:pointer;
      text-decoration:none;
  }

  .row-menu{
      justify-content:flex-start;
      gap:6px;
  }

  .menu-item{
      font-size:16px;
      color:#333333;
      text-decoration:none;
      padding:4px 8px 4px 8px;
      border-top:2px solid #d9d9d9;
  }

  .menu-item.active{
      color:#1890ff;
      border-top-color:#1890ff;
  }
  
  .menu-item:hover {
      background-color: #f1f1f1;
  }

  .section-title{
      padding:16px 16px 16px 16px;
      font-weight:600;
      font-size:16px;
  }
</style>
