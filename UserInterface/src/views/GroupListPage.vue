<template>
  <div class="group-list-container">
    <div class="header">
      <h2>群组列表</h2>
      <el-button type="primary" @click="showCreateDialog = true">
        <el-icon><Plus /></el-icon>
        新建群组
      </el-button>
    </div>

    <!-- 群组列表 -->
    <div class="group-list">
      <div 
        v-for="group in groups" 
        :key="group.groupId"
        class="group-item"
      >
        <div class="group-avatar" @click="viewGroupDetail(group.groupId)">
          <el-avatar :size="48" shape="square">
            {{ group.groupName?.charAt(0) || 'G' }}
          </el-avatar>
        </div>
        <div class="group-info" @click="viewGroupDetail(group.groupId)">
          <div class="group-name">{{ group.groupName }}</div>
          <div class="group-desc">{{ group.groupDesc || '暂无群组描述' }}</div>
          <div class="group-meta">
            <span class="member-count">{{ group.memberCount }}人</span>
            <span class="last-active">
              {{ group.lastActiveTime ? formatTime(group.lastActiveTime) : '暂无活动' }}
            </span>
          </div>
        </div>
        <div class="group-actions">
          <el-button size="small" @click.stop="enterChat(group.groupId)" type="primary" icon="ChatDotRound">
            聊天
          </el-button>
        </div>
      </div>

      <!-- 空状态 -->
      <div v-if="groups.length === 0 && !loading" class="empty-state">
        <el-empty description="暂无群组，创建一个开始聊天吧！" />
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="loading-state">
        <el-skeleton :rows="3" animated />
      </div>
    </div>

    <!-- 创建群组对话框 -->
    <el-dialog v-model="showCreateDialog" title="新建群组" width="500px">
      <el-form :model="createForm" :rules="createRules" ref="createFormRef" label-width="80px">
        <el-form-item label="群组名称" prop="groupName">
          <el-input 
            v-model="createForm.groupName" 
            placeholder="请输入群组名称"
            maxlength="50"
            show-word-limit
          />
        </el-form-item>
        
        <el-form-item label="群组描述" prop="groupDesc">
          <el-input 
            v-model="createForm.groupDesc" 
            type="textarea"
            :rows="3"
            placeholder="请输入群组描述（可选）"
            maxlength="200"
            show-word-limit
          />
        </el-form-item>
        
        <el-form-item label="群组成员" prop="memberIds">
          <el-input 
            v-model="memberIdsText" 
            type="textarea"
            :rows="2"
            placeholder="请输入成员用户ID，多个ID用逗号分隔（至少2个成员）"
          />
          <div class="form-tip">
            例如：1,2,3 （需要至少2个有效的用户ID）
          </div>
        </el-form-item>
      </el-form>

      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showCreateDialog = false">取消</el-button>
          <el-button type="primary" @click="createGroup" :loading="creating">
            创建群组
          </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, FormInstance, FormRules } from 'element-plus'
import { Plus, ArrowRight } from '@element-plus/icons-vue'
import axios from '../components/axios'

// 路由
const router = useRouter()

// 数据状态
const groups = ref<any[]>([])
const loading = ref(false)
const showCreateDialog = ref(false)
const creating = ref(false)
const currentUserId = ref(Number(localStorage.getItem('currentUserId')) || 0)

// 表单相关
const createFormRef = ref<FormInstance>()
const createForm = reactive({
  groupName: '',
  groupDesc: '',
  memberIds: [] as number[]
})

const memberIdsText = ref('')

// 表单验证规则
const createRules: FormRules = {
  groupName: [
    { required: true, message: '请输入群组名称', trigger: 'blur' },
    { min: 1, max: 50, message: '群组名称长度在1到50个字符', trigger: 'blur' }
  ],
  groupDesc: [
    { max: 200, message: '群组描述最多200个字符', trigger: 'blur' }
  ]
}

// 计算属性：解析成员ID
const parsedMemberIds = computed(() => {
  if (!memberIdsText.value.trim()) return []
  
  return memberIdsText.value
    .split(',')
    .map(id => parseInt(id.trim()))
    .filter(id => !isNaN(id) && id > 0)
})

// 获取群组列表
const loadGroups = async () => {
  loading.value = true
  try {
    const response = await axios.get(`/group?userId=${currentUserId.value}`)
    groups.value = response.data || []
  } catch (error) {
    console.error('获取群组列表失败:', error)
    ElMessage.error('获取群组列表失败')
  } finally {
    loading.value = false
  }
}

// 创建群组
const createGroup = async () => {
  if (!createFormRef.value) return
  
  // 验证表单
  const valid = await createFormRef.value.validate().catch(() => false)
  if (!valid) return
  
  // 验证成员ID
  if (parsedMemberIds.value.length < 2) {
    ElMessage.error('至少需要2个有效的成员用户ID')
    return
  }
  
  creating.value = true
  try {
    // 确保当前用户是第一个成员（作为群主）
    const allMemberIds = [currentUserId.value, ...parsedMemberIds.value]
    // 去重，避免重复添加当前用户
    const uniqueMemberIds = [...new Set(allMemberIds)]
    
    const request = {
      groupName: createForm.groupName,
      groupDesc: createForm.groupDesc || null,
      memberIds: uniqueMemberIds
    }
    
    await axios.post('/group', request)
    
    ElMessage.success('群组创建成功')
    showCreateDialog.value = false
    resetCreateForm()
    loadGroups() // 重新加载群组列表
  } catch (error: any) {
    console.error('创建群组失败:', error)
    const errorMsg = error.response?.data || '创建群组失败'
    ElMessage.error(errorMsg)
  } finally {
    creating.value = false
  }
}

// 重置创建表单
const resetCreateForm = () => {
  createForm.groupName = ''
  createForm.groupDesc = ''
  createForm.memberIds = []
  memberIdsText.value = ''
  createFormRef.value?.resetFields()
}

// 进入群组聊天
const enterChat = (groupId: number) => {
  router.push(`/group-chat/${groupId}`)
}

// 查看群组详情
const viewGroupDetail = (groupId: number) => {
  router.push(`/group/${groupId}`)
}

// 进入群组聊天（保持向后兼容）
const enterGroup = (groupId: number) => {
  router.push(`/group-chat/${groupId}`)
}

// 格式化时间
const formatTime = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  if (diff < 60000) { // 1分钟内
    return '刚刚活跃'
  } else if (diff < 3600000) { // 1小时内
    return `${Math.floor(diff / 60000)}分钟前活跃`
  } else if (diff < 86400000) { // 24小时内
    return `${Math.floor(diff / 3600000)}小时前活跃`
  } else if (diff < 604800000) { // 7天内
    return `${Math.floor(diff / 86400000)}天前活跃`
  } else {
    return date.toLocaleDateString('zh-CN')
  }
}

// 组件挂载
onMounted(() => {
  loadGroups()
})
</script>

<style scoped>
.group-list-container {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.header h2 {
  margin: 0;
  color: #303133;
}

.group-list {
  background-color: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 12px rgba(0,0,0,0.1);
}

.group-item {
  display: flex;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid #f0f0f0;
  cursor: pointer;
  transition: all 0.2s;
}

.group-item:last-child {
  border-bottom: none;
}

.group-item:hover {
  background-color: #f8f9fa;
}

.group-avatar {
  margin-right: 16px;
  flex-shrink: 0;
}

.group-info {
  flex: 1;
  min-width: 0;
}

.group-name {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
  margin-bottom: 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.group-desc {
  font-size: 14px;
  color: #606266;
  margin-bottom: 6px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.group-meta {
  display: flex;
  gap: 16px;
  font-size: 12px;
  color: #909399;
}

.group-actions {
  margin-left: 16px;
  display: flex;
  align-items: center;
}

.empty-state,
.loading-state {
  padding: 40px 20px;
}

.form-tip {
  font-size: 12px;
  color: #909399;
  margin-top: 4px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .group-list-container {
    padding: 16px;
  }
  
  .header {
    flex-direction: column;
    align-items: stretch;
    gap: 16px;
  }
  
  .group-item {
    padding: 12px 16px;
  }
  
  .group-avatar {
    margin-right: 12px;
  }
  
  .group-meta {
    flex-direction: column;
    gap: 4px;
  }
}
</style>