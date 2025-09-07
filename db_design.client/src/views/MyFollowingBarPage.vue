<template>
  <div class="my-following-bar">
    <main>
      <h1 class="title">
        {{username}} 关注的吧
      </h1>

      <div class="table-wrapper">
        <table class="following-bar-table">
          <thead>
            <tr>
              <th>选择</th>
              <th>贴吧名</th>
              <th>吧主</th>
              <th>关注时间</th>
            </tr>
          </thead>

          <tbody>
            <tr
                v-for="(item,index) in bars"
                v-bind:key="index"
                @mouseenter="hoverIndex = index"
                @mouseleave="hoverIndex = -1"
            >
              <td class="checkbox-wrapper">
                <input
                       v-if="selectMode"
                       type="checkbox"
                       v-model="selectedRows"
                       v-bind:value="index"
                />
              </td>

              <td>{{ellipsis(item.barName,15)}}</td>
              <td>{{ellipsis(item.ownerName,7)}}</td>
              <td>{{formatTime(item.followingTime)}}</td>

              <td
                  v-if="bars.length!==0 && !selectMode && hoverIndex===index"
                  class="row-action"
              >
                <button class="btn-del" @click.stop="deleteSingle(index)">
                  取消关注
                </button>
              </td>
            </tr>

            <tr v-if="bars.length===0">
              <td colspan="4" class="empty">暂无关注的吧，快去关注哦......</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div><!--选择按钮区-->
        <button v-if="bars.length!==0" class="btn-select" @click.stop="toggleSelect">
          {{selectMode ? "取消选择" : "选择"}}
        </button>

        <div v-if="selectMode">
          <button class="btn-select" @click="toggleSelectAll">
            {{isAllSelected ? "取消全选" : "全选"}}
          </button>

          <button class="btn-del" v-bind:disabled="selectedRows.length===0" @click="deleteBatch">
            取消关注
          </button>
        </div>
      </div>
    </main>
  </div>
</template>

<script>
  export default {
    data() {
      return {
        username: "_h_our",
        bars: [//测试数据
          { barName: "数学", ownerName: "_h_our1", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学1", ownerName: "_h_our11", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数1学", ownerName: "_h_our111", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "1数学", ownerName: "_h_our1111", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数11学", ownerName: "_h_ou1111", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学11", ownerName: "_h_our11111", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "11数学", ownerName: "_h_ou111r", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数111学", ownerName: "_h_o111ur", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学111", ownerName: "_h_o11ur", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "111数学", ownerName: "_h_o111ur", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 },
          { barName: "数学", ownerName: "_h_our", followingTime: "2025-09-05 20:12", barId: 15 }
        ],
        hoverIndex: -1,
        selectMode: false,
        selectedRows: [],
        isAllSelected: false
      }
    },
    mounted() {
      /* TODO：从后端获取数据 */
    },
    computed: {

    },
    methods: {
      formatTime(date) {
        if (!date) return "";
        if (typeof date === "string") return date;
        const d = new Date(date);
        const pad = (x) => x.toString().padStart(2, "0");
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
      },
      ellipsis(str,max) {
        if (!str) return "";
        return str.length > max ? str.slice(0, max) + "..." : str;
      },
      deleteSingle(index) {
        /* TODO：调用后端接口删除单条*/
        console.log("删除单条", this.bars[index]);
      },
      deleteBatch() {
        /* TODO：调用后端接口批量删除*/
        console.log("批量删除", this.selectedRows.map(i => this.bars[i]));
      },
      toggleSelect() {
        this.selectMode = !this.selectMode;
        this.hoverIndex = -1;
        this.selectedRows = [];
        this.isAllSelected = false;
      },
      toggleSelectAll() {
        if (this.isAllSelected) {
          this.selectedRows = [];
          this.isAllSelected = false;
        }
        else {
          this.selectedRows = this.bars.map((_, i) => i);
          this.isAllSelected = true;
        }
      }
    }
  }
</script>

<style scoped>
  .my-following-bar{
      display:flex;
      justify-content:center;
      min-height:97vh;
      background:#f5f5f5;
      font-family:Arial;
  }

  main{
      width:100%;
      box-sizing:border-box;
      max-width:800px;
      margin:0 auto 0 auto;
      padding:0 16px 0 16px;
      background:#ffffff;
  }

  .title {
    font-size: 20px;
    font-weight: 600;
    margin: 16px 0 16px 0;
    text-align: left;
  }

  .table-wrapper{
      width:100%;
      box-sizing:border-box;
      max-height:80vh;
      overflow-y:auto;
      border:1px solid #e5e5e5;
      border-radius:4px;
      margin-bottom:4px;
  }

  .following-bar-table{
      table-layout:fixed;
      width:100%;
      box-sizing:border-box;
      border-collapse:collapse;
      font-size:14px;
  }

  .following-bar-table thead{
      position:sticky;
      top:0;
      background:#52acfe;
      color:#ffffff;
      z-index:20;
      width:100%;
      box-sizing:border-box;
  }

  .following-bar-table tbody tr{
      position:relative;
  }

  .following-bar-table tbody tr:hover{
      background:#f0f0f0;
  }

    .following-bar-table th,
    .following-bar-table td {
      border-right: 1px solid #e5e5e5;
      border-bottom: 1px solid #e5e5e5;
      padding: 6px 8px 6px 8px;
      text-align: left;
      box-sizing: border-box;
      display: inline-block;
      height: 34px;
    }

  .following-bar-table th:nth-child(1),
  .following-bar-table td:nth-child(1){
      width:6%;
  }

  .following-bar-table th:nth-child(2),
  .following-bar-table td:nth-child(2){
      width:44%;
  }

  .following-bar-table th:nth-child(3),
  .following-bar-table td:nth-child(3){
      width:20%;
  }

  .following-bar-table th:nth-child(4),
  .following-bar-table td:nth-child(4){
      width:30%;
      border-right:none;
  }

  .following-bar-table th:last-child,
  .following-bar-table td:last-child {
      border-right: none;
  }


  .following-bar-table .empty{
      color:#999999;
      text-align:center;
      padding:40px 0 40px 0;
  }

  .checkbox-wrapper{
      text-align:center;
  }

  .row-action{
      position:absolute;
      right:8px;
      top:50%;
      transform:translateY(-50%);
      z-index:10;
  }

  .btn-del{
      background:#ff4d4f;
      color:#ffffff;
      border:none;
      padding:2px 2px 2px 2px;
      border-radius:3px;
      cursor:pointer;
      font-size:12px;
  }

  .btn-del:disabled{
      background:#cccccc;
      cursor:not-allowed;
  }

  .btn-select{
      margin:4px 8px 4px 0;
  }
</style>
