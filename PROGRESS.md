# Progress

## Current: ActionModule 通用化 + Action/ 合并进 Anim3D

### Completed
- **ActionModule.cs** — 通用字段消费模块，可配置 `actionFields` 列表（fieldName/onValue/sosValue/eosValue），支持可配置 `signalName`
- **ActionMap.cs** — 新建 ScriptableObject，可实例化复用的 action key → (layer, variants) 映射
- **Anim3D.cs** — 多目标设计：
  - `List<MotionTarget>`: 多个 Animator + 各自 ActionMap，保留 trigger cache + priority/debounce/随机选取
  - `List<ExpressionTarget>`: 多个 SkinnedMeshRenderer + 各自 ActionMap
  - `List<MouthTarget>`: 多个 SkinnedMeshRenderer + 各自 blendShapeIndex/minVolume/maxVolume
  - 口型动画从 MouthAnim 吸收（LateUpdate RMS → 平滑 → per-target 阈值）
- **删除 Action/ 目录**: ActionLoader.cs, ActionDict.cs, MouthAnim.cs 及 .meta 全部删除
- **6 个场景 YAML 更新**: 移除 MouthAnim/ActionLoader 组件块，Anim3D 添加 mouthAudioSource 引用

### TODO (用户在 Unity Editor 中完成)
- 创建 ActionMap .asset 实例（替代原 ActionDict .asset）
- 在 Anim3D Inspector 中配置 motionTargets / expressionTargets / mouthTargets
- 在 ActionModule Inspector 中配置 actionFields 事件绑定到 Anim3D.SetAction
- 清理旧的 ActionDict .asset 文件（Dev/ActionDict.asset, Models/ActionDictUnityChan.asset）
