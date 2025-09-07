<template>
  <div class="chat-wrapper">
    <div class="chat-app">
      <aside class="sidebar">
        <div class="mode-header">
          {{mode === "private" ? "私 信" : "群 聊"}}
        </div>

        <div class="mode-switch">
          <button v-bind:class="{active:mode==='private'}" @click="mode='private'">
            <span class="icon">✉</span> 私信
          </button>

          <button v-bind:class="{active:mode==='group'}" @click="mode='group'">
            <span class="icon icon-white">👥</span> 群组
          </button>
        </div>

        <div class="contact-list" ref="contactList">
          <div
               class="contact-item"
               v-for="(item,index) in filteredList"
               v-bind:key="index"
               v-bind:class="{active:activeIndex===index}"
               @click="selectContact(item,index)"
          >
            <img v-bind:src="item.avatar" class="avatar" alt="头像"/>
            <span class="name">{{ellipsis(item.name,7)}}</span>
            <span v-if="item.unread" class="unread-dot"></span>
          </div>
        </div>
      </aside>

      <main class="main-panel">
        <header class="toolbar">
          <button class="btn-mark-all" @click="markAllRead">全部标为已读</button>
          <button class="btn-setting" @click="showSetting=!showSetting">⚙️</button>
        </header>

        <section class="chat-area">
          <div class="chat-header">
            {{activeName}}
          </div>

          <div class="message-list" ref="messageList">
            <div
                 class="message"
                 v-for="(msg,msgIndex) in messages"
                 v-bind:key="msgIndex"
                 v-bind:class="{self:msg.speaker===username}"
            >
              <img v-bind:src="msg.avatar" class="avatar"/>
              <div class="bubble" v-bind:class="msg.bubbleType">
                {{msg.content}}
              </div>
            </div>
          </div>
        </section>

        <footer v-if="activeIndex>=0" class="input-area">
          <div class="toolbar-row">一条消息不超过 250 个字符：{{inputText.length}} / 250</div>
          <textarea v-model="inputText" placeholder="输入内容..." maxlength="250"></textarea>
          <button class="send-btn" @click="send">发 送</button>
        </footer>
      </main>

      <!--设置弹窗占位 TODO：-->
      <div v-if="showSetting" class="setting-dialog">
        设置
      </div>
    </div>
  </div>
</template>

<script>
  export default {
    data() {
      return {
        username:"_h_our",//我的用户名
        mode: "private",
        activeIndex: -1,//初始化为-1 表示没有选择任何一个聊天
        privateList:[//测试数据
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/55", name: "_h_our", unread: true },
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/50", name: "jiwndwined", unread: true },
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/53", name: "sienf", unread: true },
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/59", name: "dsineeddv", unread: false },
        ],
        groupList: [//测试数据
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/55", name: "group_h_our", unread: true },
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/54", name: "group_j", unread: false },
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/52", name: "group_s", unread: true },
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/51", name: "group_d", unread: true },
        ],
        messages: [//测试数据 这仅仅只是一个聊天的消息
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/55", speaker: "_h_our", content:"你好",time:"2025-09-01 20:10",bubbleType:"blue" },
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/50", speaker: "jiwndwined", content: "你好好", time: "2025-09-01 20:10", bubbleType:"white" },
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/55", speaker: "_h_our", content: "你好好好", time: "2025-09-01 20:10", bubbleType:"blue" },
          { avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/50", speaker: "jiwndwined", content: "你好好好好", time: "2025-09-01 20:10", bubbleType: "white" },
        ],

        inputText: "",
        showSetting: false
      }
    },
    computed: {
      filteredList() {
        return this.mode === "private" ? this.privateList : this.groupList;
      },
      activeName() {
        return this.activeIndex>= 0 ? this.filteredList[this.activeIndex].name : "请选择聊天";
      }
    },
    watch: {
      mode() {
        this.activeIndex = -1;
        this.messages = [];
      }
    },
    methods: {
      selectContact(item,contactIndex) {
        this.activeIndex = contactIndex;
        item.unread = false;
        /* TODO：调用后端接口拉取历史消息、可能需要后端标记为已读 */
        //this.fetchMessages(contactIndex);
      },
      ellipsis(str,max) {
        if (!str) return "";
        //保留 max 个字符
        return str.length > max ? str.slice(0, max) + "..." : str;
      },
      markAllRead() {
        this.filteredList.forEach((item) => { item.unread = false });
        /* TODO：可能需要调用后端接口 */
      },
      send() {
        if (!this.inputText.trim() || this.activateIndex === -1) return;
        const newMsg = {
          avatar: "https://himg.bdimg.com/sys/portrait/hotitem/wildkid/55",//TODO：从后端获取到 username 对应的头像
          speaker: this.username,
          content: this.inputText.trim(),
          time: Date.now(),
          bubbleType: "blue",//TODO：从后端获取到 username 的气泡类型
        };
        this.messages.push(newMsg);
        this.inputText = "";
        this.$nextTick(() => {
          this.$refs.messageList.scrollTop = this.$refs.messageList.scrollHeight;
        });
        /* TODO：调后端接口发送消息 */
      },
      fetchMessages(contactIndex) {
        /* TODO：实现：根据选择的聊天对象 activeIndex 加载这个聊天的所有消息 */
      }
    }
  }
</script>

<style scoped>
  .chat-wrapper{
      display:flex;
      height:97vh;
      width:100%;
      box-sizing:border-box;
      background:#f5f5f5;
      justify-content:center;
      font-family:Arial;
  }

  .chat-app{
      position:relative;
      margin:auto 0 auto 0;
      background:#ffffff;
      max-width:1000px;
      height:80vh;
      width:100%;
      box-sizing:border-box;
      display:flex;
      border:2px solid #dddddd;
  }

  .sidebar{
      width:20%;
      height:100%;
      box-sizing:border-box;
      background:#ffffff;
      display:flex;
      flex-direction:column;
      border-right:2px solid #dddddd;
  }

  .mode-header{
      padding:8px 10px 8px 10px;
      text-align:center;
      background:#4ba8fe;
      color:#ffffff;
      font-weight:bold;
      font-size:18px;
  }

  .mode-switch{
      display:flex;
      flex-direction:row;
  }

  .mode-switch button{
      flex:1;
      padding:8px 10px 8px 10px;
      text-align:center;
      border:none;
      border-bottom:3px solid #414a73;
      background:#414a73;
      cursor:pointer;
      font-size:16px;
      font-weight:bold;
      color:#ffffff;
  }

  .mode-switch button.active{
      border-bottom:3px solid #ff6a00;
  }

  .icon-white {
      filter: grayscale(1) brightness(0) invert(1);
  }

  .contact-list{
      flex:1;
      overflow-y:auto;
  }

  .contact-item{
      display:flex;
      align-items:center;
      padding:8px 10px 8px 10px;
      cursor:pointer;
      position:relative;
  }

  .contact-item:hover{
      background:#e6e6e6;
  }

  .contact-item.active{
      background:#d5e7ff;
  }

  .contact-item .avatar{
      width:40px;
      height:40px;
      border-radius:50%;
      margin-right:10px;
  }

  .contact-item .name{
      text-align:left;
      flex:1;
  }

  .unread-dot{
      position:absolute;
      right:4px;
      top:4px;
      width:10px;
      height:10px;
      background:red;
      border-radius:50%;
  }

  .main-panel{
      flex:1;
      display:flex;
      flex-direction:column;
  }

  .toolbar{
      display:flex;
      justify-content:space-between;
      padding:7px 16px 7px 16px;
      border-bottom:2px solid #dddddd;
  }

  .toolbar .btn-mark-all{
      color:#ffffff;
      background:#4ba8fe;
      cursor:pointer;
      font-size:14px;
      font-weight:bold;
      padding:2px 10px 2px 10px;
      border:none;
      border-radius:4px;
  }

  .toolbar .btn-mark-all:hover {
      background:#4b96fe;
  }

  .btn-setting{
      font-size:16px;
      background:#e5e5e5;
      cursor:pointer;
      border-radius:4px;
      border-color:transparent;
  }

  .btn-setting:hover{
      background:#d5d5d5;
  }

  .chat-area{
      flex:1;
      display:flex;
      flex-direction:column;
      overflow:hidden;
  }

  .chat-header{
      padding:8px 10px 8px 10px;
      background:#ffffff;
      border-bottom:2px solid #dddddd;
      font-weight:bold;
      font-size:16px;
      height:22px;
      text-align:center;
  }

  .message-list{
      flex:1;
      background:#ffffff;
      overflow-y:auto;
      padding:8px 10px 8px 10px;
  }

  .message{
      display:flex;
      align-items:flex-start;
      margin:0 0 8px 0;
  }

  .message.self{
      flex-direction:row-reverse;
  }

  .message .avatar{
      width:32px;
      height:32px;
      border-radius:50%;
      margin:0 10px 0 10px;
  }

  .bubble{
      max-width:50%;
      box-sizing:border-box;
      padding:8px 10px 8px 10px;
      border-radius:6px;
      word-break:break-all;
      font-size:14px;
  }

  .bubble.blue{
      background:#1890ff;
      color:#ffffff;
  }

  .bubble.white{
      background:#ffffff;
      border:1px solid #dddddd;
      color:#000000;
  }

  .input-area{
      border-top:2px solid #dddddd;
      background:#ffffff;
      position:relative;
  }

  .input-area .toolbar-row {
      background: #273058;
      border-bottom: 2px solid #dddddd;
      font-size:12px;
      color:#ffffff;
      padding:3px 0 3px 10px;
  }

  .input-area textarea{
      width:100%;
      box-sizing:border-box;
      font-size:16px;
      resize:none;
      min-height:120px;
      padding:8px 10px 8px 10px;
      border:none;
      outline:none;
  }

  .input-area .send-btn {
      position:absolute;
      right:20px;
      bottom:8px;
      padding: 6px 10px 6px 10px;
      background: #4ba8fe;
      color: #ffffff;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      margin:0 0 0 0;
      font-weight:bold;
      word-break:break-all;
  }

  .input-area .send-btn:hover{
      background:#4b96fe;
  }

  .setting-dialog{
      width:100px;
      height:200px;
      border-radius:6px;
      padding:4px 8px 4px 8px;
      background:rgba(0,0,0,0.8);
      color:#ffffff;
      position:absolute;
      top:38px;
      right:10px;
  }
</style>
