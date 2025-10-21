<template>
  <el-config-provider :locale="zhCn" :theme="themeConfig">
    <div class="dashboard-container" :class="{ 'dark': isDarkMode }">
      <!-- 顶部导航栏 -->
      <header class="dashboard-header" :class="{ 'dark': isDarkMode }">
        <div class="header-left">
          <el-icon class="dashboard-icon">
            <Timer/>
          </el-icon>
          <h1 class="dashboard-title">Schedule Dashboard</h1>
        </div>
        <div class="header-right">
          <!-- 暗黑模式切换 -->
          <el-button type="text" circle @click="toggleDarkMode"
                     :title="isDarkMode ? '切换到亮色模式' : '切换到暗黑模式'">
            <el-icon :size="20">
              <component :is="isDarkMode ? Sunny : Moon"/>
            </el-icon>
          </el-button>
          <!-- 其他操作按钮 -->
          <el-button type="text" circle>
            <el-icon :size="20">
              <Setting/>
            </el-icon>
          </el-button>
        </div>
      </header>

      <!-- 主要内容区域 -->
      <main class="dashboard-main">
        <div class="table-container">
          <el-table v-loading="loading" :data="scheduleData" style="width: 100%" row-key="id" :border="false"
                    :header-cell-style="headerCellStyle" :cell-style="cellStyle" :row-style="rowStyle"
                    @row-click="handleRowClick" ref="tableRef">
            <!-- 展开行 -->
            <el-table-column type="expand">
              <template #default="{ row }">
                <div class="expand-card" :class="{ 'dark-mode-card': isDarkMode }">
                  <el-timeline style="padding: 20px">

                    <el-timeline-item timestamp="2025/4/12 12:03:01" type="primary" icon="MoreFilled" placement="top">
                      <el-space direction="vertical" alignment="normal">
                        <div class="timeline-container-content">第&nbsp;<el-tag type="success" size="small">58</el-tag>&nbsp;次运行,
                          耗时&nbsp;<el-tag type="success" size="small">1000ms</el-tag>&nbsp;
                        </div>
                        <div class="timeline-container-content">状态：
                          <el-tag type="success"  size="small">完成</el-tag>
                        </div>
                      </el-space>
                    </el-timeline-item>
                    <el-timeline-item timestamp="2025/4/12 12:01:01" placement="top">
                      <el-space direction="vertical" alignment="normal">
                        <div class="timeline-container-content">第&nbsp;<el-tag type="success" size="small">57</el-tag>&nbsp;次运行,
                          耗时&nbsp;<el-tag type="success" size="small">1000ms</el-tag>&nbsp;
                        </div>
                        <div class="timeline-container-content">状态：
                          <el-tag type="warning" style="cursor: pointer"  size="small">异常</el-tag>
                        </div>
                      </el-space>
                    </el-timeline-item>
                    <el-timeline-item timestamp="2025/4/12 12:01:01" placement="top">
                      <el-space direction="vertical" alignment="normal">
                        <div class="timeline-container-content">第&nbsp;<el-tag type="success" size="small">56</el-tag>&nbsp;次运行,
                          耗时&nbsp;<el-tag type="success" size="small">1000ms</el-tag>&nbsp;
                        </div>
                        <div class="timeline-container-content">状态：
                          <el-tag type="success"  size="small">完成</el-tag>
                        </div>
                      </el-space>
                    </el-timeline-item>

                  </el-timeline>
                </div>
              </template>
            </el-table-column>
            <!-- 表格列 -->

            <el-table-column prop="id" label="ID" width="250" fixed="left" align="center">
            </el-table-column>
            <el-table-column prop="jobType" label="任务状态" width="180" align="center">
              <template #default="{ row }">
                <el-tag :type="getJobStatusTagType(row.jobStatus)" size="small">
                  {{ row.jobStatus }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="description" label="描述" width="250" align="center">
            </el-table-column>
            <el-table-column prop="numberOfRuns" label="执行次数" width="130" align="center">
              <template #default="{ row }">
                <el-tag type="success"  size="small">
                  {{ row.numberOfRuns || 0 }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="repeatInterval" label="执行周期" width="180" align="center">
            </el-table-column>
            <el-table-column prop="maxNumberOfRuns" label="最大运行次数" width="180" align="center">
              <template #default="{ row }">
                  {{ row.maxNumberOfRuns === 0 ? '无限制' : row.maxNumberOfRuns }}
              </template>
            </el-table-column>
            <el-table-column prop="numberOfErrors" label="错误数次" width="180" align="center">
            </el-table-column>
            <el-table-column prop="maxNumberOfErrors" label="最大错误次数" width="180" align="center">
              <template #default="{ row }">
                  {{ row.maxNumberOfErrors === 0 ? '无限制' : row.maxNumberOfErrors }}
              </template>
            </el-table-column>
            <el-table-column prop="startTime" label="开始时间" width="250" align="center">
              <template #default="{ row }">
                <el-tag type="danger"  size="small">
                  {{ row.startTime }}
                </el-tag>
              </template>
            </el-table-column>

            <el-table-column prop="lastRunTime" label="最后一次执行时间" width="250" fixed="right" align="center">
              <template #default="{ row }">
                <el-tag type="warning"  size="small">
                  {{ row.lastRunTime }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="nextRunTime" label="下一次执行时间" width="250" fixed="right" align="center">
              <template #default="{ row }">
                <el-tag type="primary"  size="small">
                  {{ row.nextRunTime }}
                </el-tag>
              </template>
            </el-table-column>

            <el-table-column label="操作" fixed="right" align="center">
              <template #default="{ row }">
                <el-dropdown>
                  <el-icon>
                    <MoreFilled/>
                  </el-icon>
                  <template #dropdown>
                    <el-dropdown-menu :class="{ 'dark': isDarkMode }">
                      <el-dropdown-item><el-icon><CircleCheckFilled /></el-icon>启动</el-dropdown-item>
                      <el-dropdown-item><el-icon><WarningFilled /></el-icon>暂停</el-dropdown-item>
                      <el-dropdown-item><el-icon><HelpFilled /></el-icon>立即触发</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>

              </template>
            </el-table-column>

          </el-table>
        </div>
      </main>
    </div>
  </el-config-provider>
</template>

<script setup>
import {ref, computed, onMounted, provide} from 'vue'
import {
  Timer,
  Sunny,
  Moon,
  Setting,
  MoreFilled,
  HelpFilled,
  CircleCheckFilled,
  WarningFilled
} from '@element-plus/icons-vue'
import zhCn from 'element-plus/dist/locale/zh-cn.mjs'

// 响应式数据
const isDarkMode = ref(false) // 默认使用亮色模式，更符合常规用户体验
const loading = ref(false)
const tableRef = ref(null)

// 模拟数据
const scheduleData = [
  {
    id: 'job-1',
    jobStatus: '在线',
    runOnStart: true,
    startTime: '2025-01-01 00:00:00',
    lastRunTime: '2023-05-10 14:30:00 「几秒前」',
    nextRunTime: '2023-05-10 16:30:00 「几秒内」',
    numberOfRuns: 101,
    numberOfErrors: 0,
    maxNumberOfRuns: 0,
    maxNumberOfErrors: 100,
    description: 'Sample Job 1',
    repeatInterval: "DailyAt(10)",
  }, {
    id: 'job-2',
    jobStatus: '在线',
    runOnStart: true,
    startTime: '2025-01-01 00:00:00',
    lastRunTime: '2023-05-10 14:30:00 「几秒前」',
    nextRunTime: '2023-05-10 16:30:00 「几秒内」',
    numberOfRuns: 101,
    numberOfErrors: 0,
    maxNumberOfRuns: 1000,
    maxNumberOfErrors: 0,
    description: 'Sample Job 1',
    repeatInterval: "DailyAt(10)",
  }, {
    id: 'job-3',
    jobStatus: '在线',
    runOnStart: true,
    startTime: '2025-01-01 00:00:00',
    lastRunTime: '2023-05-10 14:30:00 「几秒前」',
    nextRunTime: '2023-05-10 16:30:00 「几秒内」',
    numberOfErrors: 0,
    maxNumberOfRuns: 0,
    maxNumberOfErrors: 100,
    numberOfRuns: 2311,
    description: 'Sample Job 1',
    repeatInterval: "DailyAt(10)",

  }, {
    id: 'job-4',
    jobStatus: '在线',
    runOnStart: true,
    startTime: '2025-01-01 00:00:00',
    lastRunTime: '2023-05-10 14:30:00 「几秒前」',
    nextRunTime: '2023-05-10 16:30:00 「几秒内」',
    numberOfRuns: 101,
    numberOfErrors: 0,
    maxNumberOfRuns: 1000,
    maxNumberOfErrors: 100,
    description: 'Sample Job 1',
    repeatInterval: "DailyAt(10)",
  }, {
    id: 'job-5',
    jobStatus: '在线',
    runOnStart: true,
    startTime: '2025-01-01 00:00:00',
    lastRunTime: '2023-05-10 14:30:00 「几秒前」',
    nextRunTime: '2023-05-10 16:30:00 「几秒内」',
    numberOfRuns: 101,
    numberOfErrors: 0,
    maxNumberOfRuns: 1000,
    maxNumberOfErrors: 100,
    description: 'Sample Job 1',
    repeatInterval: "DailyAt(10)",
  }, {
    id: 'job-6',
    jobStatus: '离线',
    runOnStart: true,
    startTime: '2025-01-01 00:00:00',
    lastRunTime: '2023-05-10 14:30:00 「几秒前」',
    nextRunTime: '2023-05-10 16:30:00 「几秒内」',
    numberOfErrors: 0,
    maxNumberOfRuns: 1000,
    maxNumberOfErrors: 100,
    numberOfRuns: 101,
    description: 'Sample Job 1',
    repeatInterval: "DailyAt(10)",
  },
]

// 计算属性
const themeConfig = computed(() => ({
  dark: isDarkMode.value,
  colorPrimary: '#409EFF'
}))

// 表格计算属性
const headerCellStyle = computed(() => ({
  backgroundColor: isDarkMode.value ? '#1f2937' : '#f5f7fa',
  color: isDarkMode.value ? '#e5e7eb' : '#303133',
  padding: '20px 16px',
  borderBottom: isDarkMode.value ? '1px solid #374151' : '1px solid #ebeef5',
  // borderRight: isDarkMode.value ? '1px solid #374151' : '1px solid #ebeef5',
}))

const cellStyle = computed(() => ({
  color: isDarkMode.value ? '#d1d5db' : '#606266',
  borderColor: isDarkMode.value ? '#374151' : '#ebeef5',
  padding: '16px 12px',
  backgroundColor: isDarkMode.value ? '#232629' : '#ffffff',
  borderBottom: isDarkMode.value ? '1px solid #374151' : '1px solid #ebeef5'
}))

const rowStyle = computed(() => ({
  backgroundColor: isDarkMode.value ? '#111827' : '#ffffff',
  cursor: 'pointer'
}))

// 方法
const toggleDarkMode = () => {
  isDarkMode.value = !isDarkMode.value
  localStorage.setItem('themePreference', isDarkMode.value ? 'dark' : 'light')
  updateDarkModeClass()
}

const updateDarkModeClass = () => {
  if (isDarkMode.value) {
    document.documentElement.classList.add('dark')
  } else {
    document.documentElement.classList.remove('dark')
  }
}

const handleRowClick = (row, column, event) => {
  // 如果点击的是操作按钮或下拉菜单区域，不触发行展开/收起
  if (event.target.closest('.action-icon') || event.target.closest('.el-dropdown') || event.target.closest('.el-button')) {
    return
  }
  // 切换行的展开状态
  if (tableRef.value) {
    tableRef.value.toggleRowExpansion(row)
  }
}

const getJobStatusTagType = (jobType) => {
  if (jobType === "在线") return 'primary'
  return 'warning'
}

const handleAction = (command) => {
  // 由于Element Plus的限制，我们需要特殊处理来获取行ID
  setTimeout(() => {
    const activeItem = document.querySelector('.el-dropdown-menu__item.is-active') ||
        document.querySelector('.el-dropdown-menu__item:hover')
    const rowId = activeItem?.getAttribute('data-row-id')

    if (!rowId) return

    switch (command) {
      case 'start':
        console.log('启动任务:', rowId)
        break
      case 'pause':
        console.log('暂停任务:', rowId)
        break
      case 'delete':
        console.log('删除任务:', rowId)
        break
      case 'execute':
        console.log('立即执行任务:', rowId)
        break
    }
  }, 0)
}

// 组件挂载和初始化
onMounted(() => {
  // 检查本地存储中的主题偏好
  const savedTheme = localStorage.getItem('themePreference')
  if (savedTheme) {
    isDarkMode.value = savedTheme === 'dark'
  } else {
    // 初始检测系统暗色模式
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
    isDarkMode.value = mediaQuery.matches

    // 监听系统主题变化
    mediaQuery.addEventListener('change', (e) => {
      if (!localStorage.getItem('themePreference')) {
        isDarkMode.value = e.matches
        updateDarkModeClass()
      }
    })
  }

  // 应用初始主题
  updateDarkModeClass()
})

// 提供给子组件
provide('isDarkMode', isDarkMode)

</script>

<style>
/* 全局重置 */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html,
body {
  height: 100%;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
  overflow: hidden;
}

/* 暗黑模式变量 */
:root {
  --bg-timeline: #ffffff;
  --bg-primary: #ffffff;
  --bg-secondary: #f5f7fa;
  --bg-tertiary: #f0f2f5;
  --text-primary: #303133;
  --text-regular: #606266;
  --border-color: #ebeef5;
  --hover-bg: #f5f7fa;

}

.dark {
  --bg-timeline: #574e4e;
  --bg-primary: #1a1a1a;
  --bg-secondary: #232429;
  --bg-tertiary: #2c3e50;
  --text-primary: #ffffff;
  --text-regular: #e4e7ed;
  --border-color: #4e5969;
  --hover-bg: #313f4d;
}

/* Dashboard容器 */
.dashboard-container {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-primary);
  transition: background-color 0.3s ease;
}

/* 顶部导航栏 */
.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 24px;
  height: 70px;
  background-color: var(--bg-secondary);
  color: var(--text-primary);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  z-index: 100;
  transition: background-color 0.3s ease, color 0.3s ease, box-shadow 0.3s ease;
}

.dark .dashboard-header {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

/* 导航栏图标 */
.dashboard-icon {
  font-size: 24px;
  margin-right: 12px;
  color: #409EFF;
}

.header-left {
  display: flex;
  align-items: center;
}

.dashboard-title {
  font-size: 20px;
  font-weight: 600;
  margin: 0;
}

.header-right {
  display: flex;
  align-items: center;
}

/* 主内容区域 */
.dashboard-main {
  flex: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-primary);
}

.table-container {
  flex: 1;
  overflow: auto;
  padding: 25px;
  background-color: var(--bg-primary);
}

/* 展开行卡片 */
.expand-card {
  background-color: #ffffff;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin: 15px;
  overflow: hidden;
  width: 500px;
  transition: all 0.3s ease;
}

/* 暗黑模式下的展开卡片 - 确保边框样式优先级最高 */
.dark .expand-card,
.dark-mode-card {
  background-color: var(--bg-secondary) !important;
  border: 1px solid #3a3a3a !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3) !important;
  outline: none !important;
}

/* Element Plus 组件覆盖 */
.el-table {
  background-color: var(--bg-secondary);
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;

  /* 确保Element Plus的默认样式不会覆盖 */
  --el-border-color: #ffffff !important;
  --el-table-border-color: #ffffff !important;
  --el-border-color-lighter: #ffffff !important;

}

.dark .el-table {
  background-color: var(--bg-secondary);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.3);
  border-radius: 7px;
  border: 1px solid var(--border-color);
  --el-border-color-lighter: var(color) !important;
  --el-border-color: #var(color) !important;
  --el-table-border-color: #var(color) !important;
}

.el-table__header th {
  background-color: var(--bg-tertiary) !important;
  font-weight: bold !important;
  padding: 12px;
}

.el-table__expanded-cell {
  background-color: #f6f2f2 !important;
}

.dark .el-table__expanded-cell {
  background-color: #253033 !important;
}

.dark .el-table__header th {
  background-color: #232429 !important;
  border-bottom: 1px solid var(--border-color);
}

.el-table__body tr:hover > td {
  background-color: var(--hover-bg) !important;
}


/* 表格图标 */
.el-table__header th .el-icon {
  margin-right: 4px;
  font-size: 14px;
}

/* 滚动条 */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: var(--bg-primary);
}

::-webkit-scrollbar-thumb {
  background: var(--border-color);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: #909399;
}

/* 适配暗色模式的滚动条 */
.dark ::-webkit-scrollbar-thumb {
  background: #4e5969;
}

.dark ::-webkit-scrollbar-thumb:hover {
  background: #606266;
}

.timeline-container-content {
  display: flex;
  align-items: center;
  margin-top: 10px;
  color: var(--text-regular);
}

</style>
