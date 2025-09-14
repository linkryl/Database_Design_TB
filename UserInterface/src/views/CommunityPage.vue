<!--
 
社区页面
2351134 吕奎辰

-->

<template>
  <!-- 社区公告 -->
  <div class='harmony-notice-banner'>
    <div class='harmony-notice-content'>
      <div class='harmony-notice-icon'>🤝</div>
      <div class='harmony-notice-text'>
        <div class='harmony-notice-title'>和谐社区</div>
        <div class='harmony-notice-subtitle'>共同打造和谐社区</div>
        <div class='harmony-notice-description'>为了维护社区的秩序和氛围，请在发帖时遵守以下准则</div>
        <div class='harmony-notice-rules'>
          <span class='rule-item'>尊重他人</span>
          <span class='rule-item'>内容合规</span>
          <span class='rule-item'>版权意识</span>
          <span class='rule-item'>信息真实</span>
        </div>
      </div>
    </div>
  </div>

  <!-- 发帖按钮 -->
  <el-button class='floating-publish-button' round @click='publishPost'>
    我要发帖
  </el-button>

  <div class='background-container'>
    <h1>TreeHole树洞</h1>
  </div>

  <div class='page-container'>
    <!-- 帖子列表区域 -->
    <div class='posts-section'>
      
      <!-- 无帖子时的空状态提示 -->
      <div v-if='postIds.length === 0' class='empty-posts-container'>
        <div class='empty-posts-content'>
          <div class='empty-posts-icon'>📝</div>
          <div class='empty-posts-title'>当前还没有帖子</div>
          <div class='empty-posts-subtitle'>快来发帖试试吧</div>
          <el-button type='primary' size='large' @click='publishPost' class='empty-posts-button'>
            立即发帖
          </el-button>
        </div>
      </div>
      
      <!-- 有帖子时显示帖子列表 -->
      <template v-else>
        <PostDetailCard v-for='postId in paginatedPostIds' :key='postId' :post-id='postId' />
        
        <!-- 最后一页的结束提示 -->
        <div v-if='isLastPage && postIds.length > 0' class='end-posts-container'>
          <div class='end-posts-content'>
            <div class='end-posts-icon'>🏁</div>
            <div class='end-posts-text'>再往后就没有啦</div>
          </div>
        </div>
        
        <!-- 分页控件 -->
        <div class='pagination-container'>
          <el-pagination @current-change='handleCurrentChange'
                         :current-page='currentPage'
                         :page-size='pageSize'
                         layout='prev, pager, next'
                         :total='totalPosts'>
          </el-pagination>
        </div>
      </template>
    </div>
  </div>

</template>

<script setup lang='ts'>
import {ref, computed, onMounted} from 'vue'
import {useRouter} from 'vue-router'
import axiosInstance from '../utils/axios'
import {ElMessage} from 'element-plus'
import PostDetailCard from '../components/PostDetailCard.vue'


const router = useRouter()
const currentPage = ref(1)
const pageSize = ref(10)
const totalPosts = computed(() => postIds.value.length)
const postIds = ref([])

// 判断是否为最后一页
const isLastPage = computed(() => {
  return currentPage.value >= Math.ceil(totalPosts.value / pageSize.value)
})
// 响应式获取当前用户ID
const currentUserId = computed(() => {
  const storedValue = localStorage.getItem('currentUserId')
  const storedUserId = storedValue ? parseInt(storedValue) : 0
  return isNaN(storedUserId) ? 0 : storedUserId
})

const paginatedPostIds = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return postIds.value.slice(start, end)
})

const handleCurrentChange = (page) => {
  currentPage.value = page
}



onMounted(async () => {
  try {
    console.log('正在请求API:', '/api/post/latest-ids')
    const response = await axiosInstance.get('post/latest-ids')
    
    console.log('API响应状态:', response.status)
    console.log('后端返回的帖子数据:', response.data)
    console.log('数据类型:', typeof response.data)
    console.log('数据长度:', response.data?.length)
    console.log('前5个帖子ID:', response.data?.slice(0, 5))
    
    // 后端 /post/latest-ids 接口应该直接返回按时间排序的帖子ID数组
    const allPostIds = response.data || []
    console.log('获取到的帖子ID列表:', allPostIds.slice(0, 10))
    
    // 验证每个帖子ID是否有效（可选，但会增加请求次数）
    console.log('开始验证帖子ID的有效性...')
    const validPostIds = []
    
    for (const postId of allPostIds) {
      try {
        // 快速验证帖子是否存在
        const testResponse = await axiosInstance.get(`post/${postId}`)
        if (testResponse.status === 200) {
          validPostIds.push(postId)
          console.log(`帖子ID ${postId} 验证成功`)
        }
      } catch (error) {
        console.warn(`帖子ID ${postId} 验证失败:`, error.response?.status)
        // 继续处理下一个ID，不中断整个流程
      }
    }
    
    postIds.value = validPostIds
    console.log(`验证完成，有效帖子数量: ${validPostIds.length}/${allPostIds.length}`)
    console.log('有效的帖子ID列表:', validPostIds.slice(0, 10))
    
  } catch (error) {
    console.error('获取帖子列表失败:', error)
    console.error('错误详情:', error.response?.data)
    console.error('错误状态码:', error.response?.status)
    console.error('完整错误响应:', error.response)
    
    if (error.response?.status === 500) {
      ElMessage.error('后端服务器内部错误(500)，请检查后端服务是否正常运行')
    } else if (error.response?.status === 404) {
      ElMessage.error('API接口不存在(404)，请确认后端是否已实现 /post/latest-ids 接口')
    } else {
      ElMessage.error(`GET 请求失败: ${error.message}`)
    }
  }
})

// 发帖按钮点击事件 - 跳转到发帖页面
function publishPost() {
  if (currentUserId.value && currentUserId.value != 0) {
    router.push('/PostNew')
  } else {
    ElMessage.warning('请先进行登录！')
  }
}

// 刷新帖子列表的函数
async function refreshPosts() {
  try {
    const response = await axiosInstance.get('post/latest')
    // 后端 /post/latest 接口应该直接返回按时间排序的帖子ID数组
    postIds.value = response.data || []
    currentPage.value = 1 // 重置到第一页
  } catch (error) {
    ElMessage.error('GET 请求失败，请检查网络连接情况或稍后重试。')
  }
}
</script>

<style scoped>
:global(:root) {
  --community-background-image: linear-gradient(rgba(255, 255, 255, 0.2), rgba(255, 255, 255, 0.2)), url('[TODO: ossBaseUrl]BackgroundImage.jpg');
  --community-title-color: #393B9C;
  --community-title-shadow-color: #787ACF;
  --community-img-title-color: #FFFFFF;
  --community-categoty-bg-color: #FFF0F0;
  --community-notice-bg-color: #D8D9E8;
  --community-notice-head-color: #373F9E;
  --community-card-color: #373F9E;
  --community-card-text-color: #64616C;
  --community-card-bg-color: #FFFFFF;
  --community-publish-post-button-text: #F1EAFF;
  --community-publish-post-button: #393B9C;
  --community-publish-post-button-hover: #7F71D0;
  --community-publish-post-button-active: #46328A;
  --community-img-filter: brightness(100%);
}

/* noinspection CssUnusedSymbol */
:global(.dark) {
  --community-background-image: linear-gradient(rgba(0, 0, 0, 0.2), rgba(0, 0, 0, 0.2)), url('[TODO: ossBaseUrl]BackgroundImage.jpg');
  --community-title-color: #E4DBFF;
  --community-title-shadow-color: #473B7E;
  --community-img-title-color: #E0E0E0;
  --community-categoty-bg-color: #391A2D;
  --community-notice-bg-color: #232535;
  --community-notice-head-color: #ACD1FF;
  --community-card-color: #E5E7FF;
  --community-card-text-color: #DDDDE1;
  --community-card-bg-color: #3E3E57;
  --community-publish-post-button-text: #46328A;
  --community-publish-post-button: #C3AFFF;
  --community-publish-post-button-hover: #D3C8FF;
  --community-publish-post-button-active: #9777FF;
  --community-img-filter: brightness(80%);
}

.page-container {
  width: 1200px;
  max-width: 100%;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  margin-top: -20px;
}

h1 {
  width: 100%;
  max-width: 800px;
  position: relative;
  margin: -100px auto 0;
  padding: 5px 20px 10px;
  font-size: 48px;
  font-weight: bold;
  color: var(--community-title-color);
  text-align: center;
  text-shadow: 0 2px 4px var(--community-title-shadow-color);
  z-index: 10;
}


.background-container {
  min-height: 300px;
  width: 100%;
  background-image: var(--community-background-image);
  background-size: cover;
  background-position: center;
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 20px 0;
}


/* 帖子列表区域样式 */
.posts-section {
  max-width: 1800px;
  margin: 0 auto;
  padding: 0 20px 20px;
  margin-top: -10px;
}

/* 空状态提示样式 */
.empty-posts-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 400px;
  padding: 40px 20px;
}

.empty-posts-content {
  text-align: center;
  max-width: 400px;
}

.empty-posts-icon {
  font-size: 64px;
  margin-bottom: 20px;
  opacity: 0.6;
}

.empty-posts-title {
  font-size: 24px;
  font-weight: bold;
  color: #666;
  margin-bottom: 10px;
}

.empty-posts-subtitle {
  font-size: 16px;
  color: #999;
  margin-bottom: 30px;
}

.empty-posts-button {
  padding: 12px 32px;
  font-size: 16px;
}

/* 结束提示样式 */
.end-posts-container {
  display: flex;
  justify-content: center;
  margin: 40px 0;
  padding: 20px;
}

.end-posts-content {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 15px 25px;
  background-color: #f5f5f5;
  border-radius: 25px;
  border: 1px solid #e0e0e0;
}

.end-posts-icon {
  font-size: 20px;
}

.end-posts-text {
  font-size: 14px;
  color: #666;
  font-style: italic;
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 30px;
  margin-bottom: 30px;
}

.title {
  color: var(--community-img-title-color);
  font-size: 18px;
  text-align: center;
  position: relative;
  top: 40%;
  justify-content: space-between;
}


/* 我要发帖按钮 */
.floating-publish-button {
  position: fixed;
  top: 15px;
  right: 100px;
  width: 120px;
  height: 40px;
  font-size: 16px;
  font-weight: bold;
  color: var(--community-publish-post-button-text);
  background-color: var(--community-publish-post-button);
  border: none;
  border-radius: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  transition: all 0.3s ease;
  z-index: 1000;
}

.floating-publish-button:hover {
  background-color: var(--community-publish-post-button-hover);
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.4);
}

.floating-publish-button:active {
  background-color: var(--community-publish-post-button-active);
  transform: translateY(0);
}

/* 横版和谐社区公告 */
.harmony-notice-banner {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 25px 0;
  margin: 0;
  position: sticky;
  top: 0;
  z-index: 100;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.harmony-notice-content {
  max-width: 800px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  padding: 0 20px;
}

.harmony-notice-icon {
  font-size: 40px;
  margin-right: 20px;
  flex-shrink: 0;
}

.harmony-notice-text {
  flex: 1;
}

.harmony-notice-title {
  font-size: 24px;
  font-weight: bold;
  margin-bottom: 8px;
}

.harmony-notice-subtitle {
  font-size: 16px;
  opacity: 0.9;
  margin-bottom: 8px;
}

.harmony-notice-description {
  font-size: 14px;
  opacity: 0.8;
  line-height: 1.4;
  margin-bottom: 10px;
}

.harmony-notice-rules {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  margin-top: 10px;
}

.rule-item {
  background-color: rgba(255, 255, 255, 0.2);
  padding: 4px 12px;
  border-radius: 15px;
  font-size: 12px;
  font-weight: 500;
  border: 1px solid rgba(255, 255, 255, 0.3);
  transition: all 0.3s ease;
}

.rule-item:hover {
  background-color: rgba(255, 255, 255, 0.3);
  transform: translateY(-1px);
}


.horizontal-container {
  display: flex;
  justify-content: space-between;
  margin: 20px 0 10px;
}

.card-container {
  flex: 1;
  margin: 20px 10px 0;
  height: 360px;
  padding: 20px;
  background-color: var(--community-card-bg-color);
  text-align: center;
  border-radius: 10px;
  box-shadow: 0 1px 35px 0 rgba(0, 0, 0, 0.1);
}

.card-container:first-child {
  margin-left: 50px;
}

.card-container:last-child {
  margin-right: 50px;
}

.card-container > * {
  margin-bottom: 10px;
}

.card-container > *:last-child {
  margin-bottom: 25px;
}

.notice-container {
  background-color: var(--community-notice-bg-color);
  padding: 20px 12px;
  margin-bottom: 5px;
  margin-top: 25px;
  border-radius: 20px;
}

.centered-header {
  text-align: center;
  line-height: 1.5;
  margin-top: -20px;
}

.header-container {
  display: flex;
  align-items: center;
  justify-content: center;
}

.card-icon {
  font-size: 25px;
}

.header-icon {
  font-size: 30px;
  color: var(--community-notice-head-color);
  font-weight: 900;
  margin: 0 8px;
}

.large-purple-text {
  font-size: 35px;
  color: var(--community-notice-head-color);
  font-weight: bold;
  margin-top: 10px;
}

.card-icon-text {
  font-size: 18px;
  color: var(--community-card-color);
}

.small-text {
  font-size: 16px;
  font-weight: bold;
}

.circular-image {
  width: 90px;
  height: 90px;
  border-radius: 50%;
  object-fit: cover;
  display: block;
  margin: 0 auto 10px;
}

.card-head {
  font-size: 25px;
  color: var(--community-card-color);
  font-weight: bold;
}

.card-text {
  font-size: 15px;
  color: var(--community-card-text-color);
  text-align: center;
  margin-top: 20px;
  margin-bottom: 20px;
  line-height: 1.5;
}

.card-small-text {
  color: var(--community-card-color);
  text-align: center;
  font-size: 15px;
  margin-top: -2px;
  font-weight: 550;
}

/* 帖子详情卡片容器样式 */
.post-detail-card {
  margin-bottom: 20px;
}
</style>
