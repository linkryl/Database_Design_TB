<!--
高级富文本编辑器组件 - 支持多媒体内容和高级功能
TreeHole开发组
-->

<template>
  <div class="advanced-editor">
    <!-- 编辑器工具栏 -->
    <div class="editor-toolbar">
      <div class="toolbar-section">
        <el-button-group size="small">
          <el-button @click="insertBold" :class="{ active: isActive('bold') }">
            <strong>B</strong>
          </el-button>
          <el-button @click="insertItalic" :class="{ active: isActive('italic') }">
            <em>I</em>
          </el-button>
          <el-button @click="insertUnderline" :class="{ active: isActive('underline') }">
            <u>U</u>
          </el-button>
          <el-button @click="insertStrikethrough" :class="{ active: isActive('strikethrough') }">
            <s>S</s>
          </el-button>
        </el-button-group>
      </div>

      <div class="toolbar-section">
        <el-select v-model="currentFontSize" @change="changeFontSize" size="small" style="width: 80px">
          <el-option label="12px" value="12px" />
          <el-option label="14px" value="14px" />
          <el-option label="16px" value="16px" />
          <el-option label="18px" value="18px" />
          <el-option label="20px" value="20px" />
          <el-option label="24px" value="24px" />
        </el-select>
        
        <el-color-picker v-model="currentColor" @change="changeTextColor" size="small" />
      </div>

      <div class="toolbar-section">
        <el-button-group size="small">
          <el-button @click="insertOrderedList" :class="{ active: isActive('orderedList') }">
            📝
          </el-button>
          <el-button @click="insertUnorderedList" :class="{ active: isActive('unorderedList') }">
            📋
          </el-button>
          <el-button @click="insertQuote" :class="{ active: isActive('quote') }">
            💬
          </el-button>
          <el-button @click="insertCode">
            💻
          </el-button>
        </el-button-group>
      </div>

      <div class="toolbar-section">
        <el-button size="small" @click="showImageUpload = true">
          🖼️ 图片
        </el-button>
        <el-button size="small" @click="showVideoUpload = true">
          🎥 视频
        </el-button>
        <el-button size="small" @click="showEmojiPicker = true">
          😀 表情
        </el-button>
        <el-button size="small" @click="showLinkDialog = true">
          🔗 链接
        </el-button>
      </div>

      <div class="toolbar-section">
        <el-button size="small" @click="insertTable">
          📊 表格
        </el-button>
        <el-button size="small" @click="showMentionDialog = true">
          @ 提及
        </el-button>
        <el-button size="small" @click="showTopicDialog = true">
          # 话题
        </el-button>
      </div>

      <div class="toolbar-section">
        <el-button size="small" @click="togglePreview">
          {{ showPreview ? '编辑' : '预览' }}
        </el-button>
        <el-button size="small" @click="toggleFullscreen">
          {{ isFullscreen ? '退出全屏' : '全屏' }}
        </el-button>
      </div>
    </div>

    <!-- 编辑器内容区 -->
    <div class="editor-container" :class="{ fullscreen: isFullscreen, preview: showPreview }">
      <!-- 编辑模式 -->
      <div v-show="!showPreview" class="editor-content">
        <div
          ref="editorRef"
          class="editor-textarea"
          :contenteditable="true"
          @input="handleInput"
          @keydown="handleKeydown"
          @paste="handlePaste"
          @focus="handleFocus"
          @blur="handleBlur"
          :placeholder="placeholder"
        ></div>
        
        <!-- 字数统计 -->
        <div class="editor-status">
          <span class="word-count">
            {{ wordCount }}/{{ maxLength }} 字
          </span>
          <span v-if="wordCount > maxLength" class="over-limit">
            超出限制
          </span>
        </div>
      </div>

      <!-- 预览模式 -->
      <div v-show="showPreview" class="preview-content">
        <div class="preview-area" v-html="previewHtml"></div>
      </div>
    </div>

    <!-- 媒体文件列表 -->
    <div v-if="mediaFiles.length > 0" class="media-list">
      <h4>已添加的媒体文件</h4>
      <div class="media-items">
        <div
          v-for="(file, index) in mediaFiles"
          :key="file.id"
          class="media-item"
        >
          <div class="media-preview">
            <img v-if="file.type === 'image'" :src="file.url" :alt="file.name" />
            <video v-else-if="file.type === 'video'" :src="file.url" controls></video>
            <div v-else class="file-icon">📄</div>
          </div>
          
          <div class="media-info">
            <div class="media-name">{{ file.name }}</div>
            <div class="media-size">{{ formatFileSize(file.size) }}</div>
          </div>
          
          <div class="media-actions">
            <el-button size="small" @click="insertMediaToEditor(file)">
              插入
            </el-button>
            <el-button size="small" type="danger" @click="removeMedia(index)">
              删除
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 图片上传对话框 -->
    <el-dialog v-model="showImageUpload" title="上传图片" width="500px">
      <el-upload
        class="upload-demo"
        drag
        :action="uploadUrl"
        :headers="uploadHeaders"
        :before-upload="beforeImageUpload"
        :on-success="handleImageSuccess"
        :on-error="handleUploadError"
        accept="image/*"
        multiple
      >
        <div class="upload-area">
          <div class="upload-icon">🖼️</div>
          <div class="upload-text">
            <p>将图片拖到此处，或<em>点击上传</em></p>
            <p class="upload-tip">支持 JPG、PNG、GIF 格式，单个文件不超过 10MB</p>
          </div>
        </div>
      </el-upload>
    </el-dialog>

    <!-- 视频上传对话框 -->
    <el-dialog v-model="showVideoUpload" title="上传视频" width="500px">
      <el-upload
        class="upload-demo"
        drag
        :action="uploadUrl"
        :headers="uploadHeaders"
        :before-upload="beforeVideoUpload"
        :on-success="handleVideoSuccess"
        :on-error="handleUploadError"
        accept="video/*"
      >
        <div class="upload-area">
          <div class="upload-icon">🎥</div>
          <div class="upload-text">
            <p>将视频拖到此处，或<em>点击上传</em></p>
            <p class="upload-tip">支持 MP4、AVI、MOV 格式，单个文件不超过 100MB</p>
          </div>
        </div>
      </el-upload>
    </el-dialog>

    <!-- 表情选择器 -->
    <el-dialog v-model="showEmojiPicker" title="选择表情" width="400px">
      <div class="emoji-picker">
        <div class="emoji-categories">
          <div
            v-for="category in emojiCategories"
            :key="category.name"
            class="emoji-category"
            :class="{ active: activeEmojiCategory === category.name }"
            @click="activeEmojiCategory = category.name"
          >
            {{ category.icon }}
          </div>
        </div>
        
        <div class="emoji-grid">
          <div
            v-for="emoji in currentEmojis"
            :key="emoji.code"
            class="emoji-item"
            @click="insertEmoji(emoji)"
            :title="emoji.name"
          >
            {{ emoji.emoji }}
          </div>
        </div>
      </div>
    </el-dialog>

    <!-- 链接插入对话框 -->
    <el-dialog v-model="showLinkDialog" title="插入链接" width="400px">
      <el-form :model="linkForm" label-width="80px">
        <el-form-item label="链接文字">
          <el-input v-model="linkForm.text" placeholder="请输入链接显示文字" />
        </el-form-item>
        <el-form-item label="链接地址">
          <el-input v-model="linkForm.url" placeholder="请输入链接地址" />
        </el-form-item>
        <el-form-item label="打开方式">
          <el-radio-group v-model="linkForm.target">
            <el-radio value="_self">当前窗口</el-radio>
            <el-radio value="_blank">新窗口</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
      
      <template #footer>
        <el-button @click="showLinkDialog = false">取消</el-button>
        <el-button type="primary" @click="insertLink">确定</el-button>
      </template>
    </el-dialog>

    <!-- @提及对话框 -->
    <el-dialog v-model="showMentionDialog" title="@提及用户" width="400px">
      <el-input
        v-model="mentionSearch"
        placeholder="搜索用户名"
        @input="searchUsers"
      />
      
      <div class="mention-list" v-if="mentionUsers.length > 0">
        <div
          v-for="user in mentionUsers"
          :key="user.userId"
          class="mention-item"
          @click="insertMention(user)"
        >
          <img :src="user.avatarUrl || '/images/default-avatar.png'" class="mention-avatar" />
          <div class="mention-info">
            <div class="mention-name">{{ user.userName }}</div>
            <div class="mention-level">Lv.{{ user.level || 1 }}</div>
          </div>
        </div>
      </div>
    </el-dialog>

    <!-- #话题对话框 -->
    <el-dialog v-model="showTopicDialog" title="添加话题标签" width="400px">
      <el-input
        v-model="topicInput"
        placeholder="输入话题名称"
        @keyup.enter="insertTopic"
      />
      
      <div class="topic-suggestions" v-if="hotTopics.length > 0">
        <h4>热门话题</h4>
        <el-tag
          v-for="topic in hotTopics"
          :key="topic.name"
          class="topic-tag"
          @click="insertTopicTag(topic)"
        >
          #{{ topic.name }}
        </el-tag>
      </div>
      
      <template #footer>
        <el-button @click="showTopicDialog = false">取消</el-button>
        <el-button type="primary" @click="insertTopic">添加话题</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang='ts'>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import axios from 'axios'

// Props
const props = defineProps<{
  modelValue: string
  placeholder?: string
  maxLength?: number
  allowMedia?: boolean
  allowMention?: boolean
  allowTopic?: boolean
}>()

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: string]
  'media-added': [files: any[]]
  'mention-added': [user: any]
  'topic-added': [topic: string]
}>()

// 响应式数据
const editorRef = ref<HTMLElement>()
const content = ref(props.modelValue || '')
const showPreview = ref(false)
const isFullscreen = ref(false)
const isFocused = ref(false)

// 工具栏状态
const currentFontSize = ref('16px')
const currentColor = ref('#333333')

// 对话框状态
const showImageUpload = ref(false)
const showVideoUpload = ref(false)
const showEmojiPicker = ref(false)
const showLinkDialog = ref(false)
const showMentionDialog = ref(false)
const showTopicDialog = ref(false)

// 媒体文件
const mediaFiles = ref<any[]>([])

// 表情数据
const activeEmojiCategory = ref('smileys')
const emojiCategories = ref([
  { name: 'smileys', icon: '😀', emojis: [] },
  { name: 'people', icon: '👤', emojis: [] },
  { name: 'nature', icon: '🌿', emojis: [] },
  { name: 'food', icon: '🍔', emojis: [] },
  { name: 'activities', icon: '⚽', emojis: [] },
  { name: 'travel', icon: '🚗', emojis: [] },
  { name: 'objects', icon: '💡', emojis: [] },
  { name: 'symbols', icon: '❤️', emojis: [] },
])

// 链接表单
const linkForm = ref({
  text: '',
  url: '',
  target: '_blank'
})

// @提及相关
const mentionSearch = ref('')
const mentionUsers = ref<any[]>([])

// 话题相关
const topicInput = ref('')
const hotTopics = ref<any[]>([])

// 上传配置
const uploadUrl = '/api/upload/file'
const uploadHeaders = computed(() => {
  const token = localStorage.getItem('jwtToken')
  return token ? { Authorization: `Bearer ${token}` } : {}
})

// 计算属性
const wordCount = computed(() => {
  const text = editorRef.value?.innerText || ''
  return text.length
})

const maxLength = computed(() => props.maxLength || 2000)

const previewHtml = computed(() => {
  return processContentForPreview(content.value)
})

const currentEmojis = computed(() => {
  const category = emojiCategories.value.find(c => c.name === activeEmojiCategory.value)
  return category?.emojis || []
})

// 监听内容变化
watch(content, (newValue) => {
  emit('update:modelValue', newValue)
})

watch(() => props.modelValue, (newValue) => {
  if (newValue !== content.value) {
    content.value = newValue
    if (editorRef.value) {
      editorRef.value.innerHTML = newValue
    }
  }
})

// 组件挂载时初始化
onMounted(async () => {
  await loadEmojiData()
  await loadHotTopics()
  
  if (editorRef.value && props.modelValue) {
    editorRef.value.innerHTML = props.modelValue
  }
})

// 编辑器事件处理
const handleInput = (event: Event) => {
  const target = event.target as HTMLElement
  content.value = target.innerHTML
}

const handleKeydown = (event: KeyboardEvent) => {
  // Ctrl+B 加粗
  if (event.ctrlKey && event.key === 'b') {
    event.preventDefault()
    insertBold()
  }
  
  // Ctrl+I 斜体
  if (event.ctrlKey && event.key === 'i') {
    event.preventDefault()
    insertItalic()
  }
  
  // Ctrl+U 下划线
  if (event.ctrlKey && event.key === 'u') {
    event.preventDefault()
    insertUnderline()
  }
  
  // Enter 键处理
  if (event.key === 'Enter' && !event.shiftKey) {
    // 可以在这里添加特殊的回车处理逻辑
  }
}

const handlePaste = async (event: ClipboardEvent) => {
  event.preventDefault()
  
  const clipboardData = event.clipboardData
  if (!clipboardData) return
  
  // 处理图片粘贴
  const items = Array.from(clipboardData.items)
  for (const item of items) {
    if (item.type.startsWith('image/')) {
      const file = item.getAsFile()
      if (file) {
        await handlePastedImage(file)
        return
      }
    }
  }
  
  // 处理文本粘贴
  const text = clipboardData.getData('text/plain')
  if (text) {
    insertText(text)
  }
}

const handleFocus = () => {
  isFocused.value = true
}

const handleBlur = () => {
  isFocused.value = false
}

// 格式化功能
const insertBold = () => {
  document.execCommand('bold', false)
  focusEditor()
}

const insertItalic = () => {
  document.execCommand('italic', false)
  focusEditor()
}

const insertUnderline = () => {
  document.execCommand('underline', false)
  focusEditor()
}

const insertStrikethrough = () => {
  document.execCommand('strikeThrough', false)
  focusEditor()
}

const changeFontSize = (size: string) => {
  document.execCommand('fontSize', false, '7')
  const fontElements = editorRef.value?.querySelectorAll('font[size="7"]')
  fontElements?.forEach(el => {
    el.removeAttribute('size')
    ;(el as HTMLElement).style.fontSize = size
  })
  focusEditor()
}

const changeTextColor = (color: string) => {
  document.execCommand('foreColor', false, color)
  focusEditor()
}

const insertOrderedList = () => {
  document.execCommand('insertOrderedList', false)
  focusEditor()
}

const insertUnorderedList = () => {
  document.execCommand('insertUnorderedList', false)
  focusEditor()
}

const insertQuote = () => {
  const selection = window.getSelection()
  if (selection && selection.rangeCount > 0) {
    const range = selection.getRangeAt(0)
    const quotedText = `<blockquote>${range.toString() || '引用内容'}</blockquote>`
    insertHtml(quotedText)
  }
}

const insertCode = () => {
  const selection = window.getSelection()
  if (selection && selection.rangeCount > 0) {
    const range = selection.getRangeAt(0)
    const codeText = `<code>${range.toString() || '代码'}</code>`
    insertHtml(codeText)
  }
}

const insertTable = () => {
  const tableHtml = `
    <table border="1" style="border-collapse: collapse; width: 100%;">
      <tr>
        <th>标题1</th>
        <th>标题2</th>
        <th>标题3</th>
      </tr>
      <tr>
        <td>内容1</td>
        <td>内容2</td>
        <td>内容3</td>
      </tr>
    </table>
  `
  insertHtml(tableHtml)
}

// 工具函数
const isActive = (command: string): boolean => {
  return document.queryCommandState(command)
}

const insertText = (text: string) => {
  const selection = window.getSelection()
  if (selection && selection.rangeCount > 0) {
    const range = selection.getRangeAt(0)
    range.deleteContents()
    range.insertNode(document.createTextNode(text))
    range.collapse(false)
  }
}

const insertHtml = (html: string) => {
  const selection = window.getSelection()
  if (selection && selection.rangeCount > 0) {
    const range = selection.getRangeAt(0)
    range.deleteContents()
    const div = document.createElement('div')
    div.innerHTML = html
    const fragment = document.createDocumentFragment()
    let node
    while ((node = div.firstChild)) {
      fragment.appendChild(node)
    }
    range.insertNode(fragment)
  }
}

const focusEditor = () => {
  nextTick(() => {
    editorRef.value?.focus()
  })
}

// 媒体处理
const beforeImageUpload = (file: File) => {
  const isImage = file.type.startsWith('image/')
  const isLt10M = file.size / 1024 / 1024 < 10

  if (!isImage) {
    ElMessage.error('只能上传图片文件!')
    return false
  }
  if (!isLt10M) {
    ElMessage.error('图片大小不能超过 10MB!')
    return false
  }
  return true
}

const beforeVideoUpload = (file: File) => {
  const isVideo = file.type.startsWith('video/')
  const isLt100M = file.size / 1024 / 1024 < 100

  if (!isVideo) {
    ElMessage.error('只能上传视频文件!')
    return false
  }
  if (!isLt100M) {
    ElMessage.error('视频大小不能超过 100MB!')
    return false
  }
  return true
}

const handleImageSuccess = (response: any, file: any) => {
  const mediaFile = {
    id: Date.now(),
    name: file.name,
    type: 'image',
    url: response.url,
    size: file.size
  }
  
  mediaFiles.value.push(mediaFile)
  showImageUpload.value = false
  ElMessage.success('图片上传成功')
  
  emit('media-added', [mediaFile])
}

const handleVideoSuccess = (response: any, file: any) => {
  const mediaFile = {
    id: Date.now(),
    name: file.name,
    type: 'video',
    url: response.url,
    size: file.size
  }
  
  mediaFiles.value.push(mediaFile)
  showVideoUpload.value = false
  ElMessage.success('视频上传成功')
  
  emit('media-added', [mediaFile])
}

const handleUploadError = (error: any) => {
  console.error('上传失败:', error)
  ElMessage.error('上传失败，请重试')
}

const handlePastedImage = async (file: File) => {
  if (!beforeImageUpload(file)) return
  
  try {
    const formData = new FormData()
    formData.append('file', file)
    
    const response = await axios.post(uploadUrl, formData, {
      headers: {
        ...uploadHeaders.value,
        'Content-Type': 'multipart/form-data'
      }
    })
    
    const mediaFile = {
      id: Date.now(),
      name: `pasted-image-${Date.now()}.png`,
      type: 'image',
      url: response.data.url,
      size: file.size
    }
    
    mediaFiles.value.push(mediaFile)
    insertMediaToEditor(mediaFile)
    
    ElMessage.success('图片粘贴成功')
  } catch (error) {
    console.error('粘贴图片失败:', error)
    ElMessage.error('粘贴图片失败')
  }
}

const insertMediaToEditor = (file: any) => {
  let html = ''
  
  if (file.type === 'image') {
    html = `<img src="${file.url}" alt="${file.name}" style="max-width: 100%; height: auto;" />`
  } else if (file.type === 'video') {
    html = `<video src="${file.url}" controls style="max-width: 100%; height: auto;"></video>`
  }
  
  if (html) {
    insertHtml(html)
    focusEditor()
  }
}

const removeMedia = (index: number) => {
  mediaFiles.value.splice(index, 1)
}

// 表情处理
const loadEmojiData = async () => {
  // 这里可以从API加载表情数据，现在使用模拟数据
  const sampleEmojis = [
    { code: 'smile', emoji: '😀', name: '微笑' },
    { code: 'laugh', emoji: '😂', name: '大笑' },
    { code: 'heart', emoji: '❤️', name: '爱心' },
    { code: 'thumbs_up', emoji: '👍', name: '点赞' },
    { code: 'fire', emoji: '🔥', name: '火' },
    { code: 'star', emoji: '⭐', name: '星星' }
  ]
  
  emojiCategories.value[0].emojis = sampleEmojis
}

const insertEmoji = (emoji: any) => {
  insertText(emoji.emoji)
  showEmojiPicker.value = false
  focusEditor()
}

// 链接处理
const insertLink = () => {
  if (!linkForm.value.url) {
    ElMessage.warning('请输入链接地址')
    return
  }
  
  const text = linkForm.value.text || linkForm.value.url
  const html = `<a href="${linkForm.value.url}" target="${linkForm.value.target}">${text}</a>`
  
  insertHtml(html)
  showLinkDialog.value = false
  
  // 重置表单
  linkForm.value = {
    text: '',
    url: '',
    target: '_blank'
  }
  
  focusEditor()
}

// @提及处理
const searchUsers = async () => {
  if (!mentionSearch.value || mentionSearch.value.length < 2) {
    mentionUsers.value = []
    return
  }
  
  try {
    const response = await axios.get('/api/search/users', {
      params: { keyword: mentionSearch.value, pageSize: 10 }
    })
    
    mentionUsers.value = response.data.map((user: any) => ({
      ...user,
      level: Math.floor(user.experiencePoints / 1000) + 1
    }))
  } catch (error) {
    console.error('搜索用户失败:', error)
  }
}

const insertMention = (user: any) => {
  const html = `<span class="mention" data-user-id="${user.userId}">@${user.userName}</span>`
  insertHtml(html)
  showMentionDialog.value = false
  mentionSearch.value = ''
  mentionUsers.value = []
  
  emit('mention-added', user)
  focusEditor()
}

// 话题处理
const loadHotTopics = async () => {
  // 模拟热门话题数据
  hotTopics.value = [
    { name: '树洞论坛', count: 1234 },
    { name: '日常分享', count: 856 },
    { name: '技术讨论', count: 642 },
    { name: '生活感悟', count: 523 },
    { name: '学习交流', count: 445 }
  ]
}

const insertTopic = () => {
  if (!topicInput.value.trim()) {
    ElMessage.warning('请输入话题名称')
    return
  }
  
  insertTopicTag({ name: topicInput.value.trim() })
}

const insertTopicTag = (topic: any) => {
  const html = `<span class="topic" data-topic="${topic.name}">#${topic.name}</span>`
  insertHtml(html)
  showTopicDialog.value = false
  topicInput.value = ''
  
  emit('topic-added', topic.name)
  focusEditor()
}

// 预览处理
const processContentForPreview = (html: string): string => {
  // 处理@提及
  html = html.replace(/<span class="mention" data-user-id="(\d+)">(@\w+)<\/span>/g, 
    '<span class="mention-preview">$2</span>')
  
  // 处理话题标签
  html = html.replace(/<span class="topic" data-topic="([^"]+)">(#[^<]+)<\/span>/g, 
    '<span class="topic-preview">$2</span>')
  
  return html
}

const togglePreview = () => {
  showPreview.value = !showPreview.value
}

const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
}

// 工具函数
const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 B'
  
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i]
}

// 暴露方法给父组件
defineExpose({
  focus: () => editorRef.value?.focus(),
  blur: () => editorRef.value?.blur(),
  insertText,
  insertHtml,
  getContent: () => content.value,
  setContent: (html: string) => {
    content.value = html
    if (editorRef.value) {
      editorRef.value.innerHTML = html
    }
  },
  getMediaFiles: () => mediaFiles.value,
  clearContent: () => {
    content.value = ''
    if (editorRef.value) {
      editorRef.value.innerHTML = ''
    }
    mediaFiles.value = []
  }
})
</script>

<style scoped>
.advanced-editor {
  border: 1px solid #e4e7ed;
  border-radius: 8px;
  background: white;
  overflow: hidden;
}

.editor-toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: #f8f9fa;
  border-bottom: 1px solid #e4e7ed;
}

.toolbar-section {
  display: flex;
  align-items: center;
  gap: 8px;
}

.toolbar-section:not(:last-child)::after {
  content: '';
  width: 1px;
  height: 20px;
  background: #e4e7ed;
  margin-left: 8px;
}

.editor-container {
  min-height: 200px;
  position: relative;
}

.editor-container.fullscreen {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 9999;
  background: white;
  min-height: 100vh;
}

.editor-content {
  position: relative;
  height: 100%;
}

.editor-textarea {
  min-height: 200px;
  padding: 16px;
  outline: none;
  line-height: 1.6;
  font-size: 14px;
  color: #333;
  word-break: break-word;
}

.editor-textarea:empty::before {
  content: attr(placeholder);
  color: #c0c4cc;
  pointer-events: none;
}

.editor-status {
  position: absolute;
  bottom: 8px;
  right: 12px;
  font-size: 12px;
  color: #909399;
  display: flex;
  align-items: center;
  gap: 8px;
}

.over-limit {
  color: #f56c6c;
  font-weight: 600;
}

.preview-content {
  min-height: 200px;
  padding: 16px;
  background: #fafafa;
}

.preview-area {
  line-height: 1.6;
  color: #333;
}

/* 媒体文件列表样式 */
.media-list {
  padding: 16px;
  border-top: 1px solid #e4e7ed;
  background: #f8f9fa;
}

.media-list h4 {
  margin: 0 0 12px 0;
  font-size: 14px;
  color: #666;
}

.media-items {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.media-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px;
  background: white;
  border: 1px solid #e4e7ed;
  border-radius: 6px;
  min-width: 200px;
}

.media-preview {
  width: 40px;
  height: 40px;
  border-radius: 4px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f0f0f0;
}

.media-preview img,
.media-preview video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.file-icon {
  font-size: 20px;
}

.media-info {
  flex: 1;
}

.media-name {
  font-size: 12px;
  color: #333;
  margin-bottom: 2px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.media-size {
  font-size: 11px;
  color: #999;
}

.media-actions {
  display: flex;
  gap: 4px;
}

/* 上传区域样式 */
.upload-area {
  text-align: center;
  padding: 40px 20px;
}

.upload-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.upload-text p {
  margin: 8px 0;
  color: #606266;
}

.upload-tip {
  font-size: 12px;
  color: #909399;
}

/* 表情选择器样式 */
.emoji-picker {
  height: 300px;
  display: flex;
  flex-direction: column;
}

.emoji-categories {
  display: flex;
  border-bottom: 1px solid #e4e7ed;
  padding: 8px;
  gap: 4px;
}

.emoji-category {
  padding: 8px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.emoji-category:hover,
.emoji-category.active {
  background: #e8f4fd;
}

.emoji-grid {
  flex: 1;
  padding: 12px;
  display: grid;
  grid-template-columns: repeat(8, 1fr);
  gap: 8px;
  overflow-y: auto;
}

.emoji-item {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  cursor: pointer;
  font-size: 18px;
  transition: background-color 0.2s;
}

.emoji-item:hover {
  background: #f0f0f0;
}

/* @提及样式 */
.mention-list {
  max-height: 200px;
  overflow-y: auto;
  margin-top: 12px;
}

.mention-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px;
  border-radius: 6px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.mention-item:hover {
  background: #f0f0f0;
}

.mention-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.mention-info {
  flex: 1;
}

.mention-name {
  font-size: 14px;
  color: #333;
  margin-bottom: 2px;
}

.mention-level {
  font-size: 12px;
  color: #999;
}

/* 话题样式 */
.topic-suggestions {
  margin-top: 16px;
}

.topic-suggestions h4 {
  margin: 0 0 8px 0;
  font-size: 14px;
  color: #666;
}

.topic-tag {
  margin: 4px;
  cursor: pointer;
}

/* 预览样式 */
:deep(.mention-preview) {
  color: #409eff;
  background: #e8f4fd;
  padding: 2px 4px;
  border-radius: 3px;
  font-weight: 500;
}

:deep(.topic-preview) {
  color: #67c23a;
  background: #f0f9ff;
  padding: 2px 4px;
  border-radius: 3px;
  font-weight: 500;
}

:deep(blockquote) {
  margin: 16px 0;
  padding: 8px 16px;
  background: #f8f9fa;
  border-left: 4px solid #409eff;
  font-style: italic;
  color: #666;
}

:deep(code) {
  background: #f1f2f3;
  padding: 2px 4px;
  border-radius: 3px;
  font-family: 'Courier New', monospace;
  font-size: 13px;
}

:deep(table) {
  width: 100%;
  border-collapse: collapse;
  margin: 16px 0;
}

:deep(table th),
:deep(table td) {
  padding: 8px 12px;
  border: 1px solid #e4e7ed;
  text-align: left;
}

:deep(table th) {
  background: #f8f9fa;
  font-weight: 600;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .editor-toolbar {
    padding: 8px;
    gap: 8px;
  }
  
  .toolbar-section {
    gap: 4px;
  }
  
  .toolbar-section:not(:last-child)::after {
    display: none;
  }
  
  .media-items {
    flex-direction: column;
  }
  
  .media-item {
    min-width: auto;
  }
  
  .emoji-grid {
    grid-template-columns: repeat(6, 1fr);
  }
}

@media (max-width: 480px) {
  .editor-toolbar {
    flex-direction: column;
    align-items: stretch;
  }
  
  .toolbar-section {
    justify-content: center;
  }
  
  .editor-textarea {
    padding: 12px;
    font-size: 16px; /* 防止iOS缩放 */
  }
  
  .emoji-grid {
    grid-template-columns: repeat(5, 1fr);
  }
}
</style>
