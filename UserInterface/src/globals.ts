/*
全局变量文件
TreeHole制作组
*/

import {ref} from 'vue'
import type {Instance} from 'element-plus'

export const apiBaseUrl = '[TODO: apiBaseUrl]'
export const ossBaseUrl = 'src/assets/'
export const gaodeAccessKey = '[TODO: gaodeAccessKey]'
export const isProgressVisible = ref(false)
export const tourRef1 = ref<Instance>()
export const tourRef2 = ref<Instance>()
export const tourRef3 = ref<Instance>()
export const tourRef4 = ref<Instance>()
export const showGuidedTour = ref(false)
export const searchThresholdScore = 0.2

export const carouselImages = [
    `${ossBaseUrl}CarouselImages/CarouselImage1.jpg`,
    `${ossBaseUrl}CarouselImages/CarouselImage2.jpg`,
    `${ossBaseUrl}CarouselImages/CarouselImage3.jpg`,
    `${ossBaseUrl}CarouselImages/CarouselImage4.jpg`,
    `${ossBaseUrl}CarouselImages/CarouselImage5.jpg`,
    `${ossBaseUrl}CarouselImages/CarouselImage6.jpg`
]

export function formatDateTimeToCST(dateTimeStr: string): { date: string, dateTime: string } {
    const date = new Date(dateTimeStr)
    date.setHours(date.getHours() + 8)
    const dateFormatter = new Intl.DateTimeFormat('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        timeZone: 'Asia/Shanghai'
    })
    const timeFormatter = new Intl.DateTimeFormat('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false,
        timeZone: 'Asia/Shanghai'
    })
    return {
        date: dateFormatter.format(date).replace(/\//g, '-'),
        dateTime: timeFormatter.format(date).replace(/\//g, '-').replace(',', '')
    }
}

export function dataUrlToBlob(dataURI: any) {
    const byteString = atob(dataURI.split(',')[1])
    const mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0]
    const ab = new ArrayBuffer(byteString.length)
    const ia = new Uint8Array(ab)
    for (let i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i)
    }
    return new Blob([ab], {type: mimeString})
}
