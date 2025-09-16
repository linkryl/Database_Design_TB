<!--
高级搜索组件 - 提供全站搜索功能
TreeHole开发组
-->

<template>
  <div class="search-component">
    <!-- 搜索输入框 -->
    <div class="search-input-container">
      <el-autocomplete
        v-model="searchKeyword"
        :fetch-suggestions="fetchSuggestions"
        :trigger-on-focus="true"
        placeholder="搜索帖子、用户、贴吧..."
        class="search-input"
        @select="handleSuggestionSelect"
        @keyup.enter="performSearch"
        clearable
      >
        <template #prepend>
          <el-button :icon="Search" @click="performSearch" />
        </template>
        <template #suffix>
          <el-button 
            text 
            @click="showAdvancedSearch = !showAdvancedSearch"
            class="advanced-btn"
          >
            高级
          </el-button>
        </template>
        <template #default="{ item }">
          <div class="suggestion-item">
            <span class="suggestion-text">{{ item.value }}</span>
            <span class="suggestion-type">{{ item.type }}</span>
          </div>
        </template>
      </el-autocomplete>

      <!-- 热门搜索词 -->
      <div v-if="!searchKeyword && hotKeywords.length > 0" class="hot-keywords">
        <span class="hot-label">🔥 热门搜索：</span>
        <el-tag
          v-for="keyword in hotKeywords.slice(0, 8)"
          :key="keyword.keyword"
          class="hot-keyword-tag"
          @click="searchKeyword = keyword.keyword; performSearch()"
          effect="plain"
          size="small"
        >
          {{ keyword.keyword }}
        </el-tag>
      </div>
    </div>

    <!-- 高级搜索面板 -->
    <el-collapse-transition>
      <div v-show="showAdvancedSearch" class="advanced-search-panel">
        <el-form :model="advancedSearchForm" label-width="80px" size="small">
          <el-row :gutter="20">
            <el-col :span="8">
              <el-form-item label="搜索类型">
                <el-select v-model="advancedSearchForm.searchType" placeholder="选择类型">
                  <el-option label="全部" value="all" />
                  <el-option label="帖子" value="posts" />
                  <el-option label="用户" value="users" />
                  <el-option label="贴吧" value="bars" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="排序方式">
                <el-select v-model="advancedSearchForm.sortBy" placeholder="选择排序">
                  <el-option label="相关度" value="relevance" />
                  <el-option label="时间" value="time" />
                  <el-option label="热度" value="popularity" />
                  <el-option label="评论数" value="comments" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="帖子分类">
                <el-select v-model="advancedSearchForm.categoryId" placeholder="选择分类" clearable>
                  <el-option
                    v-for="category in categories"
                    :key="category.categoryId"
                    :label="category.category"
                    :value="category.categoryId"
                  />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row :gutter="20">
            <el-col :span="12">
              <el-form-item label="时间范围">
                <el-date-picker
                  v-model="advancedSearchForm.dateRange"
                  type="daterange"
                  range-separator="至"
                  start-placeholder="开始日期"
                  end-placeholder="结束日期"
                  format="YYYY-MM-DD"
                  value-format="YYYY-MM-DD"
                />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item>
                <el-button type="primary" @click="performAdvancedSearch">
                  高级搜索
                </el-button>
                <el-button @click="resetAdvancedSearch">重置</el-button>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </div>
    </el-collapse-transition>

    <!-- 搜索结果 -->
    <div v-if="showResults" class="search-results">
      <div class="results-header">
        <h3>搜索结果</h3>
        <span class="results-info">
          找到 {{ totalResults }} 个结果，用时 {{ searchTime }}ms
        </span>
      </div>

      <!-- 结果标签页 -->
      <el-tabs v-model="activeResultTab" @tab-change="handleTabChange">
        <el-tab-pane 
          :label="`全部 (${searchResults.total})`" 
          name="all"
          v-if="searchResults.total > 0"
        >
          <!-- 综合搜索结果 -->
          <div class="comprehensive-results">
            <!-- 帖子结果 -->
            <div v-if="searchResults.posts?.items?.length > 0" class="result-section">
              <h4 class="section-title">
                📝 相关帖子 
                <span class="section-count">({{ searchResults.posts.totalCount }})</span>
              </h4>
              <div class="post-results">
                <div
                  v-for="post in searchResults.posts.items"
                  :key="post.postId"
                  class="post-result-item"
                  @click="navigateToPost(post.postId)"
                >
                  <div class="post-title" v-html="highlightKeyword(post.title)"></div>
                  <div class="post-content" v-html="highlightKeyword(post.content)"></div>
                  <div class="post-meta">
                    <span class="author">{{ post.user.userName }}</span>
                    <span class="date">{{ formatDate(post.creationDate) }}</span>
                    <span class="stats">
                      👍 {{ post.likeCount }} 💬 {{ post.commentCount }}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <!-- 用户结果 -->
            <div v-if="searchResults.users?.length > 0" class="result-section">
              <h4 class="section-title">
                👤 相关用户 
                <span class="section-count">({{ searchResults.users.length }})</span>
              </h4>
              <div class="user-results">
                <div
                  v-for="user in searchResults.users"
                  :key="user.userId"
                  class="user-result-item"
                  @click="navigateToUser(user.userId)"
                >
                  <img :src="user.avatarUrl || '/images/default-avatar.png'" class="user-avatar">
                  <div class="user-info">
                    <div class="user-name" v-html="highlightKeyword(user.userName)"></div>
                    <div class="user-profile">{{ user.profile || '这个人很懒，什么都没写...' }}</div>
                    <div class="user-stats">
                      经验: {{ user.experiencePoints }} | 粉丝: {{ user.followedCount }}
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 贴吧结果 -->
            <div v-if="searchResults.bars?.length > 0" class="result-section">
              <h4 class="section-title">
                🏠 相关贴吧 
                <span class="section-count">({{ searchResults.bars.length }})</span>
              </h4>
              <div class="bar-results">
                <div
                  v-for="bar in searchResults.bars"
                  :key="bar.barId"
                  class="bar-result-item"
                  @click="navigateToBar(bar.barId)"
                >
                  <img :src="bar.avatarUrl || '/images/default-bar.png'" class="bar-avatar">
                  <div class="bar-info">
                    <div class="bar-name" v-html="highlightKeyword(bar.barName)"></div>
                    <div class="bar-description">{{ bar.description || '暂无简介' }}</div>
                    <div class="bar-stats">
                      关注: {{ bar.followedCount }} | 帖子: {{ bar.postCount }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </el-tab-pane>

        <el-tab-pane 
          :label="`帖子 (${postResults.length})`" 
          name="posts"
          v-if="postResults.length > 0"
        >
          <div class="post-results-detailed">
            <div
              v-for="post in postResults"
              :key="post.postId"
              class="post-result-card"
              @click="navigateToPost(post.postId)"
            >
              <div class="post-header">
                <img :src="post.user.avatarUrl || '/images/default-avatar.png'" class="author-avatar">
                <div class="post-meta">
                  <div class="author-name">{{ post.user.userName }}</div>
                  <div class="post-date">{{ formatDate(post.creationDate) }}</div>
                </div>
                <div class="post-category">{{ post.category.category }}</div>
              </div>
              <div class="post-title" v-html="highlightKeyword(post.title)"></div>
              <div class="post-content" v-html="highlightKeyword(post.content)"></div>
              <div class="post-stats">
                <span>👍 {{ post.likeCount }}</span>
                <span>💬 {{ post.commentCount }}</span>
                <span>⭐ {{ post.favoriteCount || 0 }}</span>
              </div>
            </div>
          </div>
        </el-tab-pane>
      </el-tabs>

      <!-- 加载更多 -->
      <div v-if="hasMoreResults" class="load-more-container">
        <el-button @click="loadMoreResults" :loading="loadingMore">
          加载更多结果
        </el-button>
      </div>

      <!-- 无结果提示 -->
      <div v-if="totalResults === 0 && !searching" class="no-results">
        <div class="no-results-icon">🔍</div>
        <div class="no-results-text">没有找到相关结果</div>
        <div class="no-results-suggestion">
          <p>建议：</p>
          <ul>
            <li>检查输入的关键词是否正确</li>
            <li>尝试使用更通用的关键词</li>
            <li>减少关键词数量</li>
          </ul>
        </div>
      </div>
    </div>

    <!-- 搜索中的加载状态 -->
    <div v-if="searching" class="search-loading">
      <el-skeleton :rows="5" animated />
    </div>
  </div>
</template>

<script setup lang='ts'>
import { ref, reactive, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Search } from '@element-plus/icons-vue'
import axios from 'axios'

// 路由
const router = useRouter()

// 响应式数据
const searchKeyword = ref('')
const showAdvancedSearch = ref(false)
const showResults = ref(false)
const searching = ref(false)
const loadingMore = ref(false)
const activeResultTab = ref('all')

// 搜索结果
const searchResults = ref<any>({})
const postResults = ref<any[]>([])
const totalResults = ref(0)
const searchTime = ref(0)
const hasMoreResults = ref(false)
const currentPage = ref(1)

// 热门搜索词
const hotKeywords = ref<any[]>([])

// 帖子分类
const categories = ref<any[]>([])

// 高级搜索表单
const advancedSearchForm = reactive({
  searchType: 'all',
  sortBy: 'relevance',
  categoryId: null,
  dateRange: null
})

// API基础URL
const apiBaseUrl = '/api'

// 计算属性
const totalResults = computed(() => {
  if (searchResults.value.total) {
    return searchResults.value.total
  }
  return postResults.value.length
})

// 组件挂载时初始化
onMounted(async () => {
  await loadHotKeywords()
  await loadCategories()
})

// 获取搜索建议
const fetchSuggestions = async (queryString: string, callback: Function) => {
  if (!queryString || queryString.length < 1) {
    callback([])
    return
  }

  try {
    const response = await axios.get(`${apiBaseUrl}/search/suggestions`, {
      params: { input: queryString }
    })
    
    const suggestions = response.data.map((item: string) => ({
      value: item,
      type: '建议'
    }))
    
    callback(suggestions)
  } catch (error) {
    console.error('获取搜索建议失败:', error)
    callback([])
  }
}

// 处理建议选择
const handleSuggestionSelect = (item: any) => {
  searchKeyword.value = item.value
  performSearch()
}

// 执行搜索
const performSearch = async () => {
  if (!searchKeyword.value.trim()) {
    ElMessage.warning('请输入搜索关键词')
    return
  }

  searching.value = true
  showResults.value = true
  currentPage.value = 1
  
  const startTime = Date.now()

  try {
    const response = await axios.get(`${apiBaseUrl}/search/comprehensive`, {
      params: {
        keyword: searchKeyword.value.trim(),
        page: 1,
        pageSize: 20,
        sortBy: advancedSearchForm.sortBy
      }
    })

    searchResults.value = response.data
    searchTime.value = Date.now() - startTime
    
    // 设置帖子结果用于帖子标签页
    postResults.value = response.data.posts?.items || []
    hasMoreResults.value = response.data.posts?.hasMore || false

  } catch (error: any) {
    console.error('搜索失败:', error)
    ElMessage.error('搜索失败，请稍后重试')
    searchResults.value = {}
    postResults.value = []
  } finally {
    searching.value = false
  }
}

// 执行高级搜索
const performAdvancedSearch = async () => {
  if (!searchKeyword.value.trim()) {
    ElMessage.warning('请输入搜索关键词')
    return
  }

  searching.value = true
  showResults.value = true
  currentPage.value = 1

  const startTime = Date.now()

  try {
    let url = `${apiBaseUrl}/search/`
    const params: any = {
      keyword: searchKeyword.value.trim(),
      page: 1,
      pageSize: 20,
      sortBy: advancedSearchForm.sortBy
    }

    // 根据搜索类型选择API端点
    switch (advancedSearchForm.searchType) {
      case 'posts':
        url += 'posts'
        if (advancedSearchForm.categoryId) {
          params.categoryId = advancedSearchForm.categoryId
        }
        if (advancedSearchForm.dateRange) {
          params.startDate = advancedSearchForm.dateRange[0]
          params.endDate = advancedSearchForm.dateRange[1]
        }
        break
      case 'users':
        url += 'users'
        break
      case 'bars':
        url += 'bars'
        break
      default:
        url += 'comprehensive'
    }

    const response = await axios.get(url, { params })

    if (advancedSearchForm.searchType === 'posts') {
      postResults.value = response.data.items || response.data
      searchResults.value = {
        posts: { items: postResults.value, totalCount: response.data.totalCount },
        total: response.data.totalCount
      }
    } else {
      searchResults.value = response.data
      postResults.value = response.data.posts?.items || []
    }

    searchTime.value = Date.now() - startTime
    hasMoreResults.value = response.data.hasMore || false

  } catch (error) {
    console.error('高级搜索失败:', error)
    ElMessage.error('搜索失败，请稍后重试')
  } finally {
    searching.value = false
  }
}

// 重置高级搜索
const resetAdvancedSearch = () => {
  advancedSearchForm.searchType = 'all'
  advancedSearchForm.sortBy = 'relevance'
  advancedSearchForm.categoryId = null
  advancedSearchForm.dateRange = null
}

// 标签页切换
const handleTabChange = (tabName: string) => {
  activeResultTab.value = tabName
}

// 加载更多结果
const loadMoreResults = async () => {
  if (!hasMoreResults.value || loadingMore.value) return

  loadingMore.value = true
  currentPage.value++

  try {
    const response = await axios.get(`${apiBaseUrl}/search/posts`, {
      params: {
        keyword: searchKeyword.value.trim(),
        page: currentPage.value,
        pageSize: 20,
        sortBy: advancedSearchForm.sortBy
      }
    })

    const newPosts = response.data.items || response.data
    postResults.value.push(...newPosts)
    hasMoreResults.value = response.data.hasMore || false

  } catch (error) {
    console.error('加载更多失败:', error)
    ElMessage.error('加载更多失败')
    currentPage.value-- // 恢复页码
  } finally {
    loadingMore.value = false
  }
}

// 加载热门搜索词
const loadHotKeywords = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/search/hot-keywords`, {
      params: { limit: 10 }
    })
    hotKeywords.value = response.data
  } catch (error) {
    console.error('加载热门搜索词失败:', error)
  }
}

// 加载帖子分类
const loadCategories = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/post-category`)
    categories.value = response.data
  } catch (error) {
    console.error('加载帖子分类失败:', error)
  }
}

// 高亮关键词
const highlightKeyword = (text: string) => {
  if (!searchKeyword.value || !text) return text
  
  const keywords = searchKeyword.value.trim().split(' ')
  let highlightedText = text
  
  keywords.forEach(keyword => {
    const regex = new RegExp(`(${keyword})`, 'gi')
    highlightedText = highlightedText.replace(regex, '<mark>$1</mark>')
  })
  
  return highlightedText
}

// 格式化日期
const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  if (diff < 86400000) { // 24小时内
    const hours = Math.floor(diff / 3600000)
    if (hours < 1) {
      const minutes = Math.floor(diff / 60000)
      return `${minutes}分钟前`
    }
    return `${hours}小时前`
  }
  
  return date.toLocaleDateString('zh-CN')
}

// 导航方法
const navigateToPost = (postId: number) => {
  router.push(`/PostPage/${postId}`)
}

const navigateToUser = (userId: number) => {
  router.push(`/ProfilePage/${userId}`)
}

const navigateToBar = (barId: number) => {
  router.push(`/BarPage/${barId}`)
}

// 暴露方法给父组件
defineExpose({
  performSearch,
  resetSearch: () => {
    searchKeyword.value = ''
    showResults.value = false
    searchResults.value = {}
    postResults.value = []
  }
})
</script>

<style scoped>
.search-component {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
}

.search-input-container {
  margin-bottom: 20px;
}

.search-input {
  width: 100%;
}

.search-input :deep(.el-input-group__prepend) {
  background-color: #409eff;
  border-color: #409eff;
  color: white;
}

.advanced-btn {
  color: #409eff;
  font-size: 12px;
}

.hot-keywords {
  margin-top: 10px;
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
}

.hot-label {
  font-size: 14px;
  color: #666;
  margin-right: 8px;
}

.hot-keyword-tag {
  cursor: pointer;
  transition: all 0.3s;
}

.hot-keyword-tag:hover {
  background-color: #409eff;
  color: white;
}

.advanced-search-panel {
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 20px;
}

.search-results {
  margin-top: 20px;
}

.results-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 10px;
  border-bottom: 1px solid #e9ecef;
}

.results-info {
  color: #666;
  font-size: 14px;
}

.comprehensive-results {
  space-y: 30px;
}

.result-section {
  margin-bottom: 30px;
}

.section-title {
  font-size: 18px;
  font-weight: 600;
  margin-bottom: 15px;
  color: #333;
}

.section-count {
  color: #666;
  font-weight: normal;
  font-size: 14px;
}

/* 帖子结果样式 */
.post-results {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.post-result-item, .post-result-card {
  background: white;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 16px;
  cursor: pointer;
  transition: all 0.3s;
}

.post-result-item:hover, .post-result-card:hover {
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  border-color: #409eff;
}

.post-title {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 8px;
  color: #333;
  line-height: 1.4;
}

.post-content {
  color: #666;
  line-height: 1.6;
  margin-bottom: 12px;
}

.post-meta, .post-stats {
  display: flex;
  gap: 15px;
  font-size: 12px;
  color: #999;
}

.post-header {
  display: flex;
  align-items: center;
  margin-bottom: 12px;
  gap: 12px;
}

.author-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.post-category {
  background: #e8f4fd;
  color: #409eff;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  margin-left: auto;
}

/* 用户结果样式 */
.user-results {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 15px;
}

.user-result-item {
  background: white;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 16px;
  display: flex;
  gap: 12px;
  cursor: pointer;
  transition: all 0.3s;
}

.user-result-item:hover {
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  border-color: #409eff;
}

.user-avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
}

.user-info {
  flex: 1;
}

.user-name {
  font-weight: 600;
  margin-bottom: 4px;
  color: #333;
}

.user-profile {
  color: #666;
  font-size: 14px;
  margin-bottom: 8px;
  line-height: 1.4;
}

.user-stats {
  font-size: 12px;
  color: #999;
}

/* 贴吧结果样式 */
.bar-results {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 15px;
}

.bar-result-item {
  background: white;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 16px;
  display: flex;
  gap: 12px;
  cursor: pointer;
  transition: all 0.3s;
}

.bar-result-item:hover {
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  border-color: #409eff;
}

.bar-avatar {
  width: 48px;
  height: 48px;
  border-radius: 8px;
}

.bar-info {
  flex: 1;
}

.bar-name {
  font-weight: 600;
  margin-bottom: 4px;
  color: #333;
}

.bar-description {
  color: #666;
  font-size: 14px;
  margin-bottom: 8px;
  line-height: 1.4;
}

.bar-stats {
  font-size: 12px;
  color: #999;
}

/* 建议项样式 */
.suggestion-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.suggestion-text {
  flex: 1;
}

.suggestion-type {
  font-size: 12px;
  color: #999;
  background: #f0f0f0;
  padding: 2px 6px;
  border-radius: 4px;
}

/* 高亮样式 */
:deep(mark) {
  background-color: #fff3cd;
  color: #856404;
  padding: 0 2px;
  border-radius: 2px;
}

/* 加载更多 */
.load-more-container {
  text-align: center;
  margin: 30px 0;
}

/* 无结果提示 */
.no-results {
  text-align: center;
  padding: 40px 20px;
  color: #666;
}

.no-results-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.no-results-text {
  font-size: 18px;
  margin-bottom: 20px;
}

.no-results-suggestion {
  text-align: left;
  max-width: 400px;
  margin: 0 auto;
}

.no-results-suggestion ul {
  padding-left: 20px;
}

.no-results-suggestion li {
  margin-bottom: 8px;
  line-height: 1.5;
}

/* 搜索加载状态 */
.search-loading {
  margin: 20px 0;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .results-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
  
  .user-results, .bar-results {
    grid-template-columns: 1fr;
  }
  
  .post-meta, .post-stats {
    flex-wrap: wrap;
    gap: 10px;
  }
  
  .advanced-search-panel {
    padding: 15px;
  }
  
  .advanced-search-panel .el-row {
    margin: 0;
  }
  
  .advanced-search-panel .el-col {
    margin-bottom: 15px;
  }
}
</style>
