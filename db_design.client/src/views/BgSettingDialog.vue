<template>
  <transition name="fade">
    <div v-if="modelValue.showBgSetting" class="mask">
      <div class="dialog" @click.stop>
        <div class="header">
          <span class="title">背景设置</span>
          <span class="close" @click="handleClose">✕</span>
        </div>

        <div class="grid">
          <div v-for="(item,idx) in showList"
               v-bind:key="idx+1"
               class="img-item"
               v-bind:class="{selected:selectedBg===item.url}"
               @click="selectedBg=item.url"
          >
            <img v-bind:src="item.url.trim()"/>
            <span>{{item.name}}</span>
          </div>
        </div>

        <div class="paginatioin">
          <button v-if="currentPage===2" @click="currentPage=1">上一页</button>
          <button v-bind:class="{active:currentPage===1}" @click="currentPage=1">1</button>
          <button v-bind:class="{active:currentPage===2}" @click="currentPage=2">2</button>
          <button v-if="currentPage===1" @click="currentPage=2">下一页</button>
        </div>

        <div class="footer">
          <button class="save" @click="handleSave">保存</button>
          <button class="cancle" @click="handleClose">取消</button>
        </div>
      </div>
    </div>
  </transition>
</template>

<script>
  export default {
    props: {
      modelValue: Object
    },
    emits: [
      "update:modelValue"
    ],
    data() {
      return {
        //当前背景可以从父组件得知
        currentPage: 1,
        selectedBg: "",
        bgList: [
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1001.jpg", name: "海之梦" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1002.jpg", name: "千山雪" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1003.jpg", name: "长天一色" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1004.jpg", name: "银汉迢迢" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1005.jpg", name: "春意浓" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1006.jpg", name: "出水芙蓉" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1007.jpg", name: "白色飞羽" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1008.jpg", name: "寥落星河" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1009.jpg", name: "廊桥遗梦" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1010.jpg", name: "接天莲叶" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1011.jpg", name: "雪山日出" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1012.jpg", name: "原野晨曦" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1013.jpg", name: "三叶草" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1014.jpg", name: "层林尽染" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1015.jpg", name: "水墨荷花" },
          { url: "https://tb2.bdstatic.com/tb/static-ihome/img/bg_v1_1016.jpg", name: "金沙子" }
        ]
      }
    },
    computed: {
      showList() {
        const start = (this.currentPage - 1) * 8;
        return this.bgList.slice(start, start + 8);
      }
    },
    methods: {
      handleClose() {
        this.$emit("update:modelValue", { show: false, url: undefined });
      },
      handleSave() {
        /* TODO: */
        this.$emit("update:modelValue", { show: false, url: this.selectedBg });
        this.handleClose();
      }
    }
  }
</script>

<style scoped>
  .mask{
      position:absolute;
      inset:0;
      background:rgba(0,0,0,0.45);
      display:flex;
      justify-content:center;
      align-items:start;
      z-index:9999;
  }

  .dialog{
      margin:160px 0 0 0;
      width:680px;
      background:#ffffff;
      border-radius:4px;
      padding:4px 8px 4px 8px;
      box-shadow:0 4px 24px rgba(0,0,0,0.15);
  }

  .header{
      display:flex;
      justify-content:space-between;
      align-items:center;
      margin:0 0 16px 0;
  }

  .title{
      font-size:16px;
      font-weight:500;
  }

  .close{
      cursor:pointer;
      font-size:18px;
      color:#666666;
  }

  .grid{
      display:grid;
      grid-template-columns:repeat(4,1fr);
      gap:12px 12px;
  }

  .img-item{
      display:flex;
      flex-direction:column;
      align-items:center;
      cursor:pointer;
      border:2px solid transparent;
      border-radius:4px;
      padding:4px 4px 4px 4px;
  }

  .img-item.selected{
      border-color:#1890ff;
  }

  .img-item img{
      width:100%;
      height:90px;
      object-fit:cover;
      border-radius:4px;
  }

  .paginatioin{
      margin:16px 0 16px 0;
      text-align:center;
  }

  .paginatioin button{
      margin:0 4px 0 4px;
      padding:4px 10px 4px 10px;
      border:1px solid #d9d9d9;
      background:#ffffff;
      border-radius:4px;
      cursor:pointer;
  }

  .paginatioin button.active{
      background:#1890ff;
      color:#ffffff;
      border-color:#1890ff;
  }

  .footer{
      display:flex;
      justify-content:center;
      gap:24px;
  }

  .save{
      background:#1890ff;
      color:#ffffff;
      border:none;
      padding:6px 18px 6px 18px;
      border-radius:4px;
      cursor:pointer;
  }

  .cancle{
      background:#ffffff;
      color:#333333;
      border:1px solid #d9d9d9;
      padding:5px 17px 5px 17px;
      border-radius:4px;
      cursor:pointer;
  }

  .fade-enter-active,
  .fade-leave-active{
      transition:opacity .2s;
  }

  .fade-enter,
  .fade-leave-to{
      opacity:0;
  }
</style>
