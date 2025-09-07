<template>
  <div class="my-following-user">
    <main>
      <h1 class="title">
        {{username}} 关注的人
      </h1>

      <div class="table-wrapper">
        <table class="following-user-table">
          <thead>
            <tr>
              <th>选择</th>
              <th>他(她)的用户名</th>
              <th>关注时间</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="(item,index) in users"
                v-bind:key="index"
                @mouseenter="hoverIndex = index"
                @mouseleave="hoverIndex = -1">
              <td class="checkbox-wrapper">
                <input v-if="selectMode"
                       type="checkbox"
                       v-model="selectedRows"
                       v-bind:value="index" />
              </td>

              <td>{{ellipsis(item.hisUsername,7)}}</td>
              <td>{{formatTime(item.followingTime)}}</td>

              <td v-if="users.length!==0 && !selectMode && hoverIndex===index"
                  class="row-action">
                <button class="btn-del" @click.stop="deleteSingle(index)">
                  取消关注
                </button>
              </td>
            </tr>

            <tr v-if="users.length===0">
              <td colspan="3" class="empty">暂无关注的人，快去关注哦......</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div>
        <!--选择按钮区-->
        <button v-if="users.length!==0" class="btn-select" @click.stop="toggleSelect">
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
        users: [//测试数据
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 },
          { hisUsername: "lisheywanddf", followingTime: "2025-09-05 20:12", hisUserId: 15 }
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
      ellipsis(str, max) {
        if (!str) return "";
        return str.length > max ? str.slice(0, max) + "..." : str;
      },
      deleteSingle(index) {
        /* TODO：调用后端接口删除单条*/
        console.log("删除单条", this.users[index]);
      },
      deleteBatch() {
        /* TODO：调用后端接口批量删除*/
        console.log("批量删除", this.selectedRows.map(i => this.users[i]));
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
          this.selectedRows = this.users.map((_, i) => i);
          this.isAllSelected = true;
        }
      }
    }
  }
</script>

<style scoped>
  .my-following-user {
    display: flex;
    justify-content: center;
    min-height: 97vh;
    background: #f5f5f5;
    font-family: Arial;
  }

  main {
    width: 100%;
    box-sizing: border-box;
    max-width: 800px;
    margin: 0 auto 0 auto;
    padding: 0 16px 0 16px;
    background: #ffffff;
  }

  .title {
    font-size: 20px;
    font-weight: 600;
    margin: 16px 0 16px 0;
    text-align: left;
  }

  .table-wrapper {
    width: 100%;
    box-sizing: border-box;
    max-height: 80vh;
    overflow-y: auto;
    border: 1px solid #e5e5e5;
    border-radius: 4px;
    margin-bottom: 4px;
  }

  .following-user-table {
    table-layout: fixed;
    width: 100%;
    box-sizing: border-box;
    border-collapse: collapse;
    font-size: 14px;
  }

  .following-user-table thead {
    position: sticky;
    top: 0;
    background: #52acfe;
    color: #ffffff;
    z-index: 20;
    width: 100%;
    box-sizing: border-box;
  }

  .following-user-table tbody tr {
    position: relative;
  }

  .following-user-table tbody tr:hover {
    background: #f0f0f0;
  }

  .following-user-table th,
  .following-user-table td {
    border-right: 1px solid #e5e5e5;
    border-bottom: 1px solid #e5e5e5;
    padding: 8px 8px 0 8px; 
    text-align: left;
    box-sizing: border-box;
    display:inline-block;
    height:34px;
  }

  .following-user-table th:nth-child(1),
  .following-user-table td:nth-child(1) {
    width: 6%;
  }

  .following-user-table th:nth-child(2),
  .following-user-table td:nth-child(2) {
    width: 64%;
  }

  .following-user-table th:nth-child(3),
  .following-user-table td:nth-child(3) {
    width: 30%;
    border-right: none;
  }

  .following-user-table th:last-child,
  .following-user-table td:last-child {
    border-right: none;
  }

  .following-user-table .empty {
    color: #999999;
    text-align: center;
    padding: 40px 0 40px 0;
  }

  .checkbox-wrapper {
    text-align: center;
  }

  .row-action {
    position: absolute;
    right: 8px;
    top: 50%;
    transform: translateY(-50%);
    z-index: 10;
  }

  .btn-del {
    background: #ff4d4f;
    color: #ffffff;
    border: none;
    padding: 2px 2px 2px 2px;
    border-radius: 3px;
    cursor: pointer;
    font-size: 12px;
  }

  .btn-del:disabled {
    background: #cccccc;
    cursor: not-allowed;
  }

  .btn-select {
    margin: 4px 8px 4px 0;
  }
</style>
