/*
移动端手势操作组合式函数
TreeHole开发组
*/

import { ref, onMounted, onUnmounted } from 'vue'

export interface SwipeOptions {
  threshold?: number // 触发滑动的最小距离
  velocity?: number  // 触发滑动的最小速度
  direction?: 'horizontal' | 'vertical' | 'all'
}

export interface SwipeCallbacks {
  onSwipeLeft?: () => void
  onSwipeRight?: () => void
  onSwipeUp?: () => void
  onSwipeDown?: () => void
  onPullRefresh?: () => void
  onReachBottom?: () => void
}

export function useSwipeGestures(
  element: HTMLElement | null,
  callbacks: SwipeCallbacks,
  options: SwipeOptions = {}
) {
  const {
    threshold = 50,
    velocity = 0.3,
    direction = 'all'
  } = options

  // 触摸状态
  const touchStart = ref({ x: 0, y: 0, time: 0 })
  const touchEnd = ref({ x: 0, y: 0, time: 0 })
  const isSwiping = ref(false)
  const pullRefreshDistance = ref(0)
  const isPullRefreshing = ref(false)

  // 触摸开始
  const handleTouchStart = (event: TouchEvent) => {
    const touch = event.touches[0]
    touchStart.value = {
      x: touch.clientX,
      y: touch.clientY,
      time: Date.now()
    }
    isSwiping.value = true
    pullRefreshDistance.value = 0
  }

  // 触摸移动
  const handleTouchMove = (event: TouchEvent) => {
    if (!isSwiping.value) return

    const touch = event.touches[0]
    const deltaX = touch.clientX - touchStart.value.x
    const deltaY = touch.clientY - touchStart.value.y

    // 下拉刷新检测
    if (deltaY > 0 && window.scrollY === 0 && callbacks.onPullRefresh) {
      pullRefreshDistance.value = Math.min(deltaY, 100)
      
      if (pullRefreshDistance.value > 60) {
        isPullRefreshing.value = true
      }
      
      // 阻止默认滚动
      event.preventDefault()
    }

    // 上拉加载检测
    if (deltaY < -50 && callbacks.onReachBottom) {
      const scrollHeight = document.documentElement.scrollHeight
      const scrollTop = document.documentElement.scrollTop
      const clientHeight = document.documentElement.clientHeight
      
      if (scrollTop + clientHeight >= scrollHeight - 100) {
        callbacks.onReachBottom()
      }
    }
  }

  // 触摸结束
  const handleTouchEnd = (event: TouchEvent) => {
    if (!isSwiping.value) return

    const touch = event.changedTouches[0]
    touchEnd.value = {
      x: touch.clientX,
      y: touch.clientY,
      time: Date.now()
    }

    const deltaX = touchEnd.value.x - touchStart.value.x
    const deltaY = touchEnd.value.y - touchStart.value.y
    const deltaTime = touchEnd.value.time - touchStart.value.time
    const velocityX = Math.abs(deltaX) / deltaTime
    const velocityY = Math.abs(deltaY) / deltaTime

    // 判断滑动方向和触发回调
    if (Math.abs(deltaX) > threshold && velocityX > velocity) {
      if (direction === 'all' || direction === 'horizontal') {
        if (deltaX > 0 && callbacks.onSwipeRight) {
          callbacks.onSwipeRight()
        } else if (deltaX < 0 && callbacks.onSwipeLeft) {
          callbacks.onSwipeLeft()
        }
      }
    }

    if (Math.abs(deltaY) > threshold && velocityY > velocity) {
      if (direction === 'all' || direction === 'vertical') {
        if (deltaY > 0 && callbacks.onSwipeDown) {
          callbacks.onSwipeDown()
        } else if (deltaY < 0 && callbacks.onSwipeUp) {
          callbacks.onSwipeUp()
        }
      }
    }

    // 处理下拉刷新
    if (isPullRefreshing.value && callbacks.onPullRefresh) {
      callbacks.onPullRefresh()
      isPullRefreshing.value = false
    }

    // 重置状态
    isSwiping.value = false
    pullRefreshDistance.value = 0
  }

  // 绑定事件监听器
  onMounted(() => {
    if (element) {
      element.addEventListener('touchstart', handleTouchStart, { passive: false })
      element.addEventListener('touchmove', handleTouchMove, { passive: false })
      element.addEventListener('touchend', handleTouchEnd, { passive: false })
    }
  })

  // 清理事件监听器
  onUnmounted(() => {
    if (element) {
      element.removeEventListener('touchstart', handleTouchStart)
      element.removeEventListener('touchmove', handleTouchMove)
      element.removeEventListener('touchend', handleTouchEnd)
    }
  })

  return {
    isSwiping,
    pullRefreshDistance,
    isPullRefreshing
  }
}

// 长按手势
export function useLongPress(
  element: HTMLElement | null,
  callback: () => void,
  delay: number = 500
) {
  const isPressed = ref(false)
  const timer = ref<number | null>(null)

  const handleStart = () => {
    isPressed.value = true
    timer.value = window.setTimeout(() => {
      if (isPressed.value) {
        callback()
      }
    }, delay)
  }

  const handleEnd = () => {
    isPressed.value = false
    if (timer.value) {
      clearTimeout(timer.value)
      timer.value = null
    }
  }

  onMounted(() => {
    if (element) {
      element.addEventListener('touchstart', handleStart)
      element.addEventListener('touchend', handleEnd)
      element.addEventListener('touchcancel', handleEnd)
      element.addEventListener('mousedown', handleStart)
      element.addEventListener('mouseup', handleEnd)
      element.addEventListener('mouseleave', handleEnd)
    }
  })

  onUnmounted(() => {
    if (element) {
      element.removeEventListener('touchstart', handleStart)
      element.removeEventListener('touchend', handleEnd)
      element.removeEventListener('touchcancel', handleEnd)
      element.removeEventListener('mousedown', handleStart)
      element.removeEventListener('mouseup', handleEnd)
      element.removeEventListener('mouseleave', handleEnd)
    }
    
    if (timer.value) {
      clearTimeout(timer.value)
    }
  })

  return {
    isPressed
  }
}

// 双击手势
export function useDoubleClick(
  element: HTMLElement | null,
  callback: () => void,
  delay: number = 300
) {
  const clickCount = ref(0)
  const timer = ref<number | null>(null)

  const handleClick = () => {
    clickCount.value++

    if (clickCount.value === 1) {
      timer.value = window.setTimeout(() => {
        clickCount.value = 0
      }, delay)
    } else if (clickCount.value === 2) {
      if (timer.value) {
        clearTimeout(timer.value)
        timer.value = null
      }
      clickCount.value = 0
      callback()
    }
  }

  onMounted(() => {
    if (element) {
      element.addEventListener('click', handleClick)
    }
  })

  onUnmounted(() => {
    if (element) {
      element.removeEventListener('click', handleClick)
    }
    
    if (timer.value) {
      clearTimeout(timer.value)
    }
  })

  return {
    clickCount
  }
}

// 捏合缩放手势
export function usePinchZoom(
  element: HTMLElement | null,
  callbacks: {
    onZoomIn?: (scale: number) => void
    onZoomOut?: (scale: number) => void
    onZoomEnd?: (scale: number) => void
  }
) {
  const isZooming = ref(false)
  const initialDistance = ref(0)
  const currentScale = ref(1)

  const getDistance = (touch1: Touch, touch2: Touch): number => {
    const dx = touch1.clientX - touch2.clientX
    const dy = touch1.clientY - touch2.clientY
    return Math.sqrt(dx * dx + dy * dy)
  }

  const handleTouchStart = (event: TouchEvent) => {
    if (event.touches.length === 2) {
      isZooming.value = true
      initialDistance.value = getDistance(event.touches[0], event.touches[1])
      event.preventDefault()
    }
  }

  const handleTouchMove = (event: TouchEvent) => {
    if (!isZooming.value || event.touches.length !== 2) return

    const currentDistance = getDistance(event.touches[0], event.touches[1])
    const scale = currentDistance / initialDistance.value

    if (scale > 1.1 && callbacks.onZoomIn) {
      callbacks.onZoomIn(scale)
    } else if (scale < 0.9 && callbacks.onZoomOut) {
      callbacks.onZoomOut(scale)
    }

    currentScale.value = scale
    event.preventDefault()
  }

  const handleTouchEnd = (event: TouchEvent) => {
    if (isZooming.value) {
      isZooming.value = false
      
      if (callbacks.onZoomEnd) {
        callbacks.onZoomEnd(currentScale.value)
      }
      
      currentScale.value = 1
    }
  }

  onMounted(() => {
    if (element) {
      element.addEventListener('touchstart', handleTouchStart, { passive: false })
      element.addEventListener('touchmove', handleTouchMove, { passive: false })
      element.addEventListener('touchend', handleTouchEnd, { passive: false })
    }
  })

  onUnmounted(() => {
    if (element) {
      element.removeEventListener('touchstart', handleTouchStart)
      element.removeEventListener('touchmove', handleTouchMove)
      element.removeEventListener('touchend', handleTouchEnd)
    }
  })

  return {
    isZooming,
    currentScale
  }
}
