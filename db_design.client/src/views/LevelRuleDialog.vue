<template>
  <transition name="fade">
    <div v-if="modelValue" class="mask">
      <div class="dialog" @click.stop>
        <div class="header">
          <span class="title">等级规则</span>
          <span class="close" @click="handleClose">✕</span>
        </div>

        <main class="main-context">
          <p>您在 TreeHole 等级的提升，取决于经验值的多少，以下是等级与经验值对照表：</p>
          <table class="level-table">
            <thead>
              <tr>
                <th>用户等级</th>
                <th>所需经验值</th>
                <th>用户等级</th>
                <th>所需经验值</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="row in rows" v-bind:key="row.level1">
                <td>{{row.level1}}</td>
                <td>{{row.exp1}}</td>
                <td>{{row.level2}}</td>
                <td>{{row.exp2}}</td>
              </tr>
            </tbody>
          </table>
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
        rows: [
          { level1: 1, exp1: 1, level2: 10, exp2: 2000 },
          { level1: 2, exp1: 5, level2: 11, exp2: 3000 },
          { level1: 3, exp1: 15, level2: 12, exp2: 6000 },
          { level1: 4, exp1: 30, level2: 13, exp2: 10000 },
          { level1: 5, exp1: 50, level2: 14, exp2: 18000 },
          { level1: 6, exp1: 100, level2: 15, exp2: 30000 },
          { level1: 7, exp1: 200, level2: 16, exp2: 60000 },
          { level1: 8, exp1: 500, level2: 17, exp2: 100000 },
          { level1: 9, exp1: 1000, level2: 18, exp2: 300000 }
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
    padding: 4px 8px 4px 8px;
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

  .main-context{
    display:block;
    padding:0 16px 12px 16px;
    color:#000000;
    height:400px;
    overflow-y:auto;
  }

  .level-table{
    width:100%;
    box-sizing:border-box;
    border-collapse:collapse;
    font-size:14px;
  }

  .level-table th,
  .level-table td{
      border:1px solid #e5e5e5;
      padding:8px;
      text-align:center;
  }

  .level-table th{
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
