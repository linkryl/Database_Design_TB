<!--
  个人资料展示页面
  2353814 马小龙 | TreeHole 项目开发组
-->

<template>
  <div class='personal-zone-container'>
    <!-- 顶部背景区域 -->
    <div class='profile-header'>
      <div class='header-content'>
        <el-avatar class='user-avatar' :src='`${ossBaseUrl}${userInfo.avatarUrl}`'>
          <el-icon :size='50'>
            <UserFilled/>
          </el-icon>
        </el-avatar>
        <div class='user-info'>
          <h1 class='user-name'>{{ userInfo.userName }}</h1>
          <span class='user-uid'>UID: {{ userInfo.uid }}</span>
          
          <!-- 等级信息 -->
          <div class='level-section'>
            <div class='level-badge'>LV {{ currentLevel }}</div>
            <el-progress 
              :percentage='experiencePercentage'
              :show-text='false'
              :stroke-width='6'
              style='width: 200px'>
            </el-progress>
            <span class='exp-text'>{{ userInfo.experience }} / {{ nextLevelExp }}</span>
          </div>
          
          <div class='user-stats'>
            <span class='stat-item'>
              <strong>{{ userInfo.followUser }}</strong> 关注
            </span>
            <span class='stat-item'>
              <strong>{{ userInfo.followedCount }}</strong> 粉丝
            </span>
            <span class='stat-item'>
              <strong>{{ userInfo.coinCount }}</strong> 金币
            </span>
          </div>
        </div>
        <div class='header-actions'>
          <el-button v-if='isSelf' 
                     type='warning' 
                     size='large'
                     @click='openEditPanel'>
            <el-icon><EditPen/></el-icon>
            编辑资料
          </el-button>
          <el-button v-else
                     :type='isFollowing ? "info" : "warning"'
                     size='large'
                     @click='handleFollow'>
            <el-icon><Plus v-if='!isFollowing'/><Check v-else/></el-icon>
            {{ isFollowing ? '已关注' : '关注' }}
          </el-button>
        </div>
      </div>
    </div>

    <!-- 主内容区域 -->
    <div class='main-wrapper'>
      <div class='main-content'>
        <!-- 左侧导航栏 -->
        <div class='sidebar'>
          <div class='nav-menu'>
            <div 
              v-for='item in menuItems' 
              :key='item.key'
              :class='["nav-item", { active: activeTab === item.key, disabled: !item.visible }]'
              @click='item.visible && selectTab(item.key)'
              v-show='item.visible'>
              <el-icon class='nav-icon'>
                <component :is='item.icon'/>
              </el-icon>
              <span class='nav-text'>{{ item.label }}</span>
              <span v-if='item.count !== undefined' class='nav-count'>{{ item.count }}</span>
            </div>
          </div>
        </div>

        <!-- 右侧内容区 -->
        <div class='content-area'>
          <!-- 个人信息 -->
          <div v-if='activeTab === "profile"' class='content-panel'>
            <h2 class='panel-title'>
              <el-icon><User/></el-icon>
              个人信息
            </h2>
            <div class='info-container'>
              <div class='info-section'>
                <h3 class='section-title'>基本信息</h3>
                <div class='info-grid'>
                  <div class='info-item'>
                    <label>用户ID</label>
                    <span>{{ userInfo.uid }}</span>
                  </div>
                  <div class='info-item'>
                    <label>昵称</label>
                    <span>{{ userInfo.userName }}</span>
                  </div>
                  <div class='info-item'>
                    <label>性别</label>
                    <span>{{ formatGender(userInfo.gender) }}</span>
                  </div>
                  <div class='info-item'>
                    <label>生日</label>
                    <span>{{ formatBirthdate(userInfo.birthdate) }}</span>
                  </div>
                  <div class='info-item'>
                    <label>角色</label>
                    <span>{{ formatRole(userInfo.role) }}</span>
                  </div>
                  <div class='info-item'>
                    <label>状态</label>
                    <span class='status-badge' :class='userInfo.status === 0 ? "normal" : "disabled"'>
                      {{ formatStatus(userInfo.status) }}
                    </span>
                  </div>
                </div>
              </div>
              
              <div class='info-section'>
                <h3 class='section-title'>个人简介</h3>
                <div class='profile-intro'>
                  {{ userInfo.profile || '这个人很神秘，什么都没有留下...' }}
                </div>
              </div>

              <div class='info-section'>
                <h3 class='section-title'>账户统计</h3>
                <div class='stats-grid'>
                  <div class='stat-card'>
                    <el-icon class='stat-icon' color='#3b82f6'><Document/></el-icon>
                    <div class='stat-value'>{{ userInfo.followBar }}</div>
                    <div class='stat-label'>关注贴吧</div>
                  </div>
                  <div class='stat-card'>
                    <el-icon class='stat-icon' color='#10b981'><Star/></el-icon>
                    <div class='stat-value'>{{ userInfo.favoriteCount }}</div>
                    <div class='stat-label'>收藏帖子</div>
                  </div>
                  <div class='stat-card'>
                    <el-icon class='stat-icon' color='#f59e0b'><Coin/></el-icon>
                    <div class='stat-value'>{{ userInfo.coinCount }}</div>
                    <div class='stat-label'>金币余额</div>
                  </div>
                  <div class='stat-card'>
                    <el-icon class='stat-icon' color='#ef4444'><TrophyBase/></el-icon>
                    <div class='stat-value'>{{ userInfo.experience }}</div>
                    <div class='stat-label'>经验值</div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 我的关注 -->
          <div v-if='activeTab === "following"' class='content-panel'>
            <h2 class='panel-title'>
              <el-icon><UserFilled/></el-icon>
              {{ isSelf ? '我的关注' : 'TA的关注' }}
            </h2>
            <div v-if='followingList.length === 0' class='empty-state'>
              <el-empty description='还没有关注任何人'/>
            </div>
            <div v-else class='cards-container'>
              <UserCard 
                v-for='followingId in followingList' 
                :key='followingId'
                :user-id='followingId'/>
            </div>
          </div>

          <!-- 我的粉丝 -->
          <div v-if='activeTab === "followers"' class='content-panel'>
            <h2 class='panel-title'>
              <el-icon><User/></el-icon>
              {{ isSelf ? '我的粉丝' : 'TA的粉丝' }}
            </h2>
            <div v-if='followersList.length === 0' class='empty-state'>
              <el-empty description='还没有粉丝'/>
            </div>
            <div v-else class='cards-container'>
              <UserCard 
                v-for='followerId in followersList' 
                :key='followerId'
                :user-id='followerId'/>
            </div>
          </div>

          <!-- 我的贴吧 -->
          <div v-if='activeTab === "myBars"' class='content-panel'>
            <h2 class='panel-title'>
              <el-icon><House/></el-icon>
              我创建的贴吧
            </h2>
            <div v-if='myBarsList.length === 0' class='empty-state'>
              <el-empty description='还没有创建贴吧'/>
            </div>
            <div v-else class='cards-container'>
              <BarCard 
                v-for='barId in myBarsList' 
                :key='barId'
                :bar-id='barId'/>
            </div>
          </div>

          <!-- 我关注的贴吧 -->
          <div v-if='activeTab === "followedBars"' class='content-panel'>
            <h2 class='panel-title'>
              <el-icon><CollectionTag/></el-icon>
              {{ isSelf ? '我关注的贴吧' : 'TA关注的贴吧' }}
            </h2>
            <div v-if='followedBarsList.length === 0' class='empty-state'>
              <el-empty description='还没有关注贴吧'/>
            </div>
            <div v-else class='cards-container'>
              <BarCard 
                v-for='barId in followedBarsList' 
                :key='barId'
                :bar-id='barId'/>
            </div>
          </div>

          <!-- 我的帖子 -->
          <div v-if='activeTab === "myPosts"' class='content-panel'>
            <h2 class='panel-title'>
              <el-icon><Document/></el-icon>
              我发布的帖子
            </h2>
            <div v-if='myPostsList.length === 0' class='empty-state'>
              <el-empty description='还没有发布帖子'/>
            </div>
            <div v-else class='posts-container'>
              <PostCard 
                v-for='postId in myPostsList' 
                :key='postId'
                :post-id='postId'/>
            </div>
          </div>

          <!-- 我收藏的帖子 -->
          <div v-if='activeTab === "favorites"' class='content-panel'>
            <h2 class='panel-title'>
              <el-icon><Star/></el-icon>
              我收藏的帖子
            </h2>
            <div v-if='favoritesList.length === 0' class='empty-state'>
              <el-empty description='还没有收藏帖子'/>
            </div>
            <div v-else class='posts-container'>
              <PostCard 
                v-for='postId in favoritesList' 
                :key='postId'
                :post-id='postId'/>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 编辑资料对话框 -->
    <el-dialog 
      v-model='showEditDialog' 
      title='编辑个人资料'
      width='650px'
      :close-on-click-modal='false'
      class='edit-dialog'>
      
      <el-form :model='editData' label-width='100px' class='edit-form'>
        <el-form-item label='UID'>
          <el-input 
            :value='userInfo.uid' 
            disabled/>
        </el-form-item>

        <el-form-item label='昵称'>
          <el-input 
            :value='userInfo.userName' 
            disabled/>
        </el-form-item>

        <el-form-item label='头像'>
          <div class='avatar-upload-area'>
            <el-upload
              class='avatar-uploader'
              action='#'
              :show-file-list='false'
              :before-upload='validateAvatar'
              :http-request='handleAvatarUpload'>
              <el-avatar :size='100' :src='previewAvatarUrl || `${ossBaseUrl}${editData.avatarUrl}`'>
                <el-icon :size='40'><Plus/></el-icon>
              </el-avatar>
            </el-upload>
            <div class='avatar-tips'>
              点击上传新头像<br>
              只支持 JPG，最大 2MB
            </div>
          </div>
        </el-form-item>

        <el-form-item label='性别'>
          <el-radio-group v-model='editData.gender'>
            <el-radio :label='0'>男性</el-radio>
            <el-radio :label='1'>女性</el-radio>
          </el-radio-group>
        </el-form-item>

        <el-form-item label='生日'>
          <el-date-picker
            v-model='editData.birthdate'
            type='date'
            placeholder='选择出生日期'
            format='YYYY-MM-DD'
            value-format='YYYY-MM-DD'
            :disabled-date='disableFutureDate'
            style='width: 100%'/>
        </el-form-item>

        <el-form-item label='个人简介'>
          <el-input
            v-model='editData.profile'
            type='textarea'
            :rows='5'
            maxlength='300'
            show-word-limit
            placeholder='介绍一下自己，让大家更了解你~'/>
        </el-form-item>
      </el-form>

      <template #footer>
        <div class='dialog-footer'>
          <el-button @click='cancelEdit'>取消</el-button>
          <el-button type='primary' @click='updateProfile'>保存修改</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang='ts'>

import {computed, onMounted, onBeforeUnmount, ref, watch} from 'vue'
import axiosInstance from '../utils/axios'
import {ElMessage} from 'element-plus'
import {useRoute, useRouter} from 'vue-router'
import {formatDateTimeToCST, ossBaseUrl} from '../globals'
import {
  UserFilled, User, EditPen, Plus, 
  Star, Document, Check, House, 
  CollectionTag, Coin, TrophyBase
} from '@element-plus/icons-vue'

// 引入组件
import UserCard from '../components/ProfileUserCard.vue'
import PostCard from '../components/ProfilePostCard.vue'
import BarCard from '../components/ProfileBarCard.vue'

interface UploadRequestOptions {
  action: string
  method: string
  data: any
  filename: string
  file: File
  headers: Record<string, string>
  withCredentials: boolean
  onProgress: (evt: ProgressEvent) => void
  onSuccess: (response: any) => void
  onError: (error: Error) => void
}

const route = useRoute()
const router = useRouter()
const viewedUserId = ref(parseInt(route.params.id as string))
const localUserId = localStorage.getItem('currentUserId')
const currentUserId = ref(localUserId ? parseInt(localUserId) : 0)
const isSelf = computed(() => viewedUserId.value === currentUserId.value && currentUserId.value !== 0)

// 当前激活的标签页
const activeTab = ref('profile')

// 菜单项配置
const menuItems = computed(() => [
  { 
    key: 'profile', 
    label: '个人信息', 
    icon: 'User',
    visible: true 
  },
  { 
    key: 'following', 
    label: isSelf.value ? '我的关注' : 'TA的关注', 
    icon: 'UserFilled',
    count: userInfo.value.followUser,
    visible: true 
  },
  { 
    key: 'followers', 
    label: isSelf.value ? '我的粉丝' : 'TA的粉丝', 
    icon: 'User',
    count: userInfo.value.followedCount,
    visible: true 
  },
  { 
    key: 'myBars', 
    label: '我的贴吧', 
    icon: 'House',
    visible: isSelf.value 
  },
  { 
    key: 'followedBars', 
    label: isSelf.value ? '我关注的贴吧' : 'TA关注的贴吧', 
    icon: 'CollectionTag',
    count: userInfo.value.followBar,
    visible: true 
  },
  { 
    key: 'myPosts', 
    label: '我的帖子', 
    icon: 'Document',
    visible: isSelf.value 
  },
  { 
    key: 'favorites', 
    label: '我收藏的帖子', 
    icon: 'Star',
    count: userInfo.value.favoriteCount,
    visible: isSelf.value 
  }
])

// 用户页面数据
const userInfo = ref({
  uid: viewedUserId.value,
  userName: '',
  role: 0,
  status: 0,
  avatarUrl: '',
  profile: '',
  gender: 0,
  birthdate: '',
  experience: 0,
  followBar: 0,
  followUser: 0,
  followedCount: 0,
  favoriteCount: 0,
  coinCount: 0
})

// 列表数据
const followingList = ref([])
const followersList = ref([])
const myBarsList = ref([])
const followedBarsList = ref([])
const myPostsList = ref([])
const favoritesList = ref([])

// 切换标签页
const selectTab = (tab: string) => {
  activeTab.value = tab
  loadTabData(tab)
}

// 加载对应标签页数据
const loadTabData = async (tab: string) => {
  try {
    switch(tab) {
      case 'following':
        await loadFollowingList()
        break
      case 'followers':
        await loadFollowersList()
        break
      case 'myBars':
        await loadMyBarsList()
        break
      case 'followedBars':
        await loadFollowedBarsList()
        break
      case 'myPosts':
        await loadMyPostsList()
        break
      case 'favorites':
        await loadFavoritesList()
        break
    }
  } catch (error) {
    console.error(`加载${tab}数据失败:`, error)
  }
}

// 加载关注列表id
const loadFollowingList = async () => {
  try {
    const res = await axiosInstance.get(`user-follow/following/${viewedUserId.value}`)
    followingList.value = res.data || []
  } catch (error) {
    console.error('获取关注列表失败:', error)
  }
}

// 加载粉丝列表id
const loadFollowersList = async () => {
  try {
    const res = await axiosInstance.get(`user-follow/followers/${viewedUserId.value}`)
    followersList.value = res.data || []
  } catch (error) {
    console.error('获取粉丝列表失败:', error)
  }
}

// 加载我的贴吧id
const loadMyBarsList = async () => {
  try {
    const res = await axiosInstance.get(`bar/user/${viewedUserId.value}`)
    myBarsList.value = res.data || []
  } catch (error) {
    console.error('获取贴吧列表失败:', error)
  }
}

// 加载关注的贴吧id
const loadFollowedBarsList = async () => {
  try {
    const res = await axiosInstance.get(`bar-follow/user/${viewedUserId.value}`)
    followedBarsList.value = res.data || []
  } catch (error) {
    console.error('获取关注贴吧失败:', error)
  }
}

// 加载我的帖子id
const loadMyPostsList = async () => {
  try {
    const res = await axiosInstance.get(`post/user/${viewedUserId.value}`)
    myPostsList.value = res.data || []
  } catch (error) {
    console.error('获取帖子列表失败:', error)
  }
}

// 加载收藏的帖子id
const loadFavoritesList = async () => {
  try {
    const res = await axiosInstance.get(`post-favorite/user/${viewedUserId.value}`)
    favoritesList.value = res.data || []
  } catch (error) {
    console.error('获取收藏列表失败:', error)
  }
}

// 当前等级
const currentLevel = computed(() => {
  let level = 1
  let totalExp = 0
  while (totalExp + 100 * level <= userInfo.value.experience) {
    totalExp += 100 * level
    level++
  }
  return level
})


// 当前等级累计经验
const currentLevelExp = computed(() => {
  let totalExp = 0
  for (let i = 1; i < currentLevel.value; i++) {
    totalExp += 100 * i
  }
  return totalExp
})

// 下一等级所需总经验
const nextLevelExp = computed(() => {
  return currentLevelExp.value + 100 * currentLevel.value
})

// 当前等级经验进度百分比
const experiencePercentage = computed(() => {
  const currentProgress = userInfo.value.experience - currentLevelExp.value
  const levelRequirement = nextLevelExp.value - currentLevelExp.value
  return Math.floor((currentProgress / levelRequirement) * 100)
})

// 其他数据
const showEditDialog = ref(false)
const editData = ref({
  avatarUrl: '',
  profile: '',
  gender: 0,
  birthdate: ''
})
const previewAvatarUrl = ref('')
const uploadedFile = ref<File | null>(null)
const isFollowing = ref(false)

const formatGender = (gender: number) => {
  return gender === 0 ? '男' : '女'
}

const formatBirthdate = (birthdate: string) => {
  if (!birthdate) return '未设置'
  try {
    return formatDateTimeToCST(birthdate).date
  } catch {
    return birthdate
  }
}

const formatRole = (role: number) => {
  return role === 0 ? '普通用户' : '管理员'
}

const formatStatus = (status: number) => {
  return status === 0 ? '正常' : '已禁用'
}

// 获取用户信息
const fetchUserProfile = async () => {
  try {
    const res = await axiosInstance.get(`user/${viewedUserId.value}`)
    if (res.status === 404) {
      await router.push('/404')
      return
    }
    
    const data = res.data
    userInfo.value = {
      uid: data.userId || viewedUserId.value,
      userName: data.userName || '',
      role: data.role || 0,
      status: data.status || 0,
      avatarUrl: data.avatarUrl || 'default-avatar.png',
      profile: data.profile || '',
      gender: data.gender !== undefined ? data.gender : 0,
      birthdate: data.birthdate || '',
      experience: data.experiencePoints || data.experience || 0,
      followBar: data.followBarCount || data.followBar || 0,
      followUser: data.followsCount || data.followUser || 0,
      followedCount: data.followedCount || 0,
      favoriteCount: data.favoritesCount || data.favoriteCount || 0,
      coinCount: data.coinCount || data.coin_count || 0
    }
  } catch (error) {
    ElMessage.error('获取用户信息失败')
    console.error('获取用户信息失败:', error)
  }
}

// 获取关注状态
const fetchFollowStatus = async () => {
  if (isSelf.value) return
  
  try {
    await axiosInstance.get(`user-follow/${viewedUserId.value}-${currentUserId.value}`)
    isFollowing.value = true
  } catch {
    isFollowing.value = false
  }
}

// 处理关注/取消关注
const handleFollow = async () => {
  if (isFollowing.value) {
    try {
      await axiosInstance.delete(`user-follow/${viewedUserId.value}-${currentUserId.value}`)
      isFollowing.value = false
      userInfo.value.followedCount--
      ElMessage.success('已取消关注')
    } catch {
      ElMessage.error('取消关注失败')
    }
  } else {
    try {
      await axiosInstance.post('user-follow', {
        userId: viewedUserId.value,
        followerId: currentUserId.value,
        favoriteTime: new Date().toISOString()
      })
      isFollowing.value = true
      userInfo.value.followedCount++
      ElMessage.success('关注成功')
    } catch {
      ElMessage.error('关注失败')
    }
  }
}

// 打开编辑对话框
const openEditPanel = () => {
  editData.value = {
    avatarUrl: userInfo.value.avatarUrl,
    profile: userInfo.value.profile,
    gender: userInfo.value.gender,
    birthdate: userInfo.value.birthdate
  }
  previewAvatarUrl.value = ''
  uploadedFile.value = null
  showEditDialog.value = true
}

// 取消编辑
const cancelEdit = () => {
  previewAvatarUrl.value = ''
  uploadedFile.value = null
  showEditDialog.value = false
}

// 保存修改
const updateProfile = async () => {
  try {
    if (uploadedFile.value) {
      const formData = new FormData()
      formData.append('file', uploadedFile.value, 'avatar.jpg')
      
      try {
        const uploadRes = await axiosInstance.post('upload-avatar', formData)
        if (uploadRes.data && uploadRes.data.fileName) {
          await axiosInstance.put(`user/avatar-url/${viewedUserId.value}`, {
            avatarUrl: uploadRes.data.fileName
          })
          editData.value.avatarUrl = uploadRes.data.fileName
        }
      } catch (uploadError) {
        ElMessage.error('头像上传失败')
        console.error('头像上传失败:', uploadError)
        return
      }
    }
    
    const updatePayload = {
      profile: editData.value.profile,
      gender: editData.value.gender,
      birthdate: editData.value.birthdate
    }
    
    await axiosInstance.put(`user/personal-information/${viewedUserId.value}`, updatePayload)
    
    userInfo.value.profile = editData.value.profile
    userInfo.value.gender = editData.value.gender
    userInfo.value.birthdate = editData.value.birthdate
    if (editData.value.avatarUrl) {
      userInfo.value.avatarUrl = editData.value.avatarUrl
    }
    
    previewAvatarUrl.value = ''
    uploadedFile.value = null
    showEditDialog.value = false
    ElMessage.success('资料更新成功')
  } catch (error) {
    ElMessage.error('保存失败，请稍后重试')
  }
}

// 头像验证
const validateAvatar = (file: File): boolean => {
  const isImage = file.type === 'image/jpeg' || file.type === 'image/jpg'
  const isLt2M = file.size / 1024 / 1024 < 2
  
  if (!isImage) {
    ElMessage.error('只支持 JPG 格式的图片')
    return false
  }
  if (!isLt2M) {
    ElMessage.error('图片大小不能超过 2MB')
    return false
  }
  return true
}

// 处理头像上传
const handleAvatarUpload = (options: UploadRequestOptions) => {
  try {
    previewAvatarUrl.value = URL.createObjectURL(options.file)
    uploadedFile.value = options.file
    options.onSuccess({ success: true })
    ElMessage.success('头像已选择，点击保存修改后生效')
  } catch (error) {
    ElMessage.error('头像上传失败')
  }
}

// 禁用未来日期
const disableFutureDate = (date: Date) => {
  return date.getTime() > Date.now()
}

// 监听路由变化
watch(() => route.params.id, (newId) => {
  viewedUserId.value = parseInt(newId as string)
  userInfo.value.uid = viewedUserId.value
  fetchUserProfile()
  fetchFollowStatus()
})

onMounted(() => {
  fetchUserProfile()
  fetchFollowStatus()
})

// 组件卸载时清理URL
onBeforeUnmount(() => {
  if (previewAvatarUrl.value) {
    URL.revokeObjectURL(previewAvatarUrl.value)
  }
})
</script>

<style scoped>
/* 容器 */
.personal-zone-container {
  min-height: 100vh;
  background: #f0f2f5;
}

/* 顶部背景 */
.profile-header {
  background: linear-gradient(135deg, #2563eb 0%, #3b82f6 50%, #60a5fa 100%);
  padding: 50px 0;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.15);
}

.header-content {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 24px;
  display: flex;
  align-items: center;
  gap: 35px;
}

.user-avatar {
  width: 120px;
  height: 120px;
  border: 5px solid white;
  box-shadow: 0 8px 24px rgba(0,0,0,0.12);
}

.user-info {
  flex: 1;
  color: white;
}

.user-name {
  font-size: 32px;
  margin: 0 0 10px 0;
  font-weight: 700;
  text-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.user-uid {
  display: inline-block;
  background: rgba(255,255,255,0.25);
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 14px;
  margin-bottom: 15px;
  backdrop-filter: blur(10px);
}

/* 等级信息 */
.level-section {
  display: flex;
  align-items: center;
  gap: 15px;
  margin: 15px 0;
}

.level-badge {
  background: linear-gradient(135deg, #fbbf24, #f59e0b);
  padding: 4px 12px;
  border-radius: 20px;
  font-weight: 600;
  font-size: 14px;
  box-shadow: 0 2px 8px rgba(245, 158, 11, 0.3);
}

.exp-text {
  font-size: 13px;
  opacity: 0.95;
}

.user-stats {
  display: flex;
  gap: 30px;
  font-size: 15px;
  margin-top: 10px;
}

.stat-item strong {
  font-size: 20px;
  margin-right: 6px;
  font-weight: 700;
}

.header-actions {
  flex-shrink: 0;
}

.header-actions .el-button {
  min-width: 140px;
  height: 48px;
  font-size: 16px;
  font-weight: 500;
  border-radius: 10px;
}

/* 主内容包装器 */
.main-wrapper {
  max-width: 1400px;
  margin: 30px auto 0;
  padding: 0 24px 40px;
  position: relative;
}

.main-content {
  display: flex;
  gap: 24px;
  min-height: 600px;
}

/* 左侧导航 */
.sidebar {
  width: 280px;
  background: white;
  border-radius: 16px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.04);
  padding: 12px;
  height: fit-content;
  position: sticky;
  top: 24px;
}

.nav-menu {
  display: flex;
  flex-direction: column;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 16px 20px;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s;
  margin-bottom: 6px;
  position: relative;
  font-size: 15px;
}

.nav-item:hover:not(.disabled) {
  background: #f7f8fa;
  transform: translateX(4px);
}

.nav-item.active {
  background: linear-gradient(135deg, #2563eb 0%, #3b82f6 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.25);
}

.nav-item.active::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 4px;
  height: 60%;
  background: white;
  border-radius: 0 4px 4px 0;
}

.nav-item.disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.nav-icon {
  font-size: 22px;
  margin-right: 14px;
}

.nav-text {
  flex: 1;
  font-weight: 500;
}

.nav-count {
  background: rgba(37, 99, 235, 0.1);
  color: #2563eb;
  padding: 3px 10px;
  border-radius: 14px;
  font-size: 13px;
  font-weight: 600;
}

.nav-item.active .nav-count {
  background: rgba(255,255,255,0.25);
  color: white;
}

/* 右侧内容区 */
.content-area {
  flex: 1;
  background: white;
  border-radius: 16px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.04);
  height: 800px;
  overflow-y: auto;   
  overflow-x: hidden;   
}


.content-panel {
  padding: 32px;
}

.panel-title {
  font-size: 24px;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 30px 0;
  padding-bottom: 20px;
  border-bottom: 2px solid #e2e8f0;
  display: flex;
  align-items: center;
  gap: 10px;
}

.panel-title .el-icon {
  color: #2563eb;
}

/* 信息容器 */
.info-container {
  background: #f8fafc;
  border-radius: 12px;
  padding: 24px;
}

.info-section {
  margin-bottom: 35px;
}

.info-section:last-child {
  margin-bottom: 0;
}

.section-title {
  font-size: 18px;
  font-weight: 600;
  color: #334155;
  margin: 0 0 20px 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.section-title::before {
  content: '';
  width: 4px;
  height: 20px;
  background: #2563eb;
  border-radius: 2px;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
}

.info-item {
  display: flex;
  align-items: center;
  background: white;
  padding: 16px;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
}

.info-item label {
  color: #64748b;
  font-size: 14px;
  width: 80px;
  flex-shrink: 0;
}

.info-item span {
  color: #1e293b;
  font-size: 15px;
  font-weight: 600;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 13px;
}

.status-badge.normal {
  background: #dcfce7;
  color: #16a34a;
}

.status-badge.disabled {
  background: #fee2e2;
  color: #dc2626;
}

.profile-intro {
  background: white;
  padding: 20px;
  border-radius: 10px;
  line-height: 1.8;
  color: #475569;
  border: 1px solid #e2e8f0;
}

/* 统计卡片 */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
}

.stat-card {
  background: white;
  padding: 20px;
  border-radius: 10px;
  text-align: center;
  border: 1px solid #e2e8f0;
  transition: all 0.3s;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 20px rgba(0,0,0,0.08);
}

.stat-icon {
  font-size: 32px;
  margin-bottom: 12px;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 13px;
  color: #64748b;
}

/* 卡片容器 */
.cards-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.posts-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* 空状态 */
.empty-state {
  padding: 100px 0;
  text-align: center;
}

/* 响应式设计 */
@media (max-width: 1400px) {
  .main-wrapper {
    max-width: 1200px;
  }
  
  .sidebar {
    width: 260px;
  }
}

@media (max-width: 1200px) {
  .main-content {
    flex-direction: column;
  }
  
  .sidebar {
    width: 100%;
    position: static;
  }
  
  .nav-menu {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 10px;
  }
  
  .nav-item {
    justify-content: center;
    padding: 12px 16px;
  }
  
  .nav-item.active::before {
    display: none;
  }
  
  .info-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    text-align: center;
  }
  
  .user-stats {
    justify-content: center;
  }
  
  .info-grid {
    grid-template-columns: 1fr;
  }
  
  .cards-container {
    grid-template-columns: 1fr;
  }
}
</style>