<template>
  <div class="group-detail-page">
    <!-- 面包屑导航 -->
    <div class="breadcrumb-nav">
      <el-button type="text" @click="router.push('/group-list')" class="breadcrumb-btn">
        <el-icon><ArrowLeft /></el-icon>
        返回群组列表
      </el-button>
    </div>
    
    <!-- 群组信息头部 -->
    <div class="group-header">
      <div class="group-info">
        <div class="group-avatar">
          <el-icon :size="40"><ChatDotRound /></el-icon>
        </div>
        <div class="group-details">
          <h2 class="group-name">{{ groupInfo.groupName }}</h2>
          <p class="group-description">{{ groupInfo.description || '暂无群组描述' }}</p>
          <p class="group-stats">
            <span>成员数量: {{ groupInfo.memberCount }}</span>
            <span>创建时间: {{ formatDate(groupInfo.createTime) }}</span>
          </p>
        </div>
      </div>
      <div class="group-actions">
        <el-button 
          v-if="canManageGroup" 
          type="primary" 
          @click="editGroupInfo"
          icon="Edit"
        >
          编辑群组
        </el-button>
        <el-button 
          type="success" 
          @click="enterChat"
          icon="ChatDotRound"
        >
          进入聊天
        </el-button>
        <el-dropdown v-if="canManageGroup" @command="handleCommand">
          <el-button icon="MoreFilled"></el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="settings" icon="Setting">群组设置</el-dropdown-item>
              <el-dropdown-item command="dismiss" icon="Delete" style="color: #f56c6c;">解散群组</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <!-- 成员列表 -->
    <div class="members-section">
      <div class="section-header">
        <h3>群组成员 ({{ members.length }})</h3>
        <el-button 
          v-if="canManageGroup" 
          size="small" 
          type="primary" 
          @click="inviteMember"
          icon="Plus"
        >
          邀请成员
        </el-button>
      </div>
      
      <div class="members-list">
        <div 
          v-for="member in members" 
          :key="member.memberId"
          class="member-item"
        >
          <div class="member-info">
            <div class="member-avatar">
              <el-icon><User /></el-icon>
            </div>
            <div class="member-details">
              <div class="member-name">
                {{ member.userName }}
                <el-tag v-if="member.role === 2" type="danger" size="small">群主</el-tag>
                <el-tag v-else-if="member.role === 1" type="warning" size="small">管理员</el-tag>
                <el-tag v-if="member.isMuted" type="info" size="small">已禁言</el-tag>
              </div>
              <div class="member-join-time">
                加入时间: {{ formatDate(member.joinTime) }}
              </div>
            </div>
          </div>
          <div class="member-actions" v-if="canManageMember(member)">
            <el-dropdown @command="(command) => handleMemberCommand(command, member)">
              <el-button size="small" text icon="MoreFilled"></el-button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item 
                    v-if="member.role < 1 && currentUserRole === 2" 
                    command="promote" 
                    icon="Top"
                  >
                    设为管理员
                  </el-dropdown-item>
                  <el-dropdown-item 
                    v-if="member.role === 1 && currentUserRole === 2" 
                    command="demote" 
                    icon="Bottom"
                  >
                    取消管理员
                  </el-dropdown-item>
                  <el-dropdown-item 
                    v-if="!member.isMuted" 
                    command="mute" 
                    icon="MicrophoneSlash"
                  >
                    禁言
                  </el-dropdown-item>
                  <el-dropdown-item 
                    v-if="member.isMuted" 
                    command="unmute" 
                    icon="Microphone"
                  >
                    解除禁言
                  </el-dropdown-item>
                  <el-dropdown-item 
                    command="kick" 
                    icon="CloseBold" 
                    style="color: #f56c6c;"
                  >
                    踢出群组
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
        </div>
      </div>
    </div>

    <!-- 编辑群组信息对话框 -->
    <el-dialog
      v-model="editDialogVisible"
      title="编辑群组信息"
      width="500px"
    >
      <el-form ref="editFormRef" :model="editForm" :rules="editRules" label-width="80px">
        <el-form-item label="群组名称" prop="groupName">
          <el-input v-model="editForm.groupName" placeholder="请输入群组名称" maxlength="50" />
        </el-form-item>
        <el-form-item label="群组描述" prop="description">
          <el-input
            v-model="editForm.description"
            type="textarea"
            :rows="3"
            placeholder="请输入群组描述"
            maxlength="200"
            show-word-limit
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="submitEdit">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ChatDotRound, User, Edit, MoreFilled, Plus, ArrowLeft } from '@element-plus/icons-vue'
import axios from '../components/axios'

const route = useRoute()
const router = useRouter()

interface GroupInfo {
  groupId: number
  groupName: string
  description: string
  createdBy: number
  createTime: string
  memberCount: number
}

interface Member {
  memberId: number
  groupId: number
  userId: number
  userName: string
  joinTime: string
  role: number
  isMuted: boolean
}

// 响应式数据
const groupInfo = ref<GroupInfo>({
  groupId: 0,
  groupName: '',
  description: '',
  createdBy: 0,
  createTime: '',
  memberCount: 0
})

const members = ref<Member[]>([])
const currentUserId = ref(Number(localStorage.getItem('userId')) || 0)
const currentUserRole = ref(0)
const editDialogVisible = ref(false)

const editForm = reactive({
  groupName: '',
  description: ''
})

const editFormRef = ref()
const editRules = {
  groupName: [
    { required: true, message: '请输入群组名称', trigger: 'blur' },
    { min: 1, max: 50, message: '群组名称长度在 1 到 50 个字符', trigger: 'blur' }
  ]
}

// 计算属性
const canManageGroup = computed(() => {
  return currentUserRole.value >= 1 // 管理员或群主
})

const canManageMember = (member: Member) => {
  if (member.userId === currentUserId.value) return false // 不能管理自己
  return currentUserRole.value > member.role // 只能管理比自己权限低的成员
}

// 生命周期
onMounted(() => {
  loadGroupInfo()
  loadMembers()
})

// 方法
const loadGroupInfo = async () => {
  try {
    const groupId = route.params.id
    const response = await axios.get(`/group/${groupId}`)
    groupInfo.value = response.data
  } catch (error) {
    ElMessage.error('加载群组信息失败')
    router.push('/group')
  }
}

const loadMembers = async () => {
  try {
    const groupId = route.params.id
    const response = await axios.get(`/group-member/${groupId}`)
    members.value = response.data
    
    // 找到当前用户的权限
    const currentMember = members.value.find(m => m.userId === currentUserId.value)
    if (currentMember) {
      currentUserRole.value = currentMember.role
    }
  } catch (error) {
    ElMessage.error('加载成员列表失败')
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const enterChat = () => {
  router.push(`/group-chat/${groupInfo.value.groupId}`)
}

const editGroupInfo = () => {
  editForm.groupName = groupInfo.value.groupName
  editForm.description = groupInfo.value.description
  editDialogVisible.value = true
}

const submitEdit = async () => {
  try {
    await editFormRef.value.validate()
    
    await axios.put(`/group-setting/${groupInfo.value.groupId}`, {
      operatorUserId: currentUserId.value,
      groupName: editForm.groupName,
      description: editForm.description
    })
    
    ElMessage.success('群组信息更新成功')
    editDialogVisible.value = false
    loadGroupInfo()
  } catch (error) {
    ElMessage.error('更新群组信息失败')
  }
}

const handleCommand = async (command: string) => {
  if (command === 'dismiss') {
    try {
      await ElMessageBox.confirm(
        '解散群组后，所有成员将被移除，聊天记录将被删除，此操作不可恢复。',
        '确认解散群组',
        {
          confirmButtonText: '确定解散',
          cancelButtonText: '取消',
          type: 'warning',
          confirmButtonClass: 'el-button--danger'
        }
      )
      
      await axios.delete(`/group-setting/${groupInfo.value.groupId}?operatorUserId=${currentUserId.value}`)
      ElMessage.success('群组已解散')
      router.push('/group')
    } catch (error) {
      if (error !== 'cancel') {
        ElMessage.error('解散群组失败')
      }
    }
  }
}

const handleMemberCommand = async (command: string, member: Member) => {
  try {
    switch (command) {
      case 'promote':
        await axios.put(`/api/group-setting/${groupInfo.value.groupId}/member-role`, {
          operatorUserId: currentUserId.value,
          targetUserId: member.userId,
          newRole: 1
        })
        ElMessage.success('设置管理员成功')
        break
      
      case 'demote':
        await axios.put(`/api/group-setting/${groupInfo.value.groupId}/member-role`, {
          operatorUserId: currentUserId.value,
          targetUserId: member.userId,
          newRole: 0
        })
        ElMessage.success('取消管理员成功')
        break
      
      case 'mute':
        await axios.put(`/api/group-setting/${groupInfo.value.groupId}/mute`, {
          operatorUserId: currentUserId.value,
          targetUserId: member.userId,
          isMuted: true
        })
        ElMessage.success('禁言成功')
        break
      
      case 'unmute':
        await axios.put(`/api/group-setting/${groupInfo.value.groupId}/mute`, {
          operatorUserId: currentUserId.value,
          targetUserId: member.userId,
          isMuted: false
        })
        ElMessage.success('解除禁言成功')
        break
      
      case 'kick':
        await ElMessageBox.confirm(
          `确定要将 ${member.userName} 踢出群组吗？`,
          '确认踢出',
          {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
          }
        )
        
        await axios.delete(`/api/group-member/${groupInfo.value.groupId}/kick?targetUserId=${member.userId}&operatorUserId=${currentUserId.value}`)
        ElMessage.success('踢出成功')
        break
    }
    
    loadMembers()
    loadGroupInfo()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

const inviteMember = () => {
  ElMessage.info('邀请成员功能开发中...')
}
</script>

<style scoped>
.group-detail-page {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
}

.breadcrumb-nav {
  margin-bottom: 15px;
}

.breadcrumb-btn {
  color: #409eff;
  font-size: 14px;
  padding: 8px 0;
}

.breadcrumb-btn:hover {
  color: #66b1ff;
}

.group-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
}

.group-info {
  display: flex;
  gap: 15px;
}

.group-avatar {
  width: 60px;
  height: 60px;
  background: #409eff;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.group-details h2 {
  margin: 0 0 8px 0;
  color: #303133;
}

.group-description {
  margin: 0 0 8px 0;
  color: #606266;
  font-size: 14px;
}

.group-stats {
  margin: 0;
  font-size: 12px;
  color: #909399;
}

.group-stats span {
  margin-right: 15px;
}

.group-actions {
  display: flex;
  gap: 10px;
  align-items: center;
}

.members-section {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  padding: 20px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  padding-bottom: 10px;
  border-bottom: 1px solid #ebeef5;
}

.section-header h3 {
  margin: 0;
  color: #303133;
}

.members-list {
  max-height: 400px;
  overflow-y: auto;
}

.member-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #f5f7fa;
}

.member-item:last-child {
  border-bottom: none;
}

.member-info {
  display: flex;
  gap: 12px;
  align-items: center;
}

.member-avatar {
  width: 40px;
  height: 40px;
  background: #f0f2f5;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #909399;
}

.member-name {
  font-weight: 500;
  color: #303133;
  margin-bottom: 4px;
}

.member-join-time {
  font-size: 12px;
  color: #909399;
}

.member-actions {
  opacity: 0;
  transition: opacity 0.2s;
}

.member-item:hover .member-actions {
  opacity: 1;
}
</style>