<!--
TreeHole 发帖页面
2351270 王天一
-->

<template>
  <div class="th-post-edit-container">
    <!-- 添加背景图片 -->
    <img :src="`${ossBaseUrl}BackgroundImage.jpg`" alt="Background" class="th-background-image">
    
    <el-card class="th-post-edit-card">
      <template #header>
        <div class="th-card-header">
          <h2>📝 发表树洞</h2>
          <span class="th-subtitle">分享你的心声</span>
        </div>
      </template>
      
      <el-form 
        :model="thPostForm" 
        :rules="thFormRules" 
        ref="thFormRef"
        label-position="top" 
        class="th-post-form"
      >
        <!-- 发布位置选择 -->
        <el-form-item label="发布位置" prop="publishType">
          <el-radio-group v-model="thPostForm.publishType" :disabled="thLoading">
            <el-radio value="treehole">🕳️ 树洞社区</el-radio>
            <el-radio value="bar">🏠 贴吧</el-radio>
          </el-radio-group>
        </el-form-item>

        <!-- 贴吧选择（仅当选择贴吧时显示） -->
        <el-form-item 
          v-if="thPostForm.publishType === 'bar'" 
          label="选择贴吧" 
          prop="barId"
        >
          <el-select 
            v-model="thPostForm.barId" 
            placeholder="选择要发布到的贴吧..."
            style="width: 100%"
            :disabled="thLoading"
            filterable
          >
            <el-option
              v-for="bar in availableBars"
              :key="bar.barId"
              :value="bar.barId"
              :label="`${bar.barName}${bar.isOwner ? ' (我的贴吧)' : ''}`"
            >
              <div class="bar-option">
                <span class="bar-name">{{ bar.barName }}</span>
                <span class="bar-followers">{{ bar.followedCount }} 关注</span>
              </div>
            </el-option>
          </el-select>
        </el-form-item>

        <!-- 跨发布选项（仅当选择贴吧时显示） -->
        <el-form-item v-if="thPostForm.publishType === 'bar' && thPostForm.barId">
          <el-checkbox v-model="thPostForm.alsoInTreehole" :disabled="thLoading">
            <span class="cross-publish-label">
              <span class="checkbox-icon">🔗</span>
              同时在树洞社区显示
            </span>
          </el-checkbox>
          <div class="form-hint">勾选后，帖子会在贴吧和树洞社区同时显示，树洞中会标注来源贴吧</div>
        </el-form-item>

        <!-- 帖子分类选择 -->
        <el-form-item label="帖子分类" prop="categoryId">
          <el-select 
            v-model="thPostForm.categoryId" 
            placeholder="选择帖子分类..."
            style="width: 100%"
            :disabled="thLoading"
          >
            <el-option
              v-for="category in availableCategories"
              :key="category.categoryId"
              :value="category.categoryId"
              :label="category.category"
            >
              <div class="category-option">
                <span class="category-icon">📂</span>
                <span class="category-name">{{ category.category }}</span>
              </div>
            </el-option>
          </el-select>
          <div class="form-hint">选择合适的分类有助于其他用户发现你的帖子</div>
        </el-form-item>

        <el-form-item label="标题" prop="title">
          <el-input 
            v-model="thPostForm.title" 
            placeholder="给你的树洞起个标题吧..."
            maxlength="100"
            show-word-limit
            :disabled="thLoading"
          />
        </el-form-item>
        
        <el-form-item label="分类" prop="categoryId">
          <el-select 
            v-model="thPostForm.categoryId" 
            placeholder="请选择帖子分类"
            :disabled="thLoading"
            style="width: 100%"
          >
            <el-option
              v-for="category in thCategories"
              :key="category.categoryId"
              :label="category.category"
              :value="category.categoryId"
            />
          </el-select>
        </el-form-item>
        
        <el-form-item label="内容" prop="content">
          <el-input 
            v-model="thPostForm.content" 
            type="textarea" 
            :rows="8"
            placeholder="说出你的心里话..."
            maxlength="2000"
            show-word-limit
            :disabled="thLoading"
          />
        </el-form-item>
        
        <el-form-item>
          <div class="th-form-actions">
            <el-button @click="handleCancel" :disabled="thLoading">
              取消
            </el-button>
            <el-button 
              type="primary" 
              @click="handleSubmit" 
              :loading="thLoading"
            >
              {{ thLoading ? '发布中...' : '发布树洞' }}
            </el-button>
          </div>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { createPost, getAllBars, getUserFollowedBars, getBarsByOwnerId, getAllPostCategories, getPostCategories, type THPostCategory } from '@/api/index'
import { getCurrentUserId } from '@/utils/auth'
import { ossBaseUrl } from '@/globals'
import axiosInstance from '@/utils/axios'

// TreeHole前缀命名
const router = useRouter()
const route = useRoute()
const thFormRef = ref<FormInstance>()
const thLoading = ref(false)
const thCategories = ref<THPostCategory[]>([])

// 表单数据
const thPostForm = reactive({
  title: '',
<<<<<<< HEAD
  content: '',
  publishType: 'treehole' as 'treehole' | 'bar', // 发布位置：树洞或贴吧
  barId: null as number | null, // 选择的贴吧ID
  alsoInTreehole: false, // 是否同时在树洞显示
  categoryId: null as number | null // 帖子分类ID
=======
  categoryId: 0,
  content: ''
>>>>>>> origin/main
})

// 贴吧和分类相关数据
const availableBars = ref<any[]>([])
const availableCategories = ref<any[]>([])

// 表单验证规则
const thFormRules: FormRules = {
  publishType: [
    { required: true, message: '请选择发布位置', trigger: 'change' }
  ],
  barId: [
    { 
      validator: (rule, value, callback) => {
        if (thPostForm.publishType === 'bar' && !value) {
          callback(new Error('选择贴吧发布时必须选择具体贴吧'))
        } else {
          callback()
        }
      }, 
      trigger: 'change' 
    }
  ],
  categoryId: [
    { required: true, message: '请选择帖子分类', trigger: 'change' }
  ],
  title: [
    { required: true, message: '请输入标题', trigger: 'blur' },
    { max: 100, message: '标题长度不能超过100个字符', trigger: 'blur' }
  ],
  categoryId: [
    { required: true, message: '请选择帖子分类', trigger: 'change' }
  ],
  content: [
    { required: true, message: '请输入内容', trigger: 'blur' }
  ]
}

<<<<<<< HEAD
// 加载用户可发帖的贴吧列表
const loadAvailableBars = async () => {
  const thCurrentUserId = getCurrentUserId()
  if (!thCurrentUserId) return
  
  try {
    const userId = parseInt(thCurrentUserId)
    
    // 获取用户创建的贴吧和关注的贴吧
    const [ownedBars, followedBars] = await Promise.allSettled([
      getBarsByOwnerId(userId),
      getUserFollowedBars(userId)
    ])
    
    const ownedList = ownedBars.status === 'fulfilled' ? ownedBars.value : []
    const followedList = followedBars.status === 'fulfilled' ? followedBars.value : []
    
    // 合并贴吧列表，标记是否为吧主
    const allBars = new Map()
    
    // 添加自己创建的贴吧
    ownedList.forEach((bar: any) => {
      allBars.set(bar.barId, { ...bar, isOwner: true })
    })
    
    // 添加关注的贴吧
    followedList.forEach((bar: any) => {
      if (!allBars.has(bar.barId)) {
        allBars.set(bar.barId, { ...bar, isOwner: false })
      }
    })
    
    availableBars.value = Array.from(allBars.values())
    console.log('可发帖的贴吧列表:', availableBars.value)
    
    // 检查URL参数，如果有指定贴吧则预选
    const urlBarId = route.query.barId
    const urlBarName = route.query.barName
    
    if (urlBarId) {
      const preselectedBarId = parseInt(urlBarId as string)
      if (availableBars.value.some(bar => bar.barId === preselectedBarId)) {
        // 自动选择贴吧发布模式
        thPostForm.publishType = 'bar'
        thPostForm.barId = preselectedBarId
        console.log('🎯 预选贴吧发布模式，贴吧:', urlBarName, 'ID:', preselectedBarId)
        ElMessage.info(`将发布到贴吧：${urlBarName}`)
      } else {
        console.warn('⚠️ URL中指定的贴吧不在可选列表中:', preselectedBarId)
      }
    }
  } catch (error) {
    console.error('加载贴吧列表失败:', error)
    // 失败时允许发到树洞社区
    availableBars.value = []
  }
}

// 加载可用的帖子分类
const loadAvailableCategories = async () => {
  try {
    const categories = await getAllPostCategories()
    availableCategories.value = categories || []
    
    // 如果有分类数据，默认选择第一个
    if (availableCategories.value.length > 0 && !thPostForm.categoryId) {
      thPostForm.categoryId = availableCategories.value[0].categoryId
      console.log('✅ 默认选择分类:', availableCategories.value[0].category)
    }
    
    console.log('可用分类列表:', availableCategories.value)
  } catch (error) {
    console.error('加载分类列表失败:', error)
    availableCategories.value = []
    
    // 如果分类加载失败，创建一个默认分类用于显示
    availableCategories.value = [
      { categoryId: 1, category: '学习交流' },
      { categoryId: 2, category: '生活分享' },
      { categoryId: 3, category: '技术讨论' },
      { categoryId: 4, category: '其他' }
    ]
    thPostForm.categoryId = 1
    console.log('⚠️ 使用默认分类列表')
  }
}

// 检查登录状态
=======
// 获取分类列表
const fetchCategories = async () => {
  try {
    thCategories.value = await getPostCategories()
    console.log('TreeHole: 获取分类列表成功:', thCategories.value)
  } catch (error: any) {
    console.error('TreeHole: 获取分类列表失败:', error)
    ElMessage.error('获取分类列表失败，请重试')
  }
}

// 检查登录状态并获取分类列表
>>>>>>> origin/main
onMounted(async () => {
  const thCurrentUserId = getCurrentUserId()
  if (!thCurrentUserId) {
    ElMessage.warning('请先登录')
    router.push('/login')
    return
  }
  
<<<<<<< HEAD
  // 并行加载贴吧列表和分类列表
  await Promise.all([
    loadAvailableBars(),
    loadAvailableCategories()
  ])
=======
  // 获取分类列表
  await fetchCategories()
>>>>>>> origin/main
})

// 处理取消
const handleCancel = () => {
  router.back()
}

// 处理提交
const handleSubmit = async () => {
  if (!thFormRef.value) return
  
  try {
    // 表单验证
    await thFormRef.value.validate()
    
    // 检查用户登录状态
    const thCurrentUserId = getCurrentUserId()
    if (!thCurrentUserId) {
      ElMessage.error('用户未登录')
      router.push('/login')
      return
    }
    
    // 检查用户是否被封禁
    try {
      const userResponse = await axiosInstance.get(`user/${thCurrentUserId}`)
      const userInfo = userResponse.data
      
      if (userInfo.status === 0) {
        ElMessage.error('您的账号已被封禁，无法发帖')
        router.push('/CommunityPage')
        return
      }
    } catch (error) {
      console.error('检查用户状态失败:', error)
      ElMessage.error('无法验证用户状态，请稍后重试')
      return
    }
    
    thLoading.value = true
    
    // 验证分类选择
    if (!thPostForm.categoryId) {
      ElMessage.error('请选择帖子分类')
      thLoading.value = false
      return
    }
    
    const selectedCategory = availableCategories.value.find(cat => cat.categoryId === thPostForm.categoryId)
    console.log('✅ 使用用户选择的分类:', selectedCategory?.category, 'ID:', thPostForm.categoryId)
    
    // 根据发布类型设置不同的参数
    let actualBarId = null // 树洞帖子的BarId为null，在数据库中存储为NULL
    let actualAlsoInTreehole = 0
    let publishLocation = ''
    
    if (thPostForm.publishType === 'bar' && thPostForm.barId) {
      // 发布到贴吧
      actualBarId = thPostForm.barId
      actualAlsoInTreehole = thPostForm.alsoInTreehole ? 1 : 0
      const selectedBar = availableBars.value.find(bar => bar.barId === thPostForm.barId)
      publishLocation = `贴吧：${selectedBar?.barName}`
      
      if (thPostForm.alsoInTreehole) {
        console.log('🔗 跨发布模式：发布到贴吧并同时在树洞显示')
      } else {
        console.log('🏠 贴吧专属模式：仅发布到贴吧')
      }
    } else {
      // 发布到树洞
      actualBarId = null // 重要：树洞帖子的BarId必须为null
      actualAlsoInTreehole = 0
      publishLocation = '树洞社区'
      console.log('🕳️ 树洞模式：发布到树洞社区')
    }
    
    const thCreateData = {
      userId: parseInt(thCurrentUserId),
<<<<<<< HEAD
      categoryId: thPostForm.categoryId, // 使用用户选择的分类ID
      barId: actualBarId, // 贴吧ID（树洞模式为null，贴吧模式为具体ID）
=======
      categoryId: thPostForm.categoryId, // 使用用户选择的分类
>>>>>>> origin/main
      title: thPostForm.title.trim(),
      content: thPostForm.content.trim(),
      creationDate: new Date().toISOString(),
      updateDate: new Date().toISOString(),
      isSticky: 0, // 不置顶
      likeCount: 0,
      dislikeCount: 0,
      favoriteCount: 0,
      commentCount: 0,
      imageUrl: null as any,
      alsoInTreehole: actualAlsoInTreehole // 使用计算后的跨发布标志
    }
    
    console.log('📝 发帖详情:', {
      发布位置: publishLocation,
      跨发布: thPostForm.alsoInTreehole,
      数据: thCreateData
    })
    
    console.log('TreeHole: 正在创建帖子:', thCreateData)
    
    const thResult = await createPost(thCreateData)
    
    console.log('TreeHole: 创建成功:', thResult)
    
    ElMessage.success('树洞发布成功！')
    
    // 成功后跳转到首页，延迟0.8秒以显示成功消息
    setTimeout(() => {
      router.push('/')
    }, 800)
    
  } catch (error: any) {
    console.error('TreeHole: 发布失败:', error)
    
    if (error.response?.data?.message) {
      ElMessage.error(`发布失败: ${error.response.data.message}`)
    } else {
      ElMessage.error('发布失败，请重试')
    }
  } finally {
    thLoading.value = false
  }
}
</script>

<style scoped>
.th-post-edit-container {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

/* 背景图片样式 */
.th-background-image {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  z-index: -1;
  opacity: 0.8;
}

.th-post-edit-card {
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  max-width: 800px;
  width: 100%;
}

.th-card-header {
  text-align: center;
}

.th-card-header h2 {
  margin: 0;
  color: #409eff;
  font-size: 24px;
  font-weight: 600;
}

.th-subtitle {
  color: #909399;
  font-size: 14px;
}

.th-post-form {
  margin-top: 20px;
}

.th-form-actions {
  display: flex;
  justify-content: center;
  gap: 16px;
  margin-top: 20px;
}

:deep(.el-textarea__inner) {
  resize: vertical;
  min-height: 120px;
}

:deep(.el-form-item__label) {
  font-weight: 500;
  color: #303133;
}

/* 贴吧选择相关样式 */
.bar-option {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.bar-name {
  font-weight: 500;
  color: #333;
}

.bar-followers {
  font-size: 12px;
  color: #999;
}

.form-hint {
  font-size: 12px;
  color: #666;
  margin-top: 4px;
  line-height: 1.4;
}

/* 发布位置选择样式 */
:deep(.el-radio-group) {
  display: flex;
  gap: 24px;
}

:deep(.el-radio) {
  margin-right: 0;
  font-size: 16px;
  font-weight: 500;
}

:deep(.el-radio__input.is-checked .el-radio__inner) {
  background-color: #409eff;
  border-color: #409eff;
}

/* 跨发布选项样式 */
.cross-publish-label {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  font-weight: 500;
  color: #409eff;
}

.checkbox-icon {
  font-size: 16px;
}

:deep(.el-checkbox__input.is-checked .el-checkbox__inner) {
  background-color: #67c23a;
  border-color: #67c23a;
}

/* 分类选择样式 */
.category-option {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
}

.category-icon {
  font-size: 14px;
}

.category-name {
  font-weight: 500;
  color: #333;
}
</style>
