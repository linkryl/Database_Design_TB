/*
 * Project Name:  UserInterface
 * File Name:     i18n.ts
 * File Function: Vue Internationalization (i18n) 配置文件
 * Author:        宠悦 | PetJoy 项目开发组
 * Update Date:   2024-06-28
 * License:       Creative Commons Attribution 4.0 International License
 */

import {createI18n} from 'vue-i18n'
import zh from '../languages/zh'

const languages = {
    zh  // 汉语（简体中文）支持
}

const defaultLanguage = localStorage.getItem('defaultLanguage') || 'zh'

const i18n = createI18n({
    legacy: false,
    locale: defaultLanguage,
    fallbackLocale: 'zh',
    messages: languages
})

export default i18n
