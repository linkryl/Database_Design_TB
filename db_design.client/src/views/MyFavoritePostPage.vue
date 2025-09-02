<template>
  <div class="my-favorite-post">
    <main>
      <h1 class="title">
        {{username}} 收藏的帖子
      </h1>

      <div class="table-wrapper">
        <table class="favorite-post-table">
          <thead>
            <tr>
              <th>选择</th>
              <th>标题</th>
              <th>作者</th>
              <th>收藏时间</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="(item,index) in favorites" v-bind:key="index"
                @mouseenter="hoverIndex = index"
                @mouseleave="hoverIndex = -1">
              <!-- TODO：还需实现：点击对应内容后跳转到对应页面 -->

              <!--复选框-->
              <td class="checkbox-cell">
                <input v-if="selectMode" type="checkbox" v-model="selectedRows" v-bind:value="index">
              </td>

              <!--内容列-->
              <td>{{ ellipsis(item.title, 15) }}</td>
              <td>{{ ellipsis(item.author, 7) }}</td>
              <td>{{ formatTime(item.favoriteTime) }}</td>

              <!--行内悬停按钮，仅在未选择模式出现 -->
              <td v-if="favorites.length!==0 && !selectMode && hoverIndex === index"
                  class="row-action">
                <button class="btn-del" @click.stop="deleteSingle(index)">
                  取消收藏
                </button>
              </td>
            </tr>

            <tr v-if="favorites.length===0">
              <td colspan="4" class="empty">暂无收藏的帖子，快去收藏哦...</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!--选择按钮区域-->
      <div>
        <button v-if="favorites.length!==0" class="btn-select" @click="toggleSelect">
          {{selectMode ? "取消选择" : "选择"}}
        </button>
        <div class="action-bar" v-if="selectMode">
          <button class="btn-select" @click="toggleSelectAll">
            {{ isAllSelected ? '取消全选' : '全选' }}
          </button>
          <button class="btn-del"
                  v-bind:disabled="selectedRows.length === 0"
                  @click="deleteBatch">
            取消收藏
          </button>
        </div>
      </div>
    </main>
  </div>
</template>

<script>
  export default{
    data(){
      return {
        //从后端获取的数据
        username:"_h_our",
        //收藏的帖子从后端获取，初始化为空
        favorites:[
          //测试数据
          {title:"如何评价：宇宙来源于无中生有？",author:"月兮醉夕",favoriteTime:"2025-09-01 15:34",postId:13},
          { title: "我该怎么做宇宙来源于无中生有？宇宙来源于无中生有？", author: "TURBID浊", favoriteTime: "2025-09-01 14:39", postId: 14},
          { title: "有个问题，能量守恒定律是不会凭空消失也不消失也不", author: "强q有", favoriteTime: "2025-09-01 14:14", postId: 11},
          { title: "以前看到的一个问题，还是有点不太理解，想消失也不", author: "名师首饰电镀厂名师首饰电镀厂", favoriteTime: "2025-09-01 14:18", postId: 10},
          { title: "做了一个梦。宇宙来源于无中生有？宇宙来源于无中生有？", author: "永猎双子-千珏名师首饰电镀厂", favoriteTime: "2025-09-01 16:34", postId: 12},
          { title: "我统一四种力了，有老师愿意了解吗宇宙来源于无中生有？", author: "地方本报北京", favoriteTime: "2025-09-01 12:34", postId: 11},
          { title: "如何评价：宇宙来源于无中生有？", author: "月兮醉夕", favoriteTime: "2025-09-01 15:34", postId: 13 },
          { title: "我该怎么做宇宙来源于无中生有？宇宙来源于无中生有？", author: "TURBID浊", favoriteTime: "2025-09-01 14:39", postId: 14 },
          { title: "有个问题，能量守恒定律是不会凭空消失也不消失也不", author: "强q有", favoriteTime: "2025-09-01 14:14", postId: 11 },
          { title: "以前看到的一个问题，还是有点不太理解，想消失也不", author: "名师首饰电镀厂名师首饰电镀厂", favoriteTime: "2025-09-01 14:18", postId: 10 },
          { title: "做了一个梦。宇宙来源于无中生有？宇宙来源于无中生有？", author: "永猎双子-千珏名师首饰电镀厂", favoriteTime: "2025-09-01 16:34", postId: 12 },
          { title: "我统一四种力了，有老师愿意了解吗宇宙来源于无中生有？", author: "地方本报北京", favoriteTime: "2025-09-01 12:34", postId: 11 },
          { title: "如何评价：宇宙来源于无中生有？", author: "月兮醉夕", favoriteTime: "2025-09-01 15:34", postId: 13 },
          { title: "我该怎么做宇宙来源于无中生有？宇宙来源于无中生有？", author: "TURBID浊", favoriteTime: "2025-09-01 14:39", postId: 14 },
          { title: "有个问题，能量守恒定律是不会凭空消失也不消失也不", author: "强q有", favoriteTime: "2025-09-01 14:14", postId: 11 },
          { title: "以前看到的一个问题，还是有点不太理解，想消失也不", author: "名师首饰电镀厂名师首饰电镀厂", favoriteTime: "2025-09-01 14:18", postId: 10 },
          { title: "做了一个梦。宇宙来源于无中生有？宇宙来源于无中生有？", author: "永猎双子-千珏名师首饰电镀厂", favoriteTime: "2025-09-01 16:34", postId: 12 },
          { title: "我统一四种力了，有老师愿意了解吗宇宙来源于无中生有？", author: "地方本报北京", favoriteTime: "2025-09-01 12:34", postId: 11 },
          { title: "如何评价：宇宙来源于无中生有？", author: "月兮醉夕", favoriteTime: "2025-09-01 15:34", postId: 13 },
          { title: "我该怎么做宇宙来源于无中生有？宇宙来源于无中生有？", author: "TURBID浊", favoriteTime: "2025-09-01 14:39", postId: 14 },
          { title: "有个问题，能量守恒定律是不会凭空消失也不消失也不", author: "强q有", favoriteTime: "2025-09-01 14:14", postId: 11 },
          { title: "以前看到的一个问题，还是有点不太理解，想消失也不", author: "名师首饰电镀厂名师首饰电镀厂", favoriteTime: "2025-09-01 14:18", postId: 10 },
          { title: "做了一个梦。宇宙来源于无中生有？宇宙来源于无中生有？", author: "永猎双子-千珏名师首饰电镀厂", favoriteTime: "2025-09-01 16:34", postId: 12 },
          { title: "我统一四种力了，有老师愿意了解吗宇宙来源于无中生有？", author: "地方本报北京", favoriteTime: "2025-09-01 12:34", postId: 11 },
          { title: "如何评价：宇宙来源于无中生有？", author: "月兮醉夕", favoriteTime: "2025-09-01 15:34", postId: 13 },
          { title: "我该怎么做宇宙来源于无中生有？宇宙来源于无中生有？", author: "TURBID浊", favoriteTime: "2025-09-01 14:39", postId: 14 },
          { title: "有个问题，能量守恒定律是不会凭空消失也不消失也不", author: "强q有", favoriteTime: "2025-09-01 14:14", postId: 11 },
          { title: "以前看到的一个问题，还是有点不太理解，想消失也不", author: "名师首饰电镀厂名师首饰电镀厂", favoriteTime: "2025-09-01 14:18", postId: 10 },
          { title: "做了一个梦。宇宙来源于无中生有？宇宙来源于无中生有？", author: "永猎双子-千珏名师首饰电镀厂", favoriteTime: "2025-09-01 16:34", postId: 12 },
          { title: "我统一四种力了，有老师愿意了解吗宇宙来源于无中生有？", author: "地方本报北京", favoriteTime: "2025-09-01 12:34", postId: 11 }
        ],
        //与选择按钮相关变量
        selectMode: false,         // 是否进入选择状态
        selectedRows: [],          // 选中的 index 数组
        hoverIndex: -1,             // 鼠标悬停行索引
        isAllSelected: false       // 未全选
      }
    },
    mounted(){
      /* TODO：从后端获取数据 */
    },
    methods: {
      toggleSelect() {
        this.selectMode = !this.selectMode
        this.selectedRows = []
        this.hoverIndex = -1
      },
      toggleSelectAll() {
        if (this.isAllSelected) {
          this.selectedRows = []
          this.isAllSelected = false
        }
        else {
          this.selectedRows = this.favorites.map((_, i) => i)
          this.isAllSelected = true
        }
      },
      deleteSingle(index) {
        // TODO：调后端接口删除单条
        console.log('删除单条', this.favorites[index])
      },
      deleteBatch() {
        const list = this.selectedRows.map(i => this.favorites[i])
        // TODO：调后端接口批量删除
        console.log('批量删除', list)
      },
      //格式化时间
      //确保后端传给前端的时间是 JS Date 类型
      formatTime(date){
        if(!date) return "";
        if(typeof date === "string") return date;//测试时直接写死string类型的日期
        const d = new Date(date);
        const pad = (x) => x.toString().padStart(2,"0");
        return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate)} ${pad(d.getHours())}:${pad(d.getMinutes())}`
      },
      ellipsis(str,max) {
        if (!str) return "";
        return str.length > max ? str.slice(0, max) + "..." : str; 
      }
    }
  }
</script>

<style scoped>
  .my-favorite-post{
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

  .title{
      font-size:20px;
      font-weight:600;
      margin:16px 0 16px 0;
      text-align:left;
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

  .favorite-post-table{
      table-layout:fixed;
      width:100%;
      box-sizing:border-box;
      border-collapse:collapse;
      font-size:14px;
  }

  .favorite-post-table thead {
      position: sticky;
      top: 0;
      background: #52acfe;
      color:#ffffff;
      z-index: 1;
      width: 100%;
      box-sizing: border-box;
  }

  .favorite-post-table tbody tr{
      position:relative;
  }

  .favorite-post-table th:nth-child(1),
  .favorite-post-table td:nth-child(1) {
      width: 6%;
  }

  .favorite-post-table th:nth-child(2),
  .favorite-post-table td:nth-child(2) {
      width: 44%;
  }

  .favorite-post-table th:nth-child(3),
  .favorite-post-table td:nth-child(3) {
      width: 20%;
  }

  .favorite-post-table th:nth-child(4),
  .favorite-post-table td:nth-child(4) {
    width: 30%;
  }

  .favorite-post-table th,
  .favorite-post-table td {
      border-right: 1px solid #e5e5e5;
      border-bottom: 1px solid #e5e5e5;
      padding: 6px 8px;
      text-align: left;
      box-sizing:border-box;
  }

  .favorite-post-table th:last-child,
  .favorite-post-table td:last-child {
      border-right: none;
  }

  .favorite-post-table tbody tr:hover{
      background:#f0f0f0;
  }

  .favorite-post-table .empty{
      color:#999999;
      text-align:center;
      padding:40px 0 40px 0;
  }

  .checkbox-cell {
    width: 32px;
    text-align: center;
  }

  .row-action {
    position: absolute;
    right: 8px;
    top: 50%;
    transform: translateY(-50%);
    z-index:10;
  }

  .btn-del {
    background: #ff4d4f;
    color: #fff;
    border: none;
    padding: 2px 2px;
    border-radius: 3px;
    cursor: pointer;
    font-size: 12px;
  }

  .btn-del:disabled {
    background: #ccc;
    cursor: not-allowed;
  }

  .btn-select {
    margin:4px 8px 4px 0;
  }
</style>
