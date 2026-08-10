# 更新日志 / Changelog

## 0.3.0-beta.2
- 默认关闭 stop action，避免意外中断小人行为

## 0.3.0-beta.1
- 精简 ea_observed schema 提示词，移除冗余指令减少 token 消耗

## 0.3.0-beta.0
- Beta 发布：EA 行为扩展核心模块（对话 → 物理动作）
- 支持中/英文关键词 + 多语言关键词配置（含英文回退注入）
- 自动提示词注入（RimTalk PromptEntry: "EA Action Schema"）
- Expand Memory 为可选依赖（推荐用于行为记录）
