import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
// 保留暗黑模式主题文件
import 'element-plus/theme-chalk/dark/css-vars.css'

const app = createApp(App)

// 挂载应用
app.mount('#app')
