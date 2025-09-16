<!--
  Project Name:  UserInterface
  File Name:     BarElectionPage.vue
  File Function: 吧主选举页面
  Author:        2351131 韦世贸
-->

<template>
  <div class='election-container'>
    <el-card class='card'>
      <div class='header'>
        <h2>吧主选举</h2>
        <div class='actions'>
          <el-button type='primary' @click='fetchElection'>刷新</el-button>
          <!-- 管理员 / 吧主可结束选举（此示例用 isAdmin 简化权限） -->
          <el-button v-if='isOwnerOrAdmin' type='danger' @click='onCloseElection' :disabled='!election'>结束选举</el-button>
        </div>
      </div>

      <div v-if='!election' class='empty'>
        <p>当前无进行中的选举。</p>
        <!-- 管理员 / 吧主可发起选举（此示例用 isAdmin 简化权限） -->
        <el-button v-if='isOwnerOrAdmin' type='primary' @click='onCreateElection'>发起选举</el-button>
      </div>

      <div v-else>
        <!-- 展示当前选举基本信息 -->
        <p>选举 ID：{{ election.electionId }} | 时间：{{ election.startTime }} ~ {{ election.endTime }}</p>

        <div class='signup'>
          <!-- 报名：可填写竞选宣言 -->
          <el-input v-model='manifesto' placeholder='竞选宣言（可选）' />
          <el-button type='primary' @click='onJoinElection'>报名参选</el-button>
        </div>

        <h3>候选人</h3>
        <el-table :data='candidates' style='width: 100%'>
          <!-- 将用户ID映射为用户名展示 -->
          <el-table-column label='用户名' width='200'>
            <template #default='scope'>
              {{ userNameMap[scope.row.userId] || `用户${scope.row.userId}` }}
            </template>
          </el-table-column>
          <el-table-column prop='manifesto' label='宣言' />
          <el-table-column label='操作' width='160'>
            <template #default='scope'>
              <el-button size='small' type='success' @click='onVote(scope.row.userId)'>投票</el-button>
            </template>
          </el-table-column>
        </el-table>

        <h3 style='margin-top: 16px;'>结果</h3>
        <!-- 实时结果：候选人用户名 + 票数 -->
        <el-table :data='results' style='width: 100%'>
          <el-table-column label='候选人用户名' width='220'>
            <template #default='scope'>
              {{ userNameMap[scope.row.candidateUserId] || `用户${scope.row.candidateUserId}` }}
            </template>
          </el-table-column>
          <el-table-column prop='votes' label='票数' width='120' />
        </el-table>
        <div v-if='results && results.length' class='winner'>
          领先者：{{ userNameMap[results[0].candidateUserId] || `用户${results[0].candidateUserId}` }}（{{ results[0].votes }}
          票）
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup lang='ts'>
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
// 选举相关 API（含用户详情用于映射用户名）
import { createElection, getCurrentElection, getCandidates, joinElection, voteCandidate, closeElection, getElectionResults, getUserById, getBarById } from '../api/index'

const route = useRoute()
const barId = Number(route.params.id)

const election = ref<any | null>(null)
const candidates = ref<any[]>([])
const results = ref<Array<{ candidateUserId: number; votes: number }>>([])
const userNameMap = ref<Record<number, string>>({})
const manifesto = ref('')

// 权限检查：管理员或吧主可发起/结束选举
const isOwnerOrAdmin = ref(false)

// 检查用户权限
const checkUserPermissions = async () => {
  const userRole = localStorage.getItem('userRole')
  const isAdmin = localStorage.getItem('isAdmin') === 'true'
  const currentUserId = localStorage.getItem('currentUserId')
  
  if (isAdmin) {
    isOwnerOrAdmin.value = true
    return
  }
  
  // 检查是否是当前贴吧的吧主
  try {
    const barResponse = await getBarById(barId)
    if (barResponse && currentUserId && parseInt(currentUserId) === barResponse.ownerId) {
      isOwnerOrAdmin.value = true
    }
  } catch (error) {
    console.error('检查吧主权限失败:', error)
  }
}

const fetchElection = async () => {
  try {
    election.value = await getCurrentElection(barId)
    await fetchCandidates()
    await fetchResults()
  } catch {
    election.value = null
    candidates.value = []
    results.value = []
  }
}

const fetchCandidates = async () => {
  if (!election.value) return
  const list = await getCandidates(election.value.electionId)
  candidates.value = list
  const userIds: number[] = Array.from(new Set(list.map((c: any) => c.userId)))
  await ensureUsernames(userIds)
}

const fetchResults = async () => {
  if (!election.value) return
  const list = await getElectionResults(election.value.electionId)
  results.value = list
  const userIds: number[] = Array.from(new Set(list.map((r: any) => r.candidateUserId)))
  await ensureUsernames(userIds)
}

const onCreateElection = async () => {
  try {
    const now = new Date()
    const end = new Date(now.getTime() + 3 * 24 * 60 * 60 * 1000)
    await createElection(barId, { startTime: now.toISOString(), endTime: end.toISOString() })
    ElMessage.success('选举已发起')
    await fetchElection()
  } catch (e: any) {
    ElMessage.error(e?.response?.data || '发起失败')
  }
}

const onJoinElection = async () => {
  if (!election.value) return
  try {
    await joinElection(election.value.electionId, { manifesto: manifesto.value })
    ElMessage.success('报名成功')
    await fetchCandidates()
  } catch (e: any) {
    ElMessage.error(e?.response?.data || '报名失败')
  }
}

const onVote = async (candidateUserId: number) => {
  if (!election.value) return
  try {
    await voteCandidate(election.value.electionId, candidateUserId)
    ElMessage.success('投票成功')
  } catch (e: any) {
    ElMessage.error(e?.response?.data || '投票失败')
  }
}

const onCloseElection = async () => {
  if (!election.value) return
  try {
    await ElMessageBox.confirm('确定结束本次选举并更新吧主吗？', '提示', { type: 'warning' })
    const res = await closeElection(election.value.electionId)
    ElMessage.success(res?.message || '已结束选举')
    await fetchElection()
  } catch { }
}

onMounted(async () => {
  await checkUserPermissions()
  await fetchElection()
})

// 拉取用户名，带缓存
const ensureUsernames = async (userIds: number[]) => {
  const missing = userIds.filter(id => !(id in userNameMap.value))
  if (missing.length === 0) return
  await Promise.all(missing.map(async (id) => {
    try {
      const user = await getUserById(id)
      userNameMap.value[id] = user.userName || `用户${id}`
    } catch {
      userNameMap.value[id] = `用户${id}`
    }
  }))
}
</script>

<style scoped>
.election-container {
  padding: 16px;
}

.card {
  max-width: 900px;
  margin: 0 auto;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.actions {
  display: flex;
  gap: 8px;
}

.empty {
  text-align: center;
  color: #666;
}

.signup {
  display: flex;
  gap: 8px;
  margin: 12px 0;
}
</style>
