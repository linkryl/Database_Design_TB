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
        <div class='harmony-notice-subtitle'>共同打造和谐宠物社区</div>
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
    <div class='search-container'>
      <CommunitySearchBox/>
    </div>
  </div>

  <div class='page-container'>
    <!-- 帖子列表区域 -->
    <div class='posts-section'>
      <PostCard v-for='postId in paginatedPostIds' :key='postId' :post-id='postId'/>
      <div class='pagination-container'>
        <el-pagination @current-change='handleCurrentChange'
                       :current-page='currentPage'
                       :page-size='pageSize'
                       layout='prev, pager, next'
                       :total='totalPosts'>
        </el-pagination>
      </div>
    </div>
  </div>

  <el-dialog v-model='showPublishPost'
             width='1000px'
             style='height: auto'
             title="发布帖子"
             :close-on-click-modal='false'
             :close-on-press-escape='false'
             align-center>
    <el-form :model='postRuleForm'
             ref='postRuleFormRef'
             :rules='postRules'>
      <el-form-item prop='title'>
        <div class='form-label-wrapper'>
          <div class='form-label-container'>
            <span class='required-star'>*</span>
            <el-icon :size='18' style='margin-right: 5px'>
              <Postcard/>
            </el-icon>
            <span style='font-size: 16px'>帖子标题</span>
          </div>

          <el-input v-model='postRuleForm.title'
                    maxlength='64'
                    size='large'
                    show-word-limit
                    placeholder="请输入帖子标题"/>
        </div>
      </el-form-item>

      <el-form-item prop='content'>
        <div class='form-label-wrapper'>
          <div class='form-label-container'>
            <span class='required-star'>*</span>
            <el-icon :size='18' style='margin-right: 5px'>
              <Collection/>
            </el-icon>
            <span style='font-size: 16px'>帖子内容</span>
          </div>

          <el-input v-model='postRuleForm.content'
                    maxlength='512'
                    show-word-limit
                    :autosize='{ minRows: 3 }'
                    type='textarea'
                    size='large'
                    placeholder="请输入帖子内容"/>
        </div>
      </el-form-item>

      <el-form-item prop='categoryId'>
        <div class='form-label-wrapper'>
          <div class='form-label-container'>
            <span class='required-star'>*</span>
            <el-icon :size='18' style='margin-right: 5px'>
              <CollectionTag/>
            </el-icon>
            <span style='font-size: 16px'>帖子分类</span>
          </div>

          <el-radio-group size='large' v-model='postRuleForm.categoryId'>
            <el-radio v-for='tag in sortedPostCategories' :key='tag.id' :label='tag.id'>{{ tag.name }}</el-radio>
          </el-radio-group>
        </div>
      </el-form-item>
    </el-form>

    <div style='display: flex'>
      <el-upload ref='postImage'
                 :limit='1'
                 :before-upload='handleBeforeUploadImage'
                 accept='.jpeg, .jpg'
                 :show-file-list='false'>
        <el-button size='large' plain>
          <span>上传帖子图片</span>
        </el-button>
      </el-upload>

      <el-button v-if="imageUrls[0]!=''" size='large' plain style='margin-left: 12px' @click='imageViewerVisible=true'>
        <span>查看帖子图片</span>
      </el-button>
    </div>

    <el-button-group style='display: flex; justify-content: center; margin-top: 8px'>
      <el-button size='large' @click='cancelPublishPost'>取消发帖</el-button>
      <el-button size='large' @click='submitPost(postRuleFormRef)' type='primary'>
        发布帖子
      </el-button>
    </el-button-group>
  </el-dialog>

  <el-image-viewer v-if='imageViewerVisible'
                   :url-list='addOssPrefix()'
                   :initial-index='0'
                   @close='imageViewerVisible=false'/>
</template>

<script setup lang='ts'>
import {ref, computed, onMounted, watch, reactive} from 'vue'
import {useI18n} from 'vue-i18n'
import {ossBaseUrl} from '../globals'
import PostCard from '../components/PostCard.vue'
import axiosInstance from '../utils/axios'
import {ElMessage, ElMessageBox, ElNotification, FormInstance, FormRules, UploadInstance} from 'element-plus'
import {Collection, CollectionTag, Postcard} from '@element-plus/icons-vue'

const {t, locale} = useI18n()
const currentPage = ref(1)
const pageSize = ref(10)
const totalPosts = computed(() => postIds.value.length)
const postIds = ref([])
const storedValue = localStorage.getItem('currentUserId')
const storedUserId = storedValue ? parseInt(storedValue) : 0
const currentUserId = ref(isNaN(storedUserId) ? 0 : storedUserId)
const showPublishPost = ref(false)
const postCategories = ref([])
const postImage = ref<UploadInstance>()
const postRuleFormRef = ref<FormInstance>()
const imageViewerVisible = ref(false)
const imageUrls = ref([''])

interface PostRuleForm {
  title: string
  content: string
  categoryId: string
}

const sortedPostCategories = computed(() => postCategories.value.sort((a, b) => a.id - b.id))

const postRuleForm = reactive<PostRuleForm>({
  title: '',
  content: '',
  categoryId: ''
})

const postRules: FormRules = {
  title: [
    {
      required: true,
      message: '帖子标题不能为空',
      trigger: 'blur'
    },
  ],
  content: [
    {
      required: true,
      message: '帖子内容不能为空',
      trigger: 'blur'
    },
  ],
  categoryId: [
    {
      required: true,
      message: '帖子分类不能为空',
      trigger: 'blur'
    }
  ]
}

const paginatedPostIds = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return postIds.value.slice(start, end)
})

const handleCurrentChange = (page) => {
  currentPage.value = page
}


function addOssPrefix(): string[] {
  return imageUrls.value.map(url => `${ossBaseUrl}${url}`)
}

onMounted(async () => {
  try {
    // 获取所有帖子并按时间最新顺序排序
    const response = await axiosInstance.get('post/latest')
    postIds.value = response.data
  } catch (error) {
    ElMessage.error('GET 请求失败，请检查网络连接情况或稍后重试。')
  }
})


function publishPost() {
  if (currentUserId.value && currentUserId.value != 0) {
    showPublishPost.value = true
  } else {
    ElMessage.warning('请先进行登录！')
  }
}

onMounted(async () => {
  try {
    const response = await axiosInstance.get('post-category')
    postCategories.value = response.data.map(tag => ({
      id: tag.categoryId,
      name: tag.category
    }))
  } catch (error) {
    ElMessage.error('GET 请求失败，请检查网络连接情况或稍后重试。')
  }
})

function beforeUploadImage(file) {
  const isJPG = file.type == 'image/jpeg'
  const isLt5M = file.size / 1024 / 1024 < 5
  if (!isJPG) {
    ElMessage.error('上传帖子图片只能是 JPG 格式')
    return false
  }
  if (!isLt5M) {
    ElMessage.error('上传帖子图片大小不能超过 5MB')
    return false
  }
  return true
}

const handleBeforeUploadImage = async (file: File) => {
  if (!beforeUploadImage(file)) {
    return
  }
  const formData = new FormData()
  formData.append('file', file, 'postImage.jpg')
  try {
    const response = await axiosInstance.post('upload-post-image', formData)
    imageUrls.value[0] = response.data.fileName
    ElMessage.success('上传帖子图片成功')
  } catch (error) {
    ElMessage.error('上传帖子图片失败')
  }
}

const cancelPublishPost = () => {
  ElMessageBox.confirm(
      '确认要取消发布帖子吗？您的输入内容将不会被保存。',
      '取消发布帖子',
      {
        showClose: false,
        closeOnClickModal: false,
        closeOnPressEscape: false,
        confirmButtonText: '继续发帖',
        cancelButtonText: '取消发帖'
      }
  ).catch(() => {
    showPublishPost.value = false
    resetFeedback(postRuleFormRef.value)
  })
}

const resetFeedback = (formEl: FormInstance | undefined) => {
  if (!formEl) {
    return
  }
  formEl.resetFields()
  imageUrls.value[0] = ''
}

const submitPost = async (formEl: FormInstance | undefined) => {
  if (!formEl) {
    return
  }
  await formEl.validate(async (valid) => {
    if (valid) {
      const result = await postPost()
      if (result) {
        ElNotification({
          title: '帖子发布成功',
          message: '帖子发布成功，您可以在我发布的帖子中查看或继续浏览其他内容。',
          type: 'success'
        })
        showPublishPost.value = false
        resetFeedback(postRuleFormRef.value)
      } else {
        ElNotification({
          title: '帖子发布失败',
          message: '帖子发布失败，请检查网络连接情况或稍后重试。',
          type: 'error'
        })
      }
    }
  })
}

async function postPost() {
  const now = new Date().toISOString()
  try {
    const response = await axiosInstance.post('post', {
      userId: currentUserId.value,
      categoryId: parseInt(postRuleForm.categoryId),
      title: postRuleForm.title,
      content: postRuleForm.content,
      creationDate: now,
      updateDate: now,
      isSticky: 0,
      likeCount: 0,
      dislikeCount: 0,
      favoriteCount: 0,
      commentCount: 0,
      imageUrl: imageUrls.value[0] == '' ? null : imageUrls.value[0]
    })
    
    if (response.status == 201) {
      // 发布成功后刷新帖子列表
      await refreshPosts()
    }
    
    return response.status == 201
  } catch (error) {
    return false
  }
}

// 刷新帖子列表的函数
async function refreshPosts() {
  try {
    const response = await axiosInstance.get('post/latest')
    postIds.value = response.data
    currentPage.value = 1 // 重置到第一页
  } catch (error) {
    ElMessage.error('GET 请求失败，请检查网络连接情况或稍后重试。')
  }
}
</script>

<style scoped>
:global(:root) {
  --community-background-image: linear-gradient(rgba(255, 255, 255, 0.2), rgba(255, 255, 255, 0.2)), url('[TODO: ossBaseUrl]BackgroundImages/CommunityPageBackgroundImage.jpg');
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
  --community-background-image: linear-gradient(rgba(0, 0, 0, 0.2), rgba(0, 0, 0, 0.2)), url('[TODO: ossBaseUrl]BackgroundImages/CommunityPageBackgroundImage.jpg');
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

.search-container {
  position: relative;
  margin: 0 auto 30px;
  max-width: 600px;
  padding: 0 20px;
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
  max-width: 1440px;
  margin: 0 auto;
  padding: 0 20px 20px;
  margin-top: -10px;
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

.form-label-wrapper {
  display: flex;
  flex-direction: column;
  width: 100%;
}

.form-label-container {
  display: flex;
  align-items: center;
  padding-bottom: 6px;
}

.required-star {
  color: #F56C6C;
  margin-right: 5px;
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
</style>
