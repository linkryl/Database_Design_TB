/*
Vite 构建工具配置文件
TreeHole制作组
*/

import {defineConfig} from 'vite'
import vue from '@vitejs/plugin-vue'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import {ElementPlusResolver} from 'unplugin-vue-components/resolvers'
import {resolve} from 'path'

// noinspection JSUnusedGlobalSymbols
export default defineConfig({
    plugins: [
        vue(),
        AutoImport({
            resolvers: [ElementPlusResolver()]
        }),
        Components({
            resolvers: [ElementPlusResolver()]
        })
    ],
    resolve: {
        alias: {
            '@': resolve(__dirname, 'src')
        }
    },
    server: {
        proxy: {
            '/api': {
                // 如果有环境变量VITE_PROXY_TARGET，使用它，否则使用本地开发服务器
                target: process.env.VITE_PROXY_TARGET || 'http://localhost:5101',
                changeOrigin: true,
                secure: false
            }
        },
        fs: {
            allow: ['..']
        }
    }
})
