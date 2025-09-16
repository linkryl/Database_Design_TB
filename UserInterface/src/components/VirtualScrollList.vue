<!--
虚拟滚动列表组件 - 优化大量数据渲染性能
TreeHole开发组
-->

<template>
  <div class="virtual-scroll-container" ref="containerRef" @scroll="handleScroll">
    <!-- 滚动内容容器 -->
    <div class="scroll-content" :style="{ height: `${totalHeight}px` }">
      <!-- 可视区域内容 -->
      <div 
        class="visible-content" 
        :style="{ 
          transform: `translateY(${offsetY}px)`,
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0
        }"
      >
        <div
          v-for="item in visibleItems"
          :key="getItemKey(item)"
          class="virtual-item"
          :style="{ height: `${itemHeight}px` }"
        >
          <slot :item="item" :index="item.virtualIndex"></slot>
        </div>
      </div>
    </div>

    <!-- 加载指示器 -->
    <div v-if="loading" class="loading-indicator">
      <el-skeleton :rows="3" animated />
    </div>

    <!-- 下拉刷新指示器 -->
    <div 
      v-if="pullRefreshDistance > 0" 
      class="pull-refresh-indicator"
      :style="{ transform: `translateY(${Math.min(pullRefreshDistance, 80)}px)` }"
    >
      <div class="refresh-content">
        <div class="refresh-icon" :class="{ spinning: isPullRefreshing }">
          {{ isPullRefreshing ? '🔄' : '⬇️' }}
        </div>
        <div class="refresh-text">
          {{ isPullRefreshing ? '正在刷新...' : pullRefreshDistance > 60 ? '松开刷新' : '下拉刷新' }}
        </div>
      </div>
    </div>

    <!-- 触底加载指示器 -->
    <div v-if="reachingBottom" class="bottom-loading">
      <el-skeleton :rows="2" animated />
    </div>
  </div>
</template>

<script setup lang='ts'>
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { useSwipeGestures } from '@/composables/useSwipeGestures'

// Props
const props = defineProps<{
  items: any[]
  itemHeight: number
  bufferSize?: number
  keyField?: string
  loading?: boolean
  hasMore?: boolean
}>()

// Emits
const emit = defineEmits<{
  'load-more': []
  'refresh': []
  'item-click': [item: any, index: number]
}>()

// 响应式数据
const containerRef = ref<HTMLElement>()
const scrollTop = ref(0)
const containerHeight = ref(0)
const bufferSize = computed(() => props.bufferSize || 5)
const reachingBottom = ref(false)

// 虚拟滚动计算
const totalHeight = computed(() => props.items.length * props.itemHeight)

const visibleStart = computed(() => {
  const start = Math.floor(scrollTop.value / props.itemHeight) - bufferSize.value
  return Math.max(0, start)
})

const visibleEnd = computed(() => {
  const visibleCount = Math.ceil(containerHeight.value / props.itemHeight)
  const end = visibleStart.value + visibleCount + bufferSize.value * 2
  return Math.min(props.items.length, end)
})

const visibleItems = computed(() => {
  return props.items.slice(visibleStart.value, visibleEnd.value).map((item, index) => ({
    ...item,
    virtualIndex: visibleStart.value + index
  }))
})

const offsetY = computed(() => visibleStart.value * props.itemHeight)

// 手势操作
const { pullRefreshDistance, isPullRefreshing } = useSwipeGestures(
  containerRef.value,
  {
    onPullRefresh: () => {
      emit('refresh')
    },
    onReachBottom: () => {
      if (props.hasMore && !reachingBottom.value) {
        reachingBottom.value = true
        emit('load-more')
      }
    }
  },
  {
    threshold: 30,
    direction: 'vertical'
  }
)

// 组件挂载时初始化
onMounted(() => {
  updateContainerHeight()
  window.addEventListener('resize', updateContainerHeight)
})

onUnmounted(() => {
  window.removeEventListener('resize', updateContainerHeight)
})

// 监听加载状态变化
watch(() => props.loading, (newLoading) => {
  if (!newLoading) {
    reachingBottom.value = false
  }
})

// 滚动处理
const handleScroll = (event: Event) => {
  const target = event.target as HTMLElement
  scrollTop.value = target.scrollTop
  
  // 检测是否接近底部
  const scrollHeight = target.scrollHeight
  const scrollTop = target.scrollTop
  const clientHeight = target.clientHeight
  
  if (scrollTop + clientHeight >= scrollHeight - 100) {
    if (props.hasMore && !reachingBottom.value && !props.loading) {
      reachingBottom.value = true
      emit('load-more')
    }
  }
}

// 更新容器高度
const updateContainerHeight = () => {
  if (containerRef.value) {
    containerHeight.value = containerRef.value.clientHeight
  }
}

// 获取项目唯一键
const getItemKey = (item: any): string | number => {
  if (props.keyField && item[props.keyField]) {
    return item[props.keyField]
  }
  return item.id || item.key || JSON.stringify(item)
}

// 滚动到指定项目
const scrollToItem = (index: number) => {
  if (containerRef.value) {
    const targetScrollTop = index * props.itemHeight
    containerRef.value.scrollTo({
      top: targetScrollTop,
      behavior: 'smooth'
    })
  }
}

// 滚动到顶部
const scrollToTop = () => {
  if (containerRef.value) {
    containerRef.value.scrollTo({
      top: 0,
      behavior: 'smooth'
    })
  }
}

// 滚动到底部
const scrollToBottom = () => {
  if (containerRef.value) {
    containerRef.value.scrollTo({
      top: totalHeight.value,
      behavior: 'smooth'
    })
  }
}

// 暴露方法给父组件
defineExpose({
  scrollToItem,
  scrollToTop,
  scrollToBottom,
  refresh: () => {
    scrollTop.value = 0
    updateContainerHeight()
  }
})
</script>

<style scoped>
.virtual-scroll-container {
  height: 100%;
  overflow-y: auto;
  position: relative;
  -webkit-overflow-scrolling: touch; /* iOS 平滑滚动 */
}

.scroll-content {
  position: relative;
  width: 100%;
}

.visible-content {
  width: 100%;
}

.virtual-item {
  width: 100%;
  box-sizing: border-box;
}

/* 加载指示器 */
.loading-indicator {
  padding: 20px;
  text-align: center;
}

/* 下拉刷新指示器 */
.pull-refresh-indicator {
  position: absolute;
  top: -80px;
  left: 0;
  right: 0;
  height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: white;
  z-index: 10;
}

.refresh-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.refresh-icon {
  font-size: 20px;
  transition: transform 0.3s ease;
}

.refresh-icon.spinning {
  animation: spin 1s linear infinite;
}

.refresh-text {
  font-size: 14px;
  color: #666;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* 触底加载指示器 */
.bottom-loading {
  padding: 20px;
  text-align: center;
  background: white;
}

/* 滚动条样式 */
.virtual-scroll-container::-webkit-scrollbar {
  width: 6px;
}

.virtual-scroll-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.virtual-scroll-container::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.virtual-scroll-container::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .virtual-scroll-container {
    -webkit-overflow-scrolling: touch;
    scroll-behavior: smooth;
  }
  
  .pull-refresh-indicator {
    top: -60px;
    height: 60px;
  }
  
  .refresh-icon {
    font-size: 18px;
  }
  
  .refresh-text {
    font-size: 13px;
  }
}

/* 性能优化 */
.virtual-item {
  contain: layout style paint;
  will-change: transform;
}

.visible-content {
  contain: layout style paint;
  will-change: transform;
}

/* 平滑滚动 */
@supports (scroll-behavior: smooth) {
  .virtual-scroll-container {
    scroll-behavior: smooth;
  }
}

/* 深色模式支持 */
@media (prefers-color-scheme: dark) {
  .pull-refresh-indicator,
  .bottom-loading {
    background: #2c2c2c;
  }
  
  .refresh-text {
    color: #ccc;
  }
  
  .virtual-scroll-container::-webkit-scrollbar-track {
    background: #404040;
  }
  
  .virtual-scroll-container::-webkit-scrollbar-thumb {
    background: #666;
  }
  
  .virtual-scroll-container::-webkit-scrollbar-thumb:hover {
    background: #777;
  }
}
</style>
